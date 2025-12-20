namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ListInventoryToExportDto
    {
        public string CreatedBy { get; set; }
        public DateTime? InventoryDateStart { get; set; }
        public DateTime? InventoryDateEnd { get; set; }
        public List<string> Statuses { get; set; }
    }
}
