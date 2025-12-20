using BIVN.FixedStorage.Services.Common.API.Dto.QRCode;

namespace Inventory.API.Service
{
    public interface IInventoryWebService
    {
        Task<ResponseModel<IEnumerable<ListInventoryModel>>> ListInventory(ListInventoryDto listInventory);
        Task<ResponseModel> CreateInventory(CreateInventoryDto createInventoryDto);
        Task<ResponseModel<IEnumerable<ListInventoryModel>>> ListInventoryToExport(ListInventoryDto listInventory);
        Task<ResponseModel> UpdateStatusInventory(string inventoryId, InventoryStatus status, string userId);
        Task<ResponseModel> GetInventoryDetail(string inventoryId);
        Task<ResponseModel> UpdateInventoryDetail(UpdateInventoryDetail updateInventoryDetail, string inventoryId);
        Task<InventoryResponseModel<IEnumerable<ListInventoryDocumentModel>>> GetInventoryDocument(ListInventoryDocumentDto listInventoryDocument, string inventoryId);
        Task<ResponseModel<IEnumerable<Guid>>> GetInventoryDocumentDeleteHasFilter(ListInventoryDocumentDeleteDto listInventoryDocument, string inventoryId);

        Task<ResponseModel> UpdateToReceivedDoc(List<Guid> docIds);
        Task<ResponseModel> DownloadUploadDocStatusFileTemplate();
        Task<ImportResponseModel<byte[]>> UploadChangeDocStatus(IFormFile file);

        /// <summary>
        /// Chi tiết phiếu của màn danh sách phiếu kiểm kê
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <returns></returns>
        Task<ResponseModel<DocumentDetailWebModel>> InventoryDocDetail(string docId, string searchTerm = "");

        Task<ResponseModel<IEnumerable<ListInventoryModel>>> DropdownInventories();
        Task<ResponseModel<InventoryDocsResultSet<List<ListInventoryDocumentFullModel>>>> GetInventoryDocumentFull(ListInventoryDocumentFullDto listInventoryDocumentFull);
        Task<ResponseModel> DeleteInventorys(string inventoryId, List<string> docIds);
        Task<ResponseModel<GetDetailInventoryDocumentModel>> GetDetailInventory(string inventoryId, string docId);
        Task<ResponseModel<IEnumerable<GetDocTypeCDetail>>> GetDocumentTypeC(string inventoryId, string docId, string componentCode);

        Task<ResponseModel<ResultSet<List<DocumentResultViewModel>>>> DocumentResults(DocumentResultListFilterModel filterModel);
        Task<ResponseModel<IEnumerable<DocumentResultViewModel>>> DocumentResultsToExport(DocumentResultListFilterModel filterModel);

        Task<ResponseModel<byte[]>> ExportDocumentResultExcel(DocumentResultListFilterModel filterModel);
        Task<ResponseModel> ExportTreeGroups(Guid inventoryId, string machineModel, string machineType = null);
        Task<ResponseModel<TreeGroupFilterDto>> GetTreeGroupFilters();

        Task<ResponseModel<ResultSet<IEnumerable<DocComponentC>>>> DocCComponents(Guid documentId, int skip, int take, string search);

        Task<ResponseModel> UpdateAllReceiveDoc(List<Guid> excludeIds);
        Task<ResponseModel<bool>> CheckAnyDocAssigned();
        Task<ResponseModel<bool>> CheckDownloadDocTemplate();
        Task<ResponseModel> CheckExistDocTypeA(string inventoryId);

        Task<ResponseModel> AggregateDocResults(Guid inventoryId, string userId);
        Task<ImportResponseModel<byte[]>> ImportUpdateQuantity(IFormFile file, string inventoryName);
        Task<ResponseModel<TreeGroupQRCodeFilterDto>> GetTreeGroupQRCodeFilters();
        Task<ResponseModel<TreeGroupInventoryErrorFilterDto>> GetTreeGroupInventoryErrorFilters();
    }
}
