namespace BIVN.FixedStorage.Services.Storage.API.Infrastructure.Entity
{
    public class InputFromBwin : AuditEntity<Guid>
    {
        public BwinInputStatus Status { get; set; }

        public ICollection<InputDetail> InputDetails { get; set; }
    }
}
