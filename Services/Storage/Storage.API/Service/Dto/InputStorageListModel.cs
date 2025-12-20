namespace Storage.API.Service.Dto
{
    public class InputStorageListModel
    {
        [Display(Name = "Số thứ tự")]
        public string InputId { get; set; }
        [Display(Name = "Mã nhân viên")]
        public string UserCode { get; set; }
        [Display(Name = "Tên người nhập")]
        public string UserName {get;set;}
        [Display(Name = "Ngày nhập")]
        public DateTime CreateDate {get;set;}
        [Display(Name = "Tổng số lượng linh kiện")]
        public double Total {get;set;}
        [Display(Name = "Trạng thái")]
        public int Status {get;set;}
        public string CreateBy { get; set; }
        public string FactoryId { get; set;}
    }
}
