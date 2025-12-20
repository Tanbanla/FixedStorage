namespace BIVN.FixedStorage.Services.Common.API
{
    public static class Constants
    {
        public class Permissions
        {
            public const string WEBSITE_ACCESS = nameof(WEBSITE_ACCESS);
            public const string MOBILE_ACCESS = nameof(MOBILE_ACCESS);

            //Website
            public const string DEPARTMENT_MANAGEMENT = nameof(DEPARTMENT_MANAGEMENT);
            public const string USER_MANAGEMENT = nameof(USER_MANAGEMENT);
            public const string ROLE_MANAGEMENT = nameof(ROLE_MANAGEMENT);
            public const string MASTER_DATA_READ = nameof(MASTER_DATA_READ);
            public const string MASTER_DATA_WRITE = nameof(MASTER_DATA_WRITE);
            /// <summary>
            /// Quyền xem lịch sử xuất nhập
            /// </summary>
            public const string HISTORY_MANAGEMENT = nameof(HISTORY_MANAGEMENT);
            /// <summary>
            /// Quyền nhập kho
            /// </summary>
            public const string INPUT = nameof(INPUT);


            public const string VIEW_ALL_INVENTORY = nameof(VIEW_ALL_INVENTORY);
            public const string VIEW_CURRENT_INVENTORY = nameof(VIEW_CURRENT_INVENTORY);
            public const string EDIT_INVENTORY = nameof(EDIT_INVENTORY);
            public const string VIEW_REPORT = nameof(VIEW_REPORT);
            public const string EDIT_DOCUMENT_TYPE_C = nameof(EDIT_DOCUMENT_TYPE_C);
            public const string VIEW_INVESTIGATION_DETAIL = nameof(VIEW_INVESTIGATION_DETAIL);
            public const string SUBMIT_INVESTIGATION_DETAIL = nameof(SUBMIT_INVESTIGATION_DETAIL);
            public const string CONFIRM_INVESTIGATION_DETAIL = nameof(CONFIRM_INVESTIGATION_DETAIL);
            

            //Mobile
            public const string MC_BUSINESS = nameof(MC_BUSINESS);
            public const string PCB_BUSINESS = nameof(PCB_BUSINESS);

            public const string DEPARTMENT_DATA_INQUIRY = nameof(DEPARTMENT_DATA_INQUIRY);
            public const string FACTORY_DATA_INQUIRY = nameof(FACTORY_DATA_INQUIRY);
            public const string CREATE_DOCUMENT_BY_DEPARTMENT = nameof(CREATE_DOCUMENT_BY_DEPARTMENT);

            public static readonly List<Tuple<string, string>> PermissionsList = new List<Tuple<string, string>>
            {
                new Tuple<string, string>(WEBSITE_ACCESS, WEBSITE_ACCESS),
                new Tuple<string, string>(MOBILE_ACCESS, MOBILE_ACCESS),
                new Tuple<string, string>(USER_MANAGEMENT, USER_MANAGEMENT),
                new Tuple<string, string>(DEPARTMENT_MANAGEMENT, DEPARTMENT_MANAGEMENT),
                new Tuple<string, string>(ROLE_MANAGEMENT, ROLE_MANAGEMENT),
                new Tuple<string, string>(MASTER_DATA_READ, MASTER_DATA_READ),
                new Tuple<string, string>(MASTER_DATA_WRITE, MASTER_DATA_WRITE),
                new Tuple<string, string>(INPUT, INPUT),
                new Tuple<string, string>(HISTORY_MANAGEMENT, HISTORY_MANAGEMENT),

                new Tuple<string, string>(VIEW_ALL_INVENTORY, VIEW_ALL_INVENTORY),
                new Tuple<string, string>(VIEW_CURRENT_INVENTORY, VIEW_CURRENT_INVENTORY),
                new Tuple<string, string>(EDIT_INVENTORY, EDIT_INVENTORY),
                new Tuple<string, string>(VIEW_REPORT, VIEW_REPORT),
                new Tuple<string, string>(EDIT_DOCUMENT_TYPE_C, EDIT_DOCUMENT_TYPE_C),
                new Tuple<string, string>(VIEW_INVESTIGATION_DETAIL, VIEW_INVESTIGATION_DETAIL),
                new Tuple<string, string>(SUBMIT_INVESTIGATION_DETAIL, SUBMIT_INVESTIGATION_DETAIL),
                new Tuple<string, string>(CONFIRM_INVESTIGATION_DETAIL, CONFIRM_INVESTIGATION_DETAIL),

                new Tuple<string, string>(MC_BUSINESS, MC_BUSINESS),
                new Tuple<string, string>(PCB_BUSINESS, PCB_BUSINESS),
                new Tuple<string, string>(DEPARTMENT_DATA_INQUIRY, DEPARTMENT_DATA_INQUIRY),
                new Tuple<string, string>(FACTORY_DATA_INQUIRY, FACTORY_DATA_INQUIRY),
                new Tuple<string, string>(CREATE_DOCUMENT_BY_DEPARTMENT, CREATE_DOCUMENT_BY_DEPARTMENT),
            };

            public static string PermissionTypeByValue(string value)
            {
                return PermissionsList.FirstOrDefault(x => x.Item2 == value)?.Item1 ?? string.Empty;
            }

            public static readonly Dictionary<string, string> PermissionTitle = new Dictionary<string, string>
            {
                { WEBSITE_ACCESS, "Quyền truy cập Website" },
                { MOBILE_ACCESS, "Quyền truy cập Mobile" },
                { DEPARTMENT_MANAGEMENT, "Quản lý phòng ban" },
                { USER_MANAGEMENT, "Quản lý người dùng" },
                { ROLE_MANAGEMENT, "Quản lý nhóm quyền" },
                { MASTER_DATA_READ, "Xem dữ liệu Master Data" },
                { MASTER_DATA_WRITE, "Chỉnh sửa dữ liệu Master Data" },
                { INPUT, "Nhập kho" },
                { HISTORY_MANAGEMENT, "Xem lịch sử xuất nhập kho" },

                { MC_BUSINESS, "Nghiệp vụ của MC" },
                { PCB_BUSINESS, "Nghiệp vụ của PCB" },
                { DEPARTMENT_DATA_INQUIRY, "Xem dữ liệu theo phòng ban" },
                { FACTORY_DATA_INQUIRY, "Xem dữ liệu theo nhà máy" },


                { VIEW_ALL_INVENTORY, "Xem tất cả dữ liệu kiểm kê" },
                { VIEW_CURRENT_INVENTORY, "Xem dữ liệu kiểm kê hiện tại" },
                { EDIT_INVENTORY, "Chỉnh sửa dữ liệu kiểm kê" },
                { VIEW_REPORT, "Xem báo cáo kiểm kê" },
                { EDIT_DOCUMENT_TYPE_C, "Chỉnh sửa phiếu C" },
                { CREATE_DOCUMENT_BY_DEPARTMENT, "Tạo phiếu theo phòng ban" },
                { VIEW_INVESTIGATION_DETAIL, "Xem chi tiết điều tra" },
                { SUBMIT_INVESTIGATION_DETAIL, "Điều chỉnh dữ liệu điều tra" },
                { CONFIRM_INVESTIGATION_DETAIL, "Xác nhận dữ liệu điều chỉnh" },

            };


            public static readonly List<string> WebSitePermissionList = new List<string>()
            {
                 DEPARTMENT_MANAGEMENT,
                 USER_MANAGEMENT,
                 ROLE_MANAGEMENT,
                 MASTER_DATA_READ,
                 MASTER_DATA_WRITE,
                 INPUT,
                 HISTORY_MANAGEMENT,

                 VIEW_ALL_INVENTORY,
                 VIEW_CURRENT_INVENTORY,
                 EDIT_INVENTORY,
                 VIEW_REPORT,
                 EDIT_DOCUMENT_TYPE_C,
                 VIEW_INVESTIGATION_DETAIL,
                 SUBMIT_INVESTIGATION_DETAIL,
                 CONFIRM_INVESTIGATION_DETAIL
            };

            public static List<string> MobilePermissionList = new List<string>()
            {
                 MC_BUSINESS,
                 PCB_BUSINESS,
            };

            public static class InventoryAccountRoleType
            {
                public static int Inventory = 0;
                public static int Audit = 1;
                public static int Promotion = 2;
            }
        }

        public class Roles
        {
            public const string Administrator = "Quyền Admin";
            public const string ID_Administrator = "11111111-1111-1111-1111-111111111111";
            public const string CREATE_DOCUMENT_BY_DEPARTMENT = "CREATE_DOCUMENT_BY_DEPARTMENT";
            public const string EDIT_DOCUMENT_TYPE_C = "EDIT_DOCUMENT_TYPE_C";
        }

        public class Password
        {
            public const string TinhVan_Account = "Tinhvan2023@";
            public const string TruongPhongIT_Account = "Truongphongit1@";
            public const string NhanVienMC_Account = "Nhanvienmc1@";
            public const string NhanVienPCB_Account = "Nhanvienpcb1@";
        }

        public class DefaultAccount
        {
            public const string Password = "Bivnadmin@2072";
            public const string UserName = "Administrator";
            public const string FullName = "Administrator";
            public const string DepartmentName = "Phòng Admin";
            public const string RoleName = "Quyền Admin";
            public const string UserCode = "000000001";
            //Phân loại lỗi: Kiểm kê sai:
            public const string InventoryErrorCategory = "1";
            public const string InvGRoleName = "Inv.G";
            public const string AdministratorRoleName = "Administrator";
            public const string MCRoleName = "Nhân viên MC";
            public const string InventoryRoleName = "Kiểm Kê";
        }


        public class UserClaims
        {
            public const string Token = "token";
            public const string UserId = "id";
            //public const string Identifier = "Identifier";
            public const string Username = "username";
            public const string Code = "code";
            public const string DepartmentId = "department_id";
            public const string DepartmentName = "department_name";
            public const string RoleId = "role_id";
            public const string RoleName = "role_name";
            public const string ExpiredDate = "expired_date";
            public const string FullName = "fullname";
            public const string AccountType = "account_type";
            public const string Email = "email";
            public const string Phone = "phone";
            public const string DeviceId = "device_id";
            public const string Avatar = "avatar";
            public const string Status = "status";
            public const string SecurityStamp = "security_stamp";
            public const string InventoryId = "inventoryId";
            public const string InventoryDate = "inventoryDate";
        }
        public class Endpoint
        {
            public const string Index = "/";

            // Web App
            public const string WebApp_Login = "login";
            public const string WebApp_Logout = "logout";
            public const string WebApp_Change_Password = "change-password";
            public const string WebApp_Create_User = "create-user";
            public const string WebApp_Get_User_Detail = "get-user-detail";
            public const string WebApp_Update_User = "update-user";
            public const string WebApp_Export_User_List = "export-user-list";
            public const string WebApp_Get_Component_List = "get-component-list";
            public const string WebApp_Import_Component_List = "import-component-list";
            public const string WebApp_Reset_Password = "reset-password";

            public const string WebApp_AuditMobile = "auditmobile";

            public const string WebApp_InventoryReporting = "inventory-reporting";


            // Api
            public const string API_Identity_Login = "/api/identity/login";
            public const string API_Identity_Authorize_Token = "/api/identity/authorize-token";
            public const string API_Identity_Refresh_Token = "/api/identity/refresh-token";
            public const string API_Identity_Logout = "/api/identity/logout";
            public const string API_Identity_ForceLogout = "api/identity/force_logout";
            public const string API_Identity_ForceLogoutUsers = "api/identity/force_logout/multiple";
            public const string API_Identity_Change_Password = "/api/identity/change-password";
            public const string API_Identity_Reset_Password = "/api/identity/reset-password";
            public const string API_Identity_Create_User = "/api/identity/create-user";
            public const string API_Identity_Get_User_Detail = "/api/identity/get-user-detail";
            public const string API_Identity_Update_User = "/api/identity/update-user";
            public const string API_Identity_Get_User_Info = "/api/identity/get-user-info";
            public const string API_Identity_Get_Filter_User_List = "api/identity/filter/user";
            public const string API_Storage_Get_Layout_List = "api/storage/get-layout-list";
            public const string API_Storage_Get_Component_List = "api/storage/get-component-list";
            public const string API_Storage_Get_Layout_DropDownList = "api/storage/get-layout-dropdownlist";
            public const string API_Storage_Get_Component_List_Export = "api/storage/get-component-list/export";
            public const string API_Identity_Get_Filter_User_List_Export = "api/identity/get-user-list-export";
            public const string API_Storage_Get_History_InOut_Export = "api/storage/histories/export-excel";
            public const string API_Storage_Get_InventoryStatus_DropDownList = "api/storage/get-inventory-status-dropdownlist";

            public const string API_Storage_Import_Component_List = "api/storage/import-component-list";
            // Background jobs
            public const string API_Identity_Lock_Users = "/api/identity/lock-users";
            public const string API_Identity_Remove_Expired_Token = "/api/identity/remove-tokens";
            
            // Exception endpoint list inside itself identity service that don't require token validation
            public static readonly List<string> ExceptionEndpointList_IdentityService_NoRequireTokenValidation = new List<string>
            {
                API_Identity_Login,
                API_Identity_Authorize_Token,

                API_Identity_Lock_Users,
                API_Identity_Remove_Expired_Token
            };

            // Identity service's endpoint list only validate token in itself identity service request handler middleware
            // They don't need to validate by token validation api rest request
            public static readonly List<string> EndpointList_ValidateTokenIn_ItselfIdentityServiceRequestHandlerMiddleware = new List<string>
            {
                WebApp_Logout,
                WebApp_Change_Password,
                WebApp_Create_User,
                WebApp_Get_User_Detail,
                WebApp_Update_User
            };

            public const string api_Identity_Department_Users = "api/identity/department/users";
            public const string api_Identity_Department = "api/identity/department";
            public const string api_Internal_Factories = "api/internal/factories";
            public const string api_Inventory_Location_Departments = "api/inventory/location/departments";
            public const string api_Inventory_Location_DepartmentName = "api/inventory/location/departmentname";
            public const string api_Inventory_Web_Export = "api/inventory/web/export";
            public const string api_Inventory_Web_Dropdown_InventoryName = "api/inventory/web/dropdown/inventory-name";
            public const string api_Storage_Factory = "api/storage/factory";
            public const string api_Identity_Role = "api/identity/role";
            public const string api_Identity_Role_Create = "api/identity/role/create";
            public const string api_Identity_Status = "api/identity/status";
            public const string api_Identity_Edit_Role = "api/identity/role/edit";

            public const string api_Iventory_Web_DocResult_Export = "api/inventory/web/document-results/export";
            public const string api_Iventory_Web_History_Export = "api/inventory/web/history/export";
            public const string api_Iventory_Web_Document_Export = "api/inventory/web/document/export";

            public static class Internal
            {
                public const string absolute = "api/internal";
                public const string getUsers = "api/internal/list/user";
                public const string Get_All_Roles_Department = "api/internal/role/by-department";
            }

            public static class InventoryService
            {
                public static class Inventory
                {
                    public const string root = "api/inventory";

                    public const string inventory_Check = "{inventoryId}/account/{accountId}/inventory-check";
                    public const string scan_doc_AE = "{inventoryId}/doc-ae/account/{accountId}/code/{componentCode}/action/{actionType}";
                    public const string doc_C = "doc-c";
                    public const string detail_Document = "{inventoryId}/account/{accountId}/document/{documentId}/action/{actionType}";
                    public const string detailHistoryId = "{inventoryId}/account/{accountId}/history/{historyId}";
                    public const string dropdown_DocC_Models = "{inventoryId}/account/{accountId}/doc-c/dropdown/models";
                    public const string dropdown_DocC_Machines = "{inventoryId}/account/{accountId}/doc-c/dropdown/{machineModel}/machines";
                    public const string dropdown_DocC_Lines = "{inventoryId}/account/{accountId}/doc-c/dropdown/{machineModel}/machines/{machineType}/lines";
                    public const string submit_Inventory = "{inventoryId}/account/{accountId}/document/{docId}/submit-inventory";
                    public const string submit_Confirm = "{inventoryId}/account/{accountId}/document/{docId}/action/{actionType}/submit-confirm";
                    public const string dropdown_Department = "{inventoryId}/account/{accountId}/dropdown/department";
                    public const string dropdown_Department_By_Location = "{inventoryId}/account/{accountId}/dropdown/department/{departmentName}/location";
                    public const string dropdown_Location = "{inventoryId}/account/{accountId}/dropdown/department/{departmentName}/location/{locationName}/component";
                    public const string list_Audit = "list-audit";
                    public const string audit_Scan_QR = "{inventoryId}/account/{accountId}/audit/scan/{componentCode}";
                    public const string submit_Audit = "{inventoryId}/account/{accountId}/document/{docId}/action/{actionType}/submit-audit";
                    public const string import_DocType = "{inventoryId}/{docType}/import";
                    public const string doc_C_Validate = "{inventoryId}/doc-c/validate";
                    public const string import_Doc_C = "{inventoryId}/import-doc-c";
                    public const string isHighlight_Check = "ishightlight-check";
                    public const string dropdown_DocB_Models = "{inventoryId}/account/{accountId}/doc-b/dropdown/models";
                    public const string dropdown_DocB_Machines = "{inventoryId}/account/{accountId}/doc-b/dropdown/{machineModel}/machines";
                    public const string dropdown_DocB_ModelCodes = "{inventoryId}/account/{accountId}/doc-b/dropdown/{machineModel}/machines/{machineType}/modelcodes";
                    public const string dropdown_DocB_Lines = "{inventoryId}/account/{accountId}/doc-b/dropdown/{machineModel}/machines/{machineType}/modelcodes/{modelCode}/lines";
                    public const string doc_B = "doc-b";
                    public const string doc_AE = "doc-ae";
                    public const string scan_doc_B = "scan/doc-b";
                    public const string list_doc_C = "list/doc-c";
                    public const string IMPORT_DOC_SHIP = "{inventoryId}/import-doc-ship";
                }

                public static class InventoryHistory
                {
                    public const string root = "api/inventory/history";

                    public const string historyDetail = "detail";
                }

                public static class InventoryWeb
                {
                    public const string root = "api/inventory/web";

                    public const string createInventory = "create";
                    public const string listInventoryToExport = "export";
                    public const string updateStatusInventory = "status/{inventoryId}/{status}/{userId}";
                    public const string inventoryDetail = "{inventoryId}";
                    public const string updateInventoryDetail = "{inventoryId}";
                    public const string receivedDocStatus = "update-status";
                    public const string listInventoryDocumentToExport = "{inventoryId}/document/export";
                    public const string listInventoryDocument = "{inventoryId}/document";
                    public const string downloadUpdateStatusFileTemplate = "template/download/upload-status";
                    public const string uploadChangeDocStatus = "upload/change-status";
                    public const string listInventoryDocumentFull = "document";
                    public const string listInventoryDocumentFullToExport = "document/export";
                    public const string inventoryDocDetail = "document/{docId}";
                    public const string inventoryNames = "dropdown/inventory-name";
                    public const string deleteInventorys = "{inventoryId}/document";
                    public const string getDetailInventory = "{inventoryId}/document/{docId}";
                    public const string getDocumentTypeC = "document/typec";
                    public const string documentResults = "document-results";
                    public const string exportDocumentResultExcel = "document-results/export-excel";
                    public const string documentResultsToExport = "document-results/export";
                    public const string documentResultsImportSAP = "{inventoryId}/import-sap";
                    public const string listDocumentHistories = "history";
                    public const string listDocumentHistoriesToExport = "history/export";
                    public const string importResultFromBwins = "{inventoryId}/import-result-bwins";
                    public const string exportTreeGroups = "{inventoryId}/groups/export";
                    public const string getTreeGroupFilters = "groups/filters";
                    public const string docDetailComponentsC = "doc-detail/components-c";
                    public const string receiveAllDocs = "receive-all";
                    public const string checkAssignedDocs = "update-doc/check";
                    public const string checkDownloadDocTemplate = "update-doc/template/check";
                    public const string checkExistDocTypeA = "doctype-a/check/{inventoryId}";
                    public const string aggregateDocResults = "{inventoryId}/doc-result/aggregate";
                    public const string deleteInventoryDocs = "{inventoryId}/inventory-docs";
                    public const string listDocTypeCToExportQRCode = "doc-type-c/export/qrcode";
                    public const string getTreeGroupQRCodeFilters = "groups/qrcode/filters";
                    public const string getTreeGroupInventoryErrorFilters = "groups/inventory/error/filters";
                    public const string listDocumentToInventoryError = "export/inventory/error";
                    public const string importMSLDataUpdate = "{inventoryId}/import/msl-data";
                }

               

                public static class Location
                {
                    public const string root = "api/inventory/location";
                    
                    public const string deleteLocation = "{locationId}";
                    public const string locationDetail = "{locationId}";
                    public const string inventoryAssignedList = "actors";
                    public const string changeAccountRole = "account/{userId}/role/{roleType?}";
                    public const string changeAccountLocation = "account/{userId}";
                    public const string exportActors = "assignment/export";
                    public const string deparments = "departments";
                    public const string getLocationByDepartments = "departmentname";
                    public const string checkLocationAssignedToDoc = "{locationId}/assigned-document";
                    public const string getAuditorByLocations = "auditor";
                }

                public static class Internal
                {
                    public const string root = "api/inventory/internal";

                    public const string inventoryAccount = "account/{userId}";
                    public const string deleteInventoryAccount = "inventory-account/{userId}";
                    public const string updateInventoryAccount = "inventory-account/{userId}/{newUserName}";
                    public const string checkAuditAccountAssignLocation = "account/{userId}/check-assignlocation";
                }
            }

            public static class ErrorInvestigationService
            {
                public static class ErrorInvestigationRoute
                {
                    public const string root = "api/error-investigation";

                    public const string ListErrorInvestigation = "inventory/{inventoryId}";
                    public const string UpdateErrorInvestigationStatus = "inventory/{inventoryId}/componentCode/{componentCode}/status";
                    public const string ConfirmErrorInvestigation = "inventory/{inventoryId}/componentCode/{componentCode}/type/{type}";
                    public const string ErrorInvestigationDocumentList = "inventory/{inventoryId}/componentCode/{componentCode}/documents";
                    public const string ErrorInvestigationConfirmedViewDetail = "inventory/{inventoryId}/componentCode/{componentCode}/view-detail";
                    public const string ErrorInvestigationHistories = "inventory/{inventoryId}/componentCode/{componentCode}/histories";
                    
                }
                public static class ErrorInvestigationWebRoute 
                {
                    public const string root = "api/error-investigation/web";
                    public const string ListErrorInvestigation = "inventory";
                    public const string ListErrorInvestigationExportFile = "inventory/export-file";
                    public const string ExportDataAdjustment = "inventory/{inventoryId}/export-data-adjustment";
                    public const string ListErrorInvestigationDocuments = "inventory/{inventoryId}/componentCode/{componentCode}/documents";
                    public const string ListErrorInvestigationDocumentsCheck = "inventory/{inventoryId}/componentCode/{componentCode}/documents-check";
                    public const string ListErrorInvestigationInventoryDocsHistory = "history/componentCode/{componentCode}/inventory-docs";
                    public const string ListErrorInvestigationHistory = "inventory/history";
                    public const string ListErrorInvestigationHistoryExportFile = "inventory/history/export-file";
                    public const string ListInvestigationDetail = "inventory/detail";
                    public const string UpdateErrorTypesForInvestigationHistory = "inventory/error-types";
                    public const string InvestigationPercent = "inventory/{inventoryId}/investigation-percent";
                    public const string ListInvestigationDetailExport = "inventory/detail/export";
                    public const string ErrorPercent = "inventory/{inventoryId}/error-percent";
                    public const string ImportErrorInvestigationUpdate = "inventory/error-investigation/update";
                    public const string ImportErrorInvestigationUpdatePivot = "inventory/{inventoryId}/error-investigation/update-pivot/import";
                    public const string ErrorCategoryManagement = "management/error-category";
                    public const string AddNewErrorCategoryManagement = "management/error-category/add";
                    public const string UpdateErrorCategoryManagement = "management/error-category/{errorCategoryId}/edit";
                    public const string RemoveErrorCategoryManagement = "management/error-category/{errorCategoryId}/remove";
                    public const string ErrorCategoryManagementById = "management/error-category/{errorCategoryId}";
                }
                
            }
        }
        public class ErrorInvestigationColumn
        {
            public const string ComponentCode = "ComponentCode";
            public const string ErrorQuantity = "ErrorQuantity";
            public const string Plant = "Plant";
            public const string WHLoc = "WHLoc";
            public const string OrderByAsc = "asc";
            public const string OrderByDesc = "desc";
        }
        public class InputStorage
        {
            /// <summary>
            /// Sau khi phân bổ bị dư ra, chưa có vị trí cố định
            /// </summary>
            public const string remainingPostionCode = "_________";
        }

        public class AllowableCharacters
        {
            public static readonly List<string> Vietnamese = new List<string>()
            {
                "a", "A", "á", "Á", "à", "À", "ạ", "Ạ", "ả", "Ả", "ã", "Ã",
                "ă", "Ă", "ắ", "Ắ", "ằ", "Ằ", "ặ", "Ặ", "ẳ", "Ẳ", "ẵ", "Ẵ",
                "â", "Â", "ấ", "Ấ", "ầ", "Ầ", "ậ", "Ậ", "ẩ", "Ẩ", "ẫ", "Ẫ",
                "e", "E", "é", "É", "è", "È", "ẹ", "Ẹ", "ẻ", "Ẻ", "ẽ", "Ẽ",
                "ê", "Ê", "ế", "Ế", "ề", "Ề", "ệ", "Ệ", "ể", "Ể", "ễ", "Ễ",
                "i", "I", "í", "Í", "ì", "Ì", "ị", "Ị", "ỉ", "Ỉ", "ĩ", "Ĩ",
                "o", "O", "ó", "Ó", "ò", "Ò", "ọ", "Ọ", "ỏ", "Ỏ", "õ", "Õ",
                "ô", "Ô", "ố", "Ố", "ồ", "Ồ", "ộ", "Ộ", "ổ", "Ổ", "ỗ", "Ỗ",
                "ơ", "Ơ", "ớ", "Ớ", "ờ", "Ờ", "ợ", "Ợ", "ở", "Ở", "ỡ", "Ỡ",
                "u", "U", "ú", "Ú", "ù", "Ù", "ụ", "Ụ", "ủ", "Ủ", "ũ", "Ũ",
                "ư", "Ư", "ứ", "Ứ", "ừ", "Ừ", "ự", "Ự", "ử", "Ử", "ữ", "Ữ",
                "y", "Y", "ý", "Ý", "ỳ", "Ỳ", "ỵ", "Ỵ", "ỷ", "Ỷ", "ỹ", "Ỹ",
                "b", "B", "c", "C", "d", "D", "đ", "Đ", "f", "F", "g", "G", "h", "H", "k", "K", "l", "L", "m", "M", "n", "N", "p", "P", "q", "Q", "r", "R", "s", "S", "t", "T", "v", "V", "x", "X", "z", "Z",
                "oa", "OA", "oe", "OE", "uy", "UY", "uê", "UÊ", "ch", "CH", "gh", "GH", "kh", "KH", "ng", "NG", "ngh", "NGH", "nh", "NH", "ph", "PH", "th", "TH", "tr", "TR", "gi", "GI", "qu", "QU",
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "'",
                " ","-","_"
            };

            public static readonly List<string> English = new List<string>()
            {
                "a", "A",
                "e", "E",
                "i", "I",
                "o", "O",
                "u", "U",
                "y", "Y",
                "b", "B", "c", "C", "d", "D", "f", "F", "g", "G", "h", "H", "j", "J", "k", "K", "l", "L", "m", "M", "n", "N", "p", "P", "q", "Q", "r", "R", "s", "S", "t", "T", "v", "V", "w",  "W", "x", "X", "z", "Z",
                "oa", "OA", "oe", "OE", "uy", "UY", "ch", "CH", "gh", "GH", "kh", "KH", "ng", "NG", "ngh", "NGH", "nh", "NH", "ph", "PH", "th", "TH", "tr", "TR", "gi", "GI", "qu", "QU",
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "'",
                " ","_","-"
            };
        }

        public class SpecialCharacters
        {
            public static readonly List<string> List = new List<string>()
            {
                 " \t",
                 " \n",
                 "~",
                 "`",
                 "!",
                 "@",
                 "#",
                 "$",
                 "%",
                 "^",
                 "&",
                 "*",
                 "(",
                 ")",
                 "+",
                 "=",
                 "{",
                 "}",
                 "[",
                 "]",
                 "|",
                 "\\",
                 "/",
                 ":",
                 ";",
                 "\"",
                 "<",
                 ">",
                 "'",
                 ".",
                 "?"
            };
        }

        public class ColumnComponentImport
        {
            public const string No = "NO";
            public const string ComponentCode = "Mã linh kiện";
            public const string ComponentName = "Tên linh kiện";
            public const string SupplierCode = "Mã nhà cung cấp";
            public const string SupplierName = "Tên nhà cung cấp";
            public const string SupplierShortName = "Tên nhà cung cấp rút gọn";
            public const string PositionCode = "Vị trí cố định";
            public const string MinInventoryNumber = "Trạng thái tồn kho Min";
            public const string MaxInventoryNumber = "Trạng thái tồn kho Max";
            public const string InventoryNumber = "Tồn kho thực tế";
            public const string ComponentInfo = "Thông tin LK";
            public const string Note = "Ghi chú";
            public const string Errors = "Lỗi";
        }

        public class UploadDocStateExcelHeaderName
        {
            public const string STT = "STT";
            public const string LocationName = "Khu vực";
            public const string DocCode = "Mã phiếu";
            public const string ComponentCode = "Mã linh kiện";
            public const string ModelCode = "Model code";
            public const string Plant = "Plant";
            public const string WHLoc = "WH.Loc";
            public const string DocStatus = "Trạng thái";
        }

        /// <summary>
        /// Các giá trị tầng hợp lệ: 1, 2, 3, 4
        /// </summary>
        public class ValidStorages
        {
            public const int Storage1 = 1;
            public const int Storage2 = 2;
            public const int Storage3 = 3;
            public const int Storage4 = 4;

            public static readonly List<int> Numbers = new List<int>
            {
                Storage1,
                Storage2,
                Storage3,
                Storage4
            };
        }

        public class RegexPattern
        {
            public const string PositionCodeRegex = "^(?!0)\\d{1,2}(T(?!0)\\d{1}|)(([A-Z]{1}[1-9]{1}0(\\/1|\\/2|))|([A-Z]{2}[1-9]{1}0[A-Z]{1})|([A-Z]{1}[1-9]{1}0[A-Z]{1})|([A-Z]{1,3}[1-9]{1}0)|([A-Z]{1}0[1-9]{1})|([A-Z]{1}[A-Z1-9]{3,4})|([A-Z]{1}[A-Z1-9]{1,2}(\\/1|\\/2|)))(-\\d{2}\\/\\d{2}|)((((-\\d{2})(\\/\\d{2})|)(-\\d{2}|))(-\\d{2})|)(\\/\\d{2}|)$";
            /// <summary>
            ///<list type="">
            ///<item>group 1 - Machine model</item>
            ///<item>group 2 - Machine type</item>
            ///<item>group 3 - Line name</item>
            ///<item>group 4 - Stage name</item>
            ///<item>group 5 - Stage number</item>
            /// </list> 
            /// </summary>
            public const string ModelCodeRegex = "([A-Za-z0-9]{4})(P|F|T|D|0)(0(?=a)|0(?=b)|0(?=c)|[A-Z](?=d)|[A-Z](?=e))(a|b|c|d|e)((?<!e)(?!000)[0-9]{3}|(?<=e)00[1-2])";

            public const string ShareGrpRegex = "([A-Za-z0-9]{4})(P|F|T|D)(0)(a|b|c)((?!000)[0-9]{3})";
            public const string AssenblyAndfinishGrpRegex = "([A-Za-z0-9]{4})(P|F|T|D)([A-Z])(d|e)((?!000)[0-9]{3})";
            public const string FinishGrpRegex = "([A-Za-z0-9]{4})(P|F|T|D)([A-Z])(e)((?!000)[0-9]{3})";
            public const string MainLineGrpRegex = "([A-Za-z0-9]{4})(P|F|T|D)([A-Z])(a|b|c|d|e)((?!000)[0-9]{3})";
            public const string ShareGrpByModelRegex = "([A-Za-z0-9]{4})(0)(0)(a|b|c)((?!000)[0-9]{3})";
            public const string MaterialCodeRegex = "([A-Z0-9]{9})";
            public const string DocCodeRegex = @"([ABEC]{1,2}\d{4})(\d{5})";
            public const string MachineModelRegex = "(?!0000)(^[A-Za-z0-9]{4})";
            public const string QuantityOfBom_Mutil_QuantityPerBomRegex = @"(\d+([.]\d+|))[*](\d+([.]\d+|))";
        }
        public class ImportExcelColumns
        {
            public class TypeA
            {
                public const string Plant = "Plant";
                public const string WarehouseLocation = "Warehouse Location";
                public const string Quantity = "Quantity";
                public const string C = "C";
                public const string SpecialStock = "Special Stock";
                public const string StockTypes = "Stock Types";
                public const string SONo = "S/O No.";
                public const string SOList = "S/O List";
                public const string PhysInv = "Phys.Inv.";
                public const string FiscalYear = "Fiscal year";
                public const string Item = "ITEM";
                public const string PlannedCountDate = "Planned count date";
                public const string ComponentCode = "Material code";
                public const string ComponentName = "Description";
                public const string N = "N";
                public const string O = "O";
                public const string P = "P";
                public const string Q = "Q";
                public const string R = "R";
                public const string S = "S";
                public const string PositionCode = "Storage bin";
                public const string Note = "Ghi chú";
                public const string Assignee = "Tài khoản phân phát";
                public const string ErrorContent = "Nội dung lỗi";

                public static string[] RequiredColumns = new string[] { nameof(Plant), nameof(WarehouseLocation), nameof(PhysInv), nameof(ComponentCode), nameof(ComponentName), nameof(PositionCode), nameof(SpecialStock) };
            }
            public class TypeB
            {
                public const string No = "No.";
                public const string Plant = "Plant";
                public const string WarehouseLocation = "WH Loc.";
                public const string ComponentCode = "Material Code";
                public const string StockType = "Stock Type";
                //public const string SpecialStock = "Special Stock";
                public const string ModelCode = "Model Code";
                //public const string SONo = "S/O No";
                //public const string SOList = "S/O List";
                //public const string ProOrderNo = "Pro. Order No.";
                //public const string VendorCode = "Vendor code";
                //public const string PositionCode = "Storage bin";
                //public const string StorageBin = "Storage bin";
                public const string AssemblyLoc = "Assembly Loc.";
                public const string MachineModel = "Model";
                public const string MachineType = "Dòng máy";
                public const string LineName = "Chuyền";
                public const string Assignee = "Tài khoản phân phát";
                public const string Note = "Ghi chú";
                public const string ErrorContent = "Nội dung lỗi";
                public const string ComponentName = "Description";

                public static string[] RequiredColumns = new string[] { nameof(Plant), nameof(WarehouseLocation), nameof(ComponentCode), nameof(StockType), nameof(ModelCode), nameof(AssemblyLoc), nameof(MachineModel), nameof(MachineType), nameof(LineName), nameof(Assignee) };
            }
            public class TypeE
            {
                public const string No = "No.";
                public const string Plant = "Plant";
                public const string WarehouseLocation = "WH Loc.";
                public const string ComponentCode = "Material Code";
                public const string StockType = "Stock Type";
                public const string ModelCode = "Model Code";
                public const string StorageBin = "Storage bin";
                public const string Assignee = "Tài khoản phân phát";
                public const string Note = "Ghi chú";
                public const string ErrorContent = "Nội dung lỗi";
                public const string ComponentName = "Description";

                public static string[] RequiredColumns = new string[]
                {
                    nameof(Plant), nameof(WarehouseLocation), nameof(ComponentCode), nameof(StockType), nameof(ModelCode), nameof(StorageBin), nameof(Assignee)
                };
            }
            public class TypeC
            {
                public const string No = "No.";
                public const string Plant = "Plant";
                public const string WarehouseLocation = "WH Loc.";
                public const string ModelCode = "Model Code";
                public const string MaterialCode = "Material Code";
                public const string BOMUseQty = "BOM Use Qty";
                public const string StageName = "Tên công đoạn";
                public const string Assignee = "Tài khoản phân phát";

                public static List<string> listHeaderTypeC = new List<string>
                {
                    No,
                    Plant,
                    WarehouseLocation,
                    ModelCode,
                    MaterialCode,
                    BOMUseQty,
                    StageName,
                    Assignee,
                };
            }
            public class AuditTargets
            {
                public const string No = "No";
                public const string Plant = "Plant";
                public const string WarehouseLocation = "WH Loc.";
                public const string SONo = "S/O No.";
                public const string PositionCode = "Storage bin";
                public const string ComponentName = "Description";
                public const string Assignee = "Tài khoản phân phát";
                public const string MaterialCode = "Material code";
                public const string ComponentCode = "Material code";
                public const string Note = "Ghi chú";

                public static string[] RequiredColumns = new string[] { nameof(Plant), nameof(WarehouseLocation), nameof(ComponentCode), nameof(PositionCode), nameof(Assignee) };
            }

        }

        public class FileResponse
        {
            public const string StreamType = "application/octet-stream";
            public const string ExcelType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            public const string Txt = "text/plain";
        }

        public class DocumentResult
        {
            public const string ComponentCode = "ComponentCode";
            public const string ErrorQuantity = "ErrorQuantity";
            public const string ErrorMoneyAbs = "ErrorMoneyAbs";
            public const string OrderByAsc = "asc";
            public const string OrderByDesc = "desc";
        }

        public class DocumentResultExcel
        {
            public const string STT = "STT";
            public const string ComponentCode = "Mã linh kiện";
            public const string ModelCode = "Model code";
            public const string Plant = "Plant";
            public const string WHLoc = "WH Loc.";
            public const string Quantity = "Quantity";
            public const string TotalQuantity = "Total Qty";
            public const string Account = "Account Qty";
            public const string ErrorQuantity = "Error Qty";
            public const string ErrorMoney = "Error money";
            public const string UnitPrice = "Unit price";
            public const string DocCode = "Mã phiếu";
            public const string StockTypes = "Stock types";
            public const string SpecialStock = "Special stock";
            public const string SaleOrderNo = "S/O No.";
            public const string PhysInv = "Phys.Inv";
            public const string ProOrderNo = "Pro. Order No";
            public const string InventoryBy = "Người kiểm kê";
            public const string InventoryAt = "Thời gian kiểm kê";
            public const string ConfirmBy = "Người xác nhận";
            public const string ConfirmAt = "Thời gian xác nhận";
            public const string AuditBy = "Người giám sát";
            public const string AuditAt = "Thời gian giám sát";
            public const string ComponentName = "Tên linh kiện";
            public const string Position = "Vị trí";
            public const string AssemblyLoc = "Assembly Loc.";
            public const string VendorCode = "Vendor code";
            public const string SaleOrderList = "S/O List.";
            public const string No = "No.";
            public const string CSAP = "C SAP";
            public const string KSAP = "K SAP";
            public const string MSAP = "M SAP";
            public const string OSAP = "O SAP";

            public static List<string> ExcelHeaders = new List<string>
            {
                STT,
                ComponentCode,
                ModelCode,
                Plant,
                WHLoc,
                Quantity,
                TotalQuantity,
                Account,
                ErrorQuantity,
                ErrorMoney,
                UnitPrice,
                DocCode,
                No,
                StockTypes,
                SpecialStock,
                SaleOrderNo,
                PhysInv,
                ProOrderNo,
                InventoryBy,
                InventoryAt,
                ConfirmBy,
                ConfirmAt,
                AuditBy,
                AuditAt,
                ComponentName,
                Position,
                AssemblyLoc,
                VendorCode,
                SaleOrderList,
                CSAP,
                KSAP,
                MSAP,
                OSAP
            };

            public static int GetColumnIndex(string key)
            {
                var index = ExcelHeaders.IndexOf(key);
                return ++index;
            }
        }

        public class UploadTotalBwinsExcel
        {
            public const string STT = "STT";
            public const string MaterialCode = "Material code";
            public const string Plant = "Plant";
            public const string WHLoc = "WH Loc.";
            public const string Quantity = "Quantity";
            public const string ErrorSummary = "Nội dung lỗi";

            public static List<string> ExportHeaders = new List<string>
            {
                STT,
                MaterialCode,
                Plant,
                WHLoc,
                Quantity,
                ErrorSummary
            };
        }

        public class ImportSAPExcel
        {
            public const string STT = "STT";
            public const string Plant = "Plant";
            public const string WHLoc = "WH Loc.";
            public const string CSAP = "C SAP";
            public const string MaterialCode = "Material code";
            public const string Description = "Description";
            public const string StorageBin = "Storage bin";
            public const string SONo = "S/O No.";
            public const string StockTypes = "Stock Types";
            public const string PhysInv = "Phys.Inv";
            public const string Quantity = "Quantity";
            public const string KSAP = "K SAP";
            public const string AccountQty = "Account Qty";
            public const string MSAP = "M SAP";
            public const string ErrorQty = "Error Qty";
            public const string OSAP = "O SAP";
            public const string UnitPrice = "Unit price";
            public const string ErrorMoney = "Error money";
            //public const string Note = "Ghi chú";
            public const string ErrorSummary = "Nội dung lỗi";

            public static List<string> ExcelHeaders = new List<string>
            {
                STT,
                Plant,
                WHLoc,
                CSAP,
                MaterialCode,
                Description,
                StorageBin,
                SONo,
                StockTypes,
                PhysInv,
                Quantity,
                KSAP,
                AccountQty,
                MSAP,
                ErrorQty,
                OSAP,
                UnitPrice,
                ErrorMoney,
                ErrorSummary
            };

        }

        public class ImportErrorInvestigationUpdateExcel
        {
            public const string STT = "STT";
            public const string InventoryName = "ĐỢT KIỂM KÊ(*)";
            public const string Plant = "PLANT(*)";
            public const string WHloc = "WH LOC.(*)";
            public const string ComponentCode = "MÃ LINH KIỆN(*)";
            public const string PositionCode = "VỊ TRÍ(*)";
            public const string ErrorQuantity = "SỐ LƯỢNG ĐIỀU CHỈNH(*)";
            public const string ErrorCategory = "PHÂN LOẠI LỖI(*)";
            public const string ErrorDetail = "NGUYÊN NHÂN SAI SỐ(*)";
            
            public const string ErrorSummary = "Nội dung lỗi";

            public static List<string> ExcelHeaders = new List<string>
            {
                STT,
                InventoryName,
                Plant,
                WHloc,
                ComponentCode,
                PositionCode,
                ErrorQuantity,
                ErrorCategory,
                ErrorDetail,
                ErrorSummary
            };

        }

        public class ImportMSLDataUpdateExcel
        {
            public const string MovementType = "Movement Type(*)";
            public const string DateTime = "Ngày tháng";
            public const string InputDateTime = "Ngày tháng nhập";
            public const string DeliveryNumber = "Số giao hàng";
            public const string Content = "Nội dung";
            public const string InOutDoc = "Phiếu nhập xuất";
            public const string Plant = "Plant(*)";
            public const string WHLoc = "W.H.Loc(*)";
            public const string ComponentCode = "Mã linh kiện(*)";
            public const string Quantity = "Số lượng nhập(*)";
            public const string Unit = "Đơn vị";
            public const string SpecialInventory = "Phân loại tồn kho đặc biệt";
            public const string OrderNumber = "Số đơn hàng";
            public const string OrderDetailNumber = "Sổ chi tiết đơn hàng";
            public const string GLAccountNumber = "Số tài khoản G/ L";
            public const string CostCenter = "Trung tâm chi phí";
            public const string SupplierAccountNumber = "Số tài khoản nhà cung cấp";
            public const string ReasonForMoving = "Lý do di chuyển";

            public const string ErrorSummary = "Nội dung lỗi";

            public static List<string> ExcelHeaders = new List<string>
            {
                MovementType,
                DateTime,
                InputDateTime,
                DeliveryNumber,
                Content,
                InOutDoc,
                Plant,
                WHLoc,
                ComponentCode,
                Quantity,
                Unit,
                SpecialInventory,
                OrderNumber,
                OrderDetailNumber,
                GLAccountNumber,
                CostCenter,
                SupplierAccountNumber,
                ReasonForMoving,
                ErrorSummary
                
            };

        }

        public class ImportUpdateQuantity
        {
            public const string No = "No";
            public const string DocCode = "Mã phiếu";
            public const string Note = "Note";
            public const string QuantityOfBom_Mutil_QuantityPerBom = "Số lượng thùng x số thùng";
            public const string ErrorSummary = "Nội dung lỗi";

            public static List<string> ExportHeaders = new List<string>
            {
                No,
                DocCode,
                QuantityOfBom_Mutil_QuantityPerBom,
                ErrorSummary
            };
        }

        public class ImportErrorInvestigationUpdatePivotExcel
        {
            public const string Plant = "Plant(*)";
            public const string WHloc = "Wh.Loc";
            public const string AccountQuantity = "Sum of Cur. mth. stock amt. FC(*)";

            public const string ErrorSummary = "Nội dung lỗi";

            public static List<string> ExcelHeaders = new List<string>
            {
                Plant,
                WHloc,
                AccountQuantity,
                ErrorSummary
            };

        }

        public class DocCMachineModel
        {
            public static Dictionary<string, string> MachineTypeDisplayNames = new Dictionary<string, string>
            {
                { "P", "PR" },
                { "F", "FB" },
                { "0", "Sub dùng chung" }
            };

            public static string GetDisplayLineName(string key)
            {
                if (string.IsNullOrEmpty(key)) return string.Empty;

                if (key == "0")
                {
                    return "Chuyền Sub/Unit";
                }
                else
                {
                    return $"LINE {key}";
                }
            }

            public static string GetDisplayMachineType(string key)
            {
                if (string.IsNullOrEmpty(key)) return string.Empty;

                if (MachineTypeDisplayNames.ContainsKey(key))
                {
                    return MachineTypeDisplayNames[key];
                }

                return key;
            }

            public static string GetDisplayLineType(string key)
            {
                if (string.IsNullOrEmpty(key)) return string.Empty;
                switch (key)
                {
                    case "a":
                        return $"Chuyền phụ, Unit ({key})";
                    case "b":
                        return $"Công đoạn ADF ({key})";
                    case "c":
                        return $"Scanner ({key})";
                    case "d":
                        return $"Công đoạn lắp ráp ({key})";
                    case "e":
                        return $"Kiểm tra thành phẩm ({key})";
                    default:
                        return key;
                }
            }
        }

        public class TreeGroupColumn
        {
            public const string No = "STT";
            public const string MainModelCode = "Cụm được tạo";
            public const string AttachModelCode = "Cụm đính kèm {0}";

        }

        public class UploadDocStatusExcel
        {
            public const string STT = "STT";
            public const string LocationName = "LocationName";
            public const string ComponentCode = "ComponentCode";
            public const string DocCode = "DocCode";
            public const string DocStatus = "DocStatus";
            public const string ModelCode = "ModelCode";
            public const string Plant = "Plant";
            public const string WHLoc = "WHLoc";
            public const string ErrorContent = "Nội dung lỗi";

            public static List<string> ExportHeaders = new List<string>
            {
                STT,
                LocationName,
                ComponentCode,
                DocCode,
                DocStatus,
                ModelCode,
                Plant,
                WHLoc,
                ErrorContent,
            };
        }

        public class InventoryAssignExcel
        {
            public const string STT = "STT";
            public const string Actor = "Người thao tác";
            public const string Role = "Vai trò";
            public const string Location = "Khu vực";
            public const string Factory = "Nhà máy";
            public const string Department = "Phòng ban";

            public static List<string> ExportHeaders = new List<string>
            {
                STT,
                Actor,
                Role,
                Location,
                Factory,
                Department,
            };

            public static int ColIndexByName(string colName) => ExportHeaders.IndexOf(colName); 
        }

        public const string DefaultDateFormat = "dd/MM/yyyy HH:mm";
        public const string DayMonthYearFormat = "dd/MM/yyyy";
        public const string DatetimeFormat = "yyyyMMdd_HHmmss";

        public static class AppSettings
        {
            public const string ClientId = "JwtTokens:ClientId";
            public const string ClientSecret = "JwtTokens:ClientSecret";
            public const string APIGateWayAddress = "APIGateWay:BaseAddress";
            public const string LoginRoute = "/login";

            /// <summary>
            /// Sau bao nhiêu ngày tính từ ngày kiểm kê thì có thể sửa khu vực
            /// </summary>
            public static int EditLocationScheduleDays = 30;
        }

        public static class ValidationRules
        {
            public static class Plant
            {
                public const string L401 = nameof(L401);
                public const string L402 = nameof(L402);
                public const string L404 = nameof(L404);

                public static List<string> Keys = new List<string> { L401, L402, L404 };
            }

            public static class WarehouseLocation
            {
                public const string S001 = nameof(S001);
                public const string S002 = nameof(S002);
                public const string S402 = nameof(S402);
                public const string S090 = nameof(S090);

                public static List<string> Keys = new List<string> { S001, S002, S402, S090 };
            }                                                    
        }

        public static class HttpContextModel
        {
            public const string TokenKey = "token";
            public const string AuthorizationKey = "Authorization";
            public const string DeviceIdKey = "device_id";

            public const string ClientIdKey = "ClientId";
            public const string ClientSecretKey = "ClientSecret";
            public const string DeviceId = "DeviceId";

            public const string UserKey = "User";

            public const string ContentTypeKey = "Content-Type";
            public const string ApplicationJson = "application/json";
        }

        public static class ResponseMessages {

            public const string NotFound = "Không tìm thấy dữ liệu phù hợp.";
            public const string UnAuthorized = "Không có quyền truy cập.";
            public const string InValidValidationMessage = "Dữ liệu không hợp lệ.";
            public const string InternalServer = "Lỗi hệ thống, vui lòng thử lại.";

            public const string InvalidFileFormat = "File sai định dạng, vui lòng chọn lại file.";

            public const string InvalidToken = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";

            public const string InvalidId = "Id không hợp lệ.";
            public const string InvalidName = "Tên không hợp lệ.";

            public const string InvalidInventoryDate = "Ngày kiểm kê không có dữ liệu.";
            public const string InventoryDegree = "Tỷ lệ kiểm kê lại không có dữ liệu.";
            public const string UpdateStatusErrorInvestigationSuccessfully = "Update status error investigation successfully.";
            public const string ErrorInvestigationNotFound = "Error investigation not found.";
            public const string ErrorInvestigationDocsNotFound = "Error investigation Docs not found.";
            public const string ConfirmInvestigationSuccessfully = "Confirm error investigation successfully.";
            public const string ErrorInvestigationDocumentListSuccessfully = "error investigation document list successfully.";
            public const string ErrorInvestigationConfirmedViewDetailSuccessfully = "error investigation confirmed view detail successfully.";
            public const string ErrorInvestigationHistoriesSuccessfully = "error investigation histories successfully.";
            public const string ErrorInvestigationDocTypeANotFound = "Error investigation DocType A not found.";

            public const string ListErrorInvestigationWebSuccessfully = "List Error Investigation successfully.";
            public const string ListErrorInvestigationHistoryWebSuccessfully = "List Error Investigation History Successfully.";
            public const string ListErrorInvestigationDocumentsSuccessfully = "List Error Investigation Documents Successfully.";

            public static string LogParams(string seperator = " - ", params object?[] values)
            {
                string prefix = "Lỗi hệ thống";
                if (values.Any())
                {
                    var paramsFormat = string.Join(seperator, values.Select((x, i) => $"{{{i}}}"));
                    return string.Format($"{prefix}: {paramsFormat}", values);
                }
                return string.Format($"{prefix}.");
            }


            public static class Inventory
            {
                
                public const string RequiredInputValue = "Vui lòng nhập tên nhà máy.";
                public const string RequiredFactoryName = "Vui lòng nhập tên nhà máy.";

                public const string NotFound = "Không tìm thấy dữ liệu đợt kiểm kê hiện tại.";
                public const string NotFoundInventoryList = "Không tìm thấy danh sách đợt kiểm kê.";
                public const string NotFoundDetail = "Không tìm thấy chi tiết đợt kiểm kê.";

                public const string CreateSuccess = "Thêm mới đợt kiểm kê thành công.";
                public const string DeleteSuccess = "Xóa dữ liệu kiểm kê thành công.";

                public const string UpdateInventoryAccountSuccess = "Cập nhật tài khoản kiểm kê thành công.";
                public const string UpdateStateSuccess = "Cập nhật trạng thái đợt kiểm kê thành công.";
                public const string UpdateInfoSuccess = "Cập nhật thông tin đợt kiểm kê thành công.";
                public const string DeleteDetailSuccess = "Xóa đợt kiểm kê thành công.";

                public static string ShcheduleDaysMessage (int day) => String.Format("Sau ngày kiểm kê {0} ngày mới có thể thực hiện chỉnh sửa.", day);

                
            }

            public static class Storage
            {

            }

            public static class Identity
            {

            }

        }

        public class InventoryStatusType
        {
            public const int Finish = 3;
        }

        public class DataAdjustmentExcelExport
        {
            public const string No = "No.";
            public const string Plant = "Plant";
            public const string WarehouseLocation = "WH Loc.";
            public const string ComponentCode = "Mã LK";
            public const string AdjustmentQuantity = "Số lượng điều chỉnh";
            public const string DocCode = "Mã phiếu";
            public const string Note = "Note";
            public const string BomXPerBom = "Số lượng thùng x số thùng";
        }

        public class InventoryUserName
        {
            public const string AdministratorUserName = "administrator";
        }
        public class InventoryRoleName
        {
            public const string AdministratorRoleName = "Quyền Admin";
        }
    }
}
