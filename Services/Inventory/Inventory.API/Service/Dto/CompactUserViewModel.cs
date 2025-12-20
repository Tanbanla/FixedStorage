namespace Inventory.API.Service.Dto
{
    public class CompactUserViewModel
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Code { get; set; }
        public string DepartmentName { get; set; }
    }
}
