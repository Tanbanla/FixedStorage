namespace Inventory.API.Service.Dto
{
    public class QuantityPerBomOfDetailDto
    {
        public string? ModelCode { get; set; }
        public string DirectParent { get; set; }
        public double QuantityPerBOM { get; set; }
    }
}
