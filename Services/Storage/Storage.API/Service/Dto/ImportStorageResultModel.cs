namespace Storage.API.Service.Dto
{
    public class ImportStorageResultModel
    {
        public byte[] ExcelFile { get; set; }
        /// <summary>
        /// Danh sách các bản ghi import thành công và lỗi
        /// </summary>
        public IEnumerable<ImportInputStorageDto> AllImportRecords { get; set; }
    }
}
