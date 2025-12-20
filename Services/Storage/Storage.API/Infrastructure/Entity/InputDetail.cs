namespace BIVN.FixedStorage.Services.Storage.API.Infrastructure.Entity
{
    [Index(nameof(BwinOutputCode))]
    [Index(nameof(ComponentCode))]
    [Index(nameof(SuplierCode))]
    [Index(nameof(PositionCode))]
    public class InputDetail : AuditEntity<Guid>
    {
        public InputDetailType? @Type { get; set; }
        [MaxLength(50)]
        public string? BwinOutputCode { get; set; }
        [MaxLength(50)]
        public string? ComponentCode { get; set; }
        [MaxLength(50)]
        public string? SuplierCode { get; set; }
        [MaxLength(50)]
        public string? PositionCode { get; set; }
        public double Quantity { get; set; }

        /// <summary>
        /// Số lượng được hệ thống phân bổ lần đầu khi import, theo nghiệp vụ số lượng cập nhật lần sau không được vượt quá số lượng đã được phân bổ cho vị trí này, mã nhà cung cấp này
        /// </summary>
        public double AllocatedQuantity { get; set; }
        public RemainingHanle RemainingHandle { get; set; }
        [MaxLength(500)]
        public string? Note { get; set; }

        /// <summary>
        /// Trường này để xác định giá trị cũ trước khi cập nhật số lượng
        /// </summary>
        public double OldQuantity { get; set; }
        public Guid? InputId { get; set; }
        public InputFromBwin InputFromBwin { get; set; }
    }
}
