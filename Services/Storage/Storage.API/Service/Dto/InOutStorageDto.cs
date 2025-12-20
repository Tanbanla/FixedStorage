namespace BIVN.FixedStorage.Services.Storage.API.Service.Dto
{
    public class InOutStorageDto
    {
        /// <summary>
        /// Lưu mã vị trí vào lịch sử nhập/xuất
        /// </summary>
        public string PositionCode { get; set; }
        public string SupplierCode { get; set; }

        /// <summary>
        /// Lưu thông tin người nhập/xuất vào bảng lịch sử
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Số lượng nhập/xuất
        /// </summary>
        [SwaggerSchema("Số lượng nhập/xuất")]
        public double Quantity { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? Reason { get; set; }
        public TypeOfBusiness TypeOfBusiness {get;set;}
        /// Thông tin nhân viên thực hiện
        public string? EmployeeCode { get; set; }
    }
}
