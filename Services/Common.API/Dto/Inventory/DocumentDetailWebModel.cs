namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class DocumentDetailWebModel : DocumentDetailModel
    {
        public Guid InventoryId { get; set; }
        public Guid DocumentId { get; set; }
        public string InventoryName { get; set; }
        public DateTime? InventoryDate { get; set; }
        public string DocCode { get; set; }
        //public string DocCodeCondition { get; set; }
        public int DocType { get; set; }
        public string Plant { get; set; }
        public string WHLoc { get; set; }
        public string ComponentCode { get; set; }
        public string ModelCode { get; set; }
        public string StageName { get; set; }
        public string ComponentName { get; set; }
        public double? Quantity { get; set; }
        public string PositionCode { get; set; }
        public string SaleOrderNo { get; set; }
        public string DepartmentName { get; set; }
        public string LocationName { get; set; }
        public string AssigneeAccount { get; set; }
        public string StockType { get; set; }
        public string SpecialStock { get; set; }
        public string SaleOrderList { get; set; }
        public string AssemblyLoc { get; set; }
        public string VendorCode { get; set; }
        public string PhysInv { get; set; }
        public string ProOrderNo { get; set; }
        public int? FiscalYear { get; set; }
        public string Item { get; set; }
        public string PlannedCountDate { get; set; }
        public string ColumnC { get; set; }
        public string ColumnN { get; set; }
        public string ColumnO { get; set; }
        public string ColumnP { get; set; }
        public string ColumnQ { get; set; }
        public string ColumnR { get; set; }
        public string ColumnS { get; set; }
        public string Note { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string SapInventoryNo { get; set; }

        public DateTime? ConfirmedAt { get; set; }
        public string ConfirmedBy { get; set; }
        public DateTime? AuditedAt { get; set; }
        public string AuditedBy { get; set; }
        public string ReceivedBy { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public string EnvicenceImage { get; set; }
        public string EnvicenceImageTitle { get; set; }
        public string Investigator { get; set; }
        public string ReasonInvestigator { get; set; }
        public DateTime? InvestigateTime { get; set; }
    }
}
