namespace Inventory.API.Infrastructure.Entity.Enums
{
    public enum AuditTargetStatus
    {
        [Display(Name = "Chưa giám sát")]
        NotYet = 0,
        [Display(Name = "Giám sát đạt")]
        Pass = 1,
        [Display(Name = "Giám sát không đạt")]
        Fail = 2,
    }
}
