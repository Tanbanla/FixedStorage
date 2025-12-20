namespace Storage.API.Service.Dto
{
    public class PositionFromDBDto
    {
        public string ComponentCode {get;set;}
        public string SupplierCode {get;set;}
        public string PositionCode { get;set;}
        public string ComponentName {get;set;}
        public string SupplierName {get;set;}
        public double InventoryNumber {get;set;}
        public double MaxInventoryNumber { get; set; }
        public Guid FactoryId { get; set;}
    }
}
