namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ListDocumentHistoryModel
    {
        public string InventoryName { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string DocCode { get; set; }
        public int DocType { get; set; }
        public string ComponentCode { get; set; }
        public string ModelCode { get; set; }
        public string ComponentName { get; set; }
        public string Comment { get; set; }
        public string Action { get; set; }
        public string ChangeLog { get; set; }
        public string AssigneeAccount { get; set; }
        public string AssigneeAccountDate { get; set; }
        public string HistoryId { get; set; }
        public string InventoryId { get; set; }
    }
}
