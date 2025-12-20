namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class UpdateAuditTargetDto
    {
        //public Guid Id { get; set; }
        //public Guid InventoryId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập plant.")]
        [MaxLength(4, ErrorMessage = "Tối đa {0} ký tự.")]
        public string Plant { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập WHLoc.")]
        public string WHLOC { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập khu vực.")]
        [MaxLength(50, ErrorMessage = "Tối đa {0} ký tự.")]

        public string LocationName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã linh kiện.")]
        //[RegularExpression(chỉ cho nhập ký tự chữ, số, chờ logic phần import)]
        public string ComponentCode { get; set; }
        [MaxLength(25, ErrorMessage = "Tối đa {0} ký tự.")]
        public string? SaleOrderNo { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập vị trí")]
        [MaxLength(20, ErrorMessage = "Tối đa {0} ký tự.")]
        public string PositionCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập người phân phát")]
        public string AssigneeAccount { get; set; }


    }
}
