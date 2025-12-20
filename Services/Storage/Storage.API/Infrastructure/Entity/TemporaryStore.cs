namespace BIVN.FixedStorage.Services.Storage.API.Infrastructure.Entity
{
    [Index(nameof(BwinOutputCode))]
    [Index(nameof(ComponentCode))]
    [Index(nameof(SupplierCode))]
    public class TemporaryStore : AuditEntity<Guid>
    {
        [MaxLength(50)]
        public string? BwinOutputCode { get; set; }
        [MaxLength(50)]
        public string? ComponentCode { get; set; }
        [MaxLength(50)]
        public string? SupplierCode { get; set; }
        public double Quantity { get; set; }

    }
}
