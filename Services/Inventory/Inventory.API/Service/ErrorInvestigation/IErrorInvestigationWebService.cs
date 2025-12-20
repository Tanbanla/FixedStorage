using BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;

namespace Inventory.API.Service.ErrorInvestigation
{
    public interface IErrorInvestigationWebService
    {
        Task<ErrorInvestigationResponseModel<IEnumerable<ListErrorInvestigationWebDto>>> ListErrorInvestigaiton(ListErrorInvestigationWebModel listErrorInvestigation);
        Task<ResponseModel<byte[]>> ExportDataAjustment(Guid invetoryId);
        Task<ErrorInvestigationResponseModel<IEnumerable<ListErrorInvestigationDocumentsDto>>> ListErrorInvestigationDocuments(Guid inventoryId, string componentCode, int pageNum, int pageSize);
        Task<ResponseModel> ListErrorInvestigationDocumentsCheck(string? userCode, Guid inventoryId, string componentCode);
        Task<ResponseModel> ListErrorInvestigationInventoryDocsHistory(string componentCode, ErrorInvestigationInventoryDocsHistoryModel inventories);
        Task<ErrorInvestigationResponseModel<IEnumerable<ListErrorInvestigationHistoryWebDto>>> ListErrorInvestigaitonHistory(ListErrorInvestigationHistoryWebModel listErrorInvestigationHistory);
        Task<ResponseModel> UpdateErrorTypesForInvestigationHistory(List<Guid> errorHistoryIds, AdjustmentType type);
        Task<ResponseModel> InvestigationPercent(Guid inventoryId);
        Task<ResponseModel<IEnumerable<ImportErrorInvestigationUpdatePivotDto>>> ErrorPercent(Guid inventoryId);
        Task<ImportResponseModel<byte[]>> ImportErrorInvestigationUpdate(IFormFile file);
        Task<ImportResponseWithDataModel<byte[], List<ImportErrorInvestigationUpdatePivotDto>>> ImportErrorInvestigationUpdatePivot(IFormFile file, Guid inventoryId);
        Task<ResponseModel<IEnumerable<ErrorCategoryManagementDto>>> ErrorCategoryManagement();
        Task<ResponseModel> AddNewErrorCategoryManagement(ErrorCategoryModel errorCategoryModel);
        Task<ResponseModel> UpdateErrorCategoryManagement(Guid errorCategoryId, ErrorCategoryModel errorCategoryModel);
        Task<ResponseModel> RemoveErrorCategoryManagement(Guid errorCategoryId);
        Task<ResponseModel> ErrorCategoryManagementById(Guid errorCategoryId);
    }
}
