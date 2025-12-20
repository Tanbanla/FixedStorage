using BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation;
using BIVN.FixedStorage.Services.Common.API.Dto.Inventory;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;
using Microsoft.OpenApi.Extensions;

namespace WebApp.Application.Services.ErrorInvestigationWeb
{
    public class ErrorInvestigationWebService : IErrorInvestigationWebService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<ErrorInvestigationWebService> _logger;

        public ErrorInvestigationWebService(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment hostingEnvironment, ILogger<ErrorInvestigationWebService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        public async Task<ResponseModel> ExportListErrorInvestigation(IEnumerable<ListErrorInvestigationWebDto> model, IEnumerable<ErrorCategoryManagementDto> errorCategories)
        {
            if (model == null || !model.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Data = null,
                    Message = "Không có dữ liệu"
                };
            }
            // Tạo tệp Excel template mới
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh Sách Sai Số");

                // Điều chỉnh kiểu chữ và cỡ chữ của trang tính
                worksheet.Cells.Style.WrapText = false;

                worksheet.Cells[1, 4].Style.Font.Bold = true;
                worksheet.Cells[1, 4].Value = "DANH SÁCH SAI SỐ";

                // Gán tiêu đề cột cho trang tính
                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "STT";
                worksheet.Column(1).Width = 10;

                worksheet.Cells[2, 2].Style.Font.Bold = true;
                worksheet.Cells[2, 2].Value = "Đợt kiểm kê";
                worksheet.Column(2).Width = 30;

                worksheet.Cells[2, 3].Style.Font.Bold = true;
                worksheet.Cells[2, 3].Value = "PLANT";
                worksheet.Column(3).Width = 30;

                worksheet.Cells[2, 4].Style.Font.Bold = true;
                worksheet.Cells[2, 4].Value = "WH LOC.";
                worksheet.Column(4).Width = 30;

                worksheet.Cells[2, 5].Style.Font.Bold = true;
                worksheet.Cells[2, 5].Value = "MÃ LINH KIỆN";
                worksheet.Column(5).Width = 30;

                worksheet.Cells[2, 6].Style.Font.Bold = true;
                worksheet.Cells[2, 6].Value = "VỊ TRÍ";
                worksheet.Column(6).Width = 30;

                worksheet.Cells[2, 7].Style.Font.Bold = true;
                worksheet.Cells[2, 7].Value = "SỐ LƯỢNG KIỂM KÊ";
                worksheet.Column(7).Width = 30;
                //worksheet.Column(6).AutoFit();

                worksheet.Cells[2, 8].Style.Font.Bold = true;
                worksheet.Cells[2, 8].Value = "SỐ LƯỢNG HỆ THỐNG";
                worksheet.Column(8).Width = 30;

                worksheet.Cells[2, 9].Style.Font.Bold = true;
                worksheet.Cells[2, 9].Value = "CHÊNH LỆCH";
                worksheet.Column(9).Width = 30;

                worksheet.Cells[2, 10].Style.Font.Bold = true;
                worksheet.Cells[2, 10].Value = "GIÁ TIỀN";
                worksheet.Column(10).Width = 30;

                //worksheet.Cells[2, 11].Style.Font.Bold = true;
                //worksheet.Cells[2, 11].Value = "ĐƠN GIÁ";
                //worksheet.Column(11).Width = 30;

                //worksheet.Cells[2, 12].Style.Font.Bold = true;
                //worksheet.Cells[2, 12].Value = "CHÊNH LỆCH ABS";
                //worksheet.Column(12).Width = 30;
                //worksheet.Cells[2, 13].Style.Font.Bold = true;
                //worksheet.Cells[2, 13].Value = "GIÁ TIỀN (ABS)";
                //worksheet.Column(13).Width = 30;
                worksheet.Cells[2, 11].Style.Font.Bold = true;
                worksheet.Cells[2, 11].Value = "TÀI KHOẢN PHÂN PHÁT";
                worksheet.Column(11).Width = 30;
                worksheet.Cells[2, 12].Style.Font.Bold = true;
                worksheet.Cells[2, 12].Value = "SỐ LƯỢNG ĐIỀU CHỈNH";
                worksheet.Column(12).Width = 30;
                worksheet.Cells[2, 13].Style.Font.Bold = true;
                worksheet.Cells[2, 13].Value = "PHÂN LOẠI LỖI";
                worksheet.Column(13).Width = 30;
                worksheet.Cells[2, 14].Style.Font.Bold = true;
                worksheet.Cells[2, 14].Value = "NGUYÊN NHÂN SAI SỐ";
                worksheet.Column(14).Width = 30;
                worksheet.Cells[2, 15].Style.Font.Bold = true;
                worksheet.Cells[2, 15].Value = "NGƯỜI ĐIỀU TRA";
                worksheet.Column(15).Width = 30;
                //worksheet.Cells[2, 16].Style.Font.Bold = true;
                //worksheet.Cells[2, 16].Value = "TỔNG SỐ LƯỢNG ĐIỀU TRA";
                //worksheet.Column(16).Width = 30;
                worksheet.Cells[2, 16].Style.Font.Bold = true;
                worksheet.Cells[2, 16].Value = "TRẠNG THÁI";
                worksheet.Column(16).Width = 30;
                worksheet.Cells[2, 17].Style.Font.Bold = true;
                worksheet.Cells[2, 17].Value = "TÊN LINH KIỆN";
                worksheet.Column(17).Width = 30;
                worksheet.Cells[2, 18].Style.Font.Bold = true;
                worksheet.Cells[2, 18].Value = "LỊCH SỬ ĐIỀU CHỈNH";
                worksheet.Column(18).Width = 30;
                worksheet.Cells[2, 19].Style.Font.Bold = true;
                worksheet.Cells[2, 19].Value = "GHI CHÚ";
                worksheet.Column(19).Width = 30;
                //worksheet.Column(7).AutoFit();

                int row = 3, index = 1;
                foreach (var item in model)
                {
                    worksheet.Cells[row, 1].Value = index;
                    worksheet.Cells[row, 2].Value = item.InventoryName ?? string.Empty;
                    worksheet.Cells[row, 3].Value = item.Plant ?? string.Empty;
                    worksheet.Cells[row, 4].Value = item.WHLoc ?? string.Empty;
                    worksheet.Cells[row, 5].Value = item.ComponentCode ?? string.Empty;
                    worksheet.Cells[row, 6].Value = item.Position ?? string.Empty;
                    worksheet.Cells[row, 7].Value = item.TotalQuantity.HasValue ? item.TotalQuantity.Value : 0;
                    worksheet.Cells[row, 8].Value = item.AccountQuantity.HasValue ? item.AccountQuantity.Value : 0;
                    worksheet.Cells[row, 9].Value = item.ErrorQuantity.HasValue ? item.ErrorQuantity.Value : 0;
                    worksheet.Cells[row, 10].Value = item.ErrorMoney.HasValue ? item.ErrorMoney.Value : 0;
                    //worksheet.Cells[row, 11].Value = item.UnitPrice.HasValue ? item.UnitPrice.Value : 0;
                    //worksheet.Cells[row, 12].Value = item.ErrorQuantityAbs.HasValue ? item.ErrorQuantityAbs.Value : 0;
                    //worksheet.Cells[row, 13].Value = item.ErrorMoneyAbs.HasValue ? item.ErrorMoneyAbs.Value : 0;
                    worksheet.Cells[row, 11].Value = item.AssigneeAccount ?? string.Empty;
                    worksheet.Cells[row, 12].Value = item.InvestigationQuantity.HasValue ? item.InvestigationQuantity.Value : 0;
                    worksheet.Cells[row, 13].Value = item.ErrorCategory.HasValue ? errorCategories?.FirstOrDefault(x => x.ErrorCategoryKey == item.ErrorCategory.Value.ToString())?.ErrorCategoryName : string.Empty;
                    worksheet.Cells[row, 14].Value = item.ErrorDetail ?? string.Empty;
                    worksheet.Cells[row, 15].Value = item.Investigator ?? string.Empty;
                    //worksheet.Cells[row, 16].Value = item.InvestigationTotal.HasValue ? item.InvestigationTotal.Value : 0;
                    worksheet.Cells[row, 16].Value = item.Status.GetDisplayName();
                    worksheet.Cells[row, 17].Value = item.ComponentName ?? string.Empty;
                    worksheet.Cells[row, 18].Value = item.InvestigationHistoryCount ?? string.Empty;
                    worksheet.Cells[row, 19].Value = item.NoteDocumentTypeA ?? string.Empty;

                    row++;
                    index++;
                }

                var exportData = await Task.Run(() => package.GetAsByteArray());
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Data = exportData,
                    Message = "Xuất Excel thành công!"
                };
            }
        }

        public async Task<ResponseModel> ExportListErrorInvestigationHistory(IEnumerable<ListErrorInvestigationHistoryWebDto> model)
        {
            if (model == null || !model.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Data = null,
                    Message = "Không có dữ liệu"
                };
            }

            // Tạo tệp Excel template mới
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh Sách Lịch Sử Điều Tra Sai Số");

                // Điều chỉnh kiểu chữ và cỡ chữ của trang tính
                worksheet.Cells.Style.WrapText = false;

                worksheet.Cells[1, 4].Style.Font.Bold = true;
                worksheet.Cells[1, 4].Value = "DANH SÁCH LỊCH SỬ ĐIỀU TRA SAI SỐ";

                // Gán tiêu đề cột cho trang tính
                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "STT";
                worksheet.Column(1).Width = 10;

                worksheet.Cells[2, 2].Style.Font.Bold = true;
                worksheet.Cells[2, 2].Value = "Đợt kiểm kê";
                worksheet.Column(2).Width = 30;

                worksheet.Cells[2, 3].Style.Font.Bold = true;
                worksheet.Cells[2, 3].Value = "PLANT";
                worksheet.Column(3).Width = 30;

                worksheet.Cells[2, 4].Style.Font.Bold = true;
                worksheet.Cells[2, 4].Value = "WH LOC.";
                worksheet.Column(4).Width = 30;

                worksheet.Cells[2, 5].Style.Font.Bold = true;
                worksheet.Cells[2, 5].Value = "MÃ LINH KIỆN";
                worksheet.Column(5).Width = 30;

                worksheet.Cells[2, 6].Style.Font.Bold = true;
                worksheet.Cells[2, 6].Value = "VỊ TRÍ";
                worksheet.Column(6).Width = 30;

                worksheet.Cells[2, 7].Style.Font.Bold = true;
                worksheet.Cells[2, 7].Value = "SỐ LƯỢNG KIỂM KÊ";
                worksheet.Column(7).Width = 30;
                //worksheet.Column(6).AutoFit();

                worksheet.Cells[2, 8].Style.Font.Bold = true;
                worksheet.Cells[2, 8].Value = "SỐ LƯỢNG HỆ THỐNG";
                worksheet.Column(8).Width = 30;

                worksheet.Cells[2, 9].Style.Font.Bold = true;
                worksheet.Cells[2, 9].Value = "CHÊNH LỆCH";
                worksheet.Column(9).Width = 30;

                worksheet.Cells[2, 10].Style.Font.Bold = true;
                worksheet.Cells[2, 10].Value = "GIÁ TIỀN";
                worksheet.Column(10).Width = 30;

                //worksheet.Cells[2, 11].Style.Font.Bold = true;
                //worksheet.Cells[2, 11].Value = "ĐƠN GIÁ";
                //worksheet.Column(11).Width = 30;

                //worksheet.Cells[2, 12].Style.Font.Bold = true;
                //worksheet.Cells[2, 12].Value = "CHÊNH LỆCH ABS";
                //worksheet.Column(12).Width = 30;
                //worksheet.Cells[2, 13].Style.Font.Bold = true;
                //worksheet.Cells[2, 13].Value = "GIÁ TIỀN (ABS)";
                //worksheet.Column(13).Width = 30;
                worksheet.Cells[2, 11].Style.Font.Bold = true;
                worksheet.Cells[2, 11].Value = "TÀI KHOẢN PHÂN PHÁT";
                worksheet.Column(11).Width = 30;
                worksheet.Cells[2, 12].Style.Font.Bold = true;
                worksheet.Cells[2, 12].Value = "SỐ LƯỢNG ĐIỀU CHỈNH";
                worksheet.Column(12).Width = 30;
                worksheet.Cells[2, 13].Style.Font.Bold = true;
                worksheet.Cells[2, 13].Value = "PHÂN LOẠI LỖI";
                worksheet.Column(13).Width = 30;
                worksheet.Cells[2, 14].Style.Font.Bold = true;
                worksheet.Cells[2, 14].Value = "NGUYÊN NHÂN SAI SỐ";
                worksheet.Column(14).Width = 30;
                worksheet.Cells[2, 15].Style.Font.Bold = true;
                worksheet.Cells[2, 15].Value = "NGƯỜI ĐIỀU TRA";
                worksheet.Column(15).Width = 30;
                //worksheet.Cells[2, 16].Style.Font.Bold = true;
                //worksheet.Cells[2, 16].Value = "TỔNG SỐ LƯỢNG ĐIỀU TRA";
                //worksheet.Column(16).Width = 30;
                worksheet.Cells[2, 16].Style.Font.Bold = true;
                worksheet.Cells[2, 16].Value = "TRẠNG THÁI";
                worksheet.Column(16).Width = 30;
                worksheet.Cells[2, 17].Style.Font.Bold = true;
                worksheet.Cells[2, 17].Value = "TÊN LINH KIỆN";
                worksheet.Column(17).Width = 30;
                worksheet.Cells[2, 18].Style.Font.Bold = true;
                worksheet.Cells[2, 18].Value = "LỊCH SỬ ĐIỀU CHỈNH";
                worksheet.Column(18).Width = 30;
                worksheet.Cells[2, 19].Style.Font.Bold = true;
                worksheet.Cells[2, 19].Value = "GHI CHÚ";
                worksheet.Column(19).Width = 30;
                //worksheet.Column(7).AutoFit();

                int row = 3, index = 1;
                foreach (var item in model)
                {
                    worksheet.Cells[row, 1].Value = index;
                    worksheet.Cells[row, 2].Value = item.InventoryName ?? string.Empty;
                    worksheet.Cells[row, 3].Value = item.Plant ?? string.Empty;
                    worksheet.Cells[row, 4].Value = item.WHLoc ?? string.Empty;
                    worksheet.Cells[row, 5].Value = item.ComponentCode ?? string.Empty;
                    worksheet.Cells[row, 6].Value = item.Position ?? string.Empty;
                    worksheet.Cells[row, 7].Value = item.TotalQuantity.HasValue ? item.TotalQuantity.Value : 0;
                    worksheet.Cells[row, 8].Value = item.AccountQuantity.HasValue ? item.AccountQuantity.Value : 0;
                    worksheet.Cells[row, 9].Value = item.ErrorQuantity.HasValue ? item.ErrorQuantity.Value : 0;
                    worksheet.Cells[row, 10].Value = item.ErrorMoney.HasValue ? item.ErrorMoney.Value : 0;
                    //worksheet.Cells[row, 11].Value = item.UnitPrice.HasValue ? item.UnitPrice.Value : 0;
                    //worksheet.Cells[row, 12].Value = item.ErrorQuantityAbs.HasValue ? item.ErrorQuantityAbs.Value : 0;
                    //worksheet.Cells[row, 13].Value = item.ErrorMoneyAbs.HasValue ? item.ErrorMoneyAbs.Value : 0;
                    worksheet.Cells[row, 11].Value = item.AssigneeAccount ?? string.Empty;
                    worksheet.Cells[row, 12].Value = item.InvestigationQuantity.HasValue ? item.InvestigationQuantity.Value : 0;
                    worksheet.Cells[row, 13].Value = item.ErrorCategory.HasValue ? item.ErrorCategory.Value : string.Empty;
                    worksheet.Cells[row, 14].Value = item.ErrorDetail ?? string.Empty;
                    worksheet.Cells[row, 15].Value = item.Investigator ?? string.Empty;
                    //worksheet.Cells[row, 16].Value = item.InvestigationTotal.HasValue ? item.InvestigationTotal.Value : 0;
                    worksheet.Cells[row, 16].Value = item.Status.GetDisplayName();
                    worksheet.Cells[row, 17].Value = item.ComponentName ?? string.Empty;
                    worksheet.Cells[row, 18].Value = item.InvestigationHistoryCount ?? string.Empty;
                    worksheet.Cells[row, 19].Value = item.NoteDocumentTypeA ?? string.Empty;

                    row++;
                    index++;
                }

                var exportData = await Task.Run(() => package.GetAsByteArray());
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Data = exportData,
                    Message = "Xuất Excel thành công!"
                };
            }
        }

        public async Task<ResponseModel> ExportListErrorInvestigationDetail(IEnumerable<ListErrorInvestigationHistoryWebDto> model)
        {
            if (model == null || !model.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Data = null,
                    Message = "Không có dữ liệu"
                };
            }

            // Tạo tệp Excel template mới
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh Sách Chi Tiết Điều Tra");

                // Điều chỉnh kiểu chữ và cỡ chữ của trang tính
                worksheet.Cells.Style.WrapText = false;

                worksheet.Cells[1, 4].Style.Font.Bold = true;
                worksheet.Cells[1, 4].Value = "DANH SÁCH CHI TIẾT ĐIỀU TRA";

                // Gán tiêu đề cột cho trang tính
                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "STT";
                worksheet.Column(1).Width = 10;

                worksheet.Cells[2, 2].Style.Font.Bold = true;
                worksheet.Cells[2, 2].Value = "Loại điều chỉnh";
                worksheet.Column(2).Width = 30;

                worksheet.Cells[2, 3].Style.Font.Bold = true;
                worksheet.Cells[2, 3].Value = "Đợt kiểm kê";
                worksheet.Column(3).Width = 30;

                worksheet.Cells[2, 4].Style.Font.Bold = true;
                worksheet.Cells[2, 4].Value = "PLANT";
                worksheet.Column(4).Width = 30;

                worksheet.Cells[2, 5].Style.Font.Bold = true;
                worksheet.Cells[2, 5].Value = "WH LOC.";
                worksheet.Column(5).Width = 30;

                worksheet.Cells[2, 6].Style.Font.Bold = true;
                worksheet.Cells[2, 6].Value = "MÃ LINH KIỆN";
                worksheet.Column(6).Width = 30;

                worksheet.Cells[2, 7].Style.Font.Bold = true;
                worksheet.Cells[2, 7].Value = "VỊ TRÍ";
                worksheet.Column(7).Width = 30;

                worksheet.Cells[2, 8].Style.Font.Bold = true;
                worksheet.Cells[2, 8].Value = "SỐ LƯỢNG KIỂM KÊ";
                worksheet.Column(8).Width = 30;
                //worksheet.Column(6).AutoFit();

                worksheet.Cells[2, 9].Style.Font.Bold = true;
                worksheet.Cells[2, 9].Value = "SỐ LƯỢNG HỆ THỐNG";
                worksheet.Column(9).Width = 30;

                worksheet.Cells[2, 10].Style.Font.Bold = true;
                worksheet.Cells[2, 10].Value = "CHÊNH LỆCH";
                worksheet.Column(10).Width = 30;

                worksheet.Cells[2, 11].Style.Font.Bold = true;
                worksheet.Cells[2, 11].Value = "GIÁ TIỀN";
                worksheet.Column(11).Width = 30;

                worksheet.Cells[2, 12].Style.Font.Bold = true;
                worksheet.Cells[2, 12].Value = "TÀI KHOẢN PHÂN PHÁT";
                worksheet.Column(12).Width = 30;
                worksheet.Cells[2, 13].Style.Font.Bold = true;
                worksheet.Cells[2, 13].Value = "SỐ LƯỢNG ĐIỀU CHỈNH";
                worksheet.Column(13).Width = 30;
                worksheet.Cells[2, 14].Style.Font.Bold = true;
                worksheet.Cells[2, 14].Value = "PHÂN LOẠI LỖI";
                worksheet.Column(14).Width = 30;
                worksheet.Cells[2, 15].Style.Font.Bold = true;
                worksheet.Cells[2, 15].Value = "NGUYÊN NHÂN SAI SỐ";
                worksheet.Column(15).Width = 30;
                worksheet.Cells[2, 16].Style.Font.Bold = true;
                worksheet.Cells[2, 16].Value = "NGƯỜI ĐIỀU TRA";
                worksheet.Column(16).Width = 30;
                worksheet.Cells[2, 17].Style.Font.Bold = true;
                worksheet.Cells[2, 17].Value = "THỜI GIAN ĐIỀU TRA";
                worksheet.Column(17).Width = 30;
                worksheet.Cells[2, 18].Style.Font.Bold = true;
                worksheet.Cells[2, 18].Value = "NGƯỜI XÁC NHẬN";
                worksheet.Column(18).Width = 30;
                worksheet.Cells[2, 19].Style.Font.Bold = true;
                worksheet.Cells[2, 19].Value = "NGƯỜI PHÊ DUYỆT";
                worksheet.Column(19).Width = 30;
                worksheet.Cells[2, 20].Style.Font.Bold = true;
                worksheet.Cells[2, 20].Value = "TÊN LINH KIỆN";
                worksheet.Column(20).Width = 30;
                worksheet.Cells[2, 21].Style.Font.Bold = true;
                worksheet.Cells[2, 21].Value = "LỊCH SỬ ĐIỀU CHỈNH";
                worksheet.Column(21).Width = 30;
                worksheet.Cells[2, 22].Style.Font.Bold = true;
                worksheet.Cells[2, 22].Value = "GHI CHÚ";
                worksheet.Column(22).Width = 30;

                int row = 3, index = 1;
                foreach (var item in model)
                {
                    worksheet.Cells[row, 1].Value = index;
                    worksheet.Cells[row, 2].Value = item.ErrorType.HasValue ? item.ErrorType.Value.GetDisplayName() : string.Empty;
                    worksheet.Cells[row, 3].Value = item.InventoryName ?? string.Empty;
                    worksheet.Cells[row, 4].Value = item.Plant ?? string.Empty;
                    worksheet.Cells[row, 5].Value = item.WHLoc ?? string.Empty;
                    worksheet.Cells[row, 6].Value = item.ComponentCode ?? string.Empty;
                    worksheet.Cells[row, 7].Value = item.Position ?? string.Empty;
                    worksheet.Cells[row, 8].Value = item.TotalQuantity.HasValue ? item.TotalQuantity.Value : 0;
                    worksheet.Cells[row, 9].Value = item.AccountQuantity.HasValue ? item.AccountQuantity.Value : 0;
                    worksheet.Cells[row, 10].Value = item.ErrorQuantity.HasValue ? item.ErrorQuantity.Value : 0;
                    worksheet.Cells[row, 11].Value = item.ErrorMoney.HasValue ? item.ErrorMoney.Value : 0;
                    //worksheet.Cells[row, 11].Value = item.UnitPrice.HasValue ? item.UnitPrice.Value : 0;
                    //worksheet.Cells[row, 12].Value = item.ErrorQuantityAbs.HasValue ? item.ErrorQuantityAbs.Value : 0;
                    //worksheet.Cells[row, 13].Value = item.ErrorMoneyAbs.HasValue ? item.ErrorMoneyAbs.Value : 0;
                    worksheet.Cells[row, 12].Value = item.AssigneeAccount ?? string.Empty;
                    worksheet.Cells[row, 13].Value = item.InvestigationQuantity.HasValue ? item.InvestigationQuantity.Value : 0;
                    worksheet.Cells[row, 14].Value = item.ErrorCategory.HasValue ? item.ErrorCategory.Value : string.Empty;
                    worksheet.Cells[row, 15].Value = item.ErrorDetail ?? string.Empty;
                    worksheet.Cells[row, 16].Value = item.Investigator ?? string.Empty;
                    worksheet.Cells[row, 17].Value = item.InvestigationDateTime ?? string.Empty;
                    worksheet.Cells[row, 18].Value = item.ConfirmInvestigator ?? string.Empty;
                    worksheet.Cells[row, 19].Value = item.ApproveInvestigator ?? string.Empty;
                    //worksheet.Cells[row, 16].Value = item.InvestigationTotal.HasValue ? item.InvestigationTotal.Value : 0;
                    //worksheet.Cells[row, 16].Value = item.Status.GetDisplayName();
                    worksheet.Cells[row, 20].Value = item.ComponentName ?? string.Empty;
                    worksheet.Cells[row, 21].Value = item.InvestigationHistoryCount ?? string.Empty;
                    worksheet.Cells[row, 22].Value = item.NoteDocumentTypeA ?? string.Empty;

                    row++;
                    index++;
                }

                var exportData = await Task.Run(() => package.GetAsByteArray());
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Data = exportData,
                    Message = "Xuất Excel thành công!"
                };
            }
        }
    }
}
