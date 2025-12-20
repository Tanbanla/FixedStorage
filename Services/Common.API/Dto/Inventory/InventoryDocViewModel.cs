namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class InventoryDocViewModel
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public int Status { get; set; }
        public int DocType { get; set; }
        public Guid AssignedAccountId { get; set; }
        public string ComponentCode { get; set; }
        public string ComponentName { get; set; }
        public string PositionCode { get; set; }
        public string DocCode { get; set; }
        public string ModelCode { get; set; }
        public string MachineModel { get; set; }
        public string MachineType { get; set; }
        public string LineName { get; set; }
        public string LineType { get; set; }
        public string StageNumber { get; set; }
        public string StageName { get; set; }
        public string SaleOrderNo { get; set; }
        public string Note { get; set; }
        public string InventoryBy { get; set; }
        public string AuditedBy { get; set; }
        public string ConfirmedBy { get; set; }
    }
}
