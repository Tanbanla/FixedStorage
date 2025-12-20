namespace BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Enums
{
    public enum ErrorInvestigationStatus
    {
        [Display(Name = "Chưa điều tra")]
        NotYetInvestigated = 0,
        [Display(Name = "Đang điều tra")]
        UnderInvestigation = 1,
        [Display(Name = "Đã điều tra")]
        Investigated = 2,
    }
}
