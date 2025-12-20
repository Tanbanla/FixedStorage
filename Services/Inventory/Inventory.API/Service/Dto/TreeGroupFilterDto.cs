namespace Inventory.API.Service.Dto
{
    public class TreeGroupFilterDto
    {
        public List<string> MachineModels { get; set; } = new List<string>();
        public List<string> MachineTypes { get; set; } = new List<string>();

    }
}
