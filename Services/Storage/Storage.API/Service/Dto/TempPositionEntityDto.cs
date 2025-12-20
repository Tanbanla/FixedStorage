namespace Storage.API.Service.Dto
{
    public class TempPositionEntityDto
    {
        public string ComponentCode {get;set;}
        public string ComponentName {get;set;}
        public string SupplierCode {get;set;}
        public string SupplierName {get;set;}
        public int InventoryNumber {get;set;}
        public int MaxInventoryNumber { get; set; }
        public string FactoryId { get; set; }
        public string PositionCode { get; set; }
    }
}
