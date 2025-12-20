namespace Identity.API.Service.Dtos
{
    public class GetUserInfoDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Code { get; set; }
        public string Avatar { get; set; }
        public string RoleName { get; set; }
        public string Department { get; set; }
        public string Path { get; set; }

    }
}
