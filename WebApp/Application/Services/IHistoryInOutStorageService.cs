namespace WebApp.Application.Services
{
    public interface IHistoryInOutStorageService
    {
        Task<ResponseModel> ExportExcelHistoryInOutStorageAsync(IEnumerable<HistoryInOutExportResultDto> model, string templateName);
    }
}
