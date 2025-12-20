using BIVN.FixedStorage.Services.Common.API.Dto.QRCode;

namespace Inventory.API.Service
{
    public interface IDocumentResultService
    {
        Task<ImportResponseModel<byte[]>> UploadTotalFromBwins(Guid intventoryId, Guid userId, IFormFile file);
        Task<ImportResponseModel<byte[]>> ImportFileSAP(IFormFile file, string inventoryId, string userId);
        Task<InventoryResponseModel<IEnumerable<ListDocumentHistoryModel>>> ListDocumentHistories(ListDocumentHistoryDto listDocumentHistory);
        Task<ResponseModel<IEnumerable<ListDocTypeCToExportQRCodeModel>>> ListDocTypeCToExportQRCode(ListDocTypeCToExportQRCodeDto listDocTypeCToExportQRCode);
        Task<ResponseModel<IEnumerable<ListDocToExportInventoryErrorModel>>> ListDocumentToInventoryError(ListDocToExportInventoryErrorDto listDocTypeCToExportQRCode);
        Task<ImportResponseModel<byte[]>> ImportMSLDataUpdate(IFormFile file, Guid inventoryId);

        public void DoSomething()
        {
            Console.WriteLine("Do something...");
        }
    }
}
