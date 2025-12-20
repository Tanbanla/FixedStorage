using BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;
using BIVN.FixedStorage.Services.Inventory.API.Service.Dto.ErrorInvestigation;
using Inventory.API.Service.Dto.ErrorInvestigation;

namespace Inventory.API.Service.ErrorInvestigation
{
    public interface IErrorInvestigationService
    {
        Task<ResponseModel> CheckValidInventoryRole(Guid accountId, params int[] accessTo);
        Task<ResponseModel> CheckValidInventoryDate(Guid inventoryId);

        Task<ResponseModel<IEnumerable<ErrorInvestigationListDto>>> ErrorInvestigationList(Guid inventoryId, ErrorInvestigationStatusType? status, string componentCode, int pageSize = 20, int pageNum = 1);
        Task<ResponseModel> UpdateStatusErrorInvestigation(Guid inventoryId, string componentCode);
        Task<ResponseModel> ConfirmErrorInvestigation(Guid inventoryId, string componentCode, ErrorInvestigationConfirmType type, ErrorInvestigationConfirmModel model);
        Task<ResponseModel<ErrorInvestigationDocumentListDto>> ErrorInvestigationDocumentList(string? userCode, Guid inventoryId, string componentCode);
        Task<ResponseModel> ErrorInvestigationConfirmedViewDetail(Guid inventoryId, string componentCode);
        Task<ResponseModel> ErrorInvestigationHistories(Guid inventoryId, string componentCode);
    }
}
