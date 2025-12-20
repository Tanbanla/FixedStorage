namespace Inventory.API.Service
{
    public interface IAuditTargetWebService
    {
        Task<ResponseModel<BIVN.FixedStorage.Services.Common.API.AuditTargetViewModel>> GetAuditTargetDetail(Guid inventoryId, Guid auditTargetId);
        Task<ResponseModel<Dictionary<string,string>>> UpdateAuditTarget(Guid inventoryId, Guid auditTargetId, UpdateAuditTargetDto updateAuditTargetDto);
        Task<InventoryResponseModel<IEnumerable<ListAuditTargetViewModel>>> ListAuditTarget(ListAuditTargetDto listAuditTargetDto, Guid inventoryId);
    }
}
