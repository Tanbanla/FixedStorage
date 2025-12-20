namespace BIVN.FixedStorage.Services.Storage.API.Infrastructure.Entity
{
    public class Factory : AuditEntity<Guid>
    {
        [MaxLength(50)]
        public required string Name { get; set; }
        [MaxLength(50)]
        public required string Code { get; set; }
        public bool Status { get; set; }

        public ICollection<Storage> Storages { get; set; }
    }
}
