using BIVN.FixedStorage.Services.Common.API.Dto.Component;

namespace WebApp.Application.Services
{
    public partial class ComponentService : IComponentService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ComponentService(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment hostingEnvironment)
        {
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<ResponseModel> ExportFilteredComponentListAsync(List<ComponentFilterItemResultDto> model, string templateName)
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


            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "FileExcelTemplate", templateName);

            // Tạo tệp Excel template mới
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh Sách Linh Kiện");

                // Điều chỉnh kiểu chữ và cỡ chữ của trang tính
                worksheet.Cells.Style.WrapText = false;

                worksheet.Cells[1, 4].Style.Font.Bold = true;
                worksheet.Cells[1, 4].Value = "DANH SÁCH LINH KIỆN";

                // Gán tiêu đề cột cho trang tính
                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "NO";
                worksheet.Column(1).Width = 10;

                worksheet.Cells[2, 2].Style.Font.Bold = true;
                worksheet.Cells[2, 2].Value = "Mã linh kiện";
                worksheet.Column(2).Width = 20;

                worksheet.Cells[2, 3].Style.Font.Bold = true;
                worksheet.Cells[2, 3].Value = "Tên linh kiện";
                worksheet.Column(3).Width = 40;

                worksheet.Cells[2, 4].Style.Font.Bold = true;
                worksheet.Cells[2, 4].Value = "Mã nhà cung cấp";
                worksheet.Column(4).Width = 20;

                worksheet.Cells[2, 5].Style.Font.Bold = true;
                worksheet.Cells[2, 5].Value = "Tên nhà cung cấp";
                worksheet.Column(5).Width = 60;

                worksheet.Cells[2, 6].Style.Font.Bold = true;
                worksheet.Cells[2, 6].Value = "Tên nhà cung cấp rút gọn";
                worksheet.Column(6).Width = 20;

                worksheet.Cells[2, 7].Style.Font.Bold = true;
                worksheet.Cells[2, 7].Value = "Vị trí cố định";
                worksheet.Column(7).Width = 20;
                //worksheet.Column(6).AutoFit();

                worksheet.Cells[2, 8].Style.Font.Bold = true;
                worksheet.Cells[2, 8].Value = "Trạng thái tồn kho Min";
                worksheet.Column(8).Width = 20;

                worksheet.Cells[2, 9].Style.Font.Bold = true;
                worksheet.Cells[2, 9].Value = "Trạng thái tồn kho Max";
                worksheet.Column(9).Width = 20;

                worksheet.Cells[2, 10].Style.Font.Bold = true;
                worksheet.Cells[2, 10].Value = "Tồn kho thực tế";
                worksheet.Column(10).Width = 20;

                worksheet.Cells[2, 11].Style.Font.Bold = true;
                worksheet.Cells[2, 11].Value = "Thông tin LK";
                worksheet.Column(11).Width = 40;

                worksheet.Cells[2, 12].Style.Font.Bold = true;
                worksheet.Cells[2, 12].Value = "Ghi chú";
                worksheet.Column(12).Width = 30;
                //worksheet.Column(7).AutoFit();


                //worksheet.Column(8).AutoFit();

                int row = 3, index = 1;

                foreach (var item in model)
                {
                    worksheet.Cells[row, 1].Value = index;
                    worksheet.Cells[row, 2].Value = item.ComponentCode;
                    worksheet.Cells[row, 3].Value = item.ComponentName;
                    worksheet.Cells[row, 4].Value = item.SupplierCode;
                    worksheet.Cells[row, 5].Value = item.SupplierName;
                    worksheet.Cells[row, 6].Value = item.SupplierShortName;
                    worksheet.Cells[row, 7].Value = item.ComponentPosition;
                    worksheet.Cells[row, 8].Value = item.MinInventoryNumber;
                    worksheet.Cells[row, 9].Value = item.MaxInventoryNumber;
                    worksheet.Cells[row, 10].Value = item.InventoryNumber;
                    worksheet.Cells[row, 11].Value = item.ComponentInfo;
                    worksheet.Cells[row, 12].Value = item.Note;
                    row++;
                    index++;
                }

                // Lưu trang tính vào tệp Excel template
                //package.SaveAs(new FileInfo(filePath));

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

        public async Task<bool> ImportComponentListFromExcel(List<ComponentCellDto> dataModel, string resultFileName)
        {
            var templateFilePath = _hostingEnvironment.WebRootPath + @$"/FileExcelTemplate/TemplateImportComponentList.xlsx";
            dataModel = dataModel.OrderBy(x => x.RowNumber).ToList();
            using (var memStream = new MemoryStream())
            {
                using var template = File.Open(templateFilePath, FileMode.Open);
                if (template != null)
                {
                    template.CopyTo(memStream);
                    using (var package = new ExcelPackage(memStream))
                    {
                        var workSheet = package.Workbook.Worksheets.FirstOrDefault();
                        workSheet.Cells.Style.WrapText = false;
                        workSheet.Cells[1, 1, 1, 13].Style.Font.Bold = true;


                        var row = 2;
                        var noColumnIndex = workSheet.GetColumnByName(commonAPIConstant.ColumnComponentImport.No);
                        var componentCodeColumnIndex = workSheet.GetColumnByName(commonAPIConstant.ColumnComponentImport.ComponentCode);
                        var componentNameColumnIndex = workSheet.GetColumnByName(commonAPIConstant.ColumnComponentImport.ComponentName);
                        var supplierCodeColumnIndex = workSheet.GetColumnByName(commonAPIConstant.ColumnComponentImport.SupplierCode);
                        var supplierNameColumnIndex = workSheet.GetColumnByName(commonAPIConstant.ColumnComponentImport.SupplierName);
                        var supplierShortNameColumnIndex = workSheet.GetColumnByName(commonAPIConstant.ColumnComponentImport.SupplierShortName);
                        var positionCodeColumnIndex = workSheet.GetColumnByName(commonAPIConstant.ColumnComponentImport.PositionCode);
                        var minInventoryNumberColumnIndex = workSheet.GetColumnByName(commonAPIConstant.ColumnComponentImport.MinInventoryNumber);
                        var maxInventoryNumberColumnIndex = workSheet.GetColumnByName(commonAPIConstant.ColumnComponentImport.MaxInventoryNumber);
                        var inventoryNumberColumnIndex = workSheet.GetColumnByName(commonAPIConstant.ColumnComponentImport.InventoryNumber);
                        var componentInfoColumnIndex = workSheet.GetColumnByName(commonAPIConstant.ColumnComponentImport.ComponentInfo);
                        var noteColumnIndex = workSheet.GetColumnByName(commonAPIConstant.ColumnComponentImport.Note);
                        var errorsColumnIndex = workSheet.GetColumnByName(commonAPIConstant.ColumnComponentImport.Errors);
                        if (errorsColumnIndex == 30)
                        {
                            errorsColumnIndex = workSheet.Dimension.Columns;
                        }
                        dataModel.ForEach(x =>
                        {
                            workSheet.Cells[row, noColumnIndex].Value = x.No.Value;
                            workSheet.Cells[row, componentCodeColumnIndex].Value = x.ComponentCode;
                            workSheet.Cells[row, componentNameColumnIndex].Value = x.ComponentName;
                            workSheet.Cells[row, supplierCodeColumnIndex].Value = x.SupplierCode;
                            workSheet.Cells[row, supplierNameColumnIndex].Value = x.SupplierName;
                            workSheet.Cells[row, supplierShortNameColumnIndex].Value = x.SupplierShortName;
                            workSheet.Cells[row, positionCodeColumnIndex].Value = x.PositionCode;
                            workSheet.Cells[row, minInventoryNumberColumnIndex].Value = x.MinInventoryNumber;
                            workSheet.Cells[row, maxInventoryNumberColumnIndex].Value = x.MaxInventoryNumber;
                            workSheet.Cells[row, inventoryNumberColumnIndex].Value = x.InventoryNumber;
                            workSheet.Cells[row, componentInfoColumnIndex].Value = x.ComponentInfo;
                            workSheet.Cells[row, noteColumnIndex].Value = x.Note;
                            workSheet.Cells[row, errorsColumnIndex].Value = x.Errors;
                            row++;
                        });
                        var errorsColumn = workSheet.Column(errorsColumnIndex);
                        errorsColumn.Style.WrapText = true;
                        errorsColumn.Width = 150;

                        var folderPath = _hostingEnvironment.WebRootPath + $@"\assets";
                        var filePath = Path.Combine(folderPath, resultFileName);
                        var resultFileInfo = new FileInfo(filePath);
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }
                        if (resultFileInfo.Exists)
                        {
                            resultFileInfo.Delete();
                            resultFileInfo = new FileInfo(Path.Combine(folderPath, resultFileName));
                        }
                        package.SaveAs(resultFileInfo);
                        return await Task.FromResult(true);
                    }
                }
                return await Task.FromResult(false);
            }
        }
    }
}
