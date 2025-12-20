namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class ValidateDto<T> where T : class
    {
        public ValidateDto()
        {
        }

        public ValidateDto(T data)
        {
                Data = data;
        }
        public bool IsInvalid { get; set; }
        public int? Code { get; set; } = StatusCodes.Status200OK;
        public string? Message { get; set; }       
        public string? DeviceId { get; set; }
        public T Data { get; set; } 
    }
}
