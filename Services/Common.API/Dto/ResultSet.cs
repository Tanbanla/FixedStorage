namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class ResultSet<T>
    {
        public T Data { get; set; }
        public int TotalRecords { get;set; }
    }

    public interface IPagination
    {
        int PageNum { get; set; }
        int PageSize { get; set; }
    }

}
