namespace BIVN.FixedStorage.Services.Common.API.User
{
    public class LockUserDto
    {
        public bool Success { get; set; }
        
        public string UserId { get; set; }

        public int OldStatus { get; set; }

        public string OldStatusName { get; set; }

        public int NewStatus { get; set; }

        public string NewStatusName { get; set; }

        public string Type { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
