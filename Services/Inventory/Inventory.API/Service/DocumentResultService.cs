using BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation;
using BIVN.FixedStorage.Services.Common.API.Dto.QRCode;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;
using BIVN.FixedStorage.Services.Common.API.Helpers;
using BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity;
using Inventory.API.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Inventory.API.Service
{
    public class DocumentResultService : IDocumentResultService
    {
        private readonly ILogger<DocumentResultService> _logger;
        private readonly HttpContext _httpContext;
        private readonly RestClientFactory _restClientFactory;
        private readonly IDataAggregationService _dataAggregationService;
        private readonly InventoryContext _inventoryContext;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DocumentResultService(ILogger<DocumentResultService> logger,
                                        InventoryContext inventoryContext,
                                        IHttpContextAccessor httpContextAccessor,
                                        RestClientFactory restClientFactory,
                                        IDataAggregationService dataAggregationService,
                                        IServiceScopeFactory serviceScopeFactory
                                    )
        {
            _logger = logger;
            _inventoryContext = inventoryContext;
            _httpContext = httpContextAccessor.HttpContext;
            _restClientFactory = restClientFactory;
            _dataAggregationService = dataAggregationService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<ImportResponseModel<byte[]>> UploadTotalFromBwins(Guid inventoryId, Guid userId, IFormFile file)
        {
            string fileExtension = Path.GetExtension(file.FileName);
            string[] allowExtension = new[] { ".xlsx" };

            if (allowExtension.Contains(fileExtension) == false || file.Length < 0)
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành upload kết quả từ Bwins."
                };
            }

            //Nếu chưa có phiếu A trên hệ thống thì không import
            var hasDocTypeA = await _inventoryContext.InventoryDocs.AsNoTracking().AnyAsync(x => x.InventoryId == inventoryId && x.DocType == InventoryDocType.A);
            if (!hasDocTypeA)
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = (int)HttpStatusCodes.NotExistDocTypeA,
                    Message = "Vui lòng tạo phiếu A trước khi thực hiện upload kết quả."
                };
            }

            var inventoryDocs = await _inventoryContext.InventoryDocs.AsNoTracking()
                                                                    .Where(x => x.InventoryId == inventoryId &&
                                                                                x.DocType == InventoryDocType.A)
                                                                    .Select(x => new
                                                                    {
                                                                        Id = x.Id,
                                                                        ComponentCode = x.ComponentCode,
                                                                        Plant = x.Plant,
                                                                        x.WareHouseLocation,
                                                                        x.DepartmentName,
                                                                        x.LocationName,
                                                                        AssignedAccountId = x.AssignedAccountId.HasValue ? x.AssignedAccountId.Value : default
                                                                    }).ToListAsync();

            List<UploadTotalBwinExcelValueModel> items = new();
            using (var memStream = new MemoryStream())
            {
                await file.CopyToAsync(memStream);
                using (var sourcePackage = new ExcelPackage(memStream))
                {
                    var sourceSheet = sourcePackage.Workbook.Worksheets.FirstOrDefault();
                    int totalRowsCount = sourceSheet.Dimension.End.Row > 1 ? sourceSheet.Dimension.End.Row - 1 : sourceSheet.Dimension.End.Row;
                    var rows = Enumerable.Range(2, totalRowsCount).ToList();
                    if (sourceSheet != null)
                    {
                        sourceSheet.Cells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sourceSheet.Cells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                        sourceSheet.Cells.Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        //get header column
                        int materialCodeIndex = sourceSheet.GetColumnIndex(Constants.UploadTotalBwinsExcel.MaterialCode);
                        int plantIndex = sourceSheet.GetColumnIndex(Constants.UploadTotalBwinsExcel.Plant);
                        int WHLocIndex = sourceSheet.GetColumnIndex(Constants.UploadTotalBwinsExcel.WHLoc);
                        int quantityIndex = sourceSheet.GetColumnIndex(Constants.UploadTotalBwinsExcel.Quantity);

                        int[] headers = new int[] { materialCodeIndex, plantIndex, WHLocIndex, quantityIndex };
                        bool rightSquenceHeader = materialCodeIndex == 1 && plantIndex == 2 && WHLocIndex == 3 && quantityIndex == 4;
                        //Do biếu mẫu SAP chứa các cột cột của biểu mẫu Bwin nên check nếu thứ tự cột không khớp thì không cho import
                        if (headers.Any(x => x == -1) || rightSquenceHeader == false)
                        {
                            return new ImportResponseModel<byte[]>
                            {
                                Code = (int)HttpStatusCodes.InvalidFileExcel,
                                Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành upload kết quả từ Bwins.",
                            };
                        }

                        foreach (var row in rows)
                        {
                            //Bỏ qua các dòng null
                            var isEmptyRow = sourceSheet.Cells[row, 1, row, sourceSheet.Dimension.Columns].All(x => string.IsNullOrEmpty(x.Value?.ToString()));
                            if (isEmptyRow) continue;

                            var item = new UploadTotalBwinExcelValueModel();
                            item.MaterialCode = sourceSheet.Cells[row, materialCodeIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.Plant = sourceSheet.Cells[row, plantIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.WHLoc = sourceSheet.Cells[row, WHLocIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.Quantity = sourceSheet.Cells[row, quantityIndex].Value;

                            items.Add(item);
                        }

                        //Validate STT
                        foreach (var item in items)
                        {
                            ValidateMaterialDocResultBwin(item);
                            ValidatePlantDocResultBwin(item);
                            ValidateWHLocDocResultBwin(item);
                            ValidateQuantityDocResultBwin(item);

                            //Nếu hợp lệ thì check xem có tồn tại trong hệ thống không
                            if (item.Error.IsValid)
                            {
                                var existDocA = inventoryDocs.FirstOrDefault(x => x.ComponentCode == item.MaterialCode &&
                                                                                  x.Plant == item.Plant &&
                                                                                  x.WareHouseLocation == item.WHLoc);
                                if (existDocA == null)
                                {
                                    item.Error.TryAddModelError("summary", $"Thông tin mã linh kiện {item.MaterialCode} có plant " +
                                                                            $"{item.Plant} và WH Loc. {item.WHLoc} không tồn tại trong phiếu A.");
                                }
                                else
                                {
                                    //Nếu tồn tại bản ghi import khớp phiếu A thì lưu lại một số cột để tạo phiếu E và phục vụ tìm kiếm trên web
                                    //item.DepartmentName = existDocA?.DepartmentName ?? string.Empty;
                                    //item.LocationName = existDocA?.LocationName ?? string.Empty;
                                    //item.AssignedAccount = existDocA.AssignedAccountId;
                                }
                            }
                        }
                    }
                }
            }

            if (items.Count == 0)
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            //Ghi vào excel các bản ghi bị lỗi
            var invalidItems = items.Where(x => x.Error.IsValid == false);
            MemoryStream stream = new MemoryStream();
            if (invalidItems.Any())
            {
                //Ghi lỗi vào file excel
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Lỗi nhập tổng số lượng từ Bwins");

                    // Đặt tiêu đề cho cột
                    int sttIndex = UploadTotalBwinsExcel.ExportHeaders.IndexOf(UploadTotalBwinsExcel.STT) + 1;
                    int materialCodeIndex = UploadTotalBwinsExcel.ExportHeaders.IndexOf(UploadTotalBwinsExcel.MaterialCode) + 1;
                    int plantIndex = UploadTotalBwinsExcel.ExportHeaders.IndexOf(UploadTotalBwinsExcel.Plant) + 1;
                    int warehouseLocIndex = UploadTotalBwinsExcel.ExportHeaders.IndexOf(UploadTotalBwinsExcel.WHLoc) + 1;
                    int quantityIndex = UploadTotalBwinsExcel.ExportHeaders.IndexOf(UploadTotalBwinsExcel.Quantity) + 1;
                    int errorIndex = UploadTotalBwinsExcel.ExportHeaders.IndexOf(UploadTotalBwinsExcel.ErrorSummary) + 1;

                    worksheet.Cells[1, sttIndex].Value = UploadTotalBwinsExcel.STT;
                    worksheet.Cells[1, materialCodeIndex].Value = UploadTotalBwinsExcel.MaterialCode;
                    worksheet.Cells[1, plantIndex].Value = UploadTotalBwinsExcel.Plant;
                    worksheet.Cells[1, warehouseLocIndex].Value = UploadTotalBwinsExcel.WHLoc;
                    worksheet.Cells[1, quantityIndex].Value = UploadTotalBwinsExcel.Quantity;
                    worksheet.Cells[1, errorIndex].Value = UploadTotalBwinsExcel.ErrorSummary;

                    // Đặt kiểu và màu cho tiêu đề
                    using (var range = worksheet.Cells[1, sttIndex, 1, errorIndex])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.None;
                    }

                    // Điền dữ liệu vào Excel
                    for (int i = 0; i < invalidItems.Count(); i++)
                    {
                        var item = invalidItems.ElementAtOrDefault(i);

                        int stt = i + 1;
                        //Tổng hợp message lỗi
                        var errMessage = item.Error.Values.SelectMany(x => x.Errors)
                                                             .Select(x => x.ErrorMessage)
                                                             .Distinct();

                        worksheet.Cells[i + 2, sttIndex].Value = stt;
                        worksheet.Cells[i + 2, materialCodeIndex].Value = item.MaterialCode;
                        worksheet.Cells[i + 2, plantIndex].Value = item.Plant;
                        worksheet.Cells[i + 2, warehouseLocIndex].Value = item.WHLoc;
                        worksheet.Cells[i + 2, quantityIndex].Value = item.Quantity;
                        worksheet.Cells[i + 2, errorIndex].Value = string.Join("\n", errMessage);

                        using (var errorRange = worksheet.Cells[i + 2, errorIndex, i + 2, errorIndex])
                        {
                            errorRange.Style.Font.Color.SetColor(Color.Red);
                            errorRange.Style.Fill.PatternType = ExcelFillStyle.None;
                        }
                    }

                    // Lưu file Excel
                    package.SaveAs(stream);
                }
            }

            var validItems = items.Where(x => x.Error.IsValid);
            var failCount = items.Where(x => x.Error.IsValid == false).Count();
            var successCount = items.Where(x => x.Error.IsValid == true).Count();

            if (validItems.Any())
            {
                //Sau khi đã tổng hợp các bản ghi từ file excel và đánh dấu các lỗi validate vào từng item
                //Với những item hợp lệ sẽ tạo phiếu E
                List<InventoryDoc> docsE = new();
                var currentInventory = await _inventoryContext.Inventories.FirstOrDefaultAsync(x => x.Id == inventoryId);
                int lastDocENumber = await GetLastDocENumber(inventoryId);

                var currUser = _httpContext.UserFromContext();

                foreach (var item in validItems)
                {
                    //Get Inventory Doc A Detail:
                    var getInventoryDocADetail = await _inventoryContext.InventoryDocs.FirstOrDefaultAsync(x => x.DocType == InventoryDocType.A
                                                       && x.Plant == item.Plant && x.ComponentCode == item.MaterialCode && x.WareHouseLocation == item.WHLoc);

                    //Converted quantity
                    var quantity = double.Parse(item.Quantity.ToString());

                    //Doc Code
                    string dateFormat = currentInventory.InventoryDate.ToString("yyMM") ?? string.Empty;
                    var docCodeE = $"E{dateFormat}{(++lastDocENumber).ToString().PadLeft(5, '0')}";

                    InventoryDoc doc = new InventoryDoc
                    {
                        Id = Guid.NewGuid(),
                        WareHouseLocation = item.WHLoc,
                        ComponentCode = item.MaterialCode,
                        Plant = item.Plant,
                        Quantity = quantity,
                        DocCode = docCodeE,
                        DocType = InventoryDocType.E,
                        //Trạng thái => Đã xác nhận
                        Status = InventoryDocStatus.Confirmed,
                        InventoryId = inventoryId,
                        //Thời gian kiểm kê
                        InventoryAt = DateTime.Now,
                        InventoryBy = currUser.UserCode,
                        //Thời gian xác nhận
                        ConfirmAt = DateTime.Now,
                        ConfirmBy = currUser.UserCode,
                        //Thời gian tạo phiếu
                        CreatedBy = currUser.UserCode,
                        CreatedAt = DateTime.Now,
                        //Lưu lại 3 trường này lấy từ phiếu A để tìm kiếm trên web
                        //DepartmentName = item.DepartmentName,
                        //LocationName = item.LocationName,
                        //AssignedAccountId = item.AssignedAccount,
                        ComponentName = getInventoryDocADetail.ComponentName
                    };

                    docsE.Add(doc);
                }

                try
                {
                    var executionStrategy = _inventoryContext.Database.CreateExecutionStrategy();
                    await executionStrategy.ExecuteAsync(async () =>
                    {
                        using var dbContextTransaction = _inventoryContext.Database.BeginTransaction();

                        //Gọi background để tính toán lại tổng số lượng mới tạo từ phiếu E nhập vào phiếu A (phiếu tổng)
                        InventoryDocSubmitDto updateDocTotalParam = new InventoryDocSubmitDto();
                        updateDocTotalParam.InventoryId = inventoryId;
                        await _dataAggregationService.UpdateDataFromInventoryDoc(updateDocTotalParam);

                        _inventoryContext.InventoryDocs.AddRange(docsE);

                        await _inventoryContext.SaveChangesAsync(); // Save changes to the database

                        await dbContextTransaction.CommitAsync();
                        await dbContextTransaction.DisposeAsync();
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError("Có lỗi khi thực hiện gọi background tính toán lại số lượng phiếu A.");
                    return new ImportResponseModel<byte[]>
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = "Lỗi hệ thống khi thực hiện cập nhật số lượng phiếu A."
                    };
                }
            }


            return new ImportResponseModel<byte[]>
            {
                Data = stream.ToArray(),
                Code = StatusCodes.Status200OK,
                FailCount = failCount,
                SuccessCount = successCount,
            };
        }

        private async Task<int> GetLastDocENumber(Guid inventoryId)
        {
            var lastDocE = await _inventoryContext.InventoryDocs.AsNoTracking()
                                                    .Where(x => x.InventoryId == inventoryId
                                                                && x.DocType == InventoryDocType.E
                                                                && !string.IsNullOrEmpty(x.DocCode))
                                                    .OrderByDescending(x => x.DocCode)
                                                    .FirstOrDefaultAsync(x => x.InventoryId == inventoryId
                                                                && x.DocType == InventoryDocType.E
                                                                && !string.IsNullOrEmpty(x.DocCode));

            //Nếu chưa có phiếu nào để lấy 5 số cuối từ Doc code thì mặc định là 0
            int number = lastDocE == null ? 0 : int.Parse(lastDocE.DocCode.Substring(lastDocE.DocCode.Length - 5));
            return number;
        }

        private bool ValidateMaterialDocResultBwin(UploadTotalBwinExcelValueModel item)
        {
            //Validate STT
            if (string.IsNullOrEmpty(item.MaterialCode))
            {
                item.Error.TryAddModelError(nameof(item.MaterialCode), "Vui lòng nhập MaterialCode.");
                return false;
            }
            return true;
        }
        private bool ValidatePlantDocResultBwin(UploadTotalBwinExcelValueModel item)
        {
            //Validate STT
            if (string.IsNullOrEmpty(item.Plant))
            {
                item.Error.TryAddModelError(nameof(item.Plant), "Vui lòng nhập Plant.");
                return false;
            }
            return true;
        }
        private bool ValidateWHLocDocResultBwin(UploadTotalBwinExcelValueModel item)
        {
            //Validate STT
            if (string.IsNullOrEmpty(item.WHLoc))
            {
                item.Error.TryAddModelError(nameof(item.WHLoc), "Vui lòng nhập WHLoc.");
                return false;
            }
            return true;
        }
        private bool ValidateQuantityDocResultBwin(UploadTotalBwinExcelValueModel item)
        {
            //Validate STT
            if (string.IsNullOrEmpty(item.Quantity?.ToString() ?? string.Empty))
            {
                item.Error.TryAddModelError(nameof(item.Quantity), "Vui lòng nhập số lượng.");
                return false;
            }

            if (!double.TryParse(item.Quantity.ToString(), out var convertedQuantity))
            {
                item.Error.TryAddModelError(nameof(item.Quantity), "Số lượng không hợp lệ.");
                return false;
            }

            if (convertedQuantity < 0)
            {
                item.Error.TryAddModelError(nameof(item.Quantity), "Số lượng kiểm kê phải lớn hơn hoặc bằng 0.");
                return false;
            }
            return true;
        }

        private byte[] InvalidItemsImportSAPToExcel(List<ImportSAPExcelValueModel> invalidItems)
        {
            //Ghi lỗi vào file excel
            using (MemoryStream stream = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Lỗi khi thêm dữ liệu từ SAP");

                    // Đặt tiêu đề cho cột
                    int sttIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.STT) + 1;
                    int plantIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.Plant) + 1;
                    int wHLocIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.WHLoc) + 1;
                    int cSAPIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.CSAP) + 1;
                    int materialCodeIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.MaterialCode) + 1;
                    int descriptionIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.Description) + 1;
                    int storageBinIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.StorageBin) + 1;
                    int soNoIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.SONo) + 1;
                    int stockTypesIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.StockTypes) + 1;
                    int physInvIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.PhysInv) + 1;
                    int quantityIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.Quantity) + 1;
                    int kSAPIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.KSAP) + 1;
                    int accountQtyIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.AccountQty) + 1;
                    int mSAPIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.MSAP) + 1;
                    int errorQtyIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.ErrorQty) + 1;
                    int oSAPIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.OSAP) + 1;
                    int unitPriceIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.UnitPrice) + 1;
                    int errorMoneyIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.ErrorMoney) + 1;
                    //int note = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.Note) + 1;
                    int errorSummaryIndex = ImportSAPExcel.ExcelHeaders.IndexOf(ImportSAPExcel.ErrorSummary) + 1;


                    worksheet.Cells[1, sttIndex].Value = ImportSAPExcel.STT;
                    worksheet.Cells[1, plantIndex].Value = ImportSAPExcel.Plant;
                    worksheet.Cells[1, wHLocIndex].Value = ImportSAPExcel.WHLoc;
                    worksheet.Cells[1, cSAPIndex].Value = ImportSAPExcel.CSAP;
                    worksheet.Cells[1, materialCodeIndex].Value = ImportSAPExcel.MaterialCode;
                    worksheet.Cells[1, descriptionIndex].Value = ImportSAPExcel.Description;
                    worksheet.Cells[1, storageBinIndex].Value = ImportSAPExcel.StorageBin;
                    worksheet.Cells[1, soNoIndex].Value = ImportSAPExcel.SONo;
                    worksheet.Cells[1, stockTypesIndex].Value = ImportSAPExcel.StockTypes;
                    worksheet.Cells[1, physInvIndex].Value = ImportSAPExcel.PhysInv;
                    worksheet.Cells[1, quantityIndex].Value = ImportSAPExcel.Quantity;
                    worksheet.Cells[1, kSAPIndex].Value = ImportSAPExcel.KSAP;
                    worksheet.Cells[1, accountQtyIndex].Value = ImportSAPExcel.AccountQty;
                    worksheet.Cells[1, mSAPIndex].Value = ImportSAPExcel.MSAP;
                    worksheet.Cells[1, errorQtyIndex].Value = ImportSAPExcel.ErrorQty;
                    worksheet.Cells[1, oSAPIndex].Value = ImportSAPExcel.OSAP;
                    worksheet.Cells[1, unitPriceIndex].Value = ImportSAPExcel.UnitPrice;
                    worksheet.Cells[1, errorMoneyIndex].Value = ImportSAPExcel.ErrorMoney;
                    //worksheet.Cells[1, note].Value = ImportSAPExcel.Note;
                    worksheet.Cells[1, errorSummaryIndex].Value = ImportSAPExcel.ErrorSummary;

                    // Đặt kiểu và màu cho tiêu đề
                    using (var range = worksheet.Cells[1, sttIndex, 1, errorSummaryIndex])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.None;
                    }

                    // Điền dữ liệu vào Excel
                    for (int i = 0; i < invalidItems.Count(); i++)
                    {
                        var item = invalidItems.ElementAtOrDefault(i);

                        int stt = i + 1;
                        //Tổng hợp message lỗi
                        var errMessage = item.Error.Values.SelectMany(x => x.Errors)
                                                             .Select(x => x.ErrorMessage)
                                                             .Distinct();

                        worksheet.Cells[i + 2, sttIndex].Value = stt;
                        worksheet.Cells[i + 2, plantIndex].Value = item.Plant;
                        worksheet.Cells[i + 2, wHLocIndex].Value = item.WHLoc;
                        worksheet.Cells[i + 2, cSAPIndex].Value = item.CSAP;
                        worksheet.Cells[i + 2, materialCodeIndex].Value = item.MaterialCode;
                        worksheet.Cells[i + 2, descriptionIndex].Value = item.Description;
                        worksheet.Cells[i + 2, storageBinIndex].Value = item.StorageBin;
                        worksheet.Cells[i + 2, soNoIndex].Value = item.SONo;
                        worksheet.Cells[i + 2, stockTypesIndex].Value = item.StockTypes;
                        worksheet.Cells[i + 2, physInvIndex].Value = item.PhysInv;
                        worksheet.Cells[i + 2, quantityIndex].Value = item.Quantity;
                        worksheet.Cells[i + 2, kSAPIndex].Value = item.KSAP;
                        worksheet.Cells[i + 2, accountQtyIndex].Value = item.AccountQty;
                        worksheet.Cells[i + 2, mSAPIndex].Value = item.MSAP;
                        worksheet.Cells[i + 2, errorQtyIndex].Value = item.ErrorQty;
                        worksheet.Cells[i + 2, oSAPIndex].Value = item.OSAP;
                        worksheet.Cells[i + 2, unitPriceIndex].Value = item.UnitPrice;
                        worksheet.Cells[i + 2, errorMoneyIndex].Value = item.ErrorMoney;
                        //worksheet.Cells[i + 2, note].Value = "";
                        worksheet.Cells[i + 2, errorSummaryIndex].Value = string.Join("\n", errMessage);

                        using (var errorRange = worksheet.Cells[i + 2, errorSummaryIndex, i + 2, errorSummaryIndex])
                        {
                            errorRange.Style.Font.Color.SetColor(Color.Red);
                            errorRange.Style.Fill.PatternType = ExcelFillStyle.None;
                        }
                    }

                    // Lưu file Excel
                    package.SaveAs(stream);
                }
                return stream.ToArray();
            }
        }

        public async Task<ImportResponseModel<byte[]>> ImportFileSAP(IFormFile file, string inventoryId, string userId)
        {
            var inventoryIdGuid = Guid.Parse(inventoryId);
            string fileExtension = Path.GetExtension(file.FileName);
            string[] allowExtension = new[] { ".xlsx" };

            if (allowExtension.Contains(fileExtension) == false || file.Length < 0)
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành thêm dữ liệu từ SAP."
                };
            }

            var inventoryDocs = await _inventoryContext.InventoryDocs.AsNoTracking()
                                                                    .Where(x => x.InventoryId == inventoryIdGuid &&
                                                                                x.DocType == InventoryDocType.A)
                                                                    .Select(x => new
                                                                    {
                                                                        Id = x.Id,
                                                                        ComponentCode = x.ComponentCode,
                                                                        Plant = x.Plant,
                                                                        x.WareHouseLocation
                                                                    }).ToListAsync();
            if (inventoryDocs.Count() == 0)
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = (int)HttpStatusCodes.CheckExistDoctypeA,
                    Message = "Vui lòng tạo phiếu A trước khi thực hiện thêm dữ liệu từ SAP."
                };
            }

            List<ImportSAPExcelValueModel> items = new();
            using (var memStream = new MemoryStream())
            {
                file.CopyTo(memStream);
                using (var sourcePackage = new ExcelPackage(memStream))
                {
                    var sourceSheet = sourcePackage.Workbook.Worksheets.FirstOrDefault();
                    int totalRowsCount = sourceSheet.Dimension.End.Row > 1 ? sourceSheet.Dimension.End.Row - 1 : sourceSheet.Dimension.End.Row;
                    var rows = Enumerable.Range(2, totalRowsCount).ToList();
                    if (sourceSheet != null)
                    {
                        sourceSheet.Cells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sourceSheet.Cells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                        sourceSheet.Cells.Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        //get header column
                        int plantIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.Plant);
                        int wHLocIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.WHLoc);
                        int cSAPIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.CSAP);
                        int materialCodeIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.MaterialCode);
                        int descriptionIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.Description);
                        int storageBinIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.StorageBin);
                        int soNoIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.SONo);
                        int stockTypesIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.StockTypes);
                        int physInvIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.PhysInv);
                        int quantityIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.Quantity);
                        int kSAPIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.KSAP);
                        int accountQtyIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.AccountQty);
                        int mSAPIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.MSAP);
                        int errorQtyIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.ErrorQty);
                        int oSAPIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.OSAP);
                        int unitPriceIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.UnitPrice);
                        int errorMoneyIndex = sourceSheet.GetColumnIndex(Constants.ImportSAPExcel.ErrorMoney);

                        int[] headers = new int[] {
                            plantIndex,
                            wHLocIndex,
                            cSAPIndex,
                            materialCodeIndex,
                            descriptionIndex,
                            storageBinIndex,
                            soNoIndex,
                            stockTypesIndex,
                            physInvIndex,
                            quantityIndex,
                            kSAPIndex,
                            accountQtyIndex,
                            mSAPIndex,
                            errorQtyIndex,
                            oSAPIndex,
                            unitPriceIndex,
                            errorMoneyIndex,
                        };

                        if (headers.Any(x => x == -1))
                        {
                            return new ImportResponseModel<byte[]>
                            {
                                Code = StatusCodes.Status400BadRequest,
                                Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành thêm dữ liệu từ SAP.",
                            };
                        }

                        foreach (var row in rows)
                        {
                            //Bỏ qua các dòng null
                            var isEmptyRow = sourceSheet.Cells[row, 1, row, sourceSheet.Dimension.Columns].All(x => string.IsNullOrEmpty(x.Value?.ToString()));
                            if (isEmptyRow)
                            {
                                continue;
                            }
                            var item = new ImportSAPExcelValueModel();
                            item.Plant = sourceSheet.Cells[row, plantIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.WHLoc = sourceSheet.Cells[row, wHLocIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.CSAP = sourceSheet.Cells[row, cSAPIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.MaterialCode = sourceSheet.Cells[row, materialCodeIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.Description = sourceSheet.Cells[row, descriptionIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.StorageBin = sourceSheet.Cells[row, storageBinIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.SONo = sourceSheet.Cells[row, soNoIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.StockTypes = sourceSheet.Cells[row, stockTypesIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.PhysInv = sourceSheet.Cells[row, physInvIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.Quantity = sourceSheet.Cells[row, quantityIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.KSAP = sourceSheet.Cells[row, kSAPIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.AccountQty = sourceSheet.Cells[row, accountQtyIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.MSAP = sourceSheet.Cells[row, mSAPIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.ErrorQty = sourceSheet.Cells[row, errorQtyIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.OSAP = sourceSheet.Cells[row, oSAPIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.UnitPrice = sourceSheet.Cells[row, unitPriceIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.ErrorMoney = sourceSheet.Cells[row, errorMoneyIndex].Value?.ToString()?.Trim() ?? string.Empty;

                            items.Add(item);
                        }

                        //Validate STT
                        foreach (var item in items)
                        {
                            //validate:

                            //Material code, Plant, WH Loc., Account Qty các trường này mà = false => Bắn lỗi file ko đúng định dạng:
                            var validateMaterialCodeResult = ValidateMaterialCodeSAP(item);
                            var validatePlantResult = ValidatePlantSAP(item);
                            var validateWHLocResult = ValidateWHLocSAP(item);
                            var validateIsNullOrEmptyAccountQty = ValidateIsNullOrEmptyAccountQtySAP(item);

                            var validateQuantityResult = ValidateAccountQtySAP(item);
                            //var validateErrorQtyResult = ValidateErrorQtySAP(item);
                            //var validateErrorMoneyResult = ValidateErrorMoneySAP(item);
                            var validateUnitPriceResult = ValidateUnitPriceSAP(item);

                            //Nếu hợp lệ thì check xem có tồn tại trong hệ thống không ComponentCode, WHLoc, Plant
                            if (item.Error.IsValid)
                            {
                                var existDocA = inventoryDocs.FirstOrDefault(x => x.ComponentCode == item.MaterialCode &&
                                                                                  x.Plant == item.Plant &&
                                                                                  x.WareHouseLocation == item.WHLoc);
                                if (existDocA == null)
                                {
                                    item.Error.TryAddModelError("summary", $"Thông tin mã linh kiện {item.MaterialCode} có plant " +
                                                                            $"{item.Plant} và WH Loc. {item.WHLoc} không tồn tại trong phiếu A.");
                                }
                            }
                        }
                    }
                }
            }

            if (items.Count == 0)
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }
            byte[] fileByteErrors = null;
            //Ghi vào excel các bản ghi bị lỗi
            var invalidItems = items.Where(x => x.Error.IsValid == false).ToList();

            if (invalidItems.Any())
            {
                fileByteErrors = InvalidItemsImportSAPToExcel(invalidItems);

            }

            //Sau khi đã tổng hợp các bản ghi từ file excel và đánh dấu các lỗi validate vào từng item
            //Với những item hợp lệ sẽ cập nhật Account Qty, Error Qty, Error money, Unit price của phiếu A(WH Loc, ComponentCode, Plant => Bộ 3 này là tìm được phiếu A):

            var validItems = items.Where(x => x.Error.IsValid).OrderByDescending(x => x.MaterialCode).ToList();

            //Cập nhật AccountQuantity = 0 của các phiếu A không có trong file upload:
            await _inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryIdGuid && x.DocType == InventoryDocType.A)
                                    .ExecuteUpdateAsync(x => x.SetProperty(y => y.AccountQuantity, 0));

            // Lấy danh sách MaterialCode, Plant, WareHouseLocation cần cập nhật
            var materialCodes = validItems.Select(x => x.MaterialCode).Distinct().ToList();
            var plants = validItems.Select(x => x.Plant).Distinct().ToList();
            var whLocs = validItems.Select(x => x.WHLoc).Distinct().ToList();

            // Truy vấn tất cả InventoryDocs cần cập nhật trong một lần gọi DB
            var inventoryDocList = await _inventoryContext.InventoryDocs.AsNoTracking()
                                        .Where(x => x.InventoryId.ToString() == inventoryId && x.DocType == InventoryDocType.A && materialCodes.Contains(x.ComponentCode) &&
                                                    plants.Contains(x.Plant) &&
                                                    whLocs.Contains(x.WareHouseLocation))
                                        .ToListAsync();


            // Dictionary để lưu trữ các MaterialCode đã được xử lý
            var processedMaterialCodes = new Dictionary<string, bool>();

            // Danh sách các InventoryDocs cần cập nhật
            var updatedInventoryDocs = new List<InventoryDoc>();
            var updatedInventoryDocNotImports = new List<InventoryDoc>();

            foreach (var item in validItems)
            {
                var getInventoryDocTypeA = inventoryDocList.FirstOrDefault(x => x.Plant == item.Plant && x.WareHouseLocation == item.WHLoc
                                                                                        && x.ComponentCode == item.MaterialCode && x.DocType == InventoryDocType.A);

                if (getInventoryDocTypeA != null)
                {
                    double accountQuantity;
                    double errorQuantity;
                    double unitPrice;
                    double errorMoney;

                    //Update Logic: Lần đầu tiên AccountQuantity(Phiếu A) = item.AccountQty(trong file import SAP), các lần tiếp theo sẽ cộng dồn AccountQuantity(Phiếu A):
                    if (double.TryParse(item.AccountQty.ToString(), out accountQuantity))
                    {
                        if (!processedMaterialCodes.ContainsKey(item.MaterialCode))
                        {
                            // Lần đầu tiên gán giá trị accountQuantity
                            getInventoryDocTypeA.AccountQuantity = accountQuantity;
                            processedMaterialCodes[item.MaterialCode] = true; // Đánh dấu MaterialCode đã được xử lý
                        }
                        else
                        {
                            // Các lần sau cộng dồn giá trị accountQuantity
                            getInventoryDocTypeA.AccountQuantity += accountQuantity;
                        }
                    }

                    if (double.TryParse(item.UnitPrice.ToString(), out unitPrice))
                    {
                        getInventoryDocTypeA.UnitPrice = unitPrice;
                    }

                    //Công thức tính: Error Qty = Total Qty - Account Qty
                    getInventoryDocTypeA.ErrorQuantity = getInventoryDocTypeA.TotalQuantity - getInventoryDocTypeA.AccountQuantity;

                    //Công thức tính: Error money = Error Qty * Unit price trong nó unit price lấy từ file upload
                    getInventoryDocTypeA.ErrorMoney = getInventoryDocTypeA.ErrorQuantity * unitPrice;

                    getInventoryDocTypeA.CSAP = item.CSAP.ToString();
                    getInventoryDocTypeA.KSAP = item.KSAP.ToString();
                    getInventoryDocTypeA.MSAP = item.MSAP.ToString();
                    getInventoryDocTypeA.OSAP = item.OSAP.ToString();
                    getInventoryDocTypeA.UpdatedAt = DateTime.Now;
                    getInventoryDocTypeA.UpdatedBy = userId;

                    updatedInventoryDocs.Add(getInventoryDocTypeA);
                }
            }

            var list = new List<IEnumerable<InventoryDoc>>();

            if (updatedInventoryDocs.Count < 15_000)
                list.AddRange(updatedInventoryDocs.Chunk(150));
            else if (updatedInventoryDocs.Count < 25_000)
                list.AddRange(updatedInventoryDocs.Chunk(250));
            else
                list.AddRange(updatedInventoryDocs.Chunk(500));

            Parallel.ForEach(list, async batch =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
                    context.InventoryDocs.UpdateRange(batch);
                    await context.SaveChangesAsync();
                }
            });


            var failCount = items.Where(x => x.Error.IsValid == false).Count();
            var successCount = items.Where(x => x.Error.IsValid == true).Count();

            var fileType = Constants.FileResponse.ExcelType;
            var fileName = string.Format("DulieuSAP_Fileloi_{0}", DateTime.Now.ToString(Constants.DatetimeFormat));

            return new ImportResponseModel<byte[]>
            {
                Bytes = fileByteErrors,
                Code = StatusCodes.Status200OK,
                FailCount = failCount,
                SuccessCount = successCount,
                FileType = fileType,
                FileName = fileName,
            };
        }



        private bool ValidateMaterialCodeSAP(ImportSAPExcelValueModel item)
        {
            //Validate MaterialCode required:
            if (string.IsNullOrEmpty(item.MaterialCode))
            {
                item.Error.TryAddModelError(nameof(item.MaterialCode), "Vui lòng nhập MaterialCode.");
                return false;
            }
            return true;
        }
        private bool ValidatePlantSAP(ImportSAPExcelValueModel item)
        {
            //Validate STT
            if (string.IsNullOrEmpty(item.Plant))
            {
                item.Error.TryAddModelError(nameof(item.Plant), "Vui lòng nhập Plant.");
                return false;
            }
            return true;
        }
        private bool ValidateWHLocSAP(ImportSAPExcelValueModel item)
        {
            //Validate STT
            if (string.IsNullOrEmpty(item.WHLoc))
            {
                item.Error.TryAddModelError(nameof(item.WHLoc), "Vui lòng nhập WHLoc.");
                return false;
            }
            return true;
        }

        private bool ValidateIsNullOrEmptyAccountQtySAP(ImportSAPExcelValueModel item)
        {
            //Validate STT
            if (string.IsNullOrEmpty(item.AccountQty.ToString()))
            {
                item.Error.TryAddModelError(nameof(item.AccountQty), "Account Qty: Vui lòng nhập số lượng.");
                return false;
            }
            return true;
        }

        private bool ValidateAccountQtySAP(ImportSAPExcelValueModel item)
        {
            if (!double.TryParse(item.AccountQty.ToString(), out var convertedQuantity))
            {
                item.Error.TryAddModelError(nameof(item.AccountQty), "Account Qty: Số lượng không đúng.");
                return false;
            }

            if (convertedQuantity < 0)
            {
                item.Error.TryAddModelError(nameof(item.AccountQty), "Account Qty: Số lượng phải lớn hơn hoặc bằng 0.");
                return false;
            }
            return true;
        }

        private bool ValidateErrorQtySAP(ImportSAPExcelValueModel item)
        {
            if (string.IsNullOrEmpty(item.ErrorQty.ToString()))
            {
                return true;
            }

            if (!double.TryParse(item.ErrorQty.ToString(), out var convertedQuantity))
            {
                item.Error.TryAddModelError(nameof(item.ErrorQty), "Error Qty: Số lượng không đúng.");
                return false;
            }
            return true;
        }
        private bool ValidateErrorMoneySAP(ImportSAPExcelValueModel item)
        {
            if (string.IsNullOrEmpty(item.ErrorMoney.ToString()))
            {
                return true;
            }
            if (!double.TryParse(item.ErrorMoney.ToString(), out var convertedQuantity))
            {
                item.Error.TryAddModelError(nameof(item.ErrorMoney), "Error Money: Số lượng không đúng.");
                return false;
            }
            return true;
        }
        private bool ValidateUnitPriceSAP(ImportSAPExcelValueModel item)
        {
            if (string.IsNullOrEmpty(item.UnitPrice.ToString()))
            {
                return true;
            }
            if (!double.TryParse(item.UnitPrice.ToString(), out var convertedQuantity))
            {
                item.Error.TryAddModelError(nameof(item.UnitPrice), "UnitPrice: Số lượng không đúng.");
                return false;
            }
            return true;
        }

        public async Task<InventoryResponseModel<IEnumerable<ListDocumentHistoryModel>>> ListDocumentHistories(ListDocumentHistoryDto listDocumentHistory)
        {
            var result = from dh in _inventoryContext.DocHistories.AsNoTracking()
                         join id in _inventoryContext.InventoryDocs.IgnoreQueryFilters().AsNoTracking() on dh.InventoryDocId equals id.Id
                            into idGroup
                         from T2 in idGroup.DefaultIfEmpty()
                         join i in _inventoryContext.Inventories.AsNoTracking() on dh.InventoryId equals i.Id
                            into iGroup
                         from T3 in iGroup.DefaultIfEmpty()
                         orderby dh.CreatedAt descending
                         select new ListDocumentHistoryModel
                         {
                             InventoryId = dh.InventoryId.ToString(),
                             HistoryId = dh.Id.ToString(),
                             InventoryName = !string.IsNullOrEmpty(T3.Name) ? T3.Name : string.Empty,
                             DocCode = !string.IsNullOrEmpty(T2.DocCode) ? T2.DocCode : string.Empty,
                             ComponentCode = !string.IsNullOrEmpty(T2.ComponentCode) ? T2.ComponentCode : string.Empty,
                             ModelCode = !string.IsNullOrEmpty(T2.ModelCode) ? T2.ModelCode : string.Empty,
                             ComponentName = !string.IsNullOrEmpty(T2.ComponentName) ? T2.ComponentName : string.Empty,
                             Department = !string.IsNullOrEmpty(T2.DepartmentName) ? T2.DepartmentName : string.Empty,
                             Location = !string.IsNullOrEmpty(T2.LocationName) ? T2.LocationName : string.Empty,
                             AssigneeAccount = !string.IsNullOrEmpty(dh.CreatedBy) ? dh.CreatedBy : string.Empty,
                             Action = EnumHelper<DocHistoryActionType>.GetDisplayValue(dh.Action),
                             Comment = !string.IsNullOrEmpty(dh.Comment) ? dh.Comment : string.Empty,
                             AssigneeAccountDate = dh.CreatedAt.ToString(Constants.DefaultDateFormat),
                             ChangeLog = BIVN.FixedStorage.Services.Inventory.API.Utilities.DisplayChangeLog(dh.OldQuantity, dh.NewQuantity, (int)dh.OldStatus, (int)dh.NewStatus, dh.IsChangeCDetail),
                             DocType = (int)(T2.DocType),
                         };

            if (result.Count() == 0)
            {
                return new InventoryResponseModel<IEnumerable<ListDocumentHistoryModel>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy danh sách lịch sử kiểm kê.",
                };
            }

            //Tim kiem theo Mã phiếu
            if (!string.IsNullOrEmpty(listDocumentHistory.DocCode))
            {
                result = result.Where(x => x.DocCode.Contains(listDocumentHistory.DocCode));
            }

            //Tim kiem theo Component:
            if (!string.IsNullOrEmpty(listDocumentHistory.ComponentCode))
            {
                result = result.Where(x => x.ComponentCode.Contains(listDocumentHistory.ComponentCode));
            }

            //Tim kiem theo Model Code:
            if (!string.IsNullOrEmpty(listDocumentHistory.ModelCode))
            {
                result = result.Where(x => x.ModelCode.Contains(listDocumentHistory.ModelCode));
            }

            //Tim kiem theo Người thao tác:
            if (!string.IsNullOrEmpty(listDocumentHistory.AssigneeAccount))
            {
                result = result.Where(x => x.AssigneeAccount.Contains(listDocumentHistory.AssigneeAccount));
            }

            //Tim kiem theo Phong Ban:
            if (listDocumentHistory != null && listDocumentHistory.Departments.Any())
            {
                result = result.Where(x => listDocumentHistory.Departments.Contains(x.Department));
            }

            //Tim kiem theo Khu vuc:
            if (listDocumentHistory != null && listDocumentHistory.Locations.Any())
            {
                result = result.Where(x => listDocumentHistory.Locations.Contains(x.Location));
            }

            //Tim kiem theo Dot Kiem Ke:
            if (listDocumentHistory != null && listDocumentHistory.InventoryNames.Any())
            {
                result = result.Where(x => listDocumentHistory.InventoryNames.Contains(x.InventoryName));
            }

            //Tim kiem theo Loai Phieu:
            if (listDocumentHistory != null && listDocumentHistory.DocTypes.Any())
            {
                result = result.Where(x => listDocumentHistory.DocTypes.Select(x => int.Parse(x)).Contains(x.DocType));
            }

            var totalRecords = await result.CountAsync();

            var itemsFromQuery = new List<ListDocumentHistoryModel>();
            if (listDocumentHistory.IsExport)
            {
                itemsFromQuery = await result.ToListAsync();
            }
            else
            {
                itemsFromQuery = await result.Skip(listDocumentHistory.Skip).Take(listDocumentHistory.Take).ToListAsync();
            }

            return new InventoryResponseModel<IEnumerable<ListDocumentHistoryModel>>
            {
                Code = StatusCodes.Status200OK,
                Message = "Danh sách lịch sử kiểm kê.",
                Data = itemsFromQuery,
                TotalRecords = totalRecords
            };
        }

        public async Task<ResponseModel<IEnumerable<ListDocTypeCToExportQRCodeModel>>> ListDocTypeCToExportQRCode(ListDocTypeCToExportQRCodeDto listDocTypeCToExportQRCode)
        {
            var query = from id in _inventoryContext.InventoryDocs.AsNoTracking()
                        where id.InventoryId == Guid.Parse(listDocTypeCToExportQRCode.InventoryId)
                               && id.DocType == InventoryDocType.C
                               && id.IsDeleted != true
                               && id.MachineModel == listDocTypeCToExportQRCode.MachineModel
                               && id.MachineType == listDocTypeCToExportQRCode.MachineType
                        select new ListDocTypeCToExportQRCodeModel
                        {
                            ModelCode = id.ModelCode,
                            StageName = id.StageName,
                            LineName = id.LineName,
                        };

            if (query.Count() == 0)
            {
                return new ResponseModel<IEnumerable<ListDocTypeCToExportQRCodeModel>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy phiếu C để Export QRCode.",
                };
            }
            if (!string.IsNullOrEmpty(listDocTypeCToExportQRCode.LineName))
            {
                query = query.Where(x => x.LineName == listDocTypeCToExportQRCode.LineName);
            }

            var result = await query.Distinct().ToListAsync();

            return new ResponseModel<IEnumerable<ListDocTypeCToExportQRCodeModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = result,
                Message = "Danh sách phiếu C để Export QRCode.",
            };
        }

        public async Task<ResponseModel<IEnumerable<ListDocToExportInventoryErrorModel>>> ListDocumentToInventoryError(ListDocToExportInventoryErrorDto listDocToExportInventoryErrorDto)
        {
            var locations = await (from il in _inventoryContext.InventoryLocations.AsNoTracking()
                                   join al in _inventoryContext.AccountLocations.AsNoTracking() on il.Id equals al.LocationId into alGroup
                                   from al in alGroup.DefaultIfEmpty()
                                   join ia in _inventoryContext.InventoryAccounts.AsNoTracking() on al.AccountId equals ia.Id into iaGroup
                                   from ia in iaGroup.DefaultIfEmpty()
                                   where ia.RoleType == 0
                                   select new
                                   {
                                       ia.UserId,
                                       ia.UserName,
                                       il.Name,
                                       ia.RoleType
                                   }).ToListAsync();

            var result = from id in _inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.IsDeleted != true &&
                                                                                         x.InventoryId == listDocToExportInventoryErrorDto.InventoryId &&
                                                                                         x.AssignedAccountId == listDocToExportInventoryErrorDto.AssigneeAccountId &&
                                                                                         x.Plant == listDocToExportInventoryErrorDto.Plant).AsEnumerable()
                         join l in locations on id.AssignedAccountId equals l.UserId into lGroup
                         from l in lGroup.DefaultIfEmpty()
                         join cd in _inventoryContext.DocTypeCDetails.AsNoTracking() on id.Id equals cd.InventoryDocId into cdGroup
                         from cd in cdGroup.DefaultIfEmpty()
                             //where id.InventoryId == listDocToExportInventoryErrorDto.InventoryId &&
                             //      id.AssignedAccountId == listDocToExportInventoryErrorDto.AssigneeAccountId &&
                             //      id.Plant == listDocToExportInventoryErrorDto.Plant
                         let componentCode = id?.ComponentCode ?? cd?.ComponentCode
                         group new { id, l } by componentCode into g
                         select new ListDocToExportInventoryErrorModel
                         {
                             ComponentCode = g.Key,
                             ComponentName = g.Where(x => x.id.DocType == InventoryDocType.A).Select(x => x.id.ComponentName).FirstOrDefault() ?? string.Empty,
                             ErrorMoney = g.Where(x => x.id.DocType == InventoryDocType.A).Select(x => x.id.ErrorMoney).FirstOrDefault() ?? 0,
                             ErrorQuantity = g.Where(x => x.id.DocType == InventoryDocType.A).Select(x => x.id.ErrorQuantity).FirstOrDefault(),
                             TotalQuantity = g.Where(x => x.id.DocType == InventoryDocType.A).Select(x => x.id.TotalQuantity).FirstOrDefault(),
                             AccountQuantity = g.Where(x => x.id.DocType == InventoryDocType.A).Select(x => x.id.AccountQuantity).FirstOrDefault(),
                             detailDocuments = g.Select(x => new DetailDocument
                             {
                                 DocCode = x.id.DocCode ?? string.Empty,
                                 Quantity = x.id.Quantity,
                                 Location = x.l.Name ?? string.Empty,
                                 detailDocOutputs = x.id.DocType == InventoryDocType.C ?
                                                    _inventoryContext.DocTypeCDetails.AsNoTracking()
                                                    .Where(z => x.id.Id == z.InventoryDocId && g.Key == z.ComponentCode)
                                                    .Select(dtc => new DetailDocOutput
                                                    {
                                                        QuantityOfBom = dtc.QuantityOfBom,
                                                        QuantityPerBom = dtc.QuantityPerBom,
                                                    }).ToList() :
                                                    _inventoryContext.DocOutputs.AsNoTracking()
                                                    .Where(y => x.id.Id == y.InventoryDocId)
                                                    .Select(dop => new DetailDocOutput
                                                    {
                                                        QuantityOfBom = dop.QuantityOfBom,
                                                        QuantityPerBom = dop.QuantityPerBom,
                                                    }).ToList()
                             }).ToList()
                         };
            //Searching ComponentCode:
            if (!string.IsNullOrEmpty(listDocToExportInventoryErrorDto.ComponentCode))
            {
                result = result.Where(x => x.ComponentCode.Contains(listDocToExportInventoryErrorDto.ComponentCode));
            }
            //Searching ErrorMoney:
            if (listDocToExportInventoryErrorDto.ErrorMoney.HasValue)
            {
                result = result.Where(x => x.ErrorMoney.HasValue && Math.Abs(x.ErrorMoney.Value) >= listDocToExportInventoryErrorDto.ErrorMoney.Value);
            }
            //Searching ErrorQuantity:
            if (listDocToExportInventoryErrorDto.ErrorQuantity.HasValue)
            {
                result = result.Where(x => Math.Abs(x.ErrorQuantity) >= listDocToExportInventoryErrorDto.ErrorQuantity.Value);
            }

            return new ResponseModel<IEnumerable<ListDocToExportInventoryErrorModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = result,
            };
        }


        private bool ValidateRequiredImportMSLDataUpdate(MSLDataUpdateImportDto item)
        {
            
            //Validate Plant required:
            if (string.IsNullOrEmpty(item.Plant))
            {
                item.Error.TryAddModelError(nameof(item.Plant), "Vui lòng nhập plant.");
                return false;
            }
            //Validate WHLoc required:
            if (string.IsNullOrEmpty(item.WHloc))
            {
                item.Error.TryAddModelError(nameof(item.WHloc), "Vui lòng nhập whloc.");
                return false;
            }
            //Validate ComponentCode required:
            if (string.IsNullOrEmpty(item.ComponentCode))
            {
                item.Error.TryAddModelError(nameof(item.ComponentCode), "Vui lòng nhập mã linh kiện.");
                return false;
            }

            //Validate Quantity required:
            if (string.IsNullOrEmpty(item.Quantity))
            {
                item.Error.TryAddModelError(nameof(item.Quantity), "Vui lòng nhập số lượng.");
                return false;
            }


            if (!double.TryParse(item.Quantity.ToString(), out double convertedQuantity))
            {
                item.Error.TryAddModelError(nameof(item.Quantity), "Quantity: Số lượng không đúng.");
                return false;
            }

            return true;
        }
        private bool ValidateRequiredItemExistedImportMSLDataUpdate(List<MSLDataUpdateImportDto> items, MSLDataUpdateImportDto item, HashSet<string> existingRecords)
        {
            // Tạo key duy nhất cho mỗi bản ghi (ComponentCode - Plant - WHloc)
            string key = $"{item.ComponentCode}-{item.Plant}-{item.WHloc}";

            // Nếu đã tồn tại key này thì báo lỗi
            if (!existingRecords.Add(key))
            {
                item.Error.TryAddModelError(nameof(item.ComponentCode),
                    $"Đã tồn tại linh kiện {item.ComponentCode}, Plant {item.Plant} và WHLoc {item.WHloc} trong file import.");
                return false;
            }

            return true;
        }

        private bool ValidateExistedDocTypeAImportMSLDataUpdate(MSLDataUpdateImportDto item, List<InventoryDocTypeADto> inventoryDocTypeAs)
        {
            var checkExistedDocTypeA = inventoryDocTypeAs.Any(x => x.ComponentCode == item.ComponentCode &&
                                                                x.Plant == item.Plant &&
                                                                x.WHloc == item.WHloc);
            if (!checkExistedDocTypeA)
            {
                item.Error.TryAddModelError(nameof(item.ComponentCode),
                    $"Mã linh kiện {item.ComponentCode}, Plant {item.Plant} và WHLoc {item.WHloc} không tồn tại trong phiếu A.");
                return false;
            }
            return true;
        }

        private byte[] InvalidItemsImportMSLToExcel(List<MSLDataUpdateImportDto> invalidItems)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Lỗi khi cập nhật dữ liệu MSL");

                    // Đặt tiêu đề cho cột
                    int movementTypeIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.MovementType) + 1;
                    int dateTimeIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.DateTime) + 1;
                    int inputDateTimeIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.InputDateTime) + 1;
                    int deliveryNumberIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.DeliveryNumber) + 1;
                    int contentIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.Content) + 1;
                    int inOutDocIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.InOutDoc) + 1;
                    int plantIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.Plant) + 1;
                    int wHLocIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.WHLoc) + 1;
                    int componentCodeIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.ComponentCode) + 1;
                    int quantityIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.Quantity) + 1;
                    int unitIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.Unit) + 1;
                    int specialInventoryIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.SpecialInventory) + 1;
                    int orderNumberIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.OrderNumber) + 1;
                    int orderDetailNumberIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.OrderDetailNumber) + 1;
                    int gLAccountNumberIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.GLAccountNumber) + 1;
                    int costCenterIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.CostCenter) + 1;
                    int supplierAccountNumberIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.SupplierAccountNumber) + 1;
                    int reasonForMovingIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.ReasonForMoving) + 1;
                    int errorSummaryIndex = ImportMSLDataUpdateExcel.ExcelHeaders.IndexOf(ImportMSLDataUpdateExcel.ErrorSummary) + 1;

                    worksheet.Cells[1, movementTypeIndex].Value = ImportMSLDataUpdateExcel.MovementType;
                    worksheet.Cells[1, dateTimeIndex].Value = ImportMSLDataUpdateExcel.DateTime;
                    worksheet.Cells[1, inputDateTimeIndex].Value = ImportMSLDataUpdateExcel.InputDateTime;
                    worksheet.Cells[1, deliveryNumberIndex].Value = ImportMSLDataUpdateExcel.DeliveryNumber;
                    worksheet.Cells[1, contentIndex].Value = ImportMSLDataUpdateExcel.Content;
                    worksheet.Cells[1, inOutDocIndex].Value = ImportMSLDataUpdateExcel.InOutDoc;
                    worksheet.Cells[1, plantIndex].Value = ImportMSLDataUpdateExcel.Plant;
                    worksheet.Cells[1, wHLocIndex].Value = ImportMSLDataUpdateExcel.WHLoc;
                    worksheet.Cells[1, componentCodeIndex].Value = ImportMSLDataUpdateExcel.ComponentCode;
                    worksheet.Cells[1, quantityIndex].Value = ImportMSLDataUpdateExcel.Quantity;
                    worksheet.Cells[1, unitIndex].Value = ImportMSLDataUpdateExcel.Unit;
                    worksheet.Cells[1, specialInventoryIndex].Value = ImportMSLDataUpdateExcel.SpecialInventory;
                    worksheet.Cells[1, orderNumberIndex].Value = ImportMSLDataUpdateExcel.OrderNumber;
                    worksheet.Cells[1, orderDetailNumberIndex].Value = ImportMSLDataUpdateExcel.OrderDetailNumber;
                    worksheet.Cells[1, gLAccountNumberIndex].Value = ImportMSLDataUpdateExcel.GLAccountNumber;
                    worksheet.Cells[1, costCenterIndex].Value = ImportMSLDataUpdateExcel.CostCenter;
                    worksheet.Cells[1, supplierAccountNumberIndex].Value = ImportMSLDataUpdateExcel.SupplierAccountNumber;
                    worksheet.Cells[1, reasonForMovingIndex].Value = ImportMSLDataUpdateExcel.ReasonForMoving;
                    worksheet.Cells[1, errorSummaryIndex].Value = ImportMSLDataUpdateExcel.ErrorSummary;

                    // Định dạng tiêu đề
                    using (var range = worksheet.Cells[1, movementTypeIndex, 1, errorSummaryIndex])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.None;
                    }

                    // Ghi dữ liệu vào Excel
                    for (int i = 0; i < invalidItems.Count(); i++)
                    {
                        var item = invalidItems.ElementAtOrDefault(i);

                        var errMessage = item.Error.Values.SelectMany(x => x.Errors)
                                                             .Select(x => x.ErrorMessage)
                                                             .Distinct();

                        worksheet.Cells[i + 2, movementTypeIndex].Value = item.MovementType;
                        worksheet.Cells[i + 2, plantIndex].Value = item.Plant;
                        worksheet.Cells[i + 2, wHLocIndex].Value = item.WHloc;
                        worksheet.Cells[i + 2, componentCodeIndex].Value = item.ComponentCode;
                        worksheet.Cells[i + 2, quantityIndex].Value = item.Quantity;
                        worksheet.Cells[i + 2, errorSummaryIndex].Value = string.Join("\n", errMessage);

                        using (var errorRange = worksheet.Cells[i + 2, errorSummaryIndex, i + 2, errorSummaryIndex])
                        {
                            errorRange.Style.Font.Color.SetColor(Color.Red);
                            errorRange.Style.Fill.PatternType = ExcelFillStyle.None;
                        }
                    }

                    // Lưu file Excel
                    package.SaveAs(stream);
                }

                return stream.ToArray();
            }
        }

        public async Task<ImportResponseModel<byte[]>> ImportMSLDataUpdate(IFormFile file, Guid inventoryId)
        {
            string fileExtension = Path.GetExtension(file.FileName);
            string[] allowExtension = new[] { ".xlsx" };

            if (allowExtension.Contains(fileExtension) == false || file.Length < 0)
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành cập nhật dữ liệu MSL."
                };
            }

            List<MSLDataUpdateImportDto> items = new();
            using (var memStream = new MemoryStream())
            {
                file.CopyTo(memStream);
                using (var sourcePackage = new ExcelPackage(memStream))
                {
                    // Lấy danh sách ComponentCode, Plant, WHLoc các phiếu A trong InventoryDocs:
                    var inventoryDocsTypeAs = await _inventoryContext.InventoryDocs.AsNoTracking()
                                                    .Where(x => x.InventoryId == inventoryId &&
                                                                x.DocType == InventoryDocType.A)
                                                    .Select(x => new InventoryDocTypeADto
                                                    {
                                                        ComponentCode = x.ComponentCode,
                                                        Plant = x.Plant,
                                                        WHloc = x.WareHouseLocation
                                                    }).Distinct()
                                                    .ToListAsync();

                    var sourceSheet = sourcePackage.Workbook.Worksheets.FirstOrDefault();
                    int totalRowsCount = sourceSheet.Dimension.End.Row > 1 ? sourceSheet.Dimension.End.Row - 1 : sourceSheet.Dimension.End.Row;
                    var rows = Enumerable.Range(2, totalRowsCount).ToList();
                    if (sourceSheet != null)
                    {
                        sourceSheet.Cells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sourceSheet.Cells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                        sourceSheet.Cells.Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        //get header column
                        int movementTypeIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.MovementType);
                        int dateTimeIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.DateTime);
                        int inputDateTimeIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.InputDateTime);
                        int deliveryNumberIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.DeliveryNumber);
                        int contentIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.Content);
                        int inOutDocIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.InOutDoc);
                        int plantIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.Plant);
                        int wHLocIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.WHLoc);
                        int componentCodeIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.ComponentCode);
                        int quantityIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.Quantity);
                        int unitIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.Unit);
                        int specialInventoryIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.SpecialInventory);
                        int orderNumberIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.OrderNumber);
                        int orderDetailNumberIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.OrderDetailNumber);
                        int gLAccountNumberIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.GLAccountNumber);
                        int costCenterIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.CostCenter);
                        int supplierAccountNumberIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.SupplierAccountNumber);
                        int reasonForMovingIndex = sourceSheet.GetColumnIndex(Constants.ImportMSLDataUpdateExcel.ReasonForMoving);

                        int[] headers = new int[] {
                            movementTypeIndex,
                            dateTimeIndex,
                            inputDateTimeIndex,
                            deliveryNumberIndex,
                            contentIndex,
                            inOutDocIndex,
                            plantIndex,
                            wHLocIndex,
                            componentCodeIndex,
                            quantityIndex,
                            unitIndex,
                            specialInventoryIndex,
                            orderNumberIndex,
                            orderDetailNumberIndex,
                            gLAccountNumberIndex,
                            costCenterIndex,
                            supplierAccountNumberIndex,
                            reasonForMovingIndex
                        };

                        if (headers.Any(x => x == -1))
                        {
                            return new ImportResponseModel<byte[]>
                            {
                                Code = StatusCodes.Status400BadRequest,
                                Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành cập nhật dữ liệu MSL.",
                            };
                        }

                        foreach (var row in rows)
                        {
                            //Bỏ qua các dòng null
                            var isEmptyRow = sourceSheet.Cells[row, 1, row, sourceSheet.Dimension.Columns].All(x => string.IsNullOrEmpty(x.Value?.ToString()));
                            if (isEmptyRow)
                            {
                                continue;
                            }
                            var item = new MSLDataUpdateImportDto();
                            item.MovementType = sourceSheet.Cells[row, movementTypeIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.Plant = sourceSheet.Cells[row, plantIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.WHloc = sourceSheet.Cells[row, wHLocIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.ComponentCode = sourceSheet.Cells[row, componentCodeIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.Quantity = sourceSheet.Cells[row, quantityIndex].Value?.ToString()?.Trim() ?? string.Empty;

                            items.Add(item);
                        }
                        var existingRecords = new HashSet<string>();
                        //Validate STT
                        foreach (var item in items)
                        {
                            //validate:
                            //Check ComponentCode, Plant, WHLoc trùng dữ liệu trong file import thì báo lỗi:
                            var validateDupplicateItemsImport = ValidateRequiredItemExistedImportMSLDataUpdate(items, item, existingRecords);

                            //Các trường trong file import bắt buộc nhập dữ liệu:
                            var validateItemsImportResult = ValidateRequiredImportMSLDataUpdate(item);

                            //ComponentCode, Plant, WhLoc không tồn tại ở phiếu A thì báo lỗi tại file không cho upload file lên:
                            var validateExistedDocTypeA = ValidateExistedDocTypeAImportMSLDataUpdate(item, inventoryDocsTypeAs);
                        }
                    }
                }
            }

            if (items.Count == 0)
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }
            byte[] fileByteErrors = null;
            //Ghi vào excel các bản ghi bị lỗi
            var invalidItems = items.Where(x => x.Error.IsValid == false).ToList();

            if (invalidItems.Any())
            {
                fileByteErrors = InvalidItemsImportMSLToExcel(invalidItems);
            }
            //Sau khi đã tổng hợp các bản ghi từ file excel và đánh dấu các lỗi validate vào từng item
            //Với những item hợp lệ sẽ thực hiện những logic sau:
            //Từ quantity của từng item, sẽ cập nhật lại trường AccountQuantity trong bảng InventoryDocs:

            var validItems = items.Where(x => x.Error.IsValid).ToList();
            var userId = _httpContext.UserFromContext().InventoryLoggedInfo.AccountId.ToString();
            var componentCodes = validItems.Select(x => x.ComponentCode).Distinct().ToList();
            var plants = validItems.Select(x => x.Plant).Distinct().ToList();
            var whLocs = validItems.Select(x => x.WHloc).Distinct().ToList();

            // Lấy trước toàn bộ InventoryDocs cần cập nhật, lưu vào Dictionary để truy xuất nhanh
            var inventoryDocs = await _inventoryContext.InventoryDocs.AsNoTracking()
                                    .Where(x => x.InventoryId == inventoryId &&
                                                x.DocType == InventoryDocType.A &&
                                                componentCodes.Contains(x.ComponentCode) &&
                                                plants.Contains(x.Plant) &&
                                                whLocs.Contains(x.WareHouseLocation))
                                    .Distinct().ToListAsync();

            List<InventoryDoc> updatedInventoryDocs = new List<InventoryDoc>();

            foreach (var item in validItems)
            {
                var inventoryDoc = inventoryDocs.FirstOrDefault(x => x.ComponentCode == item.ComponentCode && x.Plant == item.Plant && x.WareHouseLocation == item.WHloc);
               
                if (inventoryDoc != null)
                {
                    //MovementType = 701: Tăng số lượng:
                    if (item.MovementType == "701")
                    {
                        if (double.TryParse(item.Quantity, out double quantity))
                        {
                            inventoryDoc.AccountQuantity += quantity;
                        }
                    }
                    //MovementType = 702: Giảm số lượng:
                    else if (item.MovementType == "702")
                    {
                        if (double.TryParse(item.Quantity, out double quantity))
                        {
                            inventoryDoc.AccountQuantity -= quantity;
                        }
                    }
                    inventoryDoc.ErrorQuantity = inventoryDoc.TotalQuantity - inventoryDoc.AccountQuantity;
                    inventoryDoc.ErrorMoney = inventoryDoc.ErrorQuantity * (inventoryDoc?.UnitPrice ?? 0);
                    inventoryDoc.UpdatedAt = DateTime.Now;
                    inventoryDoc.UpdatedBy = userId;
                    updatedInventoryDocs.Add(inventoryDoc);
                }
            }

            var list = new List<IEnumerable<InventoryDoc>>();

            if (updatedInventoryDocs.Count < 15_000)
                list.AddRange(updatedInventoryDocs.Chunk(150));
            else if (updatedInventoryDocs.Count < 25_000)
                list.AddRange(updatedInventoryDocs.Chunk(250));
            else
                list.AddRange(updatedInventoryDocs.Chunk(500));

            Parallel.ForEach(list, async batch =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
                    context.InventoryDocs.UpdateRange(batch);
                    await context.SaveChangesAsync();
                }
            });



            var failCount = items.Where(x => x.Error.IsValid == false).Count();
            var successCount = items.Where(x => x.Error.IsValid == true).Count();

            var fileType = Constants.FileResponse.ExcelType;
            var fileName = string.Format("CapnhatdulieuMSL_Fileloi_{0}", DateTime.Now.ToString(Constants.DatetimeFormat));

            return new ImportResponseModel<byte[]>
            {
                Bytes = fileByteErrors,
                Code = StatusCodes.Status200OK,
                FailCount = failCount,
                SuccessCount = successCount,
                FileType = fileType,
                FileName = fileName,
            };

        }


    }
}
