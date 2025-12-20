namespace BIVN.FixedStorage.Identity.API.Infrastructure.Entity
{
    public class AppRole : IdentityRole<Guid>
    {
        public AppRole() : base()
        {
        }

        public AppRole(string name, string description) : base(name)
        {
            Description = description;
        }

        public string Description { get; set; }

        public DateTime? CreatedAt { get; set; }

#nullable enable
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? DeletedBy { get; set; }
#nullable disable

        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
