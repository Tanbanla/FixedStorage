namespace BIVN.FixedStorage.Services.Common.API.Dto.Component
{
    public class ComponentImportInfoResultDto
    {       
        public string FileUrl { get; set; }
        public string FileName { get; set; }        
        public int? FailCount { get; set; }
        public int? SuccessCount { get; set; }
    }
}
