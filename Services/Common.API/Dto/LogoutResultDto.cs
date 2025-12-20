namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class LogoutResultDto
    {
        public string Success { get; set; } = string.Empty;
        public string Fail { get; set; } = string.Empty;

        public string NotExists { get; set; } = string.Empty;
    }
}
