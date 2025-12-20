namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class BwinInputDetailModel
    {
        public Guid BwinId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public int Status { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public IEnumerable<InputDetailPositionModel> InputDetailPositionModels { get; set; }
        public double Total { get; set; }
    }
}
