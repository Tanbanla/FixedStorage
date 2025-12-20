namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ListInventoryDocumentFullModel
    {
        public Guid Id { get; set; }
        public int DocType { get; set; }
        public string InventoryName { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string DocCode { get; set; }
        public string Plant { get; set; }
        public string WHLoc { get; set; }
        public string ComponentCode { get; set; }
        public string ModelCode { get; set; }
        public string ComponentName { get; set; }
        public double? Quantity { get; set; }
        public string Position { get; set; }
        public int Status { get; set; }
        public string StockType { get; set; }
        public string SpecialStock { get; set; }
        public string SaleOrderNo { get; set; }
        public string AssigneeAccount { get; set; }
        public string SaleOrderList { get; set; }
        public string ReceiveBy { get; set; }
        public string ReceiveAt { get; set; }
        public string InventoryBy { get; set; }
        public string InventoryAt { get; set; }
        public string ConfirmBy { get; set; }
        public string ConfirmAt { get; set; }
        public string AuditBy { get; set; }
        public string AuditAt { get; set; }
        public string SapInventoryNo { get; set; }
        public string AssemblyLoc { get; set; }
        public string VendorCode { get; set; }
        public string PhysInv { get; set; }
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
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public DateTime OrderByCreatedAt { get; set; }
        public int? FiveNumberDocCode { get; set; }
    }

    public class DocumentResultViewModel : ListInventoryDocumentFullModel
    {
        public double TotalQuantity { get; set; }
        public Guid InventoryId { get; set; }
        public string ConfirmedBy { get; set; }
        public string ProductOrderNo { get; set; }
        public double UnitPrice { get; set; }
        public double ErrorQuantity { get; set; }
        public double AccountQuantity { get; set; }
        public string ConfirmedAt { get; set; }
        public string InventoryAt { get; set; }
        public double ErrorMoney { get; set; }

        //Phục vụ order by theo doc code
        public int FiveNumberDocCode { get; set; }
        public int ConvertedDocCode { get; set; }
        public string CSAP {get;set;}
        public string KSAP {get;set;}
        public string MSAP {get;set;}
        public string OSAP { get; set; }
        public string No { get; set; }
        public double ErrorQuantityAbs { get; set; }
        public double ErrorMoneyAbs { get; set; }

    }
}
