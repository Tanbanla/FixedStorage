var ListUser_DataTable;

$(function () {
    waitForLanguageData()

})

function waitForLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        PreloadMultipleSelect(); // Gọi hàm khởi tạo khi dữ liệu sẵn sàng

        CreateShowOrHideModal()
        CloseAddNewModal()
        ApplyAddNewUser()
        EditShowOrHideModal()
        CloseEditUserModal()
        ApplyEditUser()
        ResetSearch()
        ListUser_RemoveEmptyFirstAndLast()
        ListUsersViewDetail()
        ListUser_CheckInputValidate()
        ListUser_RemoveEmptyFirstAndLast()
        Validation_User()
        CheckToggleLockAccount()
        AddNewShowAndHidePassword()
        UpdateAccountTypeUser()
        UpdateDepartmentUser()
        UpdateRoleUser()
        UpdateStatusUser()
        ShowOrHide_ChangePassword_EditDetailUserModal()
        ShowOrHidePassword_ViewUserDetail()
        CheckInputUserNameValidate()
        CheckInputPasswordValidate()
        RemoveEmptyFirstAndLast()
        Validation_ChangePassword_ViewDetailUserModal()
        HandlePreventChangePassword_ViewUserDetailModal()
        Apply_ChangePassword_ViewUserDetailModal()
        CloseChangePasswordViewDetailUserModal()
        ExportExcelFilteredUserList();
        CheckMaxLengthPassword();
        CheckInputFullNameValidate();
        CheckDateTimeValid();
        ChecMaxTimeValid();
        CheckInputUserCodeValidate();
        CheckInputEditUserNameValidate();
        CheckInputEditFullNameValidate();

        $('#Edit_Users_UserName').on('input', function (event) {
            const inputValue = event.target.value;
            if (inputValue.length > maxLengthUsername) {
                // Nếu chiều dài của giá trị nhập vào lớn hơn maxLengthUsername, loại bỏ ký tự thứ maxLengthUsername + 1 trở đi
                event.target.value = inputValue.slice(0, maxLengthUsername);
            }
        });

        $('#Add_Users_UserName').on('input', function (event) {
            const inputValue = event.target.value;
            const userNameSpan = $('#Username_span').text();
            if (userNameSpan === "Tài khoản đăng nhập đã tồn tại. Vui lòng thử lại.") {
                $('#Username_span').text("")
            }
            if (inputValue.length > maxLengthUsername) {
                // Nếu chiều dài của giá trị nhập vào lớn hơn maxLengthUsername, loại bỏ ký tự thứ maxLengthUsername + 1 trở đi
                event.target.value = inputValue.slice(0, maxLengthUsername);
            }
        });

        $("#Add_Users_FullName").on('input', function () {
            if ($(this).val().length > maxLengthFullName) {
                $(this).val($(this).val().slice(0, maxLengthFullName));
            }
        });

        $("#Edit_Users_FullName").on('input', function () {
            if ($(this).val().length > maxLengthFullName) {
                $(this).val($(this).val().slice(0, maxLengthFullName));
            }
        });

        $('#Add_Users_Code').on('input', function (event) {
            const codeSpan = $('#Code_span').text();
            if (codeSpan === "Mã nhân viên đã tồn tại") {
                $('#Code_span').text("")
            }
            const inputValue = event.target.value;
            if (inputValue.length > maxLengthUserCode) {
                // Nếu chiều dài của giá trị nhập vào lớn hơn 8, loại bỏ ký tự thứ 9 trở đi
                event.target.value = inputValue.slice(0, maxLengthUserCode);
            }
        });

        $('#Edit_Users_Code').on('input', function (event) {
            const codeSpan = $('#Edit_Code_span').text();
            if (codeSpan === "Mã nhân viên đã tồn tại") {
                $('#Edit_Code_span').text("")
            }
            const inputValue = event.target.value;
            if (inputValue.length > maxLengthUserCode) {
                // Nếu chiều dài của giá trị nhập vào lớn hơn 8, loại bỏ ký tự thứ 9 trở đi
                event.target.value = inputValue.slice(0, maxLengthUserCode);
            }
        });

        $('#Add_Users_FullName').blur(function (event) {
            var $fullname = event.target.value;
            var names = $fullname.split(' ').map(str => {
                if (str != "") {
                    return str.toLowerCase().split(' ').map(function (word) {
                        return (word.charAt(0).toUpperCase() + word.slice(1));
                    });
                }
            }).join(' ');

            $('#Add_Users_FullName').val(names);
        });

        $('#Edit_Users_FullName').blur(function (event) {
            var $fullname = event.target.value;
            var names = $fullname.split(' ').map(str => {
                if (str != "") {
                    return str.toLowerCase().split(' ').map(function (word) {
                        return (word.charAt(0).toUpperCase() + word.slice(1));
                    });
                }
            }).join(' ');

            $('#Edit_Users_FullName').val(names);
        });

        //FirstLoad Danh sach nguoi dung:
        FirstLoadUsers()

        //Filter Danh Sach Nguoi Dung:
        FilterUser()

    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForLanguageData, 100); // Chờ thêm 100ms rồi kiểm tra lại
    }
}


// MAX LENGTH PASSWORD
var maxLengthPassword = 15;
var maxLengthFullName = 100;
var maxLengthUserCode = 8;
var maxLengthUsername = 15;

function PreloadMultipleSelect() {
    //Selected all Checkbox:
    $("#Users_Department,#Users_Role,#Users_Status,#Users_AccountType").find("option").attr("selected", true);
    let allText = window.languageData[window.currentLanguage]["Tất cả"];
    let noResultText = window.languageData[window.currentLanguage]["Không có kết quả"];
    let searchText = window.languageData[window.currentLanguage]["Tìm kiếm"];
    let conditionChooseText = window.languageData[window.currentLanguage]["điều kiện đã được chọn"];
    //MutilSelect Khởi tạo:
    VirtualSelect.init({
        ele: '#Users_Department,#Users_Role,#Users_Status,#Users_AccountType',
        selectAllText: allText,
        noOptionsText: noResultText,
        noSearchResultsText: noResultText,
        searchPlaceholderText: searchText,
        allOptionsSelectedText: allText,
        optionsSelectedText: conditionChooseText,
        selectAllOnlyVisible: true,
        hideClearButton: true,
    });

    $("#Users_Department").show();
    $("#Users_Role").show();
    $("#Users_Status").show();
    $("#Users_AccountType").show();
}

function CallApiFilterUsers(departments_List, roles_List, status_List) {
    var link = $("#APIGateway").val();

    if ($.fn.DataTable.isDataTable('#ListUsers_DataTable')) {

        $('#ListUsers_DataTable').DataTable().destroy();
    }

    ListUser_DataTable = $('#ListUsers_DataTable').DataTable({
        "processing": `<span class="spinner"></span>`,
        scrollX: true,
        "serverSide": true,
        select: true,
        "bStateSave": true,
        "fnStateSave": function (oSettings, oData) {
            localStorage.setItem('offersDataTablesListUser', JSON.stringify(oData));
        },
        "fnStateLoad": function (oSettings) {
            return JSON.parse(localStorage.getItem('offersDataTablesListUser'));
        },
        "filter": true,
        "lengthMenu": [10, 30, 50, 200],
        language: {
            lengthMenu: 'Display _MENU_ entries'
        },
        dom: 'rt<"bottom"flp><"clear">',
        "ordering": false,
        "ajax": {
            "url": link + "/api/identity/filter/user",
            "type": "POST",
            "contentType": "application/x-www-form-urlencoded",
            dataType: "json",
            data: function (data) {
                var userName = $("#Users_UserName").val();
                var fullName = $("#Users_FullName").val();
                var code = $("#Users_Code").val();

                //Check selected option Department:
                var checkAllDepartments = document.querySelector('#Users_Department').isAllSelected();
                var allDepartments;
                var departmentIds;
                if (checkAllDepartments) {
                    allDepartments = -1;
                    departmentIds = $('#Users_Department').val();
                }
                else {
                    departmentIds = $('#Users_Department').val();
                }

                //Check selected option Role:
                var checkAllRoles = document.querySelector('#Users_Role').isAllSelected();
                var allRoles;
                var roleIds;
                if (checkAllRoles) {
                    allRoles = -1;
                    roleIds = $('#Users_Role').val();
                }
                else {
                    roleIds = $('#Users_Role').val();
                }

                //Check selected option Status:
                var checkAllStatus = document.querySelector('#Users_Status').isAllSelected();
                var allStatus;
                var statusIds;
                if (checkAllStatus) {
                    allStatus = -1;
                    statusIds = $('#Users_Status').val();
                }
                else {
                    statusIds = $('#Users_Status').val();
                }

                //Check selected option AccountType:
                var checkAllAccountType = document.querySelector('#Users_AccountType').isAllSelected();
                var allAccountType;
                var accountTypes;
                if (checkAllAccountType) {
                    allAccountType = -1;
                    accountTypes = $('#Users_AccountType').val();
                }
                else {
                    accountTypes = $('#Users_AccountType').val();
                }

                var filterData = {
                    UserName: userName,
                    FullName: fullName,
                    Code: code,
                    AllDepartments: allDepartments,
                    DepartmentIds: departmentIds,
                    AllRoles: allRoles,
                    RoleIds: roleIds,
                    AllStatus: allStatus,
                    Status: statusIds,
                    AllAccountType: allAccountType,
                    AccountTypes: accountTypes,
                };

                Object.assign(data, filterData)

                return data
            },
            "dataSrc": function (response) {
                // Cập nhật tổng số bản ghi trong DataTables
                ListUser_DataTable.page.info().recordsTotal = response.totalRecords;
                ListUser_DataTable.page.info().recordsDisplay = response.totalRecords;

                // Trả về dữ liệu để hiển thị trong DataTables, show if response.data > 0 else hide

                if (response.data.length > 0) {
                    $(".btnExportExcel_ListUser").attr("disabled", false)
                    $(".btnExportExcel_ListUser").addClass("export-color-blue")
                    $(".btnExportExcel_ListUser").removeClass("export-color-grey")
                }
                else {
                    $(".btnExportExcel_ListUser").attr("disabled", true)
                    $(".btnExportExcel_ListUser").addClass("export-color-grey")
                    $(".btnExportExcel_ListUser").removeClass("export-color-blue")
                }

                return response.data;
            }
        },
        "drawCallback": function (settings) {
            let totalPages = ListUser_DataTable.page.info().pages;
            let totalRecords = ListUser_DataTable.page.info().recordsTotal;

            let totalText = window.languageData[window.currentLanguage]["Tổng"];
            let currPage = ListUser_DataTable.page() + 1;
            if (currPage == 1) {
                $(".ListUsers_Container").find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $(".ListUsers_Container").find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }
            if (currPage == totalPages) {
                $(".ListUsers_Container").find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $(".ListUsers_Container").find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }

            $(".ListUsers_Container").find(".pagination").before(`<div class="h5 datatable_total_records"> <b>${totalText}</b>: ${totalPages} <span class="total_record_seperate"> | </span>
                </div>`)
        },
        "columns": [
            {
                "data": "",
                "name": "STT",
                "render": function (data, type, row, index) {
                    let pagesize = index.settings._iDisplayLength;
                    let currentRow = ++index.row;
                    let currentPage = ListUser_DataTable.page() + 1;

                    let STT = ((currentPage - 1) * pagesize) + currentRow;

                    if (STT < 10) {
                        STT = `0${STT}`;
                    }
                    return STT;
                },
                "autoWidth": true
            },
            { "data": "userName", "name": "Tài khoản", "autoWidth": true },
            { "data": "fullName", "name": "Họ tên", "autoWidth": true },
            { "data": "code", "name": "Mã nhân viên", "autoWidth": true, },
            {
                "data": "departmentId",
                "name": "Phòng ban",
                "render": function (data, type, row) {
                    const checkDepartmentId = isGuid(data) ? "" : "selected";
                    const selectHtml = `
                        <select class="User_Department form-select form-select-lg" data-id="${row.userId}">
                            <option value="" ${checkDepartmentId}>Chọn phòng ban</option>
                            ${departments_List.map((department, departmentIndex) => {
                        let departmentIsSelected = data.toLowerCase() == department.Id.toLowerCase() ? "selected" : ""
                        return `
                                    <option value="${department.Id}" ${departmentIsSelected}>${department.Name}</option>
                                `
                    }).join("")}
                             
                        </select>
                    `;
                    return selectHtml;
                },
                "autoWidth": false,
                "width": "200px"
            },
            {
                "data": "accountType",
                "name": "Loại tài khoản",
                "render": function (data, type, row) {
                    var userAccType = 'User_Account_Type'
                    if (row.accountType == 1) {
                        userAccType = 'User_Account_Type_Personal'
                    }
                    const selectHtmlAccountType = `
                        <select id="accountTypeSelect" class="${userAccType} form-select form-select-lg" data-id="${row.userId}" onchange="setStyle(event)">
                            <option value="0" ${data == 0 ? 'selected' : ''}>${window.languageData[window.currentLanguage]["Tài khoản riêng"]}</option>
                            <option value="1" ${data == 1 ? 'selected' : ''}>${window.languageData[window.currentLanguage]["Tài khoản chung"]}</option>
                            <option value="2" ${data == 2 ? 'selected' : ''}>${window.languageData[window.currentLanguage]["Tài khoản giám sát"]}</option>
                        </select>
                    `;


                    return selectHtmlAccountType;
                },
                "autoWidth": false,
            },
            {
                "data": "roleId",
                "name": "Nhóm quyền",
                "render": function (data, type, row) {
                    const checkRoleId = isGuid(data) ? "" : "selected";

                    const selectHtmlRole = `
                        <select class="User_Role form-select form-select-lg" data-id="${row.userId}">

                            <option value="" ${checkRoleId}>${window.languageData[window.currentLanguage]["Chọn nhóm quyền"]}</option>

                            ${roles_List.map((role, roleIndex) => {
                        let roleIsSelected = data.toLowerCase() == role.RoleId.toLowerCase() ? "selected" : ""
                        return `
                                        <option value="${role.RoleId.toLowerCase()}" ${roleIsSelected}>${role.Name}</option>
                                    `
                    }).join("")}
                        </select>
                    `;
                    return selectHtmlRole;
                },
                "autoWidth": false,
            },
            {
                "data": "status",
                "name": "Trạng thái",
                "render": function (data, type, row) {
                    let getColor;
                    if (data == 0) {
                        getColor = "color-green";
                    } else if (data == 3) {
                        getColor = "color-grey";
                    } else if (data == 4) {
                        getColor = "color-red";
                    } else {
                        getColor = "color-grey";
                    }

                    const selectHtmlStatus = `
                        <select class="Status_User form-select form-select-lg ${getColor}" data-id="${row.userId}">
                            ${status_List.map((status, statusIndex) => {
                        let statusIsSelected = data == statusIndex ? "selected" : ""
                        let statusDisabled = (statusIndex == 1 || statusIndex == 2) ? "disabled" : ""

                        return `
                                        <option value="${statusIndex}" ${statusIsSelected} ${statusDisabled}>${window.languageData[window.currentLanguage][status]}</option>
                                    `
                    }).join("")}
                        </select>
                    `;
                    return selectHtmlStatus;

                },
                "autoWidth": false,
            },
            {
                "data": "userId",
                "name": "",
                "render": function (data, type, row) {
                    const selectHtmlSpecial = `
                        <div class="Controls_Container">
                            <div class="ViewDetail_Controls mx-3">
                                <a class="ListUsers_ViewDetail_Controls" data-id="${data}">${window.languageData[window.currentLanguage]["Xem chi tiết"]}</a>
                            </div>
                            <div class="EditUser_Controls mx-3" data-id="${data}">
                                <svg width="15" height="14" viewBox="0 0 15 14" fill="none" xmlns="http://www.w3.org/2000/svg">
                                    <path fill-rule="evenodd" clip-rule="evenodd" d="M8.48942 3.19297C9.65025 2.03214 10.7586 2.06131 11.8903 3.19297C12.4736 3.77047 12.7536 4.33047 12.7477 4.90797C12.7477 5.46797 12.4678 6.02214 11.8903 6.59381L11.1902 7.29964C11.1436 7.34631 11.0853 7.36964 11.0211 7.36964C10.9978 7.36964 10.9744 7.36381 10.9511 7.35797C9.40525 6.91464 8.16858 5.67797 7.72525 4.13214C7.70192 4.05047 7.72525 3.95714 7.78358 3.89881L8.48942 3.19297ZM9.41108 7.63214C9.56858 7.72547 9.73192 7.80714 9.90108 7.88881C10.1288 7.98747 10.1947 8.28936 10.0192 8.46483L6.73358 11.7505C6.66358 11.8263 6.51775 11.8963 6.41275 11.9138L4.17275 12.2288C4.10275 12.2405 4.03275 12.2463 3.96275 12.2463C3.64775 12.2463 3.35608 12.1355 3.14608 11.9313C2.90108 11.6805 2.79025 11.3071 2.84858 10.9105L3.16358 8.67631C3.18108 8.57714 3.25108 8.43131 3.32692 8.35547L6.60715 5.07524C6.78619 4.8962 7.08734 4.96436 7.19442 5.19381C7.27608 5.35714 7.35775 5.51464 7.45108 5.67214C7.52692 5.80047 7.60858 5.92881 7.67858 6.01631C7.76029 6.1416 7.85218 6.25162 7.91148 6.32264C7.91569 6.32767 7.91972 6.33251 7.92358 6.33714C7.93184 6.34775 7.93962 6.35789 7.94692 6.3674C7.97573 6.40496 7.99712 6.43283 8.01108 6.44214C8.20358 6.67547 8.41942 6.88547 8.61192 7.04881C8.65858 7.09547 8.69942 7.13047 8.71108 7.13631C8.82192 7.22964 8.93858 7.32297 9.03775 7.38714C9.16025 7.47464 9.28275 7.55631 9.41108 7.63214Z" fill="#87868C"/>
                                </svg>                            
                            </div>
                        </div>
                    `;
                    return selectHtmlSpecial;
                },
                "autoWidth": true
            }

        ],
    });
}

function setStyle(event) {
    var $target = event.target;
    if (event.target.value == 1) {
        $target.style.color = '#5092FC';
       
        //$target.classList.remove('User_Account_Type_Personal');
        //$target.classList.remove('User_Account_Type');
        //$target.classList.add('User_Account_Type');

    } else {
        //$target.classList.remove('User_Account_Type');
        //$target.classList.remove('User_Account_Type_Personal');
        //$target.classList.add('User_Account_Type_Personal');
        $target.style.color = '#ffd800';
    }

}
function FirstLoadUsers() {

   CallApiFilterUsers(departments_List, roles_List, status_List)

}

/*#region Filter User*/
function FilterUser() {
    $(document).delegate("#btn-search", "click", (e) => {
        //CallApiFilterUsers(departments_List, roles_List, status_List)
        ListUser_DataTable.draw();
    })
}

/*#endregion Filter User*/

/*#region Add User*/

//Create Open Modal AddNewUser:
function CreateShowOrHideModal() {
    $("#btnCreate_User").click((e) => {
        $.ajax({
            type: "GET",
            url: "create-user",
            contentType: "application/json", // post && frombody */
            /*dataType: "json",*/
            success: function (res) {
                var depElements = $('#Add_Users_Department');
                let depOptionValues = [...depElements[0].options].map(o => o.value);

                var roleElements = $('#Add_Users_Role');
                let roleOptionValues = [...roleElements[0].options].map(o => o.value);

                var accTypeElements = $('#Add_Users_AccountType');
                let accTypeOptionValues = [...accTypeElements[0].options].map(o => o.value);

                var statusElements = $('#Add_Users_Status');
                let statusOptionValues = [...statusElements[0].options].map(o => o.value);

                if (res.data != null) {
                    if (res.data.departmentList.length > 0) {
                        if (depOptionValues.length > 0) {
                            $("#Add_Users_Department").html('');
                        }
                        $.each(res.data.departmentList, function (i, item) {
                            $("#Add_Users_Department").append($('<option></option>').val(item.id).html(item.name));
                        });
                        const firstDepartment = res.data.departmentList[1];
                        $("#Add_Users_Department").val(firstDepartment.id);
                    }
                    if (res.data.roleList.length > 0) {
                        if (roleOptionValues.length > 0) {
                            $("#Add_Users_Role").html('');
                        }
                        $.each(res.data.roleList, function (i, item) {
                            $("#Add_Users_Role").append($('<option></option>').val(item.id).html(item.name));
                        });
                        const firstRole = res.data.roleList[1];
                        $("#Add_Users_Role").val(firstRole.id);
                    }
                    if (res.data.accountTypeList.length > 0 && accTypeOptionValues.length <=0) {
                        $.each(res.data.accountTypeList, function (i, item) {
                            $("#Add_Users_AccountType").append($('<option></option>').val(item.id).html(window.languageData[window.currentLanguage][item.name]));
                        });
                    }
                    if (res.data.userStatusList.length > 0 && statusOptionValues.length <=0) {
                        $.each(res.data.userStatusList, function (i, item) {
                            $("#Add_Users_Status").append($('<option></option>').val(item.id).html(window.languageData[window.currentLanguage][item.name]));
                        });
                    }
                }
                $("#AddNewUserModal").modal('show');

                $('#Add_Users_NoChangePassword').prop('checked', true);
                $('#Add_Users_NoInteration').prop('checked', true);
            },
            error: function (error) {
                if (error != undefined) {
                    toastr.error(JSON.parse(error.responseText).message);
                }
            }
        });
    })

}

function CloseAddNewModal() {
    $('#button_Delete_AddNewUser').on('click', function () {
        $("#AddNewUserModal").modal("hide");
        ResetCreateUserModal();
    });

    $('#AddNewUserModal .btn-close').on('click', function () {
        $("#AddNewUserModal").modal("hide");
        ResetCreateUserModal();
    });

}

//Reset Create, edit user modal:
function ResetCreateUserModal() {
    $('#Add_Users_UserName').val('');
    $('#Username_span').text('');
    $('#Add_Users_UserName-error').hide();
    $('#Add_Users_Password').val('');
    $('#Password_span').text('');
    $('#Add_Users_Password-error').hide();
    $('#Add_Users_FullName').val('');
    $('#FullName_span').text('');
    $('#Add_Users_FullName-error').hide();
    $('#Add_Users_Code').val('');
    $('#Code_span').text('');
    $('#Add_Users_Code-error').hide();
    $("#Add_Users_Department").html('');
    $('#Department_span').text('');   
    $("#Add_Users_Role").html('');
    $('#Role_span').text('');    
    $("#Add_Users_AccountType").html('');
    $('#AccountType_span').text('');   
    $("#Add_Users_Status").html('');
    $('#Status_span').text('');
    $('#Add_Users_Status-error').hide();
    $('#Add_Users_NoChangePassword').prop('checked', true);
    $('#NoChangePassword_span').text('');
    $('#Add_Users_DateTimeNoChangePassword').val('');
    $('#DateTimeNoChangePassword_span').text('');
    $('#Add_Users_DateTimeNoChangePassword-error').hide();
    $('#Add_Users_NoInteration').prop('checked', true);
    $('#NoInteration_span').text('');
    $('#Add_Users_DateTimeNoInteration').val('');
    $('#DateTimeNoInteration_span').text('');
    $('#Add_Users_DateTimeNoInteration-error').hide();
}

//đồng ý form Thêm mới người dùng:
function ApplyAddNewUser() {
    $(document).delegate("#button_Apply_AddNewUser", "click", (e) => {
        if ($("#AddNewUser_Form").valid()) {
            var username = $("#Add_Users_UserName").val();
            var password = $("#Add_Users_Password").val();
            var fullname = $("#Add_Users_FullName").val();
            var code = $("#Add_Users_Code").val();
            var roleId = $("#Add_Users_Role").val();
            var departmentId = $("#Add_Users_Department").val();
            var accType = $("#Add_Users_AccountType").val();
            var status = $("#Add_Users_Status").val();
            var lockPwdSetting = $("#Add_Users_NoChangePassword").is(":checked");
            var lockPwdTime = $("#Add_Users_DateTimeNoChangePassword").val();
            var lockActSetting = $("#Add_Users_NoInteration").is(":checked");
            var lockActTime = $("#Add_Users_DateTimeNoInteration").val();
            var userId = $("#App_UserId").val() != '' ? $("#App_UserId").val().toUpperCase() : '';
            
            var formData = {
                Username: username,
                Password: password,
                Fullname: fullname,
                Code: code,
                RoleId: roleId,
                DepartmentId: departmentId,
                AccountType: accType,
                Status: status,
                LockPwdSetting: lockPwdSetting,
                LockPwdTime: lockPwdTime,
                LockActSetting: lockActSetting,
                LockActTime: lockActTime,
                UserId: userId
            };
            
            $.ajax({
                type: 'POST',
                url: 'create-user',
                data: formData,
                dataType: "json",
                encode: true,
                success: function (res) {
                    if (res.code !== 200) {
                        toastr.error(res.message)
                        if (res.data.username) {
                            $('#Username_span').text(res.data.username);
                        }
                        if (res.data.code) {
                            $('#Code_span').text(res.data.code);
                        }
                    } else {
                        toastr.success("Thêm mới người dùng thành công.");
                        $("#AddNewUserModal").modal("hide");
                        ResetCreateUserModal();
                        //FirstLoadUsers();
                        ListUser_DataTable.draw();
                    }
                },
                error: function (error) {                    
                    if (error != undefined) {
                        var response = JSON.parse(error.responseText);
                        toastr.error(response.message);
                        if (response.data != null) {
                            if (response.data.username != null) {
                                $('#Username_span').text(response.data.username);
                            }
                            else {
                                $('#Username_span').text('');
                            }
                            if (response.data.code != null) {
                                $('#Code_span').text(response.data.code);
                            }
                            else {
                                $('#Code_span').text('');
                            }
                            if (response.data.accountType != null && response.data.accountType != 0 && response.data.accountType != 1) {
                                $('#AccountType_span').text(response.data.accountType);
                            }
                            else {
                                $('#AccountType_span').text('');
                            }
                            if (response.data.departmentId != null) {
                                $('#Department_span').text(response.data.departmentId);
                            }
                            else {
                                $('#Department_span').text('');
                            }
                            if (response.data.fullName != null) {
                                $('#FullName_span').text(response.data.fullName);
                            }
                            else {
                                $('#FullName_span').text('');
                            }
                           
                            if (response.data.roleId != null) {
                                $('#Role_span').text(response.data.roleId);
                            }
                            else {
                                $('#Role_span').text('');
                            }
                            if (response.data.password != null) {
                                $('#Password_span').text(response.data.password);
                            }
                            else {
                                $('#Password_span').text('');
                            }
                            //if (response.data.lockPwdSetting != null) {
                            //    $('#NoChangePassword_span').text(response.data.lockPwdSetting);
                            //}
                            if (response.data.lockPwdTime != null) {
                                $('#DateTimeNoChangePassword_span').text(response.data.lockPwdTime);
                            }
                            else {
                                $('#DateTimeNoChangePassword_span').text('');
                            }
                            //if (response.data.lockActSetting != null) {
                            //    $('#NoInteration_span').text(response.data.lockActSetting);
                            //}
                            if (response.data.lockActTime != null) {
                                $('#DateTimeNoInteration_span').text(response.data.lockActTime);
                            }
                            else {
                                $('#DateTimeNoInteration_span').text('');
                            }
                        }
                    }
                    //loading(false);

                    //Error code = 20 => Nếu là tài khoản riêng sẽ check mã nhân viên tồn tại trong hệ thống.
                    if (error.responseJSON.code == 20) {
                        toastr.error(error.responseJSON.message)
                    }
                }
            });
        }
    })
}

//đồng ý form sửa người dùng:
function ApplyEditUser() {
    $(document).delegate("#button_Apply_EditNewUser", "click", (e) => {
        if ($("#EditUser_Form").valid()) {
            var id = $("#Edit_Users_UserId").val();
            var username = $("#Edit_Users_UserName").val();
            var fullname = $("#Edit_Users_FullName").val();
            var code = $("#Edit_Users_Code").val();
            var roleId = $("#Edit_Users_Role").val();
            var departmentId = $("#Edit_Users_Department").val();
            var accType = $("#Edit_Users_AccountType").val();
            var status = $("#Edit_Users_Status").val();
            var lockPwdSetting = $("#Edit_Users_NoChangePassword").is(":checked");
            var lockPwdTime = $("#Edit_Users_DateTimeNoChangePassword").val();
            var lockActSetting = $("#Edit_Users_NoInteration").is(":checked");
            var lockActTime = $("#Edit_Users_DateTimeNoInteration").val(); 
            var data = {
                Id: id,
                Username: username,
                Fullname: fullname,
                Code: code,
                RoleId: roleId,
                DepartmentId: departmentId,
                AccountType: accType,
                Status: status,
                LockPwdSetting: lockPwdSetting,
                LockPwdTime: lockPwdTime,
                LockActSetting: lockActSetting,
                LockActTime: lockActTime
            };
            $.ajax({
                type: 'PUT',
                url: 'update-user',
                data: data,
                dataType: "json",
                success: function (res) {
                    toastr.success("Cập nhật người dùng thành công.");
                    $("#EditUserModal").modal("hide");
                    ResetEditUserModal();
                    //FirstLoadUsers();
                    ListUser_DataTable.draw();
                },
                error: function (error) {
                    if (error != undefined) {
                        var response = JSON.parse(error.responseText);

                        if (response.data != null) {
                            if (response.data.id != null) {
                                $('#Edit_Username_span').text(response.data.id);
                            }
                            else {
                                $('#Edit_Username_span').text('');
                            }
                            if (response.data.username != null) {
                                $('#Edit_Username_span').text(response.data.username);
                            }
                            else {
                                $('#Edit_Username_span').text('');
                            }
                            if (response.data.code != null) {
                                $('#Edit_Code_span').text(response.data.code);
                            }
                            else {
                                $('#Edit_Code_span').text('');
                            }
                            if (response.data.accountType != null && response.data.accountType != 0 && response.data.accountType != 1) {
                                $('#Edit_AccountType_span').text(response.data.accountType);
                            }
                            else {
                                $('#Edit_AccountType_span').text('');
                            }
                            if (response.data.departmentId != null) {
                                $('#Edit_Department_span').text(response.data.departmentId);
                            }
                            else {
                                $('#Edit_Department_span').text('');
                            }
                            if (response.data.fullName != null) {
                                $('#Edit_FullName_span').text(response.data.fullName);
                            }
                            else {
                                $('#Edit_FullName_span').text('');
                            }                           
                            if (response.data.roleId != null) {
                                $('#Edit_Role_span').text(response.data.roleId);
                            }
                            else {
                                $('#Edit_Role_span').text('');
                            }
                            //if (response.data.lockPwdSetting != null) {
                            //    $('#Edit_NoChangePassword_span').text(response.data.lockPwdSetting);
                            //}
                            if (response.data.lockPwTime != null) {
                                $('#Edit_DateTimeNoChangePassword_span').text(response.data.lockPwTime);
                            }
                            else {
                                $('#Edit_DateTimeNoChangePassword_span').text('');
                            }
                            //if (response.data.lockActSetting != null) {
                            //    $('#Edit_NoInteration_span').text(response.data.lockActSetting);
                            //}
                            if (response.data.lockActTime != null) {
                                $('#Edit_DateTimeNoInteration_span').text(response.data.lockActTime);
                            }
                            else {
                                $('#Edit_DateTimeNoInteration_span').text('');
                            }
                        }
                    }

                    //Error code = 20 => Nếu là tài khoản riêng sẽ check mã nhân viên tồn tại trong hệ thống.
                    if (error.responseJSON.code == 20) {
                        toastr.error(error.responseJSON.message)
                    }
                }
            });
        }
    })
}

function CloseEditUserModal() {
    $('#button_Delete_EditNewUser').on('click', function () {
        $("#EditUserModal").modal("hide");
        ResetEditUserModal();
    });   
}

function ResetEditUserModal() {
    $('#Edit_Users_UserId').val('');
    $('#Edit_Users_UserName').val('');
    $('#Edit_Username_span').text('');
    $('#Edit_Users_UserName-error').hide();
    $('#Edit_Users_FullName').val('');   
    $('#Edit_FullName_span').text('');
    $('#Edit_Users_FullName-error').hide();
    $('#Edit_Users_Code').val('');
    $('#Edit_Code_span').text('');
    $('#Edit_Users_Code-error').hide();
    $("#Edit_Users_Department").html('');
    $('#Edit_Department_span').text('');   
    $("#Edit_Users_Role").html('');
    $('#Edit_Role_span').text('');   
    $("#Edit_Users_AccountType").html('');
    $('#Edit_AccountType_span').text('');  
    $("#Edit_Users_Status").html('');
    $('#Edit_Status_span').text('');
    $("#Edit_Users_Status-error").text('');    
    $('#Edit_Users_NoChangePassword').prop('checked', true);
    $('#Edit_NoChangePassword_span').text('');    
    $('#Edit_Users_DateTimeNoChangePassword').val('');
    $('#Edit_DateTimeNoChangePassword_span').text('');
    $('#Edit_Users_DateTimeNoChangePassword-error').hide();
    $('#Edit_Users_NoInteration').prop('checked', true);
    $('#Edit_NoInteration_span').text('');   
    $('#Edit_Users_DateTimeNoInteration').val('');
    $('#Edit_DateTimeNoInteration_span').text('');
    $('#Edit_Users_DateTimeNoInteration-error').hide();
}

//Reset Quan Ly Nguoi Dung:
function ResetSearch() {
    $(document).delegate("#btn-reset", "click", (e) => {
        $("#Users_UserName").val("");
        $("#Users_FullName").val("");
        $("#Users_Code").val("");
        //Reload:
        $("#Users_Department")[0].reset()
        $("#Users_Role")[0].reset()
        $("#Users_Status")[0].reset()
        $("#Users_AccountType")[0].reset()

        $("#Users_Department")[0].toggleSelectAll(true);
        $("#Users_Role")[0].toggleSelectAll(true);
        $("#Users_Status")[0].toggleSelectAll(true);
        $("#Users_AccountType")[0].toggleSelectAll(true);

        dataTable.draw();
    })

}

//Function bỏ khoảng trắng ở đầu và cuối:
function ListUser_RemoveEmptyFirstAndLast() {
    // Xử lý khi trường UserName_Login mất focus
    $("#Users_UserName , #Users_FullName , #Users_Code").blur(function () {
        // Loại bỏ khoảng trắng ở hai đầu ký tự
        $(this).val($.trim($(this).val()));
    });
}

//Mở modal chi tiết người dùng:
function ListUsersViewDetail() {
    //$("a.ListUsers_ViewDetail_Controls").click((e) => {
    //    $("#ListUsers_ViewDetailUserModal").modal("show")
    //})

    $(document).delegate("a.ListUsers_ViewDetail_Controls", "click", (e) => {
        $("#ListUsers_ViewDetailUserModal").modal("show")

        e.preventDefault();
        let target = e.target;
        var id = $(target).closest(".ListUsers_ViewDetail_Controls").data("id");
        $.ajax({
            type: "GET",
            url: "get-user-detail",
            data: {
                id: id
            },
            dataType: "json",
            success: function (res) {
                if (res.data != null) {
                    $(".UserName_Content_Top_ListUsersViewDetail").text(res?.data?.username ?? "");
                    $(".FullName_Content_Top_ListUsersViewDetail").text(res?.data?.fullname ?? "");
                    $(".Code_Content_Top_ListUsersViewDetail").text(res?.data?.code ?? "");
                    $(".Department_Content_Top_ListUsersViewDetail").text(res?.data?.departmentName ?? "");
                    $(".AccountType_Content_Bottom_ListUsersViewDetail").text(window.languageData[window.currentLanguage][res?.data?.accountTypeName] ?? "");
                    $(".Role_Content_Bottom_ListUsersViewDetail").text(res?.data?.roleName ?? "");
                    $(".Status_Content_Bottom_ListUsersViewDetail").text(window.languageData[window.currentLanguage][res?.data?.statusName] ?? "");
                    $(".DatetimeChangePassword_Content_Bottom_ListUsersViewDetail").text(res?.data?.lockPwdTime != null ? res?.data?.lockPwdTime + ` ${window.languageData[window.currentLanguage]["Ngày"]}` : `${window.languageData[window.currentLanguage]["Không"]}`);
                    $(".DatetimeNoInteraction_Content_Bottom_ListUsersViewDetail").text(res?.data?.lockActTime != null ? res?.data?.lockActTime + ` ${window.languageData[window.currentLanguage]["Ngày"]}` : `${window.languageData[window.currentLanguage]["Không"]}`);
                    $(".CreatedDate_Content_Bottom_ListUsersViewDetail").text(res?.data?.createdDate ?? "");
                    $(".CreatedBy_Content_Bottom_ListUsersViewDetail").text(res?.data?.createdBy ?? "");
                    $(".UpdatedDate_Content_Bottom_ListUsersViewDetail").text(res?.data?.updatedDate ?? "");
                    $(".UpdatedBy_Content_Bottom_ListUsersViewDetail").text(res?.data?.updatedBy ?? "");
                    $(".LastActiveTime_Content_Bottom_ListUsersViewDetail").text(res?.data?.lastLoginTime ?? "");

                    //Màu loại tài khoản: chung - màu vàng , riêng - màu xanh dương:
                    var getColorAccountType = res?.data?.accountTypeName;
                    if (getColorAccountType == "Tài khoản riêng") {
                        $(".AccountType_Content_Bottom_ListUsersViewDetail").addClass("color-yellow");
                        $(".AccountType_Content_Bottom_ListUsersViewDetail").removeClass("color-blue");
                    }
                    else {
                        $(".AccountType_Content_Bottom_ListUsersViewDetail").addClass("color-blue");
                        $(".AccountType_Content_Bottom_ListUsersViewDetail").removeClass("color-yellow");
                    }

                    //Màu trạng thái: Đang sử dụng – màu xanh lá cây , Bị khóa – màu xám, Đã xóa – màu đỏ:
                    var getcolorStatus = res?.data?.status;
                    if (getcolorStatus == 0) {
                        $(".Status_Content_Bottom_ListUsersViewDetail").addClass("color-green");
                        $(".Status_Content_Bottom_ListUsersViewDetail").removeClass("color-grey");
                        $(".Status_Content_Bottom_ListUsersViewDetail").removeClass("color-red");
                    }
                    else if (getcolorStatus == 1 || getcolorStatus == 2 || getcolorStatus == 3) {
                        $(".Status_Content_Bottom_ListUsersViewDetail").addClass("color-grey");
                        $(".Status_Content_Bottom_ListUsersViewDetail").removeClass("color-green");
                        $(".Status_Content_Bottom_ListUsersViewDetail").removeClass("color-red");
                    }
                    else {
                        $(".Status_Content_Bottom_ListUsersViewDetail").addClass("color-red");
                        $(".Status_Content_Bottom_ListUsersViewDetail").removeClass("color-grey");
                        $(".Status_Content_Bottom_ListUsersViewDetail").removeClass("color-green");
                    }

                }

            },
            error: function (error) {
                $(".UserName_Content_Top_ListUsersViewDetail").text("");
                $(".FullName_Content_Top_ListUsersViewDetail").text("");
                $(".Code_Content_Top_ListUsersViewDetail").text("");
                $(".Department_Content_Top_ListUsersViewDetail").text("");
                $(".AccountType_Content_Bottom_ListUsersViewDetail").text("");
                $(".Role_Content_Bottom_ListUsersViewDetail").text("");
                $(".Status_Content_Bottom_ListUsersViewDetail").text("");
                $(".DatetimeChangePassword_Content_Bottom_ListUsersViewDetail").text("");
                $(".DatetimeNoInteraction_Content_Bottom_ListUsersViewDetail").text("");
                $(".CreatedDate_Content_Bottom_ListUsersViewDetail").text("");
                $(".CreatedBy_Content_Bottom_ListUsersViewDetail").text("");
                $(".UpdatedDate_Content_Bottom_ListUsersViewDetail").text("");
                $(".UpdatedBy_Content_Bottom_ListUsersViewDetail").text("");
                if (error != undefined) {
                    toastr.error(JSON.parse(error.responseText).message);
                }
            }
        });
    })

}

//Function Bắt nhập 1 ký tự chữ và 1 ký tự số:

function ListUser_CheckInputValidate() {
    // Thêm phương thức xác thực tùy chỉnh cho mật khẩu:
    $.validator.addMethod("customPasswordValidate", function (value, element) {        
        // Kiểm tra độ dài từ 8 đến 15 ký tự và chứa ít nhất một ký tự chữ và một ký tự số, không chứa khoảng trắng
        return this.optional(element) || /^(?=.*[a-zA-Z])(?=.*\d)(?!.*\s).{8,15}$/.test(value);        
    }, "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.");
}

//Function bỏ khoảng trắng ở đầu và cuối:
function ListUser_RemoveEmptyFirstAndLast() {
    // Xử lý khi trường UserName_Login mất focus
    $("#Add_Users_UserName , #Add_Users_Password , #Add_Users_FullName, #Add_Users_Code, #Edit_Users_UserName , #Edit_Users_Password , #Edit_Users_FullName, #Edit_Users_Code, #Add_Users_DateTimeNoChangePassword, #Add_Users_DateTimeNoInteration, #Edit_Users_DateTimeNoChangePassword, #Edit_Users_DateTimeNoInteration").blur(function () {
        // Loại bỏ khoảng trắng ở hai đầu ký tự
        $(this).val($.trim($(this).val()));
    });
}

//Ham chan cac ky tu dac biet:
function isValidText(str) {
    return !/[~`!@#$%\^&*()+=\-\[\]\\';.,/{}|\\":<>\?_\s]/g.test(str);
}

//Ham chan cac ky tu dac biet nhưng vẫn cho nhập khoảng trắng để validate tên:
function isValidSpecialText(str) {
    return !/[~`!@#$%\^&*()+=\-\[\]\\';.,/{}|\\":<>\?_]/g.test(str);
}

//Ham chi cho nhap so:
function isValidNumber(str) {
    // Loại bỏ ký tự có dấu tiếng Việt
    str = str.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
    return !/[~`!@#$%\^&*()+=\-\[\]\\';.,/{}|\\":<>\?_a-zA-Z ]/g.test(str);
}

//Function Validate Xem chi tiết người dùng - Thay đổi mật khẩu:
function Validation_User() {

    // Thêm một quy tắc kiểm tra tùy chỉnh cho số 0 ở đầu
    $.validator.addMethod("notZeroAtStartDatetime", function (value, element) {
        return !/^0+$/.test(value);
    }, "Vui lòng nhập thời gian hiệu lực.");

    $.validator.addMethod("invalidCharacter", function (value, element) {
        
        return !/[~`!@#$%\^&*()+=\-\[\]\\';.,/{}|\\":<>\?_]/g.test(value);
    }, "Tài khoản đăng nhập không đúng định dạng. Vui lòng thử lại.");

    //Them moi nguoi dung:
    $("#AddNewUser_Form").validate({
        rules: {
            Add_Users_UserName: {
                required: true,
                minlength: 1,
                maxlength: 15,
                customUserNameValidate: true
            },
            Add_Users_Password: {
                required: true,
                minlength: 8,
                maxlength: maxLengthPassword,
                customPasswordValidate: true,
            },
            Add_Users_FullName: {
                required: true,
                minlength: 1,
                maxlength: maxLengthFullName,
                customFullNameValidate: true
            },
            Add_Users_Code: {
                required: true,
                minlength: 1,
                maxlength: maxLengthUserCode,
                customUserCodeValidate: true
            },
            Add_Users_DateTimeNoChangePassword: {
                required: true,
                minlength: 1,
                notZeroAtStartDatetime: true,
                customDateTimeValidate: true,
                maxTime: true
            },
            Add_Users_DateTimeNoInteration: {
                required: true,
                minlength: 1,
                notZeroAtStartDatetime: true,
                customDateTimeValidate: true,
                maxTime:true
            }
        },
        messages: {
            Add_Users_UserName: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập tài khoản đăng nhập."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 15 ký tự."],
                customUserNameValidate: window.languageData[window.currentLanguage]["Tài khoản đăng nhập không đúng định dạng. Vui lòng thử lại."]
            },
            Add_Users_Password: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập mật khẩu."],
                minlength: window.languageData[window.currentLanguage]["Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng."],
                maxlength: window.languageData[window.currentLanguage]["Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng."],
                customPasswordValidate: window.languageData[window.currentLanguage]["Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng."]
            },
            Add_Users_FullName: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập họ và tên."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 100 ký tự."],
                customFullNameValidate: window.languageData[window.currentLanguage]["Họ và tên không đúng định dạng. Vui lòng thử lại."]
            },
            Add_Users_Code: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập mã nhân viên."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 8 ký tự."],
                customUserCodeValidate: window.languageData[window.currentLanguage]["Mã nhân viên không đúng định dạng. Vui lòng thử lại."]
            },
            Add_Users_DateTimeNoChangePassword: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập thời gian hiệu lực."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                notZeroAtStartDatetime: window.languageData[window.currentLanguage]["Vui lòng nhập thời gian hiệu lực."],
                customDateTimeValidate: window.languageData[window.currentLanguage]["Thời gian hiệu lực không đúng định dạng. Vui lòng thử lại."],
                maxTime: window.languageData[window.currentLanguage]["Vui lòng nhập không quá 90 ngày."]
            },
            Add_Users_DateTimeNoInteration: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập thời gian hiệu lực."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                notZeroAtStartDatetime: window.languageData[window.currentLanguage]["Vui lòng nhập thời gian hiệu lực."],
                customDateTimeValidate: window.languageData[window.currentLanguage]["Thời gian hiệu lực không đúng định dạng. Vui lòng thử lại."],
                maxTime: window.languageData[window.currentLanguage]["Vui lòng nhập không quá 90 ngày."]
            }

        }
    });

    //Sua nguoi dung:
    $("#EditUser_Form").validate({
        rules: {
            Edit_Users_UserName: {
                required: true,
                minlength: 1,
                maxlength: maxLengthUsername,
                customUserNameValidate: true,
            },
            Edit_Users_Password: {
                required: true,
                minlength: 8,
                maxlength: maxLengthPassword,
                customPasswordValidate: true,
            },
            Edit_Users_FullName: {
                required: true,
                minlength: 1,
                maxlength: 100,
                customEditFullNameValidate: true,
            },
            Edit_Users_Code: {
                required: true,
                minlength: 1,
                maxlength: maxLengthUserCode,
                customUserCodeValidate: true
            },
            Edit_Users_DateTimeNoChangePassword: {
                required: true,
                minlength: 1,
                notZeroAtStartDatetime: true,
                customDateTimeValidate: true,
                maxTime: true

            },
            Edit_Users_DateTimeNoInteration: {
                required: true,
                minlength: 1,
                notZeroAtStartDatetime: true,
                customDateTimeValidate: true,
                maxTime: true
            }
        },
        messages: {
            Edit_Users_UserName: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập tài khoản đăng nhập."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 15 ký tự."],
                customUserNameValidate: window.languageData[window.currentLanguage]["Tài khoản đăng nhập không đúng định dạng. Vui lòng thử lại."]
            },
            Edit_Users_Password: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập mật khẩu."],
                minlength: window.languageData[window.currentLanguage]["Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng."],
                maxlength: window.languageData[window.currentLanguage]["Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng."],
                customPasswordValidate: window.languageData[window.currentLanguage]["Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng."]
            },
            Edit_Users_FullName: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập họ và tên."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 100 ký tự."],
                customFullNameValidate: window.languageData[window.currentLanguage]["Họ và tên không đúng định dạng. Vui lòng thử lại."]
            },
            Edit_Users_Code: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập mã nhân viên."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 8 ký tự."],
                customUserCodeValidate: window.languageData[window.currentLanguage]["Mã nhân viên không đúng định dạng. Vui lòng thử lại."]
            },
            Edit_Users_DateTimeNoChangePassword: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập thời gian hiệu lực."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                notZeroAtStartDatetime: window.languageData[window.currentLanguage]["Vui lòng nhập thời gian hiệu lực."],
                customDateTimeValidate: window.languageData[window.currentLanguage]["Thời gian hiệu lực không đúng định dạng. Vui lòng thử lại."],
                maxTime: window.languageData[window.currentLanguage]["Vui lòng nhập không quá 90 ngày."]
            },
            Edit_Users_DateTimeNoInteration: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập thời gian hiệu lực."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                notZeroAtStartDatetime: window.languageData[window.currentLanguage]["Vui lòng nhập thời gian hiệu lực."],
                customDateTimeValidate: window.languageData[window.currentLanguage]["Thời gian hiệu lực không đúng định dạng. Vui lòng thử lại."],
                maxTime: window.languageData[window.currentLanguage]["Vui lòng nhập không quá 90 ngày."]
            }

        }
    });

}

//Hàm check toggle Khóa tài khoản theo điều kiện:
function CheckToggleLockAccount() {
    
    //Check cho Them moi nguoi dung:
    $("#Add_Users_NoChangePassword").change((e) => {
        if($("#Add_Users_NoChangePassword").is(":checked")) {
            $("#Add_Users_DateTimeNoChangePassword").attr("disabled", false)
        } else {
            $("#Add_Users_DateTimeNoChangePassword").attr("disabled", true)
            $("#Add_Users_DateTimeNoChangePassword").val("")
        }
    })

    $("#Add_Users_NoInteration").change(() => {
        $("#Add_Users_NoInteration").is(":checked") ? $("#Add_Users_DateTimeNoInteration").attr("disabled", false) : $("#Add_Users_DateTimeNoInteration").attr("disabled", true)
        if($("#Add_Users_NoInteration").is(":checked")) {
            $("#Add_Users_DateTimeNoInteration").attr("disabled", false)
        } else {
            $("#Add_Users_DateTimeNoInteration").attr("disabled", true)
            $("#Add_Users_DateTimeNoInteration").val("")
        }
    })

    //Check cho Sua nguoi dung:
    $("#Edit_Users_NoChangePassword").change((e) => {
        const checked = $("#Edit_Users_NoChangePassword").is(":checked");
        if(checked) {
            $("#Edit_Users_DateTimeNoChangePassword").attr("disabled", false)
            $("#Edit_Users_NoChangePassword").val("on")
        } else {
            $("#Edit_Users_DateTimeNoChangePassword").attr("disabled", true)
            $("#Edit_Users_NoChangePassword").val("off")
            $("#Edit_Users_DateTimeNoChangePassword").val("")
        }
    })

    $("#Edit_Users_NoInteration").change((e) => {
        const checked = $("#Edit_Users_NoInteration").is(":checked");
        if(checked) {
            $("#Edit_Users_DateTimeNoInteration").attr("disabled", false)
            $("#Edit_Users_NoInteration").val("on");
        } else {
            $("#Edit_Users_DateTimeNoInteration").attr("disabled", true);
            $("#Edit_Users_NoInteration").val("off");
            $("#Edit_Users_DateTimeNoInteration").val("")
        }
    })

}

//Modal Thêm mới hiển thị hoặc tắt xem mật khẩu:
function AddNewShowAndHidePassword() {
    //Show Password:
    $("#Show_AddNew_ChangePassword").click(() => {
        $("#Hide_AddNew_ChangePassword").show();
        $("#Show_AddNew_ChangePassword").hide();
        $("#Add_Users_Password").attr("type", "text");
    })

    //Hide Password:
    $("#Hide_AddNew_ChangePassword").click(() => {
        $("#Show_AddNew_ChangePassword").show();
        $("#Hide_AddNew_ChangePassword").hide();
        $("#Add_Users_Password").attr("type", "password");
    })
}

//Cập nhật loại tài khoản người dùng:
function UpdateAccountTypeUser() {
    $(document).delegate("#accountTypeSelect", "change", (e) => {
        e.preventDefault();

        var link = $("#APIGateway").val();

        let target = e.target;
        var userId = $(target).data("id");
        var accountType = $(target).val();
        
        var updateBy = App.User.UserId;

        if (accountType == 0) {
            $(target).addClass("color-yellow")
            $(target).removeClass("color-blue")
        } else if (accountType == 1) {
            $(target).addClass("color-blue")
            $(target).removeClass("color-yellow")
        }

        $.ajax({
            type: "PUT",
            url: link + `/api/identity/account-type/${userId}/${accountType}/${updateBy}`,
            success: function (res) {
                if (res.code == 200) {
                    toastr.success(res.message);
                    dataTable.draw();
                }
            },
            error: function (error) {
                toastr.error(res.message)
            }
        });
    })
}
//Cập nhật phòng ban người dùng:
function UpdateDepartmentUser() {
    $(document).delegate(".User_Department", "change", (e) => {
        e.preventDefault();

        var link = $("#APIGateway").val();

        let target = e.target;
        var userId = $(target).data("id");
        var departmentId = $(target).val();
        var updatedBy = App.User.UserId;
        if (departmentId.length > 0) {
            $.ajax({
                type: "PUT",
                url: link + `/api/identity/department/assign/user/${userId}/${departmentId}/${updatedBy}`,
                success: function (res) {
                    if (res.code == 200) {
                        toastr.success(res.message)
                    }

                },
                error: function (error) {
                    toastr.error(res.message)
                }
            });
        }

    })
}

//Cập nhật nhóm quyền người dùng:
function UpdateRoleUser() {
    $(document).delegate(".User_Role", "change", (e) => {
        e.preventDefault();

        var link = $("#APIGateway").val();

        let target = e.target;
        var userId = $(target).data("id");
        var roleId = $(target).val();
        var updatedBy = App.User.UserId;

        if (roleId.length > 0) {
            $.ajax({
                type: "PUT",
                url: link + `/api/identity/role/assign/${userId}/${roleId}/${updatedBy}`,
                success: function (res) {
                    if (res.code == 200) {
                        toastr.success(res.message)
                    }

                    AppUser.refreshUserInfo();
                },
                error: function (error) {
                    toastr.error(error)
                }
            });
        }

    })
}
//Cập nhật trạng thái người dùng:
function UpdateStatusUser() {
    $(document).delegate(".Status_User", "change", (e) => {
        e.preventDefault();
        
        var link = $("#APIGateway").val();

        let target = e.target;
        var userId = $(target).data("id");
        var status = $(target).val();
        var updateBy = App.User.UserId;
        if (status == 1 || status == 2) {
            toastr.error("Không thể cập nhật trạng thái này.")
        }
        else {
            if (status == 0) {
                $(target).addClass("color-green");
                $(target).removeClass("color-red");
                $(target).removeClass("color-grey");

            }
            else if (status == 3) {
                $(target).addClass("color-grey");
                $(target).removeClass("color-red");
                $(target).removeClass("color-green");
            }
            else if (status == 4) {
                $(target).addClass("color-red");
                $(target).removeClass("color-grey");
                $(target).removeClass("color-green");
            }

            $.ajax({
                type: "PUT",
                url: link + `/api/identity/change/status/${userId}/${status}/${updateBy}`,
                success: function (res) {
                    if (res.code == 200) {
                        toastr.success(res.message)
                    }
                    if (res.code == 400) {
                        toastr.error(res.message)
                    }
                },
                error: function (error) {
                    toastr.error(res.message)
                }
            });
        }

    })
}

/*#endregion Add User*/



/*#region Edit User*/

//Edit Open Modal EditUser:
function EditShowOrHideModal() {
    //$(".EditUser_Controls").click((e) => {
    //    $("#EditUserModal").modal("show")
    //})

    $(document).delegate(".EditUser_Controls", "click", async (e) => {
        e.preventDefault();
        let target = e.target;
        var id = $(target).closest(".EditUser_Controls").data("id");
        $('#ChangePassword_ViewDetailUserModal').attr('data-id', id);
        ResetEditUserModal();

        //Ẩn hiển nút reset password
        let isAdmin = await AppUser.getUser().isInRole("Quyền Admin");
        let changePassView = $(".user_detail_changepass");

        //Ẩn hiện nút đổi mật khẩu theo quyền
        isAdmin ? changePassView.show() : changePassView.hide();

        $.ajax({
            type: "GET",
            url: "get-user-detail",
            data: {
                id: id
            },
            dataType: "json",
            success: function (res) {

                if (res.data != null) {
                    $('#editUserId').val(res.data.id);
                    $('#Edit_Users_UserId').val(res.data.id);
                    $('#Edit_Users_UserName').val(res.data.username);
                    $('#Edit_Users_FullName').val(res.data.fullname);
                    $('#Edit_Users_Code').val(res.data.code);

                    if (res.data.departmentList.length > 0) {
                        $.each(res.data.departmentList, function (i, item) {
                            if (item.id == res.data.departmentId) {
                                $("#Edit_Users_Department").append($('<option selected=\"selected\"></option>').val(item.id).html(item.name));
                            }
                            else {
                                $("#Edit_Users_Department").append($('<option></option>').val(item.id).html(item.name));
                            }

                        });
                    }
                    //$("#Edit_Users_Department").val(res.data.departmentId);

                    if (res.data.roleList.length > 0) {
                        $.each(res.data.roleList, function (i, item) {
                            $("#Edit_Users_Role").append($('<option></option>').val(item.id).html(item.name));
                        });
                    }
                    $("#Edit_Users_Role").val(res.data.roleId);

                    if (res.data.accountTypeList.length > 0) {
                        $.each(res.data.accountTypeList, function (i, item) {
                            $("#Edit_Users_AccountType").append($('<option></option>').val(item.id).html(window.languageData[window.currentLanguage][item.name]));
                        });
                    }
                    $("#Edit_Users_AccountType").val(res.data.accountType);

                    if (res.data.userStatusList.length > 0) {
                        $.each(res.data.userStatusList, function (i, item) {
                            $("#Edit_Users_Status").append($('<option></option>').val(item.id).html(window.languageData[window.currentLanguage][item.name]));
                        });
                    }
                    $("#Edit_Users_Status").val(res.data.status);

                    $('#Edit_Users_NoChangePassword').prop('checked', res.data.lockPwdSetting);
                    $("#Edit_Users_NoChangePassword").val(res.data.lockPwdSetting ? "on" : "off");
                    $("#Edit_Users_DateTimeNoChangePassword").attr("disabled", !(res.data.lockPwdSetting));
                    $('#Edit_Users_NoInteration').prop('checked', res.data.lockActSetting);
                    $("#Edit_Users_NoInteration").val(res.data.lockPwdSetting ? "on" : "off");
                    $("#Edit_Users_DateTimeNoInteration").attr("disabled", !(res.data.lockActSetting));
                    
                    if (res.data.lockPwdSetting == true && res.data.lockPwdTime > 0) {
                        $("#Edit_Users_DateTimeNoChangePassword").val(res.data.lockPwdTime);
                    }

                    if (res.data.lockActSetting == true && res.data.lockActTime > 0) {
                        $("#Edit_Users_DateTimeNoInteration").val(res.data.lockActTime);
                    }
                }
                $("#EditUserModal").modal('show');
            },
            error: function (error) {
                if (error != undefined) {
                    toastr.error(JSON.parse(error.responseText).message);
                }
            }
        });
    })

}

//Nhấn nút thay đổi mật khẩu ở modal chỉnh sửa người dùng: 
//Open Modal ChangePassword_ViewDetailUserModal:
function ShowOrHide_ChangePassword_EditDetailUserModal() {
    $(document).delegate("a.Edit_Users_ChangePassword", "click", (e) => {                
        $("#EditUserModal").modal("hide")
        $.ajax({
            type: "GET",
            url: "reset-password",
            contentType: "application/json",
            success: function () {
                $("#ChangePassword_ViewDetailUserModal").modal("show")
            },
            error: function (error) {
                if (error != undefined) {
                    var message = JSON.parse(error.responseText).message;
                    Swal.fire({
                        title: 'Thông báo',
                        text: message,
                        confirmButtonText: 'Đã hiểu'
                    })
                    // toastr.error(JSON.parse(error.responseText).message);
                }
            }
        });
    })
}

//Ẩn hiện Password:
function ShowOrHidePassword_ViewUserDetail() {    
    ////Xem chi tiết người dùng - Thay đổi mật khẩu - MK mới:
    //Show Password:
    $("#NewPassword_ChangePassword_ViewDetailUser_Show").click(() => {
        $("#NewPassword_ChangePassword_ViewDetailUser_Hide").show();
        $("#NewPassword_ChangePassword_ViewDetailUser_Show").hide();
        $("#NewPassword_ChangePassword_ViewDetailUser").attr("type", "text");
    })

    //Hide Password:
    $("#NewPassword_ChangePassword_ViewDetailUser_Hide").click(() => {
        $("#NewPassword_ChangePassword_ViewDetailUser_Show").show();
        $("#NewPassword_ChangePassword_ViewDetailUser_Hide").hide();
        $("#NewPassword_ChangePassword_ViewDetailUser").attr("type", "password");
    })    
}
// Function báo lỗi khi nhập kí tự đặc biệt vào tài khoản đăng nhập
function CheckInputUserNameValidate() {
    // Thêm phương thức xác thực tùy chỉnh cho tài khoản đăng nhập:
    $.validator.addMethod("customUserNameValidate", function (value, element) {
        return this.optional(element) || !/\s/.test(value) && /^[a-zA-Z0-9-_\s]+$/.test(value);
    }, "Tài khoản đăng nhập không đúng định dạng. Vui lòng thử lại.");
}

//Function Bắt nhập 1 ký tự chữ và 1 ký tự số:

function CheckInputPasswordValidate() {
    // Thêm phương thức xác thực tùy chỉnh cho mật khẩu:
    $.validator.addMethod("customPasswordValidate", function (value, element) {
        // Kiểm tra độ dài từ 8 đến 15 ký tự và chứa ít nhất một ký tự chữ và một ký tự số, không chứa khoảng trắng
        return this.optional(element) || /^(?=.*[a-zA-Z])(?=.*\d)(?!.*\s).{8,15}$/.test(value);
    }, "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.");
}

//Function bỏ khoảng trắng ở đầu và cuối:
function RemoveEmptyFirstAndLast() {
    // Xử lý khi trường UserName_Login mất focus
    $("#OldPassword_ChangePassword_ViewDetailUser , #NewPassword_ChangePassword_ViewDetailUser , #ConfirmNewPassword_ChangePassword_ViewDetailUser").blur(function () {
        // Loại bỏ khoảng trắng ở hai đầu ký tự
        $(this).val($.trim($(this).val()));
    });
}

//Function Validate Xem chi tiết người dùng - Thay đổi mật khẩu:
function Validation_ChangePassword_ViewDetailUserModal() {
    $("#ChangePassword_ViewDetailUser_Form").validate({
        rules: {           
            NewPassword_ChangePassword_ViewDetailUser: {
                required: true,
                minlength: 8,
                maxlength: 15,
                customPasswordValidate: true,
            },           
        },
        messages: {           
            NewPassword_ChangePassword_ViewDetailUser: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập mật khẩu mới."],
                minlength: window.languageData[window.currentLanguage]["Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng."],
                maxlength: window.languageData[window.currentLanguage]["Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng."],
                customPasswordValidate: window.languageData[window.currentLanguage]["Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng."]
            },           
        }
    });
}

//Function Chặn submit Form: Xem chi tiết người dùng - Thay đổi mật khẩu
function HandlePreventChangePassword_ViewUserDetailModal() {
    $("#ChangePassword_ViewDetailUser_Form").submit((e) => {
        e.preventDefault()
    })
}

//Function Xem chi tiết người dùng - Thay đổi mật khẩu - Xác nhận:
function Apply_ChangePassword_ViewUserDetailModal() {
    $(document).delegate("#btnChangePassword_ViewDetailUser", "click", (e) => {        
        if ($("#ChangePassword_ViewDetailUser_Form").valid()) {
            var userId = $("#ChangePassword_ViewDetailUserModal").data("id");
            var newPassword = $("#NewPassword_ChangePassword_ViewDetailUser").val();        
            var formData = {
                UserId: userId,             
                NewPassword: newPassword,
            }; 
            $.ajax({
                type: 'POST',
                url: 'reset-password',
                data: formData,
                dataType: "json",
                success: function () {                    
                    if (App.User.UserId == userId) {                        
                        Requester.RemoveToken();

                        Swal.fire({
                            title: 'Cập nhật dữ liệu người dùng',
                            text: `Vui lòng đăng nhập lại để cập nhật mật khẩu mới.`,
                            confirmButtonText: 'Đã hiểu'
                        }).then(() => {
                            window.location.href = `login`;
                        })
                    }
                    else {                        
                        toastr.success('Đổi mật khẩu thành công.');
                        $("#ChangePassword_ViewDetailUserModal").modal("hide");
                        Reset_ChangePassword_EditUser();
                    }                                                                          
                },
                error: function (error) {
                    if (error != undefined) {
                        var response = JSON.parse(error.responseText);
                        if (response.code == 400) {
                            response.message = 'Dữ liệu không hợp lệ.';
                        }
                        toastr.error(response.message);
                        if (response.data != null) {                          
                            if (response.data.newPassword != null) {
                                $('#NewPassword_ChangePassword_ViewDetailUser_span').text(response.data.newPassword);
                            }                            
                        }
                    }
                }
            });
        }
    })
}

function CloseChangePasswordViewDetailUserModal() {
    $('#ChangePassword_ViewDetailUserModal .btn-close').on('click', function () {
        $("#ChangePassword_ViewDetailUserModal").modal("hide");
        Reset_ChangePassword_EditUser();
    });
}
function Reset_ChangePassword_EditUser() {
    $("#NewPassword_ChangePassword_ViewDetailUser").val('');    
    $('#NewPassword_ChangePassword_ViewDetailUser_span').text('');  
}

/*#endregion Edit User*/

function ExportExcelFilteredUserList() {
    $(document).delegate(".btnExportExcel_ListUser", "click", (e) => {
        var userName = $("#Users_UserName").val();
        var fullName = $("#Users_FullName").val();
        var code = $("#Users_Code").val();

        //Check selected option Department:
        var checkAllDepartments = document.querySelector('#Users_Department').isAllSelected();
        var allDepartments;
        var departmentIds;
        if (checkAllDepartments) {
            allDepartments = -1;
            departmentIds = $('#Users_Department').val();
        }
        else {
            departmentIds = $('#Users_Department').val();
        }

        //Check selected option Role:
        var checkAllRoles = document.querySelector('#Users_Role').isAllSelected();
        var allRoles;
        var roleIds;
        if (checkAllRoles) {
            allRoles = -1;
            roleIds = $('#Users_Role').val();
        }
        else {
            roleIds = $('#Users_Role').val();
        }

        //Check selected option Status:
        var checkAllStatus = document.querySelector('#Users_Status').isAllSelected();
        var allStatus;
        var statusIds;
        if (checkAllStatus) {
            allStatus = -1;
            statusIds = $('#Users_Status').val();
        }
        else {
            statusIds = $('#Users_Status').val();
        }

        //Check selected option AccountType:
        var checkAllAccountType = document.querySelector('#Users_AccountType').isAllSelected();
        var allAccountType;
        var accountTypes;
        if (checkAllAccountType) {
            allAccountType = -1;
            accountTypes = $('#Users_AccountType').val();
        }
        else {
            accountTypes = $('#Users_AccountType').val();
        }
        filterData = {
            Username: userName,
            Fullname: fullName,
            Code: code,
            AllDepartments: allDepartments,
            DepartmentIds: departmentIds,
            AllRoles: allRoles,
            RoleIds: roleIds,
            AllStatus: allStatus,
            Status: statusIds,
            AllAccountType: allAccountType,
            AccountTypes: accountTypes
        };

        var url = 'export-user-list';
        $.ajax({
            type: 'POST',
            url: url,
            data: filterData,
            cache: false,
            xhrFields: {
                responseType: 'blob'
            },
            success: function (response) {
                if (response) {
                    var blob = new Blob([response], { type: response.type });
                    const fileURL = URL.createObjectURL(blob);
                    const link = document.createElement('a');
                    link.href = fileURL;
                    link.download = 'DanhSachNguoiDung.xlsx';
                    link.click();
                } else {
                    toastr.error("Không tìm thấy file.");
                }
                toastr.success("Export danh sách người dùng thành công.");
            },
            error: function (error) {
                if (error != undefined) {
                    toastr.error(response.message);
                }
            }
        });
    })
}

// chặn nhập nếu Add_Users_Password dài quá maxLength
function CheckMaxLengthPassword() {
    $("#Add_Users_Password").on('input', function () {
        if ($(this).val().length > maxLengthPassword) {
            $(this).val($(this).val().slice(0, maxLengthPassword));
        }
    });
};

// kiểm tra Add_Users_FullName có chứa ký tự đặc biệt
function CheckInputFullNameValidate() {
    $.validator.addMethod("customFullNameValidate", function (value, element) {
        return this.optional(element) || isValidSpecialText(value);
    }, "Họ và tên không đúng định dạng. Vui lòng thử lại.");
}

// chặn nhập chữ ở Add_Users_DateTimeNoChangePassword và Add_Users_DateTimeNoInteration
function CheckDateTimeValid() {
    $.validator.addMethod("customDateTimeValidate", function (value, element) {
        return this.optional(element) || isValidNumber(value);
    }, "Thời gian hiệu lực không đúng định dạng. Vui lòng thử lại.");
};
function ChecMaxTimeValid() {
    $.validator.addMethod("maxTime", function (value, element) {

        return this.optional(element) || (isValidNumber(value) && value <=90);
    }, "Vui lòng nhập không quá 90 ngày.");
};
// kiểm tra Add_Users_Code có chứa ký tự đặc biệt
function CheckInputUserCodeValidate() {
    $.validator.addMethod("customUserCodeValidate", function (value, element) {
        return this.optional(element) || isValidText(value);
    }, "Mã nhân viên không đúng định dạng. Vui lòng thử lại.");
}

// kiểm tra Edit_Users_Name có chứa ký tự đặc biệt
function CheckInputEditUserNameValidate() {
    $.validator.addMethod("customEditUserNameValidate", function (value, element) {
        return this.optional(element) || isValidText(value);
    }, "Tài khoản đăng nhập không đúng định dạng. Vui lòng thử lại.");
}

// kiểm tra Edit_Users_FullName có chứa ký tự đặc biệt
function CheckInputEditFullNameValidate() {
    $.validator.addMethod("customEditFullNameValidate", function (value, element) {
        return this.optional(element) || isValidSpecialText(value);
    }, "Họ và tên không đúng định dạng. Vui lòng thử lại.");
}