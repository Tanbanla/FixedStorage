namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class LocationAssignedViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DepartmentName { get; set; }
        public string FactoryName { get; set; }
        public bool isAssignedInventory { get; set; }
        public bool isAssignedAudit { get; set; }
        public Guid? UserInventoryId { get; set; }
        public Guid? UserAuditId { get; set; }
    }
}
