using BIVN.FixedStorage.Services.Common.API.Dto.Inventory;
using BIVN.FixedStorage.Services.Common.API.Dto.QRCode;

namespace WebApp.Application.Services
{
    public interface IInventoryService
    {
        Task<ResponseModel> ExportListInventory(List<ListInventoryModel> model);
        Task<ResponseModel> ExportListAuditTarget(List<ListAuditTargetViewModel> model);
        Task<ResponseModel> ExportListInventoryDocument(List<ListInventoryDocumentModel> model);
        Task<ResponseModel> ExportListInventoryDocumentFull(List<ListInventoryDocumentFullModel> model);
        Task<ResponseModel> ExportTxtSummaryInventoryDocument(List<DocumentResultViewModel> model);
        Task<ResponseModel> ExportListInventoryDocumentHistory(List<ListDocumentHistoryModel> model);
        Task<ResponseModel> ExportQRCode(List<ListDocTypeCToExportQRCodeModel> model);
        Task<ResponseModel> ExportInventoryError(List<ListDocToExportInventoryErrorModel> model);
    }
}
