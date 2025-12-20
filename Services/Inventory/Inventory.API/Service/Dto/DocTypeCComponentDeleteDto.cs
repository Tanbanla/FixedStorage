namespace Inventory.API.Service.Dto
{
    public class DocTypeCComponentDeleteDto
    {
        public Guid Id { get; set; }
        public List<DocTypeCComponentDeleteDto> Parent { get; set; }
    }

    public class DocTypeCComponentDto
    {
        public Guid Id { get; set; }
        public string ComponentCode { get; set; }
        public string MainModelCode { get; set; }
        public string UnitModelCode { get; set; }
    }
}
