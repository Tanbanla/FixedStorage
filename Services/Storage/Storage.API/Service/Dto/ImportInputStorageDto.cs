namespace Storage.API.Service.Dto
{
    public class ImportInputStorageDto
    {
        public string BwinOutputCode { get; set; }
        public string ComponentCode { get; set; }
        public string ComponentName { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string SupplierShortName { get; set; }
        public string PositionCode { get; set; }
        public double Quantity { get; set; }

        /// <summary>
        /// SourceType: Phân biệt khi validate dữ liệu, chỉ các record import cần validate đầy đủ
        /// </summary>
        public SourceType SourceType { get; set; } = SourceType.IMPORT;

        public Dictionary<string, string> Errors { get; set; } = new();

        public bool Valid { get; set; }
    }

    public enum SourceType
    {
        NULL = 0,
        IMPORT = 1,
        TEMPORARY = 2
    }
}
