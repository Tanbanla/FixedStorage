namespace Inventory.API.Infrastructure.Entity
{
    public class GeneralSetting : AuditEntity<Guid>
    {
        public GeneralSettingType Type { get; set; }
        public string? Key1 { get; set; }
        public string? Value1 { get; set; }
        public string? Key2 { get; set; }
        public string? Value2 { get; set; }
        public Guid? InventoryId { get; set; }
        public bool IsDelete { get; set; } = false;
    }
}
