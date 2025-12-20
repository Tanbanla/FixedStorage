var host = $("#APIGateway").val();
$(function () {
    waitForResultInventoryLanguageData();
});

function waitForResultInventoryLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        ResultInventoryDownloadForm();
        CloseModalResultInventoryForm();

        ResultInventoryHandler.init();

        $("#synthetic-result").click((e) => {
            ResultInventoryHandler.initDataTable();


            //Publish event khi nhấn tab tổng hợp kết quả màn danh sách đợt kiểm kê
            $.pub(GlobalEventName.inventory_list_resultTabActived);
            $(".btn_reset_document_result").trigger("click");

            //Kích hoạt sự kiện khi nhấn vào tab tổng hợp kết quả
            $(window).trigger("result.inventory.active");


            const d = new Date();
            //const resDate = new Date(res.data);
            const forceAggregateAt = $("#inventory-wrapper").attr("data-aggregate-at");
            var nextAvailableTime = moment(forceAggregateAt).add(5, 'm').toDate();
            //debugger;
            var diffTime = (nextAvailableTime - d);
            if (diffTime > 0) {
                $('#spin-icon').attr('hidden', false);
                $('#sync-icon').attr('hidden', true);
                var countDown = setInterval(function () {

                    var now = new Date().getTime();
                    var ammount = nextAvailableTime - now;
                    var minutes = Math.ceil((ammount / 1000) / 60);
                    var seconds = Math.floor(ammount / 1000);
                    //debugger
                    $('.btnAggregateQuantity').attr('disabled', 'disabled');
                    if (minutes > 1) {
                        $('#countDown').text(`${window.languageData[window.currentLanguage]["Đợi"]} ${minutes} ${window.languageData[window.currentLanguage]["phút"]}..`);
                    }
                    else {
                        $('#countDown').text(`${window.languageData[window.currentLanguage]["Đợi"]} ${seconds} s..`);
                    }
                    if (ammount <= 0) {
                        clearInterval(countDown);
                        $('#sync-icon').removeAttr('hidden');
                        $('#spin-icon').attr('hidden', true);
                        $('#countDown').text(window.languageData[window.currentLanguage]['Tổng hợp']);
                        $('.btnAggregateQuantity').removeAttr('disabled');
                    }

                }, 500);


            }
        })


        //Sự kiện khi click vào tab tổng hợp kết quả
        $(window).on("result.inventory.active", function () {
            //Phân quyền màn danh sách giám sát:
            var getInventoryStatus = $("#inventory-wrapper").attr("data-status");
            let getAccountType = App.User.AccountType;
            let getInventoryRoleType = App.User.InventoryLoggedInfo.InventoryRoleType;
            let getInventoryDate = $("#inventory-wrapper").attr("data-inventory-date");


            //forceAggregateAt = $("#inventory-wrapper").attr("data-aggregate-at");
            //const d = new Date();
            //const resDate = new Date(forceAggregateAt);
            //var nextAvailableTime = moment(resDate).add(1, 'm').toDate();
            //debugger;
            //var diffTime = (nextAvailableTime - d);
            //if (diffTime > 0) {
            //    $('#spin-icon').attr('hidden', false);
            //    $('#sync-icon').attr('hidden', true);



            //var countDown = setInterval(function () {
            //    forceAggregateAt = $("#inventory-wrapper").attr("data-aggregate-at");
            //    const resDate = new Date(forceAggregateAt);
            //    var nextAvailableTime = moment(resDate).add(1, 'm').toDate();
            //    var now = new Date();

            //    var ammount = nextAvailableTime - now;
            //    if (ammount > 0) {
            //        $('#spin-icon').attr('hidden', false);
            //        $('#sync-icon').attr('hidden', true);
            //        var seconds = Math.floor((ammount % (1000 * 60)) / 1000);
            //        $('.btnAggregateQuantity').attr('disabled', 'disabled');
            //        $('#countDown').text(`Đợi ${seconds} s nữa..`);
            //        if (ammount <= 0) {
            //            clearInterval(countDown);
            //            $('#sync-icon').removeAttr('hidden');
            //            $('#spin-icon').attr('hidden', true);
            //            $('#countDown').text('Tổng hợp');
            //            $('.btnAggregateQuantity').removeAttr('disabled');
            //        }
            //    }


            //}, 1000);





            let currentDate = moment().format("YYYY-MM-DD");
            //TH1: Trang thái phiếu đã hoàn thành:
            if (getInventoryStatus === '3') {
                if (getAccountType === "TaiKhoanChung" && getInventoryRoleType === 2) {
                    $(".btnImportDocResultFromBwins, .btnImportFileSAP").hide();
                    $(".result_inventory_export_file_txt, .result_inventory_export_file, #ResultInventory_Download_Form").show();
                } else if (getAccountType === "TaiKhoanRieng") {
                    //Chỉ có quyền xem:
                    if (App.User.isGrant("VIEW_ALL_INVENTORY") || App.User.isGrant("VIEW_CURRENT_INVENTORY")) {
                        $(".result_inventory_export_file_txt, .result_inventory_export_file").show();
                        $(".btnImportDocResultFromBwins, .btnImportFileSAP, #ResultInventory_Download_Form").hide();
                    }

                    if (App.User.isGrant("EDIT_INVENTORY")) {
                        $(".result_inventory_export_file_txt, .result_inventory_export_file, #ResultInventory_Download_Form").show();
                        $(".btnImportDocResultFromBwins, .btnImportFileSAP").hide();
                    }
                }
            }
            else {
                if (getAccountType === "TaiKhoanChung" && getInventoryRoleType === 2) {
                    $(`.btnImportFileSAP, .btnImportDocResultFromBwins, 
                    .result_inventory_export_file_txt, .result_inventory_export_file,
                    #ResultInventory_Download_Form`).show();

                } else if (getAccountType === "TaiKhoanRieng") {
                    if (App.User.isGrant("VIEW_ALL_INVENTORY") || App.User.isGrant("VIEW_CURRENT_INVENTORY")) {
                        $(".result_inventory_export_file_txt, .result_inventory_export_file").show();
                        $(".btnImportDocResultFromBwins, .btnImportFileSAP, #ResultInventory_Download_Form").hide();
                    }
                    //Quá ngày kiểm kê:
                    if (moment(currentDate).isAfter(getInventoryDate)) {
                        $(".result_inventory_export_file_txt, .result_inventory_export_file").show();
                        $(".btnImportDocResultFromBwins, .btnImportFileSAP").show();
                    }

                    if (App.User.isGrant("EDIT_INVENTORY")) {
                        $(".result_inventory_export_file_txt, .result_inventory_export_file").show();
                        $(".btnImportDocResultFromBwins, .btnImportFileSAP").show();
                        $("#ResultInventory_Download_Form").show();
                    }
                }
            }
        })

        ClickExportFileTxt();
        ClickFileSAP();


        $.sub(GlobalEventName.inventory_list_resultTabActived, function (e) {
            //Lấy trạng thái đợt kiểm kê hiện tại
            let currentInventoryStatus = $("#InventoryDetail_Status").val();
            if (currentInventoryStatus == "3") {
                $(".btnImportDocResultFromBwins").hide();
            } else {
                $(".btnImportDocResultFromBwins").show();
            }
        })

        //Auto Filter When Click Cell On DataTable:
        AutoFilterClickCellDataTable()

        //Click Export File Inventory Error show modal:
        ExportFileInventoryErrorModal();

        //Export Inventory Error
        ExportFileInventoryError();

        //Export File InventoryError Validate
        ExportFileInventoryErrorValidate();

        //Import MSL Data:
        ClickFileMSL();

    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForResultInventoryLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}

//Import File MSL:
function ClickFileMSL() {
    $(document).delegate(".btnImportMSLUpdate", "click", (e) => {
        let inventoryId = $("#inventory-wrapper").data("id");
        $("#inputMSLUpdate").trigger("click");
    })

    $(document).delegate("#inputMSLUpdate", "change", (e) => {
        let file = e.target.files[0];

        if (file.size > 0 && (/\.(xlsx|xls)$/i.test(file.name))) {
            var inventoryId = $("#inventory-wrapper").data("id")
            var userId = App.User.UserId;

            let url = `${host}/api/inventory/web/${inventoryId}/import/msl-data`;
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
                            text: `${window.languageData[window.currentLanguage]["Cập nhật dữ liệu MSL thành công."]}`,
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        });
                    }

                    ResultInventoryHandler.drawTable();
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
                text: window.languageData[window.currentLanguage]["Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành cập nhật dữ liệu từ MSL."],
                confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                width: '30%'
            })
        }
        $("#inputMSLUpdate").val("")
    })
}


function ExportFileInventoryErrorValidate() {

    $('#InventoryError_Value, #InventoryError_Number').on('input', function () {
        this.value = this.value.replace(/[^0-9]/g, '');
    });

    $("#ExportInventoryError_Form").validate({
        rules: {
            InventoryError_Plant: {
                required: true
            },
            InventoryError_AccountAssignee: {
                required: true
            },
            InventoryError_Value: {
                required: true
            }
        },
        messages: {
            InventoryError_Plant: {
                required: window.languageData[window.currentLanguage]["Vui lòng lựa chọn Plant."]
            },
            InventoryError_AccountAssignee: {
                required: window.languageData[window.currentLanguage]["Vui lòng lựa chọn tài khoản."]
            },
            InventoryError_Value: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập giá trị."]
            }
        },
    });

}

function ExportFileInventoryError() {

    $(document).delegate("#btn_export_inventory_error", "click", (e) => {

        let valid = $("#ExportInventoryError_Form").valid();

        if (valid) {
            var inventoryId = $("#inventory-wrapper").data("id");

            var plant = $("#InventoryError_Plant").val();
            var assigneeAccountId = $("#InventoryError_AccountAssignee").val();
            var errorMoney = $('#InventoryError_Value').val();
            var errorQuantity = $('#InventoryError_Number').val();
            var componentCode = $("#InventoryError_ComponentCode").val();

            loading(true)

            let filterData = {
                InventoryId: inventoryId,
                Plant: plant,
                AssigneeAccountId: assigneeAccountId,
                ErrorMoney: errorMoney,
                ErrorQuantity: errorQuantity,
                ComponentCode: componentCode
            };
            var url = `/inventory/error/export`;
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

                        link.download = `FileSaiSo_${formattedTime}.xlsx`;
                        link.click();

                        $('#InventoryResultDownloadFormModal').modal("hide");
                        toastr.success(window.languageData[window.currentLanguage]["Xuất file sai số thành công."]);

                    } else {
                        toastr.error(window.languageData[window.currentLanguage]["Không tìm thấy file."]);
                    }

                },
                error: function (error) {
                    toastr.error(window.languageData[window.currentLanguage]["Không tìm thấy file."]);
                },
                complete: function () {
                    loading(false)
                }
            });
        }
    })

}

function ExportFileInventoryErrorModal() {
    $(".inventory_error_export_file").click(() => {
        //Call API get filter:
        var link = $("#APIGateway").val();

        $.ajax({
            type: "get",
            url: link + `/api/inventory/web/groups/inventory/error/filters`,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (res) {

                if (res.code == 200) {

                    let firstOpts = `<option disabled selected>${window.languageData[window.currentLanguage]["Chọn plant"]}...</option>`
                    let secondOpts = `<option disabled selected>${window.languageData[window.currentLanguage]["Chọn tài khoản"]}...</option>`
                    let resultHtml = firstOpts

                    resultHtml += res?.data.plants.map(item => {

                        return `<option value="${item}">${item}</option>`
                    }).join("");

                    $('#InventoryError_Plant').html(resultHtml);

                    let resHtml = secondOpts

                    resHtml += res?.data.assigneeAccounts.map(item => {

                        return `<option value="${item.assigneeAccountId}">${item.userName}</option>`
                    }).join("");

                    $('#InventoryError_AccountAssignee').html(resHtml);

                }

            },
            error: function (error) {
                toastr.error(error)
            }
        });


        $("#InventoryErrorModal").modal("show");
    })
}

function AutoFilterClickCellDataTable() {
    $('#monitoring-table').on('click', 'tbody td', function () {
        var mainTable = $('#monitoring-table').DataTable();
        //Lấy giá trị cell khi click
        var cellValue = mainTable.cell(this).data();

        // Lấy vị trí cột của cell được click
        var cellIndex = mainTable.cell(this).index().column;

        // Lấy tên class của cột trong thead
        var columnClass = $('#monitoring-table thead th').eq(cellIndex).attr('auto-filter-key');

        if (columnClass !== undefined) {

            if (columnClass == "ResultDoc_ComponentCode") {
                $("#result_inventory-component_code").val(cellValue);
                
            }else if (columnClass == "ResultDoc_ModelCode") {
                $("#result_inventory-model_code").val(cellValue);

            }else if (columnClass == "ResultDoc_Plant") {
                $("#result_inventory-plant").val(cellValue);

            }else if (columnClass == "ResultDoc_WHLoc") {
                $("#result_inventory-WHLoc").val(cellValue);

            }
            ResultInventoryHandler.drawTable();
        }
    });
}

function ResultInventoryDownloadForm() {
    $("#ResultInventory_Download_Form").click(function () {
        $("#InventoryResultDownloadFormModal").modal("show");
    });
}

function CloseModalResultInventoryForm() {
    $("#ResultInventory_Btn_Close").click(function () {
        $("#InventoryResultDownloadFormModal").modal("hide");
    });
}

//Click Export File txt:
function ClickExportFileTxt() {
    $(document).delegate(".result_inventory_export_file_txt", "click", (e) => {
        var inventoryId = $("#inventory-wrapper").data("id")

        var plant = $("#result_inventory-plant").val();
        var wHLoc = $("#result_inventory-WHLoc").val();
        var docNumberFrom = $("#input_docCode_from").val().replaceAll(',', '');
        var docNumberTo = $("#input_docCode_to").val().replaceAll(',', '');
        var componentCode = $("#result_inventory-component_code").val();
        var modelCode = $("#result_inventory-model_code").val();
        var docTypes = $("#result_inventory-type_form").val();

        //Check click Tat Ca:
        var isCheckAllDocType = '-1';
        let isClickAllDocType = document.querySelector('#result_inventory-type_form').isAllSelected();
        if (isClickAllDocType) {
            isCheckAllDocType = "-1";
        } else {
            isCheckAllDocType = "";
        }

        //Get column order(Columns: ComponentCode, ErrorQuantity, ErrorMoneyAbs)
        let order = ResultInventoryHandler.getResultDatatable().order();
        let sortColumnIndex = order[0][0];
        let sortColumnDirection = order[0][1];
        let sortColumn = ResultInventoryHandler.getResultDatatable().settings().init().columns[sortColumnIndex].name;


        let filterData = {
            Plant: plant,
            WHLoc: wHLoc,
            DocNumberFrom: docNumberFrom,
            DocNumberTo: docNumberTo,
            ComponentCode: componentCode,
            ModelCode: modelCode,
            DocTypes: docTypes,
            IsCheckAllDocType: isCheckAllDocType,
            InventoryId: inventoryId,
            OrderColumn: sortColumn,
            OrderColumnDirection: sortColumnDirection
        };

        loading(true)

        var url = `/inventory/summary/export-txt`;
        $.ajax({
            type: 'POST',
            url: url,
            data: filterData,
            cache: false,
            xhrFields: {
                responseType: 'blob'
            },
            success: function (response) {
                //Lấy ngày tháng hiện tại theo định dạng: yyyymmdd_hhmmss:
                var currentTime = new Date();
                var formattedTime = moment(currentTime).format("YYYYMMDD_HHmmss");

                if (response) {
                    var blob = new Blob([response], { type: "text/plain" });
                    const fileURL = URL.createObjectURL(blob);
                    const link = document.createElement('a');
                    link.href = fileURL;
                    link.download = `Tonghopketqua_${formattedTime}.txt`;
                    link.click();
                } else {
                    toastr.error(window.languageData[window.currentLanguage]["Không tìm thấy file."]);
                }
                toastr.success(window.languageData[window.currentLanguage]["Xuất txt thành công."]);
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

//Import File SAP:
function ClickFileSAP() {
    $(document).delegate(".btnImportFileSAP", "click", (e) => {
        let inventoryId = $("#inventory-wrapper").data("id");
        ResultInventoryHandler.checkExistDoctypeA(inventoryId).then(res => {
            if (res.code == 200) {
                $("#inputImportFileSAP").trigger("click");
            }
        }).catch(err => {
            if (err.responseJSON.code == 404) {
                Swal.fire({
                    title: `<b>${window.languageData[window.currentLanguage]["Không thể tạo phiếu"]}</b>`,
                    text: window.languageData[window.currentLanguage]["Vui lòng tạo phiếu A trước khi thực hiện thêm dữ liệu từ SAP."],
                    confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                    width: '30%'
                })
            }
        }).finally(() => {
        })

    })

    $(document).delegate("#inputImportFileSAP", "change", (e) => {
        let file = e.target.files[0];

        if (file.size > 0 && (/\.(xlsx|xls)$/i.test(file.name))) {
            var inventoryId = $("#inventory-wrapper").data("id")
            var userId = App.User.UserId;

            let url = `${host}/api/inventory/web/${inventoryId}/import-sap`;
            let formData = new FormData();
            formData.append("file", file);
            formData.append("userId", userId);
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
                            text: `${window.languageData[window.currentLanguage]["Thêm thành công"]} ${response?.successCount || 0} ${window.languageData[window.currentLanguage]["dòng dữ liệu từ SAP và có"]} ${response?.failCount || 0} ${window.languageData[window.currentLanguage]["dòng dữ liệu lỗi không thể thực hiện import."]}
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
                            text: `${window.languageData[window.currentLanguage]["Thêm dữ liệu từ SAP thành công."]}`,
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        });
                    }

                    ResultInventoryHandler.drawTable();
                },
                error: function (error) {
                    if (error?.responseJSON?.code === 95) {
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]["Không thể tạo phiếu"]}</b>`,
                            text: window.languageData[window.currentLanguage][error?.responseJSON?.message] || "Có lỗi khi thực hiện import.",
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        })
                        return;
                    }

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
                text: window.languageData[window.currentLanguage]["Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành thêm dữ liệu từ SAP."],
                confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                width: '30%'
            })
        }
        $("#inputImportFileSAP").val("")
    })
}


; var ResultInventoryHandler = (function () {
    let root = {
        parentEl: $("#tab-synthetic-result")
    }

    let DocumentResultDatatable;
    let dataFilter = {};
    let formValidator;

    const APIs = {
        ExportExcelAPI: function (model) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/web/document-results/export-excel`;

                $.ajax({
                    type: 'POST',
                    url: url,
                    contentType: 'application/json',
                    data: JSON.stringify(model),
                    success: function (response) {
                        resolve(response)
                    },
                    error: function (error) {
                        reject(error)
                    }
                });
            })
        },

        ImportResultFromBwinAPI: function (inventoryId, file) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/web/${inventoryId}/import-result-bwins`;

                let formData = new FormData();
                formData.append("file", file);

                $.ajax({
                    type: 'POST',
                    url: url,
                    //contentType: 'application/json',
                    contentType: false,
                    processData: false,
                    data: formData,
                    success: function (response) {
                        resolve(response)
                    },
                    error: function (error) {
                        reject(error)
                    }
                });
            })
        },
        CheckExistDocumentA: function (inventoryId) {
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
        },
        AggregateDocResults: function (inventoryId) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/web/${inventoryId}/doc-result/aggregate`;

                $.ajax({
                    type: 'GET',
                    url: url,
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

    function Cache() {
        root.Table = $(root.parentEl).find("#monitoring-table");
        root.btnSearch = $(root.parentEl).find("#btn-search-create-monitoring");

        root.btnExportExcel = $(root.parentEl).find("#export_file");

        root.searchForm = $(root.parentEl).find("#documentResultForm");

        root.btnAggregateQuantity = $(root.parentEl).find(".btnAggregateQuantity");
    }

    function InitDataTable() {
        if (DocumentResultDatatable) {
            DocumentResultDatatable.draw();
        }
        if (!DocumentResultDatatable) {
            DocumentResultDatatable = root.Table.DataTable({
                "bDestroy": true,
                "processing": `<div class="spinner"></div>`,
                pagingType: 'full_numbers',
                'language': {
                    'loadingRecords': `<div class="spinner"></div>`,
                    'processing': '<div class="spinner"></div>',
                },
                "scrollX": true,
                scrollCollapse: true,
                select: true,
                serverSide: true,
                "searching": false,
                responsive: true,
                "lengthMenu": [10, 30, 50, 200],
                dom: 'rt<"bottom"flp><"clear">',
                "ordering": true,
                order: [[2, "asc"]],
                "ajax": {
                    "url": `${host}/api/inventory/web/document-results`,
                    "type": "POST",
                    "contentType": "application/x-www-form-urlencoded",
                    dataType: "json",
                    async: true,
                    data: function (data) {
                        dataFilter.Plant = root.parentEl.find("#result_inventory-plant").val();
                        dataFilter.WHLoc = root.parentEl.find("#result_inventory-WHLoc").val();
                        dataFilter.DocNumberFrom = root.parentEl.find("#input_docCode_from").val().replaceAll(',', '');
                        dataFilter.DocNumberTo = root.parentEl.find("#input_docCode_to").val().replaceAll(',', '');
                        dataFilter.ComponentCode = root.parentEl.find("#result_inventory-component_code").val();
                        dataFilter.ModelCode = root.parentEl.find("#result_inventory-model_code").val();
                        dataFilter.AssigneeAccount = $("#input_user_distribution_inventory").val();
                        dataFilter.InventoryId = $("#inventory-wrapper").attr("data-id");

                        dataFilter.DocTypes = root.parentEl.find("#result_inventory-type_form").val();

                        //Check click Tat Ca:
                        let isCheckAllDocType = document.querySelector('#result_inventory-type_form').isAllSelected();
                        if (isCheckAllDocType) {
                            dataFilter.IsCheckAllDocType = "-1";
                        } else {
                            dataFilter.IsCheckAllDocType = "";
                        }

                        Object.assign(data, dataFilter);
                        return data;
                    },
                    "dataSrc": function ({ data }) {
                        return data;
                    }
                },
                "drawCallback": function (settings) {
                    let totalPages = DocumentResultDatatable.page.info().pages;
                    let totalRecords = DocumentResultDatatable.page.info().recordsTotal;

                    let currPage = DocumentResultDatatable.page() + 1;
                    if (currPage == 1) {
                        root.parentEl.find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                        root.parentEl.find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                    }
                    if (currPage == totalPages) {
                        root.parentEl.find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                        root.parentEl.find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                    }

                    root.parentEl.find(".pagination").before(`<div class="h5 datatable_total_records"> <b>${window.languageData[window.currentLanguage]["Tổng"]}</b>: ${totalPages} <span class="total_record_seperate"> | </span></div>`)

                    if (totalRecords > 0) {
                        root.parentEl.find("#export_file").attr("disabled", false);
                        root.parentEl.find("#export_file").removeClass("btn_disabled");

                        root.parentEl.find("#export-tree").attr("disabled", false);
                        root.parentEl.find("#export-tree").removeClass("btn_disabled");
                    } else {
                        root.parentEl.find("#export_file").attr("disabled", true);
                        root.parentEl.find("#export_file").removeClass("btn_disabled").addClass("btn_disabled");

                        root.parentEl.find("#export-tree").attr("disabled", true);
                        root.parentEl.find("#export-tree").removeClass("btn_disabled").addClass("btn_disabled");
                    }

                    if (totalRecords <= 10) {
                        $(".container-list-view .bottom").hide()
                    }

                },
                "columns": [
                    {
                        "data": "",
                        "name": "STT",
                        "render": function (data, type, row, index) {
                            let pagesize = index.settings._iDisplayLength;
                            let currentRow = ++index.row;
                            let currentPage = DocumentResultDatatable.page() + 1;

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
                        "data": "componentCode",
                        "name": "ComponentCode",
                        "autoWidth": true,
                        "createdCell": function (td, cellData, rowData, row, col) {
                            $(td).addClass('blue-underline-text');
                        }
                    },
                    {
                        "data": "modelCode",
                        "name": "ModelCode",
                        "autoWidth": true,
                        "sortable": false,
                        "createdCell": function (td, cellData, rowData, row, col) {
                            $(td).addClass('blue-underline-text');
                        }
                    },
                    {
                        "data": "plant",
                        "name": "Plant",
                        "autoWidth": true,
                        "sortable": false,
                        "createdCell": function (td, cellData, rowData, row, col) {
                            $(td).addClass('blue-underline-text');
                        }
                    },
                    {
                        "data": "whLoc",
                        "name": "WH Loc.",
                        "autoWidth": true,
                        "sortable": false,
                        "createdCell": function (td, cellData, rowData, row, col) {
                            $(td).addClass('blue-underline-text');
                        }
                    },
                    {
                        "data": "quantity", "name": "quantity", render: function (data) {
                            if (data > 0) {
                                return ValidateInputHelper.Utils.convertDecimalInventory(data);
                            }
                            return data;
                        },
                        "autoWidth": true,
                        "sortable": false
                    },
                    {
                        "data": "totalQuantity", "name": "totalQuantity", render: function (data) {
                            if (data > 0) {
                                return ValidateInputHelper.Utils.convertDecimalInventory(data);
                            }
                            return data;
                        }, "autoWidth": true,
                        "sortable": false
                    },
                    {
                        "data": "accountQuantity", "name": "accountQuantity", render: function (data) {
                            if (data > 0) {
                                return ValidateInputHelper.Utils.convertDecimalInventory(data);
                            }
                            return data;
                        }, "autoWidth": true,
                        "sortable": false
                    },
                    {
                        "data": "errorQuantity", "name": "ErrorQuantity", render: function (data) {
                            return ValidateInputHelper.Utils.convertDecimalInventory(data);
                        }, "autoWidth": true
                    },
                    {
                        "data": "errorQuantityAbs", "name": "errorQuantityAbs", render: function (data) {
                            return ValidateInputHelper.Utils.convertDecimalInventory(data);
                        }, "autoWidth": true,
                        "sortable": false
                    },
                    {
                        "data": "errorMoney", "name": "errorMoney", render: function (data) {
                            return ValidateInputHelper.Utils.convertDecimalInventory(data);
                        }, "autoWidth": true,
                        "sortable": false
                    },
                    {
                        "data": "errorMoneyAbs", "name": "ErrorMoneyAbs", render: function (data) {
                            return ValidateInputHelper.Utils.convertDecimalInventory(data);
                        }, "autoWidth": true
                    },
                    {
                        "data": "unitPrice", "name": "unitPrice", render: function (data) {
                            return ValidateInputHelper.Utils.convertDecimalInventory(data);
                        }, "autoWidth": true,
                        "sortable": false
                    },
                    {
                        "data": "docCode",
                        "name": "Mã phiếu",
                        "autoWidth": true,
                        "sortable": false,
                        "createdCell": function (td, cellData, rowData, row, col) {
                            //$(td).addClass('blue-underline-text');
                            $(td).attr('data-id', `${rowData.id}`);
                        }
                    },
                    { "data": "no", "name": "No.", "autoWidth": true, "sortable": false },
                    { "data": "stockType", "name": "Stock type", "autoWidth": true, "sortable": false },
                    { "data": "specialStock", "name": "Special stock", "autoWidth": true, "sortable": false },
                    { "data": "saleOrderNo", "name": "S/O No.", "autoWidth": true, "sortable": false },
                    { "data": "physInv", "name": "physicInv", "autoWidth": true, "sortable": false },
                    { "data": "productOrderNo", "name": "productOrderNo", "autoWidth": true, "sortable": false },
                    { "data": "inventoryBy", "name": "inventoryBy", "autoWidth": true, "sortable": false },
                    { "data": "inventoryAt", "name": "inventoryAt", "autoWidth": true, "sortable": false },
                    { "data": "confirmedBy", "name": "confirmedBy", "autoWidth": true, "sortable": false },
                    { "data": "confirmedAt", "name": "confirmedAt", "autoWidth": true, "sortable": false },
                    { "data": "auditBy", "name": "auditBy", "autoWidth": true, "sortable": false },
                    { "data": "auditAt", "name": "auditAt", "autoWidth": true, "sortable": false },
                    { "data": "componentName", "name": "componentName", "autoWidth": true, "sortable": false },
                    { "data": "position", "name": "Vị trí", "autoWidth": true, "sortable": false },
                    { "data": "assemblyLoc", "name": "assemblyLoc", "autoWidth": true, "sortable": false },
                    { "data": "vendorCode", "name": "Vendor code", "autoWidth": true, "sortable": false },
                    { "data": "saleOrderList", "name": "S/O List", "autoWidth": true, "sortable": false },

                    { "data": "csap", "name": "cSap", "autoWidth": true, "sortable": false },
                    { "data": "ksap", "name": "kSap", "autoWidth": true, "sortable": false },
                    { "data": "msap", "name": "mSap", "autoWidth": true, "sortable": false },
                    { "data": "osap", "name": "oSap", "autoWidth": true, "sortable": false },
                ]
            });
        }
    }

    function Events() {
        root.parentEl.delegate("#btn_search_result", "click", function (e) {
            let validForm = root.searchForm.valid();
            if (validForm) {
                DocumentResultDatatable.draw();
            }
        })

        root.parentEl.delegate(".btn_reset_document_result", "click", function () {
            $("#result_inventory-type_form")[0].reset();
            $("#result_inventory-type_form")[0].toggleSelectAll(true);

            $("#result_inventory-plant").val("");
            $("#result_inventory-WHLoc").val("");
            $("#input_docCode_from").val("");
            $("#input_docCode_to").val("");
            $("#result_inventory-component_code").val("");
            $("#result_inventory-model_code").val("");

            formValidator.resetForm();

            //Re-render datatable
            DocumentResultDatatable.draw();
        })

        root.btnExportExcel.click(function (e) {
            let filterModel = {};
            filterModel.Plant = root.parentEl.find("#result_inventory-plant").val();
            filterModel.WHLoc = root.parentEl.find("#result_inventory-WHLoc").val();
            filterModel.DocNumberFrom = root.parentEl.find("#input_docCode_from").val().replaceAll(',', '');
            filterModel.DocNumberTo = root.parentEl.find("#input_docCode_to").val().replaceAll(',', '');
            filterModel.ComponentCode = root.parentEl.find("#result_inventory-component_code").val();
            filterModel.ModelCode = root.parentEl.find("#result_inventory-model_code").val();
            filterModel.AssigneeAccount = $("#input_user_distribution_inventory").val();
            filterModel.InventoryId = $("#inventory-wrapper").attr("data-id");
            filterModel.DocTypes = root.parentEl.find("#result_inventory-type_form").val();


            //Get column order(Columns: ComponentCode, ErrorQuantity, ErrorMoneyAbs)
            let order = DocumentResultDatatable.order();
            let sortColumnIndex = order[0][0];
            let sortColumnDirection = order[0][1];
            let sortColumn = DocumentResultDatatable.settings().init().columns[sortColumnIndex].name;

            filterModel.OrderColumn = sortColumn;
            filterModel.OrderColumnDirection = sortColumnDirection;


            //Check click Tat Ca:
            //let isCheckAllDocType = document.querySelector('#result_inventory-type_form').isAllSelected();
            //filterModel.IsCheckAllDocType = isCheckAllDocType ? "-1" : "";

            loading(true);
            APIs.ExportExcelAPI(filterModel).then(res => {
                if (res.bytes.length) {
                    let convertedBytes = FileTemplateHandler.utils.base64ToArrayBuffer(res.bytes);
                    FileTemplateHandler.utils.saveByteArr(convertedBytes, res.fileType, res.fileName);
                }
            }).catch(err => {
                Swal.fire({
                    title: `<b>${window.languageData[window.currentLanguage][err?.responseJSON?.message]}</b>`,
                    confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                    width: '30%'
                })
            }).finally(() => {
                loading(false);
            })
        })

        root.parentEl.find(".document_result_fileTemplate").click(function (e) {
            let fileName = $(this).attr("fileName");

            loading(true);
            FileTemplateHandler.download(fileName).then(res => {
            }).catch(err => {

            }).finally(() => {
                loading(false);
            })
        })

        root.parentEl.find("#inputDocResultFromBwins").change(function (e) {
            let inventoryId = $("#inventory-wrapper").data("id");
            let file = this.files[0];

            loading(true);
            APIs.ImportResultFromBwinAPI(inventoryId, file).then(function (res) {
                if (res.failCount > 0) {
                    Swal.fire({
                        title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                        text: `${window.languageData[window.currentLanguage]["Upload kết quả từ Bwins thành công"]} ${res?.successCount || 0} ${window.languageData[window.currentLanguage]["dòng dữ liệu và có"]} ${res?.failCount || 0} ${window.languageData[window.currentLanguage]["dòng dữ liệu lỗi không thể thực hiện import."]}
                        ${window.languageData[window.currentLanguage]["Vui lòng ấn “Đồng ý” để xem dữ liệu lỗi."]}`,
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
                            let convertedByte = FileTemplateHandler.utils.base64ToArrayBuffer(res.bytes);
                            FileTemplateHandler.utils.saveByteArr(convertedByte, res.fileType, res.fileName);

                            //Render lại danh sách sau khi import kết quả bwin
                            DocumentResultDatatable.draw();
                        }
                    });
                } else if (res.failCount == 0) {
                    Swal.fire({
                        title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                        text: window.languageData[window.currentLanguage]["Upload kết quả từ Bwins thành công."],
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    });
                }

                DocumentResultDatatable.draw();

            }).catch(err => {
                let title = `${window.languageData[window.currentLanguage]["Thông báo"]}`;
                //Nếu file sai định dạng code = 70
                if (err?.responseJSON?.code == 70) {
                    title = window.languageData[window.currentLanguage]["File sai định dạng"];
                } else if (err?.responseJSON?.code == ServerResponseStatusCode.NotExistDocTypeA) {
                    title = window.languageData[window.currentLanguage]["Không thể tạo phiếu"];
                }
                Swal.fire({
                    title: `<b>${title}</b>`,
                    text: window.languageData[window.currentLanguage][err?.responseJSON?.message] || window.languageData[window.currentLanguage]["Có lỗi khi thực hiện import."],
                    confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                    width: '30%'
                })
            }).finally(() => {
                loading(false);
            })

            //Reset lại file để có thể upload lại file
            $(this).val("");
        })

        root.parentEl.find(".btnImportDocResultFromBwins").click(function (e) {
            let inventoryId = $("#inventory-wrapper").data("id");
            APIs.CheckExistDocumentA(inventoryId).then(res => {
                if (res.code == 200) {
                    root.parentEl.find("#inputDocResultFromBwins").trigger("click");
                }
            }).catch(err => {
                if (err.responseJSON.code == 404) {
                    Swal.fire({
                        title: `<b>${window.languageData[window.currentLanguage]["Không thể tạo phiếu"]}</b>`,
                        text: window.languageData[window.currentLanguage]["Vui lòng tạo phiếu A trước khi thực hiện upload kết quả từ Bwin."],
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    })
                }
            }).finally(() => {
            })

        })

        root.parentEl.delegate("#documentResultForm", "keypress", ValidateInputHelper.FormEnter(() => {
            let validForm = root.searchForm.valid();
            if (validForm) {
                DocumentResultDatatable.draw();
            }
        }))

        root.btnAggregateQuantity.click(function (e) {

            Swal.fire({
                title: `<b>${window.languageData[window.currentLanguage]["Xác nhận tổng hợp phiếu"]}</b>`,
                text: ``,
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
                    let inventoryId = $("#inventory-wrapper").data("id");
                    loading(true);

                    APIs.AggregateDocResults(inventoryId).then(res => {
                        if (res.data) {
                            $("#inventory-wrapper").attr('data-aggregate-at', `${res?.data}`)


                            const d = new Date();
                            const resDate = new Date(res.data);
                            var nextAvailableTime = moment(resDate).add(10, 'm').toDate();
                            //debugger;
                            var diffTime = (nextAvailableTime - d);
                            if (diffTime > 0) {
                                $('#spin-icon').attr('hidden', false);
                                $('#sync-icon').attr('hidden', true);
                                var countDown = setInterval(function () {

                                    var now = new Date().getTime();
                                    var ammount = nextAvailableTime - now;
                                    var minutes = Math.ceil((ammount / 1000) / 60);
                                    var seconds = Math.floor(ammount / 1000);
                                    //debugger
                                    $('.btnAggregateQuantity').attr('disabled', 'disabled');
                                    if (minutes > 1) {
                                        $('#countDown').text(`${window.languageData[window.currentLanguage]["Đợi"]} ${minutes} ${window.languageData[window.currentLanguage]["phút"]}..`);
                                    }
                                    else {
                                        $('#countDown').text(`${window.languageData[window.currentLanguage]["Đợi"]} ${seconds} s..`);
                                    }
                                    if (ammount <= 0) {
                                        clearInterval(countDown);
                                        $('#sync-icon').removeAttr('hidden');
                                        $('#spin-icon').attr('hidden', true);
                                        $('#countDown').text(window.languageData[window.currentLanguage]['Tổng hợp']);
                                        $('.btnAggregateQuantity').removeAttr('disabled');

                                        //Render lại danh sách 
                                        DocumentResultDatatable.draw();
                                    }


                                }, 500);


                            }

                        }


                        toastr.success(res.message);
                    }).catch(err => {
                        //toastr.error(err.responseJSON.message);
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]["Có lỗi xảy ra"]}</b>`,
                            text: window.languageData[window.currentLanguage]['Không tồn tại cụm: '] + err.responseJSON.message,
                            confirmButtonText: window.languageData[window.currentLanguage]['Copy mã'],
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
                                navigator.clipboard.writeText(err.responseJSON.message);
                                toastr.success(window.languageData[window.currentLanguage]['Đã copy mã vào clipboard']);
                            }
                        });

                    }).finally(() => {
                        loading(false);
                    })
                }
            });
        })
    }

    function ValidateSearchForm() {
        root.parentEl.find(`#result_inventory-plant, #result_inventory-WHLoc`).on("keypress keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(4));

        //Chặn nhập khoảng trắng và cắt khoảng trắng khi paste
        root.parentEl.find(`#result_inventory-plant, #result_inventory-WHLoc`).on("keypress", ValidateInputHelper.PreventWhiteSpace);
        root.parentEl.find(`#result_inventory-plant, #result_inventory-WHLoc`).on("keyup", ValidateInputHelper.RemoveWhiteSpaceOnKeyup);


        //root.parentEl.find(`#input_docCode_from, #input_docCode_to`).on("keypress", ValidateInputHelper.OnlyNumerOnKeyPress);
        //root.parentEl.find(`#input_docCode_from, #input_docCode_to`).on("keypress", ValidateInputHelper.LimitRawNumber(5));
        //root.parentEl.find(`#input_docCode_from, #input_docCode_to`).on("keypress", ValidateInputHelper.PreventSepcialCharacterOnKeyPress);
        //root.parentEl.find(`#input_docCode_from, #input_docCode_to`).on("keyup", ValidateInputHelper.PreventSepcialCharacterOnKeyPress);
        //root.parentEl.find(`#input_docCode_from, #input_docCode_to`).on("keyup", ValidateInputHelper.LimitRawNumber(5));

        root.parentEl.find(`#input_docCode_from, #input_docCode_to`).on("input", function (event) {
            let value = event.target.value;
            event.target.value = value.replace(/\D/g, '').slice(0, 5);
        });

        root.parentEl.find(`#input_docCode_from, #input_docCode_to`).on("keyup", function (event) {
            let value = event.target.value;
            event.target.value = value.replace(/\D/g, '').slice(0, 5);
        });


        root.parentEl.find(`#result_inventory-component_code, #result_inventory-model_code`).on("keypress keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(12));

        //Chặn nhập khoảng trắng và cắt khoảng trắng khi paste
        root.parentEl.find(`#result_inventory-component_code, #result_inventory-model_code`).on("keypress", ValidateInputHelper.PreventWhiteSpace);
        root.parentEl.find(`#result_inventory-component_code, #result_inventory-model_code`).on("keyup", ValidateInputHelper.RemoveWhiteSpaceOnKeyup);

        jQuery.validator.addMethod("docCodeNumberRange_document_result", function (value, element) {
            let valid = true;
            let fromValue = $("#input_docCode_from").val();
            let toValue = $("#input_docCode_to").val();

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
                QuantityRangeFrom: {
                    required: {
                        depends: function () {
                            let fromValue = root.parentEl.find(`#input_docCode_from`).val().length;
                            let dependValue = root.parentEl.find(`#input_docCode_to`).val().length;
                            if (dependValue) {
                                return true;
                            } else {
                                if (fromValue) return true;
                                return false;
                            }
                        }
                    },
                    docCodeNumberRange_document_result: true
                },
                QuantityRangeTo: {
                    required: {
                        depends: function () {
                            let toValue = root.parentEl.find(`#input_docCode_to`).val().length;
                            let dependValue = root.parentEl.find(`#input_docCode_from`).val().length;
                            if (dependValue) {
                                return true;
                            } else {
                                if (toValue) return true;
                                return false;
                            }
                        }
                    },
                    docCodeNumberRange_document_result: true
                },
            },
            messages: {
                QuantityRangeFrom: {
                    required: window.languageData[window.currentLanguage]["Vui lòng nhập số phiếu."]
                },
                QuantityRangeTo: {
                    required: window.languageData[window.currentLanguage]["Vui lòng nhập số phiếu."]
                },
            }
        }

        formValidator = $(root.parentEl).find("#documentResultForm").validate(validateModel);
    }

    function PreLoad() {
        VirtualSelect.init({
            ele: '#result_inventory-type_form',
            selectAllText: window.languageData[window.currentLanguage]['Tất cả'],
            noOptionsText: window.languageData[window.currentLanguage]['Không có kết quả'],
            noSearchResultsText: window.languageData[window.currentLanguage]['Không có kết quả'],
            searchPlaceholderText: window.languageData[window.currentLanguage]['Tìm kiếm'],
            allOptionsSelectedText: window.languageData[window.currentLanguage]['Tất cả'],
            optionsSelectedText: window.languageData[window.currentLanguage]["đợt kiểm kê đã được chọn"],
            selectAllOnlyVisible: true,
            hideClearButton: true,
        });

        $("#result_inventory-type_form")[0].reset();
        $("#result_inventory-type_form")[0].toggleSelectAll(true);

        ValidateSearchForm();
    }

    function DrawTable() {
        DocumentResultDatatable.draw();
    }

    function GetResultDatatable() {
        return DocumentResultDatatable;
    }

    function Init() {
        Cache()
        PreLoad()
        Events()
    }

    return {
        init: Init,
        initDataTable: InitDataTable,
        drawTable: DrawTable,
        checkExistDoctypeA: APIs.CheckExistDocumentA,
        getResultDatatable: GetResultDatatable
    }
})();