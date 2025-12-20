namespace BIVN.FixedStorage.Services.Common.API.Dto.Component
{
    public class ComponentItemImportResultDto
    {
        public ComponentItemImportResultDto(List<ComponentCellDto> failedImportComponents, int? failCount, int successCount)
        {
            FailedImportComponents = failedImportComponents;
            FailCount = failCount;
            SuccessCount = successCount;
        }

        public List<ComponentCellDto> FailedImportComponents { get; set; }

        //public byte[] FileErrorsImportResult { get; set; }

        public int? FailCount { get; set; }
        public int? SuccessCount { get; set; }        
    }
}
