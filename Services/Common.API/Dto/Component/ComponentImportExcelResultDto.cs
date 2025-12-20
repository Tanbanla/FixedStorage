namespace BIVN.FixedStorage.Services.Common.API.Dto.Component
{
    public class ComponentImportExcelResultDto
    {
        public byte[]? FileErrorsImportResult { get; set; }
        public int? FailCount { get; set; }
        public int? SuccessCount { get; set; }
    }
}
