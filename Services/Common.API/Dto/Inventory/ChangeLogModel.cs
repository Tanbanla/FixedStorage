namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ChangeLogModel
    {
        public double OldQuantity { get; set; }
        public double NewQuantity { get; set; }
        public int OldStatus { get; set; }
        public int NewStatus { get; set; }
        public bool IsChangeCDetail { get; set; }
    }
}
