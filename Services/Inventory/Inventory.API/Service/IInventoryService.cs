namespace BIVN.FixedStorage.Services.Inventory.API.Service
{
    public interface IInventoryService
    {
        Task<ResponseModel<InventoryDocumentImportResultDto>> ImportInventoryDocumentAsync([FromForm] IFormFile file, string type, Guid inventoryId);
        Task<ResponseModel<InventoryDocumentImportResultDto>> ImportAuditTargetAsync([FromForm] IFormFile file, Guid inventoryId);

        Task<ValidateCellTypeCDto> ValidateInventoryDocTypeC([FromForm] IFormFile file, Guid inventoryId,bool isBypassWarning);
        Task<ResponseModel> ImportInventoryDocTypeC([FromForm] IFormFile file, Guid inventoryId, bool isBypassWarning);

        Task<ResponseModel> CheckInventory(string inventoryId, string accountId);

        Task<ResponseModel> CheckValidInventoryDate(Guid inventoryId);
        Task<ResponseModel> CheckValidInventoryRole(Guid accountId, params int[] accessTo);

        Task<ResponseModel> ScanDocsAE(Guid inventoryId, Guid accountId, string componentCode, string positionCode, string docCode, InventoryActionType actionType, bool isErrorInvestigation);
        Task<ResponseModel<DocCListViewModel>> GetDocsC(ListDocCFilterModel listDocCFilterModel);
        Task<ResponseModel<DocumentDetailModel>> DetailOfDocument(DocumentDetailFilterModel documentDetailFilterModel);
        Task<ResponseModel<HistoryDetailViewModel>> HistoryDetail(Guid inventoryId, Guid accountId, Guid historyId, string searchTerm, int page);

        Task<ResponseModel<IEnumerable<string>>> GetModelCodesForDocC(Guid inventoryId, Guid accountId);
        Task<ResponseModel<IEnumerable<MachineTypeModel>>> GetMachineTypesDocC(Guid inventoryId, Guid accountId, string machineModel);
        Task<ResponseModel<IEnumerable<LineModel>>> GetLineNamesDocC(Guid inventoryId, Guid accountId, string machineModel, string machineType);
        Task<ResponseModel> SubmitInventory(string inventoryId, string accountId, string docId, SubmitInventoryDto submitInventoryDto);
        Task<ResponseModel> SubmitConfirm(Guid inventoryId, Guid accountId, Guid docId, SubmitInventoryAction actionType, SubmitInventoryDto submitInventoryDto);
        Task<ResponseModel> DropDownDepartment(string inventoryId, string accountId);
        Task<ResponseModel> DropDownLocation(string inventoryId, string accountId, string departmentName);
        Task<ResponseModel> DropDownComponentCode(string inventoryId, string accountId, string departmentName, string locationName);
        Task<ResponseModel> ListAudit(ListAuditFilterDto listAuditFilterDto);
        Task<ResponseModel<IEnumerable<AuditInfoModel>>> ScanQR(Guid inventoryId, Guid accountId, string componentCode);
        Task<ResponseModel> SubmitAudit(string inventoryId, string accountId, string docId, SubmitInventoryAction actionType, SubmitInventoryDto submitInventoryDto);
        Task<ResponseModel> IsHightLightCheck(CheckIsHightLightDocTypeCDto checkIsHightLightDocTypeCDto);

        Task<ResponseModel> DeleteAuditTargets(Guid inventoryId, List<Guid> IDs, bool isDeleteAll = false);
        Task<ResponseModel> DeleteInventoryDocs(Guid inventoryId, ListInventoryDocumentDeleteDto listInventoryDocumentDeleteDto, bool isDeleteAll = false);

        Task<ResponseModel<IEnumerable<string>>> GetModelCodesForDocB(Guid inventoryId, Guid accountId);
        Task<ResponseModel<IEnumerable<MachineTypeModel>>> GetMachineTypesDocB(Guid inventoryId, Guid accountId, string machineModel);
        Task<ResponseModel<IEnumerable<string>>> GetDropDownModelCodesForDocB(Guid inventoryId, Guid accountId, string machineModel, string machineType);
        Task<ResponseModel<IEnumerable<LineModel>>> GetLineNamesDocB(Guid inventoryId, Guid accountId, string machineModel, string machineType, string modelCode);
        Task<ResponseModel<DocBListViewModel>> GetDocsB(ListDocBFilterModel listDocBFilterModel);
        Task<ResponseModel<DocAEListViewModel>> GetDocsAE(ListDocAEFilterModel listDocAEFilterModel);
        Task<ResponseModel> ScanDocB(ScanDocBFilterModel model, bool isErrorInvestigation);
        Task<ResponseModel<DocCListViewModel>> ListDocC(ListDocCFilterModel listDocCFilterModel);
        Task<ResponseModel<InventoryDocumentImportResultDto>> ImportInventoryDocumentShip([FromForm] IFormFile file, Guid inventoryId);

        Task<ResponseModel> CheckValidAuditTarget(Guid inventoryId, Guid accountId, string componentCode);
    }
}
