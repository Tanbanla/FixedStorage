using BIVN.FixedStorage.Services.Common.API.User;

namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class InventoryActorInfoViewModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public Guid UserId { get; set; }
        public int? RoleType { get; set; }
        public Guid? LocationId { get; set; }
        public string LocationName { get; set; }
        public string FactoryName { get; set; }
        public string DepartmentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public AccountType? AccountType { get; set; }

        public List<AccountLocationViewModel> Locations { get; set; } = new();
        public List<LocationAssignedViewModel> AllLocations { get; set; } = new();

    }

    public class AccountLocationViewModel
    {
        public Guid LocationId { get; set; }
        public Guid UserId { get; set; }
        public string DepartmentName { get; set; }
        public string FactoryNames { get; set; }
        public string LocationName { get; set; }
    }
}
