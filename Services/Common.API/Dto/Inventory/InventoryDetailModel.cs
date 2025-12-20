namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class InventoryDetailModel
    {
        public string InventoryDate { get; set; }
        public string InventoryName { get; set; }
        public int Status { get; set; }
        public double AuditFailPercentage { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedAt { get; set; }
        public DateTime? ForceAggregateAt { get; set; }
        public bool IsLocked { get; set; }
    }
}
