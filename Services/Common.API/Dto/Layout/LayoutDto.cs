namespace BIVN.FixedStorage.Services.Common.API.Dto.Layout
{
    public class LayoutDto
    {
        /// <summary>
        /// Tên khu vực
        /// </summary>
        public string Layout { get; set; }

        /// <summary>
        /// Tên nhà máy
        /// </summary>
        public string FactoryName { get; set; }

        /// <summary>
        /// Tên tầng
        /// </summary>
        public string StorageName { get; set; } = string.Empty;

        /// <summary>
        /// Tên giá
        /// </summary>
        public string ShelfName { get; set; } = string.Empty;

        /// <summary>
        /// Tổng số lượng vị trí linh kiện 
        /// Hệ thống tính tổng số lượng bản ghi có trường vị trí trong 
        /// danh sách Quản lý linh kiện cùng thông tin trường nhà máy + tầng + giá 
        /// </summary>
        public int? PositionCount { get; set; } = 0;
       
        /// <summary>
        /// Số lượng vị trí gần hết linh kiện 
        /// Hệ thống tính tổng số lượng bản ghi 
        /// có trường Tồn kho thực tế nhỏ hơn (&lt;) Trạng thái tồn kho nhỏ nhất 
        /// trong danh sách Quản lý linh kiện 
        /// </summary>
        public int? InventoryNumberCount { get; set; } = 0;

        public string? InventoryStatus { get; set; }
    }
}
