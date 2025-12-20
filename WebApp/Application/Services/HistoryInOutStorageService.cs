namespace WebApp.Application.Services
{
    public partial class HistoryInOutStorageService : IHistoryInOutStorageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public HistoryInOutStorageService(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment hostingEnvironment)
        {
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<ResponseModel> ExportExcelHistoryInOutStorageAsync(IEnumerable<HistoryInOutExportResultDto> model, string templateName)
        {
            var filePath = _hostingEnvironment.WebRootPath + @$"/FileExcelTemplate/{templateName}";
            using (var memStream = new MemoryStream())
            {
                using var source = File.Open(filePath, FileMode.Open);
                source.CopyTo(memStream);

                using (var package = new ExcelPackage(memStream))
                {
                    var workSheet = package.Workbook.Worksheets.FirstOrDefault();
                    workSheet.Cells.Style.WrapText = false;
                    workSheet.Cells[1, 1, 1, 10].Style.Font.Bold = true;
                    var row = 3;
                    var index = 1;
                    foreach (var x in model)
                    {

                        workSheet.Cells[row, 1].Value = index;

                        workSheet.Cells[row, 2].Value = x.UserCode;
                        workSheet.Cells[row, 3].Value = x.UserName;
                        workSheet.Cells[row, 4].Value = x.CreateDate != DateTime.MinValue ? x.CreateDate.ToString("dd/MM/yyyy hh:mm") : string.Empty;
                        workSheet.Cells[row, 5].Value = x.DepartmentName;
                        workSheet.Cells[row, 6].Value = x.ActivityType == 0 ? "Nhập kho" : " Xuất kho";
                        workSheet.Cells[row, 7].Value = x.ComponentCode;
                        workSheet.Cells[row, 8].Value = x.PositionCode;
                        workSheet.Cells[row, 9].Value = x.Quantity;
                        workSheet.Cells[row, 10].Value = x.Note;
                        row++;
                        index++;
                    }

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Data = package.GetAsByteArray(),
                        Message = "Xuất Excel thành công!"
                    };
                };
            }

        }
    }
}
