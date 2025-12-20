using BIVN.FixedStorage.Services.Common.API.Dto.Role;
using BIVN.FixedStorage.Services.Common.API.Helpers;
using Dapper;
using Inventory.API.Infrastructure.Entity;
using Microsoft.Identity.Client;

namespace BIVN.FixedStorage.Services.Inventory.API.Service
{
    public partial class InventoryService : IInventoryService
    {
        private readonly ILogger<InventoryService> _logger;
        private readonly IRestClient _resClient;
        private readonly HttpContext _httpContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly RestClientFactory _restClientFactory;
        //private readonly IDatabaseFactoryService _databaseFactory;
        private readonly IConfiguration _configuration;
        private readonly InventoryContext _inventoryContext;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IDataAggregationService _dataAggregationService;
        private readonly IMemoryCache _memoryCache;
        private readonly IInventoryWebService _inventoryService;

        private readonly RestClient _identityRestClient;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;

        private InventoryDocStatus[] ExcludeDocStatus = new InventoryDocStatus[]
        {
            InventoryDocStatus.NotReceiveYet,
            InventoryDocStatus.NoInventory
        };
        public InventoryService(InventoryContext inventoryContext,
                              IHttpContextAccessor httpContextAccessor,
                              IRestClient restClient,
                              IConfiguration configuration,
                              ILogger<InventoryService> logger,
                              IServiceScopeFactory serviceProviderFactory,
                              IWebHostEnvironment webHostEnvironment,
                              RestClientFactory restClientFactory,
                              IDataAggregationService dataAggregationService,
                              IInventoryWebService inventoryService,
                              IBackgroundTaskQueue backgroundTaskQueue
                            )
        {
            _inventoryContext = inventoryContext;
            _logger = logger;
            _configuration = configuration;
            _resClient = restClient;
            _serviceScopeFactory = serviceProviderFactory;
            _httpContext = httpContextAccessor.HttpContext;
            _webHostEnvironment = webHostEnvironment;
            _restClientFactory = restClientFactory;
            _identityRestClient = _restClientFactory.IdentityClient();
            _dataAggregationService = dataAggregationService;
            _inventoryService = inventoryService;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        public async Task<ResponseModel<InventoryDocumentImportResultDto>> ImportInventoryDocumentAsync([FromForm] IFormFile file, string type, Guid inventoryId)
        {
            var result = new ResponseModel<InventoryDocumentImportResultDto>(StatusCodes.Status200OK, new InventoryDocumentImportResultDto());
            if (file == null)
            {
                result.Code = StatusCodes.Status500InternalServerError;
                result.Message = "File import không tồn tại";
                return await Task.FromResult(result);
            }
            else if (!file.FileName.EndsWith(".xlsx") && file.ContentType != FileResponse.ExcelType)
            {
                result.Code = (int)HttpStatusCodes.InvalidFileExcel;
                result.Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo phiếu kiểm kê.";
                return await Task.FromResult(result);
            }

            //Không có phiếu A thì phải tạo phiếu A trước
            var hasDocTypeA = await _inventoryContext.InventoryDocs.AsNoTracking().AnyAsync(x => x.InventoryId == inventoryId && x.DocType == InventoryDocType.A);
            if (!hasDocTypeA && type == nameof(InventoryDocType.B))
            {
                result.Code = (int)HttpStatusCodes.NotExistDocTypeA;
                result.Message = "Vui lòng tạo phiếu A trước khi thực hiện tạo phiếu B&E.";
                return await Task.FromResult(result);
            }

            switch (type)
            {
                case nameof(InventoryDocType.A):
                    result.Data = await ImportInventoryDocTypeA(file, inventoryId);
                    break;
                case nameof(InventoryDocType.B):
                    result.Data = await ImportInventoryDocTypeB(file, inventoryId);
                    break;
                case nameof(InventoryDocType.E):
                    result.Data = await ImportInventoryDocTypeE(file, inventoryId);
                    break;
                default:
                    result.Data = await ImportInventoryDocTypeB(file, inventoryId);
                    break;
            }


            return result;

        }

        public async Task<ResponseModel> CheckInventory(string inventoryId, string accountId)
        {
            //20240910 - Update Logic: Nếu Tài khoản có vai trò Xúc Tiến thì vẫn được vào và thực hiện kiểm kê, giám sát:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                //Check Tồn tại phiếu kiểm kê:
                var checkExistInventory = await _inventoryContext.Inventories.FirstOrDefaultAsync(x => x.Id.ToString().ToLower() == inventoryId.ToLower());
                if (checkExistInventory == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = $"Không tìm thấy phiếu kiểm kê.",
                    };
                }

                //Check đã tới ngày kiểm kê hay không:
                if (DateTime.Now.Date > checkExistInventory.InventoryDate.Date)
                {
                    _logger.LogInformation("[INF] Current date:{0} - InventoryDate:{1} ", DateTime.Now.Date, checkExistInventory.InventoryDate.Date);
                    return new ResponseModel
                    {
                        Code = (int)HttpStatusCodes.NotYetInventoryDate,
                        Message = $"Không thể xác nhận kiểm kê vì đã quá ngày kiểm kê của đợt kiểm kê hiện tại. Vui lòng thử lại sau.",
                    };

                }
            }

            //Kiểm tra xem có những loại phiếu nào:

            var getInventoryDocsQuery = _inventoryContext.InventoryDocs.Where(x => (x.Status != InventoryDocStatus.NotReceiveYet || x.Status != InventoryDocStatus.NoInventory) &&
                                                                                    x.InventoryId.ToString().ToLower() == inventoryId.ToLower());

            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                getInventoryDocsQuery = getInventoryDocsQuery.Where(x => x.AssignedAccountId.ToString().ToLower() == accountId.ToLower());
            }

            var getInventoryDocs = await getInventoryDocsQuery.ToListAsync();

            //check tài khoản đã được assign vào phiểu kiểm kê hay chưa?
            if (getInventoryDocs.Count() == 0)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.NotAssigneeAccountId,
                    Message = $"Tài khoản chưa được assign vào phiếu kiểm kê.",
                };
            }

            var checkInventory = new InventoryCheckModel()
            {
                IsDocTypeAE = getInventoryDocs.Any(x => x.DocType == InventoryDocType.A || x.DocType == InventoryDocType.E) == true ? true : false,
                IsDocTypeC = getInventoryDocs.Any(x => x.DocType == InventoryDocType.C) == true ? true : false,
                IsDocTypeB = getInventoryDocs.Any(x => x.DocType == InventoryDocType.B) == true ? true : false,
            };
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Loại phiếu kiểm kê",
                Data = checkInventory
            };
        }
        private async Task<InventoryDocumentImportResultDto> ImportInventoryDocTypeA(IFormFile file, Guid inventoryId)
        {
            using (var memStream = new MemoryStream())
            {
                await file.CopyToAsync(memStream);
                var checkDto = new CheckInventoryDocumentDto();
                var resultDto = new InventoryDocumentImportResultDto();
                using (var sourcePackage = new ExcelPackage(memStream))
                {
                    var sourceSheet = sourcePackage.Workbook.Worksheets.FirstOrDefault();
                    var totalRowsCount = sourceSheet.Dimension.End.Row > 1 ? sourceSheet.Dimension.End.Row - 1 : sourceSheet.Dimension.End.Row;
                    var rows = Enumerable.Range(2, totalRowsCount).ToList();
                    var validRows = new List<int>();
                    if (sourceSheet != null)
                    {
                        //Kiểm tra sai cột, thiếu cột so với file mẫu
                        var PlantIndex = sourceSheet.GetColumnIndex(TypeA.Plant);
                        var ComponentCodeIndex = sourceSheet.GetColumnIndex(TypeA.ComponentCode);
                        var PositionCodeIndex = sourceSheet.GetColumnIndex(TypeA.PositionCode);
                        var PhysInvIndex = sourceSheet.GetColumnIndex(TypeA.PhysInv);
                        var ComponentNameIndex = sourceSheet.GetColumnIndex(TypeA.ComponentName);
                        var WarehouseLocationIndex = sourceSheet.GetColumnIndex(TypeA.WarehouseLocation);
                        var SpecialStockIndex = sourceSheet.GetColumnIndex(TypeA.SpecialStock);

                        var requiredHeader = new[] {
                            PlantIndex,
                            ComponentCodeIndex,
                            PositionCodeIndex,
                            PhysInvIndex,
                            ComponentNameIndex,
                            WarehouseLocationIndex,
                            SpecialStockIndex
                        };

                        if (requiredHeader.Any(x => x == -1))
                        {
                            return new InventoryDocumentImportResultDto
                            {
                                Code = (int)HttpStatusCodes.InvalidFileExcel,
                                Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo phiếu kiểm kê."
                            };
                        }

                        var failCount = 0;
                        //get last type A document number
                        var lastDocNumberAndAssignee = await GetLastDocNumberAndAssignee(InventoryDocType.A, inventoryId);

                        //Get all Roles:
                        var request = new RestRequest(Constants.Endpoint.Internal.Get_All_Roles_Department);
                        request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
                        request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

                        var roles = await _identityRestClient.ExecuteGetAsync(request);

                        var rolesModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<GetAllRoleWithUserNameModel>>>(roles.Content ?? string.Empty);

                        var createDocumentRoles = rolesModel?.Data;
                        //init list inventory document entity
                        var inventoryDocuments = new List<InventoryDoc>();
                        var errorColumn = sourceSheet.Dimension.End.Column + 1;

                        var inventoryDocCellDtoList = new List<InventoryDocCellDto>();

                        for (var row = sourceSheet.Dimension.Start.Row + 1; row <= sourceSheet.Dimension.End.Row; row++)

                        {
                            try
                            {

                                //get data cell value
                                var dataFromCell = GetDataFromCell(sourceSheet, row);
                                //check for all blank rows
                                if (CheckRequiredColumn(true, dataFromCell.Plant, dataFromCell.WarehouseLocation, dataFromCell.ModelCode, dataFromCell.ComponentName, dataFromCell.ComponentCode, dataFromCell.PhysInv, dataFromCell.SpecialStock, dataFromCell.PositionCode))
                                {
                                    if (row < sourceSheet.Dimension.Rows)
                                    {
                                        sourceSheet.Row(row).Hidden = true;

                                    }
                                    continue;

                                }

                                if (dataFromCell != null)
                                {
                                    inventoryDocCellDtoList.Add(dataFromCell);
                                }

                                //validate data
                                var validateData = ValidateCellData(inventoryId, createDocumentRoles, lastDocNumberAndAssignee, dataFromCell, sourceSheet, row, checkDto, inventoryDocCellDtoList, nameof(TypeA), errorColumn);
                                if (validateData.IsValid)
                                {
                                    validRows.Add(row);
                                }

                                if (!validateData.IsValid)
                                {
                                    failCount += validateData.FailCount;
                                    continue;
                                }

                                SetDataToEntites(inventoryId, lastDocNumberAndAssignee, inventoryDocuments, dataFromCell);

                            }
                            catch (Exception exception)
                            {
                                var exMess = $"Exception - {exception.Message} at row {row}";
                                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                                _logger.LogError($"Request error at {_httpContext.Request.Path} : {exMess}; {innerExMess}");
                                continue;
                            }
                        }

                        AddDataToDb(inventoryDocuments);

                        //Thêm tiêu đồ cột nội dung lỗi trong file excel
                        if (failCount > 0)
                        {
                            sourceSheet.Cells[1, sourceSheet.Dimension.Columns].Value = TypeA.ErrorContent;
                        }

                        resultDto.FailCount = failCount;
                        resultDto.SuccessCount = inventoryDocuments.Count;
                    }
                    try
                    {

                        var errorWorksheet = sourcePackage.Workbook.Worksheets.Copy(sourceSheet.Name, sourceSheet.Name + " lỗi");
                        sourceSheet.Workbook.Worksheets.Delete(sourceSheet.Name);
                        foreach (var row in validRows)
                        {
                            errorWorksheet.Cells[row, 1, row, errorWorksheet.Dimension.End.Column].Clear();
                            errorWorksheet.Row(row).Hidden = true;
                        }

                    }
                    catch (Exception ex)
                    {

                        _logger.LogError(ex.Message);
                    }
                    resultDto.Result = sourcePackage.GetAsByteArray();

                }
                return resultDto;
            }

        }

        private void AddDataToDb(List<InventoryDoc> inventoryDocuments)
        {
            //parallel insert
            var list = new List<IEnumerable<InventoryDoc>>();

            for (var i = 1; i <= 100; i++)
            {
                if (inventoryDocuments.Count < 15_000)
                    list.Add(inventoryDocuments.Skip((i - 1) * 150).Take(150));
                else if (inventoryDocuments.Count < 25_000)
                    list.Add(inventoryDocuments.Skip((i - 1) * 250).Take(250));
                else if (inventoryDocuments.Count < 50_000)
                    list.Add(inventoryDocuments.Skip((i - 1) * 500).Take(500));
            }
            Parallel.ForEach(list, async invDocs =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {

                    var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
                    {
                        await context.InventoryDocs.AddRangeAsync(invDocs);
                        await context.SaveChangesAsync();
                    }
                }
            });
        }

        private void AddDataToDb(List<InventoryDoc> inventoryDocuments, List<DocOutput> docOutputs, List<DocHistory> docHistories, List<HistoryOutput> historyOutputs)
        {
            //parallel insert
            var list = new List<IEnumerable<InventoryDoc>>();

            for (var i = 1; i <= 100; i++)
            {
                if (inventoryDocuments.Count < 15_000)
                    list.Add(inventoryDocuments.Skip((i - 1) * 150).Take(150));
                else if (inventoryDocuments.Count < 25_000)
                    list.Add(inventoryDocuments.Skip((i - 1) * 250).Take(250));
                else if (inventoryDocuments.Count < 50_000)
                    list.Add(inventoryDocuments.Skip((i - 1) * 500).Take(500));
            }
            Parallel.ForEach(list, async invDocs =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {

                    var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
                    {
                        var docOutputsList = docOutputs.Where(x => invDocs.Any(y => y.Id == x.InventoryDocId)).ToList();
                        var docHistoriesList = docHistories.Where(x => invDocs.Any(y => y.Id == x.InventoryDocId)).ToList();

                        context.InventoryDocs.AddRange(invDocs);

                        context.DocOutputs.AddRange(docOutputsList);
                        context.DocHistories.AddRange(docHistoriesList);

                        await context.SaveChangesAsync();
                    }
                }
            });

            var listHistoryOutput = new List<IEnumerable<HistoryOutput>>();


            for (var i = 1; i <= 100; i++)
            {
                if (historyOutputs.Count < 15_000)
                    listHistoryOutput.Add(historyOutputs.Skip((i - 1) * 150).Take(150));
                else if (historyOutputs.Count < 25_000)
                    listHistoryOutput.Add(historyOutputs.Skip((i - 1) * 250).Take(250));
                else if (historyOutputs.Count < 50_000)
                    listHistoryOutput.Add(historyOutputs.Skip((i - 1) * 500).Take(500));
            }
            Parallel.ForEach(listHistoryOutput, async historyOutputs =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {

                    var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
                    {
                        context.HistoryOutputs.AddRange(historyOutputs);
                        await context.SaveChangesAsync();
                    }
                }
            });
        }

        private void SetDataToEntites(Guid inventoryId, InventoryDocAndUserDto lastDocNumberAndAssignee, List<InventoryDoc> inventoryDocuments, InventoryDocCellDto dataFromCell, string docType = nameof(TypeA), bool isDocShip = false)
        {

            //get map user 
            var assignedUser = lastDocNumberAndAssignee.Assignees.FirstOrDefault(x => x.UserName == dataFromCell.Assignee);

            var getInventoryDocDetail = docType != nameof(TypeA) ? lastDocNumberAndAssignee.DocAComponentNames.FirstOrDefault(x => x.ComponentCode == dataFromCell.ComponentCode
                                           && x.Plant == dataFromCell.Plant & x.WHLoc == dataFromCell.WarehouseLocation) : null;

            //create entity
            var inventoryDoc = new InventoryDoc
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                //CreatedBy = Constants.Roles.ID_Administrator,
                CreatedBy = _httpContext.CurrentUser().UserCode,
                Plant = dataFromCell.Plant,
                WareHouseLocation = dataFromCell.WarehouseLocation,

                StockType = dataFromCell.StockType,
                SalesOrderNo = dataFromCell.SONo,
                SaleOrderList = dataFromCell.SOList,
                ComponentCode = dataFromCell.ComponentCode,
                ComponentName = dataFromCell.ComponentName,

                PositionCode = dataFromCell.PositionCode,
                Note = dataFromCell.Note,
                AssignedAccountId = assignedUser?.UserId ?? null,
                LocationName = assignedUser?.LocationName,
                DepartmentName = assignedUser?.DepartmentName,
                InventoryId = inventoryId,
                No = dataFromCell.RowNumber
            };

            if (docType == nameof(TypeA))
            {
                inventoryDoc.Item = dataFromCell.Item;
                inventoryDoc.Quantity = dataFromCell.Quantity;
                inventoryDoc.PhysInv = dataFromCell.PhysInv;
                inventoryDoc.FiscalYear = dataFromCell.FiscalYear;
                inventoryDoc.PlannedCountDate = dataFromCell.PlannedCountDate;
                inventoryDoc.PhysInv = dataFromCell.PhysInv;
                inventoryDoc.ColumnC = dataFromCell.CCol;
                inventoryDoc.SpecialStock = dataFromCell.SpecialStock;
                inventoryDoc.ColumnN = dataFromCell.NCol;
                inventoryDoc.ColumnO = dataFromCell.OCol;
                inventoryDoc.ColumnP = dataFromCell.PCol;
                inventoryDoc.ColumnQ = dataFromCell.QCol;
                inventoryDoc.ColumnR = dataFromCell.RCol;
                inventoryDoc.ColumnS = dataFromCell.SCol;
                inventoryDoc.DocCode = $"{lastDocNumberAndAssignee.InventoryPart}{lastDocNumberAndAssignee.LastDocNumber++:00000}";
                inventoryDoc.StorageBin = dataFromCell.PositionCode;
                inventoryDoc.DocType = InventoryDocType.A;
                if (isDocShip)
                {
                    inventoryDoc.InventoryBy = _httpContext.CurrentUser().Username;
                    inventoryDoc.InventoryAt = DateTime.Now;
                    inventoryDoc.ConfirmBy = _httpContext.CurrentUser().Username;
                    inventoryDoc.ConfirmAt = DateTime.Now;
                }
            }
            else if (docType == nameof(TypeC))
            {

            }
            else
            {

                inventoryDoc.ModelCode = dataFromCell.ModelCode;
                inventoryDoc.MachineModel = dataFromCell.MachineModel;
                inventoryDoc.MachineType = dataFromCell.MachineType;
                inventoryDoc.LineName = dataFromCell.LineName;
                inventoryDoc.ComponentName = getInventoryDocDetail?.ComponentName ?? dataFromCell.ComponentName;

                if (isDocShip)
                {
                    inventoryDoc.Quantity = dataFromCell.Quantity;
                    inventoryDoc.AssignedAccountId = assignedUser?.UserId ?? Guid.Parse(_httpContext.CurrentUser().UserId);
                    inventoryDoc.InventoryBy = _httpContext.CurrentUser().Username;
                    inventoryDoc.InventoryAt = DateTime.Now;
                    inventoryDoc.ConfirmBy = _httpContext.CurrentUser().Username;
                    inventoryDoc.ConfirmAt = DateTime.Now;
                }

                //inventoryDoc.SpecialStock = dataFromCell.SpecialStock;
                //inventoryDoc.ModelCode = dataFromCell.ModelCode;
                //inventoryDoc.ProductOrderNo = dataFromCell.ProOrderNo;
                //inventoryDoc.VendorCode = dataFromCell.VendorCode;

                if (!string.IsNullOrEmpty(dataFromCell.AssemblyLoc))//TypeB
                {
                    inventoryDoc.DocCode = $"{lastDocNumberAndAssignee.InventoryPart.Replace("E", string.Empty)}{lastDocNumberAndAssignee.LastDocNumberTypeB++:00000}";
                    inventoryDoc.AssemblyLocation = dataFromCell.AssemblyLoc;
                    inventoryDoc.PositionCode = dataFromCell.AssemblyLoc;
                    inventoryDoc.DocType = InventoryDocType.B;
                }
                else if (!string.IsNullOrEmpty(dataFromCell.StorageBin))
                {
                    inventoryDoc.DocCode = $"{lastDocNumberAndAssignee.InventoryPart.Replace("B", string.Empty)}{lastDocNumberAndAssignee.LastDocNumberTypeE++:00000}";
                    inventoryDoc.StorageBin = dataFromCell.StorageBin;
                    inventoryDoc.PositionCode = dataFromCell.StorageBin;
                    inventoryDoc.DocType = InventoryDocType.E;

                    if (isDocShip)
                    {
                        inventoryDoc.SalesOrderNo = dataFromCell.SONo;
                        inventoryDoc.SaleOrderList = dataFromCell.SOList;
                    }
                }

            }



            //set status
            if (string.IsNullOrEmpty(dataFromCell.Assignee))
            {
                inventoryDoc.Status = InventoryDocStatus.NoInventory;
            }
            else
            {
                inventoryDoc.Status = InventoryDocStatus.NotReceiveYet;
            }

            if (isDocShip)
            {
                inventoryDoc.Status = InventoryDocStatus.Confirmed;
            }

            inventoryDocuments.Add(inventoryDoc);
        }
        private void SetDataToEntites(Guid inventoryId, InventoryDocAndUserDto lastDocNumberAndAssignee, List<AuditTarget> auditTargets, InventoryDocCellDto dataFromCell)
        {

            //get map user 
            var assignedUser = lastDocNumberAndAssignee.Assignees.FirstOrDefault(x => x.UserName == dataFromCell.Assignee && x.LocationName == dataFromCell.LocationName);

            //create entity

            var auditTarget = new AuditTarget
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                //CreatedBy = Constants.Roles.ID_Administrator,
                CreatedBy = _httpContext.CurrentUser().UserId,
                Plant = dataFromCell.Plant,
                WareHouseLocation = dataFromCell.WarehouseLocation,
                SaleOrderNo = dataFromCell.SONo,

                ComponentCode = dataFromCell.ComponentCode,
                //ComponentName = dataFromCell.ComponentName,

                PositionCode = dataFromCell.PositionCode,

                AssignedAccountId = assignedUser.UserId,
                LocationName = dataFromCell.LocationName,
                DepartmentName = assignedUser?.DepartmentName,
                InventoryId = inventoryId,
                Status = AuditTargetStatus.NotYet
            };


            auditTargets.Add(auditTarget);
        }
        private InventoryDocCellDto GetDataFromCell(ExcelWorksheet sourceSheet, int row, string docType = nameof(TypeA), bool isDocShip = false)
        {
            var inventoryDocCellDto = new InventoryDocCellDto
            {
                Plant = GetCellValue(sourceSheet, row, TypeA.Plant),
                ComponentCode = GetCellValue(sourceSheet, row, TypeA.ComponentCode),
                Note = docType != "AuditTarget" ? GetCellValue(sourceSheet, row, TypeA.Note) : null,
                Assignee = GetCellValue(sourceSheet, row, TypeA.Assignee),
                RowNumber = row.ToString()
            };
            if (docType == nameof(TypeA))
            {
                inventoryDocCellDto.WarehouseLocation = GetCellValue(sourceSheet, row, TypeA.WarehouseLocation);
                inventoryDocCellDto.StockType = GetCellValue(sourceSheet, row, TypeA.StockTypes);
                inventoryDocCellDto.SpecialStock = GetCellValue(sourceSheet, row, TypeA.SpecialStock);
                inventoryDocCellDto.CCol = GetCellValue(sourceSheet, row, TypeA.C);
                inventoryDocCellDto.PhysInv = GetCellValue(sourceSheet, row, TypeA.PhysInv);
                int.TryParse(GetCellValue(sourceSheet, row, TypeA.FiscalYear), out var fy);
                inventoryDocCellDto.FiscalYear = fy;
                inventoryDocCellDto.Item = GetCellValue(sourceSheet, row, TypeA.Item);
                inventoryDocCellDto.PlannedCountDate = GetCellValue(sourceSheet, row, TypeA.PlannedCountDate);
                inventoryDocCellDto.NCol = GetCellValue(sourceSheet, row, TypeA.N);
                inventoryDocCellDto.OCol = GetCellValue(sourceSheet, row, TypeA.O);
                inventoryDocCellDto.PCol = GetCellValue(sourceSheet, row, TypeA.P);
                inventoryDocCellDto.QCol = GetCellValue(sourceSheet, row, TypeA.Q);
                inventoryDocCellDto.RCol = GetCellValue(sourceSheet, row, TypeA.R);
                inventoryDocCellDto.SCol = GetCellValue(sourceSheet, row, TypeA.S);
                inventoryDocCellDto.PositionCode = GetCellValue(sourceSheet, row, TypeA.PositionCode);
                inventoryDocCellDto.StorageBin = GetCellValue(sourceSheet, row, TypeA.PositionCode);
                inventoryDocCellDto.ComponentName = GetCellValue(sourceSheet, row, TypeA.ComponentName);
                inventoryDocCellDto.SONo = GetCellValue(sourceSheet, row, TypeA.SONo);
                inventoryDocCellDto.SOList = GetCellValue(sourceSheet, row, TypeA.SOList);
                double.TryParse(GetCellValue(sourceSheet, row, TypeA.Quantity), out var quantity);
                inventoryDocCellDto.Quantity = quantity;
            }
            else if (docType == nameof(TypeC))
            {
                //moved to new method due to complication logic
            }
            else if (docType == nameof(TypeB))
            {
                inventoryDocCellDto.Plant = GetCellValue(sourceSheet, row, TypeB.Plant);
                inventoryDocCellDto.WarehouseLocation = GetCellValue(sourceSheet, row, TypeB.WarehouseLocation);
                inventoryDocCellDto.ComponentCode = GetCellValue(sourceSheet, row, TypeB.ComponentCode);
                inventoryDocCellDto.StockType = GetCellValue(sourceSheet, row, TypeB.StockType);
                inventoryDocCellDto.ModelCode = GetCellValue(sourceSheet, row, TypeB.ModelCode);
                inventoryDocCellDto.AssemblyLoc = GetCellValue(sourceSheet, row, TypeB.AssemblyLoc);
                inventoryDocCellDto.MachineModel = GetCellValue(sourceSheet, row, TypeB.MachineModel);
                inventoryDocCellDto.MachineType = GetCellValue(sourceSheet, row, TypeB.MachineType);
                inventoryDocCellDto.LineName = GetCellValue(sourceSheet, row, TypeB.LineName);


                //inventoryDocCellDto.ProOrderNo = GetCellValue(sourceSheet, row, TypeB.ProOrderNo);
                //inventoryDocCellDto.VendorCode = GetCellValue(sourceSheet, row, TypeB.VendorCode);
                //inventoryDocCellDto.ComponentName = GetCellValue(sourceSheet, row, TypeA.ComponentName);
                //inventoryDocCellDto.StorageBin = GetCellValue(sourceSheet, row, TypeB.StorageBin);
                //inventoryDocCellDto.SpecialStock = GetCellValue(sourceSheet, row, TypeB.SpecialStock);
                //inventoryDocCellDto.SONo = GetCellValue(sourceSheet, row, TypeB.SONo);
                //inventoryDocCellDto.SOList = GetCellValue(sourceSheet, row, TypeB.SOList);
            }
            else if (docType == nameof(TypeE))
            {
                inventoryDocCellDto.Plant = GetCellValue(sourceSheet, row, TypeE.Plant);
                inventoryDocCellDto.WarehouseLocation = GetCellValue(sourceSheet, row, isDocShip ? TypeA.WarehouseLocation : TypeE.WarehouseLocation);
                inventoryDocCellDto.ComponentCode = GetCellValue(sourceSheet, row, TypeE.ComponentCode);
                inventoryDocCellDto.StockType = GetCellValue(sourceSheet, row, TypeE.StockType);
                inventoryDocCellDto.ModelCode = GetCellValue(sourceSheet, row, TypeE.ModelCode);
                inventoryDocCellDto.StorageBin = GetCellValue(sourceSheet, row, TypeE.StorageBin);

                if (isDocShip)
                {
                    double.TryParse(GetCellValue(sourceSheet, row, TypeA.Quantity), out var quantity);
                    inventoryDocCellDto.Quantity = quantity;
                    inventoryDocCellDto.ComponentName = GetCellValue(sourceSheet, row, TypeE.ComponentName);
                    inventoryDocCellDto.SONo = GetCellValue(sourceSheet, row, TypeA.SONo);
                    inventoryDocCellDto.SOList = GetCellValue(sourceSheet, row, TypeA.SOList);
                }
            }
            else//audit target
            {
                inventoryDocCellDto.SONo = GetCellValue(sourceSheet, row, AuditTargets.SONo);
                inventoryDocCellDto.PositionCode = GetCellValue(sourceSheet, row, AuditTargets.PositionCode);
                inventoryDocCellDto.LocationName = sourceSheet.Name;
                inventoryDocCellDto.WarehouseLocation = GetCellValue(sourceSheet, row, AuditTargets.WarehouseLocation);
            }
            return inventoryDocCellDto;
        }

        private ValidateCellDto ValidateCellData(Guid inventoryId, IEnumerable<GetAllRoleWithUserNameModel> rolesModel, InventoryDocAndUserDto lastDocNumberAndAssignee, InventoryDocCellDto dataFromCell, ExcelWorksheet sourceSheet, int row, CheckInventoryDocumentDto checkDto, List<InventoryDocCellDto> inventoryDocCellDtoList, string docType = nameof(TypeA), int errorColumnIndex = 24, bool isDocShip = false)
        {
            //Get all Roles:
            //var request = new RestRequest(Constants.Endpoint.Internal.Get_All_Roles_Department);
            //request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            //request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            //var roles = _identityRestClient.GetAsync(request).GetAwaiter().GetResult();

            //var rolesModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<GetAllRoleWithUserNameModel>>>(roles.Content ?? string.Empty);

            var validateCellDto = new ValidateCellDto();
            var errorStrBuilder = new StringBuilder();

            //Check data exist in rows Excel File Import:

            if (inventoryDocCellDtoList.Any())
            {
                var checkExistRows = inventoryDocCellDtoList.Where(item =>
                                                                item.Plant == dataFromCell.Plant &&
                                                                item.WarehouseLocation == dataFromCell.WarehouseLocation &&
                                                                item.CCol == dataFromCell.CCol &&
                                                                item.SpecialStock == dataFromCell.SpecialStock &&
                                                                item.StockType == dataFromCell.StockType &&
                                                                item.SONo == dataFromCell.SONo &&
                                                                item.SOList == dataFromCell.SOList &&
                                                                item.PhysInv == dataFromCell.PhysInv &&
                                                                item.FiscalYear == dataFromCell.FiscalYear &&
                                                                item.Item == dataFromCell.Item &&
                                                                item.PlannedCountDate == dataFromCell.PlannedCountDate &&
                                                                item.ComponentCode == dataFromCell.ComponentCode &&
                                                                item.ComponentName == dataFromCell.ComponentName &&
                                                                item.NCol == dataFromCell.NCol &&
                                                                item.OCol == dataFromCell.OCol &&
                                                                item.PCol == dataFromCell.PCol &&
                                                                item.QCol == dataFromCell.QCol &&
                                                                item.RCol == dataFromCell.RCol &&
                                                                item.SCol == dataFromCell.SCol &&
                                                                item.PositionCode == dataFromCell.PositionCode &&
                                                                item.Note == dataFromCell.Note &&
                                                                item.Assignee == dataFromCell.Assignee &&
                                                                item.Quantity == dataFromCell.Quantity &&
                                                                item.ModelCode == dataFromCell.ModelCode &&
                                                                item.StorageBin == dataFromCell.StorageBin &&
                                                                item.AssemblyLoc == dataFromCell.AssemblyLoc &&
                                                                item.ProOrderNo == dataFromCell.ProOrderNo &&
                                                                item.VendorCode == dataFromCell.VendorCode &&
                                                                item.LocationName == dataFromCell.LocationName &&
                                                                item.MachineModel == dataFromCell.MachineModel &&
                                                                item.MachineType == dataFromCell.MachineType &&
                                                                item.LineName == dataFromCell.LineName).ToList();

                var existedTypeBInSheet = inventoryDocCellDtoList.Where(x => x.ComponentCode == dataFromCell.ComponentCode && x.MachineModel == dataFromCell.MachineModel).ToList();
                if (existedTypeBInSheet.Any() && existedTypeBInSheet.Count > 1 && docType == nameof(TypeB))
                {
                    errorStrBuilder.Append($"Thông tin mã linh kiện {dataFromCell.ComponentCode} có plant {dataFromCell.Plant} và WH Loc. {dataFromCell.WarehouseLocation} đã bị trùng lặp Model {dataFromCell.MachineModel}  trong file import.\n");
                    validateCellDto.IsValid = false;
                }

                if (checkExistRows.Any() && checkExistRows.Count() > 1)
                {
                    errorStrBuilder.Append($"Thông tin mã linh kiện {dataFromCell.ComponentCode} có plant {dataFromCell.Plant} và WH Loc. {dataFromCell.WarehouseLocation} đã bị trùng lặp trong file import.\n");
                    validateCellDto.IsValid = false;
                }
            }

            //validate here

            //check existed for doc ship
            if (isDocShip)
            {
                var checkExist = lastDocNumberAndAssignee.InvDocs.Any(x => x.ComponentCode == dataFromCell.ComponentCode
                                           && x.Plant == dataFromCell.Plant & x.WHLoc == dataFromCell.WarehouseLocation && x.SaleOrderNo == dataFromCell.SONo && x.PositionCode == dataFromCell.StorageBin);
                if (checkExist)
                {
                    errorStrBuilder.Append($"Thông tin mã linh kiện {dataFromCell.ComponentCode} có plant {dataFromCell.Plant}, WH Loc. {dataFromCell.WarehouseLocation},SONo {dataFromCell.SONo} và Storage bin {dataFromCell.StorageBin} đã tồn tại trên hệ thống.\n");
                    validateCellDto.IsValid = false;
                }
            }

            if (docType == nameof(TypeA))
            {

                var dataProps = dataFromCell.GetType().GetProperties();

                var requiredColumns = TypeA.RequiredColumns;
                if (isDocShip)
                {
                    requiredColumns = requiredColumns.Append(nameof(TypeA.Assignee)).ToArray();
                    requiredColumns = requiredColumns.Where(x => x != nameof(TypeA.PhysInv) && x != nameof(TypeA.SpecialStock)).ToArray();

                }

                foreach (var item in requiredColumns)
                {
                    var prop = dataProps.FirstOrDefault(x => x.Name == item);
                    if (prop != null && string.IsNullOrEmpty((string)prop.GetValue(dataFromCell, null)))
                    {
                        if (item != nameof(TypeA.ComponentName) && item != nameof(TypeA.PositionCode) && item != nameof(TypeA.ComponentCode) && item != nameof(TypeA.Assignee))
                        {
                            errorStrBuilder.Append($" Vui lòng nhập giá trị cho {item}.\n");
                        }
                        else if (item == nameof(TypeA.ComponentName))
                        {
                            errorStrBuilder.Append($" Vui lòng nhập giá trị cho {TypeA.ComponentName}.\n");
                        }
                        else if (item == nameof(TypeA.ComponentCode))
                        {
                            errorStrBuilder.Append($" Vui lòng nhập giá trị cho {TypeA.ComponentCode}.\n");
                        }
                        else if (item == nameof(TypeA.Assignee))
                        {
                            errorStrBuilder.Append($" Vui lòng nhập giá trị cho {TypeA.Assignee}.\n");
                        }
                        else
                            errorStrBuilder.Append($" Vui lòng nhập giá trị cho {TypeA.PositionCode}.\n");



                        validateCellDto.IsValid = false;
                    }
                }

                ////check required column
                //validateCellDto.IsValid = CheckRequiredColumn(dataFromCell.Plant, dataFromCell.WarehouseLocation, dataFromCell.PhysInv, dataFromCell.ComponentCode, dataFromCell.ComponentName, dataFromCell.PositionCode, dataFromCell.SpecialStock);
                //if (!validateCellDto.IsValid)
                //{
                //    validateCellDto.SpecificTitle = "File sai định dạng";
                //    validateCellDto.SpecificMessage = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo phiếu kiểm kê.";
                //    return validateCellDto;
                //}

                //var plantLocationComponentStoragebin = $"{dataFromCell.Plant}{dataFromCell.WarehouseLocation}{dataFromCell.ComponentCode}{dataFromCell.PositionCode}";
                //if (lastDocNumberAndAssignee.InvDocs.Any(x => x.PlantLocationComponentStorageBin == plantLocationComponentStoragebin))
                //{
                //    errorStrBuilder.Append($"Đã tồn tại mã linh kiện {dataFromCell.ComponentCode} có plant {dataFromCell.Plant}, WH Loc. {dataFromCell.WarehouseLocation} và vị trí {dataFromCell.PositionCode}.\n");
                //    //SetErrorMessageToCell(sourceSheet, row, validateCellDto, errorStrBuilder);
                //    //return validateCellDto;
                //    validateCellDto.IsValid = false;
                //}
                if (lastDocNumberAndAssignee.InvDocs.Any(x => x.ComponentCode == dataFromCell.ComponentCode && x.Description != dataFromCell.ComponentName) && !isDocShip)
                {
                    errorStrBuilder.Append($"Đã tồn tại mã linh kiện {dataFromCell.ComponentCode} với tên {lastDocNumberAndAssignee.InvDocs.FirstOrDefault(x => x.ComponentCode == dataFromCell.ComponentCode && x.Description != dataFromCell.ComponentName).Description}.\n");
                    //SetErrorMessageToCell(sourceSheet, row, validateCellDto, errorStrBuilder);
                    //return validateCellDto;
                    validateCellDto.IsValid = false;
                }
                if (lastDocNumberAndAssignee.InvDocs.Any(x => x.PositionCode == dataFromCell.PositionCode && x.PhysInv == dataFromCell.PhysInv) && !isDocShip)
                {
                    errorStrBuilder.Append($"Đã tồn tại vị trí {dataFromCell.PositionCode} có mã đăng ký kiểm kê {dataFromCell.PhysInv}.\n");
                    //SetErrorMessageToCell(sourceSheet, row, validateCellDto, errorStrBuilder);
                    //return validateCellDto;
                    validateCellDto.IsValid = false;
                }
                if (lastDocNumberAndAssignee.InvDocs.Any(x => x.SaleOrderNo == dataFromCell.SONo && x.PhysInv == dataFromCell.PhysInv) && !isDocShip)
                {
                    errorStrBuilder.Append($" Đã tồn tại S/O {dataFromCell.SONo} có mã đăng ký kiểm kê {dataFromCell.PhysInv}.\n");
                    //SetErrorMessageToCell(sourceSheet, row, validateCellDto, errorStrBuilder);
                    //return validateCellDto;
                    validateCellDto.IsValid = false;
                }

                if (lastDocNumberAndAssignee.InvDocs.Any(x => x.PhysInv == dataFromCell.PhysInv) && !isDocShip)
                {
                    errorStrBuilder.Append($" Đã tồn tại giá trị thông tin trường Phys.Inv.\n");
                    //SetErrorMessageToCell(sourceSheet, row, validateCellDto, errorStrBuilder);
                    //return validateCellDto;
                    validateCellDto.IsValid = false;
                }

                //check unique

                var componentUniqueName = $"{dataFromCell.ComponentCode}-{dataFromCell.ComponentName}";
                if (checkDto.ComponentUniqueName.Any(x => x.Item1 == dataFromCell.ComponentCode && x.Item2 != dataFromCell.ComponentName))
                {
                    errorStrBuilder.Append($"Đã tồn tại mã linh kiện {dataFromCell.ComponentCode} với tên {checkDto.ComponentUniqueName.FirstOrDefault(x => x.Item1 == dataFromCell.ComponentCode && x.Item2 != dataFromCell.ComponentName).Item2}.\n");
                    validateCellDto.IsValid = false;

                }
                else
                {
                    checkDto.ComponentUniqueName.Add((dataFromCell.ComponentCode, dataFromCell.ComponentName));

                }

                if (!checkDto.PhysInvDoc.Contains(dataFromCell.PhysInv))
                {
                    checkDto.PhysInvDoc.Add(dataFromCell.PhysInv);
                }
                else if (!isDocShip)
                {
                    errorStrBuilder.Append(" Trùng lặp giá trị thông tin trường Phys.Inv.\n");
                    validateCellDto.IsValid = false;

                }

                //if (!checkDto.SONo.Contains(dataFromCell.SONo) && !string.IsNullOrEmpty(dataFromCell.SONo))
                //{
                //    checkDto.SONo.Add(dataFromCell.SONo);
                //}
                //else if (checkDto.SONo.Contains(dataFromCell.SONo) && !string.IsNullOrEmpty(dataFromCell.SONo))
                //{
                //    errorStrBuilder.Append(" Trùng lặp giá trị thông tin trường S/O No.\n");

                //    validateCellDto.IsValid = false;

                //}

                //if (!checkDto.StorageBin.Contains(dataFromCell.StorageBin))
                //{
                //    checkDto.StorageBin.Add(dataFromCell.StorageBin);
                //}
                //else
                //{
                //    errorStrBuilder.Append(" Trùng lặp giá trị thông tin trường Storage bin.\n");
                //    validateCellDto.IsValid = false;

                //}

                //Bỏ Logic check S090:

                if (!string.IsNullOrEmpty(dataFromCell.WarehouseLocation) && !"S001, S002, S402, S090".Contains(dataFromCell.WarehouseLocation))
                {
                    errorStrBuilder.Append(" WH Loc. không đúng.\n");
                    validateCellDto.IsValid = false;
                }
                if (!string.IsNullOrEmpty(dataFromCell.Plant) && !"L401, L404, L402".Contains(dataFromCell.Plant))
                {
                    errorStrBuilder.Append(" Plant không đúng.\n");
                    validateCellDto.IsValid = false;
                }

                ////check duplicate
                //var positionPhysInvSONo = $"{dataFromCell.PositionCode}{dataFromCell.PhysInv}{dataFromCell.SONo}";
                //if (!checkDto.PositionPhysInvSONo.Contains(positionPhysInvSONo))
                //{
                //    checkDto.PositionPhysInvSONo.Add(positionPhysInvSONo);
                //}
                //else
                //{
                //    validateCellDto.IsValid = false;
                //    errorStrBuilder.Append("Vị trí - Mã đăng ký kiểm kê - SO No đã tồn tại trong file.\n");

                //}
                //var componentPlantLocationPosition = $"{dataFromCell.ComponentCode}{dataFromCell.Plant}{dataFromCell.WarehouseLocation}{dataFromCell.PositionCode}{dataFromCell.Assignee}";
                //if (!checkDto.ComponentPlantLocationPosition.Contains(componentPlantLocationPosition))
                //{
                //    checkDto.ComponentPlantLocationPosition.Add(componentPlantLocationPosition);
                //}
                //else
                //{
                //    validateCellDto.IsValid = false;
                //    errorStrBuilder.Append("Mã linh kiện - Plant - W.H.Loc - Vị trí đã gán cho nhân viên khác trong file.\n");

                //}

                //chek pair valid

                //Bỏ logic check S090:

                //if (dataFromCell.WarehouseLocation == "S090" && dataFromCell.SpecialStock != "E" || dataFromCell.WarehouseLocation != "S090" && dataFromCell.SpecialStock == "E")
                //{
                //    validateCellDto.IsValid = false;
                //    errorStrBuilder.Append(" Nếu Warehouse Location là S090 thì Special Stock phải có giá trị E.\n");
                //}
                //if (dataFromCell.WarehouseLocation == "S090" && dataFromCell.SpecialStock == "E" && string.IsNullOrEmpty(dataFromCell.SONo))
                //{
                //    validateCellDto.IsValid = false;
                //    errorStrBuilder.Append(" Không được để trống S/O nếu WH Loc. là S090 và Special Stock là E.\n");
                //}
                var validAssignee = lastDocNumberAndAssignee.Assignees.FirstOrDefault(x => x.UserName == dataFromCell.Assignee && x.RoleType.Value == InventoryAccountRoleType.Inventory);
                if (!string.IsNullOrEmpty(dataFromCell.Assignee) && validAssignee == null)
                {
                    errorStrBuilder.Append("Tài khoản phân phát không tồn tại. \n");
                    validateCellDto.IsValid = false;
                }

                //Nếu có quyền tạo phiếu theo phòng ban, chỉ import những phiếu có phòng ban (thuộc tài khoản phân phát) ứng những phòng ban đang được chọn trên quyền này(CREATE_DOCUMENT_BY_DEPARTMENT)
                var roleClaimTypes = rolesModel?.Where(x => x.UserName == dataFromCell.Assignee && x.ClaimType == Constants.Roles.CREATE_DOCUMENT_BY_DEPARTMENT);
                if (roleClaimTypes.Any())
                {
                    var validAssigneeBeLongToRoleDepartment = from a in lastDocNumberAndAssignee.Assignees
                                                              join r in roleClaimTypes on new { a.UserId, Department = a.DepartmentName } equals new { r.UserId, r.Department }
                                                              where a.UserName == dataFromCell.Assignee
                                                                && a.RoleType.Value == InventoryAccountRoleType.Inventory
                                                              select a;
                    if (!string.IsNullOrEmpty(dataFromCell.Assignee) && !validAssigneeBeLongToRoleDepartment.Any())
                    {
                        errorStrBuilder.Append("Tài khoản phân phát có phòng ban không thuộc trong danh sách phòng ban được phân quyền tạo phiếu. \n");
                        validateCellDto.IsValid = false;
                    }
                }


            }
            else if (docType == nameof(TypeC))
            {
                if (!lastDocNumberAndAssignee.IsTypeAExist)
                {
                    errorStrBuilder.Append("Chưa tạo phiếu A thì không được tạo phiếu B,E,C.");
                    sourceSheet.Cells[row, errorColumnIndex].Value = errorStrBuilder.ToString();
                    errorStrBuilder.Clear();
                    validateCellDto.IsValid = false;
                    validateCellDto.FailCount++;
                    return validateCellDto;
                }

                //Nếu có quyền tạo phiếu theo phòng ban, chỉ import những phiếu có phòng ban (thuộc tài khoản phân phát) ứng những phòng ban đang được chọn trên quyền này(CREATE_DOCUMENT_BY_DEPARTMENT)
                var roleClaimTypes = rolesModel?.Where(x => x.UserName == dataFromCell.Assignee && x.ClaimType == Constants.Roles.CREATE_DOCUMENT_BY_DEPARTMENT);
                if (roleClaimTypes.Any())
                {
                    var validAssigneeBeLongToRoleDepartment = from a in lastDocNumberAndAssignee.Assignees
                                                              join r in roleClaimTypes on new { a.UserId, Department = a.DepartmentName } equals new { r.UserId, r.Department }
                                                              where a.UserName == dataFromCell.Assignee
                                                                && a.RoleType.Value == InventoryAccountRoleType.Inventory
                                                              select a;
                    if (!string.IsNullOrEmpty(dataFromCell.Assignee) && !validAssigneeBeLongToRoleDepartment.Any())
                    {
                        errorStrBuilder.Append("Tài khoản phân phát có phòng ban không thuộc trong danh sách phòng ban được phân quyền tạo phiếu. \n");
                        validateCellDto.IsValid = false;
                    }
                }

            }
            else if (docType == nameof(TypeB) || docType == nameof(TypeE))
            {
                if (!lastDocNumberAndAssignee.IsTypeAExist)
                {
                    errorStrBuilder.Append("Chưa tạo phiếu A thì không được tạo phiếu B,E,C.");
                    sourceSheet.Cells[row, errorColumnIndex].Value = errorStrBuilder.ToString();
                    errorStrBuilder.Clear();
                    validateCellDto.IsValid = false;
                    validateCellDto.FailCount++;
                    return validateCellDto;
                }

                var dataProps = dataFromCell.GetType().GetProperties();
                if (docType == nameof(TypeB))
                {
                    foreach (var item in TypeB.RequiredColumns)
                    {
                        var prop = dataProps.FirstOrDefault(x => x.Name == item);
                        if (prop != null && string.IsNullOrEmpty((string)prop.GetValue(dataFromCell, null)))
                        {

                            if (item != nameof(TypeA.ComponentName) && item != nameof(TypeB.ComponentCode))
                            {
                                errorStrBuilder.Append($" Vui lòng nhập giá trị cho {item}.\n");
                            }
                            else if (item == nameof(TypeA.ComponentName))
                            {
                                errorStrBuilder.Append($" Vui lòng nhập giá trị cho {TypeB.ComponentName}.\n");
                            }
                            else if (item == nameof(TypeA.ComponentCode))
                            {
                                errorStrBuilder.Append($" Vui lòng nhập giá trị cho {TypeA.ComponentCode}.\n");
                            }





                            validateCellDto.IsValid = false;
                        }
                    }
                }
                else
                {
                    var requiredColumns = TypeE.RequiredColumns;
                    if (isDocShip)
                    {
                        requiredColumns = requiredColumns.Where(x => x != nameof(TypeE.StockType) && x != nameof(TypeE.ModelCode)).ToArray();

                    }
                    foreach (var item in requiredColumns)
                    {
                        var prop = dataProps.FirstOrDefault(x => x.Name == item);
                        if (prop != null && string.IsNullOrEmpty((string)prop.GetValue(dataFromCell, null)))
                        {

                            if (item != nameof(TypeA.ComponentName) && item != nameof(TypeE.ComponentCode) && item != nameof(TypeE.Assignee))
                            {
                                errorStrBuilder.Append($" Vui lòng nhập giá trị cho {item}.\n");
                            }
                            else if (item == nameof(TypeA.ComponentName))
                            {
                                errorStrBuilder.Append($" Vui lòng nhập giá trị cho {TypeE.ComponentName}.\n");
                            }
                            else if (item == nameof(TypeA.ComponentCode))
                            {
                                errorStrBuilder.Append($" Vui lòng nhập giá trị cho {TypeE.ComponentCode}.\n");
                            }
                            else
                            {
                                errorStrBuilder.Append($" Vui lòng nhập giá trị cho {TypeE.Assignee}.\n");
                            }





                            validateCellDto.IsValid = false;
                        }
                    }
                }

                ////check required column
                //validateCellDto.IsValid = CheckRequiredColumn(dataFromCell.Plant,
                //                                                dataFromCell.WarehouseLocation,
                //                                                dataFromCell.ComponentCode,
                //                                                dataFromCell.ComponentName,
                //                                                dataFromCell.ModelCode,
                //                                                dataFromCell.StockType,
                //                                                string.IsNullOrEmpty(dataFromCell.StorageBin) ? dataFromCell.AssemblyLoc : dataFromCell.StorageBin);

                //if (!validateCellDto.IsValid)
                //{
                //    errorStrBuilder.Append("Plant, WH Loc, Mã linh kiện, Tên linh kiện, Model code, Stock Type, Vị trí không được để trống.");
                //    sourceSheet.Cells[row, sourceSheet.GetColumnIndex(ImportExcelColumns.TypeA.Note) + 1].Value = errorStrBuilder.ToString();
                //    errorStrBuilder.Clear();
                //    validateCellDto.IsValid = false;
                //    validateCellDto.FailCount++;
                //    return validateCellDto;
                //}

                //chec pair valid

                if (!checkDto.PlanLocationForBE.Any(x => x.Equals($"{dataFromCell.ComponentCode}{dataFromCell.Plant}{dataFromCell.WarehouseLocation}")))
                {
                    validateCellDto.IsValid = false;
                    errorStrBuilder.Append($" Thông tin mã linh kiện {dataFromCell.ComponentCode} có plant {dataFromCell.Plant} và WH Loc. {dataFromCell.WarehouseLocation} không tồn tại trong phiếu A.\n");
                }

                //check null
                //if (string.IsNullOrEmpty(dataFromCell.StorageBin) && string.IsNullOrEmpty(dataFromCell.AssemblyLoc))
                //{
                //    validateCellDto.IsValid = false;
                //    errorStrBuilder.Append(" Không xác minh được loại phiếu nếu trống dữ liệu trường Storage bin hoặc Assembly Loc.\n");
                //}
                //check null
                //if (!string.IsNullOrEmpty(dataFromCell.StorageBin) && !string.IsNullOrEmpty(dataFromCell.AssemblyLoc))
                //{
                //    validateCellDto.IsValid = false;
                //    errorStrBuilder.Append(" Không xác minh được loại phiếu nếu có dữ liệu cả 2 trường Storage bin và Assembly Loc.\n");
                //}
                var validAssignee = lastDocNumberAndAssignee.Assignees.FirstOrDefault(x => x.UserName == dataFromCell.Assignee
                                                                                && x.RoleType.HasValue && x.RoleType.Value == InventoryAccountRoleType.Inventory);
                if (!string.IsNullOrEmpty(dataFromCell.Assignee) && validAssignee == null)
                {
                    errorStrBuilder.Append("Tài khoản phân phát không tồn tại. \n");
                    validateCellDto.IsValid = false;
                }
                //Bỏ Logic check S090:
                if (!"S001, S002, S402, S090".Contains(dataFromCell.WarehouseLocation))
                {
                    errorStrBuilder.Append(" WH Loc. không đúng.\n");
                    validateCellDto.IsValid = false;
                }
                if (!"L401, L404, L402".Contains(dataFromCell.Plant))
                {
                    errorStrBuilder.Append(" Plant không đúng.\n");
                    validateCellDto.IsValid = false;
                }
                if (isDocShip && !"S090".Contains(dataFromCell.WarehouseLocation))
                {
                    errorStrBuilder.Append(" WH Loc. không đúng.\n");
                    validateCellDto.IsValid = false;
                }
                //check exist
                var existedTypeB = lastDocNumberAndAssignee.InvDocs.Any(x => x.Plant == dataFromCell.Plant && x.WHLoc == dataFromCell.WarehouseLocation && x.ComponentCode == dataFromCell.ComponentCode && x.DocType == InventoryDocType.B && (!string.IsNullOrEmpty(x.AssemblyLocation) && x.AssemblyLocation == dataFromCell.AssemblyLoc));
                if (existedTypeB)
                {
                    errorStrBuilder.Append($" Đã tồn tại mã linh kiện {dataFromCell.ComponentCode} với plant {dataFromCell.Plant} và WH Loc {dataFromCell.WarehouseLocation} và vị trí {dataFromCell.AssemblyLoc} trên hệ thống.\n");
                    validateCellDto.IsValid = false;
                }

                var existedTypeE = lastDocNumberAndAssignee.InvDocs.Any(x => x.Plant == dataFromCell.Plant && x.WHLoc == dataFromCell.WarehouseLocation && x.ComponentCode == dataFromCell.ComponentCode && x.DocType == InventoryDocType.E && (!string.IsNullOrEmpty(x.StorageBin) && x.StorageBin == dataFromCell.StorageBin));
                if (existedTypeE)
                {
                    errorStrBuilder.Append($" Đã tồn tại mã linh kiện {dataFromCell.ComponentCode} với plant {dataFromCell.Plant} và WH Loc {dataFromCell.WarehouseLocation} và vị trí {dataFromCell.StorageBin} trên hệ thống.\n");
                    validateCellDto.IsValid = false;
                }

                //Nếu có quyền tạo phiếu theo phòng ban, chỉ import những phiếu có phòng ban (thuộc tài khoản phân phát) ứng những phòng ban đang được chọn trên quyền này(CREATE_DOCUMENT_BY_DEPARTMENT)
                var roleClaimTypes = rolesModel?.Where(x => x.UserName == dataFromCell.Assignee && x.ClaimType == Constants.Roles.CREATE_DOCUMENT_BY_DEPARTMENT);
                if (roleClaimTypes.Any())
                {
                    var validAssigneeBeLongToRoleDepartment = from a in lastDocNumberAndAssignee.Assignees
                                                              join r in roleClaimTypes on new { a.UserId, Department = a.DepartmentName } equals new { r.UserId, r.Department }
                                                              where a.UserName == dataFromCell.Assignee
                                                                && a.RoleType.Value == InventoryAccountRoleType.Inventory
                                                              select a;
                    if (!string.IsNullOrEmpty(dataFromCell.Assignee) && !validAssigneeBeLongToRoleDepartment.Any())
                    {
                        errorStrBuilder.Append("Tài khoản phân phát có phòng ban không thuộc trong danh sách phòng ban được phân quyền tạo phiếu. \n");
                        validateCellDto.IsValid = false;
                    }
                }

            }
            else//audit target
            {
                var materialCodeRegex = MaterialCodeForAuditTargetRegex();
                if (!lastDocNumberAndAssignee.IsTypeAExist)
                {
                    errorStrBuilder.Append("Chưa tạo phiếu A thì không được tạo phiếu danh sách cần giám sát.\n");
                    sourceSheet.Cells[row, sourceSheet.GetColumnIndex(TypeA.Assignee) + 1].Value = errorStrBuilder.ToString();
                    errorStrBuilder.Clear();
                    validateCellDto.IsValid = false;
                    validateCellDto.FailCount++;
                    return validateCellDto;
                }

                ModelStateDictionary auditModelValidation = new();
                var dataProps = dataFromCell.GetType().GetProperties();
                foreach (var item in AuditTargets.RequiredColumns)
                {
                    var prop = dataProps.FirstOrDefault(x => x.Name == item);
                    if (prop != null && string.IsNullOrEmpty((string)prop.GetValue(dataFromCell, null)))
                    {
                        validateCellDto.IsValid = false;

                        // nameof(Plant), nameof(WarehouseLocation), nameof(ComponentCode), nameof(PositionCode), nameof(Assignee) 
                        if (item != nameof(AuditTargets.ComponentCode) && item != nameof(AuditTargets.PositionCode))
                        {
                            errorStrBuilder.Append($" Vui lòng nhập giá trị cho {item}.\n");
                        }
                        else if (item == nameof(AuditTargets.ComponentCode))
                        {
                            errorStrBuilder.Append($" Vui lòng nhập giá trị cho {AuditTargets.ComponentCode}.\n");
                        }
                        else
                            errorStrBuilder.Append($" Vui lòng nhập giá trị cho {AuditTargets.PositionCode}.\n");

                        auditModelValidation.TryAddModelError(item, "Vui lòng nhập giá trị.");
                    }
                }

                if (!lastDocNumberAndAssignee.Assignees.Any(x => x.LocationName == sourceSheet.Name && x.UserName == dataFromCell.Assignee))
                {
                    errorStrBuilder.Append("Khu vực chưa được gán cho người giám sát.\n");
                    validateCellDto.IsValid = false;
                }

                //Validate plant
                var PlantTemplate = Constants.ValidationRules.Plant.Keys;
                if (!string.IsNullOrEmpty(dataFromCell.Plant) && PlantTemplate.Contains(dataFromCell.Plant) == false)
                {
                    errorStrBuilder.Append("Plant không đúng. \n");
                    validateCellDto.IsValid = false;

                    auditModelValidation.TryAddModelError(nameof(AuditTargets.Plant), "Plant không đúng.");
                }

                //Validate WH Loc.
                var WHLocTemplate = new List<string> { "S001", "S002", "S402", "S090" };
                if (!string.IsNullOrEmpty(dataFromCell.WarehouseLocation) && WHLocTemplate.Contains(dataFromCell.WarehouseLocation) == false)
                {
                    errorStrBuilder.Append("WH Loc. không đúng. \n");
                    validateCellDto.IsValid = false;

                    auditModelValidation.TryAddModelError(nameof(AuditTargets.WarehouseLocation), "WH Loc. không đúng.");
                }
                var validAssignee = lastDocNumberAndAssignee.Assignees.FirstOrDefault(x => x.UserName == dataFromCell.Assignee
                                                                                 && x.RoleType.HasValue && x.RoleType.Value == InventoryAccountRoleType.Audit);
                if (validAssignee == null)
                {
                    errorStrBuilder.Append("Tài khoản phân phát không tồn tại. \n");
                    validateCellDto.IsValid = false;
                }

                //Validate khu vực có được phân phát không
                //var validAssignedLocation = lastDocNumberAndAssignee.Assignees.FirstOrDefault(x => x.UserName == dataFromCell.Assignee
                //                                                                              && x.LocationName == dataFromCell.LocationName);
                //if (validAssignedLocation == null && validAssignee != null)
                //{
                //    errorStrBuilder.Append("Khu vực chưa được gán cho người giám sát. \n");
                //    validateCellDto.IsValid = false;
                //}


                //Validate Material Code
                if (dataFromCell.ComponentCode.Length > 0 && (dataFromCell.ComponentCode.Length < 9 || dataFromCell.ComponentCode.Length > 12 || !materialCodeRegex.IsMatch(dataFromCell.ComponentCode)))
                {
                    errorStrBuilder.Append("Mã linh kiện không đúng. \n");
                    validateCellDto.IsValid = false;

                    auditModelValidation.TryAddModelError(nameof(AuditTargets.ComponentCode), "Mã linh kiện không đúng.");
                }

                //Nếu trường Material code có 9 ký tự thì trường S / O No.phải trống không có giá trị
                if (dataFromCell.ComponentCode.Length == 9 && !string.IsNullOrEmpty(dataFromCell.SONo))
                {
                    errorStrBuilder.Append($"Mã linh kiện: {dataFromCell.ComponentCode} không thể có số S/O No. \n");
                    validateCellDto.IsValid = false;

                    auditModelValidation.TryAddModelError(nameof(AuditTargets.ComponentCode), $"Mã linh kiện: {dataFromCell.ComponentCode} không thể có số S/O No. \n");
                }

                var groupCondition = auditModelValidation.IsValid;

                if (groupCondition)
                {
                    //var activeInventory = _inventoryContext.Inventories.AsNoTracking().FirstOrDefault(x => x.InventoryStatus != InventoryStatus.Finish);
                    //var isExistInAuditTarget = _inventoryContext.AuditTargets.Any(x => x.Plant == dataFromCell.Plant &&
                    //                                                                    x.ComponentCode == dataFromCell.ComponentCode &&
                    //                                                                    x.WareHouseLocation == dataFromCell.WarehouseLocation && x.InventoryId == activeInventory.Id);
                    //if (isExistInAuditTarget)
                    //{
                    //    errorStrBuilder.Append($"Thông tin mã linh kiện: {dataFromCell.ComponentCode} có Plant: {dataFromCell.Plant} và  WH Loc: {dataFromCell.WarehouseLocation} đã tồn tại trên hệ thống. \n");
                    //    validateCellDto.IsValid = false;


                    //}

                    var existedDocA = lastDocNumberAndAssignee.InvDocs?.FirstOrDefault(x => x.Plant == dataFromCell.Plant
                                                                             && x.ComponentCode == dataFromCell.ComponentCode
                                                                             && x.WHLoc == dataFromCell.WarehouseLocation);
                    if (existedDocA == null)
                    {
                        errorStrBuilder.Append($"Thông tin mã linh kiện: {dataFromCell.ComponentCode} có Plant: {dataFromCell.Plant} và  WH Loc: {dataFromCell.WarehouseLocation} không tồn tại trong phiếu A. \n");
                        validateCellDto.IsValid = false;


                    }
                    ;
                }


                //Nếu trường Material code có 10, 11, 12  ký tự thì trường S / O No.phải có giá trị, không được để trống
                if (dataFromCell.ComponentCode.Length == 10 || dataFromCell.ComponentCode.Length == 11 || dataFromCell.ComponentCode.Length == 12)
                {
                    if (string.IsNullOrEmpty(dataFromCell.SONo))
                    {
                        errorStrBuilder.Append($"Mã linh kiện: {dataFromCell.ComponentCode} thiếu  số S/O No. \n");
                        validateCellDto.IsValid = false;

                        auditModelValidation.TryAddModelError(nameof(AuditTargets.ComponentCode), $"Mã linh kiện: {dataFromCell.ComponentCode} thiếu  số S/O No. \n");
                    }

                    //Nếu có giá trị S / O No.Thì check thông tin của Plant, Mã linh kiện, S/ O No
                    var groupConditionBySOno = auditModelValidation.IsValid;

                    if (groupConditionBySOno)
                    {

                        var isExistInAuditTarget = _inventoryContext.AuditTargets.Any(x => x.Plant == dataFromCell.Plant &&
                                                                                       x.ComponentCode == dataFromCell.ComponentCode &&
                                                                                       x.SaleOrderNo == dataFromCell.SONo);
                        if (isExistInAuditTarget)
                        {
                            errorStrBuilder.Append($"Thông tin mã linh kiện: {dataFromCell.ComponentCode} có Plant: {dataFromCell.Plant} và  Sale order: {dataFromCell.SONo} đã tồn tại trên hệ thống. \n");
                            validateCellDto.IsValid = false;


                        }

                        var existedDocA = lastDocNumberAndAssignee.InvDocs?.FirstOrDefault(x => x.Plant == dataFromCell.Plant
                                                                             && x.ComponentCode == dataFromCell.ComponentCode
                                                                             && x.SaleOrderNo == dataFromCell.SONo);

                        if (existedDocA == null)
                        {
                            errorStrBuilder.Append($"Thông tin mã linh kiện: {dataFromCell.ComponentCode} có Plant: {dataFromCell.Plant} và số Sale order: {dataFromCell.SONo} không tồn tại trong phiếu A. \n");
                            validateCellDto.IsValid = false;
                        }
                    }
                }


            }


            if (!validateCellDto.IsValid)
            {
                validateCellDto.FailCount++;

                if (docType == "AuditTarget")
                {
                    sourceSheet.Cells[1, sourceSheet.GetColumnIndex(TypeA.Assignee) + 1].Value = TypeA.ErrorContent;
                    sourceSheet.Cells[row, sourceSheet.GetColumnIndex(TypeA.Assignee) + 1].Value = errorStrBuilder.ToString();
                    sourceSheet.Cells[row, sourceSheet.GetColumnIndex(TypeA.Assignee) + 1].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                }
                else
                {
                    sourceSheet.Cells[row, errorColumnIndex].Value = errorStrBuilder.ToString();
                    sourceSheet.Cells[row, errorColumnIndex].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                }
                errorStrBuilder.Clear();
            }


            return validateCellDto;
        }

        private static void SetErrorMessageToCell(ExcelWorksheet sourceSheet, int row, ValidateCellDto validateCellDto, StringBuilder errorStrBuilder)
        {
            sourceSheet.Cells[row, sourceSheet.GetColumnIndex(TypeA.Note) + 1].Value = errorStrBuilder.ToString();
            sourceSheet.Cells[row, sourceSheet.GetColumnIndex(TypeA.Note) + 1].Style.Font.Color.SetColor(System.Drawing.Color.Red);
            errorStrBuilder.Clear();
            validateCellDto.IsValid = false;
            validateCellDto.FailCount++;
        }

        private async Task<InventoryDocAndUserDto> GetLastDocNumberAndAssignee(InventoryDocType docType, Guid inventoryId, bool onlyAssignee = false)
        {
            InventoryDocAndUserDto inventoryDocAndUerDto = new();
            inventoryDocAndUerDto.IsTypeAExist = await _inventoryContext.InventoryDocs.AsNoTracking().AnyAsync(x => x.InventoryId == inventoryId && x.DocType == InventoryDocType.A);
            if (onlyAssignee)
            {
                //get assigned user 
                inventoryDocAndUerDto.Assignees = await (from user in _inventoryContext.InventoryAccounts.AsNoTracking()
                                                         join accLoc in _inventoryContext.AccountLocations.AsNoTracking() on user.Id equals accLoc.AccountId
                                                         join location in _inventoryContext.InventoryLocations.AsNoTracking() on accLoc.LocationId equals location.Id
                                                         where (user.RoleType == InventoryAccountRoleType.Inventory || user.RoleType == InventoryAccountRoleType.Audit) && !location.IsDeleted
                                                         select new AssigneeDto
                                                         {
                                                             LocationName = location.Name,
                                                             DepartmentName = location.DepartmentName,
                                                             UserId = user.UserId,
                                                             UserName = user.UserName,
                                                             RoleType = user.RoleType.HasValue ? user.RoleType.Value : null
                                                         }).ToListAsync();

                inventoryDocAndUerDto.InvDocs = await _inventoryContext.InventoryDocs.AsNoTracking().Include(x => x.Inventory).Where(x => x.DocType == docType && x.InventoryId == inventoryId).Select(x => new InventoryDocDto
                {
                    InventoryName = x.Inventory.Name,
                    DocCode = x.DocCode,
                    PlantLocationComponentStorageBin = $"{x.Plant}{x.WareHouseLocation}{x.ComponentCode}{x.PositionCode}",
                    Plant = x.Plant,
                    Description = x.ComponentName,
                    ComponentCode = x.ComponentCode,
                    SaleOrderNo = x.SalesOrderNo,
                    WHLoc = x.WareHouseLocation,
                    PhysInv = x.PhysInv,
                    PositionCode = x.PositionCode,
                    DocType = x.DocType
                }).ToListAsync();

                return inventoryDocAndUerDto;
            }
            var docCode = string.Empty;
            var docCodeTypeB = string.Empty;
            var docCodeTypeE = string.Empty;
            if (docType == InventoryDocType.B || docType == InventoryDocType.E)
            {

                inventoryDocAndUerDto.InvDocs = await _inventoryContext.InventoryDocs.AsNoTracking().Include(x => x.Inventory)
                    .Where(x => x.DocType == docType && x.InventoryId == inventoryId)
                    .Select(x => new InventoryDocDto
                    {
                        InventoryName = x.Inventory.Name,
                        DocCode = x.DocCode,
                        PlantLocationComponentStorageBin = $"{x.Plant}{x.WareHouseLocation}{x.ComponentCode}{x.PositionCode}",
                        DocType = x.DocType,
                        Plant = x.Plant,
                        WHLoc = x.WareHouseLocation,
                        AssemblyLocation = x.AssemblyLocation,
                        ComponentCode = x.ComponentCode,
                        StorageBin = x.StorageBin,
                        Description = x.ComponentName,


                    }).OrderByDescending(x => x.DocCode).ToListAsync();

                inventoryDocAndUerDto.DocAComponentNames = await _inventoryContext.InventoryDocs.AsNoTracking().Include(x => x.Inventory)
                    .Where(x => x.DocType == InventoryDocType.A && x.InventoryId == inventoryId)
                    .Select(x => new DocAComponentNameDto
                    {
                        Plant = x.Plant,
                        WHLoc = x.WareHouseLocation,
                        ComponentCode = x.ComponentCode,
                        ComponentName = x.ComponentName
                    }).ToListAsync();

            }
            else
            {
                inventoryDocAndUerDto.InvDocs = await _inventoryContext.InventoryDocs.AsNoTracking().Include(x => x.Inventory).Where(x => x.DocType == docType && x.InventoryId == inventoryId).Select(x => new InventoryDocDto
                {
                    InventoryName = x.Inventory.Name,
                    DocCode = x.DocCode,
                    PlantLocationComponentStorageBin = $"{x.Plant}{x.WareHouseLocation}{x.ComponentCode}{x.PositionCode}",
                    Plant = x.Plant,
                    Description = x.ComponentName,
                    ComponentCode = x.ComponentCode,
                    SaleOrderNo = x.SalesOrderNo,
                    WHLoc = x.WareHouseLocation,
                    PhysInv = x.PhysInv,
                    PositionCode = x.PositionCode,
                    DocType = x.DocType
                }).OrderByDescending(x => x.DocCode).ToListAsync();
            }

            var numberPattern = DocCodeRegex();
            var inventoryEntity = await _inventoryContext.Inventories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == inventoryId);
            //20231214
            var yearAndMonth = inventoryEntity.InventoryDate.ToString("yyMM");
            if (inventoryDocAndUerDto.InvDocs?.Count > 0)
            {
                docCode = inventoryDocAndUerDto.InvDocs?.First().DocCode;
                docCodeTypeB = inventoryDocAndUerDto.InvDocs?.FirstOrDefault(x => x.DocType == InventoryDocType.B)?.DocCode;
                docCodeTypeE = inventoryDocAndUerDto.InvDocs?.FirstOrDefault(x => x.DocType == InventoryDocType.E)?.DocCode;

            }
            else
            {


                if (docType == InventoryDocType.B || docType == InventoryDocType.E)
                {
                    docCode = $"BE{yearAndMonth}00001";
                }
                else
                {
                    docCode = $"{docType}{yearAndMonth}00001";
                }
                inventoryDocAndUerDto.LastDocNumber = 1;


            }
            var match = numberPattern.Match(docCode);

            var invPart = $"{match?.Groups[1].Value}";

            var docTypeRegex = DocTypeRegex();
            if (!string.IsNullOrEmpty(invPart) && docType != InventoryDocType.B && docType != InventoryDocType.E)
            {


                if (docTypeRegex.IsMatch(invPart))
                {
                    invPart = docTypeRegex.Replace(invPart, string.Empty);
                    invPart = $"{docType}{invPart}";
                }

            }
            else if (docType == InventoryDocType.B || docType == InventoryDocType.E)
            {
                if (docTypeRegex.IsMatch(invPart))
                {
                    invPart = docTypeRegex.Replace(invPart, string.Empty);
                    invPart = $"BE{invPart}";
                }
            }
            else
            {
                if (docType == InventoryDocType.B || docType == InventoryDocType.E)
                {
                    invPart = $"BE{yearAndMonth}";
                }
                else
                {
                    invPart = $"{docType}{yearAndMonth}";
                }
            }
            inventoryDocAndUerDto.InventoryPart = invPart;

            //get lastCodeNumber


            if (docType == InventoryDocType.B || docType == InventoryDocType.E)
            {
                if (!inventoryDocAndUerDto.InvDocs.Any(x => x.DocType == InventoryDocType.B))
                {
                    inventoryDocAndUerDto.LastDocNumberTypeB = 1;
                }
                else
                {
                    var lastCodeNumberStr = numberPattern.Match(docCodeTypeB).Groups[2].Value;
                    if (!string.IsNullOrEmpty(lastCodeNumberStr))
                    {
                        if (int.Parse(lastCodeNumberStr) >= 1)
                        {
                            inventoryDocAndUerDto.LastDocNumberTypeB = int.Parse(lastCodeNumberStr) + 1;
                        }

                    }
                }
                if (!inventoryDocAndUerDto.InvDocs.Any(x => x.DocType == InventoryDocType.E))
                {
                    inventoryDocAndUerDto.LastDocNumberTypeE = 1;
                }
                else
                {
                    var lastCodeNumberStr = numberPattern.Match(docCodeTypeE).Groups[2].Value;
                    if (!string.IsNullOrEmpty(lastCodeNumberStr))
                    {
                        if (int.Parse(lastCodeNumberStr) >= 1)
                        {
                            inventoryDocAndUerDto.LastDocNumberTypeE = int.Parse(lastCodeNumberStr) + 1;
                        }

                    }
                }
            }
            else
            {
                if (!inventoryDocAndUerDto.InvDocs.Any(x => x.DocType == docType))
                {
                    inventoryDocAndUerDto.LastDocNumber = 1;
                }
                else
                {
                    var code = inventoryDocAndUerDto.InvDocs?.First(x => x.DocType == docType).DocCode;

                    var lastCodeNumberStr = numberPattern.Match(code).Groups[2].Value;
                    if (!string.IsNullOrEmpty(lastCodeNumberStr))
                    {
                        if (int.Parse(lastCodeNumberStr) >= 1)
                        {
                            inventoryDocAndUerDto.LastDocNumber = int.Parse(lastCodeNumberStr) + 1;
                        }

                    }
                }
            }







            //get assigned user 
            inventoryDocAndUerDto.Assignees = await (from user in _inventoryContext.InventoryAccounts.AsNoTracking()
                                                     join accLoc in _inventoryContext.AccountLocations.AsNoTracking() on user.Id equals accLoc.AccountId
                                                     join location in _inventoryContext.InventoryLocations.AsNoTracking() on accLoc.LocationId equals location.Id
                                                     where (user.RoleType == InventoryAccountRoleType.Inventory || user.RoleType == InventoryAccountRoleType.Audit) && !location.IsDeleted
                                                     select new AssigneeDto
                                                     {
                                                         LocationName = location.Name,
                                                         DepartmentName = location.DepartmentName,
                                                         UserId = user.UserId,
                                                         UserName = user.UserName,
                                                         RoleType = user.RoleType.HasValue ? user.RoleType.Value : null
                                                     }).ToListAsync();
            return inventoryDocAndUerDto;
        }

        private async Task<InventoryDocumentImportResultDto> ImportInventoryDocTypeE(IFormFile file, Guid inventoryId)
        {
            using (var memStream = new MemoryStream())
            {
                await file.CopyToAsync(memStream);
                var checkDto = new CheckInventoryDocumentDto();
                var resultDto = new InventoryDocumentImportResultDto();
                using (var sourcePackage = new ExcelPackage(memStream))
                {
                    var sourceSheet = sourcePackage.Workbook.Worksheets.FirstOrDefault();
                    var totalRowsCount = sourceSheet.Dimension.End.Row > 1 ? sourceSheet.Dimension.End.Row - 1 : sourceSheet.Dimension.End.Row;
                    var rows = Enumerable.Range(2, totalRowsCount).ToList();
                    var validRows = new List<int>();
                    if (sourceSheet != null)
                    {
                        //Kiểm tra sai cột, thiếu cột so với file mẫu
                        var PlantIndex = sourceSheet.GetColumnIndex(TypeE.Plant);
                        var WarehouseLocationIndex = sourceSheet.GetColumnIndex(TypeE.WarehouseLocation);
                        var ComponentCodeIndex = sourceSheet.GetColumnIndex(TypeE.ComponentCode);
                        var StockTypeIndex = sourceSheet.GetColumnIndex(TypeE.StockType);
                        var ModelCodeIndex = sourceSheet.GetColumnIndex(TypeE.ModelCode);
                        var StorageBinIndex = sourceSheet.GetColumnIndex(TypeE.StorageBin);
                        var AssigneeIndex = sourceSheet.GetColumnIndex(TypeB.Assignee);

                        var requiredHeader = new[] {
                            PlantIndex,
                            WarehouseLocationIndex,
                            ComponentCodeIndex,
                            StockTypeIndex,
                            ModelCodeIndex,
                            StorageBinIndex,
                            AssigneeIndex,
                        };

                        if (requiredHeader.Any(x => x == -1))
                        {
                            return new InventoryDocumentImportResultDto
                            {
                                Code = (int)HttpStatusCodes.InvalidFileExcel,
                                Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo phiếu kiểm kê."
                            };
                        }

                        var failCount = 0;
                        //get last document number by type
                        var lastDocNumberAndAssignee = await GetLastDocNumberAndAssignee(InventoryDocType.E, inventoryId);

                        //Get all Roles:
                        var request = new RestRequest(Constants.Endpoint.Internal.Get_All_Roles_Department);
                        request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
                        request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

                        var roles = await _identityRestClient.ExecuteGetAsync(request);

                        var rolesModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<GetAllRoleWithUserNameModel>>>(roles.Content ?? string.Empty);

                        var createDocumentRoles = rolesModel?.Data;

                        //init list inventory document entity
                        var inventoryDocuments = new List<InventoryDoc>();

                        var inventoryDocCellDtoList = new List<InventoryDocCellDto>();

                        checkDto.PlanLocationForBE = _inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryId).Select(x => $"{x.ComponentCode}{x.Plant}{x.WareHouseLocation}").Distinct().ToHashSet();
                        for (var row = sourceSheet.Dimension.Start.Row + 1; row <= sourceSheet.Dimension.End.Row; row++)

                        {
                            try
                            {
                                //get data cell value
                                var dataFromCell = GetDataFromCell(sourceSheet, row, nameof(TypeE));

                                if (dataFromCell != null)
                                {
                                    inventoryDocCellDtoList.Add(dataFromCell);
                                }


                                //check for all blank rows
                                if (CheckRequiredColumn(true, dataFromCell.Plant, dataFromCell.WarehouseLocation, dataFromCell.ComponentCode, dataFromCell.StockType, dataFromCell.ModelCode, dataFromCell.StorageBin, dataFromCell.Assignee))
                                {
                                    if (row < sourceSheet.Dimension.Rows)
                                    {
                                        sourceSheet.Row(row).Hidden = true;

                                    }
                                    continue;

                                }

                                //validate data
                                var validateData = ValidateCellData(inventoryId, createDocumentRoles, lastDocNumberAndAssignee, dataFromCell, sourceSheet, row, checkDto, inventoryDocCellDtoList, nameof(TypeE), sourceSheet.Dimension.End.Column);
                                if (validateData.IsValid)
                                {
                                    validRows.Add(row);
                                }
                                if (!validateData.IsValid)
                                {
                                    failCount += validateData.FailCount;
                                    continue;
                                }
                                SetDataToEntites(inventoryId, lastDocNumberAndAssignee, inventoryDocuments, dataFromCell, nameof(TypeB));

                            }
                            catch (Exception exception)
                            {
                                var exMess = $"Exception - {exception.Message} at row {row}";
                                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                                _logger.LogError($"Request error at {_httpContext.Request.Path} : {exMess}; {innerExMess}");
                                continue;
                            }
                        }

                        AddDataToDb(inventoryDocuments);

                        //Thêm tiêu đồ cột nội dung lỗi trong file excel
                        if (failCount > 0)
                        {
                            sourceSheet.Cells[1, sourceSheet.Dimension.Columns].Value = TypeB.ErrorContent;
                        }


                        resultDto.FailCount = failCount;
                        resultDto.SuccessCount = inventoryDocuments.Count;
                    }
                    //var errorWorksheet = sourcePackage.Workbook.Worksheets.Copy(sourceSheet.Name, sourceSheet.Name + "_Error");
                    //sourceSheet.Workbook.Worksheets.Delete(sourceSheet.Name);
                    foreach (var row in validRows.OrderByDescending(r => r))
                    {
                        //errorWorksheet.Cells[row, 1, row, errorWorksheet.Dimension.End.Column].Clear();
                        sourceSheet.DeleteRow(row);
                    }
                    resultDto.Result = sourcePackage.GetAsByteArray();
                }
                return resultDto;
            }
        }

        private async Task<InventoryDocumentImportResultDto> ImportInventoryDocTypeB(IFormFile file, Guid inventoryId)
        {
            using (var memStream = new MemoryStream())
            {
                await file.CopyToAsync(memStream);
                var checkDto = new CheckInventoryDocumentDto();
                var resultDto = new InventoryDocumentImportResultDto();
                using (var sourcePackage = new ExcelPackage(memStream))
                {
                    //get last document number by type
                    var lastDocNumberAndAssignee = await GetLastDocNumberAndAssignee(InventoryDocType.B, inventoryId);

                    //Get all Roles:
                    var request = new RestRequest(Constants.Endpoint.Internal.Get_All_Roles_Department);
                    request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
                    request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

                    var roles = await _identityRestClient.ExecuteGetAsync(request);

                    var rolesModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<GetAllRoleWithUserNameModel>>>(roles.Content ?? string.Empty);

                    var createDocumentRoles = rolesModel?.Data;

                    checkDto.PlanLocationForBE = _inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryId).Select(x => $"{x.ComponentCode}{x.Plant}{x.WareHouseLocation}").Distinct().ToHashSet();

                    var sourceSheets = sourcePackage.Workbook.Worksheets;
                    var sheetsToDelete = new List<string>();
                    var sheetsToCopy = new List<string>();


                    foreach (var sourceSheet in sourceSheets)
                    {
                        if (sourceSheet == null)
                        {
                            continue;
                        }

                        var totalRowsCount = sourceSheet.Dimension.End.Row > 1 ? sourceSheet.Dimension.End.Row - 1 : sourceSheet.Dimension.End.Row;
                        var rows = Enumerable.Range(2, totalRowsCount).ToList();
                        var validRows = new List<int>();
                        if (sourceSheet != null)
                        {
                            //Kiểm tra sai cột, thiếu cột so với file mẫu
                            var PlantIndex = sourceSheet.GetColumnIndex(TypeB.Plant);
                            var WarehouseLocationIndex = sourceSheet.GetColumnIndex(TypeB.WarehouseLocation);
                            var ComponentCodeIndex = sourceSheet.GetColumnIndex(TypeB.ComponentCode);
                            var StockTypeIndex = sourceSheet.GetColumnIndex(TypeB.StockType);
                            var ModelCodeIndex = sourceSheet.GetColumnIndex(TypeB.ModelCode);
                            var AssemblyLocIndex = sourceSheet.GetColumnIndex(TypeB.AssemblyLoc);
                            var MachineModelIndex = sourceSheet.GetColumnIndex(TypeB.MachineModel);
                            var MachineTypeIndex = sourceSheet.GetColumnIndex(TypeB.MachineType);
                            var LineNameIndex = sourceSheet.GetColumnIndex(TypeB.LineName);
                            var AssigneeIndex = sourceSheet.GetColumnIndex(TypeB.Assignee);

                            var requiredHeader = new[] {
                                PlantIndex,
                                WarehouseLocationIndex,
                                ComponentCodeIndex,
                                StockTypeIndex,
                                ModelCodeIndex,
                                AssemblyLocIndex,
                                MachineModelIndex,
                                MachineTypeIndex,
                                LineNameIndex,
                                AssigneeIndex,
                            };

                            if (requiredHeader.Any(x => x == -1))
                            {
                                return new InventoryDocumentImportResultDto
                                {
                                    Code = (int)HttpStatusCodes.InvalidFileExcel,
                                    Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo phiếu kiểm kê."
                                };
                            }

                            //init list inventory document entity
                            var inventoryDocuments = new List<InventoryDoc>();

                            var inventoryDocCellDtoList = new List<InventoryDocCellDto>();

                            var failCount = 0;

                            for (var row = sourceSheet.Dimension.Start.Row + 1; row <= sourceSheet.Dimension.End.Row; row++)

                            {
                                try
                                {
                                    //get data cell value
                                    var dataFromCell = GetDataFromCell(sourceSheet, row, nameof(TypeB));

                                    //check for all blank rows
                                    if (CheckRequiredColumn(true, dataFromCell.Plant, dataFromCell.WarehouseLocation, dataFromCell.ComponentCode, dataFromCell.StockType, dataFromCell.ModelCode, dataFromCell.AssemblyLoc, dataFromCell.MachineModel, dataFromCell.MachineType, dataFromCell.LineName, dataFromCell.Assignee))
                                    {
                                        if (row < sourceSheet.Dimension.Rows)
                                        {
                                            sourceSheet.Row(row).Hidden = true;

                                        }
                                        continue;

                                    }
                                    if (dataFromCell != null)
                                    {
                                        inventoryDocCellDtoList.Add(dataFromCell);
                                    }
                                    //validate data
                                    var validateData = ValidateCellData(inventoryId, createDocumentRoles, lastDocNumberAndAssignee, dataFromCell, sourceSheet, row, checkDto, inventoryDocCellDtoList, nameof(TypeB), sourceSheet.Dimension.End.Column);
                                    if (validateData.IsValid)
                                    {
                                        validRows.Add(row);
                                    }
                                    if (!validateData.IsValid)
                                    {
                                        failCount += validateData.FailCount;
                                        continue;
                                    }
                                    SetDataToEntites(inventoryId, lastDocNumberAndAssignee, inventoryDocuments, dataFromCell, nameof(TypeB));

                                }
                                catch (Exception exception)
                                {
                                    var exMess = $"Exception - {exception.Message} at row {row}";
                                    var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                                    _logger.LogError($"Request error at {_httpContext.Request.Path} : {exMess}; {innerExMess}");
                                    continue;
                                }
                            }
                            if (inventoryDocuments.Any())
                            {
                                AddDataToDb(inventoryDocuments);
                            }

                            //Thêm tiêu đồ cột nội dung lỗi trong file excel
                            if (failCount > 0)
                            {
                                sourceSheet.Cells[1, sourceSheet.Dimension.Columns].Value = TypeB.ErrorContent;
                            }


                            resultDto.FailCount += failCount;
                            resultDto.SuccessCount += inventoryDocuments.Count;
                        }


                        //var errorWorksheet = sourcePackage.Workbook.Worksheets.Copy(sourceSheet.Name, sourceSheet.Name + "_Error");
                        //sourceSheet.Workbook.Worksheets.Delete(sourceSheet.Name);
                        foreach (var row in validRows.OrderByDescending(r => r))
                        {
                            //sourceSheet.Cells[row, 1, row, sourceSheet.Dimension.End.Column].Clear();
                            sourceSheet.DeleteRow(row);
                        }

                        // Kiểm tra xem sheet có còn dữ liệu hay không
                        bool hasData = false;
                        for (var row = sourceSheet.Dimension.Start.Row + 1; row <= sourceSheet.Dimension.End.Row; row++)
                        {
                            for (int col = 1; col <= sourceSheet.Dimension.End.Column; col++)
                            {
                                if (sourceSheet.Cells[row, col].Value != null)
                                {
                                    hasData = true;
                                    break;
                                }
                            }
                            if (hasData)
                            {
                                break;
                            }
                        }

                        // Nếu không còn dữ liệu thì xóa sheet
                        if (!hasData && sourceSheet.Workbook.Worksheets.Count > 1)
                        {
                            sourceSheet.Workbook.Worksheets.Delete(sourceSheet.Name);
                        }
                        else
                        {
                            sourceSheet.TabColor = Color.Red;
                        }
                    }
                    resultDto.Result = sourcePackage.GetAsByteArray();
                }
                return resultDto;
            }
        }
        private string GetCellValue(ExcelWorksheet sourceSheet, int row, string header)
        {
            var column = sourceSheet.GetColumnIndex(header);
            if (column == -1)
                return string.Empty;
            var value = sourceSheet.GetValue(row, column);
            return value != null ? value.ToString() : string.Empty;
        }
        private bool CheckRequiredColumn(params string[] columns)
        {
            if (columns.Any(string.IsNullOrEmpty))
            {
                return false;
            }
            return true;
        }
        private bool CheckRequiredColumn(bool isBlankRowsCheck = false, params string[] columns)
        {
            if (isBlankRowsCheck)
            {
                return columns.All(string.IsNullOrEmpty);
            }
            if (columns.Any(string.IsNullOrEmpty))
            {
                return false;
            }
            return true;
        }

        public async Task<ResponseModel> ScanDocsAE(Guid inventoryId, Guid accountId, string componentCode, string positionCode, string docCode, InventoryActionType actionType, bool isErrorInvestigation)
        {
            //20240910: Update Logic: Tài khoản Vai trò xúc tiến được vào xem, cập nhật kiểm kê:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();

            //Kiểm tra mã linh kiện có tồn tại hay không
            var anyComponentCode = await _inventoryContext.InventoryDocs?.AsNoTracking()
                                                          ?.AnyAsync(x => x.InventoryId.Value == inventoryId
                                                                       && x.ComponentCode == componentCode
                                                                       && ExcludeDocStatus.Contains(x.Status) == false
                                                                       && (x.DocType == InventoryDocType.A || x.DocType == InventoryDocType.E));
            if (anyComponentCode == false)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.InventoryNotFoundComponentCode,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.InventoryNotFoundComponentCode)
                };
            }

            IQueryable<InventoryDoc> inventoryDocs;

            //Mã linh kiện này không nằm trong danh sách thực hiện kiểm kê của bạn
            var assignedInventoriesQuery = _inventoryContext.InventoryDocs?.AsNoTracking()
                                                          ?.Where(x => x.InventoryId.Value == inventoryId
                                                                    //&& x.AssignedAccountId.Value == accountId
                                                                    && x.ComponentCode == componentCode
                                                                    && ExcludeDocStatus.Contains(x.Status) == false
                                                                    && (x.DocType == InventoryDocType.A || x.DocType == InventoryDocType.E));

            //var assignedInventories = _inventoryContext.InventoryDocs?.AsNoTracking()
            //                                              ?.Where(x => x.InventoryId.Value == inventoryId
            //                                                        && x.AssignedAccountId.Value == accountId
            //                                                        && x.ComponentCode == componentCode
            //                                                        && ExcludeDocStatus.Contains(x.Status) == false
            //                                                        && x.DocType != InventoryDocType.C);
            if (!isErrorInvestigation)
            {
                if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
                {
                    assignedInventoriesQuery = assignedInventoriesQuery.Where(x => x.AssignedAccountId.Value == accountId);
                }
            }

            var assignedInventories = assignedInventoriesQuery;

            if (assignedInventories == null || !assignedInventories.Any())
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.ComponentNotAssigned,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentNotAssigned)
                };
            }

            //Lấy chi tiết cho phiếu A hoặc E:
            var getDetailDocs = assignedInventories;

            if (!string.IsNullOrEmpty(positionCode))
            {
                getDetailDocs = assignedInventories.Where(x => x.PositionCode == positionCode);
            }
            if (!string.IsNullOrEmpty(docCode))
            {
                getDetailDocs = assignedInventories.Where(x => x.DocCode == docCode);
            }

            inventoryDocs = getDetailDocs;

            //Nếu là xác nhận kiểm kê thì check có phiếu hợp lệ đã đúng trạng thái là đã thực hiện kiểm kê chưa.
            if (actionType == InventoryActionType.ConfirmInventory)
            {
                var totalDocsCount = inventoryDocs.Count();
                if (totalDocsCount > 1)
                {
                    var inventoriedDocuments = inventoryDocs.Where(x => (int)x.Status == (int)InventoryDocStatus.WaitingConfirm || (int)x.Status == (int)InventoryDocStatus.MustEdit
                                                                                || (int)x.Status == (int)InventoryDocStatus.Confirmed);
                    inventoryDocs = inventoriedDocuments;
                    if (inventoryDocs == null || !inventoryDocs.Any())
                    {
                        return new ResponseModel
                        {
                            Code = (int)HttpStatusCodes.ComponentCodeIsNotInventoryYetOrAudited,
                            Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentCodeIsNotInventoryYetOrAudited)
                        };
                    }
                }
                else
                {
                    var singleDoc = inventoryDocs.FirstOrDefault();
                    if ((int)singleDoc.Status == (int)InventoryDocStatus.WaitingConfirm || (int)singleDoc.Status == (int)InventoryDocStatus.MustEdit
                                                                        || (int)singleDoc.Status == (int)InventoryDocStatus.Confirmed)
                    {
                        inventoryDocs = new[] { singleDoc }.AsQueryable();
                    }
                    else if ((int)singleDoc.Status == (int)InventoryDocStatus.AuditFailed || (int)singleDoc.Status == (int)InventoryDocStatus.AuditPassed)
                    {
                        return new ResponseModel
                        {
                            Code = (int)HttpStatusCodes.ComponentCodeIsAudited,
                            Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentCodeIsAudited)
                        };
                    }
                    else if ((int)singleDoc.Status == (int)InventoryDocStatus.NotInventoryYet)
                    {
                        return new ResponseModel
                        {
                            Code = (int)HttpStatusCodes.ComponentNotInventoryYet,
                            Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentNotInventoryYet)
                        };
                    }
                }
            }


            var convertedInventoryDocs = inventoryDocs
                                                .Select(doc => new InventoryDocViewModel
                                                {
                                                    Id = doc.Id,
                                                    InventoryId = doc.InventoryId.Value,
                                                    AssignedAccountId = doc.AssignedAccountId ?? Guid.Empty,
                                                    ComponentCode = doc.ComponentCode ?? string.Empty,
                                                    ComponentName = doc.ComponentName ?? string.Empty,
                                                    DocCode = doc.DocCode ?? string.Empty,
                                                    DocType = (int)doc.DocType,
                                                    Note = doc.Note,
                                                    PositionCode = doc.PositionCode ?? string.Empty,
                                                    Status = (int)doc.Status,
                                                    SaleOrderNo = doc.SalesOrderNo ?? string.Empty,
                                                    InventoryBy = doc.InventoryBy ?? string.Empty,
                                                    AuditedBy = doc.AuditBy ?? string.Empty,
                                                    ConfirmedBy = doc.ConfirmBy ?? string.Empty
                                                }).ToList();

            var docs = from doc in convertedInventoryDocs
                       join dt in _inventoryContext.DocOutputs.AsNoTracking() on doc.Id equals dt.InventoryDocId.Value into dtGroup
                       join h in _inventoryContext.DocHistories.AsNoTracking().Where(x => ExcludeDocStatus.Contains(x.Status) == false) on doc.Id equals h.InventoryDocId.Value into hGroup
                       select new
                       {
                           InventoryDoc = new InventoryDocViewModel
                           {
                               Id = doc.Id,
                               InventoryId = doc.InventoryId,
                               AssignedAccountId = doc.AssignedAccountId,
                               ComponentCode = doc.ComponentCode,
                               ComponentName = doc.ComponentName,
                               DocCode = doc.DocCode,
                               DocType = doc.DocType,
                               Note = doc.Note,
                               PositionCode = doc.PositionCode,
                               Status = doc.Status,
                               SaleOrderNo = doc.SaleOrderNo,
                               InventoryBy = doc?.InventoryBy ?? string.Empty,
                               AuditedBy = doc?.AuditedBy ?? string.Empty,
                               ConfirmedBy = doc?.ConfirmedBy ?? string.Empty
                           },
                           Components = dtGroup != null && dtGroup.Any() ? dtGroup.OrderBy(x => x.CreatedAt).Select(c => new DocComponentABE
                           {
                               Id = c.Id,
                               InventoryId = c.InventoryId,
                               InventoryDocId = c.InventoryDocId.Value,
                               QuantityOfBom = c.QuantityOfBom,
                               QuantityPerBom = c.QuantityPerBom
                           }) : new List<DocComponentABE>(),
                           Histories = hGroup != null && hGroup.Any() ? hGroup.OrderByDescending(x => x.CreatedAt).Select(h => new DocHistoriesModel
                           {
                               Id = h.Id,
                               InventoryId = h.InventoryId,
                               InventoryDocId = h.InventoryDocId.Value,
                               Action = (int)h.Action,
                               Comment = h.Comment,
                               EvicenceImg = string.IsNullOrEmpty(h.EvicenceImg) ? string.Empty : h.EvicenceImg,
                               EvicenceImgTitle = string.IsNullOrEmpty(h.EvicenceImg) ? string.Empty : Path.GetFileName(h.EvicenceImg),
                               ChangeLogModel = new ChangeLogModel
                               {
                                   IsChangeCDetail = h.IsChangeCDetail,
                                   NewQuantity = h.NewQuantity,
                                   OldQuantity = h.OldQuantity,
                                   NewStatus = (int)h.NewStatus,
                                   OldStatus = (int)h.OldStatus
                               },
                               CreatedAt = h.CreatedAt,
                               CreatedBy = h.CreatedBy,
                               Status = (int)h.Status
                           }) : new List<DocHistoriesModel>()
                       };

            if (docs == null || docs?.Any() == false)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = docs
            };
        }

        public async Task<ResponseModel<DocCListViewModel>> GetDocsC(ListDocCFilterModel listDocCFilterModel)
        {
            if (listDocCFilterModel == null)
            {
                return new ResponseModel<DocCListViewModel>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Điều kiện lọc không hợp lệ."
                };
            }

            var docs = from d in _inventoryContext.InventoryDocs.AsNoTracking()
                       let condition = ExcludeDocStatus.Contains(d.Status) == false
                               && d.DocType == InventoryDocType.C
                               && d.InventoryId == listDocCFilterModel.InventoryId
                               && d.AssignedAccountId == listDocCFilterModel.AccountId
                               && d.MachineModel == listDocCFilterModel.MachineModel
                               && d.MachineType == listDocCFilterModel.MachineType
                               && d.LineName == listDocCFilterModel.LineName

                       let docStatusOrder = (int)d.Status == (int)InventoryDocStatus.AuditPassed ? (int)InventoryDocStatus.AuditFailed :
                                          (int)d.Status == (int)InventoryDocStatus.AuditFailed ? (int)InventoryDocStatus.AuditPassed :
                                          (int)d.Status

                       where condition
                       select new DocCInfoModel
                       {
                           Id = d.Id,
                           InventoryId = d.InventoryId.Value,
                           AccountId = d.AssignedAccountId.Value,
                           Status = (int)d.Status,
                           DocType = (int)d.DocType,
                           DocCode = d.DocCode,
                           ModelCode = d.ModelCode,
                           MachineModel = d.MachineModel,
                           MachineType = d.MachineType,
                           LineName = d.LineName,
                           LineType = DocCMachineModel.GetDisplayLineType(d.LineType),
                           StageNumber = d.StageNumber,
                           StageName = d.StageName,
                           Note = d.Note ?? string.Empty,

                           InventoryBy = d.InventoryBy ?? string.Empty,
                           AuditedBy = d.AuditBy ?? string.Empty,
                           ConfirmedBy = d.ConfirmBy ?? string.Empty,
                           DocStatusOrder = docStatusOrder
                       };

            if (docs == null || docs?.Any() == false)
            {
                return new ResponseModel<DocCListViewModel>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            //Lấy ra trạng thái hoàn thành tổng thể: 5/10 => 5: số lượng có status: chờ xác nhận, đã xác nhận, giám sát đạt, giám sát không đạt
            //                                            => 10: các trạng thái còn lại trừ chưa tiếp nhận và không kiểm kê

            DocCListViewModel resultModel = new();
            var finishedDocsCount = 0;
            var totalDocsCount = 0;
            //Tiến trình phiếu: VD: Đã kiểm kê: 5/10
            if (listDocCFilterModel.ActionType == InventoryActionType.Inventory)
            {
                finishedDocsCount = docs.Where(x => x.Status == (int)InventoryDocStatus.WaitingConfirm
                                                    || x.Status == (int)InventoryDocStatus.Confirmed
                                                    || x.Status == (int)InventoryDocStatus.AuditPassed
                                                    || x.Status == (int)InventoryDocStatus.AuditFailed
                                                    )?.Count() ?? 0;

                totalDocsCount = docs.Count();

                var notInventroyYetStatusDocs = docs?.Where(x => x.Status == (int)InventoryDocStatus.NotInventoryYet);
                if (notInventroyYetStatusDocs.Any())
                {
                    resultModel.DocCInfoModels = notInventroyYetStatusDocs?.OrderBy(x => x.DocStatusOrder)?.Skip((listDocCFilterModel.PageNum - 1) * listDocCFilterModel.PageSize)?.Take(listDocCFilterModel.PageSize)?.ToList();
                }

                resultModel.FinishCount = finishedDocsCount;
                resultModel.TotalCount = totalDocsCount;

            }
            else if (listDocCFilterModel.ActionType == InventoryActionType.ConfirmInventory)
            {
                finishedDocsCount = docs.Where(x => x.Status == (int)InventoryDocStatus.Confirmed
                                                    || x.Status == (int)InventoryDocStatus.AuditPassed
                                                    || x.Status == (int)InventoryDocStatus.AuditFailed
                                                    )?.Count() ?? 0;

                totalDocsCount = docs.Count();

                var waitingConfirmStatusDocs = docs?.Where(x => x.Status == (int)InventoryDocStatus.WaitingConfirm);
                if (waitingConfirmStatusDocs.Any())
                {
                    resultModel.DocCInfoModels = waitingConfirmStatusDocs?.OrderBy(x => x.DocStatusOrder)?.Skip((listDocCFilterModel.PageNum - 1) * listDocCFilterModel.PageSize)?.Take(listDocCFilterModel.PageSize)?.ToList();
                }

                resultModel.FinishCount = finishedDocsCount;
                resultModel.TotalCount = totalDocsCount;
            }




            return new ResponseModel<DocCListViewModel>
            {
                Code = StatusCodes.Status200OK,
                Data = resultModel
            };
        }

        public async Task<ResponseModel<DocumentDetailModel>> DetailOfDocument(DocumentDetailFilterModel documentDetailFilterModel)
        {
            //_inventoryContext.DocOutputs => Danh sách linh kiện phía trên của A,B,E, C
            //_inventoryContext.DocTypeCDetails => Danh sách linh kiện phía dưới của C  
            //=> Nếu vào phiếu C sẽ có 2 danh sách linh kiện cần hiển thị: DocOutputs, DocTypeCDetails (phân trang 10 bản ghi)

            if (!documentDetailFilterModel.ActionType.HasValue)
            {
                return new ResponseModel<DocumentDetailModel>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Loại kiểm kê không hợp lệ."
                };
            }

            var docDetail = await _inventoryContext.InventoryDocs.AsNoTracking()
                                                                 .FirstOrDefaultAsync(x => x.InventoryId == documentDetailFilterModel.InventoryId
                                                                                        && x.Id == documentDetailFilterModel.DocumentId);
            if (docDetail == null)
            {
                return new ResponseModel<DocumentDetailModel>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu của phiếu này."
                };
            }

            //Nếu account là giám sát thì check xem có quyền truy cập phiếu khu vực này không
            //var currAccount = await _inventoryContext.InventoryAccounts.Include(x => x.AccountLocations)
            //                                                               .ThenInclude(x => x.InventoryLocation)
            //                                                           .AsNoTracking()
            //                                                           .FirstOrDefaultAsync(x => x.UserId == documentDetailFilterModel.AccountId);

            //if (currAccount.RoleType.Value == InventoryAccountRoleType.Audit)
            //{
            //    //Các khu vực mà tài khoản giám sát có quyền
            //    var locations = currAccount.AccountLocations.Select(x => x.InventoryLocation.Name.ToLower()).ToList();
            //    var isDocInLocation = locations?.Contains(docDetail.LocationName.ToLower()) ?? false;
            //    if (!isDocInLocation)
            //    {
            //        return new ResponseModel<DocumentDetailModel>
            //        {
            //            Code = (int)HttpStatusCodes.ComponentNotInYourAudit,
            //            Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentNotInYourAudit)
            //        };
            //    }
            //}

            //Nếu chọn hành động xác nhận, sau đó chọn xem chi tiết phiếu
            //mà phiếu chưa được kiểm kê thì không được truy cập, cần kiểm kê trước
            if (docDetail.DocType == InventoryDocType.C && documentDetailFilterModel.ActionType.Value == InventoryActionType.ConfirmInventory)
            {
                if ((int)docDetail.Status < (int)InventoryDocStatus.WaitingConfirm)
                {
                    return new ResponseModel<DocumentDetailModel>
                    {
                        Code = (int)HttpStatusCodes.DocumentNotInventoriedYet,
                        Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.DocumentNotInventoriedYet)
                    };
                }
            }

            //Nếu ấn chi tiết phiếu A,B,E trong phần giám sát
            //=> Check phiếu đó đã được xác nhận kiểm kê hay chưa
            if ((docDetail.DocType == InventoryDocType.A ||
                docDetail.DocType == InventoryDocType.B ||
                docDetail.DocType == InventoryDocType.E)
                && documentDetailFilterModel.ActionType.Value == InventoryActionType.Audit)
            {
                //Nếu phiếu chưa thực hiện kiểm kê
                if ((int)docDetail.Status < (int)InventoryDocStatus.WaitingConfirm)
                {
                    return new ResponseModel<DocumentDetailModel>
                    {
                        Code = (int)HttpStatusCodes.ComponentNotInventoryYet,
                        Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentNotInventoryYet)
                    };
                }

                //Nếu phiếu chưa được xác nhận kiểm kê
                if ((int)docDetail.Status < (int)InventoryDocStatus.Confirmed)
                {
                    return new ResponseModel<DocumentDetailModel>
                    {
                        Code = (int)HttpStatusCodes.DocumentNotConfirmInventory,
                        Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.DocumentNotConfirmInventory)
                    };
                }
            }

            DocumentDetailModel detailModel = new();
            detailModel.Status = (int)docDetail.Status;
            detailModel.SalesOrder = docDetail.SalesOrderNo;
            detailModel.ComponentCode = docDetail.ComponentCode;
            detailModel.ComponentName = docDetail.ComponentName;
            detailModel.DocCode = docDetail.DocCode;
            detailModel.PositionCode = docDetail.PositionCode;
            detailModel.DocType = (int)docDetail.DocType;
            detailModel.InventoryBy = docDetail?.InventoryBy ?? string.Empty;
            detailModel.InventoryAt = docDetail.InventoryAt.HasValue ? docDetail.InventoryAt.Value : null;
            detailModel.Note = docDetail?.Note ?? string.Empty;
            detailModel.ConfirmedBy = docDetail?.ConfirmBy ?? string.Empty;

            //Cho phiếu C
            detailModel.MachineModel = docDetail?.MachineModel ?? string.Empty;
            detailModel.MachineType = DocCMachineModel.GetDisplayMachineType(docDetail.MachineType);
            detailModel.LineName = DocCMachineModel.GetDisplayLineName(docDetail.LineName);
            detailModel.StageName = docDetail?.StageName ?? string.Empty;

            var domain = _httpContext.Request.Host.Value;

            var components = _inventoryContext.DocOutputs.AsNoTracking()
                                                            .Where(x => x.InventoryId == documentDetailFilterModel.InventoryId
                                                                    && x.InventoryDocId.Value == documentDetailFilterModel.DocumentId)
                                                            .OrderBy(x => x.CreatedAt)
                                                            .Select(x => new DocComponentABE
                                                            {
                                                                Id = x.Id,
                                                                InventoryId = x.InventoryId,
                                                                InventoryDocId = x.InventoryDocId.Value,
                                                                QuantityOfBom = x.QuantityOfBom,
                                                                QuantityPerBom = x.QuantityPerBom,
                                                            });
            detailModel.DocComponentABEs = components;

            //Danh sách lịch sử thay đổi tổng số lượng của phiếu
            var docHistories = _inventoryContext.DocHistories.AsNoTracking()
                                                                .Where(x => x.InventoryId == documentDetailFilterModel.InventoryId
                                                                    && x.InventoryDocId.Value == documentDetailFilterModel.DocumentId)
                                                                .OrderByDescending(x => x.CreatedAt)
                                                                .Select(x => new DocHistoriesModel
                                                                {
                                                                    Id = x.Id,
                                                                    InventoryId = x.InventoryId,
                                                                    InventoryDocId = x.InventoryDocId.Value,
                                                                    Comment = x.Comment,
                                                                    EvicenceImg = string.IsNullOrEmpty(x.EvicenceImg) ? string.Empty : x.EvicenceImg,
                                                                    EvicenceImgTitle = string.IsNullOrEmpty(x.EvicenceImg) ? string.Empty : Path.GetFileName(x.EvicenceImg),
                                                                    Action = (int)x.Action,
                                                                    ChangeLogModel = new ChangeLogModel
                                                                    {
                                                                        IsChangeCDetail = x.IsChangeCDetail,
                                                                        NewQuantity = x.NewQuantity,
                                                                        OldQuantity = x.OldQuantity,
                                                                        NewStatus = (int)x.NewStatus,
                                                                        OldStatus = (int)x.OldStatus
                                                                    },
                                                                    CreatedAt = x.CreatedAt,
                                                                    CreatedBy = x.CreatedBy,
                                                                    Status = (int)x.Status
                                                                });
            detailModel.DocHistories = docHistories;

            if (docDetail.DocType == InventoryDocType.C)
            {
                const int pageSize = 10;
                var skip = (documentDetailFilterModel.Page - 1) * pageSize;

                Func<DocTypeCDetail, bool> condition = (document) =>
                {
                    var validInventoryId = true;
                    var validInventoryDocId = true;
                    var searchTerm = true;

                    validInventoryId = document.InventoryId == documentDetailFilterModel.InventoryId;
                    validInventoryDocId = document.InventoryDocId.Value == documentDetailFilterModel.DocumentId;

                    if (!string.IsNullOrEmpty(documentDetailFilterModel.SearchTerm))
                    {
                        searchTerm = !string.IsNullOrEmpty(document.ComponentCode) && document.ComponentCode.ToLower().Contains(documentDetailFilterModel.SearchTerm.ToLower()) ||
                                      !string.IsNullOrEmpty(document.ModelCode) && document.ModelCode.ToLower().Contains(documentDetailFilterModel.SearchTerm.ToLower());
                    }

                    return validInventoryId && validInventoryDocId && searchTerm;
                };

                var componentsC = _inventoryContext.DocTypeCDetails.AsNoTracking()
                                                                        .Where(condition)
                                                                        .OrderBy(x => x.No)
                                                                        .Select(x => new DocComponentC
                                                                        {
                                                                            Id = x.Id,
                                                                            InventoryId = x.InventoryId,
                                                                            InventoryDocId = x.InventoryDocId.Value,
                                                                            ComponentCode = x.ComponentCode,
                                                                            ModelCode = x.ModelCode,
                                                                            QuantityOfBom = x.QuantityOfBom,
                                                                            QuantityPerBom = x.QuantityPerBom,
                                                                            IsHighLight = x.isHighlight,
                                                                            No = x.No
                                                                        });

                //Nếu giám sát thì order các linh kiện highlight lên đầu
                if (documentDetailFilterModel.ActionType.HasValue && documentDetailFilterModel.ActionType.Value == InventoryActionType.ConfirmInventory)
                {
                    componentsC = componentsC.OrderByDescending(x => x.IsHighLight);
                }

                componentsC = componentsC.Skip(skip).Take(pageSize);
                var totalRows = _inventoryContext.DocTypeCDetails.AsNoTracking()
                                                                        ?.Where(condition)
                                                                        ?.Count() ?? 0;
                if (totalRows > 0)
                {
                    detailModel.DocCTotalPages = (int)Math.Ceiling((double)totalRows / pageSize);
                }

                detailModel.DocComponentCs = componentsC;
            }

            return new ResponseModel<DocumentDetailModel>
            {
                Code = StatusCodes.Status200OK,
                Data = detailModel
            };
        }

        public async Task<ResponseModel<HistoryDetailViewModel>> HistoryDetail(Guid inventoryId, Guid accountId, Guid historyId, string searchTerm, int page = 1)
        {
            var docHistory = await _inventoryContext.DocHistories.Include(x => x.InventoryDoc).AsNoTracking()
                                                                 .FirstOrDefaultAsync(x => x.InventoryId == inventoryId
                                                                                        && x.Id == historyId);
            if (docHistory == null)
            {
                return new ResponseModel<HistoryDetailViewModel>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu lịch sử phù hợp."
                };
            }

            //Nếu account là giám sát thì check xem có quyền truy cập phiếu khu vực này không
            //var currAccount = await _inventoryContext.InventoryAccounts.Include(x => x.AccountLocations)
            //                                                               .ThenInclude(x => x.InventoryLocation)
            //                                                                .AsNoTracking()
            //                                                           .FirstOrDefaultAsync(x => x.UserId == accountId);

            //if (currAccount.RoleType.Value == InventoryAccountRoleType.Audit)
            //{
            //    //Các khu vực mà tài khoản giám sát có quyền
            //    var locations = currAccount.AccountLocations.Select(x => x.InventoryLocation.Name.ToLower()).ToList();
            //    var isDocInLocation = locations?.Contains(docHistory.InventoryDoc.LocationName.ToLower()) ?? false;
            //    if (!isDocInLocation)
            //    {
            //        return new ResponseModel<HistoryDetailViewModel>
            //        {
            //            Code = (int)HttpStatusCodes.ComponentNotInYourAudit,
            //            Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentNotInYourAudit)
            //        };
            //    }
            //}

            var document = await _inventoryContext.InventoryDocs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == docHistory.InventoryDocId.Value);
            HistoryDetailViewModel historyDetailViewModel = new();
            historyDetailViewModel.Id = docHistory.Id;
            historyDetailViewModel.InventoryId = docHistory.InventoryId;
            historyDetailViewModel.InventoryDocId = docHistory.InventoryDocId.Value;
            historyDetailViewModel.Status = (int)document.Status;
            historyDetailViewModel.Comment = docHistory.Comment;
            historyDetailViewModel.Action = (int)docHistory.Action;
            historyDetailViewModel.DocType = document != null ? (int)document.DocType : null;
            historyDetailViewModel.DocName = document != null ? document.DocCode : null;
            historyDetailViewModel.CreatedAt = docHistory.CreatedAt;
            historyDetailViewModel.UpdatedAt = docHistory.UpdatedAt;
            historyDetailViewModel.CreatedBy = docHistory.CreatedBy;
            historyDetailViewModel.UpdatedBy = docHistory.UpdatedBy;
            historyDetailViewModel.ComponentCode = document.ComponentCode;
            historyDetailViewModel.ComponentName = document.ComponentName;

            if (!string.IsNullOrEmpty(docHistory.EvicenceImg))
            {
                historyDetailViewModel.EvicenceImg = docHistory.EvicenceImg;
                historyDetailViewModel.EvicenceImgTitle = Path.GetFileName(docHistory.EvicenceImg);
            }

            historyDetailViewModel.InventoryBy = document?.InventoryBy ?? string.Empty;

            historyDetailViewModel.MachineModel = docHistory?.InventoryDoc?.MachineModel ?? string.Empty;
            historyDetailViewModel.MachineType = DocCMachineModel.GetDisplayMachineType(docHistory?.InventoryDoc?.MachineType);
            historyDetailViewModel.LineName = DocCMachineModel.GetDisplayLineName(docHistory?.InventoryDoc?.LineName);
            historyDetailViewModel.StageName = docHistory?.InventoryDoc?.StageName ?? string.Empty;

            //Add Change log model
            historyDetailViewModel.ChangeLogModel.OldStatus = (int)docHistory.OldStatus;
            historyDetailViewModel.ChangeLogModel.NewStatus = (int)docHistory.NewStatus;
            historyDetailViewModel.ChangeLogModel.OldQuantity = docHistory.OldQuantity;
            historyDetailViewModel.ChangeLogModel.NewQuantity = docHistory.NewQuantity;
            historyDetailViewModel.ChangeLogModel.IsChangeCDetail = docHistory.IsChangeCDetail;

            historyDetailViewModel.HistoryOutputs = _inventoryContext.HistoryOutputs.AsNoTracking()
                                                                        .Where(x => x.DocHistoryId.Value == historyId)
                                                                        .Select(x => new HistoryOutputModel
                                                                        {
                                                                            Id = x.Id,
                                                                            InventoryId = x.InventoryId,
                                                                            HistoryId = x.DocHistoryId.Value,
                                                                            QuantityOfBom = x.QuantityOfBom,
                                                                            QuantityPerBom = x.QuantityPerBom
                                                                        });

            if (document.DocType == InventoryDocType.C)
            {
                //Danh sách linh kiện phiếu C
                const int pageSize = 10;
                var skip = (page - 1) * pageSize;
                Func<HistoryTypeCDetail, bool> condition = (x) =>
                {
                    var validHistoryId = true;
                    var validSearchTerm = true;

                    validHistoryId = x.HistoryId.Value == historyId;

                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        validSearchTerm = x.ComponentCode.ToLower().Contains(searchTerm.ToLower());

                        validSearchTerm = !string.IsNullOrEmpty(x.ComponentCode) && x.ComponentCode.ToLower().Contains(searchTerm.ToLower()) ||
                                      !string.IsNullOrEmpty(x.ModelCode) && x.ModelCode.ToLower().Contains(searchTerm.ToLower());
                    }

                    return validHistoryId && validSearchTerm;
                };

                //Danh sách lịch sử linh kiện phiếu C
                historyDetailViewModel.HistoryDetailCs = _inventoryContext.HistoryTypeCDetails.AsNoTracking()
                                                            .Where(condition)
                                                            .OrderByDescending(x => x.IsHighlight)
                                                            .Skip(skip)
                                                            .Take(pageSize)
                                                            .Select(x => new HistoryDetailC
                                                            {
                                                                Id = x.Id,
                                                                HistoryId = x.HistoryId.Value,
                                                                InventoryId = x.InventoryId,
                                                                QuantityPerBom = x.QuantityPerBom,
                                                                QuantityOfBom = x.QuantityOfBom,
                                                                ModelCode = x.ModelCode,
                                                                ComponentCode = x.ComponentCode,
                                                                IsHighLight = x.IsHighlight,
                                                            });

                var totalRows = _inventoryContext.HistoryTypeCDetails.AsNoTracking()
                                                                       ?.Where(condition)
                                                                       ?.Count() ?? 0;

                historyDetailViewModel.DocCTotalPages = (int)Math.Ceiling((double)totalRows / pageSize);
            }


            return new ResponseModel<HistoryDetailViewModel>
            {
                Code = StatusCodes.Status200OK,
                Data = historyDetailViewModel
            };
        }

        public async Task<ResponseModel> CheckValidInventoryDate(Guid inventoryId)
        {
            var inventory = await _inventoryContext.Inventories.AsNoTracking()
                                                               .FirstOrDefaultAsync(x => x.Id == inventoryId);
            if (inventory == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy đợt kiểm kê."
                    //Message = "Hiện đang không nằm trong thời gian kiểm kê. Vui lòng thử lại sau."
                };
            }

            var currentDate = DateTime.Now.Date;
            if (currentDate <= inventory.InventoryDate.Date == false)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.NotYetInventoryDate,
                    Message = "Hiện đang không nằm trong thời gian kiểm kê. Vui lòng thử lại sau."
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK
            };
        }

        public async Task<ResponseModel> CheckValidInventoryRole(Guid accountId, params int[] accessTos)
        {
            var account = await _inventoryContext.InventoryAccounts.AsNoTracking()
                                                                   .FirstOrDefaultAsync(x => x.UserId == accountId);

            if (account == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = commonAPIConstant.ResponseMessages.UnAuthorized
                };
            }

            var roleType = (int)account.RoleType;
            if (accessTos.Contains(roleType))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK
                };
            }
            else
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = commonAPIConstant.ResponseMessages.UnAuthorized
                };
            }
        }

        public async Task<ResponseModel> SubmitInventory(string inventoryId, string accountId, string docId, SubmitInventoryDto submitInventoryDto)
        {
            //20240910: Nếu tài khoản có vai trò xúc tiến => Được xem hoặc cập nhật phiếu kiểm kê:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                //Check Đợt kiểm kê xem có bị khóa hay không:
                var checkExistInventoryQuery = await _inventoryContext.Inventories.FirstOrDefaultAsync(x => x.Id.ToString().ToLower() == inventoryId.ToLower() && x.IsLocked != true);
                if (checkExistInventoryQuery == null)
                {
                    return new ResponseModel
                    {
                        Code = (int)HttpStatusCodes.IsLockedInventory,
                        Message = $"Đợt kiểm kê đã bị khóa.",
                    };
                }
                //Check đã tới ngày kiểm kê hay không:
                if (DateTime.Now.Date > checkExistInventoryQuery.InventoryDate.Date)
                {
                    return new ResponseModel
                    {
                        Code = (int)HttpStatusCodes.NotYetInventoryDate,
                        Message = $"Không thể xác nhận kiểm kê vì đã quá ngày kiểm kê của đợt kiểm kê hiện tại. Vui lòng thử lại sau.",
                    };

                }
                //check role xem có đang là kiểm kê hay không:
                var checkRoleType = await _inventoryContext.InventoryAccounts.FirstOrDefaultAsync(x => x.UserId.ToString().ToLower() == accountId.ToLower() && x.RoleType == InventoryAccountRoleType.Inventory);
                if (checkRoleType == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status403Forbidden,
                        Message = $"Không có quyền truy cập.",
                    };
                }
                var getInventoryDocQuery = await _inventoryContext.InventoryDocs.FirstOrDefaultAsync(x => x.Id.ToString().ToLower() == docId.ToLower() && x.InventoryId.ToString().ToLower() == inventoryId.ToLower() &&
                                                        x.AssignedAccountId.ToString().ToLower() == accountId.ToLower());
                //check tài khoản đã được assign vào phiểu kiểm kê hay chưa?
                if (getInventoryDocQuery == null)
                {
                    return new ResponseModel
                    {
                        Code = (int)HttpStatusCodes.NotAssigneeAccountId,
                        Message = $"Tài khoản chưa được assign vào phiếu kiểm kê.",
                    };
                }
                //Check trạng thái phiếu kiểm kê phải khác 0 hoặc 1:
                if (getInventoryDocQuery.Status == InventoryDocStatus.NotReceiveYet || getInventoryDocQuery.Status == InventoryDocStatus.NoInventory)
                {
                    return new ResponseModel
                    {
                        Code = (int)HttpStatusCodes.InvalidStatusInventoryDoc,
                        Message = $"Trạng thái phiếu kiểm kê đang không đúng. Vui lòng thử lại sau",
                    };
                }
            }

            var checkExistInventory = await _inventoryContext.Inventories.FirstOrDefaultAsync(x => x.Id.ToString().ToLower() == inventoryId.ToLower());
            var getInventoryDoc = await _inventoryContext.InventoryDocs.FirstOrDefaultAsync(x => x.Id.ToString().ToLower() == docId.ToLower() && x.InventoryId.ToString().ToLower() == inventoryId.ToLower());

            var InventoryName = checkExistInventory.Name;
            var DocCode = getInventoryDoc.DocCode;
            var getDocHistoryId = Guid.Empty;

            //Lấy Status hiện tại trong InventoryDoc:
            var getCurrentStatus_InventoryDoc = getInventoryDoc.Status;
            var getUpdateStatus_InventoryDoc = InventoryDocStatus.WaitingConfirm;
            var getCurrentQuantity_InventoryDoc = getInventoryDoc.Quantity;
            double updateQuantity_InventoryDoc = 0;

            //Cập nhật trạng thái, InventoryAt, InventoryBy
            //Logic cập nhật trạng thái như sau: các trạng thái 2 || 3 || 4 ==> Khi submit kiểm kê về trạng thái 3
            if (getInventoryDoc.Status == InventoryDocStatus.NotInventoryYet || getInventoryDoc.Status == InventoryDocStatus.WaitingConfirm
                || getInventoryDoc.Status == InventoryDocStatus.MustEdit)
            {
                getInventoryDoc.Status = InventoryDocStatus.WaitingConfirm;
            }
            getInventoryDoc.InventoryAt = DateTime.Now;
            getInventoryDoc.InventoryBy = submitInventoryDto.UserCode;
            //getUpdateStatus_InventoryDoc = getInventoryDoc.Status;
            //Lưu DocOutput, DocHistory, HistoryOutput:
            //Check DocOutput: Nếu chưa có dữ liệu thì thêm mới, có dữ liệu rồi thì cập nhật lại:
            var getDocOutput = await _inventoryContext.DocOutputs.Where(x => x.InventoryDocId.ToString().ToLower() == docId.ToLower()
                                      && x.InventoryId.ToString().ToLower() == inventoryId.ToLower()).ToListAsync();
            if (getDocOutput.Count() == 0)
            {
                //Lưu vào DocOutput:
                List<DocOutput> docs = new();
                if (submitInventoryDto.DocOutputs.Any())
                {
                    foreach (var item in submitInventoryDto.DocOutputs)
                    {
                        docs.Add(new DocOutput
                        {
                            Id = Guid.NewGuid(),
                            InventoryDocId = Guid.Parse(docId),
                            InventoryId = Guid.Parse(inventoryId),
                            QuantityOfBom = item.QuantityOfBom,
                            QuantityPerBom = item.QuantityPerBom,
                            CreatedAt = DateTime.Now,
                            CreatedBy = submitInventoryDto.UserCode
                        });
                    }
                    _inventoryContext.DocOutputs.AddRange(docs);
                }

                //Cập nhật số lượng: Quantity trong InventoryDoc
                updateQuantity_InventoryDoc = submitInventoryDto.DocOutputs.Sum(x => x.QuantityOfBom * x.QuantityPerBom);
                getInventoryDoc.Quantity = updateQuantity_InventoryDoc;

                //Trạng thái đã xác nhận đổi về chưa xác nhận nếu có thay đổi về quantity và doctypeCDetail:
                if (getInventoryDoc.Status == InventoryDocStatus.Confirmed && (getCurrentQuantity_InventoryDoc != updateQuantity_InventoryDoc || submitInventoryDto.DocTypeCDetails.Any()))
                {
                    getInventoryDoc.Status = InventoryDocStatus.WaitingConfirm;
                }
                getUpdateStatus_InventoryDoc = getInventoryDoc.Status;
                //Cập nhật DocHistoryDto, lần đầu chưa có DocOutput và docHistory sẽ thêm mới changelog:
                //Lưu Lịch sử:
                var newDocHistory = new DocHistory
                {
                    Id = Guid.NewGuid(),
                    InventoryId = Guid.Parse(inventoryId),
                    InventoryDocId = Guid.Parse(docId),
                    Action = DocHistoryActionType.Inventory,
                    CreatedAt = DateTime.Now,
                    CreatedBy = submitInventoryDto.UserCode,
                    OldQuantity = getCurrentQuantity_InventoryDoc,
                    NewQuantity = updateQuantity_InventoryDoc,
                    OldStatus = getCurrentStatus_InventoryDoc,
                    NewStatus = getUpdateStatus_InventoryDoc,
                    Status = getUpdateStatus_InventoryDoc,
                    IsChangeCDetail = submitInventoryDto.DocTypeCDetails.Count() == 0 ? false : true,
                    EvicenceImg = UploadImage(submitInventoryDto.Image, InventoryName, DocCode),
                };

                _inventoryContext.DocHistories.Add(newDocHistory);

                //Cập nhật dữ liệu từ DocOutput sang HistoryOutput:
                getDocHistoryId = newDocHistory.Id;

                List<HistoryOutput> hisOuts = new();
                if (submitInventoryDto.DocOutputs.Any())
                {
                    foreach (var item in submitInventoryDto.DocOutputs)
                    {
                        hisOuts.Add(new HistoryOutput
                        {
                            Id = Guid.NewGuid(),
                            DocHistoryId = getDocHistoryId,
                            InventoryId = Guid.Parse(inventoryId),
                            QuantityOfBom = item.QuantityOfBom,
                            QuantityPerBom = item.QuantityPerBom,
                            CreatedAt = DateTime.Now,
                            CreatedBy = submitInventoryDto.UserCode
                        });
                    }
                    _inventoryContext.HistoryOutputs.AddRange(hisOuts);
                }

            }
            else
            {
                //Nếu mobile truyền những Ids muốn xóa thì Xóa những danh sách Ids trong DocOutput và HistoryOutput:
                var getIdsDeleteDocOutPut = await _inventoryContext.DocOutputs.Where(x => submitInventoryDto.IdsDeleteDocOutPut.Contains(x.Id)).ToListAsync();
                _inventoryContext.DocOutputs.RemoveRange(getIdsDeleteDocOutPut);

                //Cập nhật DocOutput:
                if (submitInventoryDto.DocOutputs.Any())
                {
                    double sumIdsNull = 0, sumIdsNotNull = 0;

                    //case id != null:
                    var updateIds = submitInventoryDto.DocOutputs.Where(x => x.Id != null);

                    var getIdsNotNull = submitInventoryDto.DocOutputs.Where(x => x.Id != null).Select(x => x.Id).ToList();

                    var getDocOutputWithIdsNotNull = await _inventoryContext.DocOutputs.Where(x => getIdsNotNull.Contains(x.Id)).ToListAsync();
                    if (getDocOutputWithIdsNotNull.Count() > 0)
                    {
                        foreach (var editModel in updateIds)
                        {
                            var updateEntity = getDocOutputWithIdsNotNull.FirstOrDefault(x => x.Id == editModel.Id);
                            updateEntity.QuantityOfBom = editModel.QuantityOfBom;
                            updateEntity.QuantityPerBom = editModel.QuantityPerBom;
                            updateEntity.UpdatedAt = DateTime.Now;
                            updateEntity.UpdatedBy = submitInventoryDto.UserCode;
                        }
                        sumIdsNotNull = getDocOutputWithIdsNotNull.Sum(x => x.QuantityOfBom * x.QuantityPerBom);
                    }

                    //Xóa bỏ DocOutput trước đó mà không cập nhật:
                    var removeDocOutputList = getDocOutput.Where(x => !getDocOutputWithIdsNotNull.Any(y => y.Id == x.Id)).ToList();
                    _inventoryContext.DocOutputs.RemoveRange(removeDocOutputList);

                    //case id null:
                    List<DocOutput> docs = new();
                    var insertIds = submitInventoryDto.DocOutputs.Where(x => x.Id == null).ToList();
                    if (insertIds.Count() > 0)
                    {
                        foreach (var insertItem in insertIds)
                        {
                            docs.Add(new DocOutput
                            {
                                Id = Guid.NewGuid(),
                                InventoryDocId = Guid.Parse(docId),
                                InventoryId = Guid.Parse(inventoryId),
                                QuantityOfBom = insertItem.QuantityOfBom,
                                QuantityPerBom = insertItem.QuantityPerBom,
                                CreatedAt = DateTime.Now,
                                CreatedBy = submitInventoryDto.UserCode
                            });
                        }
                        //Sum của những Ids null:
                        sumIdsNull = insertIds.Sum(x => x.QuantityOfBom * x.QuantityPerBom);
                        _inventoryContext.DocOutputs.AddRange(docs);
                    }

                    updateQuantity_InventoryDoc = sumIdsNull + sumIdsNotNull;
                    getInventoryDoc.Quantity = updateQuantity_InventoryDoc;

                }

                //Cập nhật DocHistoryDto, đã có DocOutput và docHistory logic changelog:

                double oldQuantity_his, newQuantity_his;
                if (getCurrentQuantity_InventoryDoc == updateQuantity_InventoryDoc)
                {
                    oldQuantity_his = getCurrentQuantity_InventoryDoc;
                    newQuantity_his = getCurrentQuantity_InventoryDoc;
                }
                else
                {
                    oldQuantity_his = getCurrentQuantity_InventoryDoc;
                    newQuantity_his = updateQuantity_InventoryDoc;
                }

                //Trạng thái đã xác nhận đổi về chưa xác nhận nếu có thay đổi về quantity và doctypeCDetail:
                if (getInventoryDoc.Status == InventoryDocStatus.Confirmed && (getCurrentQuantity_InventoryDoc != updateQuantity_InventoryDoc || submitInventoryDto.DocTypeCDetails.Any()))
                {
                    getInventoryDoc.Status = InventoryDocStatus.WaitingConfirm;
                }
                getUpdateStatus_InventoryDoc = getInventoryDoc.Status;

                //Lưu Lịch sử DocHistory:
                var newDocHistory = new DocHistory
                {
                    Id = Guid.NewGuid(),
                    InventoryId = Guid.Parse(inventoryId),
                    InventoryDocId = Guid.Parse(docId),
                    Action = DocHistoryActionType.Inventory,
                    CreatedAt = DateTime.Now,
                    CreatedBy = submitInventoryDto.UserCode,
                    OldQuantity = oldQuantity_his,
                    NewQuantity = newQuantity_his,
                    OldStatus = getCurrentStatus_InventoryDoc,
                    NewStatus = getUpdateStatus_InventoryDoc,
                    Status = getUpdateStatus_InventoryDoc,
                    IsChangeCDetail = submitInventoryDto.DocTypeCDetails.Count() == 0 ? false : true,
                    EvicenceImg = UploadImage(submitInventoryDto.Image, InventoryName, DocCode),
                };

                _inventoryContext.DocHistories.Add(newDocHistory);

                getDocHistoryId = newDocHistory.Id;
                //Lưu HistoryOutput:
                if (submitInventoryDto.DocOutputs.Any())
                {
                    List<HistoryOutput> hisOuts = new();
                    foreach (var item in submitInventoryDto.DocOutputs)
                    {
                        hisOuts.Add(new HistoryOutput
                        {
                            Id = Guid.NewGuid(),
                            DocHistoryId = getDocHistoryId,
                            InventoryId = Guid.Parse(inventoryId),
                            QuantityOfBom = item.QuantityOfBom,
                            QuantityPerBom = item.QuantityPerBom,
                            CreatedAt = DateTime.Now,
                            CreatedBy = submitInventoryDto.UserCode
                        });
                    }
                    _inventoryContext.HistoryOutputs.AddRange(hisOuts);
                }
            }

            await _inventoryContext.SaveChangesAsync();

            ///Logic phiếu với DocType = C:
            if (getInventoryDoc.DocType == InventoryDocType.C)
            {
                var getDocTypeCDetail = await _inventoryContext.DocTypeCDetails.Where(x => x.InventoryDocId.ToString().ToLower() == docId.ToLower()
                                      && x.InventoryId.ToString().ToLower() == inventoryId.ToLower()).ToListAsync();
                if (getDocTypeCDetail.Count() > 0)
                {
                    if (getCurrentQuantity_InventoryDoc == updateQuantity_InventoryDoc)
                    {
                        //Cập nhật DocTypeCDetail:
                        foreach (var item in getDocTypeCDetail)
                        {
                            item.QuantityPerBom = item.QuantityPerBom < updateQuantity_InventoryDoc * item.QuantityOfBom ? item.QuantityPerBom : updateQuantity_InventoryDoc * item.QuantityOfBom;
                            item.isHighlight = item.QuantityPerBom < updateQuantity_InventoryDoc * item.QuantityOfBom ? true : false;
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedBy = submitInventoryDto.UserCode;
                        }
                    }
                    else
                    {
                        //Cập nhật DocTypeCDetail:
                        foreach (var item in getDocTypeCDetail)
                        {
                            item.QuantityPerBom = updateQuantity_InventoryDoc * item.QuantityOfBom;
                            item.isHighlight = item.QuantityPerBom < updateQuantity_InventoryDoc * item.QuantityOfBom ? true : false;
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedBy = submitInventoryDto.UserCode;
                        }
                    }

                    //Những linh kiện thay đổi từ mobile => cập nhật lại QuantityPerBom = Quantity * QuantityOfBom
                    if (submitInventoryDto.DocTypeCDetails.Any())
                    {
                        var updateIds = submitInventoryDto.DocTypeCDetails.Where(x => x.Id != null);

                        var getIdsNotNull = submitInventoryDto.DocTypeCDetails.Where(x => x.Id != null).Select(x => x.Id).ToList();

                        var getDocTypeCDetailWithIdsNotNull = await _inventoryContext.DocTypeCDetails.Where(x => getIdsNotNull.Contains(x.Id)).ToListAsync();

                        foreach (var editModel in updateIds)
                        {
                            var updateEntity = getDocTypeCDetailWithIdsNotNull.FirstOrDefault(x => x.Id == editModel.Id);
                            updateEntity.QuantityPerBom = updateQuantity_InventoryDoc * editModel.QuantityOfBom != editModel.QuantityPerBom ? editModel.QuantityPerBom : updateQuantity_InventoryDoc * editModel.QuantityOfBom;
                            updateEntity.isHighlight = updateQuantity_InventoryDoc * editModel.QuantityOfBom != editModel.QuantityPerBom ? true : false;
                            updateEntity.UpdatedAt = DateTime.Now;
                            updateEntity.UpdatedBy = submitInventoryDto.UserCode;
                        }
                    }
                }

                //Lưu lịch sử historyTypeCDetail:
                List<HistoryTypeCDetail> hisTypeC = new();
                foreach (var item in getDocTypeCDetail)
                {
                    hisTypeC.Add(new HistoryTypeCDetail
                    {
                        Id = Guid.NewGuid(),
                        HistoryId = getDocHistoryId,
                        InventoryId = Guid.Parse(inventoryId),
                        ComponentCode = item.ComponentCode,
                        ModelCode = item.ModelCode,
                        QuantityOfBom = item.QuantityOfBom,
                        QuantityPerBom = item.QuantityPerBom,
                        IsHighlight = item.isHighlight,
                        CreatedAt = DateTime.Now,
                        CreatedBy = submitInventoryDto.UserCode
                    });
                }
                _inventoryContext.HistoryTypeCDetails.AddRange(hisTypeC);


                //Call Backgroud Job để tổng hợp lại số lượng đối với phiếu C:
                //InventoryDocSubmitDto updateDocTotalParam = new InventoryDocSubmitDto();
                //updateDocTotalParam.InventoryId = Guid.Parse(inventoryId);
                //updateDocTotalParam.InventoryDocIds = getDocTypeCDetail.Select(x => x.InventoryDocId.Value).ToList();
                //updateDocTotalParam.ModelCodes = getDocTypeCDetail.Select(x => x.ModelCode).ToList();
                //updateDocTotalParam.DocType = InventoryDocType.C;
                //await _dataAggregationService.UpdateDataFromInventoryDoc(updateDocTotalParam);

            }
            await _inventoryContext.SaveChangesAsync();
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Đã thực hiện kiểm kê thành công.",
                Data = new
                {
                    Status = getUpdateStatus_InventoryDoc,
                    InventoryId = inventoryId,
                    AccountId = accountId,
                    DocId = docId,
                }
            };
        }

        public string UploadImage(IFormFile File, string InventoryName, string DocCode)
        {
            string filePath = null;
            if (File != null)
            {
                try
                {
                    var path = InitTypeDocumentPath(InitRootPath(), Path.Combine("images", "inventory"));
                    var userDirectoryPath = InitTypeDocumentPath(path, InventoryName);

                    var newFile = $"{DocCode}_{DateTime.Now.ToString(Constants.DatetimeFormat)}{Path.GetExtension(File.FileName)}";

                    filePath = Path.Combine(userDirectoryPath, newFile);
                    _logger.LogError($"Lưu ảnh filepath: {filePath}");
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        File.CopyTo(stream);
                        stream.Flush();
                    }

                    filePath = Path.Combine("images", "inventory", InventoryName, newFile);
                    _logger.LogError($"Lưu ảnh fullpath: {filePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Lỗi khi thực hiện lưu ảnh");
                    _logger.LogError(filePath);
                    _logger.LogError(ex.Message);
                    return filePath;
                }

            }
            return filePath;
        }

        public string InitRootPath()
        {
            var rootFolder = _configuration["UploadPath"];
            return rootFolder;
        }

        public string InitTypeDocumentPath(string previousPath, string typeDocument)
        {
            var path = Path.Combine(previousPath, typeDocument);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public async Task<ResponseModel<IEnumerable<string>>> GetModelCodesForDocC(Guid inventoryId, Guid accountId)
        {
            var checkAccountTypeAndRole = _httpContext.UserFromContext();

            var modelsQuery = _inventoryContext.InventoryDocs.AsNoTracking()
                                                        .Where(x => x.InventoryId.Value == inventoryId
                                                        //&& x.AssignedAccountId.Value == accountId
                                                        && x.DocType == InventoryDocType.C
                                                        && ExcludeDocStatus.Contains(x.Status) == false).AsQueryable();
            if (modelsQuery == null || !modelsQuery.Any())
            {
                return new ResponseModel<IEnumerable<string>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }
            var models = modelsQuery;
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                models = models.Where(x => x.AssignedAccountId.Value == accountId);
            }

            models = models.Where(x => !string.IsNullOrEmpty(x.MachineModel));
            var filter = await models?.Select(x => x.MachineModel)?.Distinct()?.ToListAsync();

            return new ResponseModel<IEnumerable<string>>
            {
                Code = StatusCodes.Status200OK,
                Data = filter ?? new List<string>()
            };
        }
        public async Task<ResponseModel<IEnumerable<MachineTypeModel>>> GetMachineTypesDocC(Guid inventoryId, Guid accountId, string machineModel)
        {
            var checkAccountTypeAndRole = _httpContext.UserFromContext();

            var modelsQuery = _inventoryContext.InventoryDocs.AsNoTracking().Where(x => ExcludeDocStatus.Contains(x.Status) == false
                                                                && x.InventoryId.Value == inventoryId
                                                                //&& x.AssignedAccountId.Value == accountId
                                                                && x.DocType == InventoryDocType.C
                                                                && x.MachineModel == machineModel).AsQueryable();


            if (modelsQuery == null || !modelsQuery.Any())
            {
                return new ResponseModel<IEnumerable<MachineTypeModel>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            var models = modelsQuery;
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                models = models.Where(x => x.AssignedAccountId.Value == accountId);
            }

            models = models.Where(x => !string.IsNullOrEmpty(x.MachineType));
            var filter = models?.GroupBy(x => x.MachineType).ToList();
            var result = filter.Select(x =>
            {
                MachineTypeModel model = new();
                model.Key = x.Key;
                model.DisplayName = DocCMachineModel.GetDisplayMachineType(x.Key);

                return model;
            });

            return new ResponseModel<IEnumerable<MachineTypeModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = result
            };
        }
        public async Task<ResponseModel<IEnumerable<LineModel>>> GetLineNamesDocC(Guid inventoryId, Guid accountId, string machineModel, string machineType)
        {

            var checkAccountTypeAndRole = _httpContext.UserFromContext();

            var modelsQuery = _inventoryContext.InventoryDocs.AsNoTracking().Where(x => ExcludeDocStatus.Contains(x.Status) == false
                                                                && x.InventoryId.Value == inventoryId
                                                                //&& x.AssignedAccountId.Value == accountId
                                                                && x.DocType == InventoryDocType.C
                                                                && x.MachineModel == machineModel && x.MachineType == machineType).AsQueryable();
            if (modelsQuery == null || !modelsQuery.Any())
            {
                return new ResponseModel<IEnumerable<LineModel>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }
            var models = modelsQuery;
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                models = models.Where(x => x.AssignedAccountId.Value == accountId);
            }

            models = models.Where(x => !string.IsNullOrEmpty(x.LineName));
            var filter = models?.GroupBy(x => x.LineName).ToList();
            var result = filter.Select(x =>
            {
                LineModel model = new();
                model.Key = x.Key;
                model.DisplayName = DocCMachineModel.GetDisplayLineName(x.Key);

                return model;
            });

            return new ResponseModel<IEnumerable<LineModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = result
            };
        }

        public async Task<ResponseModel<InventoryDocumentImportResultDto>> ImportAuditTargetAsync([FromForm] IFormFile file, Guid inventoryId)
        {
            var result = new ResponseModel<InventoryDocumentImportResultDto>(StatusCodes.Status200OK, new InventoryDocumentImportResultDto());
            if (file == null)
            {
                result.Code = StatusCodes.Status500InternalServerError;
                result.Message = "File import không tồn tại";
                return await Task.FromResult(result);
            }
            else if (!file.FileName.EndsWith(".xlsx") && file.ContentType != FileResponse.ExcelType)
            {
                result.Code = (int)HttpStatusCodes.InvalidFileExcel;
                result.Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo danh sách giám sát.";
                return await Task.FromResult(result);
            }

            //Kiểm tra phiếu A tồn tại mới cho import giám sát
            var existDocA = await _inventoryContext.InventoryDocs.AnyAsync(x => x.InventoryId == inventoryId && x.DocType == InventoryDocType.A);
            if (!existDocA)
            {
                result.Code = (int)HttpStatusCodes.NotExistDocTypeA;
                result.Message = "Vui lòng tạo phiếu A trước khi thực hiện tạo danh sách giám sát.";
                return await Task.FromResult(result);
            }

            result.Data = await ImportAuditTargets(file, inventoryId);
            if (result?.Data?.Code == (int)HttpStatusCodes.InvalidFileExcel)
            {
                return new ResponseModel<InventoryDocumentImportResultDto>
                {
                    Code = result.Data.Code,
                    Message = result.Data.Message
                };
            }
            return result;
        }
        private async Task<IEnumerable<string>> GetLayouts()
        {
            var req = new RestRequest(commonAPIConstant.Endpoint.Internal.absolute + "/layouts");
            req.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            req.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var result = await _restClientFactory.StorageClient().ExecuteGetAsync(req);
            var convertedResult = System.Text.Json.JsonSerializer.Deserialize<ResponseModel<IEnumerable<string>>>(result.Content, JsonDefaults.CamelCaseOtions);
            return convertedResult.Data;
        }
        private async Task<InventoryDocumentImportResultDto> ImportAuditTargets(IFormFile file, Guid inventoryId)
        {
            using (var memStream = new MemoryStream())
            {
                file.CopyTo(memStream);
                var checkDto = new CheckInventoryDocumentDto();
                var resultDto = new InventoryDocumentImportResultDto();
                using (var sourcePackage = new ExcelPackage(memStream))
                {
                    var sheets = sourcePackage.Workbook.Worksheets;

                    //init list inventory document entity
                    var auditTargets = new List<AuditTarget>();
                    var failCount = 0;
                    var successCount = 0;
                    //get last type A document number
                    var listAssignee = await GetLastDocNumberAndAssignee(InventoryDocType.A, inventoryId, true);
                    var layout = await _inventoryContext.InventoryLocations.Where(x => !x.IsDeleted).Select(x => x.Name).ToListAsync();
                    listAssignee.Layouts = layout;

                    var existedAuditTargets = await _inventoryContext.AuditTargets.AsNoTracking().Where(x => x.InventoryId == inventoryId).Select(x => new { x.WareHouseLocation, x.Plant, x.ComponentCode, x.PositionCode, x.LocationName }).ToListAsync();
                    //Get all Roles:
                    var request = new RestRequest(Constants.Endpoint.Internal.Get_All_Roles_Department);
                    request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
                    request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

                    var roles = await _identityRestClient.ExecuteGetAsync(request);

                    var rolesModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<GetAllRoleWithUserNameModel>>>(roles.Content ?? string.Empty);

                    var createDocumentRoles = rolesModel?.Data;
                    var inventoryDocCellDtoList = new List<InventoryDocCellDto>();

                    foreach (var sheet in sheets)
                    {
                        if (sheet != null)
                        {
                            List<int> validRowIndex = new();
                            for (var row = sheet.Dimension.Start.Row + 1; row <= sheet.Dimension.End.Row; row++)
                            {
                                try
                                {
                                    //Validate excel header template 
                                    var plantIndex = sheet.GetColumnIndex(AuditTargets.Plant);
                                    var whLocIndex = sheet.GetColumnIndex(AuditTargets.WarehouseLocation);
                                    var materialCodeIndex = sheet.GetColumnIndex(AuditTargets.MaterialCode);
                                    var storageBinIndex = sheet.GetColumnIndex(AuditTargets.PositionCode);
                                    var assigneeIndex = sheet.GetColumnIndex(AuditTargets.Assignee);
                                    var soNoIndex = sheet.GetColumnIndex(AuditTargets.SONo);
                                    //int noteIndex = sheet.GetColumnIndex(ImportExcelColumns.AuditTargets.Note);

                                    var requiredHeader = new List<int> {  plantIndex,
                                                                                whLocIndex,
                                                                                materialCodeIndex,
                                                                                storageBinIndex,
                                                                                assigneeIndex,
                                                                                soNoIndex
                                                                                //noteIndex
                                                                              };
                                    //- 1 => not found required header => wrong template
                                    if (requiredHeader.Any(x => x == -1))
                                    {
                                        return new InventoryDocumentImportResultDto
                                        {
                                            Code = (int)HttpStatusCodes.InvalidFileExcel,
                                            Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo danh sách giám sát."
                                        };
                                    }

                                    //get data cell value    
                                    var dataFromCell = GetDataFromCell(sheet, row, "AuditTarget");

                                    if (dataFromCell != null)
                                    {
                                        inventoryDocCellDtoList.Add(dataFromCell);
                                    }

                                    //check for all blank rows
                                    if (CheckRequiredColumn(true, dataFromCell.Plant, dataFromCell.WarehouseLocation, dataFromCell.ComponentCode, dataFromCell.PositionCode, dataFromCell.SONo, dataFromCell.Assignee))
                                    {
                                        if (row < sheet.Dimension.Rows)
                                        {
                                            sheet.Row(row).Hidden = true;

                                        }
                                        continue;

                                    }

                                    //validate data
                                    var validateData = ValidateCellData(inventoryId, createDocumentRoles, listAssignee, dataFromCell, sheet, row, checkDto, inventoryDocCellDtoList, "AuditTarget", sheet.Dimension.End.Column);
                                    if (!validateData.IsValid)
                                    {
                                        failCount++;
                                        continue;
                                    }
                                    else
                                    {
                                        //Nếu linh kiện đã được nhập vào hệ thống thì báo lỗi
                                        var isDuplicate = existedAuditTargets.Any(x => x.Plant == dataFromCell.Plant &&
                                                               x.WareHouseLocation == dataFromCell.WarehouseLocation &&
                                                               x.ComponentCode == dataFromCell.ComponentCode &&
                                                               x.LocationName == dataFromCell.LocationName &&
                                                               x.PositionCode == dataFromCell.PositionCode);

                                        if (isDuplicate)
                                        {
                                            sheet.Cells[1, sheet.GetColumnIndex(TypeA.Assignee) + 1].Value = TypeA.ErrorContent;
                                            sheet.Cells[row, sheet.GetColumnIndex(TypeA.Assignee) + 1].Value = "Linh kiện này đã được nhập vào hệ thống.";
                                            sheet.Cells[row, sheet.GetColumnIndex(TypeA.Assignee) + 1].Style.Font.Color.SetColor(System.Drawing.Color.Red);

                                            failCount++;
                                            continue;
                                        }
                                        else
                                        {

                                            validRowIndex.Add(row);
                                            successCount++;
                                            SetDataToEntites(inventoryId, listAssignee, auditTargets, dataFromCell);
                                        }
                                    }
                                }
                                catch (Exception exception)
                                {
                                    var exMess = $"Exception - {exception.Message} at row {row}";
                                    var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                                    _logger.LogError($"Request error at {_httpContext.Request.Path} : {exMess}; {innerExMess}");
                                    continue;
                                }
                            }
                            if (validRowIndex.Any())
                            {
                                if (sheet.Dimension.End.Row == (validRowIndex.Count + 1))
                                {
                                    //Delete sheet if all rows are valid
                                    sourcePackage.Workbook.Worksheets.Delete(sheet);
                                }
                                else
                                {
                                    for (int i = 0; i < validRowIndex.Count; i++)
                                    {
                                        sheet.DeleteRow(validRowIndex[i]);
                                    }
                                }
                            }
                        }

                        //if(failCount > 0)
                        //{
                        //    sheet.Cells[1, sheet.Dimension.Columns + 1].Value = "Nội dung lỗi";
                        //}
                    }

                    if (auditTargets.Any())
                    {
                        //Comment tạm để kiểm tra logic validate
                        await _inventoryContext.AuditTargets.AddRangeAsync(auditTargets);
                        await _inventoryContext.SaveChangesAsync(); ;
                    }

                    resultDto.Result = sourcePackage.GetAsByteArray();
                    resultDto.SuccessCount = successCount;
                    resultDto.FailCount = failCount;
                }
                return resultDto;
            }
        }

        private string GetEvicenceImg(string imagePath)
        {
            var domain = _httpContext.Request.Host.Value;
            return $"{domain}/{imagePath}";
        }

        public async Task<ResponseModel> SubmitConfirm(Guid inventoryId, Guid accountId, Guid docId, SubmitInventoryAction actionType, SubmitInventoryDto submitInventoryDto)
        {
            //20240910: Nếu tài khoản có vai trò xúc tiến => Được xem hoặc cập nhật phiếu kiểm kê:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                //Check Đợt kiểm kê xem có bị khóa hay không:
                var checkExistInventoryQuery = await _inventoryContext.Inventories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == inventoryId && x.IsLocked != true);
                if (checkExistInventoryQuery == null)
                {
                    return new ResponseModel
                    {
                        Code = (int)HttpStatusCodes.IsLockedInventory,
                        Message = $"Đợt kiểm kê đã bị khóa.",
                    };
                }

                //Check đã tới ngày kiểm kê hay không:
                if (DateTime.Now.Date > checkExistInventoryQuery.InventoryDate.Date)
                {
                    return new ResponseModel
                    {
                        Code = (int)HttpStatusCodes.NotYetInventoryDate,
                        Message = $"Không thể xác nhận kiểm kê vì đã quá ngày kiểm kê của đợt kiểm kê hiện tại. Vui lòng thử lại sau.",
                    };

                }
                //check role xem có đang là kiểm kê hay không:
                var checkRoleType = await _inventoryContext.InventoryAccounts.AnyAsync(x => x.UserId == accountId && x.RoleType == InventoryAccountRoleType.Inventory);
                if (!checkRoleType)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status403Forbidden,
                        Message = $"Không có quyền truy cập.",
                    };
                }
                var getInventoryDocQuery = await _inventoryContext.InventoryDocs.FirstOrDefaultAsync(x => x.Id == docId && x.InventoryId == inventoryId &&
                                                        x.AssignedAccountId == accountId);
                //check tài khoản đã được assign vào phiểu kiểm kê hay chưa?
                if (getInventoryDocQuery == null)
                {
                    return new ResponseModel
                    {
                        Code = (int)HttpStatusCodes.NotAssigneeAccountId,
                        Message = $"Tài khoản chưa được assign vào phiếu kiểm kê.",
                    };
                }
                //Check trạng thái phiếu kiểm kê phải khác 0 hoặc 1:
                if (getInventoryDocQuery.Status == InventoryDocStatus.NotReceiveYet || getInventoryDocQuery.Status == InventoryDocStatus.NoInventory)
                {
                    return new ResponseModel
                    {
                        Code = (int)HttpStatusCodes.InvalidStatusInventoryDoc,
                        Message = $"Trạng thái phiếu kiểm kê đang không đúng. Vui lòng thử lại sau",
                    };
                }
            }

            //string InventoryName = checkExistInventory.Name;
            //string DocCode = getInventoryDoc.DocCode;
            var getDocHistoryId = Guid.Empty;

            var getInventoryDoc = await _inventoryContext.InventoryDocs.FirstOrDefaultAsync(x => x.Id == docId && x.InventoryId == inventoryId);

            //Lấy Status hiện tại trong InventoryDoc:
            var getCurrentStatus_InventoryDoc = getInventoryDoc.Status;
            var getUpdateStatus_InventoryDoc = InventoryDocStatus.Confirmed;
            var getCurrentQuantity_InventoryDoc = getInventoryDoc.Quantity;
            double updateQuantity_InventoryDoc = 0;

            //Cập nhật trạng thái, InventoryAt, InventoryBy
            //Logic cập nhật trạng thái như sau: Nhấn Xác Nhận => trạng thái: 5 , Nhấn Từ chối => trạng thái: 4
            if (actionType == SubmitInventoryAction.Cancel)
            {
                getInventoryDoc.Status = InventoryDocStatus.MustEdit;
            }
            else if (actionType == SubmitInventoryAction.Confirm)
            {
                getInventoryDoc.Status = InventoryDocStatus.Confirmed;
            }
            else if (actionType == SubmitInventoryAction.Update)
            {
                if (getInventoryDoc.Status == InventoryDocStatus.MustEdit || getInventoryDoc.Status == InventoryDocStatus.Confirmed)
                {
                    getInventoryDoc.Status = InventoryDocStatus.Confirmed;
                }
                else if (getInventoryDoc.Status == InventoryDocStatus.AuditFailed)
                {
                    getInventoryDoc.Status = InventoryDocStatus.AuditFailed;
                }
            }
            getInventoryDoc.ConfirmAt = DateTime.Now;
            getInventoryDoc.ConfirmBy = submitInventoryDto.UserCode;
            getUpdateStatus_InventoryDoc = getInventoryDoc.Status;
            //Lưu DocOutput, DocHistory, HistoryOutput:
            //Check DocOutput: Nếu chưa có dữ liệu thì thêm mới, có dữ liệu rồi thì cập nhật lại:
            var getDocOutput = await _inventoryContext.DocOutputs.AsNoTracking().AnyAsync(x => x.InventoryDocId == docId
                                      && x.InventoryId == inventoryId);

            if (getDocOutput)
            {
                //Nếu mobile truyền những Ids muốn xóa thì Xóa những danh sách Ids trong DocOutput và HistoryOutput:
                var getIdsDeleteDocOutPut = _inventoryContext.DocOutputs.Where(x => submitInventoryDto.IdsDeleteDocOutPut.Contains(x.Id));
                await getIdsDeleteDocOutPut.ExecuteDeleteAsync();

                //Cập nhật DocOutput:
                if (submitInventoryDto.DocOutputs.Any())
                {
                    //case id null:
                    double sumIdsNull = 0, sumIdsNotNull = 0;

                    List<DocOutput> docs = new();
                    var insertIds = submitInventoryDto.DocOutputs.Where(x => x.Id == null).ToList();
                    if (insertIds.Any())
                    {
                        foreach (var insertItem in insertIds)
                        {
                            docs.Add(new DocOutput
                            {
                                Id = Guid.NewGuid(),
                                InventoryDocId = docId,
                                InventoryId = inventoryId,
                                QuantityOfBom = insertItem.QuantityOfBom,
                                QuantityPerBom = insertItem.QuantityPerBom,
                                CreatedAt = DateTime.Now,
                                CreatedBy = submitInventoryDto.UserCode
                            });
                        }
                        //Sum của những Ids null:
                        sumIdsNull = insertIds.Sum(x => x.QuantityOfBom * x.QuantityPerBom);
                        await _inventoryContext.DocOutputs.AddRangeAsync(docs);
                    }

                    //case id != null:
                    var updateIds = submitInventoryDto.DocOutputs.Where(x => x.Id != null).ToList();

                    var getIdsNotNull = submitInventoryDto.DocOutputs.Where(x => x.Id != null).Select(x => x.Id).ToList();

                    var getDocOutputWithIdsNotNull = await _inventoryContext.DocOutputs.Where(x => getIdsNotNull.Contains(x.Id)).ToListAsync();
                    if (getDocOutputWithIdsNotNull.Any())
                    {
                        foreach (var editModel in updateIds)
                        {
                            var updateEntity = getDocOutputWithIdsNotNull.FirstOrDefault(x => x.Id == editModel.Id);
                            updateEntity.QuantityOfBom = editModel.QuantityOfBom;
                            updateEntity.QuantityPerBom = editModel.QuantityPerBom;
                            updateEntity.UpdatedAt = DateTime.Now;
                            updateEntity.UpdatedBy = submitInventoryDto.UserCode;
                        }
                        sumIdsNotNull = getDocOutputWithIdsNotNull.Sum(x => x.QuantityOfBom * x.QuantityPerBom);
                    }

                    updateQuantity_InventoryDoc = sumIdsNull + sumIdsNotNull;
                    getInventoryDoc.Quantity = updateQuantity_InventoryDoc;

                }

                //Cập nhật DocHistoryDto, đã có DocOutput và docHistory logic changelog:

                double oldQuantity_his, newQuantity_his;
                if (getCurrentQuantity_InventoryDoc == updateQuantity_InventoryDoc)
                {
                    oldQuantity_his = getCurrentQuantity_InventoryDoc;
                    newQuantity_his = getCurrentQuantity_InventoryDoc;
                }
                else
                {
                    oldQuantity_his = getCurrentQuantity_InventoryDoc;
                    newQuantity_his = updateQuantity_InventoryDoc;
                }

                //Lưu Lịch sử DocHistory:
                var newDocHistory = new DocHistory
                {
                    Id = Guid.NewGuid(),
                    InventoryId = inventoryId,
                    InventoryDocId = docId,
                    Action = DocHistoryActionType.Confirm,
                    CreatedAt = DateTime.Now,
                    CreatedBy = submitInventoryDto.UserCode,
                    OldQuantity = oldQuantity_his,
                    NewQuantity = newQuantity_his,
                    OldStatus = getCurrentStatus_InventoryDoc,
                    NewStatus = getUpdateStatus_InventoryDoc,
                    Status = getUpdateStatus_InventoryDoc,
                    IsChangeCDetail = submitInventoryDto.DocTypeCDetails.Count != 0,
                    Comment = submitInventoryDto.Comment,
                };

                await _inventoryContext.DocHistories.AddAsync(newDocHistory);

                getDocHistoryId = newDocHistory.Id;
                //Lưu HistoryOutput:
                if (submitInventoryDto.DocOutputs.Any())
                {
                    List<HistoryOutput> hisOuts = new();
                    foreach (var item in submitInventoryDto.DocOutputs)
                    {
                        hisOuts.Add(new HistoryOutput
                        {
                            Id = Guid.NewGuid(),
                            DocHistoryId = getDocHistoryId,
                            InventoryId = inventoryId,
                            QuantityOfBom = item.QuantityOfBom,
                            QuantityPerBom = item.QuantityPerBom,
                            CreatedAt = DateTime.Now,
                            CreatedBy = submitInventoryDto.UserCode
                        });
                    }
                    await _inventoryContext.HistoryOutputs.AddRangeAsync(hisOuts);
                }
            }
            await _inventoryContext.SaveChangesAsync();
            ///Logic phiếu với DocType = C:
            if (getInventoryDoc.DocType == InventoryDocType.C)
            {
                var getDocTypeCDetail = await _inventoryContext.DocTypeCDetails.Where(x => x.InventoryDocId == docId
                                      && x.InventoryId == inventoryId).ToListAsync();
                if (getDocTypeCDetail.Any())
                {
                    if (getCurrentQuantity_InventoryDoc == updateQuantity_InventoryDoc)
                    {
                        //Cập nhật DocTypeCDetail:
                        foreach (var item in getDocTypeCDetail)
                        {
                            item.QuantityPerBom = item.QuantityPerBom < updateQuantity_InventoryDoc * item.QuantityOfBom ? item.QuantityPerBom : updateQuantity_InventoryDoc * item.QuantityOfBom;
                            item.isHighlight = item.QuantityPerBom < updateQuantity_InventoryDoc * item.QuantityOfBom;
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedBy = submitInventoryDto.UserCode;
                        }
                    }
                    else
                    {
                        //Cập nhật DocTypeCDetail:
                        foreach (var item in getDocTypeCDetail)
                        {
                            item.QuantityPerBom = updateQuantity_InventoryDoc * item.QuantityOfBom;
                            item.isHighlight = item.QuantityPerBom < updateQuantity_InventoryDoc * item.QuantityOfBom;
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedBy = submitInventoryDto.UserCode;
                        }
                    }

                    //Những linh kiện thay đổi từ mobile => cập nhật lại QuantityPerBom = Quantity * QuantityOfBom
                    if (submitInventoryDto.DocTypeCDetails.Any())
                    {
                        var updateIds = submitInventoryDto.DocTypeCDetails.Where(x => x.Id != null).ToList();

                        var getIdsNotNull = submitInventoryDto.DocTypeCDetails.Where(x => x.Id != null).Select(x => x.Id).ToList();

                        var getDocTypeCDetailWithIdsNotNull = await _inventoryContext.DocTypeCDetails.Where(x => getIdsNotNull.Contains(x.Id)).ToListAsync();

                        foreach (var editModel in updateIds)
                        {
                            var updateEntity = getDocTypeCDetailWithIdsNotNull.FirstOrDefault(x => x.Id == editModel.Id);
                            updateEntity.QuantityPerBom = updateQuantity_InventoryDoc * editModel.QuantityOfBom != editModel.QuantityPerBom ? editModel.QuantityPerBom : updateQuantity_InventoryDoc * editModel.QuantityOfBom;
                            updateEntity.isHighlight = updateQuantity_InventoryDoc * editModel.QuantityOfBom != editModel.QuantityPerBom;
                            updateEntity.UpdatedAt = DateTime.Now;
                            updateEntity.UpdatedBy = submitInventoryDto.UserCode;
                        }
                    }
                }

                //Lưu lịch sử historyTypeCDetail:
                List<HistoryTypeCDetail> hisTypeC = new();
                foreach (var item in getDocTypeCDetail)
                {
                    hisTypeC.Add(new HistoryTypeCDetail
                    {
                        Id = Guid.NewGuid(),
                        HistoryId = getDocHistoryId,
                        InventoryId = inventoryId,
                        ComponentCode = item.ComponentCode,
                        QuantityOfBom = item.QuantityOfBom,
                        QuantityPerBom = item.QuantityPerBom,
                        IsHighlight = item.isHighlight,
                        CreatedAt = DateTime.Now,
                        CreatedBy = submitInventoryDto.UserCode
                    });
                }
                await _inventoryContext.HistoryTypeCDetails.AddRangeAsync(hisTypeC);
                await _inventoryContext.SaveChangesAsync();
                //Call Backgroud Job để tổng hợp lại số lượng đối với phiếu C:
                //var updateDocTotalParam = new InventoryDocSubmitDto
                //{
                //    InventoryId = inventoryId,
                //    InventoryDocIds = getDocTypeCDetail.Select(x => x.InventoryDocId.Value).ToList(),
                //    ModelCodes = getDocTypeCDetail.Select(x => x.ModelCode).ToList(),
                //    DocType = InventoryDocType.C
                //};
                //await _dataAggregationService.UpdateDataFromInventoryDoc(updateDocTotalParam);

            }
            else
            {
                //await _dataAggregationService.UpdateDataFromInventoryDoc(new InventoryDocSubmitDto { DocType = InventoryDocType.A, InventoryId = inventoryId });
            }



            if (actionType == SubmitInventoryAction.Cancel)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Đã từ chối xác nhận kiểm kê linh kiện.",
                    Data = new
                    {
                        Status = getUpdateStatus_InventoryDoc,
                        InventoryId = inventoryId,
                        AccountId = accountId,
                        DocId = docId,
                    }
                };
            }
            else if (actionType == SubmitInventoryAction.Update)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Đã cập nhật chi tiết phiếu.",
                    Data = new
                    {
                        Status = getUpdateStatus_InventoryDoc,
                        InventoryId = inventoryId,
                        AccountId = accountId,
                        DocId = docId,
                    }
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = $"Đã xác nhận kiểm kê linh kiện thành công.",
                Data = new
                {
                    Status = getUpdateStatus_InventoryDoc,
                    InventoryId = inventoryId,
                    AccountId = accountId,
                    DocId = docId,
                }
            };

        }

        public async Task<ResponseModel> DropDownDepartment(string inventoryId, string accountId)
        {
            //Check Tồn tại phiếu kiểm kê:
            var checkExistInventory = await _inventoryContext.Inventories.AsNoTracking().FirstOrDefaultAsync(x => x.Id.ToString().ToLower() == inventoryId.ToLower());
            if (checkExistInventory == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy phiếu kiểm kê.",
                };
            }
            //Check đã tới ngày kiểm kê hay không:
            if (DateTime.Now.Date > checkExistInventory.InventoryDate.Date)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.NotYetInventoryDate,
                    Message = $"Không thể xác nhận kiểm kê vì đã quá ngày kiểm kê của đợt kiểm kê hiện tại. Vui lòng thử lại sau.",
                };

            }
            //check role xem có đang là giám sát hay không:
            var checkRoleType = await _inventoryContext.InventoryAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.UserId.ToString().ToLower() == accountId.ToLower() && x.RoleType == InventoryAccountRoleType.Audit);
            if (checkRoleType == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = $"Không có quyền truy cập.",
                };
            }

            var Departments = await _inventoryContext.AuditTargets.AsNoTracking().Where(x => x.InventoryId.ToString().ToLower() == inventoryId.ToLower()
                                                                && x.AssignedAccountId.ToString().ToLower() == accountId.ToLower())
                                                              .Select(x => x.DepartmentName)
                                                              .ToListAsync();
            if (Departments?.Count() == 0)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy danh sách phòng ban."
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = Departments.Distinct(),
                Message = "Danh sách phòng ban.",
            };

        }

        public async Task<ResponseModel> DropDownLocation(string inventoryId, string accountId, string departmentName)
        {
            //Check Tồn tại phiếu giám sát:
            var checkExistInventory = await _inventoryContext.Inventories.AsNoTracking().FirstOrDefaultAsync(x => x.Id.ToString().ToLower() == inventoryId.ToLower());
            if (checkExistInventory == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy phiếu giám sát.",
                };
            }
            //Check đã tới ngày giám sát hay không:
            if (DateTime.Now.Date > checkExistInventory.InventoryDate.Date)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.NotYetInventoryDate,
                    Message = $"Không thể xác nhận kiểm kê vì đã quá ngày kiểm kê của đợt kiểm kê hiện tại. Vui lòng thử lại sau.",
                };

            }
            //check role xem có đang là giám sát hay không:
            var checkRoleType = await _inventoryContext.InventoryAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.UserId.ToString().ToLower() == accountId.ToLower() && x.RoleType == InventoryAccountRoleType.Audit);
            if (checkRoleType == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = $"Không có quyền truy cập.",
                };
            }

            var locations = _inventoryContext.AuditTargets.AsNoTracking().Where(x => x.InventoryId.ToString().ToLower() == inventoryId.ToLower()
                                                                && x.AssignedAccountId.ToString().ToLower() == accountId.ToLower());

            if (!departmentName.IsNullOrEmpty() && departmentName != "-1")
            {
                locations = locations.Where(x => x.DepartmentName == departmentName);
            }

            if (locations?.Count() == 0)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy danh sách khu vực."
                };
            }

            var result = await locations.Select(x => x.LocationName).ToListAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = result.Distinct(),
                Message = "Danh sách khu vực.",
            };

        }

        public async Task<ResponseModel> DropDownComponentCode(string inventoryId, string accountId, string departmentName, string locationName)
        {
            //Check Tồn tại phiếu giám sát:
            var checkExistInventory = await _inventoryContext.Inventories.AsNoTracking().FirstOrDefaultAsync(x => x.Id.ToString().ToLower() == inventoryId.ToLower());
            if (checkExistInventory == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy phiếu giám sát.",
                };
            }
            //Check đã tới ngày giám sát hay không:
            if (DateTime.Now.Date > checkExistInventory.InventoryDate.Date)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.NotYetInventoryDate,
                    Message = $"Không thể xác nhận kiểm kê vì đã quá ngày kiểm kê của đợt kiểm kê hiện tại. Vui lòng thử lại sau.",
                };

            }
            //check role xem có đang là giám sát hay không:
            var checkRoleType = await _inventoryContext.InventoryAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.UserId.ToString().ToLower() == accountId.ToLower() && x.RoleType == InventoryAccountRoleType.Audit);
            if (checkRoleType == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = $"Không có quyền truy cập.",
                };
            }

            var componentCodes = _inventoryContext.AuditTargets.AsNoTracking().Where(x => x.InventoryId.ToString().ToLower() == inventoryId.ToLower()
                                                                && x.AssignedAccountId.ToString().ToLower() == accountId.ToLower());
            if (!departmentName.IsNullOrEmpty() && departmentName != "-1" && !locationName.IsNullOrEmpty() && locationName != "-1")
            {
                componentCodes = componentCodes.Where(x => x.DepartmentName == departmentName && x.LocationName == locationName);
            }
            if (componentCodes?.Count() == 0)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy danh sách mã linh kiện."
                };
            }

            var result = await componentCodes.Select(x => x.ComponentCode).ToListAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = result.Distinct(),
                Message = "Danh sách mã linh kiện.",
            };

        }

        public async Task<ResponseModel> ListAudit(ListAuditFilterDto listAuditFilterDto)
        {
            //Check Tồn tại phiếu giám sát:
            var checkExistInventory = await _inventoryContext.Inventories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == listAuditFilterDto.InventoryId);
            if (checkExistInventory == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy phiếu giám sát.",
                };
            }
            //Check đã tới ngày giám sát hay không:
            if (DateTime.Now.Date > checkExistInventory.InventoryDate.Date)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.NotYetInventoryDate,
                    Message = $"Không thể xác nhận kiểm kê vì đã quá ngày kiểm kê của đợt kiểm kê hiện tại. Vui lòng thử lại sau.",
                };

            }
            //check role xem có đang là giám sát hay không:
            var checkRoleType = await _inventoryContext.InventoryAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == listAuditFilterDto.AccountId && x.RoleType == InventoryAccountRoleType.Audit);
            if (checkRoleType == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = $"Không có quyền truy cập.",
                };
            }


            var accountLocation = from acc in _inventoryContext.InventoryAccounts.AsNoTracking()
                                  join accLoc in _inventoryContext.AccountLocations.AsNoTracking() on acc.Id equals accLoc.AccountId
                                  join loc in _inventoryContext.InventoryLocations.AsNoTracking() on accLoc.LocationId equals loc.Id
                                  where acc.UserId == listAuditFilterDto.AccountId && loc.IsDeleted == false
                                  select new
                                  {
                                      AssignedAccountId = acc.UserId,
                                      LocationName = loc.Name,
                                      loc.DepartmentName
                                  };


            var docs = from audit in _inventoryContext.AuditTargets.AsNoTracking()
                       join invDoc in _inventoryContext.InventoryDocs.AsNoTracking() on audit.InventoryId equals invDoc.InventoryId
                       join accLoc in accountLocation on audit.AssignedAccountId equals accLoc.AssignedAccountId
                       where invDoc.Status != InventoryDocStatus.NotReceiveYet && invDoc.Status != InventoryDocStatus.NoInventory
                       && (invDoc.DocType == InventoryDocType.A || invDoc.DocType == InventoryDocType.B || invDoc.DocType == InventoryDocType.E)
                       && audit.InventoryId == listAuditFilterDto.InventoryId
                       && audit.LocationName == accLoc.LocationName
                       && audit.ComponentCode == invDoc.ComponentCode
                       && audit.WareHouseLocation == invDoc.WareHouseLocation
                       && audit.Plant == invDoc.Plant
                       && audit.PositionCode == invDoc.PositionCode

                       select new AuditInfoModel
                       {
                           Id = invDoc.Id,
                           InventoryId = invDoc.InventoryId.Value,
                           AccountId = audit.AssignedAccountId,
                           Status = (int)invDoc.Status,
                           ComponentCode = audit.ComponentCode,
                           DepartmentName = audit.DepartmentName,
                           LocationName = audit.LocationName,
                           PositionCode = audit.PositionCode,
                       };

            //Tim theo phong ban:
            if (!listAuditFilterDto.DepartmentName.Contains("-1"))
            {
                docs = docs.Where(x => x.DepartmentName == listAuditFilterDto.DepartmentName);
            }

            //Tim theo khu vuc:
            if (!listAuditFilterDto.LocationName.Contains("-1"))
            {
                docs = docs.Where(x => x.LocationName == listAuditFilterDto.LocationName);
            }

            //Tim theo Ma linh kien:
            if (!listAuditFilterDto.ComponentCode.Contains("-1"))
            {
                docs = docs.Where(x => x.ComponentCode == listAuditFilterDto.ComponentCode);
            }

            if (docs?.Count() == 0)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            //Lấy ra trạng thái hoàn thành tổng thể: 5/10 => 5: số lượng có status: đã giám sát(status = 6)
            //                                            => 10: các trạng thái còn lại trừ (status = 0 || status = 1)

            var finishedDocsCount = docs.Where(x => x.Status == (int)InventoryDocStatus.AuditPassed || x.Status == (int)InventoryDocStatus.AuditFailed).Count();

            var totalDocsCount = docs.Where(x => x.Status != (int)InventoryDocStatus.NotReceiveYet && x.Status != (int)InventoryDocStatus.NoInventory).Count();


            ListAuditViewModel resultModel = new();
            resultModel.AuditInfoModels = docs.Where(x => !(x.Status == (int)InventoryDocStatus.NotReceiveYet || x.Status == (int)InventoryDocStatus.NoInventory)).OrderBy(x => x.PositionCode).ToList();
            resultModel.FinishCount = finishedDocsCount;
            resultModel.TotalCount = totalDocsCount;

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = resultModel,
                Message = "Danh sách phiếu giám sát.",
            };
        }

        public async Task<ResponseModel<IEnumerable<AuditInfoModel>>> ScanQR(Guid inventoryId, Guid accountId, string componentCode)
        {
            var currRoleType = _httpContext.UserFromContext()?.InventoryLoggedInfo?.InventoryRoleType;

            if (currRoleType != (int)InventoryAccountRoleType.Audit)
            {
                return new ResponseModel<IEnumerable<AuditInfoModel>>
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = commonAPIConstant.ResponseMessages.UnAuthorized
                };
            }

            var existComponentCode = await _inventoryContext.InventoryDocs.AsNoTracking()
                                                                    .AnyAsync(x => x.InventoryId == inventoryId
                                                                                && x.ComponentCode == componentCode);
            if (!existComponentCode)
            {
                return new ResponseModel<IEnumerable<AuditInfoModel>>
                {
                    Code = (int)HttpStatusCodes.InventoryNotFoundComponentCode,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.InventoryNotFoundComponentCode)
                };
            }


            var auditTargets = new List<Common.API.Dto.Inventory.AuditTargetViewModel>();

            var queryAuditTargets = await (from a in _inventoryContext.AuditTargets.Where(x => x.InventoryId == inventoryId).AsNoTracking()
                                           join d in _inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryId).AsNoTracking() on new { a.ComponentCode, a.WareHouseLocation, a.Plant, a.PositionCode }
                                           equals new { d.ComponentCode, d.WareHouseLocation, d.Plant, d.PositionCode } into T1group
                                           from ad in T1group.DefaultIfEmpty()
                                           let condition =
                                                   ExcludeDocStatus.Contains(ad.Status) == false &&
                                                   (ad.DocType == InventoryDocType.A || ad.DocType == InventoryDocType.B || ad.DocType == InventoryDocType.E) &&
                                                   a.InventoryId == inventoryId && a.ComponentCode == componentCode && a.AssignedAccountId == accountId

                                           where condition
                                           select new Common.API.Dto.Inventory.AuditTargetViewModel
                                           {
                                               Id = ad.Id,
                                               AccountId = ad.AssignedAccountId,
                                               InventoryId = a.InventoryId,
                                               ComponentCode = a != null ? a.ComponentCode : string.Empty,
                                               DepartmentName = a != null ? a.DepartmentName : string.Empty,
                                               LocationName = a != null ? a.LocationName : string.Empty,
                                               PositionCode = a != null ? a.PositionCode : string.Empty,
                                               Plant = a != null ? a.Plant : string.Empty,
                                               WHLoc = a != null ? a.WareHouseLocation : string.Empty,
                                               Status = a != null ? (int)ad.Status : default

                                           }).ToListAsync();

            if (queryAuditTargets.Any())
            {
                auditTargets = queryAuditTargets.ToList();

                //Phiếu đã thực hiện kiểm kê chưa
                var isInventoriedDocs = auditTargets.Where(x => x.Status >= (int)InventoryDocStatus.WaitingConfirm ||
                                                           x.Status >= (int)InventoryDocStatus.MustEdit);
                if (isInventoriedDocs == null || !isInventoriedDocs.Any())
                {
                    return new ResponseModel<IEnumerable<AuditInfoModel>>
                    {
                        Code = (int)HttpStatusCodes.ComponentNotInventoryYet,
                        Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentNotInventoryYet)
                    };
                }

                //Lấy ra các phiếu đã xác nhận kiểm kê
                var getConfirmedInventoryDocs = auditTargets.Where(x => x.Status >= (int)InventoryDocStatus.Confirmed)
                                                    .OrderBy(x => x.PositionCode)
                                                    .Select(x => new AuditInfoModel
                                                    {
                                                        Id = x.Id,
                                                        InventoryId = x.InventoryId,
                                                        AccountId = x.AccountId,
                                                        Status = x.Status,
                                                        ComponentCode = x.ComponentCode,
                                                        DepartmentName = x.DepartmentName,
                                                        LocationName = x.LocationName,
                                                        PositionCode = x.PositionCode
                                                    });

                if (getConfirmedInventoryDocs == null || !getConfirmedInventoryDocs.Any())
                {
                    return new ResponseModel<IEnumerable<AuditInfoModel>>
                    {
                        Code = (int)HttpStatusCodes.NotConfirmBeforeAudit,
                        Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.NotConfirmBeforeAudit)
                    };
                }

                return new ResponseModel<IEnumerable<AuditInfoModel>>
                {
                    Code = StatusCodes.Status200OK,
                    Data = getConfirmedInventoryDocs,
                };

            }

            auditTargets = await (from a in _inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryId).AsNoTracking()
                                  join d in _inventoryContext.AuditTargets.Where(x => x.InventoryId == inventoryId).AsNoTracking() on new { a.ComponentCode, a.WareHouseLocation, a.Plant, a.PositionCode }
                                  equals new { d.ComponentCode, d.WareHouseLocation, d.Plant, d.PositionCode } into T1group
                                  from ad in T1group.DefaultIfEmpty()
                                  let condition = (a.Status != InventoryDocStatus.NotReceiveYet && a.Status != InventoryDocStatus.NoInventory)
                                          && (a.DocType == InventoryDocType.A || a.DocType == InventoryDocType.B || a.DocType == InventoryDocType.E)
                                          && a.InventoryId == inventoryId && a.ComponentCode == componentCode


                                  where condition
                                  select new Common.API.Dto.Inventory.AuditTargetViewModel
                                  {
                                      Id = a.Id,
                                      AccountId = a.AssignedAccountId,
                                      InventoryId = a.InventoryId.Value,
                                      ComponentCode = a != null ? a.ComponentCode : string.Empty,
                                      DepartmentName = a != null ? a.DepartmentName : string.Empty,
                                      LocationName = a != null ? a.LocationName : string.Empty,
                                      PositionCode = a != null ? a.PositionCode : string.Empty,
                                      Plant = a != null ? a.Plant : string.Empty,
                                      WHLoc = a != null ? a.WareHouseLocation : string.Empty,
                                      Status = a != null ? (int)a.Status : default

                                  })
                                  // filter out BWINS docs
                                  .Where(x => !string.IsNullOrEmpty(x.PositionCode))
                                  .ToListAsync();


            if (auditTargets == null || !auditTargets.Any())
            {
                return new ResponseModel<IEnumerable<AuditInfoModel>>
                {
                    Code = (int)HttpStatusCodes.ComponentNotInAuditTarget,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentNotInAuditTarget)
                };
            }

            //Phiếu đã thực hiện kiểm kê chưa
            var inventoriedDocs = auditTargets.Where(x => x.Status >= (int)InventoryDocStatus.WaitingConfirm ||
                                                       x.Status >= (int)InventoryDocStatus.MustEdit);
            if (inventoriedDocs == null || !inventoriedDocs.Any())
            {
                return new ResponseModel<IEnumerable<AuditInfoModel>>
                {
                    Code = (int)HttpStatusCodes.ComponentNotInventoryYet,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentNotInventoryYet)
                };
            }

            //Lấy ra các phiếu đã xác nhận kiểm kê
            var confirmedInventoryDocs = auditTargets.Where(x => x.Status >= (int)InventoryDocStatus.Confirmed)
                                                .OrderBy(x => x.PositionCode)
                                                .Select(x => new AuditInfoModel
                                                {
                                                    Id = x.Id,
                                                    InventoryId = x.InventoryId,
                                                    AccountId = x.AccountId,
                                                    Status = x.Status,
                                                    ComponentCode = x.ComponentCode,
                                                    DepartmentName = x.DepartmentName,
                                                    LocationName = x.LocationName,
                                                    PositionCode = x.PositionCode
                                                });

            if (confirmedInventoryDocs == null || !confirmedInventoryDocs.Any())
            {
                return new ResponseModel<IEnumerable<AuditInfoModel>>
                {
                    Code = (int)HttpStatusCodes.NotConfirmBeforeAudit,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.NotConfirmBeforeAudit)
                };
            }

            return new ResponseModel<IEnumerable<AuditInfoModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = confirmedInventoryDocs,
            };
        }

        public async Task<ResponseModel> SubmitAudit(string inventoryId, string accountId, string docId, SubmitInventoryAction actionType, SubmitInventoryDto submitInventoryDto)
        {
            submitInventoryDto.UserCode = _httpContext.CurrentUser().UserCode ?? string.Empty;

            //Check Đợt kiểm kê xem có bị khóa hay không:
            var checkExistInventory = await _inventoryContext.Inventories.FirstOrDefaultAsync(x => x.Id.ToString().ToLower() == inventoryId.ToLower() && x.IsLocked != true);
            if (checkExistInventory == null)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.IsLockedInventory,
                    Message = $"Đợt kiểm kê đã bị khóa.",
                };
            }
            //Check đã tới ngày giám sát hay không:
            if (DateTime.Now.Date > checkExistInventory.InventoryDate.Date)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.NotYetInventoryDate,
                    Message = $"Không thể xác nhận kiểm kê vì đã quá ngày kiểm kê của đợt kiểm kê hiện tại. Vui lòng thử lại sau.",
                };

            }
            //check role xem có đang là giám sát hay không:
            var checkRoleType = await _inventoryContext.InventoryAccounts.FirstOrDefaultAsync(x => x.UserId.ToString().ToLower() == accountId.ToLower() && x.RoleType == InventoryAccountRoleType.Audit);
            if (checkRoleType == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = $"Không có quyền truy cập.",
                };
            }
            var getInventoryDoc = await _inventoryContext.InventoryDocs.FirstOrDefaultAsync(x => x.Id.ToString().ToLower() == docId.ToLower() && x.InventoryId.ToString().ToLower() == inventoryId.ToLower());
            //check tài khoản đã được assign vào phiểu giám sát hay chưa?
            //if (getInventoryDoc == null)
            //{
            //    return new ResponseModel
            //    {
            //        Code = (int)HttpStatusCodes.NotAssigneeAccountId,
            //        Message = $"Tài khoản chưa được assign vào phiếu giám sát.",
            //    };
            //}
            //Check trạng thái phiếu giám sát phải khác 0 hoặc 1:
            if (getInventoryDoc.Status == InventoryDocStatus.NotReceiveYet || getInventoryDoc.Status == InventoryDocStatus.NoInventory)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.InvalidStatusInventoryDoc,
                    Message = $"Trạng thái phiếu giám sát đang không đúng. Vui lòng thử lại.",
                };
            }

            //string InventoryName = checkExistInventory.Name;
            //string DocCode = getInventoryDoc.DocCode;
            var getDocHistoryId = Guid.Empty;

            //Lấy Status hiện tại trong InventoryDoc:
            var getCurrentStatus_InventoryDoc = getInventoryDoc.Status;
            var getUpdateStatus_InventoryDoc = InventoryDocStatus.Confirmed;
            var getCurrentQuantity_InventoryDoc = getInventoryDoc.Quantity;
            double updateQuantity_InventoryDoc = 0;

            //Cập nhật trạng thái, AuditAt, AuditBy
            //Logic cập nhật trạng thái như sau: Xác nhận đạt => trạng thái: 6 , Không đạt => trạng thái: 7
            if (actionType == SubmitInventoryAction.CancelAudit)
            {
                getInventoryDoc.Status = InventoryDocStatus.AuditFailed;
            }
            else if (actionType == SubmitInventoryAction.ConfirmAudit)
            {
                getInventoryDoc.Status = InventoryDocStatus.AuditPassed;
            }
            else if (actionType == SubmitInventoryAction.Update)
            {
                if (getInventoryDoc.Status == InventoryDocStatus.AuditFailed)
                {
                    getInventoryDoc.Status = InventoryDocStatus.AuditFailed;
                }
            }
            getInventoryDoc.AuditAt = DateTime.Now;
            getInventoryDoc.AuditBy = submitInventoryDto.UserCode;
            getUpdateStatus_InventoryDoc = getInventoryDoc.Status;

            //Cập nhật Status trong AuditTargets:
            var department_InventoryDoc = getInventoryDoc.DepartmentName;
            var location_InventoryDoc = getInventoryDoc.LocationName;
            var component_InventoryDoc = getInventoryDoc.ComponentCode;

            var getAuditTarget = await _inventoryContext.AuditTargets.FirstOrDefaultAsync(x => x.InventoryId.ToString().ToLower() == inventoryId.ToLower()
                                       //&& x.AssignedAccountId.ToString().ToLower() == accountId.ToLower() && x.DepartmentName == department_InventoryDoc
                                       && x.LocationName == location_InventoryDoc && x.ComponentCode == component_InventoryDoc
                                       && x.PositionCode == getInventoryDoc.PositionCode);
            if ((actionType == SubmitInventoryAction.CancelAudit || actionType == SubmitInventoryAction.Update) && getAuditTarget != null)
            {
                getAuditTarget.Status = AuditTargetStatus.Fail;
                getAuditTarget.UpdatedAt = DateTime.Now;
                getAuditTarget.UpdatedBy = submitInventoryDto.UserCode;

            }
            else if (actionType == SubmitInventoryAction.ConfirmAudit && getAuditTarget != null)
            {
                var checkStatusInventoryDocs = await _inventoryContext.InventoryDocs.FirstOrDefaultAsync(x => x.InventoryId.ToString().ToLower() == inventoryId.ToLower()
                                                     //&& x.AssignedAccountId.ToString().ToLower() == accountId.ToLower() && x.DepartmentName == department_InventoryDoc
                                                     && x.LocationName == location_InventoryDoc && x.ComponentCode == component_InventoryDoc
                                                     && x.PositionCode == getInventoryDoc.PositionCode
                                                     && x.Status == InventoryDocStatus.AuditFailed);
                if (checkStatusInventoryDocs == null)
                {
                    getAuditTarget.Status = AuditTargetStatus.Pass;
                    getAuditTarget.UpdatedAt = DateTime.Now;
                    getAuditTarget.UpdatedBy = submitInventoryDto.UserCode;
                }
                else
                {
                    getAuditTarget.Status = AuditTargetStatus.Fail;
                    getAuditTarget.UpdatedAt = DateTime.Now;
                    getAuditTarget.UpdatedBy = submitInventoryDto.UserCode;
                }
            }

            //Lưu DocOutput, DocHistory, HistoryOutput:
            //Check DocOutput: Nếu chưa có dữ liệu thì thêm mới, có dữ liệu rồi thì cập nhật lại:
            var getDocOutput = await _inventoryContext.DocOutputs.Where(x => x.InventoryDocId.ToString().ToLower() == docId.ToLower()
                                      && x.InventoryId.ToString().ToLower() == inventoryId.ToLower()).ToListAsync();

            if (getDocOutput?.Count() > 0)
            {
                //Nếu mobile truyền những Ids muốn xóa thì Xóa những danh sách Ids trong DocOutput và HistoryOutput:
                var getIdsDeleteDocOutPut = await _inventoryContext.DocOutputs.Where(x => submitInventoryDto.IdsDeleteDocOutPut.Contains(x.Id)).ToListAsync();
                //var getIdsDeleteHistoryOutput = await _inventoryContext.HistoryOutputs.Where(x => submitInventoryDto.IdsDeleteDocOutPut.Contains(x.Id)).ToListAsync();
                _inventoryContext.DocOutputs.RemoveRange(getIdsDeleteDocOutPut);
                //_inventoryContext.HistoryOutputs.RemoveRange(getIdsDeleteHistoryOutput);

                //Cập nhật DocOutput:
                if (submitInventoryDto.DocOutputs.Any())
                {
                    //case id null:
                    double sumIdsNull = 0, sumIdsNotNull = 0;

                    List<DocOutput> docs = new();
                    var insertIds = submitInventoryDto.DocOutputs.Where(x => x.Id == null).ToList();
                    if (insertIds?.Count() > 0)
                    {
                        foreach (var insertItem in insertIds)
                        {
                            docs.Add(new DocOutput
                            {
                                Id = Guid.NewGuid(),
                                InventoryDocId = Guid.Parse(docId),
                                InventoryId = Guid.Parse(inventoryId),
                                QuantityOfBom = insertItem.QuantityOfBom,
                                QuantityPerBom = insertItem.QuantityPerBom,
                                CreatedAt = DateTime.Now,
                                CreatedBy = submitInventoryDto.UserCode
                            });
                        }
                        //Sum của những Ids null:
                        sumIdsNull = insertIds.Sum(x => x.QuantityOfBom * x.QuantityPerBom);
                        _inventoryContext.DocOutputs.AddRange(docs);
                    }

                    //case id != null:
                    var updateIds = submitInventoryDto.DocOutputs.Where(x => x.Id != null);

                    var getIdsNotNull = submitInventoryDto.DocOutputs.Where(x => x.Id != null).Select(x => x.Id).ToList();

                    var getDocOutputWithIdsNotNull = await _inventoryContext.DocOutputs.Where(x => getIdsNotNull.Contains(x.Id)).ToListAsync();
                    if (getDocOutputWithIdsNotNull?.Count() > 0)
                    {
                        foreach (var editModel in updateIds)
                        {
                            var updateEntity = getDocOutputWithIdsNotNull.FirstOrDefault(x => x.Id == editModel.Id);
                            updateEntity.QuantityOfBom = editModel.QuantityOfBom;
                            updateEntity.QuantityPerBom = editModel.QuantityPerBom;
                            updateEntity.UpdatedAt = DateTime.Now;
                            updateEntity.UpdatedBy = submitInventoryDto.UserCode;
                        }
                        sumIdsNotNull = getDocOutputWithIdsNotNull.Sum(x => x.QuantityOfBom * x.QuantityPerBom);
                    }

                    updateQuantity_InventoryDoc = sumIdsNull + sumIdsNotNull;
                    getInventoryDoc.Quantity = updateQuantity_InventoryDoc;

                }

                //Cập nhật DocHistoryDto, đã có DocOutput và docHistory logic changelog:

                double oldQuantity_his, newQuantity_his;
                if (getCurrentQuantity_InventoryDoc == updateQuantity_InventoryDoc)
                {
                    oldQuantity_his = getCurrentQuantity_InventoryDoc;
                    newQuantity_his = getCurrentQuantity_InventoryDoc;
                }
                else
                {
                    oldQuantity_his = getCurrentQuantity_InventoryDoc;
                    newQuantity_his = updateQuantity_InventoryDoc;
                }

                //Lưu Lịch sử DocHistory:
                var newDocHistory = new DocHistory
                {
                    Id = Guid.NewGuid(),
                    InventoryId = Guid.Parse(inventoryId),
                    InventoryDocId = Guid.Parse(docId),
                    Action = DocHistoryActionType.Audit,
                    CreatedAt = DateTime.Now,
                    CreatedBy = submitInventoryDto.UserCode,
                    OldQuantity = oldQuantity_his,
                    NewQuantity = newQuantity_his,
                    OldStatus = getCurrentStatus_InventoryDoc,
                    NewStatus = getUpdateStatus_InventoryDoc,
                    Status = getUpdateStatus_InventoryDoc,
                    IsChangeCDetail = submitInventoryDto.DocTypeCDetails.Count() == 0 ? false : true,
                    Comment = submitInventoryDto.Comment,
                };

                _inventoryContext.DocHistories.Add(newDocHistory);

                getDocHistoryId = newDocHistory.Id;
                //Lưu HistoryOutput:
                if (submitInventoryDto.DocOutputs.Any())
                {
                    List<HistoryOutput> hisOuts = new();
                    foreach (var item in submitInventoryDto.DocOutputs)
                    {
                        hisOuts.Add(new HistoryOutput
                        {
                            Id = Guid.NewGuid(),
                            DocHistoryId = getDocHistoryId,
                            InventoryId = Guid.Parse(inventoryId),
                            QuantityOfBom = item.QuantityOfBom,
                            QuantityPerBom = item.QuantityPerBom,
                            CreatedAt = DateTime.Now,
                            CreatedBy = submitInventoryDto.UserCode
                        });
                    }
                    _inventoryContext.HistoryOutputs.AddRange(hisOuts);
                }
            }

            await _inventoryContext.SaveChangesAsync();

            //Call Backgroud Job để tổng hợp lại số lượng:
            var updateDocTotalParam = new InventoryDocSubmitDto();
            updateDocTotalParam.InventoryId = Guid.Parse(inventoryId);
            updateDocTotalParam.InventoryDocIds.Add(Guid.Parse(docId));
            updateDocTotalParam.DocType = getInventoryDoc.DocType;
            await _dataAggregationService.UpdateDataFromInventoryDoc(updateDocTotalParam);

            if (actionType == SubmitInventoryAction.CancelAudit)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Không đạt giám sát kiểm kê linh kiện.",
                    Data = new
                    {
                        Status = getUpdateStatus_InventoryDoc,
                        InventoryId = inventoryId,
                        AccountId = accountId,
                        DocId = docId,
                    }
                };
            }
            else if (actionType == SubmitInventoryAction.Update)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Đã cập nhật chi tiết phiếu.",
                    Data = new
                    {
                        Status = getUpdateStatus_InventoryDoc,
                        InventoryId = inventoryId,
                        AccountId = accountId,
                        DocId = docId,
                    }
                };
            }
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Đã đạt giám sát kiểm kê linh kiện.",
                Data = new
                {
                    Status = getUpdateStatus_InventoryDoc,
                    InventoryId = inventoryId,
                    AccountId = accountId,
                    DocId = docId,
                }
            };

        }


        private ValidateCellTypeCDto ValidateSheets(IEnumerable<GetAllRoleWithUserNameModel> rolesModel, Guid inventoryId, ExcelWorksheets worksheets, List<InventoryDocTypeCSheetDto> dataFromSheets, InventoryDocAndUserDto assignees, bool isBypassWarning = false)
        {
            ValidateCellTypeCDto validateCellTypeCDto = new();
            //general model code regex
            var modelCodeRegex = ModelCodeRegex();
            var finishGrpRegex = FinishGroupRegex();
            var shareGrpRegex = ShareGroupRegex();
            var assenblyAndfinishGrpRegex = AssenblyAndFinishGroupRegex();
            var mainLineGrpRegex = MainLineGroupRegex();
            var shareGrpByModelRegex = ShareGroupByModelRegex();
            var materialCodeRegex = MaterialCodeRegex();
            var machineModelRegex = MachineModelRegex();

            //get inventory document type A of this inventory
            var docTypeAs = _inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.InventoryId == inventoryId && x.DocType == InventoryDocType.A)
                                                           .Select(x => $"{x.ComponentCode}{x.WareHouseLocation}{x.Plant}").ToArray().AsSpan();

            var docTypeCs = (from invDoc in _inventoryContext.InventoryDocs.AsNoTracking()
                             join docCDetail in _inventoryContext.DocTypeCDetails.AsNoTracking() on invDoc.Id equals docCDetail.InventoryDocId into leftJoin
                             from docCDetail in leftJoin.DefaultIfEmpty()
                             where invDoc.InventoryId == inventoryId && invDoc.DocType == InventoryDocType.C
                             select new
                             {
                                 docCDetail.ComponentCode,
                                 invDoc.ModelCode,
                                 invDoc.InventoryId
                             }).ToList();



            //var docTypeCs = _inventoryContext.InventoryDocs.AsNoTracking().Include(x => x.DocTypeCDetails).AsNoTracking().Where(x => x.InventoryId == inventoryId && x.DocType == InventoryDocType.C)
            //                                                .SelectMany(x => x.DocTypeCDetails.Select(x => new
            //                                                {
            //                                                    x.ComponentCode,
            //                                                    x.ModelCode,
            //                                                    x.InventoryId
            //                                                })
            //                                                ).Where(x => x.InventoryId == inventoryId).ToList();


            //validate for warning
            //existed Model code in db
            if (!isBypassWarning)
            {
                var currUser = _httpContext.UserFromContext();

                //Check Current User has Role: Edit DocType C
                var isEditDocTypeCRole = rolesModel.Any(x => x.UserName == currUser.Username && x.ClaimType == Constants.Roles.EDIT_DOCUMENT_TYPE_C);
                validateCellTypeCDto.IsEditDocTypeCRole = isEditDocTypeCRole;

                var modelCodeFromSheets = dataFromSheets.SelectMany(x => x.Rows.Select(x => new
                {
                    x.ModelCode,
                    x.SheetName
                })).DistinctBy(x => x.ModelCode).ToList();
                var existedModelCode = modelCodeFromSheets.Where(x => docTypeCs.Select(c => c.ModelCode).Contains(x.ModelCode));
                if (existedModelCode.Any())
                {
                    validateCellTypeCDto.IsWarning = true;
                }
                foreach (var item in existedModelCode)
                {
                    var sheet = worksheets.FirstOrDefault(x => x.Name == item.SheetName);
                    sheet.Cells[2, 9].Value += $"Model code {item.ModelCode} đã tồn tại trên hệ thống. ";
                    sheet.TabColor = System.Drawing.Color.Yellow;
                }

                //var materialWithBOM = dataFromSheets.SelectMany(x => x.Rows.Select(x => new
                //{
                //    x.MaterialCode,
                //    x.BOMUseQty,
                //    x.SheetName
                //})).Where(x => finishGrpRegex.IsMatch(x.MaterialCode) && x.BOMUseQty != "1");

                //if (materialWithBOM.Any())
                //{
                //    validateCellTypeCDto.IsWarning = true;
                //}
                //foreach (var item in materialWithBOM)
                //{
                //    var sheet = worksheets.FirstOrDefault(x => x.Name == item.SheetName);
                //    sheet.Cells[2, 9].Value += $"Cụm đính kèm có số BOM > 1. ";
                //    sheet.TabColor = System.Drawing.Color.Yellow;
                //}

                var materialDuplicate = dataFromSheets.SelectMany(x => x.Rows.Select(x => new
                {
                    x.MaterialCode,
                    x.SheetName
                })).Where(x => materialCodeRegex.IsMatch(x.MaterialCode)).GroupBy(x => new { x.MaterialCode, x.SheetName }).Where(x => x.Count() > 1);
                if (materialDuplicate != null)
                {
                    if (materialDuplicate.SelectMany(x => x.Select(m => m)).Count() > 0)
                    {
                        validateCellTypeCDto.IsWarning = true;
                    }
                    foreach (var item in materialDuplicate.Select(x => x.Key.SheetName))
                    {
                        var sheet = worksheets.FirstOrDefault(x => x.Name == item);
                        sheet.Cells[2, 9].Value += $"Cụm có mã linh kiện trùng nhau. ";
                        sheet.TabColor = System.Drawing.Color.Yellow;
                    }

                }
                if (validateCellTypeCDto.IsWarning)
                {
                    validateCellTypeCDto.IsValid = false;
                    validateCellTypeCDto.Title = "Cảnh báo dữ liệu phiếu C";
                    validateCellTypeCDto.Content = "File import có một số dữ liệu cần bạn kiểm tra. Bạn có muốn tiếp tục thực hiện việc import dữ liệu?";
                    return validateCellTypeCDto;
                }

            }


            //Validate for errors
            var allModelCodes = dataFromSheets.SelectMany(x => x.Rows).Where(x => !string.IsNullOrEmpty(x.ModelCode)).Select(x => new { x.SheetName, x.ModelCode, x.MaterialCode, x.RowNumber }).ToList();

            //check for duplicate model code
            var isDuplicate = dataFromSheets.SelectMany(x => x.Rows).Where(x => !string.IsNullOrEmpty(x.ModelCode)).Select(x => new { x.SheetName, x.ModelCode, x.RowNumber }).GroupBy(x => x.ModelCode).Where(x => x.Select(s => s.SheetName).Distinct().Count() > 1);

            if (isDuplicate.Any())
            {

                foreach (var item in isDuplicate)
                {
                    var listRowWithDuplicate = allModelCodes.Where(x => x.ModelCode == item.Key).ToList();
                    foreach (var row in listRowWithDuplicate)
                    {
                        var sheet = worksheets.FirstOrDefault(x => x.Name == row.SheetName);
                        validateCellTypeCDto.IsValid = false;
                        sheet.Cells[row.RowNumber, 9].Value += $"Trùng lặp Model code {item.Key} giữa các sheet trong file. ";
                        sheet.TabColor = System.Drawing.Color.Red;
                    }

                }

            }

            if (allModelCodes.Any(x => MainLineGroupRegex().IsMatch(x.ModelCode)))
            {
                var groupedAllCode = allModelCodes.GroupBy(x => MainLineGroupRegex().Match(x.ModelCode).Groups[3].Value).Where(x => !string.IsNullOrEmpty(x.Key));

                foreach (var item in groupedAllCode)
                {
                    var maxLine = item.Where(x => MainLineGroupRegex().IsMatch(x.ModelCode)).DistinctBy(x => x.ModelCode).Max(x => Convert.ToInt32(MainLineGroupRegex().Match(x.ModelCode).Groups[5].Value));

                    var finishGrp = item.Where(x => !string.IsNullOrEmpty(x.ModelCode) && FinishGroupRegex().IsMatch(x.ModelCode) && MainLineGroupRegex().IsMatch(x.MaterialCode)).OrderBy(x => x.ModelCode).LastOrDefault();

                    if (finishGrp != null)
                    {
                        if (!finishGrp.ModelCode.Contains("e002") && (Convert.ToInt32(MainLineGroupRegex().Match(finishGrp.MaterialCode).Groups[5].Value) != maxLine))
                        {
                            var sheet = worksheets.FirstOrDefault(x => x.Name == finishGrp.SheetName);
                            validateCellTypeCDto.IsValid = false;
                            sheet.Cells[finishGrp.RowNumber, 9].Value += "Thiếu cụm hontai cuối. ";
                            sheet.TabColor = System.Drawing.Color.Red;
                        }

                    }
                }

            }

            foreach (var sheetItem in dataFromSheets.Where(x => x.Rows.Any()))
            {
                var sheet = worksheets.FirstOrDefault(x => x.Name == sheetItem.SheetName);

                //check model code unique in sheet
                var modelCodes = sheetItem.Rows.Where(x => !string.IsNullOrEmpty(x.ModelCode)).Select(x => new { x.ModelCode, x.RowNumber }).DistinctBy(x => x.ModelCode).ToList();

                //check for required columns
                var plantReq = sheetItem.Rows.Where(x => string.IsNullOrEmpty(x.Plant)).Select(x => new { x.Plant, x.RowNumber }).ToList();
                var warehouseReq = sheetItem.Rows.Where(x => string.IsNullOrEmpty(x.WareHouseLocation)).Select(x => new { x.WareHouseLocation, x.RowNumber }).ToList();
                var materialReq = sheetItem.Rows.Where(x => string.IsNullOrEmpty(x.MaterialCode)).Select(x => new { x.MaterialCode, x.RowNumber }).ToList();
                var bomUseQtyReq = sheetItem.Rows.Where(x => string.IsNullOrEmpty(x.BOMUseQty)).Select(x => new { x.BOMUseQty, x.RowNumber }).ToList();
                var stageNameReq = sheetItem.Rows.Where(x => string.IsNullOrEmpty(x.StageName)).Select(x => new { x.StageName, x.RowNumber }).ToList();
                var assigneeReq = sheetItem.Rows.Where(x => string.IsNullOrEmpty(x.Assignee)).Select(x => new { x.Assignee, x.RowNumber }).ToList();

                //check child group code unique in sheet
                var childGrpModelCodes = sheetItem.Rows.Where(x => !string.IsNullOrEmpty(x.MaterialCode) && mainLineGrpRegex.IsMatch(x.MaterialCode)).Select(x => new { x.MaterialCode, x.RowNumber, LineStage = Convert.ToInt32(mainLineGrpRegex.Match(x.MaterialCode).Groups[5].Value) }).ToList();
                if (childGrpModelCodes.Count > 1 && childGrpModelCodes.DistinctBy(x => x.MaterialCode).Count() > 1)
                {
                    var maxLine = childGrpModelCodes.Max(x => x.LineStage);
                    foreach (var item in childGrpModelCodes.Where(x => x.LineStage != maxLine))
                    {
                        validateCellTypeCDto.IsValid = false;
                        sheet.Cells[item.RowNumber, 9].Value += "Thừa cụm đính kèm. ";
                        sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.MaterialCode)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                        sheet.TabColor = System.Drawing.Color.Red;
                    }

                }
                else if (childGrpModelCodes.Count > 1 && childGrpModelCodes.DistinctBy(x => x.MaterialCode).Count() == 1)
                {
                    foreach (var item in childGrpModelCodes)
                    {
                        validateCellTypeCDto.IsValid = false;
                        sheet.Cells[item.RowNumber, 9].Value += "Trùng lặp cụm đính kèm. ";
                        sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.MaterialCode)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                        sheet.TabColor = System.Drawing.Color.Red;
                    }
                }

                if (plantReq.Count > 0 || warehouseReq.Count > 0 || materialReq.Count > 0 || bomUseQtyReq.Count > 0 || stageNameReq.Count > 0 || assigneeReq.Count > 0 || (modelCodes.Count > 1 && modelCodes.Any(x => string.IsNullOrEmpty(x.ModelCode)) || modelCodes.Count == 0))
                {
                    validateCellTypeCDto.IsValid = false;
                    validateCellTypeCDto.Title = "File sai định dạng";
                    validateCellTypeCDto.Content = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo phiếu kiểm kê.";
                    return validateCellTypeCDto;
                }


                if (modelCodes.Count > 1)
                {
                    foreach (var item in modelCodes)
                    {
                        validateCellTypeCDto.IsValid = false;
                        sheet.Cells[item.RowNumber, 9].Value += "Không thể có nhiều  Model code trong cùng 1 sheet. ";
                        sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.ModelCode)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                        sheet.TabColor = System.Drawing.Color.Red;
                    }

                }
                else
                {
                    sheetItem.ModelCode = modelCodes[0].ModelCode;
                    var materialCodes = sheetItem.Rows.Select(x => new { x.MaterialCode, x.RowNumber }).ToList();
                    var hasModelCode = materialCodes.Any(x => modelCodeRegex.IsMatch(x.MaterialCode));
                    if (!sheetItem.ModelCode.Contains("001") && !hasModelCode && mainLineGrpRegex.IsMatch(sheetItem.ModelCode))
                    {
                        validateCellTypeCDto.IsValid = false;
                        sheet.Cells[2, 9].Value += "Thiếu cụm đính kèm. ";
                        sheet.TabColor = System.Drawing.Color.Red;
                    }

                    //check for first group
                    //var firstGrp = _inventoryContext.InventoryDocs.Any(x => x.DocType == InventoryDocType.C && x.InventoryId == inventoryId && x.ModelCode.Contains("001"));
                    var firstGrpModelCode = materialCodes.Any(x => modelCodeRegex.IsMatch(x.MaterialCode) && x.MaterialCode.Contains("001"));
                    if (sheetItem.ModelCode.Contains("002") && (mainLineGrpRegex.IsMatch(sheetItem.ModelCode) || finishGrpRegex.IsMatch(sheetItem.ModelCode)) && !firstGrpModelCode)
                    {

                        validateCellTypeCDto.IsValid = false;
                        sheet.Cells[2, 9].Value += "Thiếu cụm hontai đầu. ";
                        sheet.TabColor = System.Drawing.Color.Red;
                    }

                    if (finishGrpRegex.IsMatch(sheetItem.ModelCode) && finishGrpRegex.Match(sheetItem.ModelCode).Groups[5].Value != "001" && finishGrpRegex.Match(sheetItem.ModelCode).Groups[5].Value != "002")
                    {
                        foreach (var item in modelCodes)
                        {
                            validateCellTypeCDto.IsValid = false;
                            sheet.Cells[item.RowNumber, 9].Value += "Cụm hontai cuối sai định dạng. ";
                            sheet.TabColor = System.Drawing.Color.Red;
                        }

                    }


                    var finishGrpInMaterialCode = materialCodes.Where(x => finishGrpRegex.IsMatch(x.MaterialCode));

                    if (finishGrpRegex.IsMatch(sheetItem.ModelCode) && finishGrpInMaterialCode.Any())
                    {


                        foreach (var item in finishGrpInMaterialCode)
                        {
                            if (finishGrpRegex.Match(sheetItem.ModelCode).Groups[3].Value != finishGrpRegex.Match(item.MaterialCode).Groups[3].Value)
                            {

                                validateCellTypeCDto.IsValid = false;
                                sheet.Cells[item.RowNumber, 9].Value += "Cụm hontai đính kèm không đúng. ";
                                sheet.TabColor = System.Drawing.Color.Red;
                            }
                        }

                    }


                    //get max line number
                    if (finishGrpRegex.IsMatch(sheetItem.ModelCode) && !hasModelCode)
                    {
                        validateCellTypeCDto.IsValid = false;
                        sheet.Cells[2, 9].Value += "Thiếu cụm hontai cuối. ";
                        sheet.TabColor = System.Drawing.Color.Red;
                    }


                }

                //check plant unique in sheet
                var plants = sheetItem.Rows.Where(x => !string.IsNullOrEmpty(x.Plant)).Select(x => new { x.Plant, x.RowNumber }).DistinctBy(x => x.Plant).ToList();
                if (plants.Count > 1)
                {
                    foreach (var item in plants)
                    {
                        validateCellTypeCDto.IsValid = false;
                        sheet.Cells[item.RowNumber, 9].Value += "Không thể có nhiều Plant trong cùng 1 sheet. ";
                        sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.Plant)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                        sheet.TabColor = System.Drawing.Color.Red;
                    }
                }
                else
                {
                    sheetItem.Plant = plants[0].Plant;
                }

                //check assignee unique in sheet
                var assign = sheetItem.Rows.Where(x => !string.IsNullOrEmpty(x.Assignee)).Select(x => new { x.Assignee, x.RowNumber }).DistinctBy(x => x.Assignee).ToList();
                if (assign.Count > 1)
                {
                    foreach (var item in assign)
                    {
                        validateCellTypeCDto.IsValid = false;
                        sheet.Cells[item.RowNumber, 9].Value += "Không thể có nhiều tài khoản thao tác trong cùng 1 sheet. ";
                        sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.Assignee)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                        sheet.TabColor = System.Drawing.Color.Red;
                    }

                }
                else
                {
                    //check exist assignee
                    if (assign.All(x => !string.IsNullOrEmpty(x.Assignee)))
                    {
                        var existedAssignee = assignees.Assignees.Any(a => sheetItem.Rows.Select(x => x.Assignee).Contains(a.UserName));
                        if (!existedAssignee)
                        {
                            validateCellTypeCDto.IsValid = false;
                            sheet.Cells[2, 9].Value += "Tài khoản phân phát không tồn tại. ";
                            sheet.Cells[2, sheet.GetColumnIndex(TypeC.Assignee)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                            sheet.TabColor = System.Drawing.Color.Red;
                        }
                    }

                }

                //check null stage name
                var stageName = sheetItem.Rows.Select(x => new { x.StageName, x.RowNumber }).DistinctBy(x => x.StageName).ToList();
                //check null bom 
                var bom = sheetItem.Rows.Select(x => x.BOMUseQty).Distinct().ToList();

                //check null material 
                var material = sheetItem.Rows.Select(x => x.MaterialCode).Distinct().ToList();

                //check null model code
                var model = sheetItem.Rows.Select(x => new { x.ModelCode, x.RowNumber }).Distinct().ToList();
                var whLoc = sheetItem.Rows.Select(x => x.WareHouseLocation).Distinct().ToList();
                var plant = sheetItem.Rows.Select(x => x.Plant).Distinct().ToList();
                var no = sheetItem.Rows.Select(x => x.No).Distinct().ToList();

                if (no.All(string.IsNullOrEmpty) || plant.All(string.IsNullOrEmpty) || whLoc.All(string.IsNullOrEmpty) || model.All(x => string.IsNullOrEmpty(x.ModelCode)) || material.All(string.IsNullOrEmpty) || bom.All(string.IsNullOrEmpty) || stageName.All(x => string.IsNullOrEmpty(x.StageName)) || assign.All(x => string.IsNullOrEmpty(x.Assignee)))
                {
                    validateCellTypeCDto.IsValid = false;
                    validateCellTypeCDto.HasSpecificMessage = true;
                    validateCellTypeCDto.Title = "File sai định dạng";
                    validateCellTypeCDto.Content = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo phiếu kiểm kê.";

                }

                if (stageName.Count() > 1)
                {
                    validateCellTypeCDto.IsValid = false;
                    foreach (var item in stageName)
                    {

                        sheet.Cells[item.RowNumber, 9].Value += "Không thể có nhiều tên công đoạn trong cùng 1 sheet. ";
                        sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.Assignee)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);

                    }
                    sheet.TabColor = System.Drawing.Color.Red;
                }

                foreach (var item in sheetItem.Rows)
                {
                    //check model code - normal case
                    if (item.ModelCode.Length != 10)
                    {
                        validateCellTypeCDto.IsValid = false;
                        sheet.Cells[item.RowNumber, 9].Value += "Model code phải đủ 10 ký tự (số và chữ). ";
                        sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.ModelCode)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                        sheet.TabColor = System.Drawing.Color.Red;
                    }
                    else if (!modelCodeRegex.IsMatch(item.ModelCode))
                    {
                        if (model.Any(x => !machineModelRegex.IsMatch(x.ModelCode)))
                        {
                            validateCellTypeCDto.IsValid = false;
                            sheet.TabColor = System.Drawing.Color.Red;

                            sheet.Cells[item.RowNumber, 9].Value += "Model máy phải đủ 04 ký tự (số và chữ). ";

                        }
                        else
                        {
                            validateCellTypeCDto.IsValid = false;
                            sheet.Cells[item.RowNumber, 9].Value += "Cụm không đúng quy tắc thiết lập mã. ";
                            sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.ModelCode)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                            sheet.TabColor = System.Drawing.Color.Red;
                        }
                    }

                    //check material code
                    if ((item.MaterialCode.Length <= 9 || item.MaterialCode.Length > 10) && !materialCodeRegex.IsMatch(item.MaterialCode))
                    {
                        validateCellTypeCDto.IsValid = false;
                        sheet.Cells[item.RowNumber, 9].Value += "Mã linh kiện không đúng định dạng. ";
                        sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.MaterialCode)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                        sheet.TabColor = System.Drawing.Color.Red;
                    }
                    else if (item.MaterialCode.Length == 10)
                    {
                        var existedModelCodeOtherSheet = dataFromSheets.SelectMany(x => x.Rows).Any(x => x.SheetName != item.SheetName && x.ModelCode == item.MaterialCode);
                        var existedInDb = docTypeCs.Any(x => x.ModelCode == item.MaterialCode);
                        if (!existedInDb && !existedModelCodeOtherSheet)
                        {
                            validateCellTypeCDto.IsValid = false;
                            sheet.Cells[item.RowNumber, 9].Value += "Cụm đính kèm chưa tồn tại. ";
                            sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.MaterialCode)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                            sheet.TabColor = System.Drawing.Color.Red;

                        }
                        var first7DigitsMaterialCode = item.MaterialCode.Substring(0, 7);
                        var first7DigitsModelCode = item.ModelCode.Substring(0, 7);

                        var last3DigitsMaterialCode = item.MaterialCode.Substring(7, 3);
                        var last3DigitsModelCode = item.ModelCode.Substring(7, 3);

                        if (first7DigitsMaterialCode == first7DigitsModelCode)
                        {
                            if (int.Parse(last3DigitsMaterialCode) > int.Parse(last3DigitsModelCode))
                            {
                                validateCellTypeCDto.IsValid = false;
                                sheet.Cells[item.RowNumber, 9].Value += "Cụm đính kèm không được có số thứ tự công đoạn lớn hơn cụm cha. ";
                                sheet.Cells[item.RowNumber, sheet.GetColumnIndex(ImportExcelColumns.TypeC.MaterialCode)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                                sheet.TabColor = System.Drawing.Color.Red;
                            }
                        }

                        //if (modelCodeRegex.IsMatch(item.MaterialCode))
                        //{
                        //    var materialCodeFirst5Chars = $"{modelCodeRegex.Match(item.MaterialCode).Groups[1].Value}{modelCodeRegex.Match(item.MaterialCode).Groups[2].Value}";
                        //    var modelCodeFirst5Chars = $"{modelCodeRegex.Match(item.ModelCode).Groups[1].Value}{modelCodeRegex.Match(item.ModelCode).Groups[2].Value}";
                        //    if (materialCodeFirst5Chars != modelCodeFirst5Chars)
                        //    {
                        //        validateCellTypeCDto.IsValid = false;
                        //        sheet.Cells[item.RowNumber, 9].Value += "Cụm đính kèm không đúng. ";
                        //        sheet.Cells[item.RowNumber, sheet.GetColumnIndex(ImportExcelColumns.TypeC.MaterialCode)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                        //        sheet.TabColor = System.Drawing.Color.Red;
                        //    }
                        //}
                    }

                    //check plant must be L401 or L404
                    if (item.Plant != Constants.ValidationRules.Plant.L401 && item.Plant != Constants.ValidationRules.Plant.L404)
                    {
                        validateCellTypeCDto.IsValid = false;
                        sheet.Cells[item.RowNumber, 9].Value += "Dữ liệu Plant không đúng, giá trị đúng là L401 hoặc L404. ";
                        sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.ModelCode)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                        sheet.TabColor = System.Drawing.Color.Red;
                    }

                    //check warehouse location must be S001 or S002
                    if (item.WareHouseLocation != "S001" && item.WareHouseLocation != "S002")
                    {
                        validateCellTypeCDto.IsValid = false;
                        sheet.Cells[item.RowNumber, 9].Value += "Dữ liệu WH Loc.  không đúng, giá trị đúng là S001 hoặc S002. ";
                        sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.WarehouseLocation)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                        sheet.TabColor = System.Drawing.Color.Red;
                    }

                    var materialWarehousePlant = $"{item.MaterialCode}{item.WareHouseLocation}{item.Plant}";
                    if (materialCodeRegex.IsMatch(item.MaterialCode) && !docTypeAs.Contains(materialWarehousePlant))
                    {
                        validateCellTypeCDto.IsValid = false;
                        sheet.Cells[item.RowNumber, 9].Value += $"Thông tin mã linh kiện {item.MaterialCode} có plant {item.Plant} và WH Loc. {item.WareHouseLocation} không tồn tại trong phiếu A. ";
                        sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.WarehouseLocation)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                        sheet.TabColor = System.Drawing.Color.Red;
                    }
                }

            }




            var shareGrpData = dataFromSheets.SelectMany(x => x.Rows).Where(x => shareGrpRegex.IsMatch(x.ModelCode) || shareGrpRegex.IsMatch(x.MaterialCode))

                .OrderByDescending(x => x.ModelCode).ToList();

            //---- Share group by machine type ----//
            //get share group by machine
            //get model code - material code pair
            var shareGrpPair = shareGrpData.Where(x => !string.IsNullOrEmpty(x.ModelCode) && shareGrpRegex.IsMatch(x.MaterialCode) && x.ModelCode.Contains(shareGrpRegex.Match(x.MaterialCode).Groups[1].Value)).Select(x =>
            {
                _ = int.TryParse(mainLineGrpRegex.Match(x.ModelCode).Groups[5].Value, out var mainStageNumber);
                _ = int.TryParse(shareGrpRegex.Match(x.MaterialCode).Groups[5].Value, out var attachStageNumber);

                if (mainLineGrpRegex.IsMatch(x.MaterialCode) && attachStageNumber > mainStageNumber)
                    return x;
                else
                    return null;
            });
            var assblAndFinGrpData = dataFromSheets.SelectMany(x => x.Rows).Where(x => assenblyAndfinishGrpRegex.IsMatch(x.ModelCode) || shareGrpRegex.IsMatch(x.MaterialCode))

                .OrderByDescending(x => x.ModelCode).ToList();

            //get assembly & finish (AF) group
            //get model code - material code pair
            var assblAndFinGrpPair = assblAndFinGrpData.Select(x =>
            {
                var modelAndMachineTypeOfAF = assenblyAndfinishGrpRegex.IsMatch(x.ModelCode) ? $"{assenblyAndfinishGrpRegex.Match(x.ModelCode).Groups[1].Value}{assenblyAndfinishGrpRegex.Match(x.ModelCode).Groups[2].Value}" : string.Empty;
                var modelAndMachineTypeOfShareGrp = modelCodeRegex.IsMatch(x.MaterialCode) ? $"{modelCodeRegex.Match(x.MaterialCode).Groups[1].Value}{modelCodeRegex.Match(x.MaterialCode).Groups[2].Value}" : string.Empty;

                if (!string.IsNullOrEmpty(modelAndMachineTypeOfAF) && !string.IsNullOrEmpty(modelAndMachineTypeOfShareGrp) && modelAndMachineTypeOfShareGrp != modelAndMachineTypeOfAF && !modelAndMachineTypeOfShareGrp.EndsWith("0"))
                {
                    return x;
                }
                else return null;
            });

            //check for attachment stage number >main stage number - share group by machine
            if (shareGrpPair.Where(x => x != null).Count() > 0)
            {
                foreach (var item in shareGrpPair.Where(x => x != null))
                {
                    validateCellTypeCDto.IsValid = false;
                    var sheet = worksheets.FirstOrDefault(x => x.Name == item.SheetName);
                    sheet.Cells[item.RowNumber, 9].Value += "Cụm đính kèm không được có số thứ tự công đoạn lớn hơn cụm chính. ";
                    sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.MaterialCode)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                    sheet.TabColor = System.Drawing.Color.Red;
                }

            }

            //check share group with same model and machine type with AF group
            if (assblAndFinGrpPair.Where(x => x != null).Count() > 0)
            {
                foreach (var item in assblAndFinGrpPair.Where(x => x != null))
                {
                    validateCellTypeCDto.IsValid = false;
                    var sheet = worksheets.FirstOrDefault(x => x.Name == item.SheetName);
                    sheet.Cells[item.RowNumber, 9].Value += "Cụm đính kèm không đúng. ";
                    sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.MaterialCode)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                    sheet.TabColor = System.Drawing.Color.Red;
                }
            }

            //get BOM use quantity >1
            var assblyAndFinBomQtyGrp = assblAndFinGrpData.Where(x => assenblyAndfinishGrpRegex.IsMatch(x.MaterialCode)).Select(x =>
            {
                if (int.Parse(x.BOMUseQty) != 1)
                {
                    return x;
                }
                else return null;

            });
            if (assblyAndFinBomQtyGrp.Any(x => x != null))
            {
                foreach (var item in assblyAndFinBomQtyGrp.Where(x => x != null))
                {
                    validateCellTypeCDto.IsValid = false;
                    var sheet = worksheets.FirstOrDefault(x => x.Name == item.SheetName);
                    sheet.Cells[item.RowNumber, 9].Value += "Cụm đính kèm có số BOM > 1. ";
                    sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.MaterialCode)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                    sheet.TabColor = System.Drawing.Color.Red;
                }
            }



            //---- Share group by machine type ----//

            //---- Share group by model ----//
            //get share group by model
            var shareGrpByModelData = dataFromSheets.SelectMany(x => x.Rows).Where(x => shareGrpByModelRegex.IsMatch(x.ModelCode) || shareGrpByModelRegex.IsMatch(x.MaterialCode))

                .OrderByDescending(x => x.ModelCode).ToList();
            //get model code - material code pair
            var shareGrpByModelPair = shareGrpByModelData.Where(x => !string.IsNullOrEmpty(x.ModelCode) && shareGrpByModelRegex.IsMatch(x.MaterialCode) && shareGrpByModelRegex.IsMatch(x.ModelCode)).Select(x =>
            {
                _ = int.TryParse(shareGrpByModelRegex.Match(x.ModelCode).Groups[5].Value, out var mainStageNumber);
                _ = int.TryParse(shareGrpByModelRegex.Match(x.MaterialCode).Groups[5].Value, out var attachStageNumber);

                if (attachStageNumber > mainStageNumber)
                    return x;
                else
                    return null;
            });

            //check for attachment stage number >main stage number 
            if (shareGrpByModelPair.Where(x => x != null).Count() > 0)
            {
                foreach (var item in shareGrpByModelPair.Where(x => x != null))
                {
                    validateCellTypeCDto.IsValid = false;
                    var sheet = worksheets.FirstOrDefault(x => x.Name == item.SheetName);
                    sheet.Cells[item.RowNumber, 9].Value += "Cụm đính kèm không được có số thứ tự công đoạn lớn hơn cụm chính. ";
                    sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.MaterialCode)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                    sheet.TabColor = System.Drawing.Color.Red;
                }

            }

            var assblAndFinGrpByModelData = dataFromSheets.SelectMany(x => x.Rows).Where(x => assenblyAndfinishGrpRegex.IsMatch(x.ModelCode) || shareGrpByModelRegex.IsMatch(x.MaterialCode))

               .OrderByDescending(x => x.ModelCode).ToList();
            //get assembly & finish (AF) group
            //get model code - material code pair
            var assblAndFinGrpByModelPair = assblAndFinGrpByModelData.Select(x =>
            {
                var modelAndMachineTypeOfAF = assenblyAndfinishGrpRegex.IsMatch(x.ModelCode) ? $"{assenblyAndfinishGrpRegex.Match(x.ModelCode).Groups[1].Value}" : string.Empty;
                var modelAndMachineTypeOfShareGrp = shareGrpByModelRegex.IsMatch(x.MaterialCode) ? $"{shareGrpByModelRegex.Match(x.MaterialCode).Groups[1].Value}" : string.Empty;

                if (!string.IsNullOrEmpty(modelAndMachineTypeOfAF) && !string.IsNullOrEmpty(modelAndMachineTypeOfShareGrp) && modelAndMachineTypeOfShareGrp != modelAndMachineTypeOfAF)
                {
                    return x;
                }
                else return null;
            });


            //check share group with same model and machine type with AF group
            if (assblAndFinGrpByModelPair.Where(x => x != null).Count() > 0)
            {
                foreach (var item in assblAndFinGrpByModelPair.Where(x => x != null))
                {
                    validateCellTypeCDto.IsValid = false;
                    var sheet = worksheets.FirstOrDefault(x => x.Name == item.SheetName);
                    sheet.Cells[item.RowNumber, 9].Value += "Cụm đính kèm không đúng. ";
                    sheet.Cells[item.RowNumber, sheet.GetColumnIndex(TypeC.MaterialCode)].Style.Border.BorderAround(ExcelBorderStyle.Hair, System.Drawing.Color.Red);
                    sheet.TabColor = System.Drawing.Color.Red;
                }
            }
            //---- Share group by model ----//

            //---- Main line group identical ----//

            //get main line data
            var mainLineGrpData = dataFromSheets.SelectMany(x => x.Rows).Where(x => mainLineGrpRegex.IsMatch(x.ModelCode))

               .Select(x => new InventoryDocTypeCCodeGroupDto
               {
                   Model = mainLineGrpRegex.Match(x.ModelCode).Groups[1].Value,
                   MachineType = mainLineGrpRegex.Match(x.ModelCode).Groups[2].Value,
                   LineName = mainLineGrpRegex.Match(x.ModelCode).Groups[3].Value,
                   StageName = mainLineGrpRegex.Match(x.ModelCode).Groups[4].Value,
                   StageNumber = mainLineGrpRegex.Match(x.ModelCode).Groups[5].Value,
                   SheetName = x.SheetName,
                   Code = x.ModelCode

               });



            var groupedMainLineData = mainLineGrpData.GroupBy(x => new { x.LineName, x.StageName, x.Code })

               .Select(x => new
               {
                   Key = $"{x.Key.LineName}{x.Key.StageName}",
                   Model = x.Key.Code,
                   List = x.Where(g => g.Code.Contains($"{x.Key.LineName}{x.Key.StageName}")).Select(x => new { x.Code, x.SheetName, StageNameNumber = $"{x.StageName}{x.StageNumber}" }).Distinct()
               }).ToList();


            var groupedData = groupedMainLineData.GroupBy(x => x.Key);
            //get max list
            var maxList = groupedData?.Max(x => x?.Count());

            if (groupedData.Any(x => x.Count() < maxList))
            {
                var sheetNames = new List<string>();
                var groupedStageNameAndNumber = groupedData.SelectMany(x => x.SelectMany(x => x.List)).GroupBy(x => x.StageNameNumber);

                var maxStageNameAndNumberGrp = groupedStageNameAndNumber.Max(x => x.Count());

                foreach (var item in groupedStageNameAndNumber)
                {
                    if (item.Count() < maxStageNameAndNumberGrp)
                    {
                        sheetNames.AddRange(item.Select(x => x.SheetName).ToList());
                    }
                }

                foreach (var item in sheetNames)
                {

                    {
                        validateCellTypeCDto.IsValid = false;
                        var sheet = worksheets.FirstOrDefault(x => x.Name == item);
                        sheet.Cells[2, 9].Value += "Số lượng chuyền chính không giống nhau. ";
                        sheet.TabColor = System.Drawing.Color.Red;
                    }
                }
            }

            //---- Main line group identical ----//

            //---- Main line group valid link----//
            var mainLineModelMaterialData = mainLineGrpData.DistinctBy(x => x.Code).OrderByDescending(x => x.StageName).Select(x => new
            {
                x.SheetName,
                Main = new
                {
                    x.StageName,
                    x.Code,
                    x.StageNumber,
                },
                Attach = dataFromSheets.SelectMany(x => x.Rows).Where(a => mainLineGrpRegex.IsMatch(a.MaterialCode) && a.SheetName == x.SheetName).Select(x => new InventoryDocTypeCCodeGroupDto
                {
                    Model = mainLineGrpRegex.Match(x.MaterialCode).Groups[1].Value,
                    MachineType = mainLineGrpRegex.Match(x.MaterialCode).Groups[2].Value,
                    LineName = mainLineGrpRegex.Match(x.MaterialCode).Groups[3].Value,
                    StageName = mainLineGrpRegex.Match(x.MaterialCode).Groups[4].Value,
                    StageNumber = mainLineGrpRegex.Match(x.MaterialCode).Groups[5].Value,
                    SheetName = x.SheetName,
                    Code = x.MaterialCode

                }).ToList()

            }).GroupBy(x => new { x.Main.StageName, x.SheetName });

            var lastDGroup = mainLineModelMaterialData?.Where(x => x.Key.StageName == "d")?.SelectMany(x => x.Select(d => d.Main))?.OrderBy(x => x.StageNumber);

            var hasFirstE = false;
            foreach (var item in mainLineModelMaterialData)
            {

                if (item.Key.StageName == "e" && item?.Count() > 0)
                {
                    //if (!hasFirstE && !item.Any(x => x.Main.StageNumber == "001"))
                    //{

                    //    validateCellTypeCDto.IsValid = false;
                    //    var sheet = worksheets.FirstOrDefault(x => x.Name == item.Key.SheetName);
                    //    sheet.Cells[2, 9].Value += "Thiếu cụm hontai đầu. ";
                    //    sheet.TabColor = System.Drawing.Color.Red;
                    //}
                    //else
                    {
                        hasFirstE = true;
                        var firstEnd = item.FirstOrDefault(x => x.Main.StageNumber == "001");
                        if (firstEnd != null && firstEnd.Attach.Count == 1)
                        {
                            if (firstEnd.Attach[0].StageName == "d" && lastDGroup?.Count() > 0 && firstEnd.Attach[0].StageNumber != lastDGroup.LastOrDefault(x => ModelCodeRegex().Match(x.Code).Groups[3].Value == ModelCodeRegex().Match(firstEnd.Main.Code).Groups[3].Value).StageNumber)
                            {
                                validateCellTypeCDto.IsValid = false;
                                var sheet = worksheets.FirstOrDefault(x => x.Name == item.Key.SheetName);
                                sheet.Cells[2, 9].Value += "Thiếu cụm đính kèm. ";
                                sheet.TabColor = System.Drawing.Color.Red;
                            }
                        }
                        else if (firstEnd != null && firstEnd.Attach.Count == 0)
                        {
                            validateCellTypeCDto.IsValid = false;
                            var sheet = worksheets.FirstOrDefault(x => x.Name == item.Key.SheetName);
                            sheet.Cells[2, 9].Value += "Thiếu cụm đính kèm. ";
                            sheet.TabColor = System.Drawing.Color.Red;
                        }
                        var lasttEnd = item.FirstOrDefault(x => x.Main.StageNumber == "002");
                        if (lasttEnd != null && lasttEnd.Attach.Count == 1)
                        {
                            if (lasttEnd.Attach[0].StageName != "e" && lasttEnd.Attach[0].StageNumber != "001")
                            {
                                validateCellTypeCDto.IsValid = false;
                                var sheet = worksheets.FirstOrDefault(x => x.Name == item.Key.SheetName);
                                sheet.Cells[2, 9].Value += "Thiếu cụm đính kèm. ";
                                sheet.TabColor = System.Drawing.Color.Red;
                            }
                        }
                        else if (lasttEnd != null && lasttEnd.Attach.Count == 0)
                        {
                            validateCellTypeCDto.IsValid = false;
                            var sheet = worksheets.FirstOrDefault(x => x.Name == item.Key.SheetName);
                            sheet.Cells[2, 9].Value += "Thiếu cụm đính kèm. ";
                            sheet.TabColor = System.Drawing.Color.Red;
                        }
                    }

                }
                if (item.Key.StageName == "d" && item?.Count() > 0)
                {
                    var firstD = item.FirstOrDefault(x => x.Main.StageNumber == "001");
                    if (firstD != null && firstD.Attach?.Count > 0)
                    {
                        validateCellTypeCDto.IsValid = false;
                        var sheet = worksheets.FirstOrDefault(x => x.Name == item.Key.SheetName);
                        sheet.Cells[2, 9].Value += "Cụm hontai đầu không thể có cụm đính kèm là cụm hontai. ";
                        sheet.TabColor = System.Drawing.Color.Red;
                    }
                    var otherD = item.FirstOrDefault(x => x.Main.StageNumber != "001");
                    if (otherD != null && otherD.Attach?.Count > 0 && int.Parse(otherD.Main.StageNumber) <= int.Parse(otherD.Attach[0].StageNumber))
                    {
                        validateCellTypeCDto.IsValid = false;
                        var sheet = worksheets.FirstOrDefault(x => x.Name == item.Key.SheetName);
                        sheet.Cells[2, 9].Value += "Cụm đính kèm không được có số thứ tự công đoạn lớn hơn cụm chính. ";
                        sheet.TabColor = System.Drawing.Color.Red;
                    }
                    if (otherD != null && otherD.Attach?.Count > 0 && int.Parse(otherD.Main.StageNumber) > int.Parse(otherD.Attach[0].StageNumber) + 1)
                    {
                        validateCellTypeCDto.IsValid = false;
                        var sheet = worksheets.FirstOrDefault(x => x.Name == item.Key.SheetName);
                        sheet.Cells[2, 9].Value += "Cụm đính kèm phải có số thứ tự công đoạn liền trước cụm chính. ";
                        sheet.TabColor = System.Drawing.Color.Red;
                    }
                }
            }

            //---- Main line group  valid link----//






            if (!validateCellTypeCDto.IsValid)
            {
                validateCellTypeCDto.Title = validateCellTypeCDto.HasSpecificMessage ? validateCellTypeCDto.Title : "Lỗi dữ liệu phiếu C";
                validateCellTypeCDto.Content = validateCellTypeCDto.HasSpecificMessage ? validateCellTypeCDto.Content : "File import có dữ liệu lỗi. Vui lòng kiểm tra lại dữ liệu.";
            }

            return validateCellTypeCDto;
        }
        private List<InventoryDocTypeCSheetDto> GetDataFromSheets(ExcelWorksheets worksheets)
        {
            List<InventoryDocTypeCSheetDto> inventoryDocTypeCSheetDtos = new();
            foreach (var sheet in worksheets)
            {
                InventoryDocTypeCSheetDto inventoryDocTypeCSheetDto = new();
                inventoryDocTypeCSheetDto.SheetName = sheet.Name;


                List<InventoryDocTypeCCellDto> inventoryDocCellTypeCDtos = new();
                for (var row = 2; row <= sheet.Dimension.Rows; row++)
                {

                    var inventoryDocCellTypeCDto = new InventoryDocTypeCCellDto
                    {
                        No = GetCellValue(sheet, row, TypeC.No),
                        Plant = GetCellValue(sheet, row, TypeC.Plant),
                        WareHouseLocation = GetCellValue(sheet, row, TypeC.WarehouseLocation),
                        ModelCode = GetCellValue(sheet, row, TypeC.ModelCode),
                        MaterialCode = GetCellValue(sheet, row, TypeC.MaterialCode),
                        BOMUseQty = GetCellValue(sheet, row, TypeC.BOMUseQty),
                        StageName = GetCellValue(sheet, row, TypeC.StageName),
                        Assignee = GetCellValue(sheet, row, TypeC.Assignee),
                        //No = GetCellValue(sheet, row, ImportExcelColumns.TypeC.No),
                        RowNumber = row,
                        SheetName = sheet.Name
                    };

                    //check for all blank rows
                    if (CheckRequiredColumn(true, inventoryDocCellTypeCDto.No, inventoryDocCellTypeCDto.Plant, inventoryDocCellTypeCDto.WareHouseLocation, inventoryDocCellTypeCDto.ModelCode, inventoryDocCellTypeCDto.MaterialCode, inventoryDocCellTypeCDto.BOMUseQty, inventoryDocCellTypeCDto.StageName, inventoryDocCellTypeCDto.Assignee))
                    {
                        if (row < sheet.Dimension.Rows)
                        {
                            sheet.Row(row).Hidden = true;

                        }
                        continue;

                    }
                    inventoryDocTypeCSheetDto.ModelCode = inventoryDocCellTypeCDto.ModelCode;
                    inventoryDocTypeCSheetDto.Plant = inventoryDocCellTypeCDto.Plant;
                    inventoryDocCellTypeCDtos.Add(inventoryDocCellTypeCDto);
                }
                inventoryDocTypeCSheetDto.Rows = inventoryDocCellTypeCDtos;
                inventoryDocTypeCSheetDtos.Add(inventoryDocTypeCSheetDto);
            }
            return inventoryDocTypeCSheetDtos;
        }
        public async Task<ValidateCellTypeCDto> ValidateInventoryDocTypeC([FromForm] IFormFile file, Guid inventoryId, bool isBypassWarning = false)
        {
            ValidateCellTypeCDto result = new();
            if (file == null)
            {
                result.IsValid = false;
                result.Title = "File không tồn tại";
                result.Content = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo phiếu kiểm kê.";
                return await Task.FromResult(result);
            }
            else if (!file.FileName.EndsWith(".xlsx") && file.ContentType != FileResponse.ExcelType)
            {
                result.IsValid = false;
                result.Title = "File sai định dạng";
                result.Content = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo phiếu kiểm kê.";
                return await Task.FromResult(result);
            }

            //Nếu trên hệ thống chưa có phiếu A nào được tạo thì hiển thị thông báo với nội dung: “ Vui lòng tạo phiếu A trước khi thực hiện tạo các phiếu khác.”

            var checkExistInventoryDocTypeA = await _inventoryContext.InventoryDocs.FirstOrDefaultAsync(x => x.DocType == InventoryDocType.A);
            if (checkExistInventoryDocTypeA == null)
            {
                result.IsValid = false;
                result.Title = "Không thể tạo phiếu";
                result.Content = "Vui lòng tạo phiếu A trước khi thực hiện tạo các phiếu khác.";
                return await Task.FromResult(result);
            }

            using (var memStream = new MemoryStream())
            {
                file.CopyTo(memStream);
                var checkDto = new CheckInventoryDocumentDto();
                var resultDto = new InventoryDocumentImportResultDto();
                using (var sourcePackage = new ExcelPackage(memStream))
                {
                    var sheets = sourcePackage.Workbook.Worksheets;
                    foreach (var sheet in sheets)
                    {
                        //assumption - first row is header
                        var headerValue = (object[,])sheet.Cells["A1:H1"].Value;

                        if (headerValue.Cast<string>().Any(string.IsNullOrEmpty))
                        {
                            result.IsValid = false;
                            result.Title = "File sai định dạng";
                            result.Content = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo phiếu kiểm kê.";
                            return result;
                        }

                        var listHearder = TypeC.listHeaderTypeC;

                        var headerListExcel = new List<string>();

                        foreach (var item in headerValue)
                        {
                            headerListExcel.Add(item.ToString());
                        }

                        // Kiểm tra độ dài của hai danh sách
                        if (listHearder.Count != headerListExcel.Count)
                        {
                            result.IsValid = false;
                            result.Title = "File sai định dạng";
                            result.Content = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo phiếu kiểm kê.";
                            return result;
                        }

                        // Kiểm tra từng phần tử của hai danh sách
                        for (var i = 0; i < headerListExcel.Count; i++)
                        {
                            if (headerListExcel[i] != listHearder[i])
                            {
                                result.IsValid = false;
                                result.Title = "File sai định dạng";
                                result.Content = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo phiếu kiểm kê.";
                                return result;
                            }
                        }
                    }

                    //get assignee info
                    var assignees = await GetLastDocNumberAndAssignee(InventoryDocType.C, inventoryId, true);

                    //get data from sheets
                    var dataFromSheets = GetDataFromSheets(sheets);

                    //Get all Roles:
                    var request = new RestRequest(Constants.Endpoint.Internal.Get_All_Roles_Department);
                    request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
                    request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

                    var roles = await _identityRestClient.ExecuteGetAsync(request);

                    var rolesModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<GetAllRoleWithUserNameModel>>>(roles.Content ?? string.Empty);

                    var createDocumentRoles = rolesModel?.Data;

                    try
                    {
                        //validate data all sheets
                        result = ValidateSheets(createDocumentRoles, inventoryId, sheets, dataFromSheets, assignees, isBypassWarning);
                    }
                    catch (Exception ex)
                    {

                        result.IsValid = false;
                        result.Title = "Không thể tạo phiếu";
                        result.Content = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo phiếu kiểm kê.";
                        return result;
                    }

                    if (result.Title != "File sai định dạng")
                    {
                        result.Data = sourcePackage.GetAsByteArray();
                    }

                }
            }


            return result;

        }

        /// <summary>
        /// Set data to entities of:
        /// InventoryDoc type C - DocTypeCDetail - TypeCUnit - DocTypeCUnitDetail
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="lastDocNumberAndAssignee"></param>
        /// <param name="inventoryDocs"></param>
        /// <param name="dataFromSheets">Type C data</param>
        /// <param name="importer"></param>
        private InventoryDocTypeCEntitesDto SetDataToEntities(Guid inventoryId, InventoryDocAndUserDto lastDocNumberAndAssignee, List<InventoryDocTypeCSheetDto> dataFromSheets, string importer, bool isBypassWarning = false)
        {
            InventoryDocTypeCEntitesDto dataToEntities = new();
            var shareGrpRegex = ShareGroupRegex();
            var shareGrpByModelRegex = ShareGroupByModelRegex();
            var modelRegex = ModelCodeRegex();

            var listInvDocModelCode = _inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.DocType == InventoryDocType.C && x.InventoryId == inventoryId).Select(x => new { x.ModelCode, x.Id, x.CreatedAt, x.CreatedBy }).ToArray();
            var listInvDocModelMaterialCode = _inventoryContext.DocTypeCDetails.AsNoTracking().Where(x => x.InventoryId == inventoryId).Select(x => new { x.ModelCode, x.ComponentCode, x.Id, x.CreatedAt, x.CreatedBy, x.DirectParent }).ToArray();
            //var listDocCUnitModelCode = _inventoryContext.DocTypeCUnits.AsNoTracking().Where(x => x.InventoryId == inventoryId).Select(x => new { x.ModelCode, x.Id, x.CreatedAt, x.CreatedBy }).ToArray();


            foreach (var sheetData in dataFromSheets)
            {
                //if (!shareGrpRegex.IsMatch(sheetData.ModelCode) && !shareGrpByModelRegex.IsMatch(sheetData.ModelCode))
                {
                    //InventoryDoc
                    var modelCode = modelRegex.IsMatch(sheetData.ModelCode) ? modelRegex.Match(sheetData.ModelCode) : null;

                    var invDoc = sheetData.Rows.Select(x => new InventoryDoc
                    {
                        Id = Guid.NewGuid(),
                        InventoryId = inventoryId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = importer,
                        No = x.No,
                        Plant = x.Plant,
                        DocType = InventoryDocType.C,
                        StageName = x.StageName,
                        AssignedAccountId = lastDocNumberAndAssignee.Assignees.FirstOrDefault(a => a.UserName == x.Assignee)?.UserId,
                        DocCode = $"{lastDocNumberAndAssignee.InventoryPart}{lastDocNumberAndAssignee.LastDocNumber++:00000}",
                        MachineModel = modelCode?.Groups[1].Value,
                        MachineType = modelCode?.Groups[2].Value,
                        LineName = modelCode?.Groups[3].Value,
                        LineType = modelCode?.Groups[4].Value,
                        StageNumber = modelCode?.Groups[5].Value,
                        ModelCode = sheetData.ModelCode,
                        LocationName = lastDocNumberAndAssignee.Assignees.FirstOrDefault(a => a.UserName == x.Assignee)?.LocationName,
                        DepartmentName = lastDocNumberAndAssignee.Assignees.FirstOrDefault(a => a.UserName == x.Assignee)?.DepartmentName,
                        WareHouseLocation = x.WareHouseLocation
                    }).FirstOrDefault();
                    if (!listInvDocModelCode.Any(x => x.ModelCode == invDoc?.ModelCode))
                    {
                        dataToEntities.InvDocs.Add(invDoc);
                    }
                    else
                    {
                        var existed = listInvDocModelCode.FirstOrDefault(x => x.ModelCode == invDoc.ModelCode);
                        invDoc.Id = existed.Id;
                        if (isBypassWarning)
                        {
                            invDoc.CreatedAt = existed.CreatedAt;
                            invDoc.CreatedBy = existed.CreatedBy;
                            invDoc.UpdatedAt = DateTime.Now;
                            invDoc.UpdatedBy = importer;
                            dataToEntities.InvDocsUpdate.Add(invDoc);
                        }

                    }
                    dataToEntities.OriginDocTypeCDetailIds = _inventoryContext.DocTypeCDetails.AsNoTracking().Where(x => x.InventoryDocId == invDoc.Id).Select(x => x.Id).ToList();

                    foreach (var item in sheetData.Rows)
                    {
                        //if (shareGrpRegex.IsMatch(item.MaterialCode) || shareGrpByModelRegex.IsMatch(sheetData.ModelCode))
                        //{
                        //    var docTypeCUnit = new DocTypeCUnit
                        //    {
                        //        Id = Guid.NewGuid(),
                        //        CreatedAt = DateTime.Now,
                        //        CreatedBy = importer,
                        //        InventoryId = inventoryId,
                        //        ModelCode = item.MaterialCode,
                        //        WarehouseLocation = item.WareHouseLocation,
                        //        Plant = item.Plant,
                        //        StageName = item.StageName
                        //    };

                        //    if (!listDocCUnitModelCode.Any(x => x.ModelCode == docTypeCUnit?.ModelCode))
                        //    {
                        //        dataToEntities.DocTypeCUnits.Add(docTypeCUnit);
                        //    }
                        //    else
                        //    {
                        //        var existed = listDocCUnitModelCode.FirstOrDefault(x => x.ModelCode == docTypeCUnit.ModelCode);
                        //        docTypeCUnit.Id = existed.Id;
                        //        if (isBypassWarning)
                        //        {

                        //            docTypeCUnit.CreatedAt = existed.CreatedAt;
                        //            docTypeCUnit.CreatedBy = existed.CreatedBy;
                        //            docTypeCUnit.UpdatedAt = DateTime.Now;
                        //            docTypeCUnit.UpdatedBy = importer;
                        //            dataToEntities.DocTypeCUnitsUpdate.Add(docTypeCUnit);
                        //        }
                        //    }
                        //}
                        //DocTypeCDetail

                        var parseNo = int.TryParse(item.No, out int no);
                        var docTypeCDetail = new DocTypeCDetail
                        {
                            Id = Guid.NewGuid(),
                            InventoryDocId = invDoc.Id,
                            InventoryId = inventoryId,
                            CreatedAt = DateTime.Now,
                            CreatedBy = importer,
                            WarehouseLocation = item.WareHouseLocation,
                            QuantityOfBom = double.Parse(item.BOMUseQty),
                            ComponentCode = modelRegex.IsMatch(item.MaterialCode) ? string.Empty : item.MaterialCode,
                            ModelCode = modelRegex.IsMatch(item.MaterialCode) ? item.MaterialCode : string.Empty,
                            QuantityPerBom = 0d,
                            isHighlight = false,
                            DirectParent = sheetData.ModelCode,
                            No = parseNo ? no : null

                        };
                        if (!listInvDocModelMaterialCode.Any(x => x.ModelCode == docTypeCDetail.ModelCode && x.ComponentCode == docTypeCDetail.ComponentCode && docTypeCDetail.DirectParent == x.DirectParent))
                        {
                            dataToEntities.DocTypeCDetails.Add(docTypeCDetail);
                        }
                        else
                        {
                            var existed = listInvDocModelMaterialCode.FirstOrDefault(x => x.ModelCode == docTypeCDetail.ModelCode && x.ComponentCode == docTypeCDetail.ComponentCode && docTypeCDetail.DirectParent == x.DirectParent);
                            //docTypeCDetail.Id = existed.Id;

                            //20240705: Remove check isBypassWarning:
                            //if (isBypassWarning)
                            {
                                var existedDocTypeCDetail = _inventoryContext.DocTypeCDetails.FirstOrDefault(x => x.Id == existed.Id);
                                existedDocTypeCDetail.InventoryDocId = invDoc.Id;
                                existedDocTypeCDetail.InventoryId = inventoryId;
                                existedDocTypeCDetail.CreatedAt = DateTime.Now;
                                existedDocTypeCDetail.CreatedBy = importer;
                                existedDocTypeCDetail.WarehouseLocation = item.WareHouseLocation;
                                existedDocTypeCDetail.QuantityOfBom = double.Parse(item.BOMUseQty);
                                existedDocTypeCDetail.ComponentCode = modelRegex.IsMatch(item.MaterialCode) ? string.Empty : item.MaterialCode;
                                existedDocTypeCDetail.ModelCode = modelRegex.IsMatch(item.MaterialCode) ? item.MaterialCode : string.Empty;
                                existedDocTypeCDetail.QuantityPerBom = 0d;
                                existedDocTypeCDetail.isHighlight = false;
                                existedDocTypeCDetail.DirectParent = sheetData.ModelCode;
                                existedDocTypeCDetail.No = parseNo ? no : null;


                                docTypeCDetail.CreatedAt = existed.CreatedAt;
                                docTypeCDetail.CreatedBy = existed.CreatedBy;
                                docTypeCDetail.UpdatedAt = DateTime.Now;
                                docTypeCDetail.UpdatedBy = importer;
                                dataToEntities.DocTypeCDetailsUpdate.Add(existedDocTypeCDetail);
                            }
                        }

                    }
                    dataToEntities.NewDocTypeCDetailIds = dataToEntities.DocTypeCDetailsUpdate.Select(u => u.Id).ToList();

                    if (isBypassWarning)
                    {
                        var exceptIdList = new List<Guid>();

                        dataToEntities.OriginDocTypeCDetailIds.Except(dataToEntities.NewDocTypeCDetailIds).ToList().ForEach(x => exceptIdList.Add(x));

                        var except = _inventoryContext.DocTypeCDetails.AsNoTracking().Where(x => exceptIdList.Contains(x.Id)).ToList();
                        if (except.Any())
                        {
                            foreach (var item in except)
                            {

                                var componentList = _inventoryContext.DocTypeCComponents.AsNoTracking().Where(x => x.ComponentCode == item.ComponentCode && x.InventoryId == inventoryId).Select(x => new DocTypeCComponentDto
                                {
                                    Id = x.Id,
                                    MainModelCode = x.MainModelCode,
                                    ComponentCode = x.ComponentCode,
                                    UnitModelCode = x.UnitModelCode
                                }).ToList();

                                var listGuid = new List<Guid?>();
                                foreach (var cpnt in componentList)
                                {

                                    GetParentIds(cpnt.MainModelCode, item.ComponentCode, componentList, ref listGuid);

                                }


                                //var listComponent = _inventoryContext.DocTypeCComponents.AsNoTracking().Where(x => x.ComponentCode == item.ComponentCode).ToList();
                                //var components = listComponent.Where(x => x.ComponentCode == item.ComponentCode && x.MainModelCode == item.DirectParent).Select(x => new DocTypeCComponentDeleteDto
                                //{

                                //    Id = x.Id,

                                //    Parent = GetParents(x.MainModelCode, x.ComponentCode, listComponent, new List<DocTypeCComponentDeleteDto>())

                                //}).ToList();
                                exceptIdList.AddRange(listGuid.Select(x => x.Value));
                                //exceptIdList.AddRange(components.SelectMany(x => x.Parent.Select(x => x.Id)));
                            }
                            _inventoryContext.DocTypeCComponents.Where(x => exceptIdList.Contains(x.Id)).ExecuteDelete();
                            _inventoryContext.DocTypeCDetails.Where(x => exceptIdList.Contains(x.Id)).ExecuteDelete();
                        }
                    }



                    //var except = existedInvDocDetail.Where(x => !dataToEntities.DocTypeCDetailsUpdate.Select(u => u.Id).Contains(x.Id)).ToList();

                    //if (except.Any())
                    //{
                    //    var deleteList = new List<Guid>();
                    //    foreach (var item in except)
                    //    {
                    //        var listComponent = _inventoryContext.DocTypeCComponents.AsNoTracking().Where(x => x.ComponentCode == item.ComponentCode).ToList();
                    //        //recursively get all parent
                    //        var components = listComponent.Where(x => x.ComponentCode == item.ComponentCode && x.MainModelCode == item.DirectParent).Select(x => new DocTypeCComponentDeleteDto
                    //        {

                    //            Id = x.Id,

                    //            Parent = GetParents(x.MainModelCode, x.ComponentCode, listComponent, new List<DocTypeCComponentDeleteDto>())

                    //        }).ToList();
                    //        deleteList.AddRange(components.Select(x => x.Id));
                    //        deleteList.AddRange(components.SelectMany(x => x.Parent.Select(x => x.Id)));


                    //    }


                    //    _inventoryContext.DocTypeCComponents.Where(x => deleteList.Contains(x.Id)).ExecuteDelete();
                    //    _inventoryContext.DocTypeCDetails.Where(x => except.Select(e => e.Id).Contains(x.Id)).ExecuteDelete();
                    //}

                }
                //else
                //{
                //    //TypeCUnit
                //    var docTypeCUnit = sheetData.Rows.Where(x => shareGrpRegex.IsMatch(x.ModelCode) || shareGrpByModelRegex.IsMatch(x.ModelCode)).Select(x => new DocTypeCUnit
                //    {
                //        Id = Guid.NewGuid(),
                //        CreatedAt = DateTime.Now,
                //        CreatedBy = importer,
                //        InventoryId = inventoryId,
                //        ModelCode = x.ModelCode,
                //        WarehouseLocation = x.WareHouseLocation,
                //        Plant = x.Plant,
                //        StageName = x.StageName
                //    })?.FirstOrDefault();
                //    var existedDocCUnitModelCode = listDocCUnitModelCode.FirstOrDefault(x => x.ModelCode == docTypeCUnit.ModelCode);
                //    if (!listDocCUnitModelCode.Any(x => x.ModelCode == docTypeCUnit?.ModelCode))
                //    {
                //        dataToEntities.DocTypeCUnits.Add(docTypeCUnit);
                //    }
                //    else
                //    {
                //        if (isBypassWarning && existedDocCUnitModelCode != null)
                //        {
                //            docTypeCUnit.Id = existedDocCUnitModelCode.Id;
                //            docTypeCUnit.CreatedAt = existedDocCUnitModelCode.CreatedAt;
                //            docTypeCUnit.CreatedBy = existedDocCUnitModelCode.CreatedBy;
                //            docTypeCUnit.UpdatedAt = DateTime.Now;
                //            docTypeCUnit.UpdatedBy = importer;
                //            dataToEntities.DocTypeCUnitsUpdate.Add(docTypeCUnit);
                //        }
                //    }

                //    //DocTypeCUnitDetail
                //    foreach (var item in sheetData.Rows)
                //    {





                //        var docTypeCUnitDetail = new DocTypeCUnitDetail
                //        {
                //            Id = Guid.NewGuid(),
                //            DocTypeCUnitId = existedDocCUnitModelCode != null ? existedDocCUnitModelCode.Id : docTypeCUnit.Id,
                //            CreatedAt = DateTime.Now,
                //            CreatedBy = importer,
                //            ComponentCode = modelRegex.IsMatch(item.MaterialCode) ? string.Empty : item.MaterialCode,
                //            ModelCode = modelRegex.IsMatch(item.MaterialCode) ? item.MaterialCode : string.Empty,
                //            QuantityOfBOM = int.Parse(item.BOMUseQty),
                //            IsHighLight = false,
                //            InventoryId = inventoryId,
                //            QuantityPerBOM = 0,
                //            DirectParent = sheetData.ModelCode
                //        };
                //        if (!listInvDocModelMaterialCode.Any(x => x.ModelCode == docTypeCUnitDetail.ModelCode && x.ComponentCode == docTypeCUnitDetail.ComponentCode && docTypeCUnitDetail.DirectParent == x.DirectParent))
                //        {
                //            dataToEntities.DocTypeCUnitDetails.Add(docTypeCUnitDetail);
                //        }
                //        else
                //        {
                //            var existed = listInvDocModelMaterialCode.FirstOrDefault(x => x.ModelCode == docTypeCUnitDetail.ModelCode && x.ComponentCode == docTypeCUnitDetail.ComponentCode && docTypeCUnitDetail.DirectParent == x.DirectParent);

                //            if (isBypassWarning && existed != null)
                //            {
                //                docTypeCUnitDetail.Id = existed.Id;
                //                docTypeCUnitDetail.CreatedAt = existed.CreatedAt;
                //                docTypeCUnitDetail.CreatedBy = existed.CreatedBy;
                //                docTypeCUnitDetail.UpdatedAt = DateTime.Now;
                //                docTypeCUnitDetail.UpdatedBy = importer;
                //                dataToEntities.DocTypeCUnitDetailsUpdate.Add(docTypeCUnitDetail);
                //            }
                //        }

                //    }
                //}
            }




            return dataToEntities;
        }
        private List<DocTypeCComponentDeleteDto> GetParents(string childModelCode, string componentCode, List<DocTypeCComponent> listComponent, List<DocTypeCComponentDeleteDto> result)
        {
            result.AddRange(
                listComponent.Where(x => x.UnitModelCode == childModelCode && x.ComponentCode == componentCode).Select(x => new DocTypeCComponentDeleteDto
                {
                    Id = x.Id,
                    Parent = GetParents(x.MainModelCode, x.ComponentCode, listComponent, result)
                }).ToList()
                );


            return result;
        }

        private List<Guid?> GetParentIds(string childModelCode, string componentCode, List<DocTypeCComponentDto> listComponent, ref List<Guid?> result)
        {
            var component = listComponent.FirstOrDefault(x => x.UnitModelCode == childModelCode && x.ComponentCode == componentCode);
            if (component != null && !result.Any(x => x == component.Id))
            {
                result.Add(component?.Id);

                GetParentIds(component.MainModelCode, componentCode, listComponent, ref result);
            }
            return result;
        }


        public async Task<ResponseModel> ImportInventoryDocTypeC([FromForm] IFormFile file, Guid inventoryId, bool isBypassWarning = false)
        {
            var result = new ResponseModel();
            using (var memStream = new MemoryStream())
            {
                file.CopyTo(memStream);
                var checkDto = new CheckInventoryDocumentDto();
                var resultDto = new InventoryDocumentImportResultDto();
                using (var sourcePackage = new ExcelPackage(memStream))
                {

                    try
                    {
                        var sheets = sourcePackage.Workbook.Worksheets;

                        //get assignee info
                        var assignees = await GetLastDocNumberAndAssignee(InventoryDocType.C, inventoryId);

                        //get data from sheets
                        var dataFromSheets = GetDataFromSheets(sheets);

                        //set data to entities
                        var entitiesData = SetDataToEntities(inventoryId, assignees, dataFromSheets, _httpContext.CurrentUserId(), isBypassWarning);

                        //insert to db
                        await _inventoryContext.InventoryDocs.AddRangeAsync(entitiesData.InvDocs);
                        await _inventoryContext.DocTypeCDetails.AddRangeAsync(entitiesData.DocTypeCDetails);
                        //await _inventoryContext.DocTypeCUnits.AddRangeAsync(entitiesData.DocTypeCUnits);
                        //await _inventoryContext.DocTypeCUnitDetails.AddRangeAsync(entitiesData.DocTypeCUnitDetails);

                        _inventoryContext.InventoryDocs.UpdateRange(entitiesData.InvDocsUpdate);
                        _inventoryContext.DocTypeCDetails.UpdateRange(entitiesData.DocTypeCDetailsUpdate);

                        //_inventoryContext.DocTypeCUnits.UpdateRange(entitiesData.DocTypeCUnitsUpdate);
                        //_inventoryContext.DocTypeCUnitDetails.UpdateRange(entitiesData.DocTypeCUnitDetailsUpdate);

                        await _inventoryContext.SaveChangesAsync();
                        //await Task.WhenAll(addInvDoc, addTypeCDetail,addTypeCUnit, addTypeCUnitDetail).ContinueWith(async x => await _inventoryContext.SaveChangesAsync());

                        var countInsert = entitiesData.InvDocs.Count;
                        var countUpdate = entitiesData.InvDocsUpdate.Count;

                        result.Code = StatusCodes.Status200OK;
                        result.Message = $"Import {countInsert + countUpdate} phiếu C thành công.";

                        //run background for insert data to DocTypeCComponent
                        //await _dataAggregationService.AddDataToDocTypeCComponent(dataFromSheets, _httpContext.CurrentUserId(), inventoryId);

                    }
                    catch (Exception exception)
                    {

                        var exMess = $"Exception - {exception.Message}";
                        var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                        _logger.LogError($"Request error at {_httpContext.Request.Path} : {exMess}; {innerExMess}");
                    }


                }
            }
            return result;
        }

        public async Task<ResponseModel> IsHightLightCheck(CheckIsHightLightDocTypeCDto checkIsHightLightDocTypeCDto)
        {
            var docTypeCIsHightLights = await _inventoryContext.DocTypeCDetails.Where(x => x.InventoryId.ToString().ToLower() == checkIsHightLightDocTypeCDto.InventoryId.ToLower()
                                                                                     && x.InventoryDocId.ToString().ToLower() == checkIsHightLightDocTypeCDto.DocId.ToLower()
                                                                                     && x.isHighlight == true).ToListAsync();
            if (docTypeCIsHightLights.Count() != checkIsHightLightDocTypeCDto.Ids.Count())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Vẫn còn phiếu C chưa được hightlight.",
                    Data = new
                    {
                        docTypeCIsHightLights = false
                    }
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Chi tiết phiếu C đã được hightlight.",
                Data = new
                {
                    docTypeCIsHightLights = true
                }
            };
        }

        public async Task<ResponseModel> DeleteAuditTargets(Guid inventoryId, List<Guid> IDs, bool isDeleteAll)
        {
            //Validate
            if (!Guid.TryParse(inventoryId.ToString(), out _))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage
                };
            }
            if (!isDeleteAll && !IDs.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage
                };
            }


            var inventory = await _inventoryContext.Inventories.FirstOrDefaultAsync(x => x.Id == inventoryId);
            if (inventory == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = Constants.ResponseMessages.Inventory.NotFound
                };
            }

            var currInventoryAcc = _httpContext.CurrentUser();
            var query = _inventoryContext.AuditTargets.Where(x => x.InventoryId == inventoryId).AsQueryable();

            if (isDeleteAll)
            {
                query = query.Where(x => !IDs.Contains(x.Id));
            }
            else
            {
                query = query.Where(x => IDs.Contains(x.Id));
            }

            var entities = await query.ToListAsync();
            int batchSize = 2000;
            var totalCount = entities.Count;
            var batches = Enumerable.Range(0, (totalCount + batchSize - 1) / batchSize)
                          .Select(i => entities.Skip(i * batchSize).Take(batchSize));

            Parallel.ForEach(batches, async (items) =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {

                    var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
                    {
                        context.AuditTargets.RemoveRange(items);
                        await context.SaveChangesAsync();
                    }
                }
            });

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Xóa linh kiện giám sát thành công."
            };
        }

        public async Task<ResponseModel> DeleteInventoryDocs(Guid inventoryId, ListInventoryDocumentDeleteDto listInventoryDocumentDeleteDto, bool isDeleteAll)
        {
            //Validate
            if (!Guid.TryParse(inventoryId.ToString(), out _))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Thông tin đợt kiểm kê không hợp lệ."
                };
            }
            //if (!isDeleteAll && !listInventoryDocumentDeleteDto.IDs.Any())
            //{
            //    return new ResponseModel
            //    {
            //        Code = StatusCodes.Status400BadRequest,
            //        Message = "Danh sách phiếu kiểm kê cần xóa không hợp lệ."
            //    };
            //}


            var inventory = await _inventoryContext.Inventories.FirstOrDefaultAsync(x => x.Id == inventoryId);
            if (inventory == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu đợt kiểm kê hiện tại."
                };
            }
            var currInventoryAcc = _httpContext.CurrentUser();

            if (listInventoryDocumentDeleteDto.IsDeletedAllHasFilters)
            {
                var result = await _inventoryService.GetInventoryDocumentDeleteHasFilter(listInventoryDocumentDeleteDto, inventoryId.ToString());
                var deleteIds = result.Data;
                if (deleteIds.Any())
                {
                    await _inventoryContext.InventoryDocs.Where(x => deleteIds.Contains(x.Id)).ExecuteUpdateAsync(x => x.SetProperty(x => x.IsDeleted, true)
                                            .SetProperty(x => x.DeletedAt, DateTime.Now)
                                            .SetProperty(x => x.DeletedBy, currInventoryAcc.UserCode));
                }

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Xóa các phiếu kiểm kê thành công."
                };
            }

            var query = _inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryId).AsQueryable();

            if (isDeleteAll)
            {
                query = query.Where(x => !listInventoryDocumentDeleteDto.IDs.Contains(x.Id));
            }
            else
            {
                query = query.Where(x => listInventoryDocumentDeleteDto.IDs.Contains(x.Id));
            }
            if (query.Any())
            {
                await query.ExecuteUpdateAsync(x => x.SetProperty(x => x.IsDeleted, true)
                                            .SetProperty(x => x.DeletedAt, DateTime.Now)
                                            .SetProperty(x => x.DeletedBy, currInventoryAcc.UserCode));
            }

            //Parallel.ForEach(batches, async (items) =>
            //{
            //    using (var scope = _serviceScopeFactory.CreateScope())
            //    {

            //        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
            //        {
            //            ////Delete DocOutputs, DocTypeCDetails, DocTypeCComponents:
            //            //var inventoryDocIds = items.Select(x => x.Id).ToList();

            //            //var deleteDocOutPuts = context.DocOutputs.Where(x => inventoryDocIds.Contains(x.InventoryDocId.Value)).ToList();
            //            //var deleteDocTypeCDetails = context.DocTypeCDetails.Where(x => inventoryDocIds.Contains(x.InventoryDocId.Value)).ToList();
            //            //var deleteDocTypeCComponents = context.DocTypeCComponents.Where(x => inventoryDocIds.Contains(x.InventoryDocId.Value)).ToList();

            //            //context.DocOutputs.RemoveRange(deleteDocOutPuts);
            //            //context.DocTypeCDetails.RemoveRange(deleteDocTypeCDetails);
            //            //context.DocTypeCComponents.RemoveRange(deleteDocTypeCComponents);

            //            ////Delete HistoryOutputs, HistoryTypeCDetails, DocHistories:
            //            //var deleteDocHistories = context.DocHistories.Where(x => inventoryDocIds.Contains(x.InventoryDocId.Value)).ToList();

            //            //var historyIds = context.DocHistories.Where(x => inventoryDocIds.Contains(x.InventoryDocId.Value)).Select(x => x.Id).ToList();

            //            //var deleteHistoryDocOutputs = context.HistoryOutputs.Where(x => historyIds.Contains(x.DocHistoryId.Value)).ToList();
            //            //var deleteHistoryTypeCDetails = context.HistoryTypeCDetails.Where(x => historyIds.Contains(x.HistoryId.Value)).ToList();

            //            //context.HistoryOutputs.RemoveRange(deleteHistoryDocOutputs);
            //            //context.HistoryTypeCDetails.RemoveRange(deleteHistoryTypeCDetails);
            //            //context.DocHistories.RemoveRange(deleteDocHistories);

            //            ////Delete InventoryDocs:
            //            //context.InventoryDocs.RemoveRange(items);

            //            //Xóa cứng các phiếu:
            //            var user = _httpContext.CurrentUser();
            //            items.ForEach(x =>
            //            {
            //                x.IsDeleted = true;
            //                x.DeletedAt = DateTime.Now;
            //                x.DeletedBy = user.UserCode;
            //            });
            //            context.InventoryDocs.UpdateRange(items);
            //            await context.SaveChangesAsync();
            //        }
            //    }
            //});

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Xóa các phiếu kiểm kê thành công."
            };
        }

        public async Task<ResponseModel<IEnumerable<string>>> GetModelCodesForDocB(Guid inventoryId, Guid accountId)
        {
            var checkAccountTypeAndRole = _httpContext.UserFromContext();

            var modelsQuery = _inventoryContext.InventoryDocs.AsNoTracking()
                                                        .Where(x => x.InventoryId.Value == inventoryId
                                                        //&& x.AssignedAccountId.Value == accountId
                                                        && x.DocType == InventoryDocType.B
                                                        && ExcludeDocStatus.Contains(x.Status) == false).AsQueryable();
            if (modelsQuery == null || !modelsQuery.Any())
            {
                return new ResponseModel<IEnumerable<string>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            var models = modelsQuery;

            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                models = models.Where(x => x.AssignedAccountId.Value == accountId);
            }

            models = models.Where(x => !string.IsNullOrEmpty(x.MachineModel));
            var filter = await models?.Select(x => x.MachineModel)?.Distinct()?.ToListAsync();

            return new ResponseModel<IEnumerable<string>>
            {
                Code = StatusCodes.Status200OK,
                Data = filter ?? new List<string>()
            };
        }

        public async Task<ResponseModel<IEnumerable<MachineTypeModel>>> GetMachineTypesDocB(Guid inventoryId, Guid accountId, string machineModel)
        {
            var checkAccountTypeAndRole = _httpContext.UserFromContext();

            var modelsQuery = _inventoryContext.InventoryDocs.AsNoTracking().Where(x => ExcludeDocStatus.Contains(x.Status) == false
                                                                && x.InventoryId.Value == inventoryId
                                                                //&& x.AssignedAccountId.Value == accountId
                                                                && x.DocType == InventoryDocType.B
                                                                && x.MachineModel == machineModel).AsQueryable();

            if (modelsQuery == null || !modelsQuery.Any())
            {
                return new ResponseModel<IEnumerable<MachineTypeModel>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }
            var models = modelsQuery;
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                models = models.Where(x => x.AssignedAccountId.Value == accountId);
            }

            models = models.Where(x => !string.IsNullOrEmpty(x.MachineType));
            var filter = models?.GroupBy(x => x.MachineType).ToList();
            var result = filter.Select(x =>
            {
                MachineTypeModel model = new();
                model.Key = x.Key;
                model.DisplayName = DocCMachineModel.GetDisplayMachineType(x.Key);

                return model;
            });

            return new ResponseModel<IEnumerable<MachineTypeModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = result
            };
        }

        public async Task<ResponseModel<IEnumerable<string>>> GetDropDownModelCodesForDocB(Guid inventoryId, Guid accountId, string machineModel, string machineType)
        {
            var checkAccountTypeAndRole = _httpContext.UserFromContext();

            var modelsQuery = _inventoryContext.InventoryDocs.AsNoTracking().Where(x => ExcludeDocStatus.Contains(x.Status) == false
                                                                && x.InventoryId.Value == inventoryId
                                                                //&& x.AssignedAccountId.Value == accountId
                                                                && x.DocType == InventoryDocType.B
                                                                && x.MachineModel == machineModel && x.MachineType == machineType).AsQueryable();
            if (modelsQuery == null || !modelsQuery.Any())
            {
                return new ResponseModel<IEnumerable<string>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }
            var models = modelsQuery;
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                models = models.Where(x => x.AssignedAccountId.Value == accountId);
            }

            models = models.Where(x => !string.IsNullOrEmpty(x.ModelCode));
            var filter = await models?.Select(x => x.ModelCode)?.Distinct()?.ToListAsync();

            //models = models.Where(x => !string.IsNullOrEmpty(x.LineName));
            //var filter = models?.GroupBy(x => x.LineName).ToList();
            //var result = filter.Select(x =>
            //{
            //    LineModel model = new();
            //    model.Key = x.Key;
            //    model.DisplayName = DocCMachineModel.GetDisplayLineName(x.Key);

            //    return model;
            //});

            return new ResponseModel<IEnumerable<string>>
            {
                Code = StatusCodes.Status200OK,
                Data = filter ?? new List<string>()
            };
        }


        public async Task<ResponseModel<IEnumerable<LineModel>>> GetLineNamesDocB(Guid inventoryId, Guid accountId, string machineModel, string machineType, string modelCode)
        {
            var checkAccountTypeAndRole = _httpContext.UserFromContext();

            var modelsQuery = _inventoryContext.InventoryDocs.AsNoTracking().Where(x => ExcludeDocStatus.Contains(x.Status) == false
                                                                && x.InventoryId.Value == inventoryId
                                                                //&& x.AssignedAccountId.Value == accountId
                                                                && x.DocType == InventoryDocType.B
                                                                && x.MachineModel == machineModel
                                                                && x.MachineType == machineType
                                                                && x.ModelCode == modelCode).AsQueryable();
            if (modelsQuery == null || !modelsQuery.Any())
            {
                return new ResponseModel<IEnumerable<LineModel>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            var models = modelsQuery;
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                models = models.Where(x => x.AssignedAccountId.Value == accountId);
            }

            models = models.Where(x => !string.IsNullOrEmpty(x.LineName));
            var filter = models?.GroupBy(x => x.LineName).ToList();
            var result = filter.Select(x =>
            {
                LineModel model = new();
                model.Key = x.Key;
                model.DisplayName = DocCMachineModel.GetDisplayLineName(x.Key);

                return model;
            });

            return new ResponseModel<IEnumerable<LineModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = result
            };
        }

        public async Task<ResponseModel<DocBListViewModel>> GetDocsB(ListDocBFilterModel listDocBFilterModel)
        {
            if (listDocBFilterModel == null)
            {
                return new ResponseModel<DocBListViewModel>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Điều kiện lọc không hợp lệ."
                };
            }

            var checkAccountTypeAndRole = _httpContext.UserFromContext();

            var docsQuery = from d in _inventoryContext.InventoryDocs.AsNoTracking()
                            let condition = ExcludeDocStatus.Contains(d.Status) == false
                                    && d.DocType == InventoryDocType.B
                                    && d.InventoryId == listDocBFilterModel.InventoryId
                            //&& d.AssignedAccountId == listDocBFilterModel.AccountId
                            //&& d.MachineModel == listDocBFilterModel.MachineModel
                            //&& d.MachineType == listDocBFilterModel.MachineType
                            //&& d.ModelCode == listDocBFilterModel.ModelCode

                            let docStatusOrder = (int)d.Status == (int)InventoryDocStatus.AuditPassed ? (int)InventoryDocStatus.AuditFailed :
                                               (int)d.Status == (int)InventoryDocStatus.AuditFailed ? (int)InventoryDocStatus.AuditPassed :
                                               (int)d.Status

                            where condition
                            select new DocBInfoModel
                            {
                                Id = d.Id,
                                InventoryId = d.InventoryId.Value,
                                AccountId = d.AssignedAccountId ?? Guid.Empty,
                                Status = (int)d.Status,
                                DocType = (int)d.DocType,
                                DocCode = d.DocCode ?? string.Empty,
                                ModelCode = d.ModelCode ?? string.Empty,
                                MachineModel = d.MachineModel ?? string.Empty,
                                MachineType = d.MachineType ?? string.Empty,
                                LineName = d.LineName ?? string.Empty,
                                LineType = DocCMachineModel.GetDisplayLineType(d.LineType),
                                StageNumber = d.StageNumber ?? string.Empty,
                                StageName = d.StageName ?? string.Empty,
                                Note = d.Note ?? string.Empty,

                                InventoryBy = d.InventoryBy ?? string.Empty,
                                AuditedBy = d.AuditBy ?? string.Empty,
                                ConfirmedBy = d.ConfirmBy ?? string.Empty,
                                DocStatusOrder = docStatusOrder,
                                ComponentCode = d.ComponentCode ?? string.Empty,
                                PositionCode = d.PositionCode ?? string.Empty
                            };

            if (docsQuery == null || docsQuery?.Any() == false)
            {
                return new ResponseModel<DocBListViewModel>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            var docs = docsQuery;
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                docs = docs.Where(x => x.AccountId == listDocBFilterModel.AccountId);
            }

            if (!string.IsNullOrEmpty(listDocBFilterModel.MachineModel))
            {
                docs = docs.Where(x => x.MachineModel == listDocBFilterModel.MachineModel);
            }
            if (!string.IsNullOrEmpty(listDocBFilterModel.MachineType))
            {
                docs = docs.Where(x => x.MachineType == listDocBFilterModel.MachineType);
            }
            if (!string.IsNullOrEmpty(listDocBFilterModel.ModelCode))
            {
                docs = docs.Where(x => x.ModelCode == listDocBFilterModel.ModelCode);
            }

            if (!string.IsNullOrEmpty(listDocBFilterModel.LineName))
            {
                docs = docs.Where(x => x.LineName == listDocBFilterModel.LineName);
            }

            //Lấy ra trạng thái hoàn thành tổng thể: 5/10 => 5: số lượng có status: chờ xác nhận, đã xác nhận, giám sát đạt, giám sát không đạt
            //                                            => 10: các trạng thái còn lại trừ chưa tiếp nhận và không kiểm kê

            var finishedDocsCount = 0;
            var totalDocsCount = 0;
            DocBListViewModel resultModel = new();
            //Tiến trình phiếu: VD: Đã kiểm kê: 5/10
            if (listDocBFilterModel.ActionType == InventoryActionType.Inventory)
            {
                finishedDocsCount = docs.Where(x => x.Status == (int)InventoryDocStatus.WaitingConfirm
                                                    || x.Status == (int)InventoryDocStatus.Confirmed
                                                    || x.Status == (int)InventoryDocStatus.AuditPassed
                                                    || x.Status == (int)InventoryDocStatus.AuditFailed
                                                    )?.Count() ?? 0;

                totalDocsCount = docs.Count();

                var notInventroyYetStatusDocs = docs?.Where(x => x.Status == (int)InventoryDocStatus.NotInventoryYet);
                if (notInventroyYetStatusDocs.Any())
                {
                    resultModel.DocBInfoModels = notInventroyYetStatusDocs?.OrderBy(x => x.PositionCode)?.Skip((listDocBFilterModel.PageNum - 1) * listDocBFilterModel.PageSize)?.Take(listDocBFilterModel.PageSize)?.ToList();
                }
                resultModel.FinishCount = finishedDocsCount;
                resultModel.TotalCount = totalDocsCount;

            }
            else if (listDocBFilterModel.ActionType == InventoryActionType.ConfirmInventory)
            {
                finishedDocsCount = docs.Where(x => x.Status == (int)InventoryDocStatus.Confirmed
                                                    || x.Status == (int)InventoryDocStatus.AuditPassed
                                                    || x.Status == (int)InventoryDocStatus.AuditFailed
                                                    )?.Count() ?? 0;

                totalDocsCount = docs.Count();

                var waitingConfirmStatusDocs = docs?.Where(x => x.Status == (int)InventoryDocStatus.WaitingConfirm);
                if (waitingConfirmStatusDocs.Any())
                {
                    resultModel.DocBInfoModels = waitingConfirmStatusDocs?.OrderBy(x => x.PositionCode)?.Skip((listDocBFilterModel.PageNum - 1) * listDocBFilterModel.PageSize)?.Take(listDocBFilterModel.PageSize)?.ToList();
                }

                resultModel.FinishCount = finishedDocsCount;
                resultModel.TotalCount = totalDocsCount;

            }

            return new ResponseModel<DocBListViewModel>
            {
                Code = StatusCodes.Status200OK,
                Data = resultModel
            };
        }

        public async Task<ResponseModel<DocAEListViewModel>> GetDocsAE(ListDocAEFilterModel listDocAEFilterModel)
        {
            if (listDocAEFilterModel == null)
            {
                return new ResponseModel<DocAEListViewModel>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Điều kiện lọc không hợp lệ."
                };
            }
            //20240910: Nếu tài khoản vai trò là Xúc Tiến thì được xem hoặc cập nhật kiểm kê:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();

            var docsQuery = from d in _inventoryContext.InventoryDocs.AsNoTracking()
                            let condition = ExcludeDocStatus.Contains(d.Status) == false
                                    && (d.DocType == InventoryDocType.A || d.DocType == InventoryDocType.E)
                                    && d.InventoryId == listDocAEFilterModel.InventoryId
                            //&& d.AssignedAccountId == listDocAEFilterModel.AccountId

                            let docStatusOrder = (int)d.Status == (int)InventoryDocStatus.AuditPassed ? (int)InventoryDocStatus.AuditFailed :
                                               (int)d.Status == (int)InventoryDocStatus.AuditFailed ? (int)InventoryDocStatus.AuditPassed :
                                               (int)d.Status

                            where condition
                            select new DocAEInfoModel
                            {
                                Id = d.Id,
                                InventoryId = d.InventoryId.Value,
                                AccountId = d.AssignedAccountId ?? Guid.Empty,
                                Status = (int)d.Status,
                                DocType = (int)d.DocType,
                                DocCode = d.DocCode ?? string.Empty,
                                ModelCode = d.ModelCode ?? string.Empty,
                                MachineModel = d.MachineModel ?? string.Empty,
                                MachineType = d.MachineType ?? string.Empty,
                                LineName = d.LineName ?? string.Empty,
                                LineType = DocCMachineModel.GetDisplayLineType(d.LineType),
                                StageNumber = d.StageNumber ?? string.Empty,
                                StageName = d.StageName ?? string.Empty,
                                Note = d.Note ?? string.Empty,

                                InventoryBy = d.InventoryBy ?? string.Empty,
                                AuditedBy = d.AuditBy ?? string.Empty,
                                ConfirmedBy = d.ConfirmBy ?? string.Empty,
                                DocStatusOrder = docStatusOrder,
                                ComponentCode = d.ComponentCode ?? string.Empty,
                                PositionCode = d.PositionCode ?? string.Empty
                            };

            if (docsQuery == null || docsQuery?.Any() == false)
            {
                return new ResponseModel<DocAEListViewModel>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            var docs = docsQuery;
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                docs = docs.Where(x => x.AccountId == listDocAEFilterModel.AccountId);
            }

            //Lấy ra trạng thái hoàn thành tổng thể: 5/10 => 5: số lượng có status: chờ xác nhận, đã xác nhận, giám sát đạt, giám sát không đạt
            //                                            => 10: các trạng thái còn lại trừ chưa tiếp nhận và không kiểm kê

            var finishedDocsCount = 0;
            var totalDocsCount = 0;
            DocAEListViewModel resultModel = new();
            //Tiến trình phiếu: VD: Đã kiểm kê: 5/10
            if (listDocAEFilterModel.ActionType == InventoryActionType.Inventory)
            {
                finishedDocsCount = docs.Where(x => x.Status == (int)InventoryDocStatus.WaitingConfirm
                                                    || x.Status == (int)InventoryDocStatus.Confirmed
                                                    || x.Status == (int)InventoryDocStatus.AuditPassed
                                                    || x.Status == (int)InventoryDocStatus.AuditFailed
                                                    )?.Count() ?? 0;

                totalDocsCount = docs.Count();

                var notInventroyYetStatusDocs = docs?.Where(x => x.Status == (int)InventoryDocStatus.NotInventoryYet);
                if (notInventroyYetStatusDocs.Any())
                {
                    resultModel.DocAEInfoModels = notInventroyYetStatusDocs?.OrderBy(x => x.PositionCode)?.Skip((listDocAEFilterModel.PageNum - 1) * listDocAEFilterModel.PageSize)?.Take(listDocAEFilterModel.PageSize)?.ToList();
                }
                resultModel.FinishCount = finishedDocsCount;
                resultModel.TotalCount = totalDocsCount;
            }
            else if (listDocAEFilterModel.ActionType == InventoryActionType.ConfirmInventory)
            {
                finishedDocsCount = docs.Where(x => x.Status == (int)InventoryDocStatus.Confirmed
                                                    || x.Status == (int)InventoryDocStatus.AuditPassed
                                                    || x.Status == (int)InventoryDocStatus.AuditFailed
                                                    )?.Count() ?? 0;

                totalDocsCount = docs.Count();

                var waitingConfirmStatusDocs = docs?.Where(x => x.Status == (int)InventoryDocStatus.WaitingConfirm);
                if (waitingConfirmStatusDocs.Any())
                {
                    resultModel.DocAEInfoModels = waitingConfirmStatusDocs?.OrderBy(x => x.PositionCode)?.Skip((listDocAEFilterModel.PageNum - 1) * listDocAEFilterModel.PageSize)?.Take(listDocAEFilterModel.PageSize)?.ToList();
                }
                resultModel.FinishCount = finishedDocsCount;
                resultModel.TotalCount = totalDocsCount;

            }

            return new ResponseModel<DocAEListViewModel>
            {
                Code = StatusCodes.Status200OK,
                Data = resultModel
            };
        }

        public async Task<ResponseModel> ScanDocB(ScanDocBFilterModel model, bool isErrorInvestigation)
        {
            var inventoryId = Guid.Parse(model.InventoryId);
            var accountId = Guid.Parse(model.AccountId);

            var checkAccountTypeAndRole = _httpContext.UserFromContext();

            //Kiểm tra mã linh kiện có tồn tại hay không
            var anyComponentCode = await _inventoryContext.InventoryDocs?.AsNoTracking()
                                                          ?.AnyAsync(x => x.InventoryId.Value == inventoryId
                                                                       && x.ComponentCode == model.ComponentCode
                                                                       && ExcludeDocStatus.Contains(x.Status) == false
                                                                       && x.DocType == InventoryDocType.B);
            if (anyComponentCode == false)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.InventoryNotFoundComponentCode,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.InventoryNotFoundComponentCode)
                };
            }


            //IQueryable<InventoryDoc> inventoryDocs;

            //Mã linh kiện này không nằm trong danh sách thực hiện kiểm kê của bạn
            //var assignedInventories = _inventoryContext.InventoryDocs?.AsNoTracking()
            //                                              ?.Where(x => x.InventoryId.Value == inventoryId
            //                                                        && x.AssignedAccountId.Value == accountId
            //                                                        && x.ComponentCode == model.ComponentCode
            //                                                        && ExcludeDocStatus.Contains(x.Status) == false
            //                                                        && x.DocType == InventoryDocType.B);

            //if (assignedInventories == null || !assignedInventories.Any())
            //{
            //    return new ResponseModel
            //    {
            //        Code = (int)HttpStatusCodes.ComponentNotAssigned,
            //        Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentNotAssigned)
            //    };
            //}

            var inventoryDocsQuery = _inventoryContext.InventoryDocs?.AsNoTracking()
                                                          ?.Where(x => x.InventoryId.Value == inventoryId
                                                                    //&& x.AssignedAccountId.Value == accountId
                                                                    && x.ComponentCode == model.ComponentCode
                                                                    //&& x.MachineModel == model.MachineModel
                                                                    //&& x.MachineType == model.MachineType
                                                                    //&& x.ModelCode == model.ModelCode
                                                                    && ExcludeDocStatus.Contains(x.Status) == false
                                                                    && x.DocType == InventoryDocType.B);

            var inventoryDocs = inventoryDocsQuery;

            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2 && !isErrorInvestigation)
            {
                inventoryDocs = inventoryDocs.Where(x => x.AssignedAccountId.Value == accountId);
            }

            if (!string.IsNullOrEmpty(model.MachineModel))
            {
                inventoryDocs = inventoryDocs.Where(x => x.MachineModel == model.MachineModel);
            }

            if (!string.IsNullOrEmpty(model.MachineType))
            {
                inventoryDocs = inventoryDocs.Where(x => x.MachineType == model.MachineType);
            }

            if (!string.IsNullOrEmpty(model.ModelCode))
            {
                inventoryDocs = inventoryDocs.Where(x => x.ModelCode == model.ModelCode);
            }

            if (!string.IsNullOrEmpty(model.LineName))
            {
                inventoryDocs = inventoryDocs.Where(x => x.LineName == model.LineName);
            }

            if (inventoryDocs == null || !inventoryDocs.Any())
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.ScanDocBNotFound,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ScanDocBNotFound)
                };
            }

            //Nếu là xác nhận kiểm kê thì check có phiếu hợp lệ đã đúng trạng thái là đã thực hiện kiểm kê chưa.
            if (model.ActionType == InventoryActionType.ConfirmInventory)
            {
                var totalDocsCount = inventoryDocs.Count();
                if (totalDocsCount > 1)
                {
                    var inventoriedDocuments = inventoryDocs.Where(x => (int)x.Status == (int)InventoryDocStatus.WaitingConfirm || (int)x.Status == (int)InventoryDocStatus.MustEdit
                                                                                || (int)x.Status == (int)InventoryDocStatus.Confirmed);
                    inventoryDocs = inventoriedDocuments;
                    if (inventoryDocs == null || !inventoryDocs.Any())
                    {
                        return new ResponseModel
                        {
                            Code = (int)HttpStatusCodes.ComponentCodeIsNotInventoryYetOrAudited,
                            Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentCodeIsNotInventoryYetOrAudited)
                        };
                    }
                }
                else
                {
                    var singleDoc = inventoryDocs.FirstOrDefault();
                    if ((int)singleDoc.Status == (int)InventoryDocStatus.WaitingConfirm || (int)singleDoc.Status == (int)InventoryDocStatus.MustEdit
                                                                        || (int)singleDoc.Status == (int)InventoryDocStatus.Confirmed)
                    {
                        inventoryDocs = new[] { singleDoc }.AsQueryable();
                    }
                    else if ((int)singleDoc.Status == (int)InventoryDocStatus.AuditFailed || (int)singleDoc.Status == (int)InventoryDocStatus.AuditPassed)
                    {
                        return new ResponseModel
                        {
                            Code = (int)HttpStatusCodes.ComponentCodeIsAudited,
                            Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentCodeIsAudited)
                        };
                    }
                    else if ((int)singleDoc.Status == (int)InventoryDocStatus.NotInventoryYet)
                    {
                        return new ResponseModel
                        {
                            Code = (int)HttpStatusCodes.ComponentNotInventoryYet,
                            Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentNotInventoryYet)
                        };
                    }
                }
            }


            var convertedInventoryDocs = inventoryDocs
                                                .Select(doc => new InventoryDocViewModel
                                                {
                                                    Id = doc.Id,
                                                    InventoryId = doc.InventoryId.Value,
                                                    AssignedAccountId = doc.AssignedAccountId ?? Guid.Empty,
                                                    ComponentCode = doc.ComponentCode ?? string.Empty,
                                                    ComponentName = doc.ComponentName ?? string.Empty,
                                                    DocCode = doc.DocCode ?? string.Empty,
                                                    DocType = (int)doc.DocType,
                                                    Note = doc.Note ?? string.Empty,
                                                    PositionCode = doc.PositionCode ?? string.Empty,
                                                    Status = (int)doc.Status,
                                                    SaleOrderNo = doc.SalesOrderNo ?? string.Empty,
                                                    InventoryBy = doc.InventoryBy ?? string.Empty,
                                                    AuditedBy = doc.AuditBy ?? string.Empty,
                                                    ConfirmedBy = doc.ConfirmBy ?? string.Empty
                                                }).ToList();

            var docs = from doc in convertedInventoryDocs
                       join dt in _inventoryContext.DocOutputs.AsNoTracking() on doc.Id equals dt.InventoryDocId.Value into dtGroup
                       join h in _inventoryContext.DocHistories.AsNoTracking().Where(x => ExcludeDocStatus.Contains(x.Status) == false) on doc.Id equals h.InventoryDocId.Value into hGroup
                       select new
                       {
                           InventoryDoc = new InventoryDocViewModel
                           {
                               Id = doc.Id,
                               InventoryId = doc.InventoryId,
                               AssignedAccountId = doc.AssignedAccountId,
                               ComponentCode = doc.ComponentCode,
                               ComponentName = doc.ComponentName,
                               DocCode = doc.DocCode,
                               DocType = doc.DocType,
                               Note = doc.Note,
                               PositionCode = doc.PositionCode,
                               Status = doc.Status,
                               SaleOrderNo = doc.SaleOrderNo,
                               InventoryBy = doc?.InventoryBy ?? string.Empty,
                               AuditedBy = doc?.AuditedBy ?? string.Empty,
                               ConfirmedBy = doc?.ConfirmedBy ?? string.Empty
                           },
                           Components = dtGroup != null && dtGroup.Any() ? dtGroup.OrderBy(x => x.CreatedAt).Select(c => new DocComponentABE
                           {
                               Id = c.Id,
                               InventoryId = c.InventoryId,
                               InventoryDocId = c.InventoryDocId.Value,
                               QuantityOfBom = c.QuantityOfBom,
                               QuantityPerBom = c.QuantityPerBom
                           }) : new List<DocComponentABE>(),
                           Histories = hGroup != null && hGroup.Any() ? hGroup.OrderByDescending(x => x.CreatedAt).Select(h => new DocHistoriesModel
                           {
                               Id = h.Id,
                               InventoryId = h.InventoryId,
                               InventoryDocId = h.InventoryDocId.Value,
                               Action = (int)h.Action,
                               Comment = h.Comment,
                               EvicenceImg = string.IsNullOrEmpty(h.EvicenceImg) ? string.Empty : h.EvicenceImg,
                               EvicenceImgTitle = string.IsNullOrEmpty(h.EvicenceImg) ? string.Empty : Path.GetFileName(h.EvicenceImg),
                               ChangeLogModel = new ChangeLogModel
                               {
                                   IsChangeCDetail = h.IsChangeCDetail,
                                   NewQuantity = h.NewQuantity,
                                   OldQuantity = h.OldQuantity,
                                   NewStatus = (int)h.NewStatus,
                                   OldStatus = (int)h.OldStatus
                               },
                               CreatedAt = h.CreatedAt,
                               CreatedBy = h.CreatedBy,
                               Status = (int)h.Status
                           }) : new List<DocHistoriesModel>()
                       };

            if (docs == null || docs?.Any() == false)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = docs
            };
        }

        public async Task<ResponseModel<DocCListViewModel>> ListDocC(ListDocCFilterModel listDocCFilterModel)
        {
            if (listDocCFilterModel == null)
            {
                return new ResponseModel<DocCListViewModel>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Điều kiện lọc không hợp lệ."
                };
            }

            var checkAccountTypeAndRole = _httpContext.UserFromContext();

            var docsQuery = from d in _inventoryContext.InventoryDocs.AsNoTracking()
                            let condition = ExcludeDocStatus.Contains(d.Status) == false
                                    && d.DocType == InventoryDocType.C
                                    && d.InventoryId == listDocCFilterModel.InventoryId
                            //&& d.AssignedAccountId == listDocCFilterModel.AccountId
                            //&& d.MachineModel == listDocCFilterModel.MachineModel
                            //&& d.MachineType == listDocCFilterModel.MachineType
                            //&& d.LineName == listDocCFilterModel.LineName

                            let docStatusOrder = (int)d.Status == (int)InventoryDocStatus.AuditPassed ? (int)InventoryDocStatus.AuditFailed :
                                               (int)d.Status == (int)InventoryDocStatus.AuditFailed ? (int)InventoryDocStatus.AuditPassed :
                                               (int)d.Status

                            where condition
                            select new DocCInfoModel
                            {
                                Id = d.Id,
                                InventoryId = d.InventoryId.Value,
                                AccountId = d.AssignedAccountId ?? Guid.Empty,
                                Status = (int)d.Status,
                                DocType = (int)d.DocType,
                                DocCode = d.DocCode ?? string.Empty,
                                ModelCode = d.ModelCode ?? string.Empty,
                                MachineModel = d.MachineModel ?? string.Empty,
                                MachineType = d.MachineType ?? string.Empty,
                                LineName = d.LineName ?? string.Empty,
                                LineType = DocCMachineModel.GetDisplayLineType(d.LineType),
                                StageNumber = d.StageNumber ?? string.Empty,
                                StageName = d.StageName ?? string.Empty,
                                Note = d.Note ?? string.Empty,

                                InventoryBy = d.InventoryBy ?? string.Empty,
                                AuditedBy = d.AuditBy ?? string.Empty,
                                ConfirmedBy = d.ConfirmBy ?? string.Empty,
                                DocStatusOrder = docStatusOrder
                            };

            var docs = docsQuery;
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                docs = docs.Where(x => x.AccountId == listDocCFilterModel.AccountId);
            }

            if (!string.IsNullOrEmpty(listDocCFilterModel.MachineType))
            {
                docs = docs.Where(x => x.MachineType == listDocCFilterModel.MachineType);
            }

            if (!string.IsNullOrEmpty(listDocCFilterModel.MachineModel))
            {
                docs = docs.Where(x => x.MachineModel == listDocCFilterModel.MachineModel);
            }

            if (!string.IsNullOrEmpty(listDocCFilterModel.LineName))
            {
                docs = docs.Where(x => x.LineName == listDocCFilterModel.LineName);
            }

            if (!string.IsNullOrEmpty(listDocCFilterModel.ModelCode))
            {
                docs = docs.Where(x => x.ModelCode == listDocCFilterModel.ModelCode);
            }

            if (!string.IsNullOrEmpty(listDocCFilterModel.StageName))
            {
                docs = docs.Where(x => x.StageName == listDocCFilterModel.StageName);
            }

            if (docs == null || docs?.Any() == false)
            {
                return new ResponseModel<DocCListViewModel>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            //Lấy ra trạng thái hoàn thành tổng thể: 5/10 => 5: số lượng có status: chờ xác nhận, đã xác nhận, giám sát đạt, giám sát không đạt
            //                                            => 10: các trạng thái còn lại trừ chưa tiếp nhận và không kiểm kê

            var finishedDocsCount = 0;
            var totalDocsCount = 0;
            DocCListViewModel resultModel = new();
            //Tiến trình phiếu: VD: Đã kiểm kê: 5/10
            if (listDocCFilterModel.ActionType == InventoryActionType.Inventory)
            {
                finishedDocsCount = docs.Where(x => x.Status == (int)InventoryDocStatus.WaitingConfirm ||
                                                    x.Status == (int)InventoryDocStatus.MustEdit ||
                                                    x.Status == (int)InventoryDocStatus.Confirmed ||
                                                    x.Status == (int)InventoryDocStatus.AuditPassed ||
                                                    x.Status == (int)InventoryDocStatus.AuditFailed)
                                        ?.Count() ?? 0;

                totalDocsCount = docs.Count();

                //var notInventroyYetStatusDocs = docs?.Where(x => x.Status == (int)InventoryDocStatus.NotInventoryYet);
                //if (notInventroyYetStatusDocs.Any())
                {
                    resultModel.DocCInfoModels = await docs?.OrderBy(x => x.ModelCode)?.Skip((listDocCFilterModel.PageNum - 1) * listDocCFilterModel.PageSize)?.Take(listDocCFilterModel.PageSize)?.ToListAsync();
                }
                resultModel.FinishCount = finishedDocsCount;
                resultModel.TotalCount = totalDocsCount;
            }
            else if (listDocCFilterModel.ActionType == InventoryActionType.ConfirmInventory)
            {
                finishedDocsCount = docs.Where(x => x.Status == (int)InventoryDocStatus.Confirmed)
                                        ?.Count() ?? 0;

                totalDocsCount = docs.Count();

                if (totalDocsCount > 1)
                {
                    var waitingConfirmStatusDocs = docs?.Where(x => x.Status == (int)InventoryDocStatus.WaitingConfirm || x.Status == (int)InventoryDocStatus.MustEdit
                                                                            || x.Status == (int)InventoryDocStatus.Confirmed);

                    if (waitingConfirmStatusDocs == null || !waitingConfirmStatusDocs.Any())
                    {
                        return new ResponseModel<DocCListViewModel>
                        {
                            Code = (int)HttpStatusCodes.ComponentCodeIsNotInventoryYetOrAudited,
                            Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentCodeIsNotInventoryYetOrAudited)
                        };
                    }

                    if (waitingConfirmStatusDocs.Any())
                    {
                        resultModel.DocCInfoModels = waitingConfirmStatusDocs?.OrderBy(x => x.ModelCode)?.Skip((listDocCFilterModel.PageNum - 1) * listDocCFilterModel.PageSize)?.Take(listDocCFilterModel.PageSize)?.ToList();
                    }

                }
                else
                {
                    var singleDoc = docs.First();
                    if (singleDoc.Status == (int)InventoryDocStatus.WaitingConfirm || singleDoc.Status == (int)InventoryDocStatus.MustEdit
                                                                                    || singleDoc.Status == (int)InventoryDocStatus.Confirmed)
                    {
                        resultModel.DocCInfoModels = new List<DocCInfoModel> { singleDoc };
                    }
                    else if (singleDoc.Status == (int)InventoryDocStatus.AuditFailed || singleDoc.Status == (int)InventoryDocStatus.AuditPassed)
                    {
                        return new ResponseModel<DocCListViewModel>
                        {
                            Code = (int)HttpStatusCodes.ComponentCodeIsAudited,
                            Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentCodeIsAudited)
                        };
                    }
                    else if (singleDoc.Status == (int)InventoryDocStatus.NotInventoryYet)
                    {
                        return new ResponseModel<DocCListViewModel>
                        {
                            Code = (int)HttpStatusCodes.ComponentNotInventoryYet,
                            Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentNotInventoryYet)
                        };
                    }
                }

                resultModel.FinishCount = finishedDocsCount;
                resultModel.TotalCount = totalDocsCount;
            }

            return new ResponseModel<DocCListViewModel>
            {
                Code = StatusCodes.Status200OK,
                Data = resultModel
            };
        }

        public async Task<ResponseModel<InventoryDocumentImportResultDto>> ImportInventoryDocumentShip([FromForm] IFormFile file, Guid inventoryId)
        {
            var result = new ResponseModel<InventoryDocumentImportResultDto>(StatusCodes.Status200OK, new InventoryDocumentImportResultDto());
            if (file == null)
            {
                result.Code = StatusCodes.Status500InternalServerError;
                result.Message = "File import không tồn tại";
                return await Task.FromResult(result);
            }
            else if (!file.FileName.EndsWith(".xlsx") && file.ContentType != FileResponse.ExcelType)
            {
                result.Code = (int)HttpStatusCodes.InvalidFileExcel;
                result.Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo phiếu kiểm kê.";
                return await Task.FromResult(result);
            }
            //Assume that doc type A is existed and file have data with first row is header, second row is A data
            result.Data = await ImportDocShip(file, inventoryId);

            return result;
        }

        private async Task<InventoryDocumentImportResultDto> ImportDocShip([FromForm] IFormFile file, Guid inventoryId)
        {
            // first row after header is for Doc Type A

            using var memStreamForA = new MemoryStream();
            await file.CopyToAsync(memStreamForA);
            var checkDto = new CheckInventoryDocumentDto();
            var resultDto = new InventoryDocumentImportResultDto();
            var inventoryDocuments = new List<InventoryDoc>();
            using var sourcePackageForA = new ExcelPackage(memStreamForA);
            var sourceSheetForA = sourcePackageForA.Workbook.Worksheets.FirstOrDefault();
            var validRows = new List<int>();
            if (sourceSheetForA != null)
            {

                // check header
                var PlantIndexA = sourceSheetForA.GetColumnIndex(TypeA.Plant);
                var WarehouseLocationIndexA = sourceSheetForA.GetColumnIndex(TypeA.WarehouseLocation);
                var ComponentCodeIndexA = sourceSheetForA.GetColumnIndex(TypeA.ComponentCode);
                var ComponentNameIndexA = sourceSheetForA.GetColumnIndex(TypeA.ComponentName);
                var SONoIndexA = sourceSheetForA.GetColumnIndex(TypeA.SONo);
                var SOListIndexA = sourceSheetForA.GetColumnIndex(TypeA.SOList);
                var StorageBinIndexA = sourceSheetForA.GetColumnIndex(TypeE.StorageBin);
                var QuantityIndexA = sourceSheetForA.GetColumnIndex(TypeA.Quantity);
                var AssigneeIndexA = sourceSheetForA.GetColumnIndex(TypeA.Assignee);

                var requiredHeaderA = new[] {
                    PlantIndexA,
                    WarehouseLocationIndexA,
                    ComponentCodeIndexA,
                    ComponentNameIndexA,
                    SONoIndexA,
                    SOListIndexA,
                    StorageBinIndexA,
                    QuantityIndexA,
                    AssigneeIndexA,
                };
                if (requiredHeaderA.Any(x => x == -1))
                {
                    return new InventoryDocumentImportResultDto
                    {
                        Code = (int)HttpStatusCodes.InvalidFileExcel,
                        Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo phiếu kiểm kê."
                    };
                }

                var sourceSheet = sourcePackageForA.Workbook.Worksheets.FirstOrDefault();
                var totalRowsCount = sourceSheet.Dimension.End.Row > 1 ? sourceSheet.Dimension.End.Row - 1 : sourceSheet.Dimension.End.Row;
                var rows = Enumerable.Range(2, totalRowsCount).ToList(); // get list row for E doc type

                //Kiểm tra sai cột, thiếu cột so với file mẫu
                var PlantIndex = sourceSheet.GetColumnIndex(TypeE.Plant);
                var WarehouseLocationIndex = sourceSheet.GetColumnIndex(TypeA.WarehouseLocation);
                var ComponentCodeIndex = sourceSheet.GetColumnIndex(TypeE.ComponentCode);
                var ComponentNameIndex = sourceSheet.GetColumnIndex(TypeE.ComponentName);
                var StorageBinIndex = sourceSheet.GetColumnIndex(TypeE.StorageBin);
                var AssigneeIndex = sourceSheet.GetColumnIndex(TypeB.Assignee);
                var QuantityIndex = sourceSheet.GetColumnIndex(TypeA.Quantity);

                var requiredHeader = new[] {
                            PlantIndex,
                            WarehouseLocationIndex,
                            ComponentCodeIndex,
                            ComponentNameIndex,
                            StorageBinIndex,
                            AssigneeIndex,
                            QuantityIndex
                        };

                if (requiredHeader.Any(x => x == -1))
                {
                    return new InventoryDocumentImportResultDto
                    {
                        Code = (int)HttpStatusCodes.InvalidFileExcel,
                        Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành tạo phiếu kiểm kê."
                    };
                }

                var failCount = 0;
                //get last document number by type
                var lastDocNumberAndAssignee = await GetLastDocNumberAndAssignee(InventoryDocType.A, inventoryId);
                //get last document number by type
                var lastDocNumberAndAssigneeE = await GetLastDocNumberAndAssignee(InventoryDocType.E, inventoryId);
                checkDto.PlanLocationForBE = _inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryId).Select(x => $"{x.ComponentCode}{x.Plant}{x.WareHouseLocation}").Distinct().ToHashSet();

                //Get all Roles:
                var request = new RestRequest(Constants.Endpoint.Internal.Get_All_Roles_Department);
                request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
                request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

                var roles = await _identityRestClient.ExecuteGetAsync(request);

                var rolesModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<GetAllRoleWithUserNameModel>>>(roles.Content ?? string.Empty);

                var createDocumentRoles = rolesModel?.Data;

                var errorColumn = sourceSheetForA.Dimension.End.Column + 1;
                var inventoryDocCellDtoList = new List<InventoryDocCellDto>();

                InventoryDocCellDto errorRow = null;


                var listDataFromCell = GetListDataFromCell(sourceSheetForA);

                var listDocAOfShip = listDataFromCell.GroupBy(x => x.ComponentCode).Select(x => x.FirstOrDefault() != null ? new { x.FirstOrDefault().ComponentCode, x.FirstOrDefault().SONo, x.FirstOrDefault().StorageBin } : null).ToList();


                for (var row = sourceSheetForA.Dimension.Start.Row + 1; row <= sourceSheetForA.Dimension.End.Row; row++)
                {

                    try
                    {

                        var dataFromCell = GetDataFromCell(sourceSheetForA, row, nameof(TypeA), true);
                        if (listDocAOfShip.Any(x => x.ComponentCode == dataFromCell.ComponentCode && x.SONo == dataFromCell.SONo && x.StorageBin == dataFromCell.StorageBin))
                        {

                            int previousValidRow = validRows.Count;
                            HandleADocForShip(inventoryId, checkDto, inventoryDocuments, sourceSheetForA, validRows, ref failCount, lastDocNumberAndAssignee, createDocumentRoles, errorColumn, inventoryDocCellDtoList, ref errorRow, row, dataFromCell);
                            if (validRows.Count > previousValidRow)
                            {
                                var comWhLoc = $"{dataFromCell.ComponentCode}{dataFromCell.Plant}{dataFromCell.WarehouseLocation}";
                                if (!checkDto.PlanLocationForBE.Any(x => x == comWhLoc))
                                    checkDto.PlanLocationForBE.Add(comWhLoc);
                            }


                        }
                        else
                        {
                            dataFromCell = GetDataFromCell(sourceSheetForA, row, nameof(TypeE), true);

                            HandleEDocForShip(inventoryId, checkDto, inventoryDocuments, validRows, sourceSheetForA, failCount, lastDocNumberAndAssigneeE, createDocumentRoles, inventoryDocCellDtoList, row, dataFromCell);
                        }

                    }
                    catch (Exception exception)
                    {
                        var exMess = $"Exception - {exception.Message} at row {row}";
                        var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                        _logger.LogError($"Request error at {_httpContext.Request.Path} : {exMess}; {innerExMess}");
                        continue;
                    }
                }

                //AddDataToDb(inventoryDocuments);

                //Thêm tiêu đồ cột nội dung lỗi trong file excel
                if (failCount > 0)
                {
                    sourceSheetForA.Cells[1, sourceSheetForA.Dimension.Columns].Value = TypeA.ErrorContent;
                }

                resultDto.FailCount = failCount;
                resultDto.SuccessCount = inventoryDocuments.Count;

                using (var connection = _inventoryContext.Database.GetDbConnection())
                {

                    //disable FK check befor insert data
                    var getFKQuery = "SELECT OBJECT_NAME(parent_object_id) AS [Table], [name] AS [ForeignKey] FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('DocOutputs') OR  parent_object_id = OBJECT_ID('DocHistories') OR  parent_object_id = OBJECT_ID('HistoryOutputs');";
                    var listFK = await connection.QueryAsync<ForeignKeyDto>(getFKQuery);
                    var disableFKQuery = new StringBuilder();
                    var enableFKQuery = new StringBuilder();
                    foreach (var fk in listFK)
                    {
                        disableFKQuery.AppendLine($"ALTER TABLE {fk.Table} NOCHECK CONSTRAINT {fk.ForeignKey};");
                        enableFKQuery.AppendLine($"ALTER TABLE {fk.Table} WITH CHECK CHECK CONSTRAINT {fk.ForeignKey};");
                    }
                    await connection.ExecuteAsync(disableFKQuery.ToString());

                    //insert data to DocOutputs, DocHistories, HistoryOutputs

                    var docOutputs = new List<DocOutput>();
                    var docHistories = new List<DocHistory>();
                    var historyOutputs = new List<HistoryOutput>();

                    foreach (var invDoc in inventoryDocuments)
                    {
                        var docOutput = new DocOutput
                        {
                            InventoryDocId = invDoc.Id,
                            InventoryId = invDoc.InventoryId.Value,
                            QuantityOfBom = 1,
                            QuantityPerBom = invDoc.Quantity,
                            CreatedAt = DateTime.Now,
                            CreatedBy = _httpContext.UserFromContext().Username,
                            Id = Guid.NewGuid()
                        };
                        docOutputs.Add(docOutput);
                        var docHistory = new DocHistory
                        {
                            InventoryDocId = invDoc.Id,
                            InventoryId = invDoc.InventoryId.Value,
                            Action = DocHistoryActionType.Confirm,
                            Status = InventoryDocStatus.Confirmed,
                            CreatedAt = DateTime.Now,
                            CreatedBy = _httpContext.UserFromContext().Username,
                            NewQuantity = invDoc.Quantity,
                            Id = Guid.NewGuid()
                        };
                        docHistories.Add(docHistory);


                    }
                    foreach (var docHistory in docHistories)
                    {
                        var historyOutput = new HistoryOutput
                        {
                            InventoryId = inventoryId,
                            DocHistoryId = docHistory.Id,
                            QuantityOfBom = 1,
                            QuantityPerBom = docHistory.NewQuantity,
                            CreatedAt = DateTime.Now,
                            CreatedBy = _httpContext.UserFromContext().Username,
                            Id = Guid.NewGuid()
                        };
                        historyOutputs.Add(historyOutput);
                    }
                    AddDataToDb(inventoryDocuments, docOutputs, docHistories, historyOutputs);


                    //enable FK check after insert data
                    await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(x => EnableCheckConstraint(enableFKQuery.ToString()));


                    await connection.DisposeAsync();
                }
                //Thêm tiêu đồ cột nội dung lỗi trong file excel
                if (failCount > 0)
                {
                    sourceSheet.Cells[1, sourceSheet.Dimension.Columns].Value = TypeE.ErrorContent;
                }


                resultDto.FailCount = failCount;
                resultDto.SuccessCount = inventoryDocuments.Count;


                var errorWorksheet = sourcePackageForA.Workbook.Worksheets.Copy(sourceSheet.Name, sourceSheet.Name + " lỗi");
                sourcePackageForA.Workbook.Worksheets.Delete(sourceSheet.Name);
                //foreach (var row in validRows)
                //{
                //    errorWorksheet.Cells[row, 1, row, errorWorksheet.Dimension.End.Column].Clear();
                //    errorWorksheet.Row(row).Hidden = true;
                //}
                foreach (var row in validRows.OrderByDescending(r => r))
                {
                    errorWorksheet.DeleteRow(row);
                }
                //if (errorRow != null)
                //{
                //    errorWorksheet.InsertRow(2, 1);
                //    //errorWorksheet.Cells[2, 1].Value = errorRow.Plant;
                //    //errorWorksheet.Cells[2, 2].Value = errorRow.WarehouseLocation;
                //    //errorWorksheet.Cells[2, 3].Value = errorRow.SONo;
                //    //errorWorksheet.Cells[2, 4].Value = errorRow.SOList;
                //    //errorWorksheet.Cells[2, 4].Value = errorRow.ComponentCode;
                //    //errorWorksheet.Cells[2, 6].Value = errorRow.ComponentName;
                //    //errorWorksheet.Cells[2, 7].Value = errorRow.StorageBin;
                //    //errorWorksheet.Cells[2, 8].Value = errorRow.Quantity;
                //    //errorWorksheet.Cells[2, 9].Value = errorRow.Assignee;
                //    //errorWorksheet.Cells[2, 10].Value = errorRow.Note;
                //    errorWorksheet.Cells["A2"].Value = sourceSheet.Cells["A2"].Value;
                //}



                resultDto.Result = sourcePackageForA.GetAsByteArray();

            }

            return resultDto;
        }

        private async Task EnableCheckConstraint(string queryString)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();

                await Task.Delay(30 * 1000);
                foreach (var item in queryString.Split(';'))
                {
                    context.Database.ExecuteSqlRaw(item);
                }
            }

        }
        private class ForeignKeyDto
        {
            public string Table { get; set; }
            public string ForeignKey { get; set; }
        }
        private void HandleEDocForShip(Guid inventoryId, CheckInventoryDocumentDto checkDto, List<InventoryDoc> inventoryDocuments, List<int> validRows, ExcelWorksheet sourceSheet, int failCount, InventoryDocAndUserDto lastDocNumberAndAssigneeE, IEnumerable<GetAllRoleWithUserNameModel> createDocumentRoles, List<InventoryDocCellDto> inventoryDocCellDtoList, int row, InventoryDocCellDto dataFromCell)
        {
            if (dataFromCell != null)
            {
                inventoryDocCellDtoList.Add(dataFromCell);
            }


            //check for all blank rows
            if (CheckRequiredColumn(true, dataFromCell.Plant, dataFromCell.WarehouseLocation, dataFromCell.ComponentCode, dataFromCell.StockType, dataFromCell.ModelCode, dataFromCell.StorageBin, dataFromCell.Assignee))
            {
                if (row < sourceSheet.Dimension.Rows)
                {
                    sourceSheet.Row(row).Hidden = true;

                }
                return;

            }

            //validate data
            var validateData = ValidateCellData(inventoryId, createDocumentRoles, lastDocNumberAndAssigneeE, dataFromCell, sourceSheet, row, checkDto, inventoryDocCellDtoList, nameof(TypeE), sourceSheet.Dimension.End.Column, true);
            if (validateData.IsValid)
            {
                validRows.Add(row);
            }
            if (!validateData.IsValid)
            {
                failCount += validateData.FailCount;
                return;
            }
            SetDataToEntites(inventoryId, lastDocNumberAndAssigneeE, inventoryDocuments, dataFromCell, nameof(TypeE), true);
            return;
        }

        private void HandleADocForShip(Guid inventoryId, CheckInventoryDocumentDto checkDto, List<InventoryDoc> inventoryDocuments, ExcelWorksheet sourceSheetForA, List<int> validRows, ref int failCount, InventoryDocAndUserDto lastDocNumberAndAssignee, IEnumerable<GetAllRoleWithUserNameModel> createDocumentRoles, int errorColumn, List<InventoryDocCellDto> inventoryDocCellDtoList, ref InventoryDocCellDto errorRow, int row, InventoryDocCellDto dataFromCell)
        {
            //check for all blank rows
            if (CheckRequiredColumn(true, dataFromCell.Plant, dataFromCell.WarehouseLocation, dataFromCell.ComponentName, dataFromCell.ComponentCode, dataFromCell.PositionCode
                , dataFromCell.SONo, dataFromCell.SOList, dataFromCell.StorageBin, dataFromCell.Assignee))
            {
                if (row < sourceSheetForA.Dimension.Rows)
                {
                    sourceSheetForA.Row(row).Hidden = true;

                }
                return;

            }

            if (dataFromCell != null)
            {
                inventoryDocCellDtoList.Add(dataFromCell);
            }

            //validate data
            var validateData = ValidateCellData(inventoryId, createDocumentRoles, lastDocNumberAndAssignee, dataFromCell, sourceSheetForA, row, checkDto, inventoryDocCellDtoList, nameof(TypeA), errorColumn, true);
            if (validateData.IsValid)
            {
                validRows.Add(row);
            }

            if (!validateData.IsValid)
            {
                failCount += validateData.FailCount;

                errorRow = dataFromCell;

                return;
            }

            SetDataToEntites(inventoryId, lastDocNumberAndAssignee, inventoryDocuments, dataFromCell, nameof(TypeA), true);
        }

        /// <summary>
        /// For doc type ship only
        /// </summary>
        /// <param name="sourceSheetForA"></param>
        /// <returns></returns>
        private IEnumerable<InventoryDocCellDto> GetListDataFromCell(ExcelWorksheet sourceSheetForA)
        {
            for (var row = sourceSheetForA.Dimension.Start.Row + 1; row <= sourceSheetForA.Dimension.End.Row; row++)
            {
                yield return new InventoryDocCellDto
                {
                    ComponentCode = GetCellValue(sourceSheetForA, row, TypeA.ComponentCode),
                    SONo = GetCellValue(sourceSheetForA, row, TypeA.SONo),
                    StorageBin = GetCellValue(sourceSheetForA, row, TypeE.StorageBin)
                };
            }
        }

        public async Task<ResponseModel> CheckValidAuditTarget(Guid inventoryId, Guid accountId, string componentCode)
        {
            //check component code exist in audit target
            var cpntCodeExistedInAT = await _inventoryContext.AuditTargets.AnyAsync(x => x.InventoryId == inventoryId && x.ComponentCode == componentCode);
            if (cpntCodeExistedInAT)
            {
                var isAssignedForOther = await _inventoryContext.AuditTargets.AnyAsync(x => x.InventoryId == inventoryId && x.AssignedAccountId != accountId && x.ComponentCode == componentCode);

                return new ResponseModel
                {
                    Code = isAssignedForOther ? (int)HttpStatusCodes.ComponentNotInAuditTarget : StatusCodes.Status200OK,
                    Message = isAssignedForOther ? "Linh kiện này đã được phân công cho người khác." : string.Empty
                };
            }

            var accountLocation = from acc in _inventoryContext.InventoryAccounts.AsNoTracking()
                                  join accLoc in _inventoryContext.AccountLocations.AsNoTracking() on acc.Id equals accLoc.AccountId
                                  join loc in _inventoryContext.InventoryLocations.AsNoTracking() on accLoc.LocationId equals loc.Id
                                  where acc.UserId == accountId && loc.IsDeleted == false
                                  select new
                                  {
                                      LocationName = loc.Name,
                                      loc.DepartmentName
                                  };

            var invDocs = from doc in _inventoryContext.InventoryDocs.AsNoTracking()
                          where doc.InventoryId == inventoryId && doc.ComponentCode == componentCode && !string.IsNullOrEmpty(doc.DepartmentName) && !string.IsNullOrEmpty(doc.LocationName)
                          select new
                          {
                              doc.ComponentCode,
                              doc.LocationName,
                              doc.DepartmentName,
                          };
            var existedInLocation = from loc in accountLocation
                                    join inv in invDocs on new { loc.LocationName, loc.DepartmentName } equals new { inv.LocationName, inv.DepartmentName }
                                    select new
                                    {
                                        inv.ComponentCode,
                                        inv.LocationName,
                                        inv.DepartmentName
                                    };

            if (!existedInLocation.Any(x => x.ComponentCode == componentCode))
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.ComponentNotInLocation,
                    Message = "Linh kiện này thuộc khu vực giám sát khác."
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK
            };


        }


        [GeneratedRegex("(^[A-Za-z0-9]{4})(P|F|T|D|0)(0(?=a)|0(?=b)|0(?=c)|[A-Z](?=d)|[A-Z](?=e))(a|b|c|d|e)((?<!e)(?!000)[0-9]{3}|(?<=e)00[1-2])")]
        private static partial Regex ModelCodeRegex();

        [GeneratedRegex("(^[A-Za-z0-9]{4})(P|F|T|D)([A-Z])(e)((?!000)[0-9]{3})")]
        private static partial Regex FinishGroupRegex();

        [GeneratedRegex("(^[A-Za-z0-9]{4})(P|F|T|D)(0)(a|b|c)((?!000)[0-9]{3})")]
        private static partial Regex ShareGroupRegex();

        [GeneratedRegex("(^[A-Za-z0-9]{4})(P|F|T|D)([A-Z])(d|e)((?!000)[0-9]{3})")]
        private static partial Regex AssenblyAndFinishGroupRegex();

        [GeneratedRegex("(^[A-Za-z0-9]{4})(P|F|T|D)([A-Z])(a|b|c|d|e)((?!000)[0-9]{3})")]
        private static partial Regex MainLineGroupRegex();

        [GeneratedRegex("(^[A-Za-z0-9]{4})(0)(0)(a|b|c)((?!000)[0-9]{3})")]
        private static partial Regex ShareGroupByModelRegex();

        [GeneratedRegex("([A-Z0-9]{9})")]
        private static partial Regex MaterialCodeRegex();

        [GeneratedRegex("(?!0000)(^[A-Za-z0-9]{4})")]
        private static partial Regex MachineModelRegex();

        [GeneratedRegex("([ABEC]{1,2}\\d{4})(\\d{5})")]
        private static partial Regex DocCodeRegex();

        [GeneratedRegex("[A-E]{1,4}")]
        private static partial Regex DocTypeRegex();

        [GeneratedRegex("([A-Z0-9]{9,12})")]
        private static partial Regex MaterialCodeForAuditTargetRegex();

    }
}
