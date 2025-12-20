namespace Inventory.API.Service.Dto
{
    public class InventoryDocTypeCCodeGroupDto
    {
        public required string Model { get; set; }
        public required string MachineType { get; set;}
        public required string LineName { get; set;}
        public required string StageName { get; set; }
        public required string StageNumber { get; set;}
        public string SheetName { get; set; }
        public string Code { get; set; }
    }
}
