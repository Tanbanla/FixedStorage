namespace BIVN.FixedStorage.Services.Common.API.User
{
    public class LockUserListDto
    {
        public LockUserListDto()
        {
                
        }

        public LockUserListDto(List<LockUserDto> lockUserListResult)
        {
            LockUserListResult = lockUserListResult;           
        }

        public List<LockUserDto> LockUserListResult { get; set; } = new List<LockUserDto>();
        
        public int LockCount { get; set; } = 0;       
    }
}
