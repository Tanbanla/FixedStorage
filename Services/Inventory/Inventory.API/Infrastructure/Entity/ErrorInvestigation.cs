using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;
using BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Enums;

namespace BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity
{
#nullable enable
    public class ErrorInvestigation : AuditEntity<Guid>
    {
        public Guid InventoryId { get; set; }
        public required string ComponentCode { get; set; }
        public  string? ComponentName { get; set; }
        public bool IsDelete { get; set; }
        public Guid? InvestigatorId { get; set; }
        public ErrorInvestigationStatusType Status { get; set; }
        public string? Note { get; set; }
        public int AdjustmentNo { get; set; }
        public Guid? CurrentInvestigatorId { get; set; }
        public string? InvestigatorUserCode { get; set; }

        //public required string DocCode { get; set; }
        //public InventoryDocType DocType { get; set; }
        //public required string Plant { get; set; }
        //public required string WareHouseLocation { get; set; }
        //public string? PositionCode { get; set; }
        //public double TotalQuantity { get; set; }
        //public double AccountQuantity { get; set; }
        //public double ErrorQuantity { get; set; }
        //public double? ErrorMoney { get; set; }
        //public double? UnitPrice { get; set; }
        //public Guid? AssignedAccount { get; set; }
        //public string? InventoryBy { get; set; }
        //public double? AdjustedQuantity { get; set; }
        //public double? QuantityDifference { get; set; }

        public virtual ICollection<ErrorInvestigationHistory>? ErrorInvestigationHistories { get; set; }

        public virtual Inventory? Inventory { get; set; }

        public ICollection<ErrorInvestigationInventoryDoc>? ErrorInvestigationInventoryDocs { get; set; }



    }
}
