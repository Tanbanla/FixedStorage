namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class UploadDocStatusHeaderIndexModel
    {
        public int STT {get;set;}
        public int LocationName {get;set;}
        public int DocCode {get;set;}
        public int ComponentCode {get;set;}
        public int ModelCode {get;set;}
        public int Plant {get;set;}
        public int WHLoc {get;set;}
        public int DocStatus {get;set;}
    }

    public class UploadDocStatusValueModel
    {
        public string STT { get; set; }
        public string LocationName { get; set; }
        public string DocCode { get; set; }
        public string ComponentCode { get; set; }
        public string ModelCode { get; set; }
        public string Plant { get; set; }
        public string WHLoc { get; set; }
        public string DocStatus { get; set; }
        public ModelStateDictionary ErrModel { get; set; } = new();
    }
}
