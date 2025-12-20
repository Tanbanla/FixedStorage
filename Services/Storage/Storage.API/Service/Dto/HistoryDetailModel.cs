namespace Storage.API.Service.Dto
{
    public class HistoryDetailModel
    {
        public string Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string UserName { get; set; }
        public int Type { get; set; }
        public string DepartmentName { get; set; }
        public string ComponentCode { get; set; }
        public string ComponentName { get; set; }
        public string PositionCode { get; set; }
        public string PositionName { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string SupplierShortName { get; set; }
        public double Quantity { get; set; }
        public double InventoryNumber { get; set; }
        public string Note { get; set; }
    }
}
