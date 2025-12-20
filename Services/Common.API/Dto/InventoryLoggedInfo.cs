namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class InventoryLoggedInfo
    {
        public Guid AccountId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        //0: kiểm kê
        //1: giám sát
        public int InventoryRoleType { get; set; }
        public bool HasRoleType { get; set; } = false;
        public InventoryModel InventoryModel { get; set; }
    }
}
