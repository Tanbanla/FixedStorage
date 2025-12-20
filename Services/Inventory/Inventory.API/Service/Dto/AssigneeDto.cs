namespace Inventory.API.Service.Dto
{
    public class AssigneeDto
    {
        public string LocationName { get; set; }
        public string DepartmentName { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public InventoryAccountRoleType? RoleType { get; set; }
    }
}
