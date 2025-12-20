namespace BIVN.FixedStorage.Services.Common.API
{
    public abstract class AuditEntity<T> where T : new()
    {
        public required T Id { get; set; }
        public required DateTime CreatedAt { get; set; }
        [MaxLength(50)]
        public required string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(50)]
#nullable enable
        public string? UpdatedBy { get; set; }
        [MaxLength(50)]
        public string? DeletedBy { get; set; }
#nullable disable
        public DateTime? DeletedAt { get; set; }
    }
}
