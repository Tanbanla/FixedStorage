namespace BIVN.FixedStorage.Services.Common.API.Exceptions
{
    public class InvalidAuthorizeException : Exception
    {
        public int StatusCode { get; }
        public object? Data { get; }
        public string Message { get; }
        public InvalidAuthorizeException(string message) : base(message) { }
        public InvalidAuthorizeException(int statusCode, string message, object data = null)
        {
            this.StatusCode = statusCode;
            this.Data = data;
            this.Message = message;
        }
    }
}
