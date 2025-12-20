$(function () {
    FitAuditMobileTopBar();
})

// Khai báo toàn cục
let InventoryDocStatusTitle = {};
let InventoryDocStatus = {};
let InventoryStatusTitle = {};
let ServerStatusCodeMessages = {};

GlobalSettingLanguageData();
AjaxSetupToken();
CommonJqueryValidationMethod();
ConfigKnockoutJs();

const reloadingPageDelay = 1500;

function AjaxSetupToken() {
    $.ajaxSetup({
        beforeSend: function (xhr) {
            xhr.setRequestHeader('Authorization', `${App.Token}`);
        },
        statusCode: {
            401: function (xhr) {
                let response = xhr.responseJSON;
                xhr.setRequestHeader('Authorization', ``);

                Swal.fire({
                    title: `<b>Thông báo</b>`,
                    text: `${response?.message}`,
                    confirmButtonText: "Đã hiểu",
                    allowOutsideClick: false,
                    width: '30%'
                }).then((result, e) => {
                    result;
                    if (result.isConfirmed || !result.isConfirmed) {
                        window.location.reload();
                    }
                });
            }
        }
    });
}

function GlobalSettingLanguageData() {
    if (window.languageData && window.currentLanguage) {
        console.log("languages is ready!");
        DatatableSettings();

        InventoryDocStatusTitle = {
            NotInventoryYet: window.languageData[window.currentLanguage]["Chưa kiểm kê"],
            WaitingConfirm: window.languageData[window.currentLanguage]["Chờ xác nhận"],
            MustEdit: window.languageData[window.currentLanguage]["Cần chỉnh sửa"],
        };


        InventoryDocStatus = {
            0: window.languageData[window.currentLanguage]['Chưa tiếp nhận'],
            1: window.languageData[window.currentLanguage]['Không kiểm kê'],
            2: InventoryDocStatusTitle.NotInventoryYet,
            3: InventoryDocStatusTitle.WaitingConfirm,
            4: InventoryDocStatusTitle.MustEdit,
            5: window.languageData[window.currentLanguage]['Đã xác nhận'],
            6: window.languageData[window.currentLanguage]['Đã đạt giám sát'],
            7: window.languageData[window.currentLanguage]['Không đạt giám sát'],
        };

        InventoryStatusTitle = {
            0: "",
            1: "",
            2: "",
            3: window.languageData[window.currentLanguage]["Người kiểm kê"],
            4: window.languageData[window.currentLanguage]["Người kiểm kê"],
            5: window.languageData[window.currentLanguage]["Người xác nhận"],
            6: window.languageData[window.currentLanguage]["Người giám sát"],
            7: window.languageData[window.currentLanguage]["Người giám sát"],
        }

        ServerStatusCodeMessages = {
            LoggedInOtherDevice: window.languageData[window.currentLanguage]["Tài khoản của bạn đã được đăng nhập ở một thiết bị khác. Vui lòng đăng nhập lại."]
        }

    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        console.log("languages is not ready!");
        setTimeout(GlobalSettingLanguageData, 10); // Chờ thêm 100ms rồi kiểm tra lại
    }
}

function DatatableSettings() {
    
    var displayText = window.languageData[window.currentLanguage]["Hiển thị"];
    var noDataText = window.languageData[window.currentLanguage]["Không có dữ liệu"];
    
    $.fn.dataTable.ext.errMode = 'none';
    //Pagination
    $.extend(true, $.fn.dataTable.defaults.oLanguage.oPaginate, {
        sNext: `<svg xmlns="http://www.w3.org/2000/svg" width="19" height="19" viewBox="0 0 24 24" fill="none">
                  <path d="M8.90997 19.9201L15.43 13.4001C16.2 12.6301 16.2 11.3701 15.43 10.6001L8.90997 4.08008" stroke="#87868C" stroke-width="1.5" stroke-miterlimit="10" stroke-linecap="round" stroke-linejoin="round"/>
                </svg>`,
        sPrevious: `<svg xmlns="http://www.w3.org/2000/svg" width="19" height="19" viewBox="0 0 24 24" fill="none">
                      <path d="M15 19.9201L8.47997 13.4001C7.70997 12.6301 7.70997 11.3701 8.47997 10.6001L15 4.08008" stroke="#87868C" stroke-width="1.5" stroke-miterlimit="10" stroke-linecap="round" stroke-linejoin="round"/>
                    </svg>`,
        sFirst: `<svg xmlns="http://www.w3.org/2000/svg" width="19" height="19" viewBox="0 0 24 24" fill="none">
                  <path d="M18 19.84L11.48 13.32C10.71 12.55 10.71 11.29 11.48 10.52L18 4" stroke="#87868C" stroke-width="1.5" stroke-miterlimit="10" stroke-linecap="round" stroke-linejoin="round"/>
                  <path d="M6.90247 4L6.90247 19.84" stroke="#87868C" stroke-width="1.5" stroke-linecap="round"/>
                </svg>`,
        sLast: `<svg xmlns="http://www.w3.org/2000/svg" width="19" height="19" viewBox="0 0 24 24" fill="none">
                  <path d="M6 19.84L12.52 13.32C13.29 12.55 13.29 11.29 12.52 10.52L6 4" stroke="#87868C" stroke-width="1.5" stroke-miterlimit="10" stroke-linecap="round" stroke-linejoin="round"/>
                  <path d="M17.0975 4L17.0975 19.84" stroke="#87868C" stroke-width="1.5" stroke-linecap="round"/>
                </svg>`
    });

    //Loading indicator
    $.extend(true, $.fn.dataTable.defaults, {
        "language": {
            "processing": `<div class="spinner"></div>`,
            "emptyTable": noDataText

        },
        "paging": true,
        "pagingType": "full_numbers",
        initComplete: function (settings, data) {
            let tableId = settings.sTableId;
            let datatableLength = $(`#${tableId}_length`);

            let totalPages = Math.ceil(settings.fnRecordsDisplay() / settings._iDisplayLength);
            let tableWrapper = $(settings.nTableWrapper);

            let optionValues = settings.aLengthMenu;
            let length = settings._iDisplayLength;

            let resultHtml = optionValues.map((val, i) => {
                return `<option value="${val}">${displayText} ${val}</option>`
            }).join('')

            let selectElement = datatableLength.find("select");
            selectElement.html(`
                    ${resultHtml}
                `)
            selectElement.val(length).change();

            let label = datatableLength.contents().eq(0);
            $(label).contents().each((i, el) => {
                if ($(el).is("select") == false) {
                    $(el).remove()
                }
            })
        },
        "preDrawCallback": function (oSettings) {
            let totalPages = Math.ceil(oSettings.fnRecordsDisplay() / oSettings._iDisplayLength);
            let tableWrapper = $(oSettings.nTableWrapper);
            if (totalPages <= 1) {
                tableWrapper.find('.dataTables_paginate').css("opacity", 0)
            } else {
                tableWrapper.find('.dataTables_paginate').css("opacity", 1)
            }

            if (oSettings.fnRecordsDisplay() == 0) {
                tableWrapper.find('.bottom').hide();
            } else {
                tableWrapper.find('.bottom').show();
            }

            let records = oSettings.fnRecordsDisplay();


            if (records < 10) {
                setTimeout(() => {
                    tableWrapper.find(".dataTables_length").hide();
                }, 10)
            } else {
                tableWrapper.find(".dataTables_length").show();
            }

            if (records == 0) {
                setTimeout(() => {
                    tableWrapper.find(".dataTables_scrollBody").css("overflow", "hidden");
                    let tableScroll = tableWrapper.find(".dataTables_scroll");

                    let emptyElement = tableWrapper.find(".dataTables_empty");
                    let emptyText = emptyElement.text();

                    let emptyDataElement = `<div class="table_empty_text txt-12 py-2" style="text-align: center" >${emptyText}</div>`
                    tableScroll.find(".table_empty_text").remove();
                    tableScroll.append(emptyDataElement)
                    tableScroll.find(".dataTables_scrollBody table tbody").hide();
                }, 10)
            } else {
                tableWrapper.find(".dataTables_scrollBody").css("overflow", "auto");
                let tableScroll = tableWrapper.find(".dataTables_scroll");
                tableScroll.find(".dataTables_scrollBody table tbody").show();
                tableScroll.find(".table_empty_text").remove();
            }
        },
    });
}


function CommonJqueryValidationMethod() {
    const dateFormat = "DD/MM/YYYY";

    jQuery.validator.addMethod("validDateFormat", function (value, element, params) {
        let convertedDate = moment(value, dateFormat, true);
        if (value) {
            return convertedDate.isValid();
        }
        return true;
    }, 'Sai định dạng, vui lòng nhập lại.');

    jQuery.validator.addMethod("noSpace", function (value, element) {
        return value.indexOf(" ") < 0 && value != "";
    }, "Vui lòng không để khoảng trắng.");
}

;(function ($) {
    var o = $({});

    $.sub = function () {
        o.on.apply(o, arguments);
    };

    $.unsub = function () { 
        o.off.apply(o, arguments); 
    };

    $.pub = function () {  
        o.trigger.apply(o, arguments);
    };

}(jQuery));

;function ReloadPage(delay = reloadingPageDelay) {
    setTimeout(() => {
        window.location.reload();
    }, delay)
}
function ForceLogout(roleId) {
    return new Promise(async (resolve, reject) => {
        let url = `${host}/api/identity/force_logout`

        try {
            const res = await $.ajax({
                url: url,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(roleId)
            });
            resolve(res)
        } catch (err) {
            reject(err)
        }
    })
}
;toastr.options = {
    maxOpened: 1,
    preventDuplicates: 1,
    autoDismiss: true
};


;const numberConfig = {
    maxCharacterLength: 8,
    maximumValue: 99999999,
    decimalPlaces: 6,
    minimumValue: 0,
    initAutoNumeric: function (selector) {
        new AutoNumeric(selector,
        {
            allowDecimalPadding: false,
            decimalPlaces: this.decimalPlaces,
            maximumValue: this.maximumValue,
            minimumValue: this.minimumValue
        });
    }
};

const defaultGuid = "00000000-0000-0000-0000-000000000000";


;var FileTemplateHandler = (function () {
    let root = {

    };

    function Init() {

    }

    function DownloadAPI(fileName) {
        return new Promise(async (resolve, reject) => {
            let url = `/file/template/${fileName}`;

            var xhr = new XMLHttpRequest();
            xhr.open('GET', url, true);
            //xhr.setRequestHeader('Content-Type', false);
            xhr.setRequestHeader('Authorization', `Bearer ${App.Token}`);
            xhr.processData = false;

            xhr.onreadystatechange = function () {
                if (xhr.readyState === 4) {
                    if (xhr.status == 200) {
                        resolve(xhr);
                    } else {
                        reject(xhr);
                    }
                } else if (xhr.readyState == 2) {
                    if (xhr.status == 200) {
                        xhr.responseType = 'blob';
                    } else {
                        xhr.responseType = 'text';
                    }
                }
            };

            xhr.send();
        })
    }

    function Download(fileName) {
        loading(true);
        DownloadAPI(fileName).then(xhr => {
            let contentDisposition = xhr.getResponseHeader("Content-Disposition");
            var fileName = xhr.getResponseHeader('content-disposition').split('filename=')[1].split(';')[0];
            let response = xhr.response;

            // Tạo một liên kết ẩn với dữ liệu tải xuống
            var blob = new Blob([response], { type: response.type });
            var url = window.URL.createObjectURL(blob);
            var a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            a.click();
            window.URL.revokeObjectURL(url);

        }).catch(xhr => {
            let response = JSON.parse(xhr.response);
            toastr.error(response.message || 'Xảy ra lỗi khi tải biểu mẫu.');
        }).finally(() => {
            loading(false);
        })
    }


    function Base64ToArrayBuffer(base64) {
        var binaryString = window.atob(base64);
        var binaryLen = binaryString.length;
        var bytes = new Uint8Array(binaryLen);
        for (var i = 0; i < binaryLen; i++) {
            var ascii = binaryString.charCodeAt(i);
            bytes[i] = ascii;
        }
        return bytes;
    }

    function SaveByteArr(byte, fileType, fileName) {
        var blob = new Blob([byte], { type: fileType });
        var link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        var fileName = fileName;

        link.download = fileName;
        link.click();
        link.remove();
    }

    return {
        init: Init,
        downloadFileAPI: DownloadAPI,
        download: Download,
        utils: {
            base64ToArrayBuffer: Base64ToArrayBuffer,
            saveByteArr: SaveByteArr
        }
    }

})();



; const InventoryDocStatus_CSS = {
    0: "status_not_receive",
    1: "status_no_inventory",
    2: "status_not_inventory",
    3: "status_waiting_confirm",
    4: "status_waiting_confirm",
    5: "status_confirmed",
    6: "status_audit_passed",
    7: "status_audit_notpass",
};


function ConfigKnockoutJs() {
    ko.bindingHandlers.formatDatetime = {
        update: function (element, valueAccessor) {
            var value = ko.unwrap(valueAccessor());
            var formattedValue = value ? moment(value).format("DD/MM/YYYY HH:mm") : "";
            ko.bindingHandlers.text.update(element, function () { return formattedValue; });
        }
    };

    ko.bindingHandlers.formatQuantity = {
        update: function (element, valueAccessor) {
            var value = ko.unwrap(valueAccessor());
            var formattedValue = ValidateInputHelper.Utils.convertDecimalInventory(`${value}`);
            ko.bindingHandlers.text.update(element, function () { return formattedValue; });
        }
    };
}


; const GlobalEventName = {
    inventory_list_resultTabActived: "inventory_list_resultTabActived",
    inventory_tab_audittargetActived: "inventory_tab_audittargetActived",
    reporting_audit_notfound: "reporting_audit_notfound",
    inventory_location_mangament_response: "inventory_location_mangament_response",
    audit_mobile_filter_change: "auditmobile_filter_change",
    audit_mobile_department_change: "audit_mobile_department_change",
    audit_mobile_location_change: "audit_mobile_location_change",
    audit_mobile_component_change: "audit_mobile_component_change",
}


const ServerResponseStatusCode = {
    NotExistDocTypeA: 41,
    InvalidFileExcel: 70,
    Unauthorized: 401,
    Forbid: 403,
    LoggedInOtherDevice: 15,
    UpdateLocationTakeTime: 45
}



const isPromoter = () => App.User?.InventoryLoggedInfo?.InventoryRoleType == 2;
const isInCurrentInventory = () => moment(App.User?.InventoryLoggedInfo?.InventoryModel?.InventoryDate || `1111-01-01T00:00:00`).diff(moment(), 'days') >= 0;


function DisplayChangeLogHistory(oldQty, newQty, oldStatus, newStatus, isChangeCDetail) {
    let newstatusText = InventoryDocStatus[newStatus];
    let html = ``;

    let convertedNewQty = ValidateInputHelper.Utils.convertDecimalInventory(newQty);
    let convertedOldQty = ValidateInputHelper.Utils.convertDecimalInventory(oldQty);

    if (oldQty > 0) {
        oldQty = convertedOldQty;
    }

    if (newQty > 0) {
        newQty = convertedNewQty;
    }

    if (oldStatus != newStatus) {
        title = `${window.languageData[window.currentLanguage]["Cập nhật trạng thái"]}`;
        value = newstatusText;

        html += `<div class="">
            <span class="font-size-12">${window.languageData[window.currentLanguage]["Cập nhật trạng thái"]}: </span>
            <span class="font-size-12 ${InventoryDocStatus_CSS[newStatus]}">${newstatusText}</span>
        </div>`
    }

    if (oldQty == 0) {
        html += `<div class="">
                    <span class="font-size-12">${window.languageData[window.currentLanguage]["Nhập tổng SL"]}: </span>
                    <span class="font-size-12 txt-blue">${convertedNewQty}</span>
                </div>`
    }

    if (oldQty != 0 && oldQty != newQty) {
        title = `${window.languageData[window.currentLanguage]["Cập nhật tổng SL"]}`;
        value = `${convertedOldQty} -> ${convertedNewQty}`;

        html += `<div class="">
            <span class="font-size-12">${window.languageData[window.currentLanguage]["Cập nhật tổng SL"]}: </span>
            <span class="font-size-12 txt-blue">${convertedOldQty} -> ${convertedNewQty}</span>
        </div>`
    }
    if (isChangeCDetail) {
        title = `${window.languageData[window.currentLanguage]["Cập nhật"]}`;
        value = `${window.languageData[window.currentLanguage]["Số lượng trong bảng chi tiết"]}`;

        html += `<div class="">
            <span class="font-size-12">${window.languageData[window.currentLanguage]["Cập nhật"]}: </span>
            <span class="font-size-12">${window.languageData[window.currentLanguage]["Số lượng trong bảng chi tiết"]}</span>
        </div>`
    }

    if (oldStatus == newStatus && oldQty == newQty && isChangeCDetail == false) {
        value = window.languageData[window.currentLanguage]["Cập nhật dữ liệu chi tiết phiếu"];
        html += `<div class="">
            <span class="font-size-12">${window.languageData[window.currentLanguage]["Cập nhật dữ liệu chi tiết phiếu"]}</span>
        </div>`
    }

    return html
}

const AccountType = {
    TaiKhoanRieng: `TaiKhoanRieng`,
    TaiKhoanChung: `TaiKhoanChung`,
    TaiKhoanGiamSat: `TaiKhoanGiamSat`
};


const InventoryAccountTypeValue = "Kiểm kê";
const AuditAccountTypeValue = "Giám sát";
const PromotionAccountTypeValue = "Xúc tiến";
//const PromotionPersonInChargeTypeValue = "Xúc tiến - Người phụ trách";
//const PromotionPersonInManagementValue = "Xúc tiến - Người quản lý";

const AppInventoryRoleTypes = [InventoryAccountTypeValue, AuditAccountTypeValue, PromotionAccountTypeValue];

const AccountTypeByKey = (key) => Object.keys(AccountType).indexOf(key);


const InventoryRoleType =
{
    KiemKe: 0,
    GiamSat: 1,
    XucTien: 2
};

function IsImageExist(url, callback) {
    const img = new Image();
    img.src = url;

    if (img.complete) {
        callback(true);
    } else {
        img.onload = () => {
            callback(true);
        };
        img.onerror = () => {
            callback(false);
        };
    }
}

//Hàm check Guid:
function isGuid(value) {
    // Mẫu chuỗi GUID hợp lệ
    var guidPattern = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;

    // Kiểm tra nếu chuỗi truyền vào khớp với mẫu GUID
    return guidPattern.test(value);
}

//Hàm Loading:
function loading(isLoading) {
    let overlay = $("#overlay");
    if (isLoading) {
        overlay.fadeIn(0);
    } else {
        setTimeout(() => {
            overlay.fadeOut(0);
        }, 200)
    }
}


const DropdownOption = {
    All: "-1",
    DisplayName: {
        All: "Tất cả"
    },
}


function FitAuditMobileTopBar() {
    let topbar = $(".top_bar");
    let topbarHeight = topbar.outerHeight();

    let divContent = topbar.siblings("div, .container, .container-fluid, section").eq(0);
    divContent.css("margin-top", `${topbarHeight}px`);
}

//Timeout quét mã linh kiện milliseconds
const ScanQRTimeout = 20000;

//Interval time of auditing report
const AuditReportInterval = 15000;

const NotFounData = `Không có dữ liệu phiếu.`;

const InventoryStatus = {
    Finish: 3,
}

