namespace WebApp.Application
{
    public class Constants
    {

        public const string GlobalRoutePrefix = "";

        /// <summary>
        /// AuditMobile có routing riêng so với fixedstock, inventory
        /// <para>AuditMobile có thể chỉnh sửa lại các phần sau:</para> 
        /// <para>Header:</para> 
        /// <para>-Sửa lại View/ Icon Sidebar (Hamburger)</para> 
        /// <para>-Sửa lại View/ Icon Go back (Nút quay lại)</para> 
        /// </summary>
        public class AuditMobileSettings
        {
            public const string DefaultSideBarViewPath = "~/Views/AuditMobile/Components/_SideBarView.cshtml";
            public const string DefaultGoBackViewPath = "~/Views/AuditMobile/Components/_GoBackView.cshtml";
            public const string DefaultAuditFilterViewPath = "~/Views/AuditMobile/Components/_AuditFilterView.cshtml";
        }
    }
    public class RouteDisplay
    {
        public const string DEPARTMENT = "Quản lý phòng ban";
        public const string LIST_USER = "Danh sách người dùng";
        public const string ROLE = "Quản lý nhóm quyền";
        public const string LAYOUT = "Quản lý khu vực";
        public const string COMPONENT = "Quản lý linh kiện";
        public const string STORAGE = "Nhập kho";
        public const string HISTORY = "Lịch sử xuất nhập kho";
        public const string INVENTORY = "Danh sách đợt kiểm kê";
        public const string INVENTORY_GENERAL_INFO = "Thông tin chung";
        public const string INVENTORY_HISTORY = "Lịch sử kiểm kê";
        public const string INVENTORY_REPORTING = "Danh sách báo cáo đợt kiểm kê";
        public const string INVENTORY_ASSIGNMENT = "Phân quyền thao tác kiểm kê";
        public const string INVENTORY_DOCUMENT = "Danh sách phiếu kiểm kê";
        public const string ERROR_INVESTIGATION = "Danh sách sai số";
        public const string ERROR_INVESTIGATION_HISTORY = "Lịch sử điều tra sai số";
        public const string INVESTIGATION_DETAIL = "Chi tiết điều tra";
        public const string ERROR_CATEGORY_MANAGEMENT = "Quản lý phân loại lỗi";

        public const string AUDIT_MOBILE = "Giám sát mobile";


        public const string DETAIL = "Xem chi tiết";

    }

    public static class FileTemplate
    {
        public const string rootPath = @"FileExcelTemplate";
        public const string TemplateExportHistoryInOutStorage = "TemplateExportHistoryInOutStorage.xlsx";
        public const string TemplateExportUserList = "TemplateExportUserList.xlsx";
        public const string TemplateImportComponentList = "TemplateImportComponentList.xlsx";
        public const string TemplateImportInputStorage = "TemplateImportInputStorage.csv";

        public const string BieumauBwins = "BieumauBwins.xlsx";
        public const string Bieumaudanhsachgiamsat = "Bieumaudanhsachgiamsat.xlsx";
        public const string BieumauphieuA = "BieumauphieuA.xlsx";
        public const string BieumauphieuB = "BieumauphieuB.xlsx";
        public const string BieumauphieuE = "BieumauphieuE.xlsx";
        public const string BieumauphieuC = "BieumauphieuC.xlsx";
        public const string BieumauphieuShip = "BieumauphieuShip.xlsx";
        public const string BieumauSAP = "BieumauSAP.xlsx";
        public const string Bieumaucapnhatsoluong = "Bieumaucapnhatsoluong.xlsx";
        public const string Bieumaucapnhatsoluongdieuchinh = "Bieumaucapnhatsoluongdieuchinh.xlsx";
        public const string BieumaucapnhatdulieuMSL = "BieumaucapnhatdulieuMSL.xlsx";
        public const string BieumaucapnhatsoluongPivot = "BieumaucapnhatsoluongPivot.xlsx";

        public static Dictionary<string, string> filePaths = new Dictionary<string, string>
        {
            { nameof(TemplateExportHistoryInOutStorage), TemplateExportHistoryInOutStorage },
            { nameof(TemplateExportUserList), TemplateExportUserList },
            { nameof(TemplateImportComponentList), TemplateImportComponentList },
            { nameof(TemplateImportInputStorage), TemplateImportInputStorage },

            { nameof(BieumauBwins), BieumauBwins  },
            { nameof(Bieumaudanhsachgiamsat), Bieumaudanhsachgiamsat  },
            { nameof(BieumauphieuA), BieumauphieuA  },
            { nameof(BieumauphieuB), BieumauphieuB  },
            { nameof(BieumauphieuE), BieumauphieuE  },
            { nameof(BieumauphieuC), BieumauphieuC  },
            { nameof(BieumauphieuShip), BieumauphieuShip  },
            { nameof(BieumauSAP), BieumauSAP  },
            { nameof(Bieumaucapnhatsoluong), Bieumaucapnhatsoluong  },
            { nameof(Bieumaucapnhatsoluongdieuchinh), Bieumaucapnhatsoluongdieuchinh  },
            { nameof(BieumaucapnhatdulieuMSL), BieumaucapnhatdulieuMSL  },
            { nameof(BieumaucapnhatsoluongPivot), BieumaucapnhatsoluongPivot  },
        };

        public static string FileTitle(string fileKey)
        {
            var title = string.Empty;
            switch (fileKey)
            {
                case nameof(TemplateExportHistoryInOutStorage):
                    title = "Mẫu lịch sử xuất nhập kho";
                    break;
                case nameof(TemplateExportUserList):
                    title = "Mẫu danh sách người dùng";
                    break;
                case nameof(TemplateImportComponentList):
                    title = "Mẫu danh sách linh kiện";
                    break;
                case nameof(TemplateImportInputStorage):
                    title = "Mẫu nhập kho";
                    break;
            }

            return title;
        }
    }
}
