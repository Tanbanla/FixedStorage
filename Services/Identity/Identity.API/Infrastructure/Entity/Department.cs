namespace BIVN.FixedStorage.Identity.API.Infrastructure.Entity
{
    public class Department : AuditEntity<Guid>, ISoftDelete
    {
        public required string Name { get; set; }
        /// <summary>
        /// Id của trưởng phòng
        /// </summary>

#nullable enable
        public string? ManagerId { get; set; }
#nullable disable

        public bool? IsDeleted { get; set; }
    }
}
