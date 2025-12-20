namespace Storage.API.Service.Dto
{
    public class RawImportInputStorageDto
    {
        public string BwinOutputCode { get; set; }
        public string ComponentCode { get; set; }
        //public string ComponentName { get; set; }
        public string SupplierCode { get; set; }
        //public string SupplierName { get; set; }
        //public string SupplierShortName { get; set; }
        //public string PositionCode { get; set; }
        public string Quantity { get; set; }

        /// <summary>
        /// SourceType: Phân biệt khi validate dữ liệu, chỉ các record import cần validate đầy đủ
        /// </summary>
        public SourceType SourceType { get; set; } = SourceType.IMPORT;
    }

    public sealed class RawImportInputStorageDtoMap : ClassMap<RawImportInputStorageDto>
    {
        public RawImportInputStorageDtoMap()
        {
            Map(m => m.BwinOutputCode).Name("Mã chỉ thị xuất kho");
            Map(m => m.ComponentCode).Name("Mã linh kiện");
            //Map(m => m.ComponentName).Name("Tên linh kiện");
            //Map(m => m.SupplierName).Name("Tên nhà cung cấp");
            //Map(m => m.SupplierShortName).Name("Tên nhà cung cấp rút gọn");
            //Map(m => m.PositionCode).Name("Vị trí store");
            Map(m => m.SupplierCode).Name("Mã nhà cung cấp");
            Map(m => m.Quantity).Name("Số lượng");
        }
    }
}
