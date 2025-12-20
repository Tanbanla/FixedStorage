namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class InventoryDocumentImportResultDto
    {
        //public List<FailedImportInventoryDocumentDto> FailedImportInventoryDocuments { get; set; }

        public int Code { get; set; }
        public int? FailCount { get; set; }
        public int? SuccessCount { get; set; }
        public byte[] Result { get; set; }
        public string  Message { get; set; }

        public InventoryDocumentImportResultDto()
        {
            FailCount = 0;
            SuccessCount = 0;
            Result = new byte[0];
            //FailedImportInventoryDocuments = new List<FailedImportInventoryDocumentDto>();
        }
    }

}
