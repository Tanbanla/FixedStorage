namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ImportSAPExcelValueModel
    {
        public string Plant {get;set;}
        public string WHLoc {get;set;}
        public string CSAP {get;set;}
        public string MaterialCode {get;set;}
        public string Description {get;set;}
        public string StorageBin {get;set;}
        public string SONo {get;set;}
        public string StockTypes {get;set;}
        public string PhysInv {get;set;}
        public object Quantity { get; set; }
        public string KSAP { get; set; }
        public object AccountQty { get; set; }
        public string MSAP { get; set; }
        public object ErrorQty { get; set; }
        public string OSAP { get; set; }
        public object UnitPrice { get; set; }
        public object ErrorMoney { get; set; }
        public ModelStateDictionary Error { get; set; } = new();
    }
}
