let ErrorInvestigationDetailDatatable;

let maxLengthSearchDetailTypeC = 10;
let maxLengthCreateInventoryPlant = 4;
let maxLengthCreateInventoryWhLoc = 4;
let maxLengthCreateInventoryQuantityFrom = 5;
let maxLengthCreateInventoryQuantityTo = 5;
let maxLengthCreateInventoryComponentCode = 12;
let maxLengthCreateInventoryModelCode = 11;
let maxLengthCreateInventoryUserDistribution = 20;
let maxLengthErrorInvestigationConfirmErrorQuantity = 9;

$(function () {
    waitForInvestigationDetailLanguageData();
});

function waitForInvestigationDetailLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        InvestigationDetailShowMutilDropdown();
        InitListErrorInvestigationDetail_Datatable();
        ResetErrorInvestigationDetail();
        ErrorInvestigationDetailCheckAllItems();
        $(document).delegate("#InvestigationDetail_Search_Option_Form #btn-search", "click", ValidateInputHelper.Utils.debounce(function (e) {
            let validForm = $("#InvestigationDetail_Search_Option_Form").valid();
            if (validForm) {
                ErrorInvestigationDetailDatatable.draw();
                CallInvestigationPercentAPI();
            }
        }, 200))

        UpdateErrorTypes();
        CallInvestigationPercentAPI();
        ExportFileErrorInvestigationDetail();
        ValidateSearchErrorInvestigationDetail();
    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForInvestigationDetailLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}

function ValidateSearchErrorInvestigationDetail() {

    $("#InvestigationDetail_Plant").on("input", function () {
        if ($(this).val().length > maxLengthCreateInventoryPlant) {
            $(this).val($(this).val().slice(0, maxLengthCreateInventoryPlant));
        }
    });

    $("#InvestigationDetail_WHLoc").on("input", function () {
        if ($(this).val().length > maxLengthCreateInventoryWhLoc) {
            $(this).val($(this).val().slice(0, maxLengthCreateInventoryWhLoc));
        }
    });

    $("#InvestigationDetail_ComponentCode").on("input", function () {
        if ($(this).val().length > maxLengthCreateInventoryComponentCode) {
            $(this).val($(this).val().slice(0, maxLengthCreateInventoryComponentCode));
        }
    });

}

function ExportFileErrorInvestigationDetail() {
    $(document).off("click", ".export_file.InvestigationDetail_ExportExcel").on("click", ".export_file.InvestigationDetail_ExportExcel", (e) => {
        let host = App.ApiGateWayUrl;
        let dataFilter = {
            Plant: $("#InvestigationDetail_Plant").val() || "",
            WHLoc: $("#InvestigationDetail_WHLoc").val() || "",
            ComponentCode: $("#InvestigationDetail_ComponentCode").val() || "",
            AssigneeAccount: "",
            ErrorQuantityFrom: null,
            ErrorQuantityTo: null,
            ErrorMoneyFrom: null,
            ErrorMoneyTo: null,
            ErrorCategories: document.querySelector('#InvestigationDetail_ErrorCategory').isAllSelected() ? [] : $("#InvestigationDetail_ErrorCategory").val().map(Number),
            ErrorTypes: document.querySelector('#InvestigationDetail_ErrorType').isAllSelected() ? [] : $("#InvestigationDetail_ErrorType").val().map(Number),
            InventoryIds: [App?.User?.InventoryLoggedInfo?.InventoryModel?.InventoryId],
            IsExportExcel: true
        };

        loading(true)

        var url = `/investigation-detail/inventory/export`;
        $.ajax({
            type: 'POST',
            url: url,
            data: JSON.stringify(dataFilter), 
            contentType: "application/json; charset=utf-8", 
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

                    link.download = `Chitietdieutra_${formattedTime}.xlsx`;
                    link.click();
                } else {
                    toastr.error(window.languageData[window.currentLanguage]["Không tìm thấy file."]);
                }
                toastr.success(window.languageData[window.currentLanguage]["Export danh sách chi tiết điều tra thành công."]);
            },
            error: function (error) {
                toastr.error("Không tìm thấy file.");
            },
            complete: function () {
                loading(false);
            }
        });

    })

}

function CallInvestigationPercentAPI() {
    let inventoryId = App?.User?.InventoryLoggedInfo?.InventoryModel?.InventoryId;
    let host = App.ApiGateWayUrl;
    $.ajax({
        url: host + `/api/error-investigation/web/inventory/${inventoryId}/investigation-percent`, 
        type: "GET",
        contentType: "application/json",
        success: function (response) {
            if (response.code === 200) {
                $("#btn_InvestigationDetail_UpdateDataInvestigating .Percent").text(response.data + '%')
            }
        },
        error: function (xhr, status, error) {
            console.error("Lỗi API:", xhr.responseText);
        }
    });
}
function UpdateErrorTypes() {
    $(document).off("click", "#btn_InvestigationDetail_Investigating").on("click", "#btn_InvestigationDetail_Investigating", (e) => {
        e.preventDefault();
        let host = App.ApiGateWayUrl;

        let errorHistoryIds = [];

        $(".InvestigationDetail_CheckItem:checked").each(function () {
            errorHistoryIds.push($(this).data("id"));
        });

        if (errorHistoryIds.length === 0) {
            toastr.error(window.languageData[window.currentLanguage]["Vui lòng chọn linh kiện để điều chỉnh."]);
            return;
        }

        $.ajax({
            url: host + "/api/error-investigation/web/inventory/error-types?type=0",
            type: "PUT",
            contentType: "application/json",
            data: JSON.stringify(errorHistoryIds),
            success: function (response) {
                toastr.success(window.languageData[window.currentLanguage]["Điều chỉnh loại sai số thành công."]);
                ErrorInvestigationDetailDatatable.draw();
            },
            error: function (xhr) {
                var err = xhr?.responseJSON;
                toastr.error(err?.message);
            }
        });
    });

    $(document).off("click", "#btn_InvestigationDetail_ConfirmInvestigating").on("click", "#btn_InvestigationDetail_ConfirmInvestigating", (e) => {
        e.preventDefault();
        let host = App.ApiGateWayUrl;

        let errorHistoryIds = [];

        $(".InvestigationDetail_CheckItem:checked").each(function () {
            errorHistoryIds.push($(this).data("id"));
        });

        if (errorHistoryIds.length === 0) {
            toastr.error(window.languageData[window.currentLanguage]["Vui lòng chọn linh kiện để xác nhận điều chỉnh."]);
            return;
        }

        Swal.fire({
                title: `<b>${window.languageData[window.currentLanguage]['Xác nhận điều chỉnh']}</b>`,
                text: `${window.languageData[window.currentLanguage]['Bạn có chắc chắn muốn xác nhận điều chỉnh. Khi điều chỉnh xong dữ liệu sẽ ẩn khỏi danh sách sai số?']}`,
                confirmButtonText: window.languageData[window.currentLanguage]['Đồng ý'],
                showCancelButton: true,
                showLoaderOnConfirm: true,
                cancelButtonText: window.languageData[window.currentLanguage]['Từ chối'],
                reverseButtons: true,
                allowOutsideClick: false,
                customClass: {
                    actions: "swal_confirm_actions"
                }
            }).then((result, e) => {
                if (result.isConfirmed) {
                    loading(true);
                    $.ajax({
                        url: host + "/api/error-investigation/web/inventory/error-types?type=1",
                        type: "PUT",
                        contentType: "application/json",
                        data: JSON.stringify(errorHistoryIds),
                        success: function (response) {
                            toastr.success(window.languageData[window.currentLanguage]["Xác nhận điều chỉnh loại sai số thành công."]);
                            ErrorInvestigationDetailDatatable.draw();
                        },
                        error: function (xhr) {
                            var err = xhr?.responseJSON;
                            toastr.error(err?.message);
                        },
                        complete: function () {
                            loading(false);
                        }
                    });
                }
                else if (result.dismiss === Swal.DismissReason.cancel) {
                    loading(true);
                    $.ajax({
                        url: host + "/api/error-investigation/web/inventory/error-types?type=2",
                        type: "PUT",
                        contentType: "application/json",
                        data: JSON.stringify(errorHistoryIds),
                        success: function (response) {
                            toastr.success(window.languageData[window.currentLanguage]["Từ chối điều chỉnh loại sai số thành công."]);
                            ErrorInvestigationDetailDatatable.draw();
                        },
                        error: function (xhr) {
                            var err = xhr?.responseJSON;
                            toastr.error(err?.message);
                        },
                        complete: function () {
                            loading(false);
                        }
                    });
                
                }
        });

    });
}
function ErrorInvestigationDetailCheckAllItems() {
    
    // Khi click vào checkbox "Check All"
    $(document).off("change", "#InvestigationDetail_CheckAll").on("change", "#InvestigationDetail_CheckAll", (e) => {
        $('.InvestigationDetail_CheckItem').prop('checked', $(e.target).prop('checked'));
    });

    // Khi click vào checkbox item
    $(document).off("change", ".InvestigationDetail_CheckItem").on("change", ".InvestigationDetail_CheckItem", () => {
        let total = $('.InvestigationDetail_CheckItem').length; // Tổng số checkbox
        let checked = $('.InvestigationDetail_CheckItem:checked').length; // Số checkbox đã chọn

        // Nếu tất cả được chọn thì check "Check All", nếu không thì bỏ chọn
        $('#InvestigationDetail_CheckAll').prop('checked', total === checked);
    });

}

function ResetErrorInvestigationDetail() {

    $(document).delegate("#InvestigationDetail_Search_Option_Form #btn-reset", "click", ValidateInputHelper.Utils.debounce(function (e) {
        //$("#InvestigationDetail_InventoryName")[0].reset();
        //let currInventory = App?.User?.InventoryLoggedInfo?.InventoryModel?.InventoryId;
        //let firstInventoryOption = $("#InvestigationDetail_InventoryName")[0]?.options[0]?.value || "";
        //$("#InvestigationDetail_InventoryName")[0].setValue(currInventory || firstInventoryOption);

        $("#InvestigationDetail_ErrorCategory")[0].reset();
        $("#InvestigationDetail_ErrorCategory")[0].toggleSelectAll(true);

        $("#InvestigationDetail_ErrorType")[0].reset();
        $("#InvestigationDetail_ErrorType")[0].toggleSelectAll(true);

        $("#InvestigationDetail_Plant").val('');
        $("#InvestigationDetail_WHLoc").val('');
        $("#InvestigationDetail_ComponentCode").val('');
        ErrorInvestigationDetailDatatable.draw();

    }, 200))
}

function attachErrorDetailTooltip_Detail() {
    const $cells = $('#InvestigationDetail_DataTable').find('span.error-detail-cell');

    $cells.tooltip('dispose');   // xoá tooltip cũ (tránh nhân đôi)
    $cells.tooltip({
        trigger: 'click',     // 'hover' nếu muốn
        placement: 'auto',
        container: 'body',
        html: true
    });

    // Click ngoài cell → ẩn
    $(document)
        .off('click.closeEDDetail')
        .on('click.closeEDDetail', e => {
            if (!$(e.target).closest('#InvestigationDetail_DataTable span.error-detail-cell').length) {
                $('#InvestigationDetail_DataTable span.error-detail-cell').tooltip('hide');
            }
        });
}


function InitListErrorInvestigationDetail_Datatable() {
    let host = App.ApiGateWayUrl;

    ErrorInvestigationDetailDatatable = $('#InvestigationDetail_DataTable').DataTable({
        "bDestroy": true,
        "processing": `<div class="spinner"></div>`,
        pagingType: 'full_numbers',
        'language': {
            'loadingRecords': `<div class="spinner"></div>`,
            'processing': '<div class="spinner"></div>',
        },
        "serverSide": true,
        "scrollX": true,
        select: true,
        "filter": true,
        "searching": false,
        responsive: true,
        "lengthMenu": [10, 30, 50, 200],
        dom: 'rt<"bottom"flp><"clear">',
        "ordering": false,
        "ajax": {
            "url": host + `/api/error-investigation/web/inventory/detail`,
            "type": "POST",
            "contentType": "application/x-www-form-urlencoded",
            dataType: "json",
            data: function (data) {
                let dataFilter = {
                    Plant: "",
                    WHLoc: "",
                    ComponentCode: "",
                    AssigneeAccount: "",
                    ErrorQuantityFrom: "",
                    ErrorQuantityTo: "",
                    ErrorMoneyFrom: "",
                    ErrorMoneyTo: "",
                    ErrorCategories: "",
                    InventoryIds: "",
                    ErrorTypes: ""
                };

                dataFilter.Plant = $("#InvestigationDetail_Plant").val();
                dataFilter.WHLoc = $("#InvestigationDetail_WHLoc").val();
                dataFilter.ComponentCode = $("#InvestigationDetail_ComponentCode").val();
                dataFilter.ErrorCategories = document.querySelector('#InvestigationDetail_ErrorCategory').isAllSelected() ? "" : $("#InvestigationDetail_ErrorCategory").val();
                dataFilter.ErrorTypes = document.querySelector('#InvestigationDetail_ErrorType').isAllSelected() ? "" : $("#InvestigationDetail_ErrorType").val();
                dataFilter.InventoryIds = [App?.User?.InventoryLoggedInfo?.InventoryModel?.InventoryId];

                Object.assign(data, dataFilter);

                return data;
            },
            "dataSrc": function ({ data }) {
                return data;
            }
        },
        "drawCallback": function (settings) {
            let totalPages = ErrorInvestigationDetailDatatable.page.info().pages;
            let totalRecords = ErrorInvestigationDetailDatatable.page.info().recordsTotal;


            let currPage = ErrorInvestigationDetailDatatable.page() + 1;
            if (currPage == 1) {
                $("#InvestigationDetail_DataTable").find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $("#InvestigationDetail_DataTable").find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }
            if (currPage == totalPages) {
                $("#InvestigationDetail_DataTable").find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $("#InvestigationDetail_DataTable").find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }

            $("#InvestigationDetail_DataTable").find(".pagination").before(`<div class="h5 datatable_total_records"> <b>${window.languageData[window.currentLanguage]["Tổng"]}</b>: ${totalPages} <span class="total_record_seperate"> | </span>
                </div>`)

            if (totalRecords <= 10) {
                $(".container-list-view .bottom").hide()
            }

            attachErrorDetailTooltip_Detail();

        },
        "columns": [
            {
                "data": "",
                "name": "checkbox",
                "render": function (data, type, row, table) {
                    //let checkRole = App.User.InventoryLoggedInfo.InventoryRoleType == InventoryRoleType.GiamSat &&
                    //                App.User.isGrant("EDIT_INVENTORY");
                    //if (!checkRole) {
                    //    return ``;
                    //}

                    //return `<input type="checkbox" class="InvestigationDetailCheck" data-id="${row.id}" />`
                    return `<input type="checkbox" class="InvestigationDetail_CheckItem" data-id="${row.errorInvestigationHistoryId}"/>`
                },
                "autoWidth": true
            },
            {
                "data": "",
                "name": "STT",
                "render": function (data, type, row, index) {
                    let pagesize = index.settings._iDisplayLength;
                    let currentRow = ++index.row;
                    let currentPage = ErrorInvestigationDetailDatatable.page() + 1;

                    let STT = ((currentPage - 1) * pagesize) + currentRow;

                    if (STT < 10) {
                        STT = `0${STT}`;
                    }
                    return STT;
                },
                "autoWidth": true
            },
            {
                "data": "errorType", "name": "Loại sai số", render: function (data, type, row, index) {
                    if (data || data == 0) {
                        let textErrorType = "";
                        let colorClass = ""
                        if (data == 0) {
                            textErrorType = window.languageData[window.currentLanguage]["Giữ lại"];
                            colorClass = "color-orange";
                        }
                        else if (data == 1) {
                            textErrorType = window.languageData[window.currentLanguage]["Chờ xác nhận"];
                            colorClass = "color-green";
                        }
                        else if (data == 2) {
                            textErrorType = window.languageData[window.currentLanguage]["Điều chỉnh"];
                            colorClass = "color-red";
                        }
                        else if (data == 3) {
                            textErrorType = window.languageData[window.currentLanguage]["Từ chối"];
                            colorClass = "color-blue";
                        }

                        return `<div class="${colorClass}" txt-bolder">${textErrorType}</div>`;
                    }
                    return ``;
                },
                "autoWidth": true
            },
            { "data": "inventoryName", "name": "Đợt kiểm kê", "autoWidth": true },
            { "data": "plant", "name": "Plant", "autoWidth": true },
            { "data": "whLoc", "name": "WH Loc.", "autoWidth": true },
            {
                "data": "componentCode", "name": "Mã linh kiện",
                "autoWidth": true
            },
            { "data": "position", "name": "Vị trí", "autoWidth": true },
            {
                "data": "totalQuantity", "name": "Số lượng kiểm kê", render: function (data) {
                    if (data > 0) {
                        return ValidateInputHelper.Utils.convertDecimalInventory(data);
                    }
                    return data;
                }, "autoWidth": true },
            {
                "data": "accountQuantity", "name": "Số lượng hệ thống", render: function (data) {
                    if (data > 0) {
                        return ValidateInputHelper.Utils.convertDecimalInventory(data);
                    }
                    return data;
                }, "autoWidth": true
            },
            {
                "data": "errorQuantity",
                "name": "Chênh lệch",
                render: function (data, type, row, index) {
                    if (data < 0) {
                        // Chuyển giá trị âm thành dạng (value) và thêm class color-red
                        return `<div class="color-red">(${ValidateInputHelper.Utils.convertDecimalInventory(Math.abs(data))})</div>`;
                    }
                    return `<div>${ValidateInputHelper.Utils.convertDecimalInventory(data)}</div>`;
                },
                "autoWidth": true
            },
            {
                "data": "errorMoney", "name": "Giá trị",
                render: function (data, type, row, index) {
                    if (data < 0) {
                        // Chuyển giá trị âm thành dạng (value) và thêm class color-red
                        return `<div class="color-red">(${Math.abs(ValidateInputHelper.Utils.convertDecimalInventory(data))})</div>`;
                    }
                    return `<div>${ValidateInputHelper.Utils.convertDecimalInventory(data)}</div>`;
                },
                "autoWidth": true
            },
            //{
            //    "data": "unitPrice", "name": "Đơn giá",
            //    "autoWidth": true
            //},
            //{
            //    "data": "errorQuantityAbs", "name": "Chênh lệch ABS",
            //    "autoWidth": true
            //},
            //{
            //    "data": "errorMoneyAbs", "name": "Giá tiền ABS",
            //    "autoWidth": true
            //},
            {
                "data": "assigneeAccount", "name": "Tài khoản phân phát",
                "autoWidth": true
            },
            {
                "data": "investigationQuantity", "name": "Số lượng điều chỉnh", render: function (data) {
                    if (data > 0) {
                        return ValidateInputHelper.Utils.convertDecimalInventory(data);
                    }
                    return data;
                },
                "autoWidth": true
            },
            {
                "data": "errorCategoryName", "name": "Phân loại lỗi", "autoWidth": true
            },
            {
                data: "errorDetail",
                name: "Nguyên nhân sai số",
                autoWidth: true,
                sortable: false,

                render: function (data, type) {
                    if (type !== 'display') return data;          // sort/filter ⇒ dữ liệu thô
                    const safe = $('<div>').text(data ?? '').html();
                    return `<span class="error-detail-cell" title="${safe}">${safe}</span>`;
                }
            },
            {
                "data": "investigator", "name": "Người điều tra",
                "autoWidth": true
            },
            {
                "data": "investigationDateTime", "name": "Thời gian điều tra",
                "autoWidth": true
            },
            //{
            //    "data": "investigationTotal", "name": "Tổng số lượng điều tra",
            //    "autoWidth": true
            //},
            //{
            //    "data": "status", "name": "Trạng thái", render: function (data, type, row, index) {
            //        if (data || data == 0) {
            //            let textStatus = data == 0 ? "Chưa điều tra" : data == 1 ? "Đang điều tra" : "Đã điều tra";
            //            return `<div txt-bolder">${textStatus}</div>`;
            //        }
            //        return ``;
            //    }, "autoWidth": true
            //},
            //{
            //    "data": "",
            //    "name": "Lịch sử điều tra",
            //    "render": function (data, type, row) {
            //        const selectHtmlSpecial = `
            //            <div class="Controls_Container">
            //                <div class="ErrorInvestigationHistory_Controls mx-3">
            //                    <a class="ErrorInvestigationHistory_ViewDetail" data-inventoryId="${row.inventoryId}">${window.languageData[window.currentLanguage]["Xem"]}</a>
            //                </div>
            //            </div>
            //        `;
            //        return selectHtmlSpecial;
            //    },
            //    "autoWidth": true
            //},
            //{
            //    "data": "",
            //    "name": "Lịch sử các đợt kiểm kê",
            //    "render": function (data, type, row) {
            //        const selectHtmlSpecial = `
            //            <div class="Controls_Container">
            //                <div class="ErrorInvestigationInventoryHistoryControls mx-3">
            //                    <a class="ErrorInvestigationInventoryHistoryControls_ViewDetail" data-inventoryId="${row.inventoryId}">${window.languageData[window.currentLanguage]["Xem"]}</a>
            //                </div>
            //            </div>
            //        `;
            //        return selectHtmlSpecial;
            //    },
            //    "autoWidth": true
            //},
            {
                "data": "confirmInvestigator", "name": "Người xác nhận",
                "autoWidth": true
            },
            {
                "data": "approveInvestigator", "name": "Người phê duyệt",
                "autoWidth": true
            },
            {
                "data": "componentName", "name": "Tên linh kiện",
                "autoWidth": true
            },
            {
                "data": "investigationHistoryCount", "name": "Lịch sử điều chỉnh",
                "autoWidth": true
            },
            {
                "data": "noteDocumentTypeA", "name": "Ghi chú",
                "autoWidth": true
            }


        ],
    });
}
function InvestigationDetailShowMutilDropdown() {
    var dropdownSelectors = [
        //'#InvestigationDetail_InventoryName',
        `#InvestigationDetail_ErrorCategory`,
        `#InvestigationDetail_ErrorType`,
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

    //$("#InvestigationDetail_InventoryName")[0].reset();
    //let currInventory = App?.User?.InventoryLoggedInfo?.InventoryModel?.InventoryId;
    //let firstInventoryOption = $("#InvestigationDetail_InventoryName")[0]?.options[0]?.value || "";
    //$("#InvestigationDetail_InventoryName")[0].setValue(currInventory || firstInventoryOption);


    $("#InvestigationDetail_ErrorCategory")[0].reset();
    $("#InvestigationDetail_ErrorCategory")[0].toggleSelectAll(true);

    $("#InvestigationDetail_ErrorType")[0].reset();
    $("#InvestigationDetail_ErrorType")[0].toggleSelectAll(true);

}