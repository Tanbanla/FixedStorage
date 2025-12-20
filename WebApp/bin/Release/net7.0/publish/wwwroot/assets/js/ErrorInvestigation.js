let ErrorInvestigationDatatable;
let ErrorInvestigationDocumentDatatable;

let maxLengthSearchDetailTypeC = 10;
let maxLengthCreateInventoryPlant = 4;
let maxLengthCreateInventoryWhLoc = 4;
let maxLengthCreateInventoryQuantityFrom = 10;
let maxLengthCreateInventoryQuantityTo = 10;
let maxLengthCreateInventoryComponentCode = 12;
let maxLengthCreateInventoryModelCode = 11;
let maxLengthCreateInventoryUserDistribution = 20;
let maxLengthErrorInvestigationConfirmErrorQuantity = 13;

$(function () {
    waitForErrorInvestigationLanguageData();
});

function waitForErrorInvestigationLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        ShowMutilDropdown();
        InitListErrorInvestigation_Datatable();

        ValidateSearchErrorInvestigation();

        $(document).delegate("#ErrorInvestigation_Search_Option_Form #btn-search", "click", ValidateInputHelper.Utils.debounce(function (e) {
            let validForm = $("#ErrorInvestigation_Search_Option_Form").valid();
            if (validForm) {
                ErrorInvestigationDatatable.draw();
            }
        }, 200))
        ResetErrorInvestigation();
        ClickConfirmInvestingToInvestigationDetailUI();
        ExportFileErrorInvestigation();
        ErrorInvestigationDocumentView();
        ErrorInvestigationConfirm();
        ErrorInvestigationInventoryHistoryViewDetail();
        ErrorInvestigationHistory();

        ErrorInvestigationDocumentDetailHandler.init();
        PrintPDFDocumentDetail();
        ErrorPercent();
        DownloadTemplateUpdateErrorInvestigation();
        ImportUpdateInvestigationData();
        ExportFileDataAdjustments();
        DownloadTemplateUpdateErrorInvestigationPivot();
        ClickImportUpdateErrorInvestigationPivot();
        CloseErrorInvestigationDocs()
    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForErrorInvestigationLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}

function CloseErrorInvestigationDocs() {
    $(document).off("click", "#closeErrorInvestigationDocumentModal").on("click", "#closeErrorInvestigationDocumentModal", (e) => {
        e.preventDefault();
        let host = App.ApiGateWayUrl;
        let componentCode = $("#ErrorInvestigationDocument_ComponentCode_Detail").text();
        let inventoryId = $('.ErrorInvestigationDocument_title').attr("data-inventoryid");
        $.ajax({
            url: host + `/api/error-investigation/inventory/${inventoryId}/componentCode/${componentCode}/status`,
            type: 'PUT',
            contentType: 'application/json',
            success: function (response) {
                if (response?.code === 200) {
                    $("#ErrorInvestigationDocumentModal").modal("hide");
                    ErrorInvestigationDatatable.draw();
                }
            },
            error: function (xhr, status, error) {
                $("#ErrorInvestigationDocumentModal").modal("hide");
            }
        });
    })
}

//Import Cập nhật dữ liệu Pivot:
function ClickImportUpdateErrorInvestigationPivot() {
    $(document).delegate("#btn_ErrorInvestigation_UpdatePivot", "click", (e) => {
        $("#inputImportUpdatePivotInvestigationData").trigger("click");
    })

    $(document).delegate("#inputImportUpdatePivotInvestigationData", "change", (e) => {
        let host = App.ApiGateWayUrl;
        let file = e.target.files[0];

        if (file.size > 0 && (/\.(xlsx|xls)$/i.test(file.name))) {
            var inventoryId = App.User.InventoryLoggedInfo.InventoryModel.InventoryId;
            var userId = App.User.UserId;

            let url = host + `/api/error-investigation/web/inventory/${inventoryId}/error-investigation/update-pivot/import`;
            let formData = new FormData();
            formData.append("file", file);
            loading(true)
            $.ajax({
                type: 'POST',
                url: url,
                data: formData,
                contentType: false,
                processData: false,
                async: true,
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
                            text: `${window.languageData[window.currentLanguage]["Cập nhật dữ liệu pivot thành công."]}`,
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        });
                    }

                },
                error: function (error) {
                    Swal.fire({
                        title: `<b>${window.languageData[window.currentLanguage]["File sai định dạng"]}</b>`,
                        text: window.languageData[window.currentLanguage][error?.responseJSON?.message] || window.languageData[window.currentLanguage]["Có lỗi khi thực hiện import."],
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    })
                },
                complete: function () {
                    loading(false);
                }
            });
        }
        else {
            Swal.fire({
                title: `<b>${window.languageData[window.currentLanguage]["File sai định dạng"]}</b>`,
                text: window.languageData[window.currentLanguage]["Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành cập nhật dữ liệu Pivot."],
                confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                width: '30%'
            })
        }
        $("#inputImportUpdatePivotInvestigationData").val("")
    })
}

function DownloadTemplateUpdateErrorInvestigationPivot() {
    //Tải biểu mẫu cập nhật số lượng điều chỉnh:
    $(document).off("click", "#download-file-update-investigation-pivot").on("click", "#download-file-update-investigation-pivot", (e) => {
        let fileKey = "BieumaucapnhatsoluongPivot";
        FileTemplateHandler.download(fileKey);
    })

}

function ExportFileDataAdjustments() {
    $(document).off("click", "#btn_ErrorInvestigation_Investigating").on("click", "#btn_ErrorInvestigation_Investigating", (e) => {
        let host = App.ApiGateWayUrl;

        var inventoryId = App.User.InventoryLoggedInfo.InventoryModel.InventoryId;

        loading(true)

        var url = host + `/api/error-investigation/web/inventory/${inventoryId}/export-data-adjustment`;
        $.ajax({
            type: 'POST',
            url: url,
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

                    link.download = `Dieuchinhdulieu_${formattedTime}.xlsx`;
                    link.click();
                } else {
                    toastr.error(window.languageData[window.currentLanguage]["Không tìm thấy file."]);
                }
                toastr.success(window.languageData[window.currentLanguage]["Export điều chỉnh dữ liệu thành công."]);
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

function ImportUpdateInvestigationData() {
    $(document).off("click", "#btn_ErrorInvestigation_UploadInvestigatingData").on("click", "#btn_ErrorInvestigation_UploadInvestigatingData", (e) => {
        e.preventDefault();
        $("#inputImportUpdateInvestigationData").trigger("click");

    })

    $(document).off("change", "#inputImportUpdateInvestigationData").on("change", "#inputImportUpdateInvestigationData", (e) => {
        let host = App.ApiGateWayUrl;
        let file = e.target.files[0];

        if (file.size > 0 && (/\.(xlsx|xls)$/i.test(file.name))) {

            let url = `${host}/api/error-investigation/web/inventory/error-investigation/update`;
            let formData = new FormData();
            formData.append("file", file);
            loading(true)
            $.ajax({
                type: 'POST',
                url: url,
                data: formData,
                contentType: false,
                processData: false,
                async: true,
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
                            text: `${window.languageData[window.currentLanguage]["Cập nhật số lượng điều chỉnh thành công."]}`,
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        });
                    }

                    ErrorInvestigationDatatable.draw();
                },
                error: function (error) {
                    Swal.fire({
                        title: `<b>${window.languageData[window.currentLanguage]["File sai định dạng"]}</b>`,
                        text: window.languageData[window.currentLanguage][error?.responseJSON?.message] || "Có lỗi khi thực hiện import.",
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    })
                },
                complete: function () {
                    loading(false);
                }
            });
        }
        else {

            Swal.fire({
                title: `<b>${window.languageData[window.currentLanguage]["File sai định dạng"]}</b>`,
                text: window.languageData[window.currentLanguage]["Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành cập nhật dữ liệu điều chỉnh."],
                confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                width: '30%'
            })
        }
        $("#inputImportUpdateInvestigationData").val("")
    })
}

function DownloadTemplateUpdateErrorInvestigation() {
    //Tải biểu mẫu cập nhật số lượng điều chỉnh:
    $(document).off("click", "#download-file-update-investigation").on("click", "#download-file-update-investigation", (e) => {
        let fileKey = "Bieumaucapnhatsoluongdieuchinh";
        FileTemplateHandler.download(fileKey);
    })

}
function ErrorPercent() {
    $(document).off("click", "#btn_ErrorInvestigation_SaveData").on("click", "#btn_ErrorInvestigation_SaveData", (e) => {
        var inventoryIds = $("#ErrorInvestigation_InventoryName").val();
        if (inventoryIds && inventoryIds.length > 1) {
            toastr.error(window.languageData[window.currentLanguage]["Tỷ lệ sai số chỉ được xem trong 1 đợt kiểm kê."]);
            return;
        }

        let inventoryId = '';
        if (inventoryIds && inventoryIds.length === 1) {
            inventoryId = inventoryIds[0];
        }

        $("#ErrorPercentModal").modal("show");

        let host = App.ApiGateWayUrl;
        var url = host + `/api/error-investigation/web/inventory/${inventoryId}/error-percent`;
        $.ajax({
            type: 'GET',
            url: url,
            dataType: "json",
            success: function (response) {
                let data = response?.data;
                if (data.length <= 0) {
                    $("#ErrorPercentModal .modal-body h5").show();
                    $("#ErrorPercentModal .modal-body table").hide();
                    return;
                }

                $("#ErrorPercentModal .modal-body h5").hide();
                $("#ErrorPercentModal .modal-body table").show();

                const tableBody = $("#ErrorPercentModalTableBody");
                tableBody.empty();

                data.forEach(item => {
                    const row = `<tr>
                            <td>${item.plant}</td>
                            <td>${item.errorPercent}</td>
                         </tr>`;
                    tableBody.append(row);
                });
            },
            error: function (error) {
                var err = error?.responseJSON;
                toastr.error(window.languageData[window.currentLanguage][err?.message]);
            }
        });

    })
}

function ClickConfirmInvestingToInvestigationDetailUI() {
    $(document).off("click", "#btn_ErrorInvestigation_ConfirmInvestigation").on("click", "#btn_ErrorInvestigation_ConfirmInvestigation", (e) => {
        window.location.href = "/investigation-detail";
    })
} 
function removeImage(event, imageId) {
    event.preventDefault(); 
    $("#" + imageId).attr("src", "/assets/images/icons/default-image.png"); 

    // Reset input file tương ứng
    if (imageId === "defaultImage1") {
        $("#uploadImage1").val(""); // Reset giá trị input file
    } else if (imageId === "defaultImage2") {
        $("#uploadImage2").val(""); // Reset giá trị input file
    }
}
function ResetErrorInvestigation() {

    $(document).delegate("#ErrorInvestigation_Search_Option_Form #btn-reset", "click", ValidateInputHelper.Utils.debounce(function (e) {
       
        $("#ErrorInvestigation_InventoryName")[0].reset();
        let currInventory = App?.User?.InventoryLoggedInfo?.InventoryModel?.InventoryId;
        let firstInventoryOption = $("#ErrorInvestigation_InventoryName")[0]?.options[0]?.value || "";
        $("#ErrorInvestigation_InventoryName")[0].setValue(currInventory || firstInventoryOption);

        $("#ErrorInvestigation_Plant").val("");
        $("#ErrorInvestigation_WHLoc").val("");

        $("#ErrorInvestigation_ErrorCategory")[0].reset();
        $("#ErrorInvestigation_ErrorCategory")[0].toggleSelectAll(true);

        $("#ErrorInvestigation_AssigneeAccount").val("");
        $("#ErrorInvestigation_ComponentCode").val("");
        $("#ErrorInvestigation_ErrorQuantity_Qty_Start").val("");
        $("#ErrorInvestigation_ErrorQuantity_Qty_End").val("");
        $("#ErrorInvestigation_ErrorMonney_Qty_Start").val("");
        $("#ErrorInvestigation_ErrorMonney_Qty_End").val("");

        $("#ErrorInvestigation_Status")[0].reset();
        $("#ErrorInvestigation_Status")[0].toggleSelectAll(true);
        ErrorInvestigationDatatable.draw();
    }, 200))
}
function ErrorInvestigationInventoryHistoryViewDetail() {
    let componentCode = "";
    let componentName = "";
    $(document).off("click", ".ErrorInvestigationInventoryHistoryControls_ViewDetail").on("click", ".ErrorInvestigationInventoryHistoryControls_ViewDetail", (e) => {
        e.preventDefault();
        $("#ErrorInvestigationInventoryHistoryModal").modal("show")
        $("#ErrorInvestigationDocumentInventoryHistory_Inventory")[0].reset();

        var rowElement = $(e.target).closest('tr');
        var rowIndex = ErrorInvestigationDatatable.row(rowElement).index();
        var pageLength = ErrorInvestigationDatatable.page.info().length;
        var currentPage = ErrorInvestigationDatatable.page();
        var STT = (currentPage * pageLength) + rowIndex + 1;

        var rowData = ErrorInvestigationDatatable.row(rowElement).data();
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
                $container.append('<p class="no-data-message">Không có dữ liệu</p>');
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
                                                <label class="ErrorInvestigationDocumentInventoryHistory_OldValue">${ValidateInputHelper.Utils.convertDecimalInventory(item.oldValue)}</label> => ${window.languageData[window.currentLanguage]["Đến"]} <label class="ErrorInvestigationDocumentInventoryHistory_NewValue">${ValidateInputHelper.Utils.convertDecimalInventory(item.newValue)}</label>
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

function ErrorInvestigationHistory() {
    $(document).off("click", ".ErrorInvestigationHistory_ViewDetail").on("click", ".ErrorInvestigationHistory_ViewDetail", (e) => {
        e.preventDefault();
        //$("#ErrorInvestigationHistoryModal").modal("show");
        var rowElement = $(e.target).closest('tr');
        var rowIndex = ErrorInvestigationDatatable.row(rowElement).index();
        var pageLength = ErrorInvestigationDatatable.page.info().length;
        var currentPage = ErrorInvestigationDatatable.page();
        var STT = (currentPage * pageLength) + rowIndex + 1;

        var rowData = ErrorInvestigationDatatable.row(rowElement).data();
        componentCode = rowData?.componentCode;
        componentName = rowData?.componentName;

        $("#ErrorInvestigationHistory_ComponentCode").text(componentCode);
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
                                                ${window.languageData[window.currentLanguage]["Điều chỉnh lần"]} <label class="ErrorInvestigationHistoryCenter_Index"> ${item?.index}:</label> ${window.languageData[window.currentLanguage]["Từ"]}
                                                <label class="ErrorInvestigationHistoryCenter_OldValue">${ValidateInputHelper.Utils.convertDecimalInventory(item?.oldValue)}</label> => ${window.languageData[window.currentLanguage]["Đến"]} <label class="ErrorInvestigationHistoryCenter_NewValue">${ValidateInputHelper.Utils.convertDecimalInventory(item?.newValue)}</label>
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

function ErrorInvestigationConfirm() {
    $(document).off("click", "button.btn-ErrorInvestigationDocument").on("click", "button.btn-ErrorInvestigationDocument", (e) => {
        e.preventDefault();
        let componentCode = $("#ErrorInvestigationDocument_ComponentCode_Detail").text();
        let inventoryId = $('.ErrorInvestigationDocument_title').attr("data-inventoryid");
        
        ErrrorInvestigationDocumentConfirmDetail(componentCode, inventoryId);
        UploadImages();
        ValidateErrorInvestigationConfirm();
        ErrorInvestigationConfirmOrUpdateOrCancel(inventoryId);

    })
}
function ErrorInvestigationConfirmOrUpdateOrCancel(inventoryId) {
    const defaultImageSrc = "/assets/images/icons/default-image.png";
    //Confirm:
    $(document).off("click", ".btn-ErrorInvestigationDocumentConfirmComplete").on("click", ".btn-ErrorInvestigationDocumentConfirmComplete", (e) => {
        e.preventDefault();
        let host = App.ApiGateWayUrl;
        var valid = $("#wrapper_table_error_investigation_confirm_form").valid();
        // After successful submit, clear the form fields
        if (valid) {
            // Lấy dữ liệu từ các trường
            const componentCode = $(".ErrorInvestigationConfirm_ComponentCode").text();
            const errorQuantity = $(".ErrorInvestigationConfirm_ErrorQuantity").val();
            const errorDetail = $("#ErrorInvestigationConfirm_ErrorDetail").val();
            const errorCategory = $(".ErrorInvestigationConfirm_ErrorCategory").val();
            const confirmationImage1 = $('#uploadImage1')[0].files[0];
            const confirmationImage2 = $('#uploadImage2')[0].files[0]; // Assuming you have similar setup for Image2

            // Tạo đối tượng FormData để gửi dữ liệu
            const formData = new FormData();
            formData.append('Quantity', errorQuantity);
            formData.append('ErrorCategory', errorCategory);
            formData.append('ErrorDetails', errorDetail);
            if (confirmationImage1) {
                formData.append('ConfirmationImage1', confirmationImage1);
            }
            if (confirmationImage2) {
                formData.append('ConfirmationImage2', confirmationImage2);
            }

            const isDeleteImage1 = $("#defaultImage1").attr("src") === defaultImageSrc;
            const isDeleteImage2 = $("#defaultImage2").attr("src") === defaultImageSrc;
            formData.append('IsDeleteImage1', isDeleteImage1);
            formData.append('IsDeleteImage2', isDeleteImage2);
            //Type = 0, thực hiện xác nhận:
            const type = 0; 

            $.ajax({
                url: host + `/api/error-investigation/inventory/${inventoryId}/componentCode/${componentCode}/type/${type}`,
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    if (response?.code === 200) {
                        toastr.success(window.languageData[window.currentLanguage]["Đã thực hiện điều chỉnh dữ liệu điều tra sai số."]);
                        $("#ErrorInvestigationDocumentConfirmModal").modal("hide");
                        ErrorInvestigationDatatable.draw();
                        // Clear form fields after submit
                        $(".ErrorInvestigationConfirm_ErrorQuantity").val('');
                        $("#ErrorInvestigationConfirm_ErrorDetail").val('');
                        $(".ErrorInvestigationConfirm_ErrorCategory").val('');
                        $("#uploadImage1").val('');
                        $("#uploadImage2").val('');
                        $("#defaultImage1").attr("src", "/assets/images/icons/default-image.png");
                        $("#defaultImage2").attr("src", "/assets/images/icons/default-image.png");
                    }
                },
                error: function (xhr, status, error) {
                    var err = xhr?.responseJSON;
                    toastr.error(err?.message);
                }
            });
        }
        
    })

    //Update:
    $(document).off("click", ".btn-ErrorInvestigationDocumentConfirmUpdate").on("click", ".btn-ErrorInvestigationDocumentConfirmUpdate", (e) => {
        e.preventDefault();
        let host = App.ApiGateWayUrl;
        var valid = $("#wrapper_table_error_investigation_confirm_form").valid();
        if (valid) {
            // Lấy dữ liệu từ các trường
            const componentCode = $(".ErrorInvestigationConfirm_ComponentCode").text();
            const errorQuantity = $(".ErrorInvestigationConfirm_ErrorQuantity").val();
            const errorDetail = $("#ErrorInvestigationConfirm_ErrorDetail").val();
            const errorCategory = $(".ErrorInvestigationConfirm_ErrorCategory").val();
            const confirmationImage1 = $('#uploadImage1')[0].files[0];
            const confirmationImage2 = $('#uploadImage2')[0].files[0]; // Assuming you have similar setup for Image2

            // Tạo đối tượng FormData để gửi dữ liệu
            const formData = new FormData();
            formData.append('Quantity', errorQuantity);
            formData.append('ErrorCategory', errorCategory);
            formData.append('ErrorDetails', errorDetail);
            if (confirmationImage1) {
                formData.append('ConfirmationImage1', confirmationImage1);
            }
            if (confirmationImage2) {
                formData.append('ConfirmationImage2', confirmationImage2);
            }

            const isDeleteImage1 = $("#defaultImage1").attr("src") === defaultImageSrc;
            const isDeleteImage2 = $("#defaultImage2").attr("src") === defaultImageSrc;
            formData.append('IsDeleteImage1', isDeleteImage1);
            formData.append('IsDeleteImage2', isDeleteImage2);
            //Type = 1, thực hiện chinh sửa:
            const type = 1;

            $.ajax({
                url: host + `/api/error-investigation/inventory/${inventoryId}/componentCode/${componentCode}/type/${type}`,
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    if (response?.code === 200) {
                        toastr.success(window.languageData[window.currentLanguage]["Đã thực hiện cập nhật dữ liệu điều tra sai số."]);
                        $("#ErrorInvestigationDocumentConfirmModal").modal("hide");
                        ErrorInvestigationDatatable.draw();
                    }
                },
                error: function (xhr, status, error) {
                    var err = xhr?.responseJSON;
                    toastr.error(err?.message);
                }
            });
        }

    })

    //Cancel:
    $(document).off("click", ".btn-ErrorInvestigationDocumentConfirmCancel").on("click", ".btn-ErrorInvestigationDocumentConfirmCancel", (e) => {
        e.preventDefault();
        let host = App.ApiGateWayUrl;
        const componentCode = $(".ErrorInvestigationConfirm_ComponentCode").text();

        Swal.fire({
            title: `<b>${window.languageData[window.currentLanguage]['Thông báo']}</b>`,
            text: `${window.languageData[window.currentLanguage]['Bạn có chắc chắn muốn thoát? Dữ liệu đã nhập sẽ không được lưu lại.']}`,
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
                $.ajax({
                    url: host + `/api/error-investigation/inventory/${inventoryId}/componentCode/${componentCode}/status`,
                    type: 'PUT',
                    contentType: 'application/json',
                    success: function (response) {
                        if (response?.code === 200) {
                            $("#ErrorInvestigationDocumentConfirmModal").modal("hide");
                            ErrorInvestigationDatatable.draw();
                        }
                    },
                    error: function (xhr, status, error) {
                        $("#ErrorInvestigationDocumentConfirmModal").modal("hide");
                    }
                });
            }
        });
    })

}
function ErrrorInvestigationDocumentConfirmDetail(componentCode, inventoryId) {
    let host = App.ApiGateWayUrl;
    
    $.ajax({
        type: "GET",
        url: host + `/api/error-investigation/inventory/${inventoryId}/componentCode/${componentCode}/view-detail`,
        dataType: "json",
        success: function (res) {
            if (res.code == 200) {
                $("#ErrorInvestigationDocumentModal").modal("hide");
                $("#ErrorInvestigationDocumentConfirmModal").modal("show");
                let textStatus = res?.data?.status == 0 ? window.languageData[window.currentLanguage]["Chưa điều tra"] : res?.data?.status == 1 ? window.languageData[window.currentLanguage]["Đang điều tra"] : window.languageData[window.currentLanguage]["Đã điều tra"];
                $(".ErrorInvestigationConfirm_Status").text(textStatus);
                $(".ErrorInvestigationConfirm_ComponentCode").text(componentCode);

                //Điều trả là reset data default:
                $(".ErrorInvestigationConfirm_ErrorQuantity").val('');
                $("#ErrorInvestigationConfirm_ErrorDetail").val('');
                $(".ErrorInvestigationConfirm_ErrorCategory").val('');
                // Reset the image source to the default image
                $("#defaultImage1").attr("src", "/assets/images/icons/default-image.png");
                $("#defaultImage2").attr("src", "/assets/images/icons/default-image.png");

                $(".ErrorInvestigationDocumentConfirm_Compelete_Container").hide();
                $(".ErrorInvestigationDocumentConfirm_Update_Container").show();

                //let errorQuantity = res?.data?.errorQuantity;
                //if (errorQuantity.includes(".000")) {
                //    errorQuantity = errorQuantity.replace(".000", "");
                //}

                //$(".ErrorInvestigationConfirm_ErrorQuantity").val(errorQuantity);
                //$("#ErrorInvestigationConfirm_ErrorDetail").val(res?.data?.errorDetails);
                //$(".ErrorInvestigationConfirm_ErrorCategory").val(res?.data?.errorCategory);
                //// Reset the image source to the default image
                //let urlImage1 = `${AppUser.getApiGateway}/${res?.data?.confirmationImage1.replaceAll("\\", "/")}`;
                //IsImageExist(urlImage1, (valid) => {
                //    if (valid) {
                //        $("#defaultImage1").attr("src", urlImage1);
                //    } else {
                //        // Nếu hình ảnh không tồn tại, đặt hình ảnh mặc định
                //        $("#defaultImage1").attr("src", "/assets/images/icons/default-image.png");
                //    }
                //});

                //let urlImage2 = `${AppUser.getApiGateway}/${res?.data?.confirmationImage2.replaceAll("\\", "/")}`;
                //IsImageExist(urlImage2, (valid) => {
                //    if (valid) {
                //        $("#defaultImage2").attr("src", urlImage2);
                //    } else {
                //        // Nếu hình ảnh không tồn tại, đặt hình ảnh mặc định
                //        $("#defaultImage2").attr("src", "/assets/images/icons/default-image.png");
                //    }
                //});

                
            }
        },
        error: function (error) {
            var err = error.responseJSON;
            if (err.code === 403) {
                toastr.error("Bạn không có quyền thực hiện điều tra!");
                $("#ErrorInvestigationDocumentModal").modal("hide");
                $("#ErrorInvestigationDocumentConfirmModal").modal("hide");
                return;
            }
            $("#ErrorInvestigationDocumentModal").modal("hide");
            $("#ErrorInvestigationDocumentConfirmModal").modal("show");
            $(".ErrorInvestigationConfirm_ComponentCode").text(componentCode);
            let textStatus = err?.data == 0 ? window.languageData[window.currentLanguage]["Chưa điều tra"] : err?.data == 1 ? window.languageData[window.currentLanguage]["Đang điều tra"] : window.languageData[window.currentLanguage]["Đã điều tra"];
            $(".ErrorInvestigationConfirm_Status").text(textStatus);
            $(".ErrorInvestigationConfirm_ErrorQuantity").val('');
            $("#ErrorInvestigationConfirm_ErrorDetail").val('');
            $(".ErrorInvestigationConfirm_ErrorCategory").val('');
            // Reset the image source to the default image
            $("#defaultImage1").attr("src", "/assets/images/icons/default-image.png");
            $("#defaultImage2").attr("src", "/assets/images/icons/default-image.png");

            $(".ErrorInvestigationDocumentConfirm_Compelete_Container").show();
            $(".ErrorInvestigationDocumentConfirm_Update_Container").hide();
        }
    });
}
function ValidateErrorInvestigationConfirm() {

    $.validator.addMethod("decimalWithThreeDigits", function (value, element) {
        if (value === "") return true; // để required tự xử lý

        // Biểu thức chính quy: cho phép số âm, số nguyên hoặc thập phân với tối đa 3 chữ số sau dấu '.'
        return /^-?\d+(\.\d{1,3})?$/.test(value);
    }, "Vui lòng nhập số hợp lệ (tối đa 3 chữ số thập phân).");


    $("#wrapper_table_error_investigation_confirm_form").validate({
        rules: {
            ErrorInvestigationConfirm_ErrorQuantity: {
                required: true,
                decimalWithThreeDigits: true
            },
            ErrorInvestigationConfirm_ErrorCategory: {
                required: true,
            },
            ErrorInvestigationConfirm_ErrorDetail: {
                required: true,
            }

        },
        messages: {
            ErrorInvestigationConfirm_ErrorQuantity: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập số lượng điều chỉnh."],
                decimalWithThreeDigits: window.languageData[window.currentLanguage]["Số lượng điều chỉnh không đúng định dạng."]
            },
            ErrorInvestigationConfirm_ErrorCategory: {
                required: window.languageData[window.currentLanguage]["Vui lòng chọn phân loại."]
            },
            ErrorInvestigationConfirm_ErrorDetail: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập chi tiết điều tra."]
            }
        }
    });

    $(".ErrorInvestigationConfirm_ErrorQuantity").on("input", function () {
        var value = $(this).val();

        // Xác định xem có dấu '-' ở đầu không
        var isNegative = value.startsWith("-");

        // Loại bỏ tất cả ký tự trừ số và dấu '.'
        value = value.replace(/[^0-9.]/g, '');

        // Chỉ giữ lại dấu '.' đầu tiên nếu có nhiều dấu '.'
        var parts = value.split('.');
        if (parts.length > 2) {
            value = parts[0] + '.' + parts.slice(1).join('');
            parts = value.split('.');
        }

        // Nếu có phần thập phân, chỉ cho phép tối đa 3 chữ số sau dấu '.'
        if (parts.length === 2) {
            parts[1] = parts[1].substring(0, 3);  // Giới hạn tối đa 3 chữ số sau dấu '.'
            value = parts[0] + '.' + parts[1];
        }

        // Nếu có dấu '-' ở đầu, thêm lại dấu '-' đó
        if (isNegative) {
            value = '-' + value;
        }

        // Cập nhật lại giá trị trong input
        $(this).val(value);
    });

    $(".ErrorInvestigationConfirm_ErrorQuantity").on("input", function () {
        if ($(this).val().length > maxLengthErrorInvestigationConfirmErrorQuantity) {
            $(this).val($(this).val().slice(0, maxLengthErrorInvestigationConfirmErrorQuantity));
        }
    });

}

function UploadImages() {
    // Xử lý click cho nút tải ảnh, tránh đăng ký nhiều lần
    $(".btn-UploadImage1").off("click").on("click", function (e) {
        e.preventDefault();
        $('#uploadImage1').click();
    });

    $(".btn-UploadImage2").off("click").on("click", function (e) {
        e.preventDefault();
        $('#uploadImage2').click();
    });

    // Sự kiện change cho input upload ảnh
    $('#uploadImage1').off("change").on("change", function (event) {
        const file = event.target.files[0];
        if (file) {
            // Kiểm tra nếu file không phải là hình ảnh
            if (!file.type.startsWith("image/")) {
                toastr.error(window.languageData[window.currentLanguage]["Chỉ được phép tải lên file ảnh!"]);
                $(this).val(""); // Reset input file
                return;
            }

            const reader = new FileReader();
            reader.onload = function (e) {
                $('#defaultImage1').attr('src', e.target.result);
            };
            reader.readAsDataURL(file);
        }
    });

    $('#uploadImage2').off("change").on("change", function (event) {
        const file = event.target.files[0];
        if (file) {
            // Kiểm tra nếu file không phải là hình ảnh
            if (!file.type.startsWith("image/")) {
                toastr.error(window.languageData[window.currentLanguage]["Chỉ được phép tải lên file ảnh!"]);
                $(this).val(""); // Reset input file
                return;
            }

            const reader = new FileReader();
            reader.onload = function (e) {
                $('#defaultImage2').attr('src', e.target.result);
            };
            reader.readAsDataURL(file);
        }
    });
}


function ErrorInvestigationDocumentView() {
    $(document).off("click", ".Investigation_Controls_Documents_ViewDetail").on("click", ".Investigation_Controls_Documents_ViewDetail", (e) => {
        let host = App.ApiGateWayUrl;
        var inventoryId = $(e.target).attr("data-inventoryid");

        var rowElement = $(e.target).closest('tr');
        var rowIndex = ErrorInvestigationDatatable.row(rowElement).index();
        var pageLength = ErrorInvestigationDatatable.page.info().length;
        var currentPage = ErrorInvestigationDatatable.page();
        var STT = (currentPage * pageLength) + rowIndex + 1;

        var rowData = ErrorInvestigationDatatable.row(rowElement).data();
        var componentCode = rowData?.componentCode;
        $.ajax({
            type: "GET",
            url: host + `/api/error-investigation/web/inventory/${inventoryId}/componentCode/${componentCode}/documents-check`,
            dataType: "json",
            success: function (res) {
                $("#ErrorInvestigationDocumentModal").modal("show");
                $('.ErrorInvestigationDocument_title').attr('data-inventoryid', inventoryId);
                /*$("#ErrorInvestigationDocument_Top_Detail").text(STT);*/
                $("#ErrorInvestigationDocument_ComponentCode_Detail").text(rowData?.componentCode);
                $("#ErrorInvestigationDocument_Position_Detail").text(rowData?.position);
                $("#ErrorInvestigationDocument_TotalQuantity_Detail").text(rowData?.totalQuantity);
                $("#ErrorInvestigationDocument_AccountQuantity_Detail").text(rowData?.accountQuantity);
                $("#ErrorInvestigationDocument_ErrorQuantity_Detail").text(rowData?.errorQuantity);
                $("#ErrorInvestigationDocument_ErrorMoney_Detail").text(rowData?.errorMoney);

                setTimeout(async () => {
                    InitListErrorInvestigationDocument_Datatable(rowData?.componentCode, inventoryId);  // Chờ hoàn thành
                }, 200);
            },
            error: function (error) {
                var err = error?.responseJSON;
                if (err?.code === 106) {
                    Swal.fire({
                        title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                        text: window.languageData[window.currentLanguage][err?.message],
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    })
                }
            }
        });

    })

}

function InitListErrorInvestigationDocument_Datatable(componentCode, inventoryId) {
    let host = App.ApiGateWayUrl;

    ErrorInvestigationDocumentDatatable = $('#ErrorInvestigationDocumentTable').DataTable({
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
            "url": host + `/api/error-investigation/web/inventory/${inventoryId}/componentCode/${componentCode}/documents`,
            "type": "POST",
            "contentType": "application/x-www-form-urlencoded",
            dataType: "json",
            data: function (data) {
                //Object.assign(data, dataFilter);

                return data;
            },
            "dataSrc": function ({ data }) {
                return data;
            }
        },
        "drawCallback": function (settings) {
            let totalPages = ErrorInvestigationDocumentDatatable.page.info().pages;
            let totalRecords = ErrorInvestigationDocumentDatatable.page.info().recordsTotal;


            let currPage = ErrorInvestigationDocumentDatatable.page() + 1;
            if (currPage == 1) {
                $("#ErrorInvestigationDocumentTable").find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $("#ErrorInvestigationDocumentTable").find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }
            if (currPage == totalPages) {
                $("#ErrorInvestigationDocumentTable").find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $("#ErrorInvestigationDocumentTable").find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }

            $("#ErrorInvestigationDocumentTable").find(".pagination").before(`<div class="h5 datatable_total_records"> <b>${window.languageData[window.currentLanguage]["Tổng"]}</b>: ${totalPages} <span class="total_record_seperate"> | </span>
                </div>`)

            if (totalRecords <= 10) {
                $(".container-list-view .bottom").hide()
            }

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
                    let currentPage = ErrorInvestigationDocumentDatatable.page() + 1;

                    let STT = ((currentPage - 1) * pagesize) + currentRow;

                    if (STT < 10) {
                        STT = `0${STT}`;
                    }
                    return STT;
                },
                "autoWidth": true
            },
            { "data": "docCode", "name": "Số phiếu", "autoWidth": true },
            { "data": "attachModule", "name": "Model code", "autoWidth": true },
            {
                "data": "componentCode", "name": "Mã linh kiện",
                "autoWidth": true
            },
            { "data": "bom", "name": "BOM", "autoWidth": true },
            { "data": "totalQuantity", "name": "Số lượng kiểm kê", "autoWidth": true },
            {
                "data": "assigneeAccount", "name": "Tài khoản phân phát",
                "autoWidth": true
            },
            {
                "data": "position", "name": "Vị trí",
                "autoWidth": true
            },
            {
                "data": "modelCode", "name": "Model đính kèm",
                "autoWidth": true
            },
            {
                "data": "",
                "name": "Xem chi tiết",
                "render": function (data, type, row) {
                    const selectHtmlSpecial = `
                        <div class="Controls_Container">
                            <div class="ErrorInvestigationDocument_Controls mx-3">
                                <a class="ErrorInvestigationDocument_ViewDetail" data-docId="${row.docId}">${window.languageData[window.currentLanguage]["Xem"]}</a>
                            </div>
                        </div>
                    `;
                    return selectHtmlSpecial;
                },
                "autoWidth": true
            }
            
        ],
        "rowCallback": function (row, data) {
            if (data.docCode.startsWith('C')) {
                $(row).addClass('highlight-orange');
            }
        }
    });
}

function ValidateSearchErrorInvestigation() {

    $("#ErrorInvestigation_Plant").on("input", function () {
        if ($(this).val().length > maxLengthCreateInventoryPlant) {
            $(this).val($(this).val().slice(0, maxLengthCreateInventoryPlant));
        }
    });

    $("#ErrorInvestigation_WHLoc").on("input", function () {
        if ($(this).val().length > maxLengthCreateInventoryWhLoc) {
            $(this).val($(this).val().slice(0, maxLengthCreateInventoryWhLoc));
        }
    });

    $("#ErrorInvestigation_ErrorQuantity_Qty_Start").maskMoney({
        allowZero: true,
        defaultZero: false,
        allowEmpty: true,
        allowNegative: true,
        precision: 0,
        selectAllOnFocus: false,
        bringCaretAtEndOnFocus: false,
        affixesStay: false
    });
    $("#ErrorInvestigation_ErrorQuantity_Qty_Start").on(
        "keypress",
        ValidateInputHelper.OnlyNumerOnKeyPress
    );
    $("#ErrorInvestigation_ErrorQuantity_Qty_Start").on(
        "keypress",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityFrom)
    );
    $("#ErrorInvestigation_ErrorQuantity_Qty_Start").on(
        "keyup",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityFrom)
    );

    $("#ErrorInvestigation_ErrorQuantity_Qty_End").maskMoney({
        allowZero: true,
        defaultZero: false,
        allowEmpty: true,
        allowNegative: true,
        precision: 0,
        selectAllOnFocus: false,
        bringCaretAtEndOnFocus: false,
        affixesStay: false
    });
    $("#ErrorInvestigation_ErrorQuantity_Qty_End").on(
        "keypress",
        ValidateInputHelper.OnlyNumerOnKeyPress
    );
    $("#ErrorInvestigation_ErrorQuantity_Qty_End").on(
        "keypress",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityTo)
    );
    $("#ErrorInvestigation_ErrorQuantity_Qty_End").on(
        "keyup",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityTo)
    );


    $("#ErrorInvestigation_ErrorMonney_Qty_Start").maskMoney({
        allowZero: true,
        defaultZero: false,
        allowEmpty: true,
        allowNegative: true,
        precision: 0,
        selectAllOnFocus: false,
        bringCaretAtEndOnFocus: false,
        affixesStay: false
    });
    $("#ErrorInvestigation_ErrorMonney_Qty_Start").on(
        "keypress",
        ValidateInputHelper.OnlyNumerOnKeyPress
    );
    $("#ErrorInvestigation_ErrorMonney_Qty_Start").on(
        "keypress",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityFrom)
    );
    $("#ErrorInvestigation_ErrorMonney_Qty_Start").on(
        "keyup",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityFrom)
    );

    $("#ErrorInvestigation_ErrorMonney_Qty_End").maskMoney({
        allowZero: true,
        defaultZero: false,
        allowEmpty: true,
        allowNegative: true,
        precision: 0,
        selectAllOnFocus: false,
        bringCaretAtEndOnFocus: false,
        affixesStay: false
    });
    $("#ErrorInvestigation_ErrorMonney_Qty_End").on(
        "keypress",
        ValidateInputHelper.OnlyNumerOnKeyPress
    );
    $("#ErrorInvestigation_ErrorMonney_Qty_End").on(
        "keypress",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityFrom)
    );
    $("#ErrorInvestigation_ErrorMonney_Qty_End").on(
        "keyup",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityFrom)
    );


    $("#ErrorInvestigation_ComponentCode").on("input", function () {
        if ($(this).val().length > maxLengthCreateInventoryComponentCode) {
            $(this).val($(this).val().slice(0, maxLengthCreateInventoryComponentCode));
        }
    });

    $("#ErrorInvestigation_AssigneeAccount").on("input", function () {
        if ($(this).val().length > maxLengthCreateInventoryUserDistribution) {
            $(this).val($(this).val().slice(0, maxLengthCreateInventoryUserDistribution));
        }
    });

    $("#ErrorInvestigation_ErrorQuantity_Qty_Start, #ErrorInvestigation_ErrorQuantity_Qty_End, #ErrorInvestigation_ErrorMonney_Qty_Start, #ErrorInvestigation_ErrorMonney_Qty_End").change(
        function (e) {
            $("#ErrorInvestigation_Search_Option_Form").valid()
        }
    );

    $("#ErrorInvestigation_Search_Option_Form").validate({
        rules: {
            ErrorInvestigation_ErrorQuantity_Qty_Start: {
                number: true,
                quantityRangeEmptyFromValidate: true,
                quantityRangeValidate: true,
            },
            ErrorInvestigation_ErrorQuantity_Qty_End: {
                number: true,
                quantityRangeEmptyToValidate: true,
                quantityRangeValidate: true,
            },
            ErrorInvestigation_ErrorMonney_Qty_Start: {
                number: true,
                moneyRangeEmptyFromValidate: true,
                moneyRangeValidate: true,
            },
            ErrorInvestigation_ErrorMonney_Qty_End: {
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

            let quantityFrom = $("#ErrorInvestigation_ErrorQuantity_Qty_Start")
                .val()
                .replaceAll(",", "");
            let quantityTo = $("#ErrorInvestigation_ErrorQuantity_Qty_End")
                .val()
                .replaceAll(",", "");
            
            if ((quantityFrom == "" && quantityTo != "" )) {
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

            let quantityFrom = $("#ErrorInvestigation_ErrorQuantity_Qty_Start")
                .val()
                .replaceAll(",", "");
            let quantityTo = $("#ErrorInvestigation_ErrorQuantity_Qty_End")
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

            let quantityFrom = $("#ErrorInvestigation_ErrorMonney_Qty_Start")
                .val()
                .replaceAll(",", "");
            let quantityTo = $("#ErrorInvestigation_ErrorMonney_Qty_End")
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

            let quantityFrom = $("#ErrorInvestigation_ErrorMonney_Qty_Start")
                .val()
                .replaceAll(",", "");
            let quantityTo = $("#ErrorInvestigation_ErrorMonney_Qty_End")
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

            let quantityFrom = $("#ErrorInvestigation_ErrorQuantity_Qty_Start")
                .val()
                .replaceAll(",", "");
            let quantityTo = $("#ErrorInvestigation_ErrorQuantity_Qty_End")
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

            let moneyFrom = $("#ErrorInvestigation_ErrorMonney_Qty_Start")
                .val()
                .replaceAll(",", "");
            let moneyTo = $("#ErrorInvestigation_ErrorMonney_Qty_End")
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

function ExportFileErrorInvestigation() {
    $(document).off("click", ".ErrorInvestigation_ExportExcel").on("click", ".ErrorInvestigation_ExportExcel", (e) => {
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
            Statuses: "",
            InventoryIds: "",
            ComponentName: "",
        };

        dataFilter.InventoryIds = $("#ErrorInvestigation_InventoryName").val();
        dataFilter.Plant = $("#ErrorInvestigation_Plant").val();
        dataFilter.WHLoc = $("#ErrorInvestigation_WHLoc").val();
        dataFilter.AssigneeAccount = $("#ErrorInvestigation_AssigneeAccount").val();
        dataFilter.ComponentCode = $("#ErrorInvestigation_ComponentCode").val();
        dataFilter.ErrorQuantityFrom = $("#ErrorInvestigation_ErrorQuantity_Qty_Start").val();
        dataFilter.ErrorQuantityTo = $("#ErrorInvestigation_ErrorQuantity_Qty_End").val();
        dataFilter.ErrorMoneyFrom = $("#ErrorInvestigation_ErrorMonney_Qty_Start").val();
        dataFilter.ErrorMoneyTo = $("#ErrorInvestigation_ErrorMonney_Qty_End").val();
        dataFilter.ErrorCategories = document.querySelector('#ErrorInvestigation_ErrorCategory').isAllSelected() ? "" : $("#ErrorInvestigation_ErrorCategory").val();
        dataFilter.Statuses = document.querySelector('#ErrorInvestigation_Status').isAllSelected() ? "" : $("#ErrorInvestigation_Status").val();
        dataFilter.ComponentName = $("#ErrorInvestigation_ComponentName").val();

        loading(true)

        var url = `/error-investigation/inventory/export`;
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

                    link.download = `Danhsachsaiso_${formattedTime}.xlsx`;
                    link.click();
                } else {
                    toastr.error(window.languageData[window.currentLanguage]["Không tìm thấy file."]);
                }
                toastr.success(window.languageData[window.currentLanguage]["Export danh sách sai số thành công."]);
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

function ShowMutilDropdown() {
    var dropdownSelectors = [
        `#ErrorInvestigation_InventoryName`,
        `#ErrorInvestigation_ErrorCategory`,
        `#ErrorInvestigation_Status`,
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

    $("#ErrorInvestigation_InventoryName")[0].reset();
    let currInventory = App?.User?.InventoryLoggedInfo?.InventoryModel?.InventoryId;
    let firstInventoryOption = $("#ErrorInvestigation_InventoryName")[0]?.options[0]?.value || "";
    $("#ErrorInvestigation_InventoryName")[0].setValue(currInventory || firstInventoryOption);


    $("#ErrorInvestigation_ErrorCategory")[0].reset();
    $("#ErrorInvestigation_ErrorCategory")[0].toggleSelectAll(true);

    $("#ErrorInvestigation_Status")[0].reset();
    $("#ErrorInvestigation_Status")[0].toggleSelectAll(true);

}

function attachErrorDetailTooltips() {
    const $cells = $('#Error_Investigation_DataTable').find('span.error-detail-cell');

    // Xoá tooltip cũ để tránh nhân bản khi table redraw
    $cells.tooltip('dispose');

    // Khởi tạo tooltip Bootstrap
    $cells.tooltip({
        trigger: 'click',     // ⇒ click mới hiện; dùng 'hover' nếu muốn
        placement: 'auto',
        container: 'body',
        html: true
    });

    // Đóng tooltip khi click ra ngoài
    $(document).off('click.closeErrorDetail')
        .on('click.closeErrorDetail', function (e) {
            if (!$(e.target).closest('span.error-detail-cell').length) {
                $('span.error-detail-cell').tooltip('hide');
            }
        });
}



function InitListErrorInvestigation_Datatable() {
    let host = App.ApiGateWayUrl;
    var inventoryId = App.User.InventoryLoggedInfo.InventoryModel.InventoryId;

    ErrorInvestigationDatatable = $('#Error_Investigation_DataTable').DataTable({
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
        "ordering": true,
        order: [[9, "desc"]],
        "ajax": {
            "url": host + `/api/error-investigation/web/inventory`,
            "type": "POST",
            "contentType": "application/x-www-form-urlencoded",
            dataType: "json",
            data: function (data) {
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
                    Statuses: "",
                    InventoryIds: "",
                    ComponentName: "",
                };

                dataFilter.InventoryIds = $("#ErrorInvestigation_InventoryName").val();
                dataFilter.Plant = $("#ErrorInvestigation_Plant").val();
                dataFilter.WHLoc = $("#ErrorInvestigation_WHLoc").val();
                dataFilter.AssigneeAccount = $("#ErrorInvestigation_AssigneeAccount").val();
                dataFilter.ComponentCode = $("#ErrorInvestigation_ComponentCode").val();
                dataFilter.ErrorQuantityFrom = $("#ErrorInvestigation_ErrorQuantity_Qty_Start").val();
                dataFilter.ErrorQuantityTo = $("#ErrorInvestigation_ErrorQuantity_Qty_End").val();
                dataFilter.ErrorMoneyFrom = $("#ErrorInvestigation_ErrorMonney_Qty_Start").val();
                dataFilter.ErrorMoneyTo = $("#ErrorInvestigation_ErrorMonney_Qty_End").val();
                dataFilter.ErrorCategories = document.querySelector('#ErrorInvestigation_ErrorCategory').isAllSelected() ? "" : $("#ErrorInvestigation_ErrorCategory").val();
                dataFilter.Statuses = document.querySelector('#ErrorInvestigation_Status').isAllSelected() ? "" : $("#ErrorInvestigation_Status").val();
                dataFilter.ComponentName = $("#ErrorInvestigation_ComponentName").val();


                Object.assign(data, dataFilter);

                return data;
            },
            "dataSrc": function ({ data }) {
                return data;
            }
        },
        "drawCallback": function (settings) {
            let totalPages = ErrorInvestigationDatatable.page.info().pages;
            let totalRecords = ErrorInvestigationDatatable.page.info().recordsTotal;


            let currPage = ErrorInvestigationDatatable.page() + 1;
            if (currPage == 1) {
                $("#Error_Investigation_DataTable").find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $("#Error_Investigation_DataTable").find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }
            if (currPage == totalPages) {
                $("#Error_Investigation_DataTable").find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $("#Error_Investigation_DataTable").find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }

            $("#Error_Investigation_DataTable").find(".pagination").before(`<div class="h5 datatable_total_records"> <b>${window.languageData[window.currentLanguage]["Tổng"]}</b>: ${totalPages} <span class="total_record_seperate"> | </span>
                </div>`)

            if (totalRecords <= 10) {
                $(".container-list-view .bottom").hide()
            }

            attachErrorDetailTooltips();

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
                    let currentPage = ErrorInvestigationDatatable.page() + 1;

                    let STT = ((currentPage - 1) * pagesize) + currentRow;

                    if (STT < 10) {
                        STT = `0${STT}`;
                    }
                    return STT;
                },
                "autoWidth": true,
                "sortable": false
            },
            {
                "data": "",
                "name": "Điều tra",
                "render": function (data, type, row) {
                    const selectHtmlSpecial = `
                        <div class="Controls_Container">
                            <div class="Investigation_Controls mx-3">
                                <a class="Investigation_Controls_Documents_ViewDetail" data-inventoryId="${row.inventoryId}"> ${window.languageData[window.currentLanguage]["Chi tiết"]}</a>
                            </div>
                        </div>
                    `;
                    return selectHtmlSpecial;
                },
                "autoWidth": true, "sortable": false
            },
            { "data": "inventoryName", "name": "Đợt kiểm kê", "autoWidth": true, "sortable": false },
            { "data": "plant", "name": "Plant", "autoWidth": true },
            { "data": "whLoc", "name": "WHLoc", "autoWidth": true },
            {
                "data": "componentCode", "name": "ComponentCode",
                "autoWidth": true
            },
            { "data": "position", "name": "Vị trí", "autoWidth": true, "sortable": false },
            {
                "data": "totalQuantity", "name": "Số lượng kiểm kê", render: function (data) {
                    if (data > 0) {
                        return ValidateInputHelper.Utils.convertDecimalInventory(data);
                    }
                    return data;
                } , "autoWidth": true, "sortable": false },
            {
                "data": "accountQuantity", "name": "Số lượng hệ thống", render: function (data) {
                    if (data > 0) {
                        return ValidateInputHelper.Utils.convertDecimalInventory(data);
                    }
                    return data;
                } , "autoWidth": true, "sortable": false
            },
            {
                "data": "errorQuantity",
                "name": "ErrorQuantity",
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
                        return `<div class="color-red">(${ValidateInputHelper.Utils.convertDecimalInventory(Math.abs(data))})</div>`;
                    }
                    return `<div>${ValidateInputHelper.Utils.convertDecimalInventory(data)}</div>`;
                },
                "autoWidth": true, "sortable": false
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
                "autoWidth": true, "sortable": false
            },
            {
                "data": "investigationQuantity", "name": "Số lượng điều chỉnh", render: function (data) {
                    if (data > 0) {
                        return ValidateInputHelper.Utils.convertDecimalInventory(data);
                    }
                    return data;
                }
                ,"autoWidth": true, "sortable": false
            },
            {
                "data": "errorCategoryName", "name": "Phân loại lỗi", "autoWidth": true, "sortable": false
            },
            {
                data: "errorDetail",
                name: "Nguyên nhân sai số",
                sortable: false,
                autoWidth: true,
                render: function (data, type) {
                    if (type !== 'display') return data;              // sort / filter => trả thô
                    // escape để tránh XSS
                    const safe = $('<div>').text(data ?? '').html();
                    return `<span class="error-detail-cell" title="${safe}">${safe}</span>`;
                }
            },
            {
                "data": "investigator", "name": "Người điều tra",
                "autoWidth": true, "sortable": false
            },
            {
                "data": "investigationTotal", "name": "Tổng số lượng điều tra", render: function (data) {
                    if (data > 0) {
                        return ValidateInputHelper.Utils.convertDecimalInventory(data);
                    }
                    return data;
                }
                ,"autoWidth": true, "sortable": false
            },
            {
                "data": "status", "name": "Trạng thái", render: function (data, type, row, index) {
                    if (data || data == 0) {
                        let textStatus = data == 0 ? window.languageData[window.currentLanguage]["Chưa điều tra"] : data == 1 ? window.languageData[window.currentLanguage]["Đang điều tra"] : window.languageData[window.currentLanguage]["Đã điều tra"];
                        let textColor = data == 0 ? "color-grey-status" : data == 1 ? "color-red-status" : "color-green-status";
                        return `<div class="${textColor}" txt-bolder">${textStatus}</div>`;
                    }
                    return ``;
                }, "autoWidth": true, "sortable": false
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
                "autoWidth": true, "sortable": false
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
                "autoWidth": true, "sortable": false
            },
            {
                "data": "componentName", "name": "Tên linh kiện",
                "autoWidth": true, "sortable": false
            },
            {
                "data": "investigationHistoryCount", "name": "Lịch sử điều chỉnh",
                render: function (data, type, row, index) {
                    if (data) {
                        return `<div class="color-blue">${data}</div>`;
                    }
                    return ``;
                },
                "autoWidth": true, "sortable": false
            },
            {
                "data": "noteDocumentTypeA", "name": "Ghi chú",
                "autoWidth": true, "sortable": false
            }

            
        ],
    });
}

function PrintPDFDocumentDetail() {
    $(document).off("click", ".btn-PrintDocDetail").on("click", ".btn-PrintDocDetail", (e) => {
        //Lấy ngày tháng hiện tại theo định dạng: yyyymmdd_hhmmss:
        var currentTime = new Date();
        var formattedTime = moment(currentTime).format("YYYYMMDD_HHmmss");

        // Ẩn các nút không muốn in:
        $(".btn-PrintDocDetail").hide()

        let isOverFlowWidth = false;
        let tableWidth = $("#inventory-document-detail-modal .modal-content").outerWidth();


        let body = document.body
        let html = document.documentElement
        let height = Math.max(body.scrollHeight, body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight)
        let target = document.querySelector('#inventory-document-detail-modal .modal-content')
        let heightCM = height / 20;

        let maxScreenWidth = Math.max(tableWidth);
        let cacheBodyScreen = $('#inventory-document-detail-modal .modal-content').outerWidth();
        $('#inventory-document-detail-modal .modal-content').width(maxScreenWidth);
        //window.departmentReportChart.resize();
        $('#inventory-document-detail-modal .modal-content').css("margin", "auto");


        loading(true);
        html2pdf(target, {
            margin: 0,
            filename: `Chitietphieu_${formattedTime}.pdf`,
            html2canvas: { dpi: 200, letterRendering: false, scale: 2 },
            jsPDF: {
                orientation: 'portrait',
                unit: 'cm',
                //format: [isOverFlowWidth ? (heightCM + 30) : heightCM, isOverFlowWidth ? 100 : 60],
                format: 'a4',
                compress: true,
                precision: 16
            }
        }).then(function () {
            loading(false);
            // Ẩn các nút không muốn in:
            $(".btn-PrintDocDetail").show()
        })

    })
}

//Error Investigation Document View Detail:

var ErrorInvestigationDocumentDetailHandler = (function () {
    
    let root = {
        parentEl: $("#ErrorInvestigationDocumentModal")
    }

    let self = new ReceiveView();
    ko.applyBindings(self, document.querySelector(".table_controls"));


    let InventoryListDatable;
    let dataFilter = {};
    let inventoryDocumentDetailViewModel;
    let formValidator;

    function Cache() {
        root.btn_upload_status_list_inventory = $(root.parentEl).find("#upload-status-list-inventory");
        root.inputFile_UploadDocStatus = $(root.parentEl).find("#inputFile_UploadDocStatus");

        root.searchForm = $(root.parentEl).find("#input_storage_search_form");
    }

    function PreLoad() {
        self.receive_CheckedIds([]);
        self.receiveAll_UncheckedIds([]);
        inventoryDocumentDetailViewModel = new InventoryDocumentDetailViewModel();
        ko.applyBindings(inventoryDocumentDetailViewModel, document.querySelector("#inventory-document-detail-modal"));

        //Khi vào màn danh sách phiếu
        let canPerformUpdateStatus = isPromoter() ||
            (!isPromoter() && App.User.isGrant("EDIT_INVENTORY") && isInCurrentInventory());

        if (!canPerformUpdateStatus) {
            $("#download-file-list-inventory").remove();
            $("#upload-status-list-inventory").remove();
        }
    }

    function Events() {
        
        //Xem chi tiết phiếu
        root.parentEl.delegate(".ErrorInvestigationDocument_ViewDetail", "click", function (e) {
            let thisBtn = $(e.target).closest(".ErrorInvestigationDocument_ViewDetail");
            $("#ErrorInvestigationDocumentModal").modal('hide');
            $("#inventory-document-detail-modal").modal('show');

            let docId = $(thisBtn).attr("data-docid");
            inventoryDocumentDetailViewModel.loadDocDetail(docId);
        });

        $(document).off("click", "#inventory-document-detail-modal button.btn-close").on("click", "#inventory-document-detail-modal button.btn-close", (e) => {
            $("#ErrorInvestigationDocumentModal").modal('show');
            $("#inventory-document-detail-modal").modal('hide');
        });

    }

    function ReloadDatatable(keepCurrentPage = false) {
        if (keepCurrentPage) {
            InventoryListDatable.ajax.reload(null, false);
        } else {
            InventoryListDatable.draw();
        }
    }

    function Init() {
        Cache()
        PreLoad()
        Events()
    }

    return {
        init: Init,
        drawDatatable: ReloadDatatable
    }
})();



function DocumentDetailModel(model) {

    var self = this;

    self.investigator = ko.observable(model?.investigator);
    self.investigateTime = ko.observable(model?.investigateTime);
    self.reasonInvestigator = ko.observable(model?.reasonInvestigator);

    self.inventoryId = ko.observable(model?.inventoryId);
    self.documentId = ko.observable(model?.documentId);
    self.inventoryName = ko.observable(model?.inventoryName);
    self.inventoryDate = ko.observable(model?.inventoryDate);
    self.componentCode = ko.observable(model?.componentCode);
    self.componentName = ko.observable(model?.componentName);
    self.docCode = ko.observable(model?.docCode);
    self.docType = ko.observable(model?.docType);
    self.status = ko.observable(model?.status);
    self.positionCode = ko.observable(model?.positionCode);
    self.assemblyLoc = ko.observable(model?.assemblyLoc);
    self.locationName = ko.observable(model?.locationName);
    self.departmentName = ko.observable(model?.departmentName);
    self.quantity = ko.observable((model?.quantity == null || model?.quantity == undefined) ? "" : model.quantity);
    self.plant = ko.observable(model?.plant);
    self.whLoc = ko.observable(model?.whLoc);
    self.note = ko.observable(model?.note);
    self.vendorCode = ko.observable(model?.vendorCode);
    self.columnQ = ko.observable(model?.columnQ);
    self.columnN = ko.observable(model?.columnN);
    self.columnR = ko.observable(model?.columnR);
    self.columnS = ko.observable(model?.columnS);
    self.columnO = ko.observable(model?.columnO);
    self.columnC = ko.observable(model?.columnC);
    self.columnP = ko.observable(model?.columnP);
    self.saleOrderList = ko.observable(model?.saleOrderList);
    self.saleOrderNo = ko.observable(model?.saleOrderNo);
    self.physInv = ko.observable(model?.physInv);
    self.fiscalYear = ko.observable(model?.fiscalYear);
    self.item = ko.observable(model?.item);
    self.plannedCountDate = ko.observable(model?.plannedCountDate);
    self.sapInventoryNo = ko.observable(model?.sapInventoryNo);
    self.stockType = ko.observable(model?.stockType);
    self.specialStock = ko.observable(model?.specialStock);
    self.modelCode = ko.observable(model?.modelCode);
    self.createdAt = ko.observable(model?.createdAt);
    self.createdBy = ko.observable(model?.createdBy);
    self.inventoryAt = ko.observable(model?.inventoryAt);
    self.inventoryBy = ko.observable(model?.inventoryBy);
    self.confirmedAt = ko.observable(model?.confirmedAt);
    self.confirmedBy = ko.observable(model?.confirmedBy);
    self.auditedAt = ko.observable(model?.auditedAt);
    self.auditedBy = ko.observable(model?.auditedBy);
    self.assigneeAccount = ko.observable(model?.assigneeAccount);

    self.receivedAt = ko.observable(model?.receivedAt);
    self.receivedBy = ko.observable(model?.receivedBy);

    self.docComponentABEs = ko.observableArray(model?.docComponentABEs || []);
    self.docComponentCs = ko.observableArray(model?.docComponentCs || []);
    self.docHistories = ko.observableArray(model?.docHistories || []);


    self.existInventoryImage = ko.observable(false);
    self.envicenceImage = ko.observable(model?.envicenceImage ? `${AppUser.getApiGateway}/${model.envicenceImage.replaceAll("\\", "/")}` : null);
    self.envicenceImageTitle = ko.observable(model?.envicenceImageTitle || null);

    IsImageExist(self.envicenceImage(), (valid) => {
        self.existInventoryImage(valid);
    });
}

function DocTypeCModel(model) {
    let self = this;

    self.quantityPerBom = ko.observable(model?.quantityPerBom || "");
    self.quantityOfBom = ko.observable(model?.quantityOfBom || "");
}

function InventoryDocumentDetailViewModel(documentId) {
    let host = App.ApiGateWayUrl;
    var self = this;
    var docCDatatable;
    let zoomistSelector;

    self.detailModel = ko.observable(new DocumentDetailModel(null));
    self.canReceiveDoc = ko.observable(false);
    self.documentId = ko.observable(null);

    let APIs = {
        UpdateStatus: function (docId) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/web/update-status`;
                let docIds = [docId];

                $.ajax({
                    type: 'PUT',
                    url: url,
                    contentType: 'application/json',
                    data: JSON.stringify(docIds),
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
    }

    self.totalComponentsQuantity = ko.computed(function () {
        let total = ko.unwrap(self.detailModel().docComponentABEs).reduce((acc, curr) => {
            let cal = curr.quantityOfBom * curr.quantityPerBom;
            return acc + cal;
        }, 0);

        //Check quyền ẩn hiện nút tiếp nhận mỗi khi mở modal
        var currUser = App.User;
        //Nếu là tài khoản chung và có quyền chỉnh sửa thì bật
        let validRole = App.User.isGrant("EDIT_INVENTORY") && isInCurrentInventory() &&
            (App.User.InventoryLoggedInfo.InventoryRoleType == InventoryRoleType.KiemKe && App.User.InventoryLoggedInfo.HasRoleType);

        self.canReceiveDoc(validRole);

        return total;
    });


    self.convertedStatus = function (val) {
        return InventoryDocStatus[val] ?? "";
    }

    self.handleReceiveDoc = function (e) {
        loading(true);
        APIs.UpdateStatus(self.detailModel().documentId).then(res => {
            //Alert success message
            toastr.success(res?.message);

            //Reload current document detail
            self.loadDocDetail(self.detailModel().documentId);

            //Re-render datable list
            ErrorInvestigationDocumentDetailHandler.drawDatatable(true);
        }).catch(err => {
            toastr.error(err?.message || `Có lỗi khi thực hiện tiếp nhận phiếu`);
        }).finally(() => {
            loading(false);
        })
    }

    self.loadDocDetail = function (documentId) {
        let url = `${host}/api/inventory/web/document/${documentId}`;
        $.get(url, (res) => {
            self.detailModel(new DocumentDetailModel(res.data));
            self.documentId(documentId);

            let hasDocC = res.data.docComponentCs.length > 0;
            self.hasComponentDoc_C(hasDocC);

            //Nếu phiếu C thì mới load Datatable
            if (res.data.docType == 3) {
                new InitDocCTable()
            }
        });
    }

    self.searchTerm = ko.observable();

    self.hasComponentDoc_C = ko.observable(false);

    self.handleKeyUp = ValidateInputHelper.Utils.debounce((data, event) => {
        if (event.keyCode === 13) {
            self.searchHandle();
        }
        return true;
    }, 400);

    self.searchHandle = function (e) {
        let searchValue = self.searchTerm();
        let documentId = self.detailModel().documentId;

        docCDatatable.draw();
    }

    self.changeLogModelHtml = function ({ changeLogModel }) {
        let { oldQuantity, newQuantity, newStatus, oldStatus, isChangeCDetail } = changeLogModel;
        return DisplayChangeLogHistory(oldQuantity, newQuantity, oldStatus, newStatus, isChangeCDetail);
    }

    self.viewInventoryImage = function (item) {
        let imgSrc = self.detailModel().envicenceImage();
        let imgTitle = self.detailModel().envicenceImageTitle();

        let imageModal = $("#InventoryHistoryViewImage");
        imageModal.modal("show");

        imageModal.find(".img_inventory_zoom #image_title").text(imgTitle);
        document.querySelector(".history_detail_image").setAttribute("data-zoomist-src", imgSrc);

        setTimeout(() => {
            if (zoomistSelector) {
                zoomistSelector.destroy();
            }
            const myZoomist = document.querySelector(".history_detail_image")
            zoomistSelector = new Zoomist(myZoomist, {
                slider: {
                    direction: 'vertical'
                },
                fill: 'contain',
                height: '70%',
                zoomer: {
                    inEl: false,
                    outEl: false,
                    disableOnBounds: false
                },
                zoomRatio: 0.1,
                draggable: true,
                wheelable: true,
            })

            zoomistSelector.slideTo(50);
        }, 200);
    }


    $("#inventory_doc_list_searchForm").on("submit", function (e) {
        e.preventDefault();
    })

    $("#btnSearchDocList_Detail").on("keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(10));
    $("#btnSearchDocList_Detail").on("keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(10));

    //Chặn nhập khoảng trắng và cắt khoảng trắng khi paste
    $("#btnSearchDocList_Detail").on("keypress", ValidateInputHelper.PreventWhiteSpace);
    $("#btnSearchDocList_Detail").on("keyup", ValidateInputHelper.RemoveWhiteSpaceOnKeyup);

    //Validate search form
    $("#inventory_doc_list_searchForm").validate({
        rules: {
            'btnSearchDocList_Detail': {
                maxlength: 10
            }
        },
        messages: {
            'btnSearchDocList_Detail': {
                maxlength: "Cho phép tối đa 10 ký tự."
            }
        }
    })

    function InitDocCTable() {
        docCDatatable = $(".docdetail_components_c").DataTable({
            "bDestroy": true,
            "processing": `<div class="spinner"></div>`,
            pagingType: 'full_numbers',
            'language': {
                'loadingRecords': `<div class="spinner"></div>`,
                'processing': '<div class="spinner"></div>',
            },
            colReorder: true,
            select: true,
            "serverSide": true,
            "filter": true,
            "searching": false,
            responsive: true,
            "lengthMenu": [10, 30, 50, 200],
            dom: 'rt<"bottom"flp><"clear">',
            "ordering": false,
            "ajax": {
                "url": `${host}/api/inventory/web/doc-detail/components-c`,
                "type": "POST",
                "contentType": "application/x-www-form-urlencoded",
                dataType: "json",
                data: function (data) {
                    data.documentId = self.documentId();
                    data.search = $("#btnSearchDocList_Detail").val();
                    return data;
                },
                "dataSrc": function ({ data }) {
                    return data;
                }
            },
            "drawCallback": function (settings) {
                let totalPages = docCDatatable.page.info().pages;
                let totalRecords = docCDatatable.page.info().recordsTotal;

                let currPage = docCDatatable.page() + 1;
                if (currPage == 1) {
                    $(".docdetail_components_c").find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                    $(".docdetail_components_c").find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                }
                if (currPage == totalPages) {
                    $(".docdetail_components_c").find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                    $(".docdetail_components_c").find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                }

                $(".docdetail_components_c").find(".pagination").before(`<div class="h5 datatable_total_records"> <b>Tổng</b>: ${totalPages} <span class="total_record_seperate"> | </span>
                </div>`)

                if (totalPages == 1) {
                    $("#inventory-detail-table_paginate").hide();
                }
            },
            "columns": [
                {
                    "data": "",
                    "name": "STT",
                    "render": function (data, type, row, index) {
                        let pagesize = index.settings._iDisplayLength;
                        let currentRow = ++index.row;
                        let currentPage = docCDatatable.page() + 1;

                        let STT = ((currentPage - 1) * pagesize) + currentRow;

                        if (STT < 10) {
                            STT = `0${STT}`;
                        }
                        return STT;
                    },
                    "autoWidth": true
                },
                {
                    "data": "componentCode", "name": "componentCode", render: function (data, type, row, index) {
                        return data || row.modelCode;
                    }
                },
                { "data": "quantityOfBom", "name": "quantityOfBom", "autoWidth": true },
                { "data": "quantityPerBom", "name": "quantityPerBom", "autoWidth": true },
            ],
        });
    }

    return self;
}


function ReceiveView() {
    let self = this;

    self.receive_CheckedIds = ko.observableArray([]);
    self.receiveAll_UncheckedIds = ko.observableArray([]);

    self.notReceiveDocsCount = ko.observable(0);

    //Chọn toàn bộ phiếu
    self.receiveAll = ko.observable(false);

    self.receiveAll_UncheckedIds.subscribe(function (arr) {

    })

    self.receiveAll.subscribe(function (isChecked) {
        if (!isChecked) {
            self.receive_CheckedIds([]);
            self.receiveAll_UncheckedIds([]);

            $(".InventoryDocument_check").prop("checked", false).change();
        }
    })

    self.enableReceiveBtn = ko.computed(function () {

        let enable = (self.receiveAll() && (self.notReceiveDocsCount() - self.receiveAll_UncheckedIds().length != 0)) || (self.receive_CheckedIds().length > 0);
        if (enable) {
            $("#receive-list-inventory").removeClass("btn_disabled");
        } else {
            $("#receive-list-inventory").removeClass("btn_disabled").addClass("btn_disabled");
        }

        return enable;
    })

    return self;
}