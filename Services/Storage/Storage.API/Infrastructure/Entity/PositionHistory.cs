namespace BIVN.FixedStorage.Services.Storage.API.Infrastructure.Entity
{
    [Index(nameof(PositionCode))]
    public class PositionHistory : AuditEntity<Guid>
    {
        //Thông tin người dùng:
        public Guid? AppUserId { get; set; }
        /// <summary>
        /// Thông tin phòng ban
        /// </summary>
        [MaxLength(50)]
        public Guid DepartmentId { get; set; }
        [MaxLength(20)]
        public string? FactoryName { get; set; }
        [MaxLength(50)]
        public Guid? PositionId { get; set; }

        /// <summary>
        /// Mã vị trí #Lưu vào history thay vì PositionId
        /// </summary>
        [MaxLength(50)]
        public string PositionCode { get; set; }
        /// <summary>
        /// Số lượng tồn kho
        /// </summary>
        [MaxLength(30)]
        public double? Quantity { get; set; }
        //Số lượng tồn kho
        [MaxLength(30)]
        public double? InventoryNumber { get; set; }
        /// <summary>
        /// Loại hành động: Nhập/ Xuất
        /// </summary>
        public PositionHistoryType? PositionHistoryType { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        [MaxLength(500)]
        public string? Note { get; set; }
        public Guid? FactoryId { get; set; }
        /// <summary>
        /// Giành cho mobile, người thực hiện là MC hay PCB
        /// </summary>
        public TypeOfBusiness? TypeOfBusiness { get; set; }

        [MaxLength(50)]
        public string? ComponentCode { get; set; }
        [MaxLength(250)]
        public string? ComponentName { get; set; }
        [MaxLength(50)]
        public string? SupplierCode { get; set; }
        [MaxLength(250)]
        public string? SupplierName { get; set; }
        [MaxLength(250)]
        public string? SupplierShortName { get; set; }
        [MaxLength(10)]
        public string? Layout { get; set; }

        [MaxLength(50)]
        public string? EmployeeCode { get; set; }
    }
    public enum PositionHistoryType
    {
        Input,
        Output,
    }
    public enum TypeOfBusiness
    {
        NULL = 0,
        MC = 1,
        PCB = 2,
    }
}
