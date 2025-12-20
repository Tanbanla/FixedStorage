using BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity;

namespace Inventory.API.Infrastructure.Entity
{
    [Index(nameof(ComponentCode))]
    [Index(nameof(ModelCode))]
    [Index(nameof(Status))]
    public class InventoryDoc : AuditEntity<Guid>, ISoftDelete
    {
        public InventoryDocType DocType { get; set; }
        [MaxLength(150)]
        public string LocationName { get; set; }
        [MaxLength(250)]
        public string DepartmentName { get; set; }
        public Guid? AssignedAccountId { get; set; }
        /// <summary>
        /// Use as Material Code
        /// </summary>
        [MaxLength(150)]
        public string ComponentCode { get; set; }
        [MaxLength(250)]
        public string ComponentName { get; set; }
        [MaxLength(50)]
        public string PositionCode { get; set; }
        [MaxLength(150)]
        public string Plant { get; set; }
        [MaxLength(150)]
        public string WareHouseLocation { get; set; }
        [MaxLength(50)]
        public string StockType { get; set; }
        public string SpecialStock { get; set; }
        [MaxLength(100)]
        public string VendorCode { get; set; }
        public double Quantity { get; set; }
        public double TotalQuantity { get; set; }
        public double AccountQuantity { get; set; }
        public double ErrorQuantity { get; set; }
        public double? UnitPrice { get; set; }
        [MaxLength(100)]
        public string DocCode { get; set; }
        [MaxLength(50)]
        public string No { get; set; }
        [MaxLength(50)]
        public string SapInventoryNo { get; set; }
        [MaxLength(100)]
        public string ModelCode { get; set; }
        [MaxLength(100)]
        public string MachineModel { get; set; }
        [MaxLength(100)]
        public string MachineType { get; set; }
        [MaxLength(100)]
        public string LineName { get; set; }
        [MaxLength(100)]
        public string LineType { get; set; }
        [MaxLength(10)]
        public string StageNumber { get; set; }
        [MaxLength(100)]
        public string StageName { get; set; }
        public double BomUseQuantity { get; set; }
        [MaxLength(50)]
        public string SalesOrderNo { get; set; }
        [MaxLength(50)]
        public string SaleOrderList { get; set; }
        [MaxLength(50)]
        public string ProductOrderNo { get; set; }
        public string? InventoryBy { get; set; }
        public DateTime? InventoryAt { get; set; }
        public string? ConfirmBy { get; set; }
        public DateTime? ConfirmAt { get; set; }
        public string? AuditBy { get; set; }
        public DateTime? AuditAt { get; set; }
        public Guid? ReceiveBy { get; set; }
        public DateTime? ReceiveAt { get; set; }
        [MaxLength(250)] 
        public string AssemblyLocation { get; set; }
        [MaxLength(50)]
        public string ColumnC { get; set; }
        [MaxLength(50)]
        public string ColumnN { get; set; }
        [MaxLength(50)]
        public string ColumnO { get; set; }
        [MaxLength(50)]
        public string ColumnP { get; set; }
        [MaxLength(50)]
        public string ColumnQ { get; set; }
        [MaxLength(50)]
        public string ColumnR { get; set; }
        [MaxLength(50)]
        public string ColumnS { get; set; }
        public InventoryDocStatus Status { get; set; }
        [MaxLength(250)]
        public string? Note { get; set; }
        [MaxLength(50)]
        public string PhysInv { get; set; }
        [MaxLength(4),MinLength(4)]
        public int? FiscalYear { get; set; }
        [MaxLength(50)]
        public string? Item { get; set; }
        [MaxLength(50)]
        public string? PlannedCountDate { get; set; }
        [MaxLength(50)]
        public string StorageBin { get; set; }
        public Guid? InventoryId { get; set; }
        public string? CSAP { get; set; }
        public string? KSAP { get; set; }
        public string? OSAP { get; set; }
        public string? MSAP { get; set; }
        public double? ErrorMoney { get; set; }
        public int AggregateCount { get; set; }
        public bool? IsDeleted { get; set; }
        public string? Investigator { get; set; }
        public string ReasonInvestigator { get; set; }
        public DateTime? InvestigateTime { get; set; }
        public BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Inventory Inventory { get; set; }

        public ICollection<DocHistory> DocHistories { get; set; }
        public ICollection<DocTypeCDetail> DocTypeCDetails { get; set; }
        public ICollection<DocOutput> DocOutputs { get; set; }

        public ICollection<ErrorInvestigationInventoryDoc>? ErrorInvestigationInventoryDocs { get; set; }
    }
}
