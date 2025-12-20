namespace Inventory.API.Service
{
    public interface IInternalService
    {
        Task<ResponseModel<InventoryLoggedInfo>> GetInventoryLoggedInfo(Guid userId);
        Task<ResponseModel> DeleteInventoryAccount(Guid userId);
        Task<ResponseModel> UpdateInventoryAccount(Guid userId, string newUserName);
        Task<ResponseModel<bool>> CheckAuditAccountAssignLocation(Guid userId);
    }
}
