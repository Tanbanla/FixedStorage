var host = $("#APIGateway").val();
let AuditDatatable;

//KO view model wrap all page
var auditTargetViewModel = new AuditTargetView();
$(function () {
    waitForAuditTargetLanguageData();
});
function waitForAuditTargetLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        //Validate Form Search:
        ValidateFormSearchListAudit()

        ExportFileAuditTarget();


        //Click nut tao phieu giam sat:
        $(document).delegate("#monitoring-create", "click", (e) => {
            var dropdownSelectors = [
                `#monitoring_list-department`,
                `#monitoring_list-area`,
                `#monitoring_list-status`
            ]
            dropdownSelectors.map(selctor => {
                if ($(selctor).find("option").length > 1) {
                    VirtualSelect.init({
                        ele: selctor,
                        selectAllText: window.languageData[window.currentLanguage]['Tất cả'],
                        multiple: true,
                        alwaysShowSelectedOptionsCount: false,
                        alwaysShowSelectedOptionsLabel: false,
                        disableAllOptionsSelectedText: false,
                        selectAllOnlyVisible: false,
                        noOptionsText: window.languageData[window.currentLanguage]['Không có kết quả'],
                        noSearchResultsText: window.languageData[window.currentLanguage]['Không có kết quả'],
                        searchPlaceholderText: window.languageData[window.currentLanguage]['Tìm kiếm'],
                        allOptionsSelectedText: window.languageData[window.currentLanguage]['Tất cả'],
                        optionsSelectedText: window.languageData[window.currentLanguage]["điều kiện đã được chọn"],
                        optionSelectedText: window.languageData[window.currentLanguage]["điều kiện đã được chọn"],
                        hideClearButton: true,
                        autoSelectFirstOption: true,
                    });
                } else {
                    VirtualSelect.init({
                        ele: selctor,
                        selectAllText: window.languageData[window.currentLanguage]['Tất cả'],
                        multiple: true,
                        alwaysShowSelectedOptionsCount: false,
                        alwaysShowSelectedOptionsLabel: true,
                        disableAllOptionsSelectedText: true,
                        noOptionsText: window.languageData[window.currentLanguage]['Không có kết quả'],
                        noSearchResultsText: window.languageData[window.currentLanguage]['Không có kết quả'],
                        searchPlaceholderText: window.languageData[window.currentLanguage]['Tìm kiếm'],
                        allOptionsSelectedText: window.languageData[window.currentLanguage]['Tất cả'],
                        optionsSelectedText: window.languageData[window.currentLanguage]["điều kiện đã được chọn"],
                        optionSelectedText: window.languageData[window.currentLanguage]["điều kiện đã được chọn"],
                        selectAllOnlyVisible: false,
                        hideClearButton: true,
                    });
                }
            })

            $("#monitoring_list-status")[0].reset();
            $("#monitoring_list-status")[0].toggleSelectAll(true);

            $("#monitoring_list-department")[0].reset();
            $("#monitoring_list-department")[0].toggleSelectAll(true);

            $("#monitoring_list-area")[0].reset();
            $("#monitoring_list-area")[0].toggleSelectAll(true);

            InitListAuditDatatable()
            //chuyển tab thì refresh lại:
            $("#btn-reset-create-monitoring").trigger("click");


            //Kiểm tra trạng thái hoàn thành đợt kiểm kê
            let isInventoryCompleted = $("#InventoryDetail_Status").val() == "3";
            if (isInventoryCompleted) {
                //Publish sự kiện đợt kiểm kê hoàn thành
                $(window).trigger("inventory_completed");
            }
            //Phân quyền màn danh sách giám sát:
            var getInventoryStatus = $("#inventory-wrapper").attr("data-status");
            let getAccountType = App.User.AccountType;
            let getInventoryRoleType = App.User.InventoryLoggedInfo.InventoryRoleType;
            let getInventoryDate = $("#inventory-wrapper").attr("data-inventory-date");
            let currentDate = moment().format("YYYY-MM-DD");
            //TH1: Trang thái phiếu đã hoàn thành:
            if (getInventoryStatus === '3') {
                if (getAccountType === "TaiKhoanChung" && getInventoryRoleType === 2) {
                    $(".btnImportAuditTargets").hide();
                    $("#export-file-monitoring-list").show();
                    $("#download-form-monitoring-list").show();
                } else if (getAccountType === "TaiKhoanRieng") {
                    //Chỉ có quyền xem:
                    if (App.User.isGrant("VIEW_ALL_INVENTORY") || App.User.isGrant("VIEW_CURRENT_INVENTORY")) {
                        $(".btnImportAuditTargets").hide();
                        $("#export-file-monitoring-list").show();
                        $("#download-form-monitoring-list").hide();
                    }

                    if (App.User.isGrant("EDIT_INVENTORY")) {
                        $(".btnImportAuditTargets").hide();
                        $("#export-file-monitoring-list").show();
                        $("#download-form-monitoring-list").show();
                    }
                }

            }
            else {
                if (getAccountType === "TaiKhoanChung" && getInventoryRoleType === 2) {
                    $(".btnImportAuditTargets").show();
                    $("#export-file-monitoring-list").show();
                    $("#download-form-monitoring-list").show();


                } else if (getAccountType === "TaiKhoanRieng") {
                    //Quá ngày kiểm kê:
                    if (moment(currentDate).isAfter(getInventoryDate)) {
                        $(".btnImportAuditTargets").hide();
                        $("#export-file-monitoring-list").show();
                        if (App.User.isGrant("EDIT_INVENTORY")) {
                            $("#download-form-monitoring-list").show();
                        }
                        else {
                            $("#download-form-monitoring-list").hide();
                        }


                    } else {
                        if (App.User.isGrant("VIEW_ALL_INVENTORY") || App.User.isGrant("VIEW_CURRENT_INVENTORY")) {
                            $(".btnImportAuditTargets").hide();
                            $("#export-file-monitoring-list").show();
                            $("#download-form-monitoring-list").hide();

                        }

                        if (App.User.isGrant("EDIT_INVENTORY")) {
                            $(".btnImportAuditTargets").show();
                            $("#export-file-monitoring-list").show();
                            $("#download-form-monitoring-list").show();

                        }
                    }
                }
            }
        });

        //Lắng nghe khi sự kiện đợt kiểm kê hoàn thành
        $(window).on("inventory_completed", function () {
            $(".btnImportAuditTargets").hide();
        })


        //Click nut search:
        $(document).delegate("#btn-search-create-monitoring", "click", (e) => {
            AuditDatatable.draw()
        });
        //Click nut reset:
        $(document).delegate("#btn-reset-create-monitoring", "click", (e) => {

            $("#monitoring_list-component_code").val('');
            $("#monitoring_list-so_no").val('');
            $("#monitoring_list-location").val('');
            $("#monitoring_list-distribution_account").val('');

            $("#monitoring_list-status")[0].reset();
            $("#monitoring_list-status")[0].toggleSelectAll(true);

            $("#monitoring_list-department")[0].reset();
            $("#monitoring_list-department")[0].toggleSelectAll(true);

            $("#monitoring_list-area")[0].reset();
            $("#monitoring_list-area")[0].toggleSelectAll(true);

            AuditDatatable.draw()
        });

        //Chon phong ban se hien thi ra danh sach khu vuc thuoc phong ban:
        ChangeDepartmentGetLocatin()

        AuditTargetHandler.init();


    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForAuditTargetLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}

function ValidateFormSearchListAudit() {
    $("#monitoring_list-component_code").on("input", function () {
        var inputValue = $(this).val();

        // Giới hạn chỉ nhập đến ký tự thứ 12
        if (inputValue.length > 12) {
            $(this).val(inputValue.substr(0, 12));
        }

        // Loại bỏ dấu cách
        //$(this).val(function (index, value) {
        //    return value.replace(/\s/g, '');
        //});
    });

    $("#monitoring_list-so_no, #monitoring_list-location, #monitoring_list-distribution_account").on("input", function () {
        var inputValue = $(this).val();

        // Giới hạn chỉ nhập đến ký tự thứ 20
        if (inputValue.length > 20) {
            $(this).val(inputValue.substr(0, 20));
        }

        // Loại bỏ dấu cách
        //$(this).val(function (index, value) {
        //    return value.replace(/\s/g, '');
        //});
    });

}

function ChangeDepartmentGetLocatin() {
    $(document).delegate("#monitoring_list-department", "change", (e) => {
        var listDepartments = $('#monitoring_list-department').val();

        //Nếu bỏ tích hết phòng ban thì bỏ tích hết khu vực
        if (listDepartments.length == 0) {
            $("#monitoring_list-area")[0].setOptions([]);
            $("#monitoring_list-area")[0].reset();
            return;
        }

        //Call Api Xem chi tiết:
        var link = $("#APIGateway").val();
        var filterData = JSON.stringify({
            Departments: listDepartments
        });

        $.ajax({
            type: "POST",
            url: link + `/api/inventory/location/departmentname`,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: filterData,
            success: function (res) {
                if (res.code == 200) {

                    //document.querySelector("#monitoring_list-area").reset();

                    var options = [
                    ];
                    let resultHtml = res?.data.map(item => {
                        options.push({ label: item.locationName, value: item.locationName })
                        return `
                        <option value="${item.locationName}">${item.locationName}</option>
                    `
                    }).join("");

                    $("#monitoring_list-area")[0].virtualSelect.alwaysShowSelectedOptionsLabel = !(options.length > 1);
                    $("#monitoring_list-area")[0].virtualSelect.disableAllOptionsSelectedText = !(options.length > 1);
                    $("#monitoring_list-area")[0].virtualSelect.selectAllOnlyVisible = !(options.length > 1);
                    $("#monitoring_list-area")[0].virtualSelect.autoSelectFirstOption = true;

                    document.querySelector('#monitoring_list-area').setOptions(options);

                    $("#monitoring_list-area")[0].reset();
                    $("#monitoring_list-area")[0].toggleSelectAll(true);


                }

            },
            error: function (error) {
                toastr.error(error.message)
            }
        });


    })
}

function InitListAuditDatatable() {
    let host = App.ApiGateWayUrl;

    var inventoryId = $("#inventory-wrapper").data("id")

    AuditDatatable = $('#audit-monitoring-table').DataTable({
        "bDestroy": true,
        "processing": `<div class="spinner"></div>`,
        pagingType: 'full_numbers',
        'language': {
            'loadingRecords': `<div class="spinner"></div>`,
            'processing': '<div class="spinner"></div>',
        },
        "serverSide": true,
        select: true,
        "filter": true,
        "searching": false,
        responsive: true,
        "lengthMenu": [10, 30, 50, 200],
        dom: 'rt<"bottom"flp><"clear">',
        "ordering": false,
        "ajax": {
            "url": host + `/api/inventory/${inventoryId}/audit-target`,
            "type": "POST",
            "contentType": "application/x-www-form-urlencoded",
            dataType: "json",
            data: function (data) {

                let dataFilter = {
                    ComponentCode: "",
                    SaleOrderNo: "",
                    Position: "",
                    AssigneeAccount: "",
                    Statuses: "",
                    Departments: "",
                    Locations: "",
                };

                dataFilter.ComponentCode = $("#monitoring_list-component_code").val();
                dataFilter.SaleOrderNo = $("#monitoring_list-so_no").val();
                dataFilter.Position = $("#monitoring_list-location").val();
                dataFilter.AssigneeAccount = $("#monitoring_list-distribution_account").val();
                dataFilter.Statuses = $("#monitoring_list-status").val();
                dataFilter.Departments = $("#monitoring_list-department").val();
                dataFilter.Locations = $("#monitoring_list-area").val();

                Object.assign(data, dataFilter);

                return data;
            },
            "dataSrc": function ({ data }) {
                return data;
            }
        },
        "drawCallback": async function (settings) {
            let totalPages = AuditDatatable.page.info().pages;
            let totalRecords = AuditDatatable.page.info().recordsTotal;

            let currPage = AuditDatatable.page() + 1;
            if (currPage == 1) {
                $("#tab-monitoring-create").find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $("#tab-monitoring-create").find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }
            if (currPage == totalPages) {
                $("#tab-monitoring-create").find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $("#tab-monitoring-create").find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }

            $("#tab-monitoring-create").find(".pagination").before(`<div class="h5 datatable_total_records"> <b>${window.languageData[window.currentLanguage]["Tổng"]}</b>: ${totalPages} <span class="total_record_seperate"> | </span>
                </div>`)

            //Ẩn xuất file nếu không có dữ liệu
            if (totalRecords == 0) {
                $("#export-file-monitoring-list").removeClass("btn_disabled").addClass("btn_disabled").attr("disabled", true);
            } else {
                $("#export-file-monitoring-list").removeClass("btn_disabled").attr("disabled", false);
            }

            if (totalRecords <= 10) {
                $(".container-list-view .bottom").hide()
            }

            //Cập nhật tổng số linh kiện có trong hệ thống
            auditTargetViewModel.totalRowsCount(totalRecords);

            $("#select_all").prop("checked", false);
            if (auditTargetViewModel.selectAllPages()) {
                $(`.checkbox_item`).prop("checked", true);

                if (auditTargetViewModel.selectAll_UncheckedIds().length) {
                    auditTargetViewModel.selectAll_UncheckedIds().forEach(item => {
                        $(`.checkbox_item[id="${item}"]`).prop("checked", false).change();
                    })
                }
            } else {
                if (auditTargetViewModel.checkedIds().length) {
                    auditTargetViewModel.checkedIds().forEach(item => {
                        $(`.checkbox_item[id="${item}"]`).prop("checked", true).change();
                    })
                }
            }

            let isAllOnePage = ($(`.checkbox_item`).length == $(`.checkbox_item:checked`).length) && $(`.checkbox_item:checked`).length > 0;
            if (isAllOnePage) {
                $("#select_all").prop("checked", true).change();
            }
        },
        "columns": [
            {
                "data": "auditTargetId",
                "name": "auditTargetId",
                "render": function (data, type, row, index) {
                    return `<input type="checkbox" id="${data}" class="checkbox_item" />`;
                },
                "autoWidth": true
            },
            {
                "data": "",
                "name": "STT",
                "render": function (data, type, row, index) {
                    let pagesize = index.settings._iDisplayLength;
                    let currentRow = ++index.row;
                    let currentPage = AuditDatatable.page() + 1;

                    let STT = ((currentPage - 1) * pagesize) + currentRow;

                    if (STT < 10) {
                        STT = `0${STT}`;
                    }
                    return STT;
                },
                "autoWidth": true
            },
            { "data": "plant", "name": "Plant", "autoWidth": true },
            { "data": "whLoc", "name": "WH Loc.", "autoWidth": true },
            { "data": "location", "name": "Khu vực", "autoWidth": true },
            {
                "data": "componentCode", "name": "Mã linh kiện",
                "autoWidth": true
            },
            { "data": "saleOrderNo", "name": "S/O No.", "autoWidth": true },
            { "data": "position", "name": "Vị trí", "autoWidth": true },
            {
                "data": "componentName", "name": "Tên linh kiện", "autoWidth": true
            },
            {
                "data": "status",
                "name": "Trạng thái",
                "render": function (data, type, row) {
                    var newStatus = "";
                    var color = "";
                    if (data == 0) {
                        newStatus = window.languageData[window.currentLanguage]["Chưa giám sát"];
                        color = "#6A707E";
                    } else if (data == 1) {
                        newStatus = window.languageData[window.currentLanguage]["Giám sát đạt"];
                        color = "#0D2EA0";
                    } else if (data == 2) {
                        newStatus = window.languageData[window.currentLanguage]["Giám sát không đạt"];
                        color = "#E60000";
                    }
                    const selectHtmlSpecial = `
                        <div style="color: ${color}; font-weight:400;">${newStatus}</div>
                    `;
                    return selectHtmlSpecial;
                }
                ,
                "autoWidth": true
            },
            {
                "data": "assigneeAccount", "name": "Tài khoản phân phát",
                "autoWidth": true
            },
            {
                "data": "",
                "name": "",
                "render": function (data, type, row) {

                    //Phân quyền màn danh sách giám sát:
                    var getInventoryStatus = $("#inventory-wrapper").attr("data-status");
                    let getAccountType = App.User.AccountType;
                    let getInventoryRoleType = App.User.InventoryLoggedInfo.InventoryRoleType;
                    let getInventoryDate = $("#inventory-wrapper").attr("data-inventory-date");
                    let currentDate = moment().format("YYYY-MM-DD");

                    let disabledEdit = "";

                    //TH1: Trang thái phiếu đã hoàn thành:
                    if (getInventoryStatus === '3') {
                        disabledEdit = "d-none";
                    }
                    else {
                        if (getAccountType === "TaiKhoanChung" && getInventoryRoleType === 2) {
                            disabledEdit = ""

                        } else if (getAccountType === "TaiKhoanRieng") {
                            //Quá ngày kiểm kê:
                            if (moment(currentDate).isAfter(getInventoryDate)) {
                                disabledEdit = "d-none";

                            } else {
                                if (App.User.isGrant("VIEW_ALL_INVENTORY") || App.User.isGrant("VIEW_CURRENT_INVENTORY")) {
                                    disabledEdit = "d-none";

                                }

                                if (App.User.isGrant("EDIT_INVENTORY")) {
                                    disabledEdit = "";

                                }
                            }
                        }
                    }

                    const selectHtmlSpecial = `
                        <div class="Controls_Container">
                            <div class="EditAudit_Controls mx-3 ${disabledEdit}" data-inventoryId="${row.inventoryId}" data-auditId="${row.auditTargetId}" disabledEdit>
                                <svg width="15" height="14" viewBox="0 0 15 14" fill="none" xmlns="http://www.w3.org/2000/svg">
                                    <path fill-rule="evenodd" clip-rule="evenodd" d="M8.48942 3.19297C9.65025 2.03214 10.7586 2.06131 11.8903 3.19297C12.4736 3.77047 12.7536 4.33047 12.7477 4.90797C12.7477 5.46797 12.4678 6.02214 11.8903 6.59381L11.1902 7.29964C11.1436 7.34631 11.0853 7.36964 11.0211 7.36964C10.9978 7.36964 10.9744 7.36381 10.9511 7.35797C9.40525 6.91464 8.16858 5.67797 7.72525 4.13214C7.70192 4.05047 7.72525 3.95714 7.78358 3.89881L8.48942 3.19297ZM9.41108 7.63214C9.56858 7.72547 9.73192 7.80714 9.90108 7.88881C10.1288 7.98747 10.1947 8.28936 10.0192 8.46483L6.73358 11.7505C6.66358 11.8263 6.51775 11.8963 6.41275 11.9138L4.17275 12.2288C4.10275 12.2405 4.03275 12.2463 3.96275 12.2463C3.64775 12.2463 3.35608 12.1355 3.14608 11.9313C2.90108 11.6805 2.79025 11.3071 2.84858 10.9105L3.16358 8.67631C3.18108 8.57714 3.25108 8.43131 3.32692 8.35547L6.60715 5.07524C6.78619 4.8962 7.08734 4.96436 7.19442 5.19381C7.27608 5.35714 7.35775 5.51464 7.45108 5.67214C7.52692 5.80047 7.60858 5.92881 7.67858 6.01631C7.76029 6.1416 7.85218 6.25162 7.91148 6.32264C7.91569 6.32767 7.91972 6.33251 7.92358 6.33714C7.93184 6.34775 7.93962 6.35789 7.94692 6.3674C7.97573 6.40496 7.99712 6.43283 8.01108 6.44214C8.20358 6.67547 8.41942 6.88547 8.61192 7.04881C8.65858 7.09547 8.69942 7.13047 8.71108 7.13631C8.82192 7.22964 8.93858 7.32297 9.03775 7.38714C9.16025 7.47464 9.28275 7.55631 9.41108 7.63214Z" fill="#87868C"/>
                                </svg>                            
                            </div>
                        </div>
                    `;
                    return selectHtmlSpecial;
                },
                "autoWidth": true
            },
        ],
    });
}

function ExportFileAuditTarget() {

    $(document).delegate("#export-file-monitoring-list", "click", (e) => {
        var inventoryId = $("#inventory-wrapper").data("id")
        var componentCode = $("#monitoring_list-component_code").val();
        var saleOrderNo = $("#input_date_from").val();
        var position = $("#monitoring_list-location").val();
        var assigneeAccount = $("#monitoring_list-distribution_account").val();
        var statuses = $("#monitoring_list-status").val();
        var departments = $("#monitoring_list-department").val();
        var locations = $("#monitoring_list-area").val();

        var filterData = {
            ComponentCode: componentCode,
            SaleOrderNo: saleOrderNo,
            Position: position,
            AssigneeAccount: assigneeAccount,
            Statuses: statuses,
            Departments: departments,
            Locations: locations,

        };

        loading(true);

        var url = `/inventory/${inventoryId}/audit-target/export`;
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

                    //Lấy ngày tháng hiện tại theo định dạng: yyyymmdd_hhmmss:
                    var currentTime = new Date();
                    var formattedTime = moment(currentTime).format("YYYYMMDD_HHmmss");

                    link.download = `DanhSachGiamSat_${formattedTime}.xlsx`;
                    link.click();
                } else {
                    toastr.error(window.languageData[window.currentLanguage]["Không tìm thấy file."]);
                }
                toastr.success(window.languageData[window.currentLanguage]["Export danh sách giám sát thành công."]);
            },
            error: function (error) {
                if (error != undefined) {
                    toastr.error(error.message);
                }
            },
            complete: function () {
                loading(false);
            }
        });

    })

}


$(document).delegate("#download-form-monitoring-list", "click", function (e) {
    let btn = $(this).closest("#download-form-monitoring-list");
    let fileKey = "Bieumaudanhsachgiamsat";
    FileTemplateHandler.download(fileKey);
})

    ; var AuditTargetHandler = (function () {
        let root = {
            parentEl: $("#inventory-wrapper")
        }
        let editFormValidator;

        function GetAuditComponentDetailAPI(inventoryId, auditTargetId) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/${inventoryId}/audit-target/${auditTargetId}`

                try {
                    const res = await $.ajax({
                        url: url,
                        type: 'GET',
                        contentType: 'application/json',
                    });
                    resolve(res)
                } catch (err) {
                    reject(err)
                }
            })
        }

        function EditAuditComponentAPI(inventoryId, auditTargetId, updateModel) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/${inventoryId}/audit-target/${auditTargetId}`

                try {
                    const res = await $.ajax({
                        url: url,
                        type: 'PUT',
                        contentType: 'application/json',
                        data: JSON.stringify(updateModel)
                    });
                    resolve(res)
                } catch (err) {
                    reject(err)
                }
            })
        }

        function PostImportAuditTargetAPI(inventoryId, file) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/${inventoryId}/audit-target/import`;
                let formData = new FormData();
                formData.append("importFile", file);

                $.ajax({
                    type: 'POST',
                    url: url,
                    //contentType: "multipart/form-data",
                    data: formData,
                    contentType: false,
                    processData: false,
                    success: function (response) {
                        if (response.failCount > 0) {
                            Swal.fire({
                                title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                                text: `${window.languageData[window.currentLanguage]["Cập nhật thành công"]} ${response?.successCount || 0} ${window.languageData[window.currentLanguage]["dòng dữ liệu và có"]} ${response?.failCount || 0} ${window.languageData[window.currentLanguage]["dòng dữ liệu lỗi không thể thực hiện import."]}
        ${window.languageData[window.currentLanguage]["Vui lòng ấn “Đồng ý” để xem dữ liệu lỗi."]}`,
                                confirmButtonText: window.languageData[window.currentLanguage]['Đồng ý'],
                                showCancelButton: true,
                                showLoaderOnConfirm: true,
                                cancelButtonText: window.languageData[window.currentLanguage]['Hủy bỏ'],
                                reverseButtons: true,
                                allowOutsideClick: false,
                                customClass: {
                                    actions: "swal_confirm_actions"
                                }
                            }).then((result, e) => {
                                if (result.isConfirmed) {
                                    let convertedByte = FileTemplateHandler.utils.base64ToArrayBuffer(response.bytes);
                                    FileTemplateHandler.utils.saveByteArr(convertedByte, response.fileType, response.fileName);
                                }
                            });
                        } else if (response.failCount == 0) {
                            Swal.fire({
                                title: `${window.languageData[window.currentLanguage]["Thông báo"]}`,
                                text: `${window.languageData[window.currentLanguage]["Cập nhật thành công"]}`,
                                confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                                width: '30%'
                            });
                        }
                    },

                    error: function (error) {
                        reject(error)
                    }
                });

            })
        }

        function CheckExistDoctypeA(inventoryId) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/web/doctype-a/check/${inventoryId}`;

                $.ajax({
                    type: 'GET',
                    url: url,
                    contentType: false,
                    processData: false,
                    async: true,
                    success: function (response) {
                        resolve(response)
                    },
                    error: function (error) {
                        reject(error)
                    }
                });
            })
        }

        function Cache() {
            //Modal chỉnh sửa linh kiện giám sát
            root.EditAuditTargetModal = $(root.parentEl).find("#edit-component-by-area");
            //Form sửa linh kiện giám sát
            root.EditComponentByArea_Form = $(root.EditAuditTargetModal).find("#EditComponentByArea_Form");

            //Nút xác nhận sửa 
            root.btnApplyEditForm = $(root.EditAuditTargetModal).find("#btn_Apply_EditComponentMonitoring");
        }

        async function PreLoad() {
            ValidateEditForm();

            //let lastestUser = await AppUser.getUser();
            //let lastestInventory = await lastestUser.inventoryLoggedInfo();

        }

        function ValidateEditForm() {
            jQuery.validator.addMethod("validatePlant", function (value, element) {
                let valid = true;
                let template = ["L401", "L402", "L404"];

                return template.includes(value);
            }, 'Plant không đúng.');

            jQuery.validator.addMethod("validateWHLoc", function (value, element) {
                let valid = true;
                let template = ["S001", "S002", "S402", "S090"];

                return template.includes(value);
            }, 'WH Loc không đúng.');

            let validateEditAuditModel = {
                rules: {
                    'Plant': {
                        required: true,
                        validatePlant: true
                    },
                    'WHLOC': {
                        required: true,
                        validateWHLoc: true
                    },
                    'Area': {
                        required: true
                    },
                    'ComponentCode': {
                        required: true,
                        minlength: 9,
                        pattern: '[a-zA-Z0-9]+'
                    },
                    'PositionCode': {
                        required: true
                    },
                    'AssigneeAccount': {
                        required: true
                    }
                },
                messages: {
                    Plant: {
                        required: window.languageData[window.currentLanguage]["Vui lòng nhập plant."],
                        validatePlant: window.languageData[window.currentLanguage]["Plant không đúng."]
                    },
                    WHLOC: {
                        required: window.languageData[window.currentLanguage]["Vui lòng nhập WH Loc."],
                        validateWHLoc: window.languageData[window.currentLanguage]["WH Loc. không đúng."]
                    },
                    Area: {
                        required: window.languageData[window.currentLanguage]["Vui lòng nhập khu vực."]
                    },
                    ComponentCode: {
                        required: window.languageData[window.currentLanguage]["Vui lòng nhập mã linh kiện."],
                        minlength: window.languageData[window.currentLanguage]["Mã linh kiện không đúng."],
                        pattern: window.languageData[window.currentLanguage]["Mã linh kiện không đúng."]
                    },
                    PositionCode: {
                        required: window.languageData[window.currentLanguage]["Vui lòng nhập vị trí."]
                    },
                    AssigneeAccount: {
                        required: window.languageData[window.currentLanguage]["Vui lòng nhập tài khoản phân phát."]
                    }
                },
            }

            editFormValidator = root.EditComponentByArea_Form.validate(validateEditAuditModel);

            //root.EditAuditTargetModal.delegate("#Plant", "keypress, keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(4));

            //root.EditAuditTargetModal.delegate("#WHLOC", "keypress, keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(4));
            //root.EditAuditTargetModal.delegate("#Area", "keypress, keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50));
            //root.EditAuditTargetModal.delegate("#ComponentCode", "keypress, keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(12));

            //root.EditAuditTargetModal.delegate("#SaleOrderNo", "keypress, keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(25));
            //root.EditAuditTargetModal.delegate("#PositionCode", "keypress, keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(20));


            root.EditAuditTargetModal.delegate("#Plant", "keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(4));
            root.EditAuditTargetModal.delegate("#Plant", "keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(4));
            //Chặn nhập khoảng trắng và cắt khoảng trắng khi paste
            root.EditAuditTargetModal.delegate("#Plant", "keypress", ValidateInputHelper.PreventWhiteSpace);
            root.EditAuditTargetModal.delegate("#Plant", "keyup", ValidateInputHelper.RemoveWhiteSpaceOnKeyup);


            root.EditAuditTargetModal.delegate("#WHLOC", "keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(4));
            root.EditAuditTargetModal.delegate("#WHLOC", "keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(4));
            //Chặn nhập khoảng trắng và cắt khoảng trắng khi paste
            root.EditAuditTargetModal.delegate("#WHLOC", "keypress", ValidateInputHelper.PreventWhiteSpace);
            root.EditAuditTargetModal.delegate("#WHLOC", "keyup", ValidateInputHelper.RemoveWhiteSpaceOnKeyup);

            root.EditAuditTargetModal.delegate("#Area", "keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50));
            root.EditAuditTargetModal.delegate("#Area", "keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50));


            root.EditAuditTargetModal.delegate("#ComponentCode", "keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(12));
            root.EditAuditTargetModal.delegate("#ComponentCode", "keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(12));
            //Chặn nhập khoảng trắng và cắt khoảng trắng khi paste
            root.EditAuditTargetModal.delegate("#ComponentCode", "keypress", ValidateInputHelper.PreventWhiteSpace);
            root.EditAuditTargetModal.delegate("#ComponentCode", "keyup", ValidateInputHelper.RemoveWhiteSpaceOnKeyup);


            root.EditAuditTargetModal.delegate("#SaleOrderNo", "keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(25));
            root.EditAuditTargetModal.delegate("#SaleOrderNo", "keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(25));

            root.EditAuditTargetModal.delegate("#PositionCode", "keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(20));
            root.EditAuditTargetModal.delegate("#PositionCode", "keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(20));
            //Chặn nhập khoảng trắng và cắt khoảng trắng khi paste
            root.EditAuditTargetModal.delegate("#PositionCode", "keypress", ValidateInputHelper.PreventWhiteSpace);
            root.EditAuditTargetModal.delegate("#PositionCode", "keyup", ValidateInputHelper.RemoveWhiteSpaceOnKeyup);


            root.EditAuditTargetModal.delegate("#AssigneeAccount", "keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50));
            root.EditAuditTargetModal.delegate("#AssigneeAccount", "keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50));
        }

        function Events() {
            root.parentEl.delegate(".EditAudit_Controls", "click", function (e) {
                let editBtn = $(this).closest(".EditAudit_Controls");

                let inventoryId = $(editBtn).attr("data-inventoryid");
                let auditId = $(editBtn).attr("data-auditid");

                root.EditAuditTargetModal.data("editInventoryId", inventoryId);
                root.EditAuditTargetModal.data("editAuditId", auditId);

                loading(true);
                GetAuditComponentDetailAPI(inventoryId, auditId).then(res => {
                    let item = res.data;

                    if (item) {
                        //Nếu status = 0 : chưa giám sát thì cho sửa
                        if (item.status == 0) {
                            root.EditComponentByArea_Form.find("#Plant").attr("disabled", false);
                            root.EditComponentByArea_Form.find("#WHLOC").attr("disabled", false);
                            root.EditComponentByArea_Form.find("#Area").attr("disabled", false);
                            root.EditComponentByArea_Form.find("#ComponentCode").attr("disabled", false);
                            root.EditComponentByArea_Form.find("#SaleOrderNo").attr("disabled", false);
                            root.EditComponentByArea_Form.find("#PositionCode").attr("disabled", false);
                            root.EditComponentByArea_Form.find("#AssigneeAccount").attr("disabled", false);

                            root.EditComponentByArea_Form.find("#Plant").val(item?.plant || "");
                            root.EditComponentByArea_Form.find("#WHLOC").val(item?.whloc || "");
                            root.EditComponentByArea_Form.find("#Area").val(item?.locationName || "");
                            root.EditComponentByArea_Form.find("#ComponentCode").val(item?.componentCode || "");
                            root.EditComponentByArea_Form.find("#SaleOrderNo").val(item?.saleOrderNo || "");
                            root.EditComponentByArea_Form.find("#PositionCode").val(item?.positionCode || "");
                            root.EditComponentByArea_Form.find("#AssigneeAccount").val(item?.assigneeName || "");

                            root.EditAuditTargetModal.find(".InventoryModal_Title").text(window.languageData[window.currentLanguage]["Chỉnh sửa linh kiện giám sát"]);
                            root.EditAuditTargetModal.find(".custom_bottom_modal").show();
                            root.EditAuditTargetModal.find(".btn-close").hide();
                            root.EditAuditTargetModal.find(".label_required").show();

                        } else {
                            root.EditComponentByArea_Form.find("#Plant").val(item?.plant || " ");
                            root.EditComponentByArea_Form.find("#WHLOC").val(item?.whloc || " ");
                            root.EditComponentByArea_Form.find("#Area").val(item?.locationName || " ");
                            root.EditComponentByArea_Form.find("#ComponentCode").val(item?.componentCode || " ");
                            root.EditComponentByArea_Form.find("#SaleOrderNo").val(item?.saleOrderNo || " ");
                            root.EditComponentByArea_Form.find("#PositionCode").val(item?.positionCode || " ");
                            root.EditComponentByArea_Form.find("#AssigneeAccount").val(item?.assigneeName || " ");

                            root.EditAuditTargetModal.find(".label_required").hide();
                            root.EditAuditTargetModal.find(".btn-close").show();
                            root.EditAuditTargetModal.find(".InventoryModal_Title").text(window.languageData[window.currentLanguage]["Chi tiết linh kiện giám sát"]);
                            root.EditAuditTargetModal.find(".custom_bottom_modal").hide();

                            root.EditComponentByArea_Form.find("#Plant").attr("disabled", true);
                            root.EditComponentByArea_Form.find("#WHLOC").attr("disabled", true);
                            root.EditComponentByArea_Form.find("#Area").attr("disabled", true);
                            root.EditComponentByArea_Form.find("#ComponentCode").attr("disabled", true);
                            root.EditComponentByArea_Form.find("#SaleOrderNo").attr("disabled", true);
                            root.EditComponentByArea_Form.find("#PositionCode").attr("disabled", true);
                            root.EditComponentByArea_Form.find("#AssigneeAccount").attr("disabled", true);
                        }
                    }
                }).catch(err => {

                }).finally(() => {
                    loading(false);
                    root.EditAuditTargetModal.modal('show');
                })
            })

            //On create modal show
            $(window).on('shown.bs.modal', function (e) {
                let target = e.target;
                let isEditAuditTargetModal = $(target).is(root.EditAuditTargetModal);

                let inventoryId = root.EditAuditTargetModal.data("editInventoryId");
                let auditId = root.EditAuditTargetModal.data("editAuditId");

                if (isEditAuditTargetModal) {

                }
            })

            $(window).on('hidden.bs.modal', function (e) {
                let target = e.target;
                let isEditAuditTargetModal = $(target).is(root.EditAuditTargetModal);
                if (isEditAuditTargetModal) {
                    editFormValidator.resetForm();
                }
            })

            root.btnApplyEditForm.click(function (e) {
                let isValid = root.EditComponentByArea_Form.valid();
                if (!isValid) {
                    return;
                }

                let inventoryId = root.EditAuditTargetModal.data("editInventoryId");
                let auditId = root.EditAuditTargetModal.data("editAuditId");

                let model = {
                    Plant: root.EditComponentByArea_Form.find("#Plant").val(),
                    WHLOC: root.EditComponentByArea_Form.find("#WHLOC").val(),
                    LocationName: root.EditComponentByArea_Form.find("#Area").val(),
                    ComponentCode: root.EditComponentByArea_Form.find("#ComponentCode").val(),
                    SaleOrderNo: root.EditComponentByArea_Form.find("#SaleOrderNo").val(),
                    PositionCode: root.EditComponentByArea_Form.find("#PositionCode").val(),
                    AssigneeAccount: root.EditComponentByArea_Form.find("#AssigneeAccount").val()
                }

                loading(true);
                EditAuditComponentAPI(inventoryId, auditId, model).then(res => {
                    toastr.success(res.message);

                    root.EditAuditTargetModal.modal("hide");
                }).catch(err => {
                    let response = err.responseJSON;
                    let data = response.data;

                    toastr.error(response?.message || "Có lỗi khi thực hiện chỉnh sửa linh kiện");

                    if (response?.code == 500) {
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                            text: response?.message || "",
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        })
                    }

                    let errorModel = {};
                    if (data["positionCode"]) {
                        errorModel["PositionCode"] = data["positionCode"];
                    }
                    if (data["plant"]) {
                        errorModel["Plant"] = data["plant"];
                    }
                    if (data["whloc"]) {
                        errorModel["WHLOC"] = data["whloc"];
                    }
                    if (data["locationName"]) {
                        errorModel["Area"] = data["locationName"];
                    }
                    if (data["componentCode"]) {
                        errorModel["ComponentCode"] = data["componentCode"];
                    }
                    if (data["saleOrderNo"]) {
                        errorModel["SaleOrderNo"] = data["saleOrderNo"];
                    }
                    if (data["assigneeAccount"]) {
                        errorModel["AssigneeAccount"] = data["assigneeAccount"];
                    }

                    editFormValidator.showErrors(errorModel)

                }).finally(() => {
                    AuditDatatable.draw();
                    loading(false);
                })
            })

            root.parentEl.delegate(".btnImportAuditTargets", "click", function (e) {
                let inventoryId = root.parentEl.attr("data-id");
                CheckExistDoctypeA(inventoryId).then(res => {
                    if (res.code == 200) {
                        let inventoryId = root.parentEl.attr("data-id");
                        root.parentEl.find("#inputImportAuditTargets").trigger("click");
                    }
                }).catch(err => {
                    if (err.responseJSON.code == 404) {
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                            text: window.languageData[window.currentLanguage]["Vui lòng tạo phiếu A trước khi thực hiện tạo danh sách giám sát."],
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        })
                    }
                }).finally(() => {
                })
            })

            root.parentEl.delegate("#inputImportAuditTargets", "change", function (e) {
                let inventoryId = root.parentEl.attr("data-id");
                let file = e.target.files[0];
                if (file) {
                    loading(true);
                    PostImportAuditTargetAPI(inventoryId, file).then(res => {
                        if (res.failCount > 0) {
                            let fileType = res.fileType;
                            let fileName = res.fileName;

                            Swal.fire({
                                title: `<b>${window.languageData[window.currentLanguage]["Lỗi dữ liệu danh sách giám sát."]}</b>`,
                                text: `${window.languageData[window.currentLanguage]["File import có dữ liệu lỗi. Vui lòng kiểm tra lại dữ liệu."]}`,
                                confirmButtonText: window.languageData[window.currentLanguage]['Đồng ý'],
                                //showCancelButton: true,
                                showLoaderOnConfirm: true,
                                width: '30%'
                                //cancelButtonText: 'Hủy bỏ',
                                //reverseButtons: true,
                                //allowOutsideClick: false,
                                //customClass: {
                                //    actions: "swal_confirm_actions"
                                //}
                            }).then((result, e) => {
                                if (result.isConfirmed) {
                                    var convertBytes = FileTemplateHandler.utils.base64ToArrayBuffer(res.bytes);
                                    FileTemplateHandler.utils.saveByteArr(convertBytes, fileType, fileName);
                                }
                            });
                        } else {
                            Swal.fire({
                                title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                                text: `${window.languageData[window.currentLanguage]["Import thành công"]} ${res.successCount} ${window.languageData[window.currentLanguage]["dòng dữ liệu."]}`,
                                confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                                width: '30%'
                            })
                        }
                    }).catch(err => {
                        let title = `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`;
                        if (err?.responseJSON?.code == ServerResponseStatusCode.InvalidFileExcel) {
                            title = window.languageData[window.currentLanguage]["File sai định dạng"]
                        } else if (err?.responseJSON?.code == ServerResponseStatusCode.NotExistDocTypeA) {
                            title = window.languageData[window.currentLanguage]["Không thể import"]
                        }

                        Swal.fire({
                            title: title,
                            text: err?.responseJSON?.message || "Có lỗi khi thực hiện import phiếu giám sát",
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        })

                    }).finally(() => {
                        loading(false);

                        AuditDatatable.draw();
                    })
                }

                $(e.target).val("");
            })
        }

        function Init() {
            Cache()
            PreLoad()
            Events()
        }

        return {
            init: Init
        }
    })();


function AuditTargetView() {
    let self = this;
    self.checkedIds = ko.observableArray([]);
    self.isCheckedAll = ko.observable(false);
    self.selectAllPages = ko.observable(false);
    self.selectAll_UncheckedIds = ko.observableArray([]);
    self.totalRowsCount = ko.observable(0);

    let deleteBtn = $("#tab-monitoring-create").find("#delete");
    let apis = {
        DeleteAuditTargets: function (inventoryId, arrIds, deleteAll) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/${inventoryId}/audit-target?deleteAll=${deleteAll}`;

                $.ajax({
                    type: 'DELETE',
                    url: url,
                    contentType: 'application/json',
                    data: JSON.stringify(arrIds),
                    success: function (response) {
                        resolve(response)
                    },
                    error: function (error) {
                        reject(error)
                    }
                });
            })
        }
    }

    self.enableDeleteButton = ko.computed(function (e) {
        let enable = self.checkedIds().length > 0;
        if (enable) {
            deleteBtn.attr("disabled", false);
            deleteBtn.removeClass("btn_disabled");
        } else {
            deleteBtn.attr("disabled", true);
            deleteBtn.removeClass("btn_disabled").addClass("btn_disabled");
        }

        return self.checkedIds().length > 0;
    })

    self.selectAllPages.subscribe(function (isChecked) {
        if (isChecked) {
            self.selectAll_UncheckedIds([]);
            $("#tab-monitoring-create").find("#select_all").prop("checked", true).change();
        } else {
            $("#tab-monitoring-create").find("#select_all").prop("checked", false).change();
            self.selectAll_UncheckedIds([]);
            self.checkedIds([]);
        }
    })


    //Các nút checkbox con
    $("#tab-monitoring-create").delegate(".checkbox_item", "change", function (e) {
        // Ẩn hiện nút chọn tất cả dựa theo checkbox
        let isCheckedAll = $("#tab-monitoring-create").find(".checkbox_item").length == $("#tab-monitoring-create").find(".checkbox_item:checked").length;
        $("#tab-monitoring-create").find("#select_all").prop("checked", isCheckedAll);

        // Lưu Id vào mảng
        let auditId = $(this).attr("id");
        let isChecked = $(this).is(":checked");
        if (isChecked) {
            if (!self.checkedIds().includes(auditId))
                self.checkedIds.push(auditId);
        } else {
            removeElement(self.checkedIds, auditId);
        }

        if (self.selectAllPages() && !isChecked) {
            if (!self.selectAll_UncheckedIds().includes(auditId))
                self.selectAll_UncheckedIds.push(auditId);
        } else if (self.selectAllPages() && isChecked) {
            removeElement(self.selectAll_UncheckedIds, auditId);
        }
    })

    //Nút checkbox chọn tất cả
    $("#tab-monitoring-create").delegate("#select_all", "change", function (e) {
        let checked = $(this).is(":checked");
        $("#tab-monitoring-create").find(".checkbox_item").prop("checked", checked).change();

        if (!checked) {
            if (self.selectAllPages() && !self.enableDeleteButton()) {
                $("#tab-monitoring-create").find("#select_AllPages").prop("checked", false).change();
            }
        }
    })

    $("#tab-monitoring-create").delegate("#select_AllPages", "change", function (e) {
        let isChecked = $(this).is(":checked");
        self.selectAllPages(isChecked);
    })


    //Delete event
    $("#tab-monitoring-create").delegate("#delete", "click", function (e) {
        let itemCount = self.selectAllPages() ? (self.totalRowsCount() - self.selectAll_UncheckedIds().length) : self.checkedIds().length;

        Swal.fire({
            title: `<b>${window.languageData[window.currentLanguage]["Xác nhận xóa"]}</b>`,
            text: `${window.languageData[window.currentLanguage]["Bạn có chắc chắn muốn xóa"]} ${itemCount} ${window.languageData[window.currentLanguage]["linh kiện giám sát này ?"]}`,
            confirmButtonText: window.languageData[window.currentLanguage]['Đồng ý'],
            showCancelButton: true,
            showLoaderOnConfirm: true,
            cancelButtonText: window.languageData[window.currentLanguage]["Hủy bỏ"],
            reverseButtons: true,
            allowOutsideClick: false,
            customClass: {
                actions: "swal_confirm_actions"
            }
        }).then((result, e) => {
            if (result.isConfirmed) {
                let inventoryId = $("#inventory-wrapper").attr("data-id");

                let Ids = self.selectAllPages() ? self.selectAll_UncheckedIds() : self.checkedIds();

                //Gọi API xóa
                loading(true);
                apis.DeleteAuditTargets(inventoryId, Ids, self.selectAllPages()).then(res => {
                    toastr.success(res.message);

                }).catch(err => {
                    toastr.error(err.responseJSON.message);
                }).finally(() => {
                    loading(false);

                    //Reset checkedIds
                    auditTargetViewModel.checkedIds([]);
                    auditTargetViewModel.selectAll_UncheckedIds([]);

                    AuditDatatable.draw();
                });
            }
        })
    })


    function removeElement(array, elem) {
        var index = array.indexOf(elem);
        if (index > -1) {
            array.splice(index, 1);
        }
    }

    return self;
}

ko.applyBindings(auditTargetViewModel, document.querySelector("#tab-monitoring-create"));
