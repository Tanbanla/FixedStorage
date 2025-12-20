namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ListInventoryDocumentModel
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public string DocCode { get; set; }
        //public string DocCodeCondition { get; set; }
        public int DocType { get; set; }
        public string Plant { get; set; }
        public string WHLoc { get; set; }
        public string ComponentCode { get; set; }
        public string ModelCode { get; set; }
        public string StageName { get; set; }
        public string ComponentName { get; set; }
        public double Quantity { get; set; }
        public string Position { get; set; }
        public string SaleOrderNo { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string AssigneeAccount { get; set; }
        public string StockType { get; set; }
        public string SpecialStock { get; set; }
        public string SaleOrderList { get; set; }
        public string AssemblyLoc { get; set; }
        public string VendorCode { get; set; }
        public string PhysInv { get; set; }
        public string ProOrderNo { get; set; }
        public int FiscalYear { get; set; }
        public string Item { get; set; }
        public string PlantedCount { get; set; }
        public string ColumnC { get; set; }
        public string ColumnN { get; set; }
        public string ColumnO { get; set; }
        public string ColumnP { get; set; }
        public string ColumnQ { get; set; }
        public string ColumnR { get; set; }
        public string ColumnS { get; set; }
        public string Note { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string SAPInventoryNo { get; set; }
        public int FiveNumberFromDocCode { get; set; }

    }
}
