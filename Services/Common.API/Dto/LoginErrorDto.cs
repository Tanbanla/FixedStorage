namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class LoginErrorDto
    {
        public LoginErrorDto()
        {
                
        }

        public LoginErrorDto(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string? Username { get; set; } = string.Empty;
        public string? Password { get; set; } = string.Empty;
    }
}
