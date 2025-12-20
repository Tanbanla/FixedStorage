namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class InventoryResponseModel<T> : ResponseModel<T>
    {
        public bool IsExistDocTypeC { get; set; }
        public int TotalRecords { get; set; }
    }
    public class ErrorInvestigationResponseModel<T> : ResponseModel<T>
    {
        public int TotalRecords { get; set; }
    }
}
