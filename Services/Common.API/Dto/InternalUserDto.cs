using BIVN.FixedStorage.Services.Common.API.User;

namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class InternalUserDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public AccountType? AccountType { get; set; }
    }
}
