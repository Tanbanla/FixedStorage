namespace Inventory.API.Service.Dto
{
    public class InventoryDocTypeCEntitesDto
    {
        public List<InventoryDoc> InvDocs { get; set; } = new();
        public List<InventoryDoc> InvDocsUpdate { get; set; } = new();
        public List<DocTypeCDetail> DocTypeCDetails { get; set; } = new();
        public List<DocTypeCDetail> DocTypeCDetailsUpdate { get; set; } = new();
        public List<Guid> OriginDocTypeCDetailIds { get; set; } = new();
        public List<Guid> NewDocTypeCDetailIds { get; set; } = new();
        public List<Guid> OriginDocTypeCComponentIds { get; set; } = new();
        public List<Guid> NewDocTypeCComponentIds { get; set; } = new();


    }
}
