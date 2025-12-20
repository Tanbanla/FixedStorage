namespace BIVN.FixedStorage.Services.Common.API
{
    public class ResponseModelDetail : ResponseModel
    {
        public bool Success { get; set; }
    }

    public class ResponseModelDetail<T> : ResponseModel<T>
    {
        public ResponseModelDetail() : base() { }
        public ResponseModelDetail(T response) : base(response) { }
        public bool Success { get; set; }
    }
}
