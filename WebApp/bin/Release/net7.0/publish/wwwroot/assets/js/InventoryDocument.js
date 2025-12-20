var host = $("#APIGateway").val();

$(function () {
    waitForInventoryDocumentLanguageData();
});

function waitForInventoryDocumentLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        InventoryListHandler.init();

        ExportFileInventoryDocumentFull();

        ClickImportFileUpdateQuantity();

        //Tải biểu mẫu cập nhật số lượng:
        $(document).delegate("#download-file-update-quantity", "click", function (e) {
            let fileKey = "Bieumaucapnhatsoluong";
            FileTemplateHandler.download(fileKey);
        })

        //In chi tiết phiếu:
        PrintPDFDocumentDetail();

    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForInventoryDocumentLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}


function PrintPDFDocumentDetail() {

    $(document).delegate(".btn-PrintDocDetail", "click", (e) => {
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
        let heightCM = height / 25;

        let maxScreenWidth = Math.max(tableWidth);
        let cacheBodyScreen = $('#inventory-document-detail-modal .modal-content').outerWidth();
        $('#inventory-document-detail-modal .modal-content').width(maxScreenWidth);
        //window.departmentReportChart.resize();
        $('#inventory-document-detail-modal .modal-content').css("margin", "auto");


        loading(true);
        html2pdf(target, {
            margin: 0,
            filename: `Chitietphieu_${formattedTime}.pdf`,
            html2canvas: {
                dpi: 200,
                letterRendering: false,
                scale: 2,
                useCORS: true,
                imageTimeout: 15000, // Đợi tối đa 15 giây để tải hình ảnh
                onclone: (clonedDoc) => {
                    // Đảm bảo phần lịch sử hiển thị trong bản sao
                    const historyContainer = clonedDoc.querySelector('.wrapper_content_inventory .container_history');
                    if (historyContainer) {
                        historyContainer.style.display = 'block';
                    }
                }
            },
            jsPDF: {
                orientation: 'portrait',
                unit: 'cm',
                //format: [isOverFlowWidth ? (heightCM + 30) : heightCM, isOverFlowWidth ? 100 : 60],
                format: [40,28],
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

function ExportFileInventoryDocumentFull() {

    $(document).delegate("#export-file-list-inventory", "click", (e) => {

        var plant = $("#input_plant_inventory").val();
        var wHLoc = $("#input_whloc_inventory").val();
        var docNumberFrom = $("#input_quantity_inventory_document_from").val().replaceAll(',', '');
        var docNumberTo = $("#input_quantity_inventory_document_to").val().replaceAll(',', '');
        var modelCode = $("#input_modelcode_inventory").val();
        var assigneeAccount = $("#input_user_distribution_inventory").val();
        var componentCode = $("#input_componentcode_inventory").val();

        var departments = $("#select_inventory_department").val();
        var locations = $("#select_inventory_location").val();
        var docTypes = $("#select_inventory_doctype").val();
        var inventoryNames = $("#select_inventory_name").val();
        var statuses = $("#select_inventory_docstatus").val();


        let filterData = {
            Plant: plant,
            WHLoc: wHLoc,
            DocNumberFrom: docNumberFrom,
            DocNumberTo: docNumberTo,
            ModelCode: modelCode,
            AssigneeAccount: assigneeAccount,
            ComponentCode: componentCode,
            Departments: departments,
            Locations: locations,
            DocTypes: docTypes,
            InventoryNames: inventoryNames,
            Statuses: statuses,
        };


        var url = `/inventory/document/export`;
        $.ajax({
            type: 'POST',
            url: url,
            data: filterData,
            cache: false,
            xhrFields: {
                responseType: 'blob'
            },
            beforeSend: function () {
                loading(true);
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

                    link.download = `Danhsachphieukiemke_${formattedTime}.xlsx`;
                    link.click();
                } else {
                    toastr.error(window.languageData[window.currentLanguage]['Không tìm thấy file.']);
                }
                toastr.success(window.languageData[window.currentLanguage]["Export danh sách phiếu kiểm kê các đợt thành công."]);
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

//Import File Cập nhật quantity các phiếu:
function ClickImportFileUpdateQuantity() {
    $(document).delegate("#import-file-update-quantity", "click", (e) => {
        var inventoryNames = $("#select_inventory_name").val();
        if (inventoryNames && inventoryNames.length === 1) {
            $("#inputImportFileUpdateQuantity").trigger("click");
        }
        else {
            toastr.error(window.languageData[window.currentLanguage]['Chỉ được cập nhật số lượng trong 1 đợt kiểm kê.']);
        }
    })

    $(document).delegate("#inputImportFileUpdateQuantity", "change", (e) => {
        let file = e.target.files[0];

        if (file.size > 0 && (/\.(xlsx|xls)$/i.test(file.name))) {
            let inventoryNames = $("#select_inventory_name").val();
            let inventoryName = inventoryNames[0];
            let url = `${host}/api/inventory/web/${inventoryName}/import/update-quantity`;
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
                            title: `<b>${window.languageData[window.currentLanguage]['Thông báo']}</b>`,
                            text: `${window.languageData[window.currentLanguage]['Cập nhật số lượng']} ${response?.successCount || 0} ${window.languageData[window.currentLanguage]['phiếu thành công và có']} ${response?.failCount || 0} ${window.languageData[window.currentLanguage]['dòng dữ liệu lỗi không thể thực hiện import.']}
                            ${window.languageData[window.currentLanguage]['Vui lòng ấn “Đồng ý” để xem dữ liệu lỗi.']}`,
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
                            title: `<b>${window.languageData[window.currentLanguage]['Thông báo']}</b>`,
                            text: `${window.languageData[window.currentLanguage]['Cập nhật số lượng các phiếu thành công thành công.']}`,
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        });
                    }

                },
                error: function (error) {
                   
                    Swal.fire({
                        title: `<b>${window.languageData[window.currentLanguage]['File sai định dạng']}</b>`,
                        text: window.languageData[window.currentLanguage][error?.responseJSON?.message] || "Có lỗi khi thực hiện import.",
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    })
                },
                complete: function () {
                    InventoryListHandler.drawDatatable();

                    loading(false);
                }
            });
        }
        else {
            Swal.fire({
                title: `<b>${window.languageData[window.currentLanguage]['File sai định dạng']}</b>`,
                text: `${window.languageData[window.currentLanguage]['Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành cập nhật số lượng các phiếu.']}`,
                confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                width: '30%'
            })
        }
        $("#inputImportFileUpdateQuantity").val("")
    })
}

var InventoryListHandler = (function () {
    let root = {
        parentEl: $(".container_inventory_document")
    }

    let self = new ReceiveView();
    ko.applyBindings(self, document.querySelector(".table_controls"));

    //let receive_CheckedIds = [];
    //let receiveAll_UncheckedIds = [];

    localStorage.removeItem("receiveIds")
    localStorage.removeItem("receiveAll_unchecked_ids")

    let InventoryListDatable;
    let dataFilter = {};
    let inventoryDocumentDetailViewModel;
    let formValidator;

    let APIs = {
        UploadDocStatus: function (file) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/web/upload/change-status`;
                let formData = new FormData();
                formData.append("file", file);

                $.ajax({
                    type: 'POST',
                    url: url,
                    data: formData,
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
        },
        DownloadFileTemplateAPI: function () {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/web/template/download/upload-status`;

                $.ajax({
                    type: 'GET',
                    url: url,
                    async: true,
                    success: function (response) {
                        resolve(response)
                    },
                    error: function (error) {
                        reject(error)
                    }
                });
            })
        },
        UpdateStatusDocsAPI: function (ids) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/web/update-status`;

                $.ajax({
                    type: 'PUT',
                    url: url,
                    contentType: 'application/json',
                    data: JSON.stringify(ids),
                    async: true,
                    success: function (response) {
                        resolve(response)
                    },
                    error: function (error) {
                        reject(error)
                    }
                });
            })
        },
        ReceiveAllDocs: function (excludeIds) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/web/receive-all`;

                $.ajax({
                    type: 'PUT',
                    url: url,
                    contentType: 'application/json',
                    data: JSON.stringify(excludeIds),
                    async: true,
                    success: function (response) {
                        resolve(response)
                    },
                    error: function (error) {
                        reject(error)
                    }
                });
            })
        },
        PreCheckUpdateDocs: function () {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/web/update-doc/check`;
                $.ajax({
                    type: 'GET',
                    url: url,
                    contentType: 'application/json',
                    async: true,
                    success: function (response) {
                        resolve(response)
                    },
                    error: function (error) {
                        reject(error)
                    }
                });
            })
        },
        PreCheckDownloadTemplate: function () {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/web/update-doc/template/check`;
                $.ajax({
                    type: 'GET',
                    url: url,
                    contentType: 'application/json',
                    async: true,
                    success: function (response) {
                        resolve(response)
                    },
                    error: function (error) {
                        reject(error)
                    }
                });
            })
        },

    }

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

        var select_inventory_name = $("#select_inventory_name");
        var select_inventory_department = $("#select_inventory_department");
        var select_inventory_location = $("#select_inventory_location");
        var select_inventory_doctype = $("#select_inventory_doctype");
        var select_inventory_docstatus = $("#select_inventory_docstatus");

        var dropdownSelectors = [
            `#select_inventory_name`,
            `#select_inventory_department`,
            `#select_inventory_location`,
            `#select_inventory_doctype`,
            `#select_inventory_docstatus`,
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
        $("#select_inventory_name")[0].reset();
        //$("#select_inventory_name")[0].toggleSelectAll(true);
        let currInventory = App?.User?.InventoryLoggedInfo?.InventoryModel?.Name;
        let firstInventoryOption = $("#select_inventory_name")[0]?.options[0]?.value || "";
        $("#select_inventory_name")[0].setValue(currInventory || firstInventoryOption);

        $("#select_inventory_department")[0].reset();
        $("#select_inventory_department")[0].toggleSelectAll(true);

        $("#select_inventory_location")[0].reset();
        $("#select_inventory_location")[0].toggleSelectAll(true);

        $("#select_inventory_doctype")[0].reset();
        $("#select_inventory_doctype")[0].toggleSelectAll(true);

        $("#select_inventory_docstatus")[0].reset();
        //$("#select_inventory_docstatus")[0].toggleSelectAll(true);

        $("#select_inventory_docstatus")[0].setValue($("#select_inventory_docstatus")[0].options[0].value)

        InitDataTable();

        ValidateSearchForm();

        //Khi vào màn danh sách phiếu
        let canPerformUpdateStatus = isPromoter() ||
                                    (!isPromoter() && App.User.isGrant("EDIT_INVENTORY") && isInCurrentInventory());

        if (!canPerformUpdateStatus) {
            $("#download-file-list-inventory").remove();
            $("#upload-status-list-inventory").remove();
        }
    }

    function ValidateSearchForm() {
        root.parentEl.find(`#input_quantity_inventory_document_from, #input_quantity_inventory_document_to`).on("input", function (event) {
            let value = event.target.value;
            event.target.value = value.replace(/\D/g, '').slice(0, 5); /* Giới hạn nhập 5 ký tự*/
        });

        root.parentEl.find(`#input_quantity_inventory_document_from, #input_quantity_inventory_document_to`).on("keyup", function (event) {
            let value = event.target.value;
            event.target.value = value.replace(/\D/g, '').slice(0, 5);
        });

        //root.parentEl.find("#input_quantity_inventory_document_from, #input_quantity_inventory_document_to").on("blur", function (e) {
        //    let fromValue = root.parentEl.find("#input_quantity_inventory_document_from").val();
        //    let toValue = root.parentEl.find("#input_quantity_inventory_document_to").val();

        //    if (fromValue.length && toValue.length) {
        //        $(root.searchForm).valid();
        //    }
        //});

        $("#input_plant_inventory").on("keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(4));
        $("#input_plant_inventory").on("keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(4));

        //Chặn nhập khoảng trắng và cắt khoảng trắng khi paste
        $("#input_plant_inventory").on("keypress", ValidateInputHelper.PreventWhiteSpace);
        $("#input_plant_inventory").on("keyup", ValidateInputHelper.RemoveWhiteSpaceOnKeyup);


        $("#input_whloc_inventory").on("keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(4));
        $("#input_whloc_inventory").on("keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(4));

        //Chặn nhập khoảng trắng và cắt khoảng trắng khi paste
        $("#input_whloc_inventory").on("keypress", ValidateInputHelper.PreventWhiteSpace);
        $("#input_whloc_inventory").on("keyup", ValidateInputHelper.RemoveWhiteSpaceOnKeyup);


        $("#input_modelcode_inventory").on("keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(10));
        $("#input_modelcode_inventory").on("keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(10));

        //Chặn nhập khoảng trắng và cắt khoảng trắng khi paste
        $("#input_modelcode_inventory").on("keypress", ValidateInputHelper.PreventWhiteSpace);
        $("#input_modelcode_inventory").on("keyup", ValidateInputHelper.RemoveWhiteSpaceOnKeyup);

        $("#input_user_distribution_inventory").on("keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50));
        $("#input_user_distribution_inventory").on("keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50));

        $("#input_componentcode_inventory").on("keypress", ValidateInputHelper.PreventWhiteSpace);
        $("#input_componentcode_inventory").on("keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(12));
        $("#input_componentcode_inventory").on("keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(12));

        jQuery.validator.addMethod("docCodeNumer", function (value, element) {
            let valid = true;
            let pattern = /^[0-9]{1,5}$/g;

            return pattern.test(value);
        }, window.languageData[window.currentLanguage]['Số phiếu vừa nhập không hợp lệ.']);

        jQuery.validator.addMethod("docCodeNumberRange", function (value, element) {
            let valid = true;
            let fromValue = $("#input_quantity_inventory_document_from").val();
            let toValue = $("#input_quantity_inventory_document_to").val();

            if (fromValue.length && toValue.length) {
                let convertedFromValue = parseInt(fromValue);
                let convertedToValue = parseInt(toValue);

                if (!convertedFromValue || !convertedToValue) {
                    return false;
                }

                if (convertedFromValue > convertedToValue) {
                    return false;
                } 
            }

            return true;
        }, window.languageData[window.currentLanguage]['Số phiếu vừa nhập không hợp lệ.']);

        let validateModel = {
            rules: {
                input_quantity_inventory_document_from: {
                    required: {
                        depends: function () {
                            let dependValue = root.parentEl.find(`#input_quantity_inventory_document_from`).val().length;
                            let toValue = root.parentEl.find(`#input_quantity_inventory_document_to`).val().length;

                            if (dependValue) {
                                return true;
                            } else {
                                if (toValue) {
                                    return true;
                                }

                                return false;
                            }
                        }
                    },
                    docCodeNumberRange: true
                },
                input_quantity_inventory_document_to: {
                    required: {
                        depends: function () {
                            let fromValue = root.parentEl.find(`#input_quantity_inventory_document_from`).val().length;
                            let dependValue = root.parentEl.find(`#input_quantity_inventory_document_to`).val().length;

                            if (dependValue) {
                                return true;
                            } else {
                                if (fromValue) {
                                    return true;
                                }
                                return false;
                            }
                        }
                    },
                    docCodeNumberRange: true
                }
            },
            messages: {
                input_quantity_inventory_document_from: {
                    required: window.languageData[window.currentLanguage]["Vui lòng nhập số phiếu."]
                },
                input_quantity_inventory_document_to: {
                    required: window.languageData[window.currentLanguage]["Vui lòng nhập số phiếu."]
                },
            }
        }

        formValidator = $(root.parentEl).find("#input_storage_search_form").validate(validateModel);
    }

    function InitDataTable() {
        InventoryListDatable = $(root.parentEl).find("#inventory_list_table").DataTable({
            "bDestroy": true,
            "processing": `<div class="spinner"></div>`,
            pagingType: 'full_numbers',
            'language': {
                'loadingRecords': `<div class="spinner"></div>`,
                'processing': '<div class="spinner"></div>',
            },
            select: true,
            stateSave: true,
            colReorder: true,
            "scrollX": true,
            scrollCollapse: true,
            "serverSide": true,
            "filter": true,
            "searching": false,
            responsive: true,
            "lengthMenu": [10, 30, 50, 200],
            dom: 'rt<"bottom"flp><"clear">',
            "ordering": false,
            "ajax": {
                "url": `${host}/api/inventory/web/document`,
                "type": "POST",
                "contentType": "application/x-www-form-urlencoded",
                dataType: "json",
                data: function (data) {
                    dataFilter.Plant = $("#input_plant_inventory").val();
                    dataFilter.WHLoc = $("#input_whloc_inventory").val();
                    dataFilter.DocNumberFrom = $("#input_quantity_inventory_document_from").val().replaceAll(',', '');
                    dataFilter.DocNumberTo = $("#input_quantity_inventory_document_to").val().replaceAll(',', '');
                    dataFilter.ModelCode = $("#input_modelcode_inventory").val();
                    dataFilter.AssigneeAccount = $("#input_user_distribution_inventory").val();
                    dataFilter.componentCode = $("#input_componentcode_inventory").val();

                    dataFilter.InventoryNames = $("#select_inventory_name").val();
                    dataFilter.Departments = $("#select_inventory_department").val();
                    dataFilter.Locations = $("#select_inventory_location").val();
                    dataFilter.DocTypes = $("#select_inventory_doctype").val();
                    dataFilter.Statuses = $("#select_inventory_docstatus").val();

                    Object.assign(data, dataFilter);
                    return data;
                },
                "dataSrc": function ({ data, docsNotReceiveCount, cursor }) {
                    self.notReceiveDocsCount(docsNotReceiveCount);

                    dataFilter.Cursor = cursor;
                    return data;
                }
            },
            "drawCallback": function (settings) {
                let totalPages = InventoryListDatable.page.info().pages;
                let totalRecords = InventoryListDatable.page.info().recordsTotal;

                let currPage = InventoryListDatable.page() + 1;
                if (currPage == 1) {
                    root.parentEl.find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                    root.parentEl.find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                }
                if (currPage == totalPages) {
                    root.parentEl.find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                    root.parentEl.find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                }

                root.parentEl.find(".pagination").before(`<div class="h5 datatable_total_records"> <b>${window.languageData[window.currentLanguage]['Tổng']}</b>: ${totalPages} <span class="total_record_seperate"> | </span>
                </div>`)

                if (totalRecords == 0) {
                    $("#export-file-list-inventory").removeClass("btn_disabled").addClass("btn_disabled").attr("disabled", true);
                } else {
                    $("#export-file-list-inventory").removeClass("btn_disabled").attr("disabled", false)
                }

                let isAutoSelectAll = $("#receiveAllPage").is(":checked");
                if (isAutoSelectAll) {
                    let checkboxEls = root.parentEl.find(".InventoryDocument_check");
                    checkboxEls.map((i, el) => {
                        let id = $(el).attr("data-id");
                        if (!self.receiveAll_UncheckedIds().includes(id)) {
                            root.parentEl.find(`.InventoryDocument_check[data-id="${id}"]`).prop("checked", true).change();
                        }
                    })
                } else {
                    let checkboxEls = root.parentEl.find(".InventoryDocument_check");
                    checkboxEls.map((i, el) => {
                        let id = $(el).attr("data-id");
                        if (self.receive_CheckedIds().includes(id)) {
                            $(el).prop("checked", true).trigger("change");
                        }
                    })
                }

                //Nếu được tích hết thì nút tích hết bật
                let checkboxItemLength = root.parentEl.find(`.InventoryDocument_check`).length;
                let checkedItemLength = root.parentEl.find(`.InventoryDocument_check:checked`).length;
                let isAllChecked = (checkboxItemLength == checkedItemLength) && checkboxItemLength > 0;
                root.parentEl.find(`#input_checkall`).prop("checked", isAllChecked);
            },
            "columns": [
                {
                    "data": "id",
                    "name": "checkbox",
                    "render": function (data, type, row, table) {
                        var validRole = isInCurrentInventory() && App.User.isGrant("EDIT_INVENTORY") &&
                                    ((App.User.InventoryLoggedInfo.InventoryRoleType == InventoryRoleType.KiemKe) &&
                                        App.User.AccountType == AccountType.TaiKhoanChung);
                        if (!validRole) {
                            return ``;
                        }

                        let status = row.status;
                        if (status == 0) {
                            return `<input type="checkbox" class="InventoryDocument_check" data-id="${row.id}" />`
                        }
                        return ``;
                    },
                    "autoWidth": true
                },
                {
                    "data": "",
                    "name": "STT",
                    "render": function (data, type, row, index) {
                        let pagesize = index.settings._iDisplayLength;
                        let currentRow = ++index.row;
                        let currentPage = InventoryListDatable.page() + 1;

                        let STT = ((currentPage - 1) * pagesize) + currentRow;

                        if (STT < 10) {
                            STT = `0${STT}`;
                        }
                        return STT;
                    },
                    "autoWidth": true
                },
                { "data": "inventoryName", "name": "inventoryName", "autoWidth": true },
                { "data": "department", "name": "department", "autoWidth": true },
                { "data": "location", "name": "location", "autoWidth": true },
                { "data": "docCode", "name": "Mã phiếu", "autoWidth": true },
                { "data": "plant", "name": "Plant", "autoWidth": true },
                { "data": "whLoc", "name": "WH Loc.", "autoWidth": true },
                { "data": "componentCode", "name": "Mã linh kiện", "autoWidth": true },
                { "data": "modelCode", "name": "ModelCode", "autoWidth": true },
                { "data": "componentName", "name": "Tên linh kiện", "autoWidth": true },
                {
                    "data": "quantity", "name": "quantity", render: function (data) {
                        
                        if (data == null || data == undefined) {
                            return ``;
                        }

                        return `${ValidateInputHelper.Utils.convertDecimalInventory(data)}`;
                    }, "autoWidth": true
                },
                { "data": "position", "name": "Vị trí", "autoWidth": true },
                {
                    "data": "status", "name": "status", render: function (data, type, row, index) {
                        if (data || data == 0) {
                            return `<div class="${InventoryDocStatus_CSS[data]} txt-bolder">${InventoryDocStatus[data]}</div>`;
                        }
                        return ``;
                    }, "autoWidth": true
                },
                { "data": "stockType", "name": "Stock type", "autoWidth": true },
                { "data": "specialStock", "name": "Special stock", "autoWidth": true },
                { "data": "saleOrderNo", "name": "S/O No.", "autoWidth": true },
                { "data": "saleOrderList", "name": "S/O List", "autoWidth": true },
                { "data": "assigneeAccount", "name": "Tài khoản phân phát", "autoWidth": true },
                { "data": "receiveBy", "name": "receiveBy", "autoWidth": true },
                { "data": "receiveAt", "name": "Tên công đoạn", "autoWidth": true },
                { "data": "inventoryBy", "name": "inventoryBy", "autoWidth": true },
                { "data": "inventoryAt", "name": "inventoryAt", "autoWidth": true },
                { "data": "confirmBy", "name": "confirmBy", "autoWidth": true },
                { "data": "confirmAt", "name": "confirmAt", "autoWidth": true },
                { "data": "auditBy", "name": "auditBy", "autoWidth": true },
                { "data": "auditAt", "name": "auditAt", "autoWidth": true },
                { "data": "sapInventoryNo", "name": "sapInventoryNo", "autoWidth": true },
                { "data": "assemblyLoc", "name": "assemblyLoc", "autoWidth": true },
                { "data": "vendorCode", "name": "Vendor code", "autoWidth": true },
                { "data": "physInv", "name": "Phys.Inv", "autoWidth": true },
                { "data": "fiscalYear", "name": "Fiscal year", "autoWidth": true },
                { "data": "item", "name": "ITEM", "autoWidth": true },
                { "data": "plantedCount", "name": "Planned count", "autoWidth": true },
                { "data": "columnC", "name": "Cột C", "autoWidth": true },
                { "data": "columnN", "name": "Cột N", "autoWidth": true },
                { "data": "columnO", "name": "Cột O", "autoWidth": true },
                { "data": "columnP", "name": "Cột P", "autoWidth": true },
                { "data": "columnQ", "name": "Cột Q", "autoWidth": true },
                { "data": "columnR", "name": "Cột R", "autoWidth": true },
                { "data": "columnS", "name": "Cột S", "autoWidth": true },
                { "data": "createdBy", "name": "Người tạo", "autoWidth": true },
                { "data": "createdAt", "name": "Thời gian tạo", "autoWidth": true },
                { "data": "", "name": "", "render": function (data, type, row) {
                        const selectHtmlSpecial = `
                        <div class="Controls_Container">
                            <div class="ViewDetail_InventoryDoc_Controls mx-3">
                                <a class="detail-inventory-doc view_detail_Indoc btn_detail_inventory_doc" docId="${row.id}">${window.languageData[window.currentLanguage]['Xem chi tiết']}</a>
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

    function removeElement(array, elem) {
        var index = array.indexOf(elem);
        if (index > -1) {
            array.splice(index, 1);
        }
    }

    var removeIds = (idsToRemove, originalArray) => originalArray.filter(id => !idsToRemove.toArray().includes(id));

    function Events() {
        //$(window).on("countSelectedCheckbox", function () {
        //    let oldValues = localStorage.getItem("receiveIds")?.split(",") || [];
        //    if (oldValues?.length) {
        //        $("#inventory_list_checked_count").text(`${oldValues?.length} bản ghi đã được chọn.`);
        //    } else {
        //        $("#inventory_list_checked_count").empty();
        //    }
        //})

        root.parentEl.delegate("#receiveAllPage", "change", function (e) {
            let isChecked = $(this).is(":checked");
            if (isChecked) {
                let checkboxEls = root.parentEl.find(".InventoryDocument_check");
                checkboxEls.map((i, el) => {
                    let id = $(el).attr("data-id");
                    if (!self.receiveAll_UncheckedIds().includes(id)) {
                        root.parentEl.find(`.InventoryDocument_check[data-id="${id}"]`).prop("checked", true).change();
                    }
                })
            }

        })

        root.parentEl.delegate("#input_checkall", "change", function (e) {
            let childCheckboxes = root.parentEl.find("#inventory_list_table").find(".InventoryDocument_check");
            let isChecked = $(this).is(":checked");
            childCheckboxes.prop("checked", isChecked).change();

            if (self.receive_CheckedIds().length == self.notReceiveDocsCount()) {
                self.receiveAll(true)
            }
        })

        root.parentEl.delegate(".InventoryDocument_check", "change", function (e) {
            let childCheckboxes = root.parentEl.find("#inventory_list_table").find(".InventoryDocument_check");
            let checkedElements = root.parentEl.find("#inventory_list_table").find(".InventoryDocument_check:checked");
            let isCheckedAll = childCheckboxes.length == checkedElements.length;
            root.parentEl.find("#input_checkall").prop("checked", isCheckedAll);

        })

        $("#receiveAllPage").on("input", function (e) {
           
        })

        root.parentEl.delegate(".InventoryDocument_check", "change", function (e) {
            let target = e.target;
            let id = $(target).attr("data-id");

            let isReceiveAll = $("#receiveAllPage").is(":checked");
            if (isReceiveAll) {
                if ($(target).is(":checked")) {
                    removeElement(self.receiveAll_UncheckedIds, id);
                } else {
                    self.receiveAll_UncheckedIds.push(id);
                    removeElement(self.receive_CheckedIds, id);
                }
            } else {
                if ($(target).is(":checked")) {
                    if (!self.receive_CheckedIds().includes(id)) {
                        self.receive_CheckedIds.push(id);
                    }
                } else {
                    removeElement(self.receive_CheckedIds, id);
                }
            }
        })


        root.parentEl.delegate("#receive-list-inventory", "click", function (e) {
            //Phải chọn tất cả hoặc chọn ít nhất một item mới được tiếp nhận
            if (!self.enableReceiveBtn()) {
                return;
            }

            let checkedCount = self.receiveAll() ? (self.notReceiveDocsCount() - self.receiveAll_UncheckedIds().length) : self.receive_CheckedIds().length;
            let title = `${window.languageData[window.currentLanguage]['Bạn có chắc chắn muốn tiếp nhận']} ${checkedCount} ${window.languageData[window.currentLanguage]['phiếu này ?']}`;

            Swal.fire({
                title: `<b>${window.languageData[window.currentLanguage]['Xác nhận tiếp nhận phiếu']}</b>`,
                text: `${title}`,
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
                    loading(true);
                    let isReceiveAll = $("#receiveAllPage").is(":checked");

                    if (isReceiveAll) {
                        // Tiếp nhận tất cả
                        APIs.ReceiveAllDocs(self.receiveAll_UncheckedIds()).then(res => {
                            toastr.success(res?.message);

                            root.parentEl.find("#receive-list-inventory").removeClass("btn_disabled").addClass("btn_disabled");
                            //reset store ids
                            
                        }).catch(err => {
                            Swal.fire({
                                title: `<b>${window.languageData[window.currentLanguage][err?.responseJSON?.message]}</b>`,
                                confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                                width: '30%'
                            })
                        }).finally(() => {
                            InventoryListHandler.drawDatatable(true);
                            self.receiveAll_UncheckedIds([]);
                            self.receive_CheckedIds([]);
                            self.receiveAll(false);

                            loading(false);
                        })
                    }
                    else {
                        //Tiếp nhận các phiếu được chọn
                        loading(true);
                        APIs.UpdateStatusDocsAPI(self.receive_CheckedIds()).then(res => {
                            toastr.success(res?.message);

                            root.parentEl.find("#receive-list-inventory").removeClass("btn_disabled").addClass("btn_disabled");

                            //reset store ids
                            
                        }).catch(err => {
                            Swal.fire({
                                title: `<b>${window.languageData[window.currentLanguage][err?.responseJSON?.message]}</b>`,
                                confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                                width: '30%'
                            })
                        }).finally(() => {
                            InventoryListHandler.drawDatatable(true);
                            self.receiveAll_UncheckedIds([]);
                            self.receive_CheckedIds([]);
                            self.receiveAll(false);

                            loading(false);
                        })
                    }
                }
            })
        })

        //Xem chi tiết phiếu
        root.parentEl.delegate(".btn_detail_inventory_doc", "click", function (e) {
            let thisBtn = $(e.target).closest(".btn_detail_inventory_doc");
            $("#inventory-document-detail-modal").modal('show');

            let docId = $(thisBtn).attr("docId");
            inventoryDocumentDetailViewModel.loadDocDetail(docId);
        });

        root.btn_upload_status_list_inventory.click(function (e) {
            let target = this;

            APIs.PreCheckUpdateDocs().then((res) => {
                root.inputFile_UploadDocStatus.trigger("click");
            }).catch(err => {
                Swal.fire({
                    title: `<b>${window.languageData[window.currentLanguage]['Thông báo']}</b>`,
                    text: window.languageData[window.currentLanguage][err?.responseJSON?.message] || "Có lỗi khi thực hiện import.",
                    confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                    width: '30%'
                })
            }).finally(() => {
            })
        })

        root.inputFile_UploadDocStatus.change(function (e) {
            let target = this;
            let file = this.files[0];

            loading(true);
            APIs.UploadDocStatus(file).then(function (res) {
                if (res.failCount > 0) {
                    Swal.fire({
                        title: `<b>${window.languageData[window.currentLanguage]['Thông báo']}</b>`,
                        text: `${window.languageData[window.currentLanguage]['Upload chuyển trạng thái thành công']} ${res?.successCount || 0} 
                                ${window.languageData[window.currentLanguage]['dòng dữ liệu và có']} ${res?.failCount || 0} ${window.languageData[window.currentLanguage]['dòng dữ liệu lỗi không thể chuyển trạng thái.']} 
                                ${window.languageData[window.currentLanguage]['Vui lòng ấn “Đồng ý” để tải file dữ liệu lỗi.']}`,
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
                            let convertedByte = FileTemplateHandler.utils.base64ToArrayBuffer(res.bytes);
                            FileTemplateHandler.utils.saveByteArr(convertedByte, res.fileType, res.fileName);
                        }
                    });
                } else if (res.failCount == 0) {
                    Swal.fire({
                        title: `<b>${window.languageData[window.currentLanguage]['Thông báo']}</b>`,
                        text: `${window.languageData[window.currentLanguage]['Upload chuyển trạng thái thành công']} ${res.successCount} ${window.languageData[window.currentLanguage]['dòng dữ liệu.']}`,
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    });
                }
            }).catch(err => {
                Swal.fire({
                    title: `<b>${window.languageData[window.currentLanguage]['Thông báo']}</b>`,
                    text: window.languageData[window.currentLanguage][err?.responseJSON?.message] || "Có lỗi khi thực hiện import.",
                    confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                    width: '30%'
                })
            }).finally(() => {
                InventoryListDatable.draw();
                loading(false);
            })

            $(this).val("");
        })

        $(document).delegate("#select_inventory_department", "change", (e) => {
            var listDepartments = $('#select_inventory_department').val();

            //Nếu bỏ tích hết phòng ban thì bỏ tích hết khu vực
            if (listDepartments.length == 0) {
                $("#select_inventory_location")[0].setOptions([]);
                $("#select_inventory_location")[0].reset();
                return;
            }

            //Call Api Xem chi tiết:
            var filterData = JSON.stringify({
                Departments: listDepartments
            });
            //Check phân quyền tài khoản: Nếu là tài khoản riêng hoặc Tài khoản chung(Xúc tiến) => Thêm 1 option với giá trị Location = ""
            //Mục đích để tìm kiếm phòng ban và khu vực trống:
            var showOptionEmpty_Location = (App.User.AccountType == AccountType.TaiKhoanRieng && (App.User.isGrant("EDIT_INVENTORY") || App.User.isGrant("VIEW_ALL_INVENTORY") || App.User.isGrant("VIEW_CURRENT_INVENTORY"))) || isPromoter();

            $.ajax({
                type: "POST",
                url: `${host}/api/inventory/location/departmentname`,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: filterData,
                success: function (res) {
                    if (res.code == 200) {
                        var options = [];
                        if (showOptionEmpty_Location) {
                            options.push({ label: "", value: "" })
                        }

                        let resultHtml = res?.data.map(item => {
                            options.push({ label: item.locationName, value: item.locationName })
                        });

                        $("#select_inventory_location")[0].virtualSelect.alwaysShowSelectedOptionsLabel = !(options.length > 1);
                        $("#select_inventory_location")[0].virtualSelect.disableAllOptionsSelectedText = !(options.length > 1);
                        $("#select_inventory_location")[0].virtualSelect.selectAllOnlyVisible = !(options.length > 1);
                        $("#select_inventory_location")[0].virtualSelect.autoSelectFirstOption = true;
                        //alwaysShowSelectedOptionsLabel: false,
                        //disableAllOptionsSelectedText: false,
                        //selectAllOnlyVisible: false,
                        //$("#select_inventory_location")[0].virtualSelect.disableAllOptionsSelectedText = options.length > 1;
                          
                        document.querySelector('#select_inventory_location').setOptions(options);

                        $("#select_inventory_location")[0].reset();
                        $("#select_inventory_location")[0].toggleSelectAll(true);
                    }
                },
                error: function (error) {
                    toastr.error(error.message)

                    //Nếu không tìm thấy khu vực thì reset dữ liệu khu vực
                }
            });
        });

        root.parentEl.delegate("#download-file-list-inventory", "click", async function (e) {
            let thisBtn = $(this).closest("#download-file-list-inventory");

            try {
                let checkTemplateResponse = await APIs.PreCheckDownloadTemplate();

                loading(true);
                APIs.DownloadFileTemplateAPI().then(res => {
                    if (res.bytes.length) {
                        let convertedBytes = FileTemplateHandler.utils.base64ToArrayBuffer(res.bytes);
                        FileTemplateHandler.utils.saveByteArr(convertedBytes, res.fileType, res.fileName);
                    }

                }).catch(err => {
                    Swal.fire({
                        title: `<b>${window.languageData[window.currentLanguage]['Thông báo']}</b>`,
                        text: err?.responseJSON?.message || "",
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    })
                }).finally(() => {
                    loading(false);
                })

            } catch(err) {

                Swal.fire({
                    title: `<b>${window.languageData[window.currentLanguage]['Thông báo']}</b>`,
                    text: window.languageData[window.currentLanguage][err?.responseJSON?.message] || "",
                    confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                    width: '30%'
                });
                return false;
            }
        })

        root.parentEl.delegate("#btn-search", "click", ValidateInputHelper.Utils.debounce(function (e) {
            let validForm = root.searchForm.valid();
            if (validForm) {
                InventoryListDatable.draw();
            }
        }, 200))

        root.searchForm.on("keypress", ValidateInputHelper.FormEnter(function (e) {
            let validForm = root.searchForm.valid();
            if (validForm) {
                InventoryListDatable.draw();
            }
        }))

        root.parentEl.delegate("#btn-reset", "click", ValidateInputHelper.Utils.debounce(function (e) {
            //$("#select_inventory_name")[0].reset();
            //$("#select_inventory_name")[0].toggleSelectAll(true);

            $("#select_inventory_name")[0].reset();
            //$("#select_inventory_name")[0].toggleSelectAll(true);
            let currInventory = App?.User?.InventoryLoggedInfo?.InventoryModel?.Name;
            let firstInventoryOption = $("#select_inventory_name")[0]?.options[0]?.value || "";
            $("#select_inventory_name")[0].setValue(currInventory || firstInventoryOption);

            $("#select_inventory_department")[0].reset();
            $("#select_inventory_department")[0].toggleSelectAll(true);

            $("#select_inventory_location")[0].reset();
            $("#select_inventory_location")[0].toggleSelectAll(true);

            $("#select_inventory_doctype")[0].reset();
            $("#select_inventory_doctype")[0].toggleSelectAll(true);

            $("#select_inventory_docstatus")[0].reset();
            //$("#select_inventory_docstatus")[0].toggleSelectAll(true);
            $("#select_inventory_docstatus")[0].setValue($("#select_inventory_docstatus")[0].options[0].value)

            $("#input_quantity_inventory_document_from").val("")
            $("#input_quantity_inventory_document_to").val("")
            $("#input_plant_inventory").val("")
            $("#input_whloc_inventory").val("")
            $("#input_modelcode_inventory").val("")
            
            $("#input_componentcode_inventory").val("")
            //Check phân quyền tài khoản: Tài khoản chung(Vai trò Giám sát hoặc kiểm kê) => Tài khoản phân phát luôn luôn là UserName
            var showOptionEmpty_Location = (App.User.AccountType == AccountType.TaiKhoanRieng && (App.User.isGrant("EDIT_INVENTORY") || App.User.isGrant("VIEW_ALL_INVENTORY") || App.User.isGrant("VIEW_CURRENT_INVENTORY"))) || isPromoter();
            if (showOptionEmpty_Location) {
                $("#input_user_distribution_inventory").val("")
            }
            formValidator.resetForm();

            InventoryListDatable.draw();
        }))
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
    self.envicenceImage = ko.observable(model?.envicenceImage ? `${AppUser.getApiGateway}/${model.envicenceImage.replaceAll("\\", "/") }` : null);
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
            InventoryListHandler.drawDatatable(true);
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

    self.handleKeyUp = ValidateInputHelper.Utils.debounce((data,event) => {
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

