namespace BIVN.FixedStorage.Services.Common.API.Dto.PagedList
{
    /// <summary>
    /// Represents a class to paging a list of result objects.
    /// </summary>
    /// <summary>
    /// Represents a class to paging a list of result objects.
    /// </summary>
    public class PagingInfo
    {
        #region "fields"
        private int _pageSize;
        private int _currentPage;
        private int _rowsCount;

        // instance of PagingInfo that no paging
        private static PagingInfo _noPaging = new PagingInfo(0, 0);
        #endregion

        #region "properties"
        /// <summary>
        /// startRowIndex of page.
        /// </summary>
        public int StartRowIndex
        {
            get { return (this._pageSize * (this._currentPage - 1)); }
        }

        /// <summary>
        /// Number of objects per page.
        /// </summary>
        public int PageSize
        {
            get { return this._pageSize; }
            set { this._pageSize = value; }
        }

        /// <summary>
        /// Current page index.
        /// </summary>
        public int CurrentPage
        {
            get { return this._currentPage; }
            set { _currentPage = value; }
        }

        /// <summary>
        /// Number of total rows.
        /// </summary>
        public int RowsCount
        {
            get { return this._rowsCount; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("RowsCount");
                this._rowsCount = value;
            }
        }

        /// <summary>
        /// Number of total pages.
        /// </summary>
        public long PagesCount
        {
            get { return (int)Math.Ceiling((double)this._rowsCount / (double)this._pageSize); }
        }

        /// <summary>
        /// No paging.
        /// </summary>
        public static PagingInfo All
        {
            get { return _noPaging; }
        }
        #endregion

        public PagingInfo()
        {

        }

        /// <summary>
        /// Initialize a new instance of PagingInfo class.
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="currentPage"></param>
        public PagingInfo(int pageSize, int currentPage)
        {
            this._pageSize = pageSize;
            this._currentPage = currentPage;
        }

        /// <summary>
        /// Returns a <b>string</b> that represents the current PagingInfo object.
        /// </summary>
        /// <returns>A <b>string</b> that represents the current PagingInfo object.</returns>
        public override string ToString()
        {
            return string.Format("PagingInfo [PageSize={0}, CurrentPage={1}, RowsCount={2}]",
                this._pageSize, this._currentPage, this._rowsCount);
        }
    }
}
