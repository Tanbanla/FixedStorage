namespace Inventory.API.Service.Dto
{
    public class UpdateInventoryDetail
    {
        public string UserId { get; set; }

        [Required(ErrorMessage = "Nhập vào thời gian đợt kiểm kê.")]
        public DateTime? InventoryDate { get; set; }

        [Required(ErrorMessage = "Nhập vào trạng thái đợt kiểm kê.")]
        public InventoryStatus Status { get; set; }

        [Required(ErrorMessage = "Nhập vào tỉ lệ kiểm tra lại đợt kiểm kê.")]
        public double AuditFailPercentage { get; set; }
        public bool IsLocked { get; set; }
    }
}
