namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public  class UploadTotalBwinExcelValueModel
    {
        public string MaterialCode {get;set;}
        public string Plant {get;set;}
        public string WHLoc {get;set;}
        public object Quantity { get; set; }
        public ModelStateDictionary Error { get; set; } = new();

        //Nếu có phiếu A thì fill vào các trường sau để tạo phiếu E, và có điều kiện để tìm kiếm trên web
        public string DepartmentName { get; set; }
        public string LocationName { get; set; }
        public Guid AssignedAccount { get; set; }
    }
}
