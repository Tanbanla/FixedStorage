namespace Storage.API.Service.Dto
{
    public class InOutMobileResultModel
    {
        public Params @Params { get; set; }
        public ResponseModel Response { get; set; }
    }

    public class Params 
    {
        public string SupplierCode { get; set; }
    }
}
