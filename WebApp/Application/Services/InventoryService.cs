using System.Diagnostics;
//using System.Drawing;
using System.Text;
using BIVN.FixedStorage.Services.Common.API.Dto.Inventory;
using BIVN.FixedStorage.Services.Common.API.Dto.QRCode;
using BIVN.FixedStorage.Services.Common.API.Enum.Storage;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using QRCoder;
using SkiaSharp;
//using ZXing;
//using ZXing.QrCode;

namespace WebApp.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public InventoryService(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment hostingEnvironment)
        {
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<ResponseModel> ExportListAuditTarget(List<ListAuditTargetViewModel> model)
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
                var worksheet = package.Workbook.Worksheets.Add("Danh Sách Giám Sát");

                // Điều chỉnh kiểu chữ và cỡ chữ của trang tính
                worksheet.Cells.Style.WrapText = false;

                worksheet.Cells[1, 4].Style.Font.Bold = true;
                worksheet.Cells[1, 4].Value = "DANH SÁCH GIÁM SÁT";

                // Gán tiêu đề cột cho trang tính
                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "STT";
                worksheet.Column(1).Width = 10;

                worksheet.Cells[2, 2].Style.Font.Bold = true;
                worksheet.Cells[2, 2].Value = "PLANT";
                worksheet.Column(2).Width = 20;

                worksheet.Cells[2, 3].Style.Font.Bold = true;
                worksheet.Cells[2, 3].Value = "WH LOC.";
                worksheet.Column(3).Width = 20;

                worksheet.Cells[2, 4].Style.Font.Bold = true;
                worksheet.Cells[2, 4].Value = "KHU VỰC";
                worksheet.Column(4).Width = 20;

                worksheet.Cells[2, 5].Style.Font.Bold = true;
                worksheet.Cells[2, 5].Value = "MÃ LINH KIỆN";
                worksheet.Column(5).Width = 30;

                worksheet.Cells[2, 6].Style.Font.Bold = true;
                worksheet.Cells[2, 6].Value = "S/O NO.";
                worksheet.Column(6).Width = 30;
                //worksheet.Column(6).AutoFit();

                worksheet.Cells[2, 7].Style.Font.Bold = true;
                worksheet.Cells[2, 7].Value = "VỊ TRÍ";
                worksheet.Column(7).Width = 20;

                worksheet.Cells[2, 8].Style.Font.Bold = true;
                worksheet.Cells[2, 8].Value = "TÊN LINH KIỆN";
                worksheet.Column(8).Width = 40;

                worksheet.Cells[2, 9].Style.Font.Bold = true;
                worksheet.Cells[2, 9].Value = "TRẠNG THÁI";
                worksheet.Column(9).Width = 30;

                worksheet.Cells[2, 10].Style.Font.Bold = true;
                worksheet.Cells[2, 10].Value = "TÀI KHOẢN PHÂN PHÁT";
                worksheet.Column(10).Width = 30;

                //worksheet.Column(7).AutoFit();

                int row = 3, index = 1;
                foreach (var item in model)
                {
                    var newStatus = string.Empty;

                    if (item.Status == 0)
                    {
                        newStatus = "Chưa giám sát";
                    }
                    else if (item.Status == 1)
                    {
                        newStatus = "Giám sát đạt";
                    }
                    else if (item.Status == 2)
                    {
                        newStatus = "Giám sát không đạt";
                    }

                    worksheet.Cells[row, 1].Value = index;
                    worksheet.Cells[row, 2].Value = item.Plant;
                    worksheet.Cells[row, 3].Value = item.WHLoc;
                    worksheet.Cells[row, 4].Value = item.Location;
                    worksheet.Cells[row, 5].Value = item.ComponentCode;
                    worksheet.Cells[row, 6].Value = item.SaleOrderNo;
                    worksheet.Cells[row, 7].Value = item.Position;
                    worksheet.Cells[row, 8].Value = item.ComponentName;
                    worksheet.Cells[row, 9].Value = newStatus;
                    worksheet.Cells[row, 10].Value = item.AssigneeAccount;
                    row++;
                    index++;
                }


                var ExportData = package.GetAsByteArray();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Data = ExportData,
                    Message = "Xuất Excel thành công!"
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = null,
                Message = "Xuất Excel thành công!"
            };
        }

        public async Task<ResponseModel> ExportListInventory(List<ListInventoryModel> model)
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
                var worksheet = package.Workbook.Worksheets.Add("Danh Sách Đợt kiểm kê");

                // Điều chỉnh kiểu chữ và cỡ chữ của trang tính
                worksheet.Cells.Style.WrapText = false;

                worksheet.Cells[1, 4].Style.Font.Bold = true;
                worksheet.Cells[1, 4].Value = "DANH SÁCH ĐỢT KIỂM KÊ";

                // Gán tiêu đề cột cho trang tính
                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "STT";
                worksheet.Column(1).Width = 10;

                worksheet.Cells[2, 2].Style.Font.Bold = true;
                worksheet.Cells[2, 2].Value = "ĐỢT KIỂM KÊ";
                worksheet.Column(2).Width = 20;

                worksheet.Cells[2, 3].Style.Font.Bold = true;
                worksheet.Cells[2, 3].Value = "NGÀY KIỂM KÊ";
                worksheet.Column(3).Width = 20;

                worksheet.Cells[2, 4].Style.Font.Bold = true;
                worksheet.Cells[2, 4].Value = "TỈ LỆ KIỂM KÊ LẠI(%)";
                worksheet.Column(4).Width = 20;

                worksheet.Cells[2, 5].Style.Font.Bold = true;
                worksheet.Cells[2, 5].Value = "TRẠNG THÁI";
                worksheet.Column(5).Width = 30;

                worksheet.Cells[2, 6].Style.Font.Bold = true;
                worksheet.Cells[2, 6].Value = "NGƯỜI TẠO";
                worksheet.Column(6).Width = 40;
                //worksheet.Column(6).AutoFit();

                worksheet.Cells[2, 7].Style.Font.Bold = true;
                worksheet.Cells[2, 7].Value = "NGÀY TẠO";
                worksheet.Column(7).Width = 20;
                //worksheet.Column(7).AutoFit();

                int row = 3, index = 1;

                foreach (var item in model)
                {
                    var newInventoryDate = string.Empty;
                    var newCreateAt = string.Empty;
                    var newStatus = string.Empty;

                    newInventoryDate = item.InventoryDate.HasValue ? item.InventoryDate.Value.ToString(commonAPIConstant.DayMonthYearFormat) : "";
                    newCreateAt = item.CreateAt.HasValue ? item.CreateAt.Value.ToString(commonAPIConstant.DefaultDateFormat) : "";
                    if (item.Status == 0)
                    {
                        newStatus = "Chưa kiểm kê";
                    }
                    else if (item.Status == 1)
                    {
                        newStatus = "Đang kiểm kê";
                    }
                    else if (item.Status == 2)
                    {
                        newStatus = "Đang giám sát";
                    }
                    else if (item.Status == 3)
                    {
                        newStatus = "Hoàn thành";
                    }

                    worksheet.Cells[row, 1].Value = index;
                    worksheet.Cells[row, 2].Value = item.InventoryName;
                    worksheet.Cells[row, 3].Value = newInventoryDate;
                    worksheet.Cells[row, 4].Value = item.AuditFailPercentage;
                    worksheet.Cells[row, 5].Value = newStatus;
                    worksheet.Cells[row, 6].Value = item.FullName;
                    worksheet.Cells[row, 7].Value = newCreateAt;
                    row++;
                    index++;
                }


                var ExportData = package.GetAsByteArray();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Data = ExportData,
                    Message = "Xuất Excel thành công!"
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = null,
                Message = "Xuất Excel thành công!"
            };

        }

        public async Task<ResponseModel> ExportListInventoryDocument(List<ListInventoryDocumentModel> model)
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
                var worksheet = package.Workbook.Worksheets.Add("Phiếu kiểm kê");

                // Điều chỉnh kiểu chữ và cỡ chữ của trang tính
                worksheet.Cells.Style.WrapText = false;

                worksheet.Cells[1, 4].Style.Font.Bold = true;
                worksheet.Cells[1, 4].Value = "PHIẾU KIỂM KÊ";

                // Gán tiêu đề cột cho trang tính
                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "STT";
                worksheet.Column(1).Width = 10;

                worksheet.Cells[2, 2].Style.Font.Bold = true;
                worksheet.Cells[2, 2].Value = "MÃ PHIẾU";
                worksheet.Column(2).Width = 20;

                worksheet.Cells[2, 3].Style.Font.Bold = true;
                worksheet.Cells[2, 3].Value = "PLANT";
                worksheet.Column(3).Width = 20;

                worksheet.Cells[2, 4].Style.Font.Bold = true;
                worksheet.Cells[2, 4].Value = "WH LOC.";
                worksheet.Column(4).Width = 20;

                worksheet.Cells[2, 5].Style.Font.Bold = true;
                worksheet.Cells[2, 5].Value = "MÃ LINH KIỆN";
                worksheet.Column(5).Width = 30;

                worksheet.Cells[2, 6].Style.Font.Bold = true;
                worksheet.Cells[2, 6].Value = "TÊN LINH KIỆN";
                worksheet.Column(6).Width = 40;

                worksheet.Cells[2, 7].Style.Font.Bold = true;
                worksheet.Cells[2, 7].Value = "MODEL CODE";
                worksheet.Column(7).Width = 40;

                worksheet.Cells[2, 8].Style.Font.Bold = true;
                worksheet.Cells[2, 8].Value = "TÊN CÔNG ĐOẠN";
                worksheet.Column(8).Width = 30;

                worksheet.Cells[2, 9].Style.Font.Bold = true;
                worksheet.Cells[2, 9].Value = "VỊ TRÍ";
                worksheet.Column(9).Width = 30;

                worksheet.Cells[2, 10].Style.Font.Bold = true;
                worksheet.Cells[2, 10].Value = "PHÒNG BAN";
                worksheet.Column(10).Width = 30;

                worksheet.Cells[2, 11].Style.Font.Bold = true;
                worksheet.Cells[2, 11].Value = "KHU VỰC";
                worksheet.Column(11).Width = 30;

                worksheet.Cells[2, 12].Style.Font.Bold = true;
                worksheet.Cells[2, 12].Value = "TÀI KHOẢN PHÂN PHÁT";
                worksheet.Column(12).Width = 30;

                worksheet.Cells[2, 13].Style.Font.Bold = true;
                worksheet.Cells[2, 13].Value = "STOCK TYPE";
                worksheet.Column(13).Width = 30;

                worksheet.Cells[2, 14].Style.Font.Bold = true;
                worksheet.Cells[2, 14].Value = "SPECIAL STOCK";
                worksheet.Column(14).Width = 30;

                worksheet.Cells[2, 15].Style.Font.Bold = true;
                worksheet.Cells[2, 15].Value = "S/O NO.";
                worksheet.Column(15).Width = 30;

                worksheet.Cells[2, 16].Style.Font.Bold = true;
                worksheet.Cells[2, 16].Value = "S/O LIST";
                worksheet.Column(16).Width = 30;

                worksheet.Cells[2, 17].Style.Font.Bold = true;
                worksheet.Cells[2, 17].Value = "SAP INVENTORY NO.";
                worksheet.Column(17).Width = 30;

                worksheet.Cells[2, 18].Style.Font.Bold = true;
                worksheet.Cells[2, 18].Value = "ASSEMBLY LOC.";
                worksheet.Column(18).Width = 30;

                worksheet.Cells[2, 19].Style.Font.Bold = true;
                worksheet.Cells[2, 19].Value = "VENDER CODE";
                worksheet.Column(19).Width = 30;

                worksheet.Cells[2, 20].Style.Font.Bold = true;
                worksheet.Cells[2, 20].Value = "PHYS.INV";
                worksheet.Column(20).Width = 30;

                worksheet.Cells[2, 21].Style.Font.Bold = true;
                worksheet.Cells[2, 21].Value = "FISCAL YEAR";
                worksheet.Column(21).Width = 30;

                worksheet.Cells[2, 22].Style.Font.Bold = true;
                worksheet.Cells[2, 22].Value = "ITEM";
                worksheet.Column(22).Width = 30;

                worksheet.Cells[2, 23].Style.Font.Bold = true;
                worksheet.Cells[2, 23].Value = "PLANNED COUNT";
                worksheet.Column(23).Width = 30;

                worksheet.Cells[2, 24].Style.Font.Bold = true;
                worksheet.Cells[2, 24].Value = "CỘT C";
                worksheet.Column(24).Width = 30;

                worksheet.Cells[2, 25].Style.Font.Bold = true;
                worksheet.Cells[2, 25].Value = "CỘT N";
                worksheet.Column(25).Width = 30;

                worksheet.Cells[2, 26].Style.Font.Bold = true;
                worksheet.Cells[2, 26].Value = "CỘT O";
                worksheet.Column(26).Width = 30;

                worksheet.Cells[2, 27].Style.Font.Bold = true;
                worksheet.Cells[2, 27].Value = "CỘT P";
                worksheet.Column(27).Width = 30;

                worksheet.Cells[2, 28].Style.Font.Bold = true;
                worksheet.Cells[2, 28].Value = "CỘT Q";
                worksheet.Column(28).Width = 30;

                worksheet.Cells[2, 29].Style.Font.Bold = true;
                worksheet.Cells[2, 29].Value = "CỘT R";
                worksheet.Column(29).Width = 30;

                worksheet.Cells[2, 30].Style.Font.Bold = true;
                worksheet.Cells[2, 30].Value = "CỘT S";
                worksheet.Column(30).Width = 30;

                worksheet.Cells[2, 31].Style.Font.Bold = true;
                worksheet.Cells[2, 31].Value = "GHI CHÚ";
                worksheet.Column(31).Width = 30;

                //worksheet.Cells[2, 8].Style.Font.Bold = true;
                //worksheet.Cells[2, 8].Value = "TÊN LINH KIỆN";
                //worksheet.Column(8).Width = 30;

                //worksheet.Cells[2, 9].Style.Font.Bold = true;
                //worksheet.Cells[2, 9].Value = "QUANTITY";
                //worksheet.Column(9).Width = 30;

                //worksheet.Cells[2, 21].Style.Font.Bold = true;
                //worksheet.Cells[2, 21].Value = "PRO. ORDER NO";
                //worksheet.Column(21).Width = 30;

                //worksheet.Cells[2, 33].Style.Font.Bold = true;
                //worksheet.Cells[2, 33].Value = "NGƯỜI TẠO";
                //worksheet.Column(33).Width = 30;

                //worksheet.Cells[2, 34].Style.Font.Bold = true;
                //worksheet.Cells[2, 34].Value = "THỜI GIAN TẠO";
                //worksheet.Column(34).Width = 30;

                //worksheet.Column(7).AutoFit();

                int row = 3, index = 1;
                foreach (var item in model)
                {
                    worksheet.Cells[row, 1].Value = index;
                    worksheet.Cells[row, 2].Value = item.DocCode;
                    worksheet.Cells[row, 3].Value = item.Plant;
                    worksheet.Cells[row, 4].Value = item.WHLoc;
                    worksheet.Cells[row, 5].Value = item.ComponentCode;
                    worksheet.Cells[row, 6].Value = item.ComponentName;
                    worksheet.Cells[row, 7].Value = item.ModelCode;
                    worksheet.Cells[row, 8].Value = item.StageName;
                    worksheet.Cells[row, 9].Value = item.Position;
                    worksheet.Cells[row, 10].Value = item.Department;
                    worksheet.Cells[row, 11].Value = item.Location;
                    worksheet.Cells[row, 12].Value = item.AssigneeAccount;
                    worksheet.Cells[row, 13].Value = item.StockType;
                    worksheet.Cells[row, 14].Value = item.SpecialStock;
                    worksheet.Cells[row, 15].Value = item.SaleOrderNo;
                    worksheet.Cells[row, 16].Value = item.SaleOrderList;
                    worksheet.Cells[row, 17].Value = item.SAPInventoryNo;
                    worksheet.Cells[row, 18].Value = item.AssemblyLoc;
                    worksheet.Cells[row, 19].Value = item.VendorCode;
                    worksheet.Cells[row, 20].Value = item.PhysInv;
                    worksheet.Cells[row, 21].Value = item.FiscalYear;
                    worksheet.Cells[row, 22].Value = item.Item;
                    worksheet.Cells[row, 23].Value = item.PlantedCount;
                    worksheet.Cells[row, 24].Value = item.ColumnC;
                    worksheet.Cells[row, 25].Value = item.ColumnN;
                    worksheet.Cells[row, 26].Value = item.ColumnO;
                    worksheet.Cells[row, 27].Value = item.ColumnP;
                    worksheet.Cells[row, 28].Value = item.ColumnQ;
                    worksheet.Cells[row, 29].Value = item.ColumnR;
                    worksheet.Cells[row, 30].Value = item.ColumnS;
                    worksheet.Cells[row, 31].Value = item.Note;
                    row++;
                    index++;
                }


                var ExportData = package.GetAsByteArray();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Data = ExportData,
                    Message = "Xuất Excel thành công!"
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = null,
                Message = "Xuất Excel thành công!"
            };
        }

        public async Task<ResponseModel> ExportListInventoryDocumentFull(List<ListInventoryDocumentFullModel> model)
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
                var worksheet = package.Workbook.Worksheets.Add("Danh Sách phiếu kiểm kê các đợt");

                // Điều chỉnh kiểu chữ và cỡ chữ của trang tính
                worksheet.Cells.Style.WrapText = false;

                worksheet.Cells[1, 4].Style.Font.Bold = true;
                worksheet.Cells[1, 4].Value = "DANH SÁCH PHIẾU KIỂM KÊ CÁC ĐỢT";

                // Gán tiêu đề cột cho trang tính
                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "STT";
                worksheet.Column(1).Width = 10;

                worksheet.Cells[2, 2].Style.Font.Bold = true;
                worksheet.Cells[2, 2].Value = "ĐỢT KIỂM KÊ";
                worksheet.Column(2).Width = 20;

                worksheet.Cells[2, 3].Style.Font.Bold = true;
                worksheet.Cells[2, 3].Value = "PHÒNG BAN";
                worksheet.Column(3).Width = 20;

                worksheet.Cells[2, 4].Style.Font.Bold = true;
                worksheet.Cells[2, 4].Value = "KHU VỰC";
                worksheet.Column(4).Width = 20;

                worksheet.Cells[2, 5].Style.Font.Bold = true;
                worksheet.Cells[2, 5].Value = "MÃ PHIẾU";
                worksheet.Column(5).Width = 30;

                worksheet.Cells[2, 6].Style.Font.Bold = true;
                worksheet.Cells[2, 6].Value = "PLANT";
                worksheet.Column(6).Width = 40;

                worksheet.Cells[2, 7].Style.Font.Bold = true;
                worksheet.Cells[2, 7].Value = "WH LOC.";
                worksheet.Column(7).Width = 30;

                worksheet.Cells[2, 8].Style.Font.Bold = true;
                worksheet.Cells[2, 8].Value = "MÃ LINH KIỆN";
                worksheet.Column(8).Width = 30;

                worksheet.Cells[2, 9].Style.Font.Bold = true;
                worksheet.Cells[2, 9].Value = "MODEL CODE";
                worksheet.Column(9).Width = 30;

                worksheet.Cells[2, 10].Style.Font.Bold = true;
                worksheet.Cells[2, 10].Value = "TÊN LINH KIỆN";
                worksheet.Column(10).Width = 30;

                worksheet.Cells[2, 11].Style.Font.Bold = true;
                worksheet.Cells[2, 11].Value = "QUANTITY";
                worksheet.Column(11).Width = 30;

                worksheet.Cells[2, 12].Style.Font.Bold = true;
                worksheet.Cells[2, 12].Value = "VỊ TRÍ";
                worksheet.Column(12).Width = 30;

                worksheet.Cells[2, 13].Style.Font.Bold = true;
                worksheet.Cells[2, 13].Value = "TRẠNG THÁI";
                worksheet.Column(13).Width = 30;

                worksheet.Cells[2, 14].Style.Font.Bold = true;
                worksheet.Cells[2, 14].Value = "STOCK TYPES";
                worksheet.Column(14).Width = 30;

                worksheet.Cells[2, 15].Style.Font.Bold = true;
                worksheet.Cells[2, 15].Value = "SPECIAL STOCK";
                worksheet.Column(15).Width = 30;

                worksheet.Cells[2, 16].Style.Font.Bold = true;
                worksheet.Cells[2, 16].Value = "S/O NO.";
                worksheet.Column(16).Width = 30;

                worksheet.Cells[2, 17].Style.Font.Bold = true;
                worksheet.Cells[2, 17].Value = "S/O LIST";
                worksheet.Column(17).Width = 30;

                worksheet.Cells[2, 18].Style.Font.Bold = true;
                worksheet.Cells[2, 18].Value = "TÀI KHOẢN PHÂN PHÁT";
                worksheet.Column(18).Width = 30;

                worksheet.Cells[2, 19].Style.Font.Bold = true;
                worksheet.Cells[2, 19].Value = "TÀI KHOẢN TIẾP NHẬN";
                worksheet.Column(19).Width = 30;

                worksheet.Cells[2, 20].Style.Font.Bold = true;
                worksheet.Cells[2, 20].Value = "THỜI GIAN TIẾP NHẬN";
                worksheet.Column(20).Width = 30;

                worksheet.Cells[2, 21].Style.Font.Bold = true;
                worksheet.Cells[2, 21].Value = "TÀI KHOẢN KIỂM KÊ";
                worksheet.Column(21).Width = 30;

                worksheet.Cells[2, 22].Style.Font.Bold = true;
                worksheet.Cells[2, 22].Value = "THỜI GIAN KIỂM KÊ";
                worksheet.Column(22).Width = 30;

                worksheet.Cells[2, 23].Style.Font.Bold = true;
                worksheet.Cells[2, 23].Value = "THỜI GIAN XÁC NHẬN";
                worksheet.Column(23).Width = 30;

                worksheet.Cells[2, 24].Style.Font.Bold = true;
                worksheet.Cells[2, 24].Value = "TÀI KHOẢN GIÁM SÁT";
                worksheet.Column(24).Width = 30;

                worksheet.Cells[2, 25].Style.Font.Bold = true;
                worksheet.Cells[2, 25].Value = "THỜI GIAN GIÁM SÁT";
                worksheet.Column(25).Width = 30;

                worksheet.Cells[2, 26].Style.Font.Bold = true;
                worksheet.Cells[2, 26].Value = "SAP INVENTORY NO.";
                worksheet.Column(26).Width = 30;

                worksheet.Cells[2, 27].Style.Font.Bold = true;
                worksheet.Cells[2, 27].Value = "ASSEMBLY LOC.";
                worksheet.Column(27).Width = 30;

                worksheet.Cells[2, 28].Style.Font.Bold = true;
                worksheet.Cells[2, 28].Value = "VENDOR CODE";
                worksheet.Column(28).Width = 30;

                worksheet.Cells[2, 29].Style.Font.Bold = true;
                worksheet.Cells[2, 29].Value = "PHYS.INV";
                worksheet.Column(29).Width = 30;

                worksheet.Cells[2, 30].Style.Font.Bold = true;
                worksheet.Cells[2, 30].Value = "FISSCAL YEAR";
                worksheet.Column(30).Width = 30;

                worksheet.Cells[2, 31].Style.Font.Bold = true;
                worksheet.Cells[2, 31].Value = "ITEM";
                worksheet.Column(31).Width = 30;

                worksheet.Cells[2, 32].Style.Font.Bold = true;
                worksheet.Cells[2, 32].Value = "PLANNED COUNT";
                worksheet.Column(32).Width = 30;

                worksheet.Cells[2, 33].Style.Font.Bold = true;
                worksheet.Cells[2, 33].Value = "CỘT C";
                worksheet.Column(33).Width = 30;

                worksheet.Cells[2, 34].Style.Font.Bold = true;
                worksheet.Cells[2, 34].Value = "CỘT N";
                worksheet.Column(34).Width = 30;

                worksheet.Cells[2, 35].Style.Font.Bold = true;
                worksheet.Cells[2, 35].Value = "CỘT O";
                worksheet.Column(35).Width = 30;

                worksheet.Cells[2, 36].Style.Font.Bold = true;
                worksheet.Cells[2, 36].Value = "CỘT P";
                worksheet.Column(36).Width = 30;

                worksheet.Cells[2, 37].Style.Font.Bold = true;
                worksheet.Cells[2, 37].Value = "CỘT Q";
                worksheet.Column(37).Width = 30;

                worksheet.Cells[2, 38].Style.Font.Bold = true;
                worksheet.Cells[2, 38].Value = "CỘT R";
                worksheet.Column(38).Width = 30;

                worksheet.Cells[2, 39].Style.Font.Bold = true;
                worksheet.Cells[2, 39].Value = "CỘT S";
                worksheet.Column(39).Width = 30;

                worksheet.Cells[2, 40].Style.Font.Bold = true;
                worksheet.Cells[2, 40].Value = "NGƯỜI TẠO";
                worksheet.Column(40).Width = 30;

                worksheet.Cells[2, 41].Style.Font.Bold = true;
                worksheet.Cells[2, 41].Value = "THỜI GIAN TẠO";
                worksheet.Column(41).Width = 30;

                //worksheet.Column(7).AutoFit();

                int row = 3, index = 1;
                foreach (var item in model)
                {
                    var StatusName = string.Empty;

                    if (item.Status == 0)
                    {
                        StatusName = "Chưa tiếp nhận";
                    }
                    else if (item.Status == 1)
                    {
                        StatusName = "Không kiểm kê";
                    }
                    else if (item.Status == 2)
                    {
                        StatusName = "Chưa kiểm kê";
                    }
                    else if (item.Status == 3)
                    {
                        StatusName = "Chờ xác nhận";

                    }
                    else if (item.Status == 4)
                    {
                        StatusName = "Cần chỉnh sửa";
                    }
                    else if (item.Status == 5)
                    {
                        StatusName = "Đã xác nhận";
                    }
                    else if (item.Status == 6)
                    {
                        StatusName = "Giám sát đạt";
                    }
                    else if (item.Status == 7)
                    {
                        StatusName = "Giám sát không đạt";

                    }

                    //Quantity == 0 and status == 0(Chưa tiếp nhận) or status == 1(Không kiểm kê) or status == 2(Chưa kiểm kê) ==> Quantity is Empty
                    var newQty = string.Empty;
                    if((item.Status == 0 || item.Status == 1 || item.Status == 2) && item.Quantity == 0)
                    {
                        newQty = string.Empty;
                    }
                    else
                    {
                        newQty = item.Quantity.ToString();
                    }

                    worksheet.Cells[row, 1].Value = index;
                    worksheet.Cells[row, 2].Value = item.InventoryName;
                    worksheet.Cells[row, 3].Value = item.Department;
                    worksheet.Cells[row, 4].Value = item.Location;
                    worksheet.Cells[row, 5].Value = item.DocCode;
                    worksheet.Cells[row, 6].Value = item.Plant;
                    worksheet.Cells[row, 7].Value = item.WHLoc;
                    worksheet.Cells[row, 8].Value = item.ComponentCode;
                    worksheet.Cells[row, 9].Value = item.ModelCode;
                    worksheet.Cells[row, 10].Value = item.ComponentName;
                    worksheet.Cells[row, 11].Value = newQty;
                    worksheet.Cells[row, 12].Value = item.Position;
                    worksheet.Cells[row, 13].Value = StatusName;
                    worksheet.Cells[row, 14].Value = item.StockType;
                    worksheet.Cells[row, 15].Value = item.SpecialStock;
                    worksheet.Cells[row, 16].Value = item.SaleOrderNo;
                    worksheet.Cells[row, 17].Value = item.SaleOrderList;
                    worksheet.Cells[row, 18].Value = item.AssigneeAccount;
                    worksheet.Cells[row, 19].Value = item.ReceiveBy;
                    worksheet.Cells[row, 20].Value = item.ReceiveAt;
                    worksheet.Cells[row, 21].Value = item.InventoryBy;
                    worksheet.Cells[row, 22].Value = item.InventoryAt;
                    worksheet.Cells[row, 23].Value = item.ConfirmAt;
                    worksheet.Cells[row, 24].Value = item.AuditBy;
                    worksheet.Cells[row, 25].Value = item.AuditAt;
                    worksheet.Cells[row, 26].Value = item.SapInventoryNo;
                    worksheet.Cells[row, 27].Value = item.AssemblyLoc;
                    worksheet.Cells[row, 28].Value = item.VendorCode;
                    worksheet.Cells[row, 29].Value = item.PhysInv;
                    worksheet.Cells[row, 30].Value = item.FiscalYear;
                    worksheet.Cells[row, 31].Value = item.Item;
                    worksheet.Cells[row, 32].Value = item.PlannedCountDate;
                    worksheet.Cells[row, 33].Value = item.ColumnC;
                    worksheet.Cells[row, 34].Value = item.ColumnN;
                    worksheet.Cells[row, 35].Value = item.ColumnO;
                    worksheet.Cells[row, 36].Value = item.ColumnP;
                    worksheet.Cells[row, 37].Value = item.ColumnQ;
                    worksheet.Cells[row, 38].Value = item.ColumnR;
                    worksheet.Cells[row, 39].Value = item.ColumnS;
                    worksheet.Cells[row, 40].Value = item.CreatedBy;
                    worksheet.Cells[row, 41].Value = item.CreatedAt;
                    row++;
                    index++;
                }


                var ExportData = package.GetAsByteArray();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Data = ExportData,
                    Message = "Xuất Excel thành công!"
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = null,
                Message = "Xuất Excel thành công!"
            };
        }

        public async Task<ResponseModel> ExportTxtSummaryInventoryDocument(List<DocumentResultViewModel> model)
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

            // Tạo dữ liệu mẫu cho file txt
            var txtData = new List<string>();

            foreach (var item in model)
            {
                var dataRow = $"{Utilities.RemoveNewLineAndCarriageReturn(item?.Plant)}\t" +
                    $"{Utilities.RemoveNewLineAndCarriageReturn(item?.WHLoc)}\t" +
                    $"{Utilities.RemoveNewLineAndCarriageReturn(item?.ColumnC)}\t" +
                    $"{Utilities.RemoveNewLineAndCarriageReturn(item?.SpecialStock)}\t" +
                    $"{Utilities.RemoveNewLineAndCarriageReturn(item?.StockType)}\t" +
                    $"{Utilities.RemoveNewLineAndCarriageReturn(item?.SaleOrderNo)}\t" +
                    $"{Utilities.RemoveNewLineAndCarriageReturn(item?.SaleOrderList)}\t" +
                    $"{Utilities.RemoveNewLineAndCarriageReturn(item?.PhysInv)}\t" +
                    $"{Utilities.RemoveNewLineAndCarriageReturn(item?.FiscalYear?.ToString())}\t" +
                    $"{Utilities.RemoveNewLineAndCarriageReturn(item?.Item)}\t" +
                    $"{Utilities.RemoveNewLineAndCarriageReturn(item?.PlannedCountDate)}\t" +
                    $"{Utilities.RemoveNewLineAndCarriageReturn(item?.ComponentCode)}\t" +
                    $"{Utilities.RemoveNewLineAndCarriageReturn(item?.ComponentName)}\t" +
                    $"{Utilities.RemoveNewLineAndCarriageReturn(item?.Quantity?.ToDisplayValue())}\t" +
                    $"{Utilities.RemoveNewLineAndCarriageReturn(item?.AccountQuantity.ToDisplayValue())}";
                txtData.Add(dataRow + "\n");
            }

            // Chuyển đổi nội dung thành mảng byte
            var data = Encoding.UTF8.GetBytes(string.Join("", txtData));

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = data,
                Message = "Xuất File txt thành công!"
            };

        }

        public async Task<ResponseModel> ExportListInventoryDocumentHistory(List<ListDocumentHistoryModel> model)
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
                var worksheet = package.Workbook.Worksheets.Add("Danh Sách Lịch Sử Kiểm Kê");

                // Điều chỉnh kiểu chữ và cỡ chữ của trang tính
                worksheet.Cells.Style.WrapText = false;

                worksheet.Cells[1, 4].Style.Font.Bold = true;
                worksheet.Cells[1, 4].Value = "DANH SÁCH LỊCH SỬ KIỂM KÊ";

                // Gán tiêu đề cột cho trang tính
                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "STT";
                worksheet.Column(1).Width = 10;

                worksheet.Cells[2, 2].Style.Font.Bold = true;
                worksheet.Cells[2, 2].Value = "ĐỢT KIỂM KÊ";
                worksheet.Column(2).Width = 20;

                worksheet.Cells[2, 3].Style.Font.Bold = true;
                worksheet.Cells[2, 3].Value = "PHÒNG BAN";
                worksheet.Column(3).Width = 20;

                worksheet.Cells[2, 4].Style.Font.Bold = true;
                worksheet.Cells[2, 4].Value = "KHU VỰC";
                worksheet.Column(4).Width = 20;

                worksheet.Cells[2, 5].Style.Font.Bold = true;
                worksheet.Cells[2, 5].Value = "MÃ PHIẾU";
                worksheet.Column(5).Width = 20;

                worksheet.Cells[2, 6].Style.Font.Bold = true;
                worksheet.Cells[2, 6].Value = "MÃ LINH KIỆN";
                worksheet.Column(6).Width = 20;
                //worksheet.Column(6).AutoFit();

                worksheet.Cells[2, 7].Style.Font.Bold = true;
                worksheet.Cells[2, 7].Value = "MODEL CODE";
                worksheet.Column(7).Width = 20;
                //worksheet.Column(7).AutoFit();

                worksheet.Cells[2, 8].Style.Font.Bold = true;
                worksheet.Cells[2, 8].Value = "TÊN LINH KIỆN";
                worksheet.Column(8).Width = 20;

                worksheet.Cells[2, 9].Style.Font.Bold = true;
                worksheet.Cells[2, 9].Value = "LOẠI THAO TÁC";
                worksheet.Column(9).Width = 20;

                worksheet.Cells[2, 10].Style.Font.Bold = true;
                worksheet.Cells[2, 10].Value = "GHI CHÚ";
                worksheet.Column(10).Width = 30;

                worksheet.Cells[2, 11].Style.Font.Bold = true;
                worksheet.Cells[2, 11].Value = "CHANGE LOG";
                worksheet.Column(11).Width = 60;

                worksheet.Cells[2, 12].Style.Font.Bold = true;
                worksheet.Cells[2, 12].Value = "NGƯỜI THAO TÁC";
                worksheet.Column(12).Width = 20;

                worksheet.Cells[2, 13].Style.Font.Bold = true;
                worksheet.Cells[2, 13].Value = "THỜI GIAN THAO TÁC";
                worksheet.Column(13).Width = 20;

                int row = 3, index = 1;

                foreach (var item in model)
                {
                    worksheet.Cells[row, 1].Value = index;
                    worksheet.Cells[row, 2].Value = item.InventoryName;
                    worksheet.Cells[row, 3].Value = item.Department;
                    worksheet.Cells[row, 4].Value = item.Location;
                    worksheet.Cells[row, 5].Value = item.DocCode;
                    worksheet.Cells[row, 6].Value = item.ComponentCode;
                    worksheet.Cells[row, 7].Value = item.ModelCode;
                    worksheet.Cells[row, 8].Value = item.ComponentName;
                    worksheet.Cells[row, 9].Value = item.Action;
                    worksheet.Cells[row, 10].Value = item.Comment;
                    worksheet.Cells[row, 11].Value = item.ChangeLog;
                    worksheet.Cells[row, 12].Value = item.AssigneeAccount;
                    worksheet.Cells[row, 13].Value = item.AssigneeAccountDate;
                    row++;
                    index++;
                }

                var ExportData = package.GetAsByteArray();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Data = ExportData,
                    Message = "Xuất Excel thành công!"
                };
            }
        }

        public async Task<ResponseModel> ExportQRCode(List<ListDocTypeCToExportQRCodeModel> model)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet1");

            worksheet.PageSetup.Margins.SetLeft(0);
            worksheet.PageSetup.Margins.SetRight(0);
            worksheet.PageSetup.Margins.SetTop(0);
            worksheet.PageSetup.Margins.SetBottom(0);
            worksheet.PageSetup.CenterHorizontally = true;
            worksheet.PageSetup.CenterVertically = true;

            int index1 = 0, index2 = 0, count = 0, heightCount = 1, pageBreakInterval = 6, countPageBreakInterval = 0;

            // Create QR code using QRCoder
            var qrGenerator = new QRCodeGenerator();

            // Generate QRCode:
            for (int i = 0; i < model.Count(); i++)
            {
                var qrCodeData = qrGenerator.CreateQrCode(model[i].ModelCode, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);

                // Set drawQuietZones to false to remove the white border
                var qrCodeBytes = qrCode.GetGraphic(20);


                // Convert QRCode image to SKBitmap (SkiaSharp)
                //SKBitmap skBitmap;
                //using (var ms = new MemoryStream(qrCodeBytes))
                //{
                //    skBitmap = SKBitmap.Decode(ms);
                //}

                // Remove padding by resizing the SKBitmap
                //skBitmap = RemovePadding(skBitmap);

                using (var ms = new MemoryStream(qrCodeBytes))
                {
                    //skBitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
                    //ms.Position = 0;
                    ms.Seek(0, SeekOrigin.Begin);

                    if (i % 2 == 0)
                    {
                        heightCount++;
                        index1 = (i + 1) + count;
                        index2 = index1 + 1;
                        worksheet.Range($"A{index1}:A{index2}").Merge();
                        worksheet.Row(index1).Height = 29;
                        worksheet.Row(index2).Height = 69;
                        var offset = worksheet.Cell($"A{index1}");
                        worksheet.AddPicture(ms).MoveTo(offset, 4, 5).WithSize(121, 121);
                        worksheet.Column("A").Width = 16.3;
                        worksheet.Cell($"A{index1}").Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

                        worksheet.Cell($"B{index1}").Value = model[i].ModelCode;
                        worksheet.Cell($"B{index2}").Value = model[i].StageName;
                        worksheet.Column("B").Width = 27.9;
                        worksheet.Column("B").Style.Alignment.WrapText = true;
                        worksheet.Cell($"B{index1}").Style.Font.FontSize = 20;

                        worksheet.Range($"A{index1}:B{index2}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell($"B{index1}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell($"B{index1}").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell($"B{index1}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        worksheet.Cell($"B{index2}").Style.Font.FontSize = 14;
                        worksheet.Cell($"B{index2}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell($"B{index2}").Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                        worksheet.Cell($"B{index2}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        count++;
                    }
                    else
                    {
                        heightCount++;
                        worksheet.Range($"D{index1}:D{index2}").Merge();
                        worksheet.Row(index1).Height = 29;
                        worksheet.Row(index2).Height = 69;
                        var offset = worksheet.Cell($"D{index1}");
                        worksheet.AddPicture(ms).MoveTo(offset, 4, 5).WithSize(121, 121);
                        worksheet.Column("D").Width = 16.3;
                        worksheet.Cell($"D{index1}").Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

                        worksheet.Cell($"E{index1}").Value = model[i].ModelCode;
                        worksheet.Cell($"E{index2}").Value = model[i].StageName;
                        worksheet.Column("E").Width = 27.9;
                        worksheet.Column("E").Style.Alignment.WrapText = true;
                        worksheet.Cell($"E{index1}").Style.Font.FontSize = 20;

                        worksheet.Range($"D{index1}:E{index2}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell($"E{index1}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell($"E{index1}").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell($"E{index1}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        worksheet.Cell($"E{index2}").Style.Font.FontSize = 14;
                        worksheet.Cell($"E{index2}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell($"E{index2}").Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                        worksheet.Cell($"E{index2}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        countPageBreakInterval++;
                    }

                    worksheet.Column("C").Width = 2;
                }

                if (countPageBreakInterval == pageBreakInterval)
                {
                    worksheet.PageSetup.AddHorizontalPageBreak(index2 + 1);
                    countPageBreakInterval = 0;
                }
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Data = stream.ToArray(),
                    Message = "Xuất excel QRCode thành công!"
                };
            }

        }

        private SKBitmap RemovePadding(SKBitmap bitmap)
        {
            // Find the bounding box of non-white pixels
            int minX = bitmap.Width;
            int minY = bitmap.Height;
            int maxX = 0;
            int maxY = 0;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y) != SKColors.White)
                    {
                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);
                    }
                }
            }

            // Calculate the new size without padding
            int newWidth = maxX - minX + 1;
            int newHeight = maxY - minY + 1;

            // Create a new bitmap with the new size
            SKBitmap newBitmap = new SKBitmap(newWidth, newHeight);

            // Copy pixels from the original bitmap to the new bitmap
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    SKColor pixelColor = bitmap.GetPixel(minX + x, minY + y);
                    newBitmap.SetPixel(x, y, pixelColor);
                }
            }

            return newBitmap;
        }

        public async Task<ResponseModel> ExportInventoryError(List<ListDocToExportInventoryErrorModel> model)
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
            using (var package = new ExcelPackage())
            {
                foreach (var item in model)
                {
                    var worksheet = package.Workbook.Worksheets.Add($"{item.ComponentCode}");

                    worksheet.Cells.Style.WrapText = false;

                    worksheet.Cells[1, 2].Style.Font.Bold = true;
                    worksheet.Cells[1, 2].Value = "Chi tiết phiếu";

                    worksheet.Cells[2, 2].Style.Font.Bold = true;
                    worksheet.Cells[2, 2].Value = $"{item.ComponentCode}";
                    worksheet.Column(2).Width = 20;
                    worksheet.Column(3).Width = 10;
                    worksheet.Column(4).Width = 20;
                    worksheet.Column(5).Width = 20;

                    worksheet.Cells[2, 4].Style.Font.Bold = true;
                    worksheet.Cells[2, 4].Value = $"{item.ComponentName}";

                    worksheet.Cells[3, 2].Style.Font.Bold = true;
                    worksheet.Cells[3, 2].Value = "Total Qty: ";

                    worksheet.Cells[3, 3].Style.Font.Bold = true;
                    worksheet.Cells[3, 3].Value = $"{item.TotalQuantity}";

                    worksheet.Cells[3, 4].Style.Font.Bold = true;
                    worksheet.Cells[3, 4].Value = "Account Qty: ";

                    worksheet.Cells[3, 5].Style.Font.Bold = true;
                    worksheet.Cells[3, 5].Value = $"{item.AccountQuantity}";

                    worksheet.Cells[4, 2].Style.Font.Bold = true;
                    worksheet.Cells[4, 2].Value = "Chênh lệch: ";

                    worksheet.Cells[4, 3].Style.Font.Bold = true;
                    worksheet.Cells[4, 3].Value = $"{item.ErrorQuantity}";

                    worksheet.Cells[4, 4].Style.Font.Bold = true;
                    worksheet.Cells[4, 4].Value = "Giá trị: ";

                    worksheet.Cells[4, 5].Style.Font.Bold = true;
                    worksheet.Cells[4, 5].Value = $"{item.ErrorMoney}";


                    var detailDocs = item.detailDocuments;
                    int rowDoc = 6, rowBom = 0;
                    foreach (var doc in detailDocs)
                    {
                        worksheet.Cells[rowDoc, 2].Style.Font.Bold = true;
                        worksheet.Cells[rowDoc, 2].Value = $"{doc.DocCode}";

                        worksheet.Cells[rowDoc, 4].Style.Font.Bold = true;
                        worksheet.Cells[rowDoc, 4].Value = $"Số lượng/thùng";

                        worksheet.Cells[rowDoc, 5].Style.Font.Bold = true;
                        worksheet.Cells[rowDoc, 5].Value = $"Số thùng";

                        worksheet.Cells[rowDoc + 1, 2].Style.Font.Bold = true;
                        worksheet.Cells[rowDoc + 1, 2].Value = $"{doc.Location}";

                        rowBom = rowDoc + 1;
                        if (doc.detailDocOutputs.Any())
                        {
                            foreach (var docOutPut in doc.detailDocOutputs)
                            {
                                worksheet.Cells[rowBom, 4].Style.Font.Bold = true;
                                worksheet.Cells[rowBom, 4].Value = $"{docOutPut.QuantityPerBom}";

                                worksheet.Cells[rowBom, 5].Style.Font.Bold = true;
                                worksheet.Cells[rowBom, 5].Value = $"{docOutPut.QuantityOfBom}";
                                rowBom++;
                            }

                            worksheet.Cells[rowBom, 4].Style.Font.Bold = true;
                            worksheet.Cells[rowBom, 4].Value = $"Tổng:";

                            worksheet.Cells[rowBom, 5].Style.Font.Bold = true;
                            worksheet.Cells[rowBom, 5].Value = $"{doc.Quantity}";
                        }
                        rowDoc = rowBom + 2;

                    }

                }
                var ExportData = package.GetAsByteArray();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Data = ExportData,
                    Message = "Xuất Excel thành công!"
                };
            }
            
        }
    }
}
