namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class FactoriesStoragesQueryDto
    {
        public Guid FactoryId { get; set; }

        public string FactoryCode { get; set; }

        public string FactoryName { get; set; }

        public Guid? StorageId { get; set; }
        
        public string? Layout { get; set; }        
    }
}
