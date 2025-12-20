using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BIVN.FixedStorage.Services.Common.API
{
    public abstract class PagedListModel<T>
    {
        private IEnumerable<T> _items;
        private int _totalCount;
        private bool _isGetAll;

        public IEnumerable<T> Items { get => _items; }
        public int TotalCount { get => _totalCount; }

        protected PagedListModel()
        {

        }
        protected PagedListModel(bool isGetAll = false)
        {

        }

        protected PagedListModel(IEnumerable<T> items, bool isGetAll = false)
        {
            _items = items;
            _isGetAll = isGetAll;
        }

        public virtual async Task CreateAsync(IQueryable<T> records, int skip = 0, int take = 10, bool isGetAll = false)
        {
            _totalCount = await records.CountAsync();

            if (isGetAll)
            {
                _items = await records.ToListAsync();
            }
            else
            {
                _items = await records.Skip(skip).Take(take).ToListAsync();
            }
        }

        // Overload to support building paged list from in-memory collections (e.g., Dapper results)
        public virtual Task CreateAsync(IEnumerable<T> records, int skip = 0, int take = 10, bool isGetAll = false)
        {
            var list = records?.ToList() ?? new List<T>();
            _totalCount = list.Count;

            if (isGetAll)
            {
                _items = list;
            }
            else
            {
                _items = list.Skip(skip).Take(take).ToList();
            }

            return Task.CompletedTask;
        }
    }
}
