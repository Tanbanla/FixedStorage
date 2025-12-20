namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class AuditTargetViewModel
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public Guid? AccountId { get; set; }
        public string DepartmentName { get; set; }
        public string LocationName { get; set; }
        public string ComponentCode { get; set; }
        public string Plant { get; set; }
        public string WHLoc { get; set; }
        public string PositionCode { get; set; }
        public int Status { get; set; }
        
    }
}
