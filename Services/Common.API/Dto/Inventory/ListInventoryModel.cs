namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ListInventoryModel 
    {
        public Guid InventoryId { get; set; }
        public string InventoryName { get; set; }
        public DateTime? InventoryDate { get; set; }
        public double? AuditFailPercentage { get; set; }
        public int Status { get; set; }
        public DateTime? CreateAt { get; set; }
        public string FullName { get; set; }

    }
}
