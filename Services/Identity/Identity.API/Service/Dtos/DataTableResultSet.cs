namespace BIVN.FixedStorage.Identity.API.Service.Dtos
{
    public class DataTableResultSet<T> where T : class
    {
        /// <summary>Array of records. Each element of the array is itself an array of columns</summary>
        public List<T> data = new List<T>();

        /// <summary>value of draw parameter sent by client</summary>
        public int draw;

        /// <summary>filtered record count</summary>
        public int recordsFiltered;

        /// <summary>total record count in resultset</summary>
        public int recordsTotal;

        public bool isAdmin;

        //public string ToJSON()
        //{
        //    return JsonConvert.SerializeObject(this);
        //}
    }
}
