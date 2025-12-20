namespace BIVN.FixedStorage.Identity.API.Infrastructure.Entity
{
    public class AppUser : IdentityUser<Guid>
    {
        public DateTime? DeletedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Gender? Gender { get; set; }

        public UserStatus Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public AccountType? AccountType { get; set; }

#nullable enable
        public string? FullName { get; set; }
        public override string UserName
        {
            get { return base.UserName; }
            set { base.UserName = value; }
        }


        public string? Address { get; set; }

        public string? Avatar { get; set; }



        public string? CreatedBy { get; set; }

        public string? UpdatedBy { get; set; }

        public string? DeletedBy { get; set; }


        public string? Code { get; set; }

        public string? DepartmentId { get; set; }
#nullable disable


        //public bool? IsLockedByExpiredPassword { get; set; }
        //public DateTime? ExpiredDateLockedByExpiredPassword { get; set; }

        //public bool? IsLockedByUnactive { get; set; }
        //public DateTime? ExpiredDateLockedByUnactive { get; set; }

        //public LockCause? LockCause { get; set; }

        public DateTime? UpdatedPasswordAt { get; set; }

        public DateTime? LastActiveTime { get; set; }

        public bool LockPwdSetting { get; set; } = false;

        public int? LockPwTime { get; set; }

        public bool LockActSetting { get; set; } = false;

        public int? LockActTime { get; set; }

        public string RefreshToken { get; set; }
    }
}
