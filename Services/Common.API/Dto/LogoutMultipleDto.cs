namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class LogoutMultipleDto
    {
        public List<LogoutDto> UserIdList { get; set; } = new List<LogoutDto>();
    }
}
