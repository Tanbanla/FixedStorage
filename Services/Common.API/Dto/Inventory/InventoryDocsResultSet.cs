namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class InventoryDocsResultSet<T> : ResultSet<T>
    {
        public int DocsNotReceiveCount { get; set; }
        public DateTime LastCursor { get; set; }
    }
}
