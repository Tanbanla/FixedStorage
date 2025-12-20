namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class BaseFilterDto
    {
        /// <summary>
        /// Phân trang dữ liệu cần xem
        /// </summary>
        public PagingInfo? Paging { set; get; } = new PagingInfo();
    }
}
