namespace Inventory.API.Service.Dto.InventoryWeb
{
    public class ListInventoryDto
    {
        [MaxLength(50, ErrorMessage = "Yêu cầu nhập tối đa 50 ký tự.")]
        public string CreatedBy { get; set; }
        public DateTime? InventoryDateStart { get; set; }
        public DateTime? InventoryDateEnd { get; set; }
        public List<string> Statuses { get; set; }
    }
}
