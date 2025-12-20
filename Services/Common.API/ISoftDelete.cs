namespace BIVN.FixedStorage.Services.Common.API
{
    public interface ISoftDelete
    {
        /// <summary>
        /// Xóa mềm
        /// </summary>
        bool? IsDeleted { get; set; }
    }
}
