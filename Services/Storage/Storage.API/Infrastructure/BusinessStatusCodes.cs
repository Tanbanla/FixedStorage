namespace BIVN.FixedStorage.Services.Storage.API.Infrastructure
{
    public static class BusinessStatusCodes
    {
        public const int PositionNotFound = 51;
        public const int NotEnoughToOutPut = 52;
        public const int NotEnoughToInPut = 53;

       
        public const string PositionNotFoundMessage = "Không tìm thấy vị trí tương ứng";
        public const string NotEnoughToOutPutMessage = "Số lượng tồn kho tại vị trí không đủ để xuất kho";
        public const string NotEnoughToInPutMessage = "Sức chứa tại vị trí không đủ để nhập kho";
    }
}
