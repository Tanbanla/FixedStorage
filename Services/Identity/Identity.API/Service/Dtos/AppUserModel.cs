namespace BIVN.FixedStorage.Identity.API.Service.Dtos
{
    public class AppUserModel
    {
        public AppUserModel()
        {
            Status = UserStatus.Active;
        }

        public string Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }



        //public string SystemCode { get; set; } = StringHelper.GenerateRandomCode();

        //public string StaffCode { get; set; }

        //public Position Position { get; set; }

        //public int PositionValue { get; set; }

        //public string PositionText
        //{
        //    get
        //    {
        //        //return $"{(Enum.IsDefined(typeof(Position), Position) ? $"{EnumHelper<Position>.GetDisplayValue(Position)}" : string.Empty)}";
        //        return EnumHelper<Position>.GetDisplayValue(Position);
        //    }
        //    set { }
        //}

        public int? RoleId { get; set; }


        //public int? DirectLeaderId { get; set; }

        //public string DirectLeaderText { get; set; }

        public int? DepartmentId { get; set; }


        public UserStatus Status { set; get; }

        public string StatusText
        {
            get
            {
                return $"{(Enum.IsDefined(typeof(UserStatus), Status) ? $"{EnumHelper<UserStatus>.GetDisplayValue(Status)}" : string.Empty)}";
                //return EnumHelper<StaffStatus>.GetDisplayValue(Status);
            }
            set { }
        }

        public int StatusValue { get; set; }

        public int? Gender { get; set; }

        public string GenderText
        {
            get
            {
                return $"{(Gender.HasValue && Enum.IsDefined(typeof(Gender), Gender.Value) ? $"{EnumHelper<Gender>.GetDisplayValue((Gender)Gender.Value)}" : string.Empty)}";
                //return EnumHelper<Gender>.GetDisplayValue(Gender);
            }
            set { }
        }

        //public int GenderValue { get; set; }

        public DateTime CreatedAt { set; get; }

        public string CreatedAtText
        {
            get { return CreatedAt.ToString("dd-MM-yyyy"); }
            set { }
        }

        public int? CreatedBy { set; get; }


        public DateTime? BirthDay { set; get; }


        public string BirthDayStr { get; set; }


        //public IFormFile AvatarImageFile { set; get; }

        //public string FacebookLink { get; set; }

        //public string Hotline { get; set; }

        //public bool TwoFactorEnabled { get; set; }

        //public bool LockoutEnabled { get; set; }

        public DateTime? UpdatedAt { set; get; }


        public int? UpdatedBy { set; get; }


        public bool? IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

#nullable enable
        public string? FullName { get; set; }
        public string? RoleText { get; set; }
        public string? DepartmentValueOption { get; set; }

        public string? DepartmentText { get; set; }
        public string? CreatedByText { get; set; }
        public string? BirthDayText
        {
            get
            {
                return $"{(BirthDay.HasValue ? $"{BirthDay.Value:dd-MM-yyyy}" : string.Empty)}";
                //return BirthDay.HasValue ? BirthDay.Value.ToString("dd-MM-yyyy") : string.Empty;
            }
            set { }
        }
        public string? Address { get; set; }

        public string? Avatar { get; set; }
        public string? UpdatedAtText
        {
            get
            {
                return $"{(UpdatedAt.HasValue ? $"{UpdatedAt.Value:dd-MM-yyyy}" : string.Empty)}";
                //return UpdatedAt.HasValue ? UpdatedAt.Value.ToString("dd-MM-yyyy") : string.Empty;
            }
            set { }
        }
        public string? UpdatedByText { get; set; }
        public string? Errors { get; set; }
#nullable disable

    }
}
