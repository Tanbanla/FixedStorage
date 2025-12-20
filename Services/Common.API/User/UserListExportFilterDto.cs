namespace BIVN.FixedStorage.Services.Common.API.User
{
    public class UserListExportFilterDto
    {        
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Code { get; set; }
        public IList<string> DepartmentIds { get; set; }
        public string AllDepartments { get; set; }
        public IList<string> RoleIds { get; set; }
        public string AllRoles { get; set; }
        public IList<string> Status { get; set; }
        public string AllStatus { get; set; }
        public string AllAccountType { get; set; }
        public IList<string> AccountTypes { get; set; }        
    }    
}
