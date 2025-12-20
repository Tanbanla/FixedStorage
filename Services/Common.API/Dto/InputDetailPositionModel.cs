namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class InputDetailPositionModel
    {
        public Guid DetailId { get; set; }
        public Guid? InputId { get; set; }
        public string PositionCode { get; set; }
        public string ComponentCode { get; set; }
        public string SuplierCode { get; set; }
        public double Quantity { get; set; }
        public string FactoryId { get; set; }
    }
}
