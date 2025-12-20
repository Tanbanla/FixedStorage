using BIVN.FixedStorage.Services.Common.API.User;

namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ListUserModel 
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? Code { get; set; }
        public AccountType? AccountType { get; set; }
        public int Status { get; set; }
        public string DepartmentName { get; set; }
    }
}
