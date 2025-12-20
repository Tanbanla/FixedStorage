namespace BIVN.FixedStorage.Identity.API.Service.Dtos
{
    public class FilterListUserDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Code { get; set; }
        public int? Status { get; set; }
        public int? AccountType { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }
}
