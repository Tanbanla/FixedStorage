namespace BIVN.FixedStorage.Services.Storage.API.Infrastructure.Entity
{
    public class Storage : AuditEntity<Guid>
    {
        public Guid FactoryId { get; set; }
        [MaxLength(50)]
        public string Layout { get; set; }
        public bool? isDeleted { get; set; }

        public ICollection<Position> Positions { get; set; }
    }
}
