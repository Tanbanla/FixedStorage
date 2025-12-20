namespace BIVN.FixedStorage.Services.Storage.API.Infrastructure.Entity
{
    [Index(nameof(PositionCode))]
    [Index(nameof(ComponentCode))]
    [Index(nameof(ComponentName))]
    public class Position : AuditEntity<Guid>
    {
        public required Guid StorageId { get; set; }
        /// <summary>
        /// Example: 5T2C1/2-13/03-02 
        /// </summary> 

        //Vị trí cố định:
        [StringLength(50)]
        public required string PositionCode { get; set; }
        //Tên nhà máy:
        [MaxLength(50)]
        public string? FactoryName { get; set; }
        public Guid? FactoryId { get; set; }

        [StringLength(50)]
        public string? SepcialRequire { get; set; }
        public int? ShelfSide { get; set; }
        public int? Cell { get; set; }
        public int? Compartment { get; set; }
        public int Floor { get; set; }
        [StringLength(50)]
        public required string Layout { get; set; }

        //Thông tin về linh kiện:

        //Tên linh kiện
        [MaxLength(150)]
        public string? ComponentName { get; set; }
        //Mã linh kiện:
        [MaxLength(50)]
        public string? ComponentCode { get; set; }
        //Thông tin linh kiện:
        [MaxLength(255)]
        public string? ComponentInfo { get; set; }
        //Mã nhà cung cấp:
        [MaxLength(50)]
        public string? SupplierCode { get; set; }
        //Tên nhà cung cấp:
        [MaxLength(250)]
        public string? SupplierName { get; set; }
        //Ghi chú:
        [MaxLength(255)]
        public string? Note { get; set; }
        //Tồn kho thực tế:
        [Required]
        public required double InventoryNumber { get; set; }

        //Tồn kho Min:
        [Required]
        public required double MinInventoryNumber { get; set; }
        //Tồn kho Max:
        [Required]
        public required double MaxInventoryNumber { get; set; }

        //Tên nhà cung cấp rút gọn:
        [MaxLength(250)]
        public string? SupplierShortName { get; set; }
        public ICollection<PositionHistory> PositionHistories { get; set; }
    }
}
