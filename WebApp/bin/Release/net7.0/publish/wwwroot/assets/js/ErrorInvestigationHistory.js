let ErrorInvestigationHistoryDatatable;

let maxLengthSearchDetailTypeC = 10;
let maxLengthCreateInventoryPlant = 4;
let maxLengthCreateInventoryWhLoc = 4;
let maxLengthCreateInventoryQuantityFrom = 10;
let maxLengthCreateInventoryQuantityTo = 10;
let maxLengthCreateInventoryComponentCode = 12;
let maxLengthCreateInventoryModelCode = 11;
let maxLengthCreateInventoryUserDistribution = 20;
let maxLengthErrorInvestigationConfirmErrorQuantity = 9;

$(function () {
    waitForErrorInvestigationHistoryLanguageData();
});

function waitForErrorInvestigationHistoryLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        ErrorInvestigationHistoryShowMutilDropdown();
        InitListErrorInvestigationHistory_Datatable();
        ValidateSearchErrorInvestigationHistory();
        $(document).delegate("#ErrorInvestigationHistory_Search_Option_Form #btn-search", "click", ValidateInputHelper.Utils.debounce(function (e) {
            let validForm = $("#ErrorInvestigationHistory_Search_Option_Form").valid();
            if (validForm) {
                ErrorInvestigationHistoryDatatable.draw();
            }
        }, 200))
        ResetErrorInvestigationHistory();
        ExportFileErrorInvestigationHistory();
        ErrorInvestigationHistoryInventoryViewDetail();
        ErrorInvestigationHistory();


    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForErrorInvestigationHistoryLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}

function ErrorInvestigationHistory() {
    $(document).off("click", ".ErrorInvestigationHistory_ViewDetail").on("click", ".ErrorInvestigationHistory_ViewDetail", (e) => {
        e.preventDefault();
        //$("#ErrorInvestigationHistoryModal").modal("show");
        var rowElement = $(e.target).closest('tr');
        var rowIndex = ErrorInvestigationHistoryDatatable.row(rowElement).index();
        var pageLength = ErrorInvestigationHistoryDatatable.page.info().length;
        var currentPage = ErrorInvestigationHistoryDatatable.page();
        var STT = (currentPage * pageLength) + rowIndex + 1;
        var rowData = ErrorInvestigationHistoryDatatable.row(rowElement).data();
        componentCode = rowData?.componentCode;
        componentName = rowData?.componentName;

        $("#ErrorInvestigationHistory_Component").text(componentCode);
        $("#ErrorInvestigationHistory_ComponentName").text(componentName);
        $("#ErrorInvestigationHistory_Inventory").text(rowData?.inventoryName);
        $("#ErrorInvestigationHistory_InvestigationCount").text(rowData?.investigationHistoryCount);
        $("#ErrorInvestigationHistory_Position").text(rowData?.position);

        var inventoryId = $(e.target).attr("data-inventoryid");
        CallApiErrorInvestigationHistory(componentCode, inventoryId);
    })
}

function CallApiErrorInvestigationHistory(componentCode, inventoryId) {
    let host = App.ApiGateWayUrl;
    loading(true)
    var url = host + `/api/error-investigation/inventory/${inventoryId}/componentCode/${componentCode}/histories`;
    $.ajax({
        type: 'GET',
        url: url,
        dataType: "json",
        success: function (response) {
            let data = response?.data;

            $("#ErrorInvestigationHistoryModal").modal("show");

            const $container = $('.ErrorInvestigationHistoryCenter');
            $container.empty();
            if (!data || data.length === 0) {
                // Hiển thị thông báo khi không có dữ liệu
                $container.append(`<p class="no-data-message">${window.languageData[window.currentLanguage]["Không có dữ liệu"]}</p>`);
            } else {
                data.forEach((item, index) => {
                    let resUrlImage1 = (item.confirmationImage1 != null && item.confirmationImage1 != "") ? `${AppUser.getApiGateway}/${item.confirmationImage1.replaceAll("\\", "/")}` : "/assets/images/icons/default-image.png";
                    let resUrlImage2 = (item.confirmationImage2 != null && item.confirmationImage2 != "") ? `${AppUser.getApiGateway}/${item.confirmationImage2.replaceAll("\\", "/")}` : "/assets/images/icons/default-image.png";

                    const $itemHtml = $(`
                                        <div class="ErrorInvestigationHistoryCenter_Item">
                                            <h5>
                                                ${window.languageData[window.currentLanguage]["Điều chỉnh lần"]}<label class="ErrorInvestigationHistoryCenter_Index"> ${item?.index}:</label> ${window.languageData[window.currentLanguage]["Từ"]}
                                                <label class="ErrorInvestigationHistoryCenter_OldValue">${ValidateInputHelper.Utils.convertDecimalInventory(item?.oldValue)}</label> => <label class="ErrorInvestigationHistoryCenter_NewValue">${ValidateInputHelper.Utils.convertDecimalInventory(item?.newValue)}</label>
                                            </h5>
                                            <p>${window.languageData[window.currentLanguage]["Phân loại"]}: <label class="ErrorInvestigationHistoryCenter_ErrorCategory">${item?.errorCategoryName}</label></p>
                                            <p>${window.languageData[window.currentLanguage]["Chi tiết điều tra"]}: <label class="ErrorInvestigationHistoryCenter_Error_detail">${item?.errorDetail}</label></p>
                                            <p>${window.languageData[window.currentLanguage]["Người điều tra"]}: <label class="ErrorInvestigationHistoryCenter_Invesgator">${item?.investigator}</label></p>
                                            <p>${window.languageData[window.currentLanguage]["Thời điểm điều tra"]}: <label class="ErrorInvestigationHistoryCenter_InvesgationDate">${item?.investigationTime}</label></p>
                                            <p>${window.languageData[window.currentLanguage]["Thời điểm xác nhận"]}: <label class="ErrorInvestigationHistoryCenter_InvesgatorConfirmDate">${item?.confirmInvestigationTime}</label></p>
                                            <p>${window.languageData[window.currentLanguage]["Hình ảnh"]}:</p>
                                            <div class="ErrorInvestigationHistoryCenter_Images">
                                                <div class="ErrorInvestigationHistoryCenterItem_Images">
                                                    <img src="${resUrlImage1}" alt="Ảnh mặc định" style="width: 100%; max-width: 300px;">
                                                </div>
                                                <div class="ErrorInvestigationHistoryCenterItem_Images">
                                                    <img src="${resUrlImage2}" alt="Ảnh mặc định" style="width: 100%; max-width: 300px;">
                                                </div>
                                            </div>
                                        </div>
                                    `);
                    $container.append($itemHtml);
                });
            }
        },
        error: function (error) {
            var err = error?.responseJSON;
            Swal.fire({
                title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                text: window.languageData[window.currentLanguage][err?.message],
                confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                width: '30%'
            })
            $("#ErrorInvestigationHistoryModal").modal("hide");
        },
        complete: function () {
            loading(false);
        }
    });
}

function ErrorInvestigationHistoryInventoryViewDetail() {
    let componentCode = "";
    let componentName = "";
    $(document).off("click", ".ErrorInvestigationInventoryHistoryControls_ViewDetail").on("click", ".ErrorInvestigationInventoryHistoryControls_ViewDetail", (e) => {
        e.preventDefault();
        $("#ErrorInvestigationInventoryHistoryModal").modal("show")
        $("#ErrorInvestigationDocumentInventoryHistory_Inventory")[0].reset();

        var rowElement = $(e.target).closest('tr');
        var rowIndex = ErrorInvestigationHistoryDatatable.row(rowElement).index();
        var pageLength = ErrorInvestigationHistoryDatatable.page.info().length;
        var currentPage = ErrorInvestigationHistoryDatatable.page();
        var STT = (currentPage * pageLength) + rowIndex + 1;

        var rowData = ErrorInvestigationHistoryDatatable.row(rowElement).data();
        componentCode = rowData?.componentCode;
        componentName = rowData?.componentName;

        $("#ErrorInvestigationDocumentInventoryHistory_ComponentCode").text(componentCode);
        $("#ErrorInvestigationDocumentInventoryHistory_ComponentName").text(componentName);

        CallApiErrorInvestigationInventoryDocsHistory(componentCode);

    })

    $(document).off("click", "#ErrorInvestigationDocumentInventoryHistory_Search").on("click", "#ErrorInvestigationDocumentInventoryHistory_Search", (e) => {
        e.preventDefault();
        CallApiErrorInvestigationInventoryDocsHistory(componentCode);
    })
}

function CallApiErrorInvestigationInventoryDocsHistory(componentCode) {
    let host = App.ApiGateWayUrl;
    var inventoryId = App.User.InventoryLoggedInfo.InventoryModel.InventoryId;
    let dataFilter = {
        InventoryNames: []
    };
    dataFilter.InventoryNames = $("#ErrorInvestigationDocumentInventoryHistory_Inventory").val();
    loading(true)
    var url = host + `/api/error-investigation/web/history/componentCode/${componentCode}/inventory-docs`;
    $.ajax({
        type: 'POST',
        url: url,
        data: JSON.stringify(dataFilter),
        contentType: 'application/json',
        cache: false,
        success: function (response) {
            let data = response?.data;
            const $container = $('.ErrorInvestigationDocumentInventoryHistoryCenter');
            $container.empty();
            if (!data || data.length === 0) {
                // Hiển thị thông báo khi không có dữ liệu
                $container.append(`<p class="no-data-message">${window.languageData[window.currentLanguage]["Không có dữ liệu"]}</p>`);
            } else {
                data.forEach((item, index) => {
                    let resUrlImage1 = (item.investigationImage1 != null && item.investigationImage1 != "") ? `${AppUser.getApiGateway}/${item.investigationImage1.replaceAll("\\", "/")}` : "/assets/images/icons/default-image.png";
                    let resUrlImage2 = (item.investigationImage2 != null && item.investigationImage2 != "") ? `${AppUser.getApiGateway}/${item.investigationImage2.replaceAll("\\", "/")}` : "/assets/images/icons/default-image.png";

                    const $itemHtml = $(`
                                        <div class="ErrorInvestigationDocumentInventoryHistoryCenter_Item">
                                            <h5>
                                                 ${window.languageData[window.currentLanguage]["Đợt kiểm kê"]}:
                                                <label class="ErrorInvestigationDocumentInventoryHistoryCenterDate">${item.inventoryName}</label>
                                            </h5>
                                            <p>
                                                ${window.languageData[window.currentLanguage]["Điều chỉnh lần"]} <label class="ErrorInvestigationDocumentInventoryHistory_Index">${item.investigatingCount}:</label> ${window.languageData[window.currentLanguage]["Từ"]}
                                                <label class="ErrorInvestigationDocumentInventoryHistory_OldValue">${ValidateInputHelper.Utils.convertDecimalInventory(item.oldValue)}</label> => <label class="ErrorInvestigationDocumentInventoryHistory_NewValue">${ValidateInputHelper.Utils.convertDecimalInventory(item.newValue)}</label>
                                            </p>
                                            <p>${window.languageData[window.currentLanguage]["Phân loại"]}: <label class="ErrorInvestigationDocumentInventoryHistory_ErrorCategory">${item.errorCategoryName}</label></p>
                                            <p>${window.languageData[window.currentLanguage]["Chi tiết điều tra"]}: <label class="ErrorInvestigationDocumentInventoryHistory_Error_detail">${item.errorDetails}</label></p>
                                            <p>${window.languageData[window.currentLanguage]["Người điều tra"]}: <label class="ErrorInvestigationDocumentInventoryHistory_Invesgator">${item.investigator}</label></p>
                                            <p>${window.languageData[window.currentLanguage]["Thời điểm điều tra"]}: <label class="ErrorInvestigationDocumentInventoryHistory_InvesgationDate">${item.investigationDatetime}</label></p>
                                            <p>${window.languageData[window.currentLanguage]["Thời điểm xác nhận"]}: <label class="ErrorInvestigationDocumentInventoryHistory_InvesgatorConfirmDate">${item.confirmInvestigationDatetime}</label></p>
                                            <p>${window.languageData[window.currentLanguage]["Hình ảnh"]}:</p>
                                            <div class="ErrorInvestigationDocumentInventoryHistory_Images">
                                                <div class="ErrorInvestigationDocumentInventoryHistoryItem_Images">
                                                    <img src="${resUrlImage1}" alt="Ảnh điều tra 1" style="width: 100%; max-width: 300px;">
                                                </div>
                                                <div class="ErrorInvestigationDocumentInventoryHistoryItem_Images">
                                                    <img src="${resUrlImage2}" alt="Ảnh điều tra 2" style="width: 100%; max-width: 300px;">
                                                </div>
                                            </div>
                                        </div>
                                    `);
                    $container.append($itemHtml);
                });
            }
        },
        error: function (error) {
            console.log(error);
        },
        complete: function () {
            loading(false);
        }
    });
}
function getErrorCategoryName(errorCategory) {
    const categoryMap = {
        0: "Kiểm kê sai",
        1: "Quy cách đóng gói",
        2: "Lỗi không thống kê",
        3: "Không rõ nguyên nhân",
        4: "BOM sai",
        5: "Dùng nhầm",
        6: "Khác",
    };
    return categoryMap[errorCategory] || "Không xác định";
}
function ExportFileErrorInvestigationHistory() {
    $(document).off("click", ".ErrorInvestigationHistory_ExportExcel").on("click", ".ErrorInvestigationHistory_ExportExcel", (e) => {
        let host = App.ApiGateWayUrl;

        let dataFilter = {
            Plant: "",
            WHLoc: "",
            AssigneeAccount: "",
            ComponentCode: "",
            ErrorQuantityFrom: "",
            ErrorQuantityTo: "",
            ErrorMoneyFrom: "",
            ErrorMoneyTo: "",
            ErrorCategories: "",
            InventoryIds: ""
        };

        dataFilter.InventoryIds = $("#ErrorInvestigationHistory_InventoryName").val();
        dataFilter.Plant = $("#ErrorInvestigationHistory_Plant").val();
        dataFilter.WHLoc = $("#ErrorInvestigationHistory_WHLoc").val();
        dataFilter.AssigneeAccount = $("#ErrorInvestigationHistory_AssigneeAccount").val();
        dataFilter.ComponentCode = $("#ErrorInvestigationHistory_ComponentCode").val();
        dataFilter.ErrorQuantityFrom = $("#ErrorInvestigationHistory_ErrorQuantity_Qty_Start").val();
        dataFilter.ErrorQuantityTo = $("#ErrorInvestigationHistory_ErrorQuantity_Qty_End").val();
        dataFilter.ErrorMoneyFrom = $("#ErrorInvestigationHistory_ErrorMonney_Qty_Start").val();
        dataFilter.ErrorMoneyTo = $("#ErrorInvestigationHistory_ErrorMonney_Qty_End").val();
        dataFilter.ErrorCategories = document.querySelector('#ErrorInvestigationHistory_ErrorCategory').isAllSelected() ? "" : $("#ErrorInvestigationHistory_ErrorCategory").val();

        loading(true)

        var url = `/error-investigation-history/inventory/export`;
        $.ajax({
            type: 'POST',
            url: url,
            data: dataFilter,
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

                    link.download = `Danhsachlichsudieutrasaiso_${formattedTime}.xlsx`;
                    link.click();
                } else {
                    toastr.error(window.languageData[window.currentLanguage]["Không tìm thấy file."]);
                }
                toastr.success(window.languageData[window.currentLanguage]["Export danh sách lịch sử điều tra sai số thành công."]);
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
function ResetErrorInvestigationHistory() {

    $(document).delegate("#ErrorInvestigationHistory_Search_Option_Form #btn-reset", "click", ValidateInputHelper.Utils.debounce(function (e) {
        
        $("#ErrorInvestigationHistory_InventoryName")[0].reset();
        let currInventory = App?.User?.InventoryLoggedInfo?.InventoryModel?.InventoryId;
        let firstInventoryOption = $("#ErrorInvestigationHistory_InventoryName")[0]?.options[0]?.value || "";
        $("#ErrorInvestigationHistory_InventoryName")[0].setValue(currInventory || firstInventoryOption);

        $("#ErrorInvestigationHistory_Plant").val("");
        $("#ErrorInvestigationHistory_WHLoc").val("");

        $("#ErrorInvestigationHistory_ErrorCategory")[0].reset();
        $("#ErrorInvestigationHistory_ErrorCategory")[0].toggleSelectAll(true);

        $("#ErrorInvestigationHistory_AssigneeAccount").val("");
        $("#ErrorInvestigationHistory_ComponentCode").val("");
        $("#ErrorInvestigationHistory_ErrorQuantity_Qty_Start").val("");
        $("#ErrorInvestigationHistory_ErrorQuantity_Qty_End").val("");
        $("#ErrorInvestigationHistory_ErrorMonney_Qty_Start").val("");
        $("#ErrorInvestigationHistory_ErrorMonney_Qty_End").val("");
        ErrorInvestigationHistoryDatatable.draw();

    }, 200))
}
function ValidateSearchErrorInvestigationHistory() {

    $("#ErrorInvestigationHistory_Plant").on("input", function () {
        if ($(this).val().length > maxLengthCreateInventoryPlant) {
            $(this).val($(this).val().slice(0, maxLengthCreateInventoryPlant));
        }
    });

    $("#ErrorInvestigationHistory_WHLoc").on("input", function () {
        if ($(this).val().length > maxLengthCreateInventoryWhLoc) {
            $(this).val($(this).val().slice(0, maxLengthCreateInventoryWhLoc));
        }
    });

    $("#ErrorInvestigationHistory_ErrorQuantity_Qty_Start").maskMoney({
        allowZero: true,
        defaultZero: false,
        allowEmpty: true,
        allowNegative: true,
        precision: 0,
        selectAllOnFocus: false,
        bringCaretAtEndOnFocus: false,
        affixesStay: false
    });
    $("#ErrorInvestigationHistory_ErrorQuantity_Qty_Start").on(
        "keypress",
        ValidateInputHelper.OnlyNumerOnKeyPress
    );
    $("#ErrorInvestigationHistory_ErrorQuantity_Qty_Start").on(
        "keypress",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityFrom)
    );
    $("#ErrorInvestigationHistory_ErrorQuantity_Qty_Start").on(
        "keyup",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityFrom)
    );

    $("#ErrorInvestigationHistory_ErrorQuantity_Qty_End").maskMoney({
        allowZero: true,
        defaultZero: false,
        allowEmpty: true,
        allowNegative: true,
        precision: 0,
        selectAllOnFocus: false,
        bringCaretAtEndOnFocus: false,
        affixesStay: false
    });
    $("#ErrorInvestigationHistory_ErrorQuantity_Qty_End").on(
        "keypress",
        ValidateInputHelper.OnlyNumerOnKeyPress
    );
    $("#ErrorInvestigationHistory_ErrorQuantity_Qty_End").on(
        "keypress",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityTo)
    );
    $("#ErrorInvestigationHistory_ErrorQuantity_Qty_End").on(
        "keyup",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityTo)
    );


    $("#ErrorInvestigationHistory_ErrorMonney_Qty_Start").maskMoney({
        allowZero: true,
        defaultZero: false,
        allowEmpty: true,
        allowNegative: true,
        precision: 0,
        selectAllOnFocus: false,
        bringCaretAtEndOnFocus: false,
        affixesStay: false
    });
    $("#ErrorInvestigationHistory_ErrorMonney_Qty_Start").on(
        "keypress",
        ValidateInputHelper.OnlyNumerOnKeyPress
    );
    $("#ErrorInvestigationHistory_ErrorMonney_Qty_Start").on(
        "keypress",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityFrom)
    );
    $("#ErrorInvestigationHistory_ErrorMonney_Qty_Start").on(
        "keyup",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityFrom)
    );

    $("#ErrorInvestigationHistory_ErrorMonney_Qty_End").maskMoney({
        allowZero: true,
        defaultZero: false,
        allowEmpty: true,
        allowNegative: true,
        precision: 0,
        selectAllOnFocus: false,
        bringCaretAtEndOnFocus: false,
        affixesStay: false
    });
    $("#ErrorInvestigationHistory_ErrorMonney_Qty_End").on(
        "keypress",
        ValidateInputHelper.OnlyNumerOnKeyPress
    );
    $("#ErrorInvestigationHistory_ErrorMonney_Qty_End").on(
        "keypress",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityFrom)
    );
    $("#ErrorInvestigationHistory_ErrorMonney_Qty_End").on(
        "keyup",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityFrom)
    );


    $("#ErrorInvestigationHistory_ComponentCode").on("input", function () {
        if ($(this).val().length > maxLengthCreateInventoryComponentCode) {
            $(this).val($(this).val().slice(0, maxLengthCreateInventoryComponentCode));
        }
    });

    $("#ErrorInvestigationHistory_AssigneeAccount").on("input", function () {
        if ($(this).val().length > maxLengthCreateInventoryUserDistribution) {
            $(this).val($(this).val().slice(0, maxLengthCreateInventoryUserDistribution));
        }
    });

    $("#ErrorInvestigationHistory_ErrorQuantity_Qty_Start, #ErrorInvestigationHistory_ErrorQuantity_Qty_End, #ErrorInvestigationHistory_ErrorMonney_Qty_Start, #ErrorInvestigationHistory_ErrorMonney_Qty_End").change(
        function (e) {
            $("#ErrorInvestigationHistory_Search_Option_Form").valid()
        }
    );

    $("#ErrorInvestigationHistory_Search_Option_Form").validate({
        rules: {
            ErrorInvestigationHistory_ErrorQuantity_Qty_Start: {
                number: true,
                quantityRangeEmptyFromValidate: true,
                quantityRangeValidate: true,
            },
            ErrorInvestigationHistory_ErrorQuantity_Qty_End: {
                number: true,
                quantityRangeEmptyToValidate: true,
                quantityRangeValidate: true,
            },
            ErrorInvestigationHistory_ErrorMonney_Qty_Start: {
                number: true,
                moneyRangeEmptyFromValidate: true,
                moneyRangeValidate: true,
            },
            ErrorInvestigationHistory_ErrorMonney_Qty_End: {
                number: true,
                moneyRangeEmptyToValidate: true,
                moneyRangeValidate: true,
            },
        },
        messages: {},
    });


    jQuery.validator.addMethod(
        "quantityRangeEmptyFromValidate",
        function (value, element) {
            let valid = true;

            let quantityFrom = $("#ErrorInvestigationHistory_ErrorQuantity_Qty_Start")
                .val()
                .replaceAll(",", "");
            let quantityTo = $("#ErrorInvestigationHistory_ErrorQuantity_Qty_End")
                .val()
                .replaceAll(",", "");

            if ((quantityFrom == "" && quantityTo != "")) {
                valid = false;
            }

            return valid;
        },
        "Vui lòng nhập số lượng sai số."
    );

    jQuery.validator.addMethod(
        "quantityRangeEmptyToValidate",
        function (value, element) {
            let valid = true;

            let quantityFrom = $("#ErrorInvestigationHistory_ErrorQuantity_Qty_Start")
                .val()
                .replaceAll(",", "");
            let quantityTo = $("#ErrorInvestigationHistory_ErrorQuantity_Qty_End")
                .val()
                .replaceAll(",", "");

            if ((quantityTo == "" && quantityFrom != "")) {
                valid = false;
            }

            return valid;
        },
        "Vui lòng nhập số lượng sai số."
    );

    jQuery.validator.addMethod(
        "moneyRangeEmptyFromValidate",
        function (value, element) {
            let valid = true;

            let quantityFrom = $("#ErrorInvestigationHistory_ErrorMonney_Qty_Start")
                .val()
                .replaceAll(",", "");
            let quantityTo = $("#ErrorInvestigationHistory_ErrorMonney_Qty_End")
                .val()
                .replaceAll(",", "");

            if ((quantityFrom == "" && quantityTo != "")) {
                valid = false;
            }

            return valid;
        },
        "Vui lòng nhập giá trị sai số."
    );

    jQuery.validator.addMethod(
        "moneyRangeEmptyToValidate",
        function (value, element) {
            let valid = true;

            let quantityFrom = $("#ErrorInvestigationHistory_ErrorMonney_Qty_Start")
                .val()
                .replaceAll(",", "");
            let quantityTo = $("#ErrorInvestigationHistory_ErrorMonney_Qty_End")
                .val()
                .replaceAll(",", "");

            if ((quantityTo == "" && quantityFrom != "")) {
                valid = false;
            }

            return valid;
        },
        "Vui lòng nhập giá trị sai số."
    );

    jQuery.validator.addMethod(
        "quantityRangeValidate",
        function (value, element) {
            let valid = true;

            let quantityFrom = $("#ErrorInvestigationHistory_ErrorQuantity_Qty_Start")
                .val()
                .replaceAll(",", "");
            let quantityTo = $("#ErrorInvestigationHistory_ErrorQuantity_Qty_End")
                .val()
                .replaceAll(",", "");

            if (quantityFrom && quantityTo) {
                let convertQuantityFrom = Number(quantityFrom);
                let convertQuantityTo = Number(quantityTo);

                if (convertQuantityFrom > convertQuantityTo) {
                    valid = false;
                }
            }

            return valid;
        },
        "Số lượng sai số vừa nhập không hợp lệ."
    );

    jQuery.validator.addMethod(
        "moneyRangeValidate",
        function (value, element) {
            let valid = true;

            let moneyFrom = $("#ErrorInvestigationHistory_ErrorMonney_Qty_Start")
                .val()
                .replaceAll(",", "");
            let moneyTo = $("#ErrorInvestigationHistory_ErrorMonney_Qty_End")
                .val()
                .replaceAll(",", "");


            if (moneyFrom && moneyTo) {
                let convertQuantityFrom = Number(moneyFrom);
                let convertQuantityTo = Number(moneyTo);

                if (convertQuantityFrom > convertQuantityTo) {
                    valid = false;
                }
            }

            return valid;
        },
        "Giá trị sai số vừa nhập không hợp lệ."
    );

}

// đặt hàm ngay dưới InitListErrorInvestigationHistory_Datatable
function attachErrorDetailTooltip_History() {
    const $cells = $('#Error_Investigation_History_DataTable').find('span.error-detail-cell');

    $cells.tooltip('dispose');           
    $cells.tooltip({
        trigger: 'click',              
        placement: 'auto',
        container: 'body',
        html: true
    });

    // Click ngoài bảng → ẩn
    $(document)
        .off('click.closeEDHistory')
        .on('click.closeEDHistory', e => {
            if (!$(e.target).closest('#Error_Investigation_History_DataTable span.error-detail-cell').length) {
                $('#Error_Investigation_History_DataTable span.error-detail-cell').tooltip('hide');
            }
        });
}

function InitListErrorInvestigationHistory_Datatable() {
    let host = App.ApiGateWayUrl;
    var inventoryId = App.User.InventoryLoggedInfo.InventoryModel.InventoryId;

    ErrorInvestigationHistoryDatatable = $('#Error_Investigation_History_DataTable').DataTable({
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
            "url": host + `/api/error-investigation/web/inventory/history`,
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
                    InventoryIds: ""
                };

                dataFilter.InventoryIds = $("#ErrorInvestigationHistory_InventoryName").val();
                dataFilter.Plant = $("#ErrorInvestigationHistory_Plant").val();
                dataFilter.WHLoc = $("#ErrorInvestigationHistory_WHLoc").val();
                dataFilter.ComponentCode = $("#ErrorInvestigationHistory_ComponentCode").val();
                dataFilter.AssigneeAccount = $("#ErrorInvestigationHistory_AssigneeAccount").val();
                dataFilter.ErrorQuantityFrom = $("#ErrorInvestigationHistory_ErrorQuantity_Qty_Start").val();
                dataFilter.ErrorQuantityTo = $("#ErrorInvestigationHistory_ErrorQuantity_Qty_End").val();
                dataFilter.ErrorMoneyFrom = $("#ErrorInvestigationHistory_ErrorMonney_Qty_Start").val();
                dataFilter.ErrorMoneyTo = $("#ErrorInvestigationHistory_ErrorMonney_Qty_End").val();
                dataFilter.ErrorCategories = document.querySelector('#ErrorInvestigationHistory_ErrorCategory').isAllSelected() ? "" : $("#ErrorInvestigationHistory_ErrorCategory").val();


                Object.assign(data, dataFilter);

                return data;
            },
            "dataSrc": function ({ data }) {
                return data;
            }
        },
        "drawCallback": function (settings) {
            let totalPages = ErrorInvestigationHistoryDatatable.page.info().pages;
            let totalRecords = ErrorInvestigationHistoryDatatable.page.info().recordsTotal;


            let currPage = ErrorInvestigationHistoryDatatable.page() + 1;
            if (currPage == 1) {
                $("#Error_Investigation_History_DataTable").find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $("#Error_Investigation_History_DataTable").find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }
            if (currPage == totalPages) {
                $("#Error_Investigation_History_DataTable").find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $("#Error_Investigation_History_DataTable").find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }

            $("#Error_Investigation_History_DataTable").find(".pagination").before(`<div class="h5 datatable_total_records"> <b>${window.languageData[window.currentLanguage]["Tổng"]}</b>: ${totalPages} <span class="total_record_seperate"> | </span>
                </div>`)

            if (totalRecords <= 10) {
                $(".container-list-view .bottom").hide()
            }

            attachErrorDetailTooltip_History();
        },
        "columns": [
            //{
            //    "data": "",
            //    "name": "checkbox",
            //    "render": function (data, type, row, table) {
            //        //let checkRole = App.User.InventoryLoggedInfo.InventoryRoleType == InventoryRoleType.GiamSat &&
            //        //                App.User.isGrant("EDIT_INVENTORY");
            //        //if (!checkRole) {
            //        //    return ``;
            //        //}
            //        return `<input type="checkbox" class="InventoryDocument_check" data-id="${row.id}" />`
            //    },
            //    "autoWidth": true
            //},
            {
                "data": "",
                "name": "STT",
                "render": function (data, type, row, index) {
                    let pagesize = index.settings._iDisplayLength;
                    let currentRow = ++index.row;
                    let currentPage = ErrorInvestigationHistoryDatatable.page() + 1;

                    let STT = ((currentPage - 1) * pagesize) + currentRow;

                    if (STT < 10) {
                        STT = `0${STT}`;
                    }
                    return STT;
                },
                "autoWidth": true
            },
            //{
            //    "data": "",
            //    "name": "Điều tra",
            //    "render": function (data, type, row) {
            //        const selectHtmlSpecial = `
            //            <div class="Controls_Container">
            //                <div class="Investigation_Controls mx-3">
            //                    <a class="Investigation_Controls_Documents_ViewDetail" data-inventoryId="${row.inventoryId}"> ${window.languageData[window.currentLanguage]["Chi tiết"]}</a>
            //                </div>
            //            </div>
            //        `;
            //        return selectHtmlSpecial;
            //    },
            //    "autoWidth": true
            //},
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
                "name": "Chênh lệch", render: function (data) {
                    if (data > 0) {
                        return ValidateInputHelper.Utils.convertDecimalInventory(data);
                    }
                    return data;
                },
                "autoWidth": true
            },
            {
                "data": "errorMoney", "name": "Giá trị", render: function (data) {
                    if (data > 0) {
                        return ValidateInputHelper.Utils.convertDecimalInventory(data);
                    }
                    return data;
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
                    if (type !== 'display') return data;          // giữ nguyên cho sort/filter
                    const safe = $('<div>').text(data ?? '').html(); // escape chống XSS
                    return `<span class="error-detail-cell" title="${safe}">${safe}</span>`;
                }
            },
            {
                "data": "investigator", "name": "Người điều tra",
                "autoWidth": true
            },
            //{
            //    "data": "investigationTotal", "name": "Tổng số lượng điều tra",
            //    "autoWidth": true
            //},
            {
                "data": "status", "name": "Trạng thái", render: function (data, type, row, index) {
                    if (data || data == 0) {
                        let textStatus = data == 0 ? window.languageData[window.currentLanguage]["Chưa điều tra"] : data == 1 ? window.languageData[window.currentLanguage]["Đang điều tra"] : window.languageData[window.currentLanguage]["Đã điều tra"];
                        return `<div txt-bolder">${textStatus}</div>`;
                    }
                    return ``;
                }, "autoWidth": true
            },
            {
                "data": "",
                "name": "Lịch sử điều tra",
                "render": function (data, type, row) {
                    const selectHtmlSpecial = `
                        <div class="Controls_Container">
                            <div class="ErrorInvestigationHistory_Controls mx-3">
                                <a class="ErrorInvestigationHistory_ViewDetail" data-inventoryId="${row.inventoryId}">${window.languageData[window.currentLanguage]["Xem"]}</a>
                            </div>
                        </div>
                    `;
                    return selectHtmlSpecial;
                },
                "autoWidth": true
            },
            {
                "data": "",
                "name": "Lịch sử các đợt kiểm kê",
                "render": function (data, type, row) {
                    const selectHtmlSpecial = `
                        <div class="Controls_Container">
                            <div class="ErrorInvestigationInventoryHistoryControls mx-3">
                                <a class="ErrorInvestigationInventoryHistoryControls_ViewDetail" data-inventoryId="${row.inventoryId}">${window.languageData[window.currentLanguage]["Xem"]}</a>
                            </div>
                        </div>
                    `;
                    return selectHtmlSpecial;
                },
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

function ErrorInvestigationHistoryShowMutilDropdown() {
    var dropdownSelectors = [
        `#ErrorInvestigationHistory_InventoryName`,
        `#ErrorInvestigationHistory_ErrorCategory`,
        `#ErrorInvestigationDocumentInventoryHistory_Inventory`
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

    $("#ErrorInvestigationHistory_InventoryName")[0].reset();
    let currInventory = App?.User?.InventoryLoggedInfo?.InventoryModel?.InventoryId;
    let firstInventoryOption = $("#ErrorInvestigationHistory_InventoryName")[0]?.options[0]?.value || "";
    $("#ErrorInvestigationHistory_InventoryName")[0].setValue(currInventory || firstInventoryOption);

    $("#ErrorInvestigationHistory_ErrorCategory")[0].reset();
    $("#ErrorInvestigationHistory_ErrorCategory")[0].toggleSelectAll(true);

}