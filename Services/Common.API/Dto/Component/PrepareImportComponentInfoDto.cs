namespace BIVN.FixedStorage.Services.Common.API.Dto.Component
{
    public class PrepareImportComponentInfoDto
    {
        /// <summary>
        /// Vị trí cố định
        /// </summary>
        public string PositionCode { get; set; }

        /// <summary>
        /// Mã linh kiện
        /// </summary>
        public string ComponentCode { get; set; }

        /// <summary>
        /// Mã nhà cung cấp
        /// </summary>
        public string SupplierCode { get; set; }
    }
}
