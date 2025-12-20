var host = $("#APIGateway").val();
let InventoryDocDatatable;
let DocTypeCDetailDatatable;

//KO view model wrap all page
var inventoryViewModel = new InventoryView();

$(function () {
    waitForCreateInventoryLanguageData();

});

function waitForCreateInventoryLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        CreateInventory();
        ResetSearchCreateInventory();
        ValidateSearchCreateInventory();
        BtnExportClusterTree();
        BtnCancelClusterTree();

        CreateInventoryHandler.init();
        //VirtualSelect.init({
        //    ele: '#CreateInventory_Department,#CreateInventory_Area,#CreateInventory_Form_Type',
        //    selectAllText: 'Tất cả',
        //    noOptionsText: 'Không có kết quả',
        //    noSearchResultsText: 'Không có kết quả',
        //    searchPlaceholderText: 'Tìm kiếm...',
        //    allOptionsSelectedText: 'Tất cả',
        //    optionsSelectedText: "điều kiện đã được chọn",
        //    selectAllOnlyVisible: true,
        //    hideClearButton: true,
        //});


        //Click nut tao phieu kiem ke:
        $(document).delegate("#inventory-create", "click", (e) => {
            var dropdownSelectors = [
                `#CreateInventory_Department`,
                `#CreateInventory_Area`,
                `#CreateInventory_Form_Type`
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


            $("#CreateInventory_Department")[0].reset();
            $("#CreateInventory_Department")[0].toggleSelectAll(true);

            $("#CreateInventory_Area")[0].reset();
            $("#CreateInventory_Area")[0].toggleSelectAll(true);

            $("#CreateInventory_Form_Type")[0].reset();
            $("#CreateInventory_Form_Type")[0].toggleSelectAll(true);

            InitListInventoryDocument_Datatable();
            $("#CreateInventory_Btn_Reset").trigger("click");
            //Phân quyền các nút:
            var getInventoryStatus = $("#inventory-wrapper").attr("data-status");
            let getAccountType = App.User.AccountType;
            let getInventoryRoleType = App.User.InventoryLoggedInfo.InventoryRoleType;

            let getInventoryDate = $("#inventory-wrapper").attr("data-inventory-date");
            let currentDate = moment().format("YYYY-MM-DD");
            //TH1: Trang thái phiếu đã hoàn thành:
            if (getInventoryStatus === '3') {
                if (getAccountType === "TaiKhoanChung" && getInventoryRoleType === 2) {
                    $("#btnCreateform_A").hide();
                    $("#btnCreateform_BE").hide();
                    $("#btnCreateform_C").hide();
                    $(".ListInventoryDocument_Delete").hide();
                    $("#export-tree").show();
                    $("#export_file_inventory_document").show();
                    $("#download-form").show();
                } else if (getAccountType === "TaiKhoanRieng") {
                    if (App.User.isGrant("VIEW_ALL_INVENTORY") || App.User.isGrant("VIEW_CURRENT_INVENTORY")) {
                        $("#btnCreateform_A").hide();
                        $("#btnCreateform_BE").hide();
                        $("#btnCreateform_C").hide();
                        $(".ListInventoryDocument_Delete").hide();
                        $("#export-tree").hide();
                        $("#export_file_inventory_document").show();
                        $("#download-form").hide();
                    }
                    if (App.User.isGrant("EDIT_INVENTORY")) {
                        $("#btnCreateform_A").hide();
                        $("#btnCreateform_BE").hide();
                        $("#btnCreateform_C").hide();
                        $(".ListInventoryDocument_Delete").hide();
                        $("#export-tree").show();
                        $("#export_file_inventory_document").show();
                        $("#download-form").show();
                    }

                    //Quá ngày kiểm kê:
                    if (moment(currentDate).isAfter(getInventoryDate)) {
                        $("#btnCreateform_A").hide();
                        $("#btnCreateform_BE").hide();
                        $("#btnCreateform_C").hide();
                        $(".ListInventoryDocument_Delete").hide();
                        $("#export_file_inventory_document").show();

                        if (App.User.isGrant("EDIT_INVENTORY")) {
                            $("#export-tree").show();
                            $("#download-form").show();
                        }
                        else {
                            $("#export-tree").hide();
                            $("#download-form").hide();
                        }
                    }
                }
            }
            else {
                if (getAccountType === "TaiKhoanChung" && getInventoryRoleType === 2) {
                    $("#btnCreateform_A").show();
                    $("#btnCreateform_BE").show();
                    $("#btnCreateform_C").show();
                    $(".ListInventoryDocument_Delete").show();
                    $("#export-tree").show();
                    $("#export_file_inventory_document").show();
                    $("#download-form").show();
                } else if (getAccountType === "TaiKhoanRieng") {
                    if (App.User.isGrant("VIEW_ALL_INVENTORY") || App.User.isGrant("VIEW_CURRENT_INVENTORY")) {
                        $("#btnCreateform_A").hide();
                        $("#btnCreateform_BE").hide();
                        $("#btnCreateform_C").hide();
                        $(".ListInventoryDocument_Delete").hide();
                        $("#export-tree").hide();
                        $("#export_file_inventory_document").show();
                        $("#download-form").hide();
                    }
                    if (App.User.isGrant("EDIT_INVENTORY")) {
                        $("#btnCreateform_A").show();
                        $("#btnCreateform_BE").show();
                        $("#btnCreateform_C").show();
                        $(".ListInventoryDocument_Delete").show();
                        $("#export-tree").show();
                        $("#export_file_inventory_document").show();
                        $("#download-form").show();
                    }
                    //Quá ngày kiểm kê:
                    if (moment(currentDate).isAfter(getInventoryDate)) {
                        $("#btnCreateform_A").hide();
                        $("#btnCreateform_BE").hide();
                        $("#btnCreateform_C").hide();
                        $(".ListInventoryDocument_Delete").hide();
                        $("#export_file_inventory_document").show();

                        if (App.User.isGrant("EDIT_INVENTORY")) {
                            $("#export-tree").show();
                            $("#download-form").show();
                        }
                        else {
                            $("#export-tree").hide();
                            $("#download-form").hide();
                        }
                    }
                }
            }

            //UserName = 'administrator' => Hide Button "Tất cả các phiếu":
            let checkCurrentUserIsAdministrator = App.User.InventoryLoggedInfo.UserName;
            if (checkCurrentUserIsAdministrator == 'administrator') {
                $('.AllDocumentButton').show()
            }
            else {
                $('.AllDocumentButton').hide()
            }
        });

        ChangeDepartmentGetLocation_InventoryDoc()

        //Click nut search:
        $(document).delegate("#CreateInventory_Btn_Search", "click", (e) => {
            if ($("#CreateInventory_Form").valid()) {
                InventoryDocDatatable.draw()
            }
        });

        //Export Excel:
        ExportFileInventoryDocument()



        //Xoa tat ca:
        //CheckedAllDeteleInventory()
        //ShowAndHideButtonDeleteInventory()
        GetDetailInventoryDocument()

        //search doctype c detail:
        $(document).delegate(".doctypec_detail_search img", "click", (e) => {
            DocTypeCDetailDatatable.draw()
        })

        //Xoa danh sach Phieu kiem Ke:
        /*DeleteInventoryDoc()*/

        //Xuất Excel QRCode Cụm:
        BtnCancelQRCodeClusterTree();
        ExportQRCode()

    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForCreateInventoryLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}


var maxLengthSearchDetailTypeC = 10;
var maxLengthCreateInventoryPlant = 4;
var maxLengthCreateInventoryWhLoc = 4;
var maxLengthCreateInventoryQuantityFrom = 5;
var maxLengthCreateInventoryQuantityTo = 5;
var maxLengthCreateInventoryComponentCode = 12;
var maxLengthCreateInventoryModelCode = 11;
var maxLengthCreateInventoryUserDistribution = 20;

function CreateInventory() {
    $("#view-detail-A").click(function () {
        $("#inventory-list-detail-modal-A").modal("show");
    });

    $("#view-detail-BE").click(function () {
        $("#inventory-list-detail-modal-BE").modal("show");
    });

    $("#view-detail-C").click(function () {
        $("#inventory-list-detail-modal-C").modal("show");
    });

    $(".btn_remove_file").click(function (e) {
        e.preventDefault();
    });

    $("#download-form").click(function (e) {
        $("#InventoryListDownloadFormModal").modal("show");
    });

    $("#export-tree").click(function (e) {

        //Call API get filter:
        var link = $("#APIGateway").val();

        $.ajax({
            type: "get",
            url: link + `/api/inventory/web/groups/filters`,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (res) {

                if (res.code == 200) {

                    let firstOpts = `<option disabled selected>${window.languageData[window.currentLanguage]["Chọn model"]}...</option>`
                    let secondOpts = `<option disabled selected>${window.languageData[window.currentLanguage]["Chọn dòng máy"]}...</option>`
                    let resultHtml = firstOpts

                    resultHtml += res?.data.machineModels.map(item => {

                        return `<option value="${item}">${item}</option>`
                    }).join("");

                    $('#ClusterTreeModel_Model_Code').html(resultHtml);

                    let resHtml = secondOpts

                    resHtml += res?.data.machineTypes.map(item => {

                        return `<option value="${item}">${item}</option>`
                    }).join("");

                    $('#ClusterTreeModel_Machine_Type').html(resHtml);


                }

            },
            error: function (error) {
                toastr.error(error)
            }
        });


        $("#InventoryListExportClusterTreeModal").modal("show");
    });

    $("#export-QRCode").click(function (e) {
        //Call API get filter:
        var link = $("#APIGateway").val();

        $.ajax({
            type: "get",
            url: link + `/api/inventory/web/groups/qrcode/filters`,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (res) {

                if (res.code == 200) {

                    let firstOpts = `<option disabled selected>${window.languageData[window.currentLanguage]["Chọn model"]}...</option>`
                    let secondOpts = `<option disabled selected>${window.languageData[window.currentLanguage]["Chọn dòng máy"]}...</option>`
                    let thirdOpts = `<option disabled selected>${window.languageData[window.currentLanguage]["Chọn chuyền"]}...</option>`
                    let resultHtml = firstOpts

                    resultHtml += res?.data.machineModels.map(item => {

                        return `<option value="${item}">${item}</option>`
                    }).join("");

                    $('#QRCodeClusterTreeModel_Model_Code').html(resultHtml);
                    
                    let resHtml = secondOpts

                    resHtml += res?.data.machineTypes.map(item => {

                        return `<option value="${item}">${item}</option>`
                    }).join("");

                    $('#QRCodeClusterTreeModel_Machine_Type').html(resHtml);

                    let resLineNameHtml = thirdOpts

                    resLineNameHtml += res?.data.lineNames.map(item => {

                        return `<option value="${item}">${item}</option>`
                    }).join("");

                    $('#QRCodeClusterTreeModel_LineName').html(resLineNameHtml);
                }

            },
            error: function (error) {
                toastr.error(error)
            }
        });


        $("#InventoryListExportQRCodeClusterTreeModal").modal("show");
    });
}

function GetDetailInventoryDocument() {
    $(document).delegate("a.in_doc_detail", "click", (e) => {
        //Call Api Xem chi tiết:
        var link = $("#APIGateway").val();

        var inventoryId = $(e.target).attr("data-inventoryid")
        var docId = $(e.target).attr("data-id")
        var docType = $(e.target).attr("data-doctype")

        $.ajax({
            type: "GET",
            url: link + `/api/inventory/web/${inventoryId}/document/${docId}`,
            success: function (res) {
                if (docType == 3) {
                    $("#inventory-list-detail-modal-C").modal("show");

                    $(".input_search_doctypec_detail").val("");

                    $(".doctypec_detail_doccode").text(res?.data?.docCode);
                    $(".doctypec_detail_plant").text(res?.data?.plant);
                    $(".doctypec_detail_whloc").text(res?.data?.whLoc);
                    $(".doctypec_detail_modelcode").text(res?.data?.modelCode);
                    $(".doctypec_detail_stagename").text(res?.data?.stageName);
                    $(".doctypec_detail_department").text(res?.data?.department);
                    $(".doctypec_detail_location").text(res?.data?.location);
                    $(".doctypec_detail_assigneeaccount").text(res?.data?.assigneeAccount);
                    //$(".doctypec_detail_note").text(res?.data?.note);
                    $(".doctypec_detail_createby").text(res?.data?.createdBy);
                    $(".doctypec_detail_createat").text(res?.data?.createdAt);

                    InitDocTypeCDetail_Datatable(inventoryId, docId)
                } else if (docType == 0) {
                    $("#inventory-list-detail-modal-A").modal("show");

                    $(".doctypea_detail_doccode").text(res?.data?.docCode);
                    $(".doctypea_detail_plant").text(res?.data?.plant);
                    $(".doctypea_detail_whloc").text(res?.data?.whLoc);
                    $(".doctypea_detail_componentcode").text(res?.data?.componentCode);
                    $(".doctypea_detail_modelcode").text(res?.data?.modelCode);
                    $(".doctypea_detail_componentname").text(res?.data?.componentName);
                    $(".doctypea_detail_quantity").text(res?.data?.quantity);
                    $(".doctypea_detail_position").text(res?.data?.position);
                    $(".doctypea_detail_sono").text(res?.data?.saleOrderNo);
                    $(".doctypea_detail_department").text(res?.data?.department);
                    $(".doctypea_detail_location").text(res?.data?.location);
                    $(".doctypea_detail_assigneeaccount").text(res?.data?.assigneeAccount);
                    $(".doctypea_detail_stocktypes").text(res?.data?.stockType);
                    $(".doctypea_detail_specialstock").text(res?.data?.specialStock);
                    $(".doctypea_detail_solist").text(res?.data?.saleOrderList);

                    $(".doctypea_detail_physinv").text(res?.data?.physInv);
                    $(".doctypea_detail_fisscalyear").text(res?.data?.fiscalYear);
                    $(".doctypea_detail_item").text(res?.data?.item);
                    $(".doctypea_detail_plannedcount").text(res?.data?.plantedCount);
                    $(".doctypea_detail_columnc").text(res?.data?.columnC);
                    $(".doctypea_detail_columnN").text(res?.data?.columnN);
                    $(".doctypea_detail_columnO").text(res?.data?.columnO);
                    $(".doctypea_detail_columnP").text(res?.data?.columnP);
                    $(".doctypea_detail_columnQ").text(res?.data?.columnQ);
                    $(".doctypea_detail_columnR").text(res?.data?.columnR);
                    $(".doctypea_detail_columnS").text(res?.data?.columnS);
                    $(".doctypea_detail_note").text(res?.data?.note);
                    $(".doctypea_detail_createdBy").text(res?.data?.createdBy);
                    $(".doctypea_detail_createdAt").text(res?.data?.createdAt);

                } else {
                    $("#inventory-list-detail-modal-BE").modal("show");

                    $(".doctypebe_detail_doccode").text(res?.data?.docCode);
                    $(".doctypebe_detail_plant").text(res?.data?.plant);
                    $(".doctypebe_detail_whloc").text(res?.data?.whLoc);
                    $(".doctypebe_detail_componentcode").text(res?.data?.componentCode);
                    $(".doctypebe_detail_modelcode").text(res?.data?.modelCode);
                    $(".doctypebe_detail_componentname").text(res?.data?.componentName);

                    if (docType == 2) {
                        $(".doctypebe_detail_position").text(res?.data?.position);
                    } else {
                        $(".doctypebe_detail_position").text('');
                    }

                    $(".doctypebe_detail_sono").text(res?.data?.saleOrderNo);
                    $(".doctypebe_detail_department").text(res?.data?.department);
                    $(".doctypebe_detail_location").text(res?.data?.location);
                    $(".doctypebe_detail_assigneeaccount").text(res?.data?.assigneeAccount);
                    $(".doctypebe_detail_stocktype").text(res?.data?.stockType);
                    $(".doctypebe_detail_specialstock").text(res?.data?.specialStock);
                    $(".doctypebe_detail_solist").text(res?.data?.saleOrderList);
                    $(".doctypebe_detail_assemblyloc").text(res?.data?.assemblyLoc);
                    $(".doctypebe_detail_vendercode").text(res?.data?.vendorCode);
                    $(".doctypebe_detail_proorderno").text(res?.data?.proOrderNo);
                    $(".doctypebe_detail_note").text(res?.data?.note);
                    $(".doctypebe_detail_createby").text(res?.data?.createdBy);
                    $(".doctypebe_detail_createat").text(res?.data?.createdAt);
                }

            },
            error: function (error) {
                toastr.error(error.message)
            }
        });
    })
}

function InitDocTypeCDetail_Datatable(inventoryId, docId) {
    let host = App.ApiGateWayUrl;
    DocTypeCDetailDatatable = $('#doctypec-detail-table').DataTable({
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
            "url": host + `/api/inventory/web/document/typec`,
            "type": "POST",
            "contentType": "application/x-www-form-urlencoded",
            dataType: "json",
            data: function (data) {
                let dataFilter = {
                    InventoryId: "",
                    DocId: "",
                    ComponentCode: ""
                };

                dataFilter.InventoryId = inventoryId;
                dataFilter.DocId = docId;
                dataFilter.ComponentCode = $(".input_search_doctypec_detail").val();

                Object.assign(data, dataFilter);

                return data;
            },
            "dataSrc": function ({ data }) {
                return data;
            }
        },
        "drawCallback": function (settings) {
            let totalPages = DocTypeCDetailDatatable.page.info().pages;
            let totalRecords = DocTypeCDetailDatatable.page.info().recordsTotal;

            let currPage = DocTypeCDetailDatatable.page() + 1;

            setTimeout(() => {
                if (currPage == 1) {
                    $("#inventory-list-detail-modal-C").find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                    $("#inventory-list-detail-modal-C").find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                }
                if (currPage == totalPages) {
                    $("#inventory-list-detail-modal-C").find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                    $("#inventory-list-detail-modal-C").find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                }
                $("#inventory-list-detail-modal-C").find(".datatable_total_records").remove();
                $("#inventory-list-detail-modal-C").find(".pagination").before(`<div class="h5 datatable_total_records"> <b>${window.languageData[window.currentLanguage]["Tổng"]}</b>: ${totalPages} <span class="total_record_seperate"> | </span>
                </div>`)
                if (totalRecords <= 10) {
                    $("#inventory-list-detail-modal-C .bottom").hide()
                }
            }, 10)

        },
        "columns": [

            {
                "data": "",
                "name": "STT",
                "render": function (data, type, row, index) {
                    let pagesize = index.settings._iDisplayLength;
                    let currentRow = ++index.row;
                    let currentPage = DocTypeCDetailDatatable.page() + 1;

                    let STT = ((currentPage - 1) * pagesize) + currentRow;

                    if (STT < 10) {
                        STT = `0${STT}`;
                    }
                    return STT;
                },
                "autoWidth": true
            },
            { "data": "componentCode", "name": "Mã linh kiện", "autoWidth": true },
            { "data": "quantityOfBom", "name": "BOM", "autoWidth": true }
        ],
    });
}



function ResetSearchCreateInventory() {
    $("#CreateInventory_Btn_Reset").click(function () {
        $("#CreateInventory_Plant").val("");
        $("#CreateInventory_Plant_WhLoc").val("");

        $("#CreateInventory_Department")[0].reset();
        $("#CreateInventory_Department")[0].toggleSelectAll(true);

        $("#CreateInventory_Area")[0].reset();
        $("#CreateInventory_Area")[0].toggleSelectAll(true);

        $("#CreateInventory_Form_Type")[0].reset();
        $("#CreateInventory_Form_Type")[0].toggleSelectAll(true);

        $("#CreateInventory_Quantity_From").val("");
        $("#CreateInventory_Quantity_From-error").hide();
        $("#CreateInventory_Quantity_To").val("");
        $("#CreateInventory_Quantity_To-error").hide();
        $("#CreateInventory_Component_Code").val("");
        $("#CreateInventory_Model_Code").val("");
        $("#CreateInventory_User_Distribution").val("");

        InventoryDocDatatable.draw()
    });
}

function ValidateSearchCreateInventory() {

    $(".input_search_doctypec_detail").on("input", function () {
        if ($(this).val().length > maxLengthSearchDetailTypeC) {
            $(this).val($(this).val().slice(0, maxLengthSearchDetailTypeC));
        }
    });

    $("#CreateInventory_Plant").on("input", function () {
        if ($(this).val().length > maxLengthCreateInventoryPlant) {
            $(this).val($(this).val().slice(0, maxLengthCreateInventoryPlant));
        }
    });

    $("#CreateInventory_Plant_WhLoc").on("input", function () {
        if ($(this).val().length > maxLengthCreateInventoryWhLoc) {
            $(this).val($(this).val().slice(0, maxLengthCreateInventoryWhLoc));
        }
    });

    $("#CreateInventory_Quantity_From").maskMoney({
        allowZero: true,
        defaultZero: false,
        allowEmpty: true,
        allowNegative: false,
        precision: 0,
        selectAllOnFocus: false,
        bringCaretAtEndOnFocus: false,
    });
    $("#CreateInventory_Quantity_From").on(
        "keypress",
        ValidateInputHelper.OnlyNumerOnKeyPress
    );
    $("#CreateInventory_Quantity_From").on(
        "keypress",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityFrom)
    );
    $("#CreateInventory_Quantity_From").on(
        "keyup",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityFrom)
    );

    $("#CreateInventory_Quantity_To").maskMoney({
        allowZero: true,
        defaultZero: false,
        allowEmpty: true,
        allowNegative: false,
        precision: 0,
        selectAllOnFocus: false,
        bringCaretAtEndOnFocus: false,
    });
    $("#CreateInventory_Quantity_To").on(
        "keypress",
        ValidateInputHelper.OnlyNumerOnKeyPress
    );
    $("#CreateInventory_Quantity_To").on(
        "keypress",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityTo)
    );
    $("#CreateInventory_Quantity_To").on(
        "keyup",
        ValidateInputHelper.LimitInputLengthOnKeyPress(maxLengthCreateInventoryQuantityTo)
    );

    $("#CreateInventory_Component_Code").on("input", function () {
        if ($(this).val().length > maxLengthCreateInventoryComponentCode) {
            $(this).val($(this).val().slice(0, maxLengthCreateInventoryComponentCode));
        }
    });

    $("#CreateInventory_Model_Code").on("input", function () {
        if ($(this).val().length > maxLengthCreateInventoryModelCode) {
            $(this).val($(this).val().slice(0, maxLengthCreateInventoryModelCode));
        }
    });

    $("#CreateInventory_User_Distribution").on("input", function () {
        if ($(this).val().length > maxLengthCreateInventoryUserDistribution) {
            $(this).val($(this).val().slice(0, maxLengthCreateInventoryUserDistribution));
        }
    });

    $("#CreateInventory_Quantity_From, #CreateInventory_Quantity_To").change(
        function (e) {
            $("#CreateInventory_Form").valid()
        }
    );

    $("#CreateInventory_Form").validate({
        rules: {
            CreateInventory_Quantity_From: {
                number: true,
                quantityRangeEmptyFromValidate: true,
                quantityRangeValidate: true,
            },
            CreateInventory_Quantity_To: {
                number: true,
                quantityRangeEmptyToValidate: true,
                quantityRangeValidate: true,
            },
        },
        messages: {},
    });


    jQuery.validator.addMethod(
        "quantityRangeEmptyFromValidate",
        function (value, element) {
            let valid = true;

            let quantityFrom = $("#CreateInventory_Quantity_From")
                .val()
                .replaceAll(",", "");
            let quantityTo = $("#CreateInventory_Quantity_To")
                .val()
                .replaceAll(",", "");

            if ((quantityFrom == "" && quantityTo != "")) {
                valid = false;
            }

            return valid;
        },
        "Vui lòng nhập số phiếu."
    );

    jQuery.validator.addMethod(
        "quantityRangeEmptyToValidate",
        function (value, element) {
            let valid = true;

            let quantityFrom = $("#CreateInventory_Quantity_From")
                .val()
                .replaceAll(",", "");
            let quantityTo = $("#CreateInventory_Quantity_To")
                .val()
                .replaceAll(",", "");

            if ((quantityTo == "" && quantityFrom != "")) {
                valid = false;
            }

            return valid;
        },
        "Vui lòng nhập số phiếu."
    );

    jQuery.validator.addMethod(
        "quantityRangeValidate",
        function (value, element) {
            let valid = true;

            let quantityFrom = $("#CreateInventory_Quantity_From")
                .val()
                .replaceAll(",", "");
            let quantityTo = $("#CreateInventory_Quantity_To")
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
        "Số phiếu vừa nhập không hợp lệ."
    );


    $("#Exportclustertree_Form").validate({
        rules: {
            ClusterTreeModel_Model_Code: {
                required: true
            },
        },
        messages: {
            ClusterTreeModel_Model_Code: {
                required: window.languageData[window.currentLanguage]["Vui lòng lựa chọn model code."]
            }
        },
    });

    $("#ExportQRCodeclustertree_Form").validate({
        rules: {
            QRCodeClusterTreeModel_Model_Code: {
                required: true
            },
            QRCodeClusterTreeModel_Machine_Type: {
                required: true
            },
        },
        messages: {
            QRCodeClusterTreeModel_Model_Code: {
                required: window.languageData[window.currentLanguage]["Vui lòng lựa chọn model code."]
            },
            QRCodeClusterTreeModel_Machine_Type: {
                required: window.languageData[window.currentLanguage]["Vui lòng lựa chọn dòng máy."]
            }
        },
    });

}

function BtnCancelClusterTree() {
    $("#btn_cancel_export_cluster_tree").click(function () {
        $("#ClusterTreeModel_Model_Code").val('');
        $("#ClusterTreeModel_Machine_Type").val('');
        $("#Exportclustertree_Form").validate().resetForm();
    })
};

function BtnCancelQRCodeClusterTree() {
    $("#btn_cancel_export_qrcode_cluster_tree").click(function () {
        $("#QRCodeClusterTreeModel_Model_Code").val('');
        $("#QRCodeClusterTreeModel_Machine_Type").val('');
        $("#QRCodeClusterTreeModel_LineName").val('');
        $("#ExportQRCodeclustertree_Form").validate().resetForm();
    })
};

function BtnExportClusterTree() {
    $("#btn_export_cluster_tree").click(function () {
        var link = $("#APIGateway").val();
        let valid = $("#Exportclustertree_Form").valid();
        if (valid) {
            let machineModel = $('#ClusterTreeModel_Model_Code').val()
            let machineType = $('#ClusterTreeModel_Machine_Type').val()
            let inventoryId = $("#inventory-wrapper").attr("data-id");

            //let myform = document.getElementById("Exportclustertree_Form");
            //let fd = new FormData(myform);

            var filterData = new FormData();
            filterData.append("machineModel", machineModel);
            filterData.append("machineType", machineType === null ? '' : machineType);
            //var data = {
            //    machineModel: machineModel,
            //    machineType: machineType
            //}

            $.ajax({
                type: "POST",
                url: link + `/api/inventory/web/${inventoryId}/groups/export`,
                data: filterData,
                processData: false,
                contentType: false,
                xhrFields: {
                    responseType: 'blob'
                },
                success: function (res) {
                    if (res) {
                        var blob = new Blob([res], { type: res.type });
                        const fileURL = URL.createObjectURL(blob);
                        const link = document.createElement('a');
                        link.href = fileURL;

                        var modelName = $("#ClusterTreeModel_Model_Code").val();
                        var machineName = $("#ClusterTreeModel_Machine_Type").val();

                        if (modelName !== null && machineName !== null) {
                            link.download = `Cumcay_${modelName}_${machineName}.xlsx`;
                        } else {
                            link.download = `Cumcay_${modelName}.xlsx`;
                        }

                        link.click();
                        $('#InventoryListExportClusterTreeModal').modal("hide");
                    } else {
                        toastr.error("Không tìm thấy file.");
                    }

                },
                error: function (error) {
                    toastr.error("Không tìm thấy dữ liệu.");
                }
            });
        }


    })
};

function ChangeDepartmentGetLocation_InventoryDoc() {
    $(document).delegate("#CreateInventory_Department", "change", (e) => {
        var listDepartments = $('#CreateInventory_Department').val();

        //Nếu bỏ tích hết phòng ban thì bỏ tích hết khu vực
        if (listDepartments.length == 0) {
            $("#CreateInventory_Area")[0].setOptions([]);
            $("#CreateInventory_Area")[0].reset();
            return;
        }

        //Call Api Xem chi tiết:
        var link = $("#APIGateway").val();
        var filterData = JSON.stringify({
            Departments: listDepartments
        });

        //Check phân quyền tài khoản: Nếu là tài khoản riêng hoặc Tài khoản chung(Xúc tiến) => Thêm 1 option với giá trị Location = ""
        //Mục đích để tìm kiếm phòng ban và khu vực trống:
        var showOptionEmpty_Location = (App.User.AccountType == AccountType.TaiKhoanRieng && (App.User.isGrant("EDIT_INVENTORY") || App.User.isGrant("VIEW_ALL_INVENTORY") || App.User.isGrant("VIEW_CURRENT_INVENTORY"))) || isPromoter();


        $.ajax({
            type: "POST",
            url: link + `/api/inventory/location/departmentname`,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: filterData,
            success: function (res) {
                if (res.code == 200) {

                    var options = [
                    ];

                    if (showOptionEmpty_Location) {
                        options.push({ label: "", value: "" })
                    }

                    let resultHtml = res?.data.map(item => {
                        options.push({ label: item.locationName, value: item.locationName })
                        return `
                        <option value="${item.locationName}">${item.locationName}</option>
                    `
                    }).join("");


                    $("#CreateInventory_Area")[0].virtualSelect.alwaysShowSelectedOptionsLabel = !(options.length > 1);
                    $("#CreateInventory_Area")[0].virtualSelect.disableAllOptionsSelectedText = !(options.length > 1);
                    $("#CreateInventory_Area")[0].virtualSelect.selectAllOnlyVisible = !(options.length > 1);
                    $("#CreateInventory_Area")[0].virtualSelect.autoSelectFirstOption = true;
                    //alwaysShowSelectedOptionsLabel: false,
                    //disableAllOptionsSelectedText: false,
                    //selectAllOnlyVisible: false,
                    //$("#select_inventory_location")[0].virtualSelect.disableAllOptionsSelectedText = options.length > 1;

                    document.querySelector('#CreateInventory_Area').setOptions(options);

                    $("#CreateInventory_Area")[0].reset();
                    $("#CreateInventory_Area")[0].toggleSelectAll(true);

                }

            },
            error: function (error) {
                toastr.error(error.message)
            }
        });


    })
}

function disableImportButtons() {
    $('#btnCreateform_A, #btnCreateform_BE, #btnCreateform_E, #btnCreateform_C, #btnCreateform_Ship')
        .attr('disabled', true)
        .css('background-color', '#0d2ea08f');
}

function enableImportButtons() {
    $('#btnCreateform_A, #btnCreateform_BE, #btnCreateform_E, #btnCreateform_C, #btnCreateform_Ship')
        .attr('disabled', false)
        .css('background-color', '#0D2EA0');
}


function InitListInventoryDocument_Datatable() {
    let host = App.ApiGateWayUrl;
    var inventoryId = $("#inventory-wrapper").data("id")

    InventoryDocDatatable = $('#inventory_document_table').DataTable({
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
            "url": host + `/api/inventory/web/${inventoryId}/document`,
            "type": "POST",
            "contentType": "application/x-www-form-urlencoded",
            dataType: "json",
            data: function (data) {
                let dataFilter = {
                    Plant: "",
                    WHLoc: "",
                    DocNumberFrom: "",
                    DocNumberTo: "",
                    ComponentCode: "",
                    ModelCode: "",
                    AssigneeAccount: "",
                    Departments: "",
                    Locations: "",
                    DocTypes: "",
                    IsCheckAllDepartment: "",
                    IsCheckAllLocation: "",
                    IsCheckAllDocType: "",
                };
                //Check click Tat Ca:
                //var isCheckAllDepartment = document.querySelector('#CreateInventory_Department').isAllSelected();
                //var isCheckAllLocation = document.querySelector('#CreateInventory_Area').isAllSelected();
                //var isCheckAllDocType = document.querySelector('#CreateInventory_Form_Type').isAllSelected();

                //if (isCheckAllDepartment) {
                //    dataFilter.IsCheckAllDepartment = "-1";
                //} else {
                //    dataFilter.IsCheckAllDepartment = "";
                //}
                //if (isCheckAllLocation) {
                //    dataFilter.IsCheckAllLocation = "-1";
                //} else {
                //    dataFilter.IsCheckAllLocation = "";
                //}
                //if (isCheckAllDocType) {
                //    dataFilter.IsCheckAllDocType = "-1";
                //} else {
                //    dataFilter.IsCheckAllDocType = "";
                //}

                dataFilter.Plant = $("#CreateInventory_Plant").val();
                dataFilter.WHLoc = $("#CreateInventory_Plant_WhLoc").val();
                dataFilter.DocNumberFrom = $("#CreateInventory_Quantity_From").val().replaceAll(',', '');
                dataFilter.DocNumberTo = $("#CreateInventory_Quantity_To").val().replaceAll(',', '');
                dataFilter.ComponentCode = $("#CreateInventory_Component_Code").val();
                dataFilter.ModelCode = $("#CreateInventory_Model_Code").val();
                dataFilter.AssigneeAccount = $("#CreateInventory_User_Distribution").val();

                dataFilter.Departments = $("#CreateInventory_Department").val();
                dataFilter.Locations = $("#CreateInventory_Area").val();
                dataFilter.DocTypes = $("#CreateInventory_Form_Type").val();

                Object.assign(data, dataFilter);

                return data;
            },
            "dataSrc": function ({ data }) {
                return data;
            }
        },
        "drawCallback": function (settings) {
            let totalPages = InventoryDocDatatable.page.info().pages;
            let totalRecords = InventoryDocDatatable.page.info().recordsTotal;

            //Nếu tồn tại phiếu C thì sẽ hiện nút xuất cụm cây:
            let isExistDocTypeC = settings.json.isExistDocTypeC;
            if (isExistDocTypeC) {
                $("#export-tree").removeClass("btn_disabled").attr("disabled", false);
            }
            else {
                $("#export-tree").removeClass("btn_disabled").addClass("btn_disabled").attr("disabled", true);
            }

            let currPage = InventoryDocDatatable.page() + 1;
            if (currPage == 1) {
                $("#tab-inventory-create").find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $("#tab-inventory-create").find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }
            if (currPage == totalPages) {
                $("#tab-inventory-create").find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $("#tab-inventory-create").find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }

            $("#tab-inventory-create").find(".pagination").before(`<div class="h5 datatable_total_records"> <b>${window.languageData[window.currentLanguage]["Tổng"]}</b>: ${totalPages} <span class="total_record_seperate"> | </span>
                </div>`)

            //Ẩn xuất file nếu không có dữ liệu
            if (totalRecords == 0) {
                $("#export_file_inventory_document").removeClass("btn_disabled").addClass("btn_disabled").attr("disabled", true);
            } else {
                $("#export_file_inventory_document").removeClass("btn_disabled").attr("disabled", false);
            }

            if (totalRecords <= 10) {
                $(".container-list-view .bottom").hide()
            }

            //Cập nhật tổng số linh kiện có trong hệ thống
            inventoryViewModel.totalRowsCount(totalRecords);

            $("#InventoryDocument_CheckAll").prop("checked", false);
            if (inventoryViewModel.selectAllPages()) {
                $(`.InventoryDocument_check`).prop("checked", true);

                if (inventoryViewModel.selectAll_UncheckedIds().length) {
                    inventoryViewModel.selectAll_UncheckedIds().forEach(item => {
                        $(`.InventoryDocument_check[data-id="${item}"]`).prop("checked", false).change();
                    })
                }
            } else {
                if (inventoryViewModel.checkedIds().length) {
                    inventoryViewModel.checkedIds().forEach(item => {
                        $(`.InventoryDocument_check[data-id="${item}"]`).prop("checked", true).change();
                    })
                }
            }

            let isAllOnePage = ($(`.InventoryDocument_check`).length == $(`.InventoryDocument_check:checked`).length) && $(`.InventoryDocument_check:checked`).length > 0 ;
            if (isAllOnePage) {
                $("#InventoryDocument_CheckAll").prop("checked", true).change();
            }
            //bỏ disabled buttons import document:
            enableImportButtons();

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
                    return `<input type="checkbox" class="InventoryDocument_check" data-id="${row.id}" />`
                },
                "autoWidth": true
            },
            {
                "data": "",
                "name": "STT",
                "render": function (data, type, row, index) {
                    let pagesize = index.settings._iDisplayLength;
                    let currentRow = ++index.row;
                    let currentPage = InventoryDocDatatable.page() + 1;

                    let STT = ((currentPage - 1) * pagesize) + currentRow;

                    if (STT < 10) {
                        STT = `0${STT}`;
                    }
                    return STT;
                },
                "autoWidth": true
            },
            { "data": "docCode", "name": "Mã phiếu", "autoWidth": true },
            { "data": "plant", "name": "Plant", "autoWidth": true },
            { "data": "whLoc", "name": "WH Loc.", "autoWidth": true },
            {
                "data": "componentCode", "name": "Mã linh kiện",
                "autoWidth": true
            },
            { "data": "modelCode", "name": "ModelCode", "autoWidth": true },
            /*{ "data": "stageName", "name": "Tên công đoạn", "autoWidth": true },*/
            {
                "data": "componentName", "name": "Tên linh kiện", "autoWidth": true
            },
            {
                "data": "quantity",
                "name": "Quantity",
                "autoWidth": true
            },
            {
                "data": "position", "name": "Vị trí",
                "autoWidth": true
            },
            {
                "data": "saleOrderNo", "name": "S/O No.",
                "autoWidth": true
            },
            {
                "data": "department", "name": "Phòng ban",
                "autoWidth": true
            },
            {
                "data": "location", "name": "Khu vực",
                "autoWidth": true
            },
            {
                "data": "assigneeAccount", "name": "Tài khoản phân phát",
                "autoWidth": true
            },
            {
                "data": "stockType", "name": "Stock type",
                "autoWidth": true
            },
            {
                "data": "specialStock", "name": "Special stock",
                "autoWidth": true
            },
            {
                "data": "saleOrderList", "name": "S/O List",
                "autoWidth": true
            },
            {
                "data": "assemblyLoc", "name": "Assembly Loc.",
                "autoWidth": true
            },
            {
                "data": "vendorCode", "name": "Vendor code",
                "autoWidth": true
            },
            {
                "data": "physInv", "name": "Phys.Inv",
                "autoWidth": true
            },
            {
                "data": "proOrderNo", "name": "Pro. Order No",
                "autoWidth": true
            },
            {
                "data": "fiscalYear", "name": "Fiscal year",
                "autoWidth": true
            },
            {
                "data": "item", "name": "ITEM",
                "autoWidth": true
            },
            {
                "data": "plantedCount", "name": "Planned count",
                "autoWidth": true
            },
            {
                "data": "columnC", "name": "Cột C",
                "autoWidth": true
            },
            {
                "data": "columnN", "name": "Cột N",
                "autoWidth": true
            },
            {
                "data": "columnO", "name": "Cột O",
                "autoWidth": true
            },
            {
                "data": "columnP", "name": "Cột P",
                "autoWidth": true
            },
            {
                "data": "columnQ", "name": "Cột Q",
                "autoWidth": true
            },
            {
                "data": "columnR", "name": "Cột R",
                "autoWidth": true
            },
            {
                "data": "columnS", "name": "Cột S",
                "autoWidth": true
            },
            {
                "data": "note", "name": "Ghi chú",
                "autoWidth": true
            },
            {
                "data": "createdBy", "name": "Người tạo",
                "autoWidth": true
            },
            {
                "data": "createdAt", "name": "Thời gian tạo",
                "autoWidth": true
            },
            {
                "data": "",
                "name": "",
                "render": function (data, type, row) {
                    const selectHtmlSpecial = `
                        <div class="Controls_Container">
                            <div class="ViewDetail_InventoryDoc_Controls mx-3">
                                <a class="detail-inventory-doc view_detail_Indoc in_doc_detail" data-id="${row.id}" data-inventoryId="${row.inventoryId}" data-docType="${row.docType}">${window.languageData[window.currentLanguage]["Xem chi tiết"]}</a>
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

function ExportFileInventoryDocument() {

    $(document).delegate("#export_file_inventory_document", "click", (e) => {
        var inventoryId = $("#inventory-wrapper").data("id")

        var plant = $("#CreateInventory_Plant").val();
        var wHLoc = $("#CreateInventory_Plant_WhLoc").val();
        var docNumberFrom = $("#CreateInventory_Quantity_From").val().replaceAll(',', '');
        var docNumberTo = $("#CreateInventory_Quantity_To").val().replaceAll(',', '');
        var componentCode = $("#CreateInventory_Component_Code").val();
        var modelCode = $("#CreateInventory_Model_Code").val();
        var assigneeAccount = $("#CreateInventory_User_Distribution").val();

        var departments = $("#CreateInventory_Department").val();
        var locations = $("#CreateInventory_Area").val();
        var docTypes = $("#CreateInventory_Form_Type").val();

        let filterData = {
            Plant: plant,
            WHLoc: wHLoc,
            DocNumberFrom: docNumberFrom,
            DocNumberTo: docNumberTo,
            ComponentCode: componentCode,
            ModelCode: modelCode,
            AssigneeAccount: assigneeAccount,
            Departments: departments,
            Locations: locations,
            DocTypes: docTypes,
        };

        loading(true)

        var url = `/inventory/${inventoryId}/document/export`;
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

                    link.download = `Phieukiemke_${formattedTime}.xlsx`;
                    link.click();
                } else {
                    toastr.error(window.languageData[window.currentLanguage]["Không tìm thấy file."]);
                }
                toastr.success(window.languageData[window.currentLanguage]["Export phiếu kiểm kê thành công."]);
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

function ExportQRCode() {

    $(document).delegate("#btn_export_qrcode_cluster_tree", "click", (e) => {
        
        let valid = $("#ExportQRCodeclustertree_Form").valid();

        if (valid) {
            var inventoryId = $("#inventory-wrapper").data("id");

            var machineModel = $('#QRCodeClusterTreeModel_Model_Code').val();
            var machineType = $('#QRCodeClusterTreeModel_Machine_Type').val();
            var lineName = $('#QRCodeClusterTreeModel_LineName').val();

            loading(true)

            let filterData = {
                MachineModel: machineModel,
                MachineType: machineType,
                LineName: lineName,
                InventoryId: inventoryId,
            };
            var url = `/inventory/generate/qrcode`;
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

                        link.download = `QRCode_${formattedTime}.xlsx`;
                        link.click();

                        $('#InventoryListExportQRCodeClusterTreeModal').modal("hide");
                        toastr.success(window.languageData[window.currentLanguage]["Export QRCode thành công."]);

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


; var CreateInventoryHandler = (function () {
    var root = {
        parentEl: $("#tab-inventory-create")
    }

    const APIs = {
        ImportDocumentA: function (inventoryId, docType = "A", file) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/${inventoryId}/${docType}/import`;
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
        ImportDocumentBE: function (inventoryId, docType = "B", file) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/${inventoryId}/${docType}/import`;
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
        ImportDocumentE: function (inventoryId, docType = "E", file) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/${inventoryId}/${docType}/import`;
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
        }
        ,ValidateDocumentC: function (inventoryId, isBypassWarning = false, file) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/${inventoryId}/doc-c/validate?isBypassWarning=${isBypassWarning}`;
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
        ImportDocumentC: function (inventoryId, isBypassWarning = false, byteData) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/${inventoryId}/import-doc-c?isBypassWarning=${isBypassWarning}`;
                let formData = new FormData();

                formData.append("file", byteData);

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
        ImportDocumentShip: function (inventoryId, byteData) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/${inventoryId}/import-doc-ship`;
                let formData = new FormData();

                formData.append("file", byteData);

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
        }
    }

    function Cache() {
        root.btnCreateform_A = $(root.parentEl).find("#btnCreateform_A");
        root.btnCreateform_BE = $(root.parentEl).find("#btnCreateform_BE");
        root.btnCreateform_E = $(root.parentEl).find("#btnCreateform_E");
        root.btnCreateform_C = $(root.parentEl).find("#btnCreateform_C");
        root.btnCreateform_Ship = $(root.parentEl).find("#btnCreateform_Ship");
    }

    function PreLoad() {

    }

    function Events() {
        root.btnCreateform_A.click(function (e) {
            root.parentEl.find("#inputFileFormA").trigger("click");
        })

        root.btnCreateform_BE.click(function (e) {
            let inventoryId = root.parentEl.closest("#inventory-wrapper").attr("data-id");
            APIs.CheckExistDocumentA(inventoryId).then(res => {
                if (res.code == 200) {
                    root.parentEl.find("#inputFileFormBE").trigger("click");
                }
            }).catch(err => {
                if (err.responseJSON.code == 404) {
                    Swal.fire({
                        title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                        text: window.languageData[window.currentLanguage][err?.responseJSON?.message] || window.languageData[window.currentLanguage]["Có lỗi khi thực hiện import."],
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    })
                }
            }).finally(() => {
            })

        })

        root.btnCreateform_E.click(function (e) {
            let inventoryId = root.parentEl.closest("#inventory-wrapper").attr("data-id");
            APIs.CheckExistDocumentA(inventoryId).then(res => {
                if (res.code == 200) {
                    root.parentEl.find("#inputFileFormE").trigger("click");
                }
            }).catch(err => {
                if (err.responseJSON.code == 404) {
                    Swal.fire({
                        title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                        text: window.languageData[window.currentLanguage][err?.responseJSON?.message] || window.languageData[window.currentLanguage]["Có lỗi khi thực hiện import."],
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    })
                }
            }).finally(() => {
            })

        })


        root.btnCreateform_C.click(function (e) {
            let inventoryId = root.parentEl.closest("#inventory-wrapper").attr("data-id");
            APIs.CheckExistDocumentA(inventoryId).then(res => {
                if (res.code == 200) {
                    root.parentEl.find("#inputFileFormC").trigger("click");
                }
            }).catch(err => {
                if (err.responseJSON.code == 404) {
                    Swal.fire({
                        title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                        text: window.languageData[window.currentLanguage][err?.responseJSON?.message] || window.languageData[window.currentLanguage]["Có lỗi khi thực hiện import."],
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    })
                }
            }).finally(() => {
            })
        })

        root.btnCreateform_Ship.click(function (e) {
            root.parentEl.find("#inputFileFormShip").trigger("click");
        })

        root.parentEl.find("#inputFileFormA").change(function (e) {
            let target = e.target;
            let inventoryId = root.parentEl.closest("#inventory-wrapper").attr("data-id");


            let file = e.target.files[0];
            if (file.size > 0) {
                loading(true);
                disableImportButtons();
                APIs.ImportDocumentA(inventoryId, "A", file).then(res => {
                    if (res.failCount > 0) {
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                            text: `${window.languageData[window.currentLanguage]["Tạo mới thành công"]} ${res?.successCount || 0} ${window.languageData[window.currentLanguage]["phiếu kiểm kê và có"]} 
                            ${res?.failCount || 0} ${window.languageData[window.currentLanguage]["dòng dữ liệu lỗi không thể thực hiện import."]}
                                ${window.languageData[window.currentLanguage]["Vui lòng ấn Đồng ý để xem dữ liệu lỗi."]}`,
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
                            disableImportButtons();
                            InventoryDocDatatable.draw();

                            if (result.isConfirmed) {
                                let bytes = res?.bytes || "";
                                if (bytes.length > 0) {
                                    let convertedByte = FileTemplateHandler.utils.base64ToArrayBuffer(bytes);
                                    FileTemplateHandler.utils.saveByteArr(convertedByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", res.fileName);

                                }
                            }
                        });
                    } else if (res.failCount == 0) {
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                            text: `${window.languageData[window.currentLanguage]["Tạo mới thành công"]} ${res?.successCount || 0} ${window.languageData[window.currentLanguage]["phiếu kiểm kê."]}`,
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        });
                    }
                }).catch(err => {
                    let title = window.languageData[window.currentLanguage]["Thông báo"];
                    if (err?.responseJSON.code == 70) {
                        title = window.languageData[window.currentLanguage]["File sai định dạng"];
                    }

                    Swal.fire({
                        title: `<b>${title}</b>`,
                        text: window.languageData[window.currentLanguage][err?.responseJSON?.message] || window.languageData[window.currentLanguage]["Có lỗi khi thực hiện import."],
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    })
                }).finally(() => {
                    //Render lại danh sách sau khi import
                    InventoryDocDatatable.draw();
                    loading(false);
                })
            }

            $(target).val("");
        })

        root.parentEl.find("#inputFileFormBE").change(function (e) {
            let target = e.target;
            let inventoryId = root.parentEl.closest("#inventory-wrapper").attr("data-id");

            let file = e.target.files[0];
            if (file.size > 0) {
                loading(true);
                APIs.ImportDocumentBE(inventoryId, "B", file).then(res => {
                    if (res.failCount > 0) {
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]['Thông báo']}</b>`,
                            text: `${window.languageData[window.currentLanguage]["Tạo mới thành công"]} ${res?.successCount || 0} ${window.languageData[window.currentLanguage]["phiếu kiểm kê và có"]} 
                            ${res?.failCount || 0} ${window.languageData[window.currentLanguage]["dòng dữ liệu lỗi không thể thực hiện import."]}
                                ${window.languageData[window.currentLanguage]["Vui lòng ấn Đồng ý để xem dữ liệu lỗi."]}`,
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
                                let bytes = res?.bytes || "";
                                if (bytes.length > 0) {
                                    let convertedByte = FileTemplateHandler.utils.base64ToArrayBuffer(bytes);
                                    FileTemplateHandler.utils.saveByteArr(convertedByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", res.fileName);
                                }
                            }
                        });
                    } else if (res.failCount == 0) {
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                            text: `${window.languageData[window.currentLanguage]["Tạo mới thành công"]} ${res?.successCount || 0} ${window.languageData[window.currentLanguage]["phiếu kiểm kê."]}`,
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        });
                    }
                }).catch(err => {
                    let title = window.languageData[window.currentLanguage]["Thông báo"];
                    if (err?.responseJSON.code == ServerResponseStatusCode.InvalidFileExcel) {
                        title = window.languageData[window.currentLanguage]["File sai định dạng"];
                    } else if (err?.responseJSON.code == ServerResponseStatusCode.NotExistDocTypeA) {
                        title = window.languageData[window.currentLanguage]["Không thể import"];
                    }

                    Swal.fire({
                        title: `<b>${title}</b>`,
                        text: window.languageData[window.currentLanguage][err?.responseJSON?.message] || window.languageData[window.currentLanguage]["Có lỗi khi thực hiện import."],
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    })
                }).finally(() => {

                    InventoryDocDatatable.draw();
                    loading(false);
                })
            }

            $(target).val("");
        })

        root.parentEl.find("#inputFileFormE").change(function (e) {
            let target = e.target;
            let inventoryId = root.parentEl.closest("#inventory-wrapper").attr("data-id");

            let file = e.target.files[0];
            if (file.size > 0) {
                loading(true);
                APIs.ImportDocumentE(inventoryId, "E", file).then(res => {
                    if (res.failCount > 0) {
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                            text: `${window.languageData[window.currentLanguage]["Tạo mới thành công"]} ${res?.successCount || 0} ${window.languageData[window.currentLanguage]["phiếu kiểm kê và có"]} 
                            ${res?.failCount || 0} ${window.languageData[window.currentLanguage]["dòng dữ liệu lỗi không thể thực hiện import."]}
                                ${window.languageData[window.currentLanguage]["Vui lòng ấn Đồng ý để xem dữ liệu lỗi."]}`,
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
                                let bytes = res?.bytes || "";
                                if (bytes.length > 0) {
                                    let convertedByte = FileTemplateHandler.utils.base64ToArrayBuffer(bytes);
                                    FileTemplateHandler.utils.saveByteArr(convertedByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", res.fileName);
                                }
                            }
                        });
                    } else if (res.failCount == 0) {
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                            text: `${window.languageData[window.currentLanguage]["Tạo mới thành công"]} ${res?.successCount || 0} ${window.languageData[window.currentLanguage]["phiếu kiểm kê."]}`,
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        });
                    }
                }).catch(err => {
                    let title = window.languageData[window.currentLanguage]["Thông báo"];
                    if (err?.responseJSON.code == ServerResponseStatusCode.InvalidFileExcel) {
                        title = window.languageData[window.currentLanguage]["File sai định dạng"];
                    } else if (err?.responseJSON.code == ServerResponseStatusCode.NotExistDocTypeA) {
                        title = window.languageData[window.currentLanguage]["Không thể import"];
                    }

                    Swal.fire({
                        title: `<b>${title}</b>`,
                        text: window.languageData[window.currentLanguage][err?.responseJSON?.message] || window.languageData[window.currentLanguage]["Có lỗi khi thực hiện import."],
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    })
                }).finally(() => {

                    InventoryDocDatatable.draw();
                    loading(false);
                })
            }

            $(target).val("");
        })

        root.parentEl.find("#inputFileFormC").change(function (e) {
            let target = e.target;
            let inventoryId = root.parentEl.closest("#inventory-wrapper").attr("data-id");
            let isByPassWarning = false;

            let file = e.target.files[0];
            if (file.size > 0) {
                loading(true);
                APIs.ValidateDocumentC(inventoryId, isByPassWarning, file).then(res => {
                    if (res.isWarning == false) {
                        if (res.isValid) {
                            // Tạo một liên kết ẩn với dữ liệu tải xuống
                            var convertedfileBytes = FileTemplateHandler.utils.base64ToArrayBuffer(res.data);
                            var byteData = new Blob([convertedfileBytes], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });

                            APIs.ImportDocumentC(inventoryId, isByPassWarning, byteData).then(res => {
                                if (res.code == 200) {
                                    InventoryDocDatatable.draw();
                                    toastr.success(res.message);

                                }

                            }).catch(err => {
                                Swal.fire({
                                    title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                                    text: window.languageData[window.currentLanguage][err?.responseJSON?.message] || window.languageData[window.currentLanguage]["Có lỗi khi thực hiện import."],
                                    confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                                    width: '30%'
                                })
                            }).finally(() => {
                                loading(false);
                            })
                        }
                        else {
                            if (res?.title === "File sai định dạng" || res?.title === "File không tồn tại" || res?.title === "Không thể tạo phiếu") {
                                Swal.fire({
                                    title: `<b>${window.languageData[window.currentLanguage][res?.title]}</b>`,
                                    text: `${window.languageData[window.currentLanguage][res?.content]}`,
                                    confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                                    width: '30%'
                                })
                            } else {
                                Swal.fire({
                                    title: `<b>${window.languageData[window.currentLanguage][res?.title]}</b>`,
                                    text: `${window.languageData[window.currentLanguage][res?.content]}`,
                                    confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                                    width: '30%'
                                })
                                //Lấy ngày tháng hiện tại theo định dạng: yyyymmdd_hhmmss:
                                var currentTime = new Date();
                                var formattedTime = moment(currentTime).format("YYYYMMDD_HHmmss");

                                let bytes = res.data;
                                var convertedfileBytes = FileTemplateHandler.utils.base64ToArrayBuffer(bytes);
                                FileTemplateHandler.utils.saveByteArr(convertedfileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", `PhieuC_FileLoi_${formattedTime}.xlsx`);
                            }

                        }

                    }
                    else {
                        if (res.isEditDocTypeCRole) {
                            Swal.fire({
                                title: `<b>${window.languageData[window.currentLanguage][res?.title]}</b>`,
                                text: `${window.languageData[window.currentLanguage][res?.content]}`,
                                confirmButtonText: window.languageData[window.currentLanguage]['Tiếp tục'],
                                showCancelButton: true,
                                showLoaderOnConfirm: true,
                                cancelButtonText: window.languageData[window.currentLanguage]['Kiểm tra'],
                                reverseButtons: true,
                                allowOutsideClick: false,
                                customClass: {
                                    actions: "swal_confirm_actions"
                                }
                            }).then((result, e) => {
                                if (result.isConfirmed) {
                                    loading(true);
                                    let isByPassWarning = true;
                                    APIs.ValidateDocumentC(inventoryId, isByPassWarning, file)
                                        .then(res => {
                                            if (res.isValid) {
                                                // Tạo một liên kết ẩn với dữ liệu tải xuống
                                                var convertedfileBytes = FileTemplateHandler.utils.base64ToArrayBuffer(res.data);
                                                var byteData = new Blob([convertedfileBytes], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });


                                                return APIs.ImportDocumentC(inventoryId, isByPassWarning, byteData);

                                                //APIs.ImportDocumentC(inventoryId, isByPassWarning, byteData).then(res => {
                                                //    if (res.code == 200) {
                                                //        InventoryDocDatatable.draw();
                                                //        toastr.success(res.message);

                                                //    }

                                                //}).catch(err => {
                                                //    Swal.fire({
                                                //        title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                                                //        text: window.languageData[window.currentLanguage][err?.responseJSON?.message] || window.languageData[window.currentLanguage]["Có lỗi khi thực hiện import."],
                                                //        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                                                //        width: '30%'
                                                //    })
                                                //}).finally(() => {
                                                //    loading(false);
                                                //})
                                            }
                                            else {

                                                Swal.fire({
                                                    title: `<b>${window.languageData[window.currentLanguage][res?.title]}</b>`,
                                                    text: `${window.languageData[window.currentLanguage][res?.content]}`,
                                                    confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                                                    width: '30%'
                                                })
                                                if (res?.title != "File sai định dạng" && res?.title != "Không thể tạo phiếu") {
                                                    //Lấy ngày tháng hiện tại theo định dạng: yyyymmdd_hhmmss:
                                                    var currentTime = new Date();
                                                    var formattedTime = moment(currentTime).format("YYYYMMDD_HHmmss");

                                                    let bytes = res.data;
                                                    var convertedfileBytes = FileTemplateHandler.utils.base64ToArrayBuffer(bytes);
                                                    FileTemplateHandler.utils.saveByteArr(convertedfileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", `PhieuC_FileLoi_${formattedTime}.xlsx`);
                                                }
                                            }
                                        })
                                        .then(res => {
                                            if (res.code === 200) {
                                                InventoryDocDatatable.draw();
                                                toastr.success(res.message);
                                            }
                                        })
                                        .catch(err => {
                                            Swal.fire({
                                                title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                                                text: window.languageData[window.currentLanguage][err?.responseJSON?.message] || window.languageData[window.currentLanguage]["Có lỗi khi thực hiện import."],
                                                confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                                                width: '30%'
                                            })
                                        }).finally(() => {
                                            loading(false);
                                        })

                                }
                                else if (result.dismiss === Swal.DismissReason.cancel) {

                                    //Swal.fire({
                                    //    title: `<b>${res?.title}</b>`,
                                    //    text: `${res?.content}`,
                                    //    confirmButtonText: "Đã hiểu",
                                    //    width: '30%'
                                    //})

                                    //Lấy ngày tháng hiện tại theo định dạng: yyyymmdd_hhmmss:
                                    var currentTime = new Date();
                                    var formattedTime = moment(currentTime).format("YYYYMMDD_HHmmss");

                                    let bytes = res.data;
                                    var convertedfileBytes = FileTemplateHandler.utils.base64ToArrayBuffer(bytes);
                                    FileTemplateHandler.utils.saveByteArr(convertedfileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", `PhieuC_FileCanhBao_${formattedTime}.xlsx`);
                                }
                            });
                        }
                        else {
                            Swal.fire({
                                title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                                text: window.languageData[window.currentLanguage]["Bạn không có quyền nhập phiếu."],
                                confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                                width: '30%'
                            })
                        }

                    }

                }).catch(err => {
                    Swal.fire({
                        title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                        text: window.languageData[window.currentLanguage][err?.responseJSON?.message] || window.languageData[window.currentLanguage]["Có lỗi khi thực hiện import."],
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    })
                }).finally(() => {
                    InventoryDocDatatable.draw();
                    loading(false);
                })
            }

            $(target).val("");
        })

        root.parentEl.find("#inputFileFormShip").change(function (e) {
            let target = e.target;
            let inventoryId = root.parentEl.closest("#inventory-wrapper").attr("data-id");


            let file = e.target.files[0];
            if (file.size > 0) {
                loading(true);
                APIs.ImportDocumentShip(inventoryId, file).then(res => {
                    if (res.failCount > 0) {
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                            text: `${window.languageData[window.currentLanguage]["Tạo mới thành công"]} ${res?.successCount || 0} ${window.languageData[window.currentLanguage]["phiếu kiểm kê và có"]} 
                            ${res?.failCount || 0} ${window.languageData[window.currentLanguage]["dòng dữ liệu lỗi không thể thực hiện import."]}
                                ${window.languageData[window.currentLanguage]["Vui lòng ấn Đồng ý để xem dữ liệu lỗi."]}`,
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
                            InventoryDocDatatable.draw();

                            if (result.isConfirmed) {
                                let bytes = res?.bytes || "";
                                if (bytes.length > 0) {
                                    let convertedByte = FileTemplateHandler.utils.base64ToArrayBuffer(bytes);
                                    FileTemplateHandler.utils.saveByteArr(convertedByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", res.fileName);

                                }
                            }
                        });
                    } else if (res.failCount == 0) {
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                            text: `${window.languageData[window.currentLanguage]["Tạo mới thành công"]} ${res?.successCount || 0} ${window.languageData[window.currentLanguage]["phiếu kiểm kê."]}`,
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        });
                    }
                }).catch(err => {
                    let title = window.languageData[window.currentLanguage]["Thông báo"];
                    if (err?.responseJSON.code == 70) {
                        title = window.languageData[window.currentLanguage]["File sai định dạng"];
                    }

                    Swal.fire({
                        title: `<b>${title}</b>`,
                        text: window.languageData[window.currentLanguage][err?.responseJSON?.message] || window.languageData[window.currentLanguage]["Có lỗi khi thực hiện import."],
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    })
                }).finally(() => {
                    //Render lại danh sách sau khi import
                    InventoryDocDatatable.draw();
                    loading(false);
                })
            }

            $(target).val("");
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


function InventoryView() {
    
    let self = this;
    self.checkedIds = ko.observableArray([]);
    self.isCheckedAll = ko.observable(false);
    self.selectAllPages = ko.observable(false);
    self.selectAll_UncheckedIds = ko.observableArray([]);
    self.totalRowsCount = ko.observable(0);

    let deleteBtn = $("#tab-inventory-create").find(".ListInventoryDocument_Delete");
    let apis = {
        DeleteInventoryDocs: function (inventoryId, filterData, deleteAll) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/web/${inventoryId}/inventory-docs?deleteAll=${deleteAll}`;

                $.ajax({
                    type: 'DELETE',
                    url: url,
                    contentType: 'application/json',
                    data: JSON.stringify(filterData),
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
            $("#tab-inventory-create").find("#InventoryDocument_CheckAll").prop("checked", true).change();
        } else {
            $("#tab-inventory-create").find("#InventoryDocument_CheckAll").prop("checked", false).change();
            self.selectAll_UncheckedIds([]);
            self.checkedIds([]);
        }
    })


    //Các nút checkbox con
    $("#tab-inventory-create").delegate(".InventoryDocument_check", "change", function (e) {
        // Ẩn hiện nút chọn tất cả dựa theo checkbox
        //let isCheckedAll = $("#tab-inventory-create").find(".InventoryDocument_check").length == $("#tab-inventory-create").find(".InventoryDocument_check:checked").length;
        //$("#tab-inventory-create").find("#InventoryDocument_CheckAll").prop("checked", isCheckedAll);

        // Lưu Id vào mảng
        let inventoryId = $(this).attr("data-id");
        let isChecked = $(this).is(":checked");
        if (isChecked) {
            if (!self.checkedIds().includes(inventoryId))
                self.checkedIds.push(inventoryId);
        } else {
            removeElement(self.checkedIds, inventoryId);
        }

        if (self.selectAllPages() && !isChecked) {
            if (!self.selectAll_UncheckedIds().includes(inventoryId))
                self.selectAll_UncheckedIds.push(inventoryId);
        } else if (self.selectAllPages() && isChecked) {
            removeElement(self.selectAll_UncheckedIds, inventoryId);
        }
    })

    //Nút checkbox chọn tất cả
    $("#tab-inventory-create").delegate("#InventoryDocument_CheckAll", "change", function (e) {
        let checked = $(this).is(":checked");
        $("#tab-inventory-create").find(".InventoryDocument_check").prop("checked", checked).change();

        if (!checked) {
            if (self.selectAllPages() && !self.enableDeleteButton()) {
                $("#tab-inventory-create").find("#select_AllPages_Inventory").prop("checked", false).change();
            }
        }
    })

    $("#tab-inventory-create").delegate("#select_AllPages_Inventory, #InventoryDocument_CheckAll", "change", function (e) {
        let isChecked = $(this).is(":checked");
        self.selectAllPages(isChecked);
    })


    //Delete event
    $("#tab-inventory-create").delegate("#delete", "click", function (e) {
        let itemCount = self.selectAllPages() ? (self.totalRowsCount() - self.selectAll_UncheckedIds().length) : self.checkedIds().length;

        Swal.fire({
            title: `<b>${window.languageData[window.currentLanguage]["Xác nhận xóa"]}</b>`,
            text: `${window.languageData[window.currentLanguage]["Bạn có chắc chắn muốn xóa"]} ${itemCount} ${window.languageData[window.currentLanguage]["phiếu kiểm kê này ?"]}`,
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

                let isDeleteDocs = $("#tab-inventory-create").find("#select_AllPages_Inventory").prop("checked");
                let Ids = self.selectAllPages() ? self.selectAll_UncheckedIds() : self.checkedIds();

                let plant = $("#CreateInventory_Plant").val();
                let wHLoc = $("#CreateInventory_Plant_WhLoc").val();
                let docNumberFrom = $("#CreateInventory_Quantity_From").val().replaceAll(',', '');
                let docNumberTo = $("#CreateInventory_Quantity_To").val().replaceAll(',', '');
                let componentCode = $("#CreateInventory_Component_Code").val();
                let modelCode = $("#CreateInventory_Model_Code").val();
                let assigneeAccount = $("#CreateInventory_User_Distribution").val();

                let departments = $("#CreateInventory_Department").val();
                let locations = $("#CreateInventory_Area").val();
                let docTypes = $("#CreateInventory_Form_Type").val();
                let isDeletedAllHasFilters = $("#tab-inventory-create").find("#InventoryDocument_CheckAll").prop("checked");
                


                let filterData = {
                    Plant: plant,
                    WHLoc: wHLoc,
                    DocNumberFrom: docNumberFrom,
                    DocNumberTo: docNumberTo,
                    ComponentCode: componentCode,
                    ModelCode: modelCode,
                    AssigneeAccount: assigneeAccount,
                    Departments: departments,
                    Locations: locations,
                    DocTypes: docTypes,
                    IsDeletedAllHasFilters: isDeletedAllHasFilters,
                    IDs: Ids,
                };
                //Gọi API xóa
                loading(true);
                apis.DeleteInventoryDocs(inventoryId, filterData, isDeleteDocs).then(res => {
                    toastr.success(res.message);

                }).catch(err => {
                    toastr.error(err.responseJSON.message);
                }).finally(() => {

                    //Reset checkedIds
                    inventoryViewModel.checkedIds([]);
                    inventoryViewModel.selectAll_UncheckedIds([]);
                    inventoryViewModel.selectAllPages(false);

                    //Turn off Select All:
                    $("#tab-inventory-create").find("#select_AllPages_Inventory").prop("checked", false)
                    $("#tab-inventory-create").find("#InventoryDocument_CheckAll").prop("checked", false)

                    //Reset Filters:
                    $("#CreateInventory_Plant").val("");
                    $("#CreateInventory_Plant_WhLoc").val("");

                    $("#CreateInventory_Department")[0].reset();
                    $("#CreateInventory_Department")[0].toggleSelectAll(true);

                    $("#CreateInventory_Area")[0].reset();
                    $("#CreateInventory_Area")[0].toggleSelectAll(true);

                    $("#CreateInventory_Form_Type")[0].reset();
                    $("#CreateInventory_Form_Type")[0].toggleSelectAll(true);

                    $("#CreateInventory_Quantity_From").val("");
                    $("#CreateInventory_Quantity_From-error").hide();
                    $("#CreateInventory_Quantity_To").val("");
                    $("#CreateInventory_Quantity_To-error").hide();
                    $("#CreateInventory_Component_Code").val("");
                    $("#CreateInventory_Model_Code").val("");
                    $("#CreateInventory_User_Distribution").val("");

                    InventoryDocDatatable.draw();
                    loading(false);
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

ko.applyBindings(inventoryViewModel, document.querySelector("#tab-inventory-create"));
