namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class ValidateFilterDto<T> where T : class
    {
        public ValidateFilterDto()
        {
        }

        public ValidateFilterDto(T data)
        {
                Data = data;
        }
        public bool IsInvalid { get; set; }
        public int? Code { get; set; } = StatusCodes.Status200OK;
        public string? Message { get; set; }              
        public T Data { get; set; } 
    }
}
