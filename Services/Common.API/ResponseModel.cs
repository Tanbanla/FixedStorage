
namespace BIVN.FixedStorage.Services.Common.API.Response
{
    public class ResponseModel
    {
        public string? Message { get; set; }
        public int Code { get; set; }
        public object? Data { get; set; }
    }

    public class ResponseModel<T>
    {
        public ResponseModel()
        {
           
        }

        public ResponseModel(T data)
        {
            Data = data;
        }

        public ResponseModel(int code, T data)
        {
            Code = code;
            Data = data;
        }

        public ResponseModel(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public ResponseModel(int code, string message, T data)
        {
            Code = code;
            Message = message;
            Data = data;
        }
        public string? Message { get; set; }
        public int Code { get; set; }
        public T Data { get; set; }
    }   
}
