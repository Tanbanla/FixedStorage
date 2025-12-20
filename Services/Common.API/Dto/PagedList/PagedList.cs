namespace BIVN.FixedStorage.Services.Common.API.Dto.PagedList
{
    public class PagedList<T>
    {
        public PagedList()
        {
                
        }

        public PagedList(List<T> list, PagingInfo paging)
        {
            List = list;
            Paging = paging;
        }

        public List<T> List { set; get; }

        public PagingInfo Paging { set; get; }
    }
}
