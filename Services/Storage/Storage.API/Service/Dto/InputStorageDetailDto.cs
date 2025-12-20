namespace Storage.API.Service.Dto
{
    public class InputStorageDetailDto
    {
        public Guid? InputId { get; set; }
        public string BwinOutputCode { get; set; }
        public string ComponentCode { get; set; }
        public string SuplierCode { get; set; }
        public string PositionCode { get; set; }
        public string Quantity { get; set; }
        public string Note { get; set; }
        public Guid? DetailId { get; set; }
        public int @Type { get; set; }
        public RemainingHanle RemainingHandle { get; set; }
        public DateTime CreateAt { get; set; }
        public string CreateBy { get; set; }
        public Guid? FactoryId { get; set; }
        public int BwinStatus { get; set; }
    }
}
