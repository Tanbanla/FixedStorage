using BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation;

namespace WebApp.Application.Services.ErrorInvestigationWeb
{
    public interface IErrorInvestigationWebService
    {
        Task<ResponseModel> ExportListErrorInvestigation(IEnumerable<ListErrorInvestigationWebDto> model, IEnumerable<ErrorCategoryManagementDto> errorCategories);
        Task<ResponseModel> ExportListErrorInvestigationHistory(IEnumerable<ListErrorInvestigationHistoryWebDto> model);
        Task<ResponseModel> ExportListErrorInvestigationDetail(IEnumerable<ListErrorInvestigationHistoryWebDto> model);
    }
}
