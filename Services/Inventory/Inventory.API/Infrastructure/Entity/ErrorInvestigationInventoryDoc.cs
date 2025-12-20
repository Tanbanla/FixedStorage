namespace BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity
{
#nullable enable
    public class ErrorInvestigationInventoryDoc : AuditEntity<Guid>
    {
        public Guid ErrorInvestigationId { get; set; }
        public Guid InventoryDocId { get; set; }
        public InventoryDocType DocType { get; set; }
        public required string DocCode { get; set; }
        public double? BOM { get; set; }
        public required string Plant { get; set; }
        public required string WareHouseLocation { get; set; }
        public string? PositionCode { get; set; }
        /// <summary>
        /// Số lượng kiểm kê trên danh sách phiếu
        /// </summary>
        public double? Quantity { get; set; }
        public double? TotalQuantity { get; set; }
        /// <summary>
        /// Số lượng hệ thống trên danh sách sai số
        /// </summary>
        public double? AccountQuantity { get; set; }
        public double? ErrorQuantity { get; set; }
        public double? ErrorMoney { get; set; }
        public double? UnitPrice { get; set; }
        public Guid? AssignedAccount { get; set; }
        public string? InventoryBy { get; set; }
        public double? AdjustedQuantity { get; set; }
        public double? QuantityDifference { get; set; }
        public string? ModelCode { get; set; }
        public string? AttachModule { get; set; }

        public virtual ErrorInvestigation? ErrorInvestigation { get; set; }
        public virtual InventoryDoc? InventoryDoc { get; set; }

    }
}
