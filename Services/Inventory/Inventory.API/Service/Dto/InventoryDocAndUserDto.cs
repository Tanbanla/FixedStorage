namespace Inventory.API.Service.Dto
{
    public class InventoryDocAndUserDto
    {
        public List<InventoryDocDto> InvDocs { get; set; }
        public List<AssigneeDto> Assignees { get; set; }
        public List<string> Layouts { get; set; }
        public bool IsTypeAExist { get; set; }
        public string InventoryPart { get; set; }
        public int LastDocNumber { get; set; }
        public int LastDocNumberTypeB { get; set; }
        public int LastDocNumberTypeE { get; set; }

        public List<DocAComponentNameDto> DocAComponentNames { get; set; }

    }
}
