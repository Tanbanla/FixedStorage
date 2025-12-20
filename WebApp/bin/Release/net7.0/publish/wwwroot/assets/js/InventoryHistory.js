var host = $("#APIGateway").val();
let InventoryDocHistoryDatatable;
$(function () {
    waitForHistoryInventoryLanguageData();
    
});

function waitForHistoryInventoryLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        //InventoryViewDetail();
        //OpenImageInventory();
        var dropdownSelectors_History = [
            `#Inventory_Round`,
            `#Inventory_Area`,
            `#Inventory_Department`,
            `#Inventory_Type`
        ]

        dropdownSelectors_History.map(selctor => {
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

        //Hiển thị đợt kiểm kê gần nhất:

        $("#Inventory_Round")[0].reset();
        let currInventory_History = App?.User?.InventoryLoggedInfo?.InventoryModel?.Name;
        let firstInventoryOption_History = $("#Inventory_Round")[0]?.options[0]?.value || "";
        $("#Inventory_Round")[0].setValue(currInventory_History || firstInventoryOption_History);

        $("#Inventory_Department")[0].reset();
        $("#Inventory_Department")[0].toggleSelectAll(true);

        $("#Inventory_Area")[0].reset();
        $("#Inventory_Area")[0].toggleSelectAll(true);

        $("#Inventory_Type")[0].reset();
        $("#Inventory_Type")[0].toggleSelectAll(true);

        //Chon phong ban => hien thi danh sach khu vuc thuoc phong ban do:
        ChangeDepartmentGetLocation_HistoryInventoryDoc();

        //Validate Form Search:
        ValidateFormSearch()

        //Reset:
        ResetSearchHistoryInventoryDocument();

        //Khoi tao DataTable
        InitInventoryDocumentHistory_Datatable();

        //Export Excel:
        ExportFileInventoryDocumentHistory();

        //Click nut search:
        $(document).delegate(".btn_search", "click", (e) => {
            InventoryDocHistoryDatatable.draw()
        });

        InventoryHistoryHandler.init();

    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForHistoryInventoryLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}

function ResetSearchHistoryInventoryDocument() {
    $(".btn_reset").click(function () {
        $("#Inventory_Department")[0].reset();
        $("#Inventory_Department")[0].toggleSelectAll(true);

        $("#Inventory_Area")[0].reset();
        $("#Inventory_Area")[0].toggleSelectAll(true);

        $("#Inventory_Type")[0].reset();
        $("#Inventory_Type")[0].toggleSelectAll(true);

        $("#Inventory_Round")[0].reset();
        $("#Inventory_Round")[0].toggleSelectAll(true);

        $("#History_Inventory_Code").val("");
        $("#History_Inventory_Component_Code").val("");
        $("#History_Inventory_Model_Code").val("");
        $("#History_Inventory_Employee").val("");

        InventoryDocHistoryDatatable.draw()
    });
}
function ChangeDepartmentGetLocation_HistoryInventoryDoc() {
    $(document).delegate("#Inventory_Department", "change", (e) => {
        var listDepartments = $('#Inventory_Department').val();

        //Nếu bỏ tích hết phòng ban thì bỏ tích hết khu vực
        if (listDepartments.length == 0) {
            $("#Inventory_Area")[0].setOptions([]);
            $("#Inventory_Area")[0].reset();
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

                    var options = [
                    ];
                    let resultHtml = res?.data.map(item => {
                        options.push({ label: item.locationName, value: item.locationName })
                        return `
                        <option value="${item.locationName}">${item.locationName}</option>
                    `
                    }).join("");

                    $("#Inventory_Area")[0].virtualSelect.alwaysShowSelectedOptionsLabel = !(options.length > 1);
                    $("#Inventory_Area")[0].virtualSelect.disableAllOptionsSelectedText = !(options.length > 1);
                    $("#Inventory_Area")[0].virtualSelect.selectAllOnlyVisible = !(options.length > 1);
                    $("#Inventory_Area")[0].virtualSelect.autoSelectFirstOption = true;


                    document.querySelector('#Inventory_Area').setOptions(options);

                    $("#Inventory_Area")[0].reset();
                    $("#Inventory_Area")[0].toggleSelectAll(true);

                }

            },
            error: function (error) {
                toastr.error(error.message)
            }
        });


    })
}
function InitInventoryDocumentHistory_Datatable() {
    let host = App.ApiGateWayUrl;

    InventoryDocHistoryDatatable = $('#InventoryHistory_DataTable').DataTable({
        "bDestroy": true,
        "processing": `<div class="spinner"></div>`,
        pagingType: 'full_numbers',
        'language': {
            'loadingRecords': `<div class="spinner"></div>`,
            'processing': '<div class="spinner"></div>',
        },
        select: true,
        "serverSide": true,
        "scrollX": true,
        "filter": true,
        "searching": false,
        responsive: true,
        "lengthMenu": [10, 30, 50, 200],
        dom: 'rt<"bottom"flp><"clear">',
        "ordering": false,
        "ajax": {
            "url": host + `/api/inventory/web/history`,
            "type": "POST",
            "contentType": "application/x-www-form-urlencoded",
            dataType: "json",
            data: function (data) {
                let dataFilter = {
                    ComponentCode: "",
                    ModelCode: "",
                    DocCode: "",
                    AssigneeAccount: "",
                    Departments: "",
                    Locations: "",
                    DocTypes: "",
                    InventoryNames: "",
                    IsCheckAllDepartment: "",
                    IsCheckAllLocation: "",
                    IsCheckAllDocType: "",
                    IsCheckAllInventoryName: "",
                };
                //Check click Tat Ca:
                var isCheckAllDepartment = document.querySelector('#Inventory_Department').isAllSelected();
                var isCheckAllLocation = document.querySelector('#Inventory_Area').isAllSelected();
                var isCheckAllDocType = document.querySelector('#Inventory_Type').isAllSelected();
                var isCheckAllInventoryName = document.querySelector('#Inventory_Round').isAllSelected();

                if (isCheckAllDepartment) {
                    dataFilter.IsCheckAllDepartment = "-1";
                } else {
                    dataFilter.IsCheckAllDepartment = "";
                }
                if (isCheckAllLocation) {
                    dataFilter.IsCheckAllLocation = "-1";
                } else {
                    dataFilter.IsCheckAllLocation = "";
                }
                if (isCheckAllDocType) {
                    dataFilter.IsCheckAllDocType = "-1";
                } else {
                    dataFilter.IsCheckAllDocType = "";
                }
                if (isCheckAllInventoryName) {
                    dataFilter.IsCheckAllInventoryName = "-1";
                } else {
                    dataFilter.IsCheckAllInventoryName = "";
                }
                dataFilter.ComponentCode = $("#History_Inventory_Component_Code").val();
                dataFilter.ModelCode = $("#History_Inventory_Model_Code").val();
                dataFilter.AssigneeAccount = $("#History_Inventory_Employee").val();
                dataFilter.DocCode = $("#History_Inventory_Code").val();

                dataFilter.Departments = $("#Inventory_Department").val();
                dataFilter.Locations = $("#Inventory_Area").val();
                dataFilter.DocTypes = $("#Inventory_Type").val();
                dataFilter.InventoryNames = $("#Inventory_Round").val();

                Object.assign(data, dataFilter);

                return data;
            },
            "dataSrc": function ({ data }) {
                return data;
            }
        },
        "drawCallback": function (settings) {

            let totalPages = InventoryDocHistoryDatatable.page.info().pages;
            let totalRecords = InventoryDocHistoryDatatable.page.info().recordsTotal;

            let currPage = InventoryDocHistoryDatatable.page() + 1;
            if (currPage == 1) {
                $(".InventoryHistory_Container").find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $(".InventoryHistory_Container").find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }
            if (currPage == totalPages) {
                $(".InventoryHistory_Container").find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $(".InventoryHistory_Container").find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }

            $(".InventoryHistory_Container").find(".pagination").before(`<div class="h5 datatable_total_records"> <b>${window.languageData[window.currentLanguage]["Tổng"]}</b>: ${totalPages} <span class="total_record_seperate"> | </span>
                </div>`)

            //Ẩn xuất file nếu không có dữ liệu
            if (totalRecords == 0) {
                $("#export-file-listInventoryHistory").removeClass("btn_disabled").addClass("btn_disabled").attr("disabled", true);
            } else {
                $("#export-file-listInventoryHistory").removeClass("btn_disabled").attr("disabled", false);
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
                    let currentPage = InventoryDocHistoryDatatable.page() + 1;

                    let STT = ((currentPage - 1) * pagesize) + currentRow;

                    if (STT < 10) {
                        STT = `0${STT}`;
                    }
                    return STT;
                },
                "autoWidth": true
            },
            { "data": "inventoryName", "name": "Đợt kiểm kê", "autoWidth": true },
            { "data": "department", "name": "Phòng ban", "autoWidth": true },
            { "data": "location", "name": "Khu vực", "autoWidth": true },
            { "data": "docCode", "name": "Mã phiếu", "autoWidth": true },
            { "data": "componentCode", "name": "Mã linh kiện", "autoWidth": true },
            { "data": "modelCode", "name": "Model code", "autoWidth": true },
            { "data": "action", "name": "Loại thao tác", "autoWidth": true },
            {
                "data": "changeLog", "name": "Change log", render: function (data) {
                    return `<div class="pre-line">${data}</div>`
                }, "autoWidth": true
            },
            { "data": "assigneeAccount", "name": "Người thao tác", "autoWidth": true },
            { "data": "assigneeAccountDate", "name": "Thời gian thao tác", "autoWidth": true },
            {
                "data": "",
                "name": "",
                "render": function (data, type, row) {
                    const selectHtmlSpecial = `
                        <div class="Controls_Container">
                            <div class="ViewDetail_HistoryInventoryDoc_Controls mx-3">
                                <a class="detail-inventory-doc view_detail_Indoc history_in_doc_detail" data-id="${row.historyId}" data-inventoryId="${row.inventoryId}">${window.languageData[window.currentLanguage]["Xem chi tiết"]}</a>
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
function ExportFileInventoryDocumentHistory() {

    $(document).delegate("#export-file-listInventoryHistory", "click", (e) => {

        let dataFilter = {
            ComponentCode: "",
            ModelCode: "",
            DocCode: "",
            AssigneeAccount: "",
            Departments: "",
            Locations: "",
            DocTypes: "",
            InventoryNames: "",
            IsCheckAllDepartment: "",
            IsCheckAllLocation: "",
            IsCheckAllDocType: "",
            IsCheckAllInventoryName: "",
        };
        //Check click Tat Ca:
        var isCheckAllDepartment = document.querySelector('#Inventory_Department').isAllSelected();
        var isCheckAllLocation = document.querySelector('#Inventory_Area').isAllSelected();
        var isCheckAllDocType = document.querySelector('#Inventory_Type').isAllSelected();
        var isCheckAllInventoryName = document.querySelector('#Inventory_Round').isAllSelected();

        if (isCheckAllDepartment) {
            dataFilter.IsCheckAllDepartment = "-1";
        } else {
            dataFilter.IsCheckAllDepartment = "";
        }
        if (isCheckAllLocation) {
            dataFilter.IsCheckAllLocation = "-1";
        } else {
            dataFilter.IsCheckAllLocation = "";
        }
        if (isCheckAllDocType) {
            dataFilter.IsCheckAllDocType = "-1";
        } else {
            dataFilter.IsCheckAllDocType = "";
        }
        if (isCheckAllInventoryName) {
            dataFilter.IsCheckAllInventoryName = "-1";
        } else {
            dataFilter.IsCheckAllInventoryName = "";
        }
        dataFilter.ComponentCode = $("#History_Inventory_Component_Code").val();
        dataFilter.ModelCode = $("#History_Inventory_Model_Code").val();
        dataFilter.AssigneeAccount = $("#History_Inventory_Employee").val();
        dataFilter.DocCode = $("#History_Inventory_Code").val();

        dataFilter.Departments = $("#Inventory_Department").val();
        dataFilter.Locations = $("#Inventory_Area").val();
        dataFilter.DocTypes = $("#Inventory_Type").val();
        dataFilter.InventoryNames = $("#Inventory_Round").val();

        loading(true);

        var url = `/inventory/history/export`;
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

                    link.download = `Lichsukiemke_${formattedTime}.xlsx`;
                    link.click();
                } else {
                    toastr.error("Không tìm thấy file.");
                }
                toastr.success("Export danh sách lịch sử kiểm kê thành công.");
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
function ValidateFormSearch() {
    $("#History_Inventory_Code, #History_Inventory_Model_Code").on("input", function () {
        var inputValue = $(this).val();

        // Giới hạn chỉ nhập đến ký tự thứ 10
        if (inputValue.length > 10) {
            $(this).val(inputValue.substr(0, 10));
        }

        // Loại bỏ dấu cách
        $(this).val(function (index, value) {
            return value.replace(/\s/g, ''); // Loại bỏ dấu cách từ giá trị
        });
    });

    $("#History_Inventory_Component_Code").on("input", function () {
        var inputValue = $(this).val();

        // Giới hạn chỉ nhập đến ký tự thứ 9
        if (inputValue.length > 9) {
            $(this).val(inputValue.substr(0, 9));
        }

        // Loại bỏ dấu cách
        $(this).val(function (index, value) {
            return value.replace(/\s/g, ''); // Loại bỏ dấu cách từ giá trị
        });
    });

    $("#History_Inventory_Employee").on("input", function () {
        var inputValue = $(this).val();

        // Giới hạn chỉ nhập đến ký tự thứ 8
        if (inputValue.length > 8) {
            $(this).val(inputValue.substr(0, 8));
        }

        // Loại bỏ dấu cách
        $(this).val(function (index, value) {
            return value.replace(/\s/g, ''); // Loại bỏ dấu cách từ giá trị
        });
    });
}

; var InventoryHistoryHandler = (function () {
    let root = {
        parentEl: $("#InventoryHistory")
    }
    let detailFilterModel = {};
    let detailC_datatable;
    let zoomistSelector;

    let APIs = {
        HistoryDetailAPI: function (model) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/history/detail`;

                $.ajax({
                    url: url,
                    type: "POST",
                    contentType: "application/x-www-form-urlencoded",
                    dataType: "json",
                    data: model,
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

    }

    function Events() {
        //$(document).on("shown.bs.modal", function (e) {


        //})


        root.parentEl.delegate(".history_in_doc_detail", "click", function (e) {
            let inventoryId = $(this).attr("data-inventoryid");
            let historyId = $(this).attr("data-id");

            detailFilterModel.inventoryId = inventoryId;
            detailFilterModel.historyId = historyId;

            $("#Inventory_ViewDetailModal").modal("show");

            loading(true);
            APIs.HistoryDetailAPI(detailFilterModel).then(res => {
                let doc = res?.data;

                if (doc) {
                    let docType = doc.docType;

                    $("#Inventory_Round_Detail").text(doc.inventoryName)
                    $("#Inventory_Round_Detail").text(doc.inventoryName)
                    $("#Department_Detail").text(doc.departmentName)
                    $("#Area_Detail").text(doc.locationName)
                    $("#Form_Code_Detail").text(doc.docCode)
                    $("#Component_Code_Detail").text(doc.componentCode)
                    $("#Model_Code_Detail").text(doc.modelCode)
                    $("#Component_Name_Detail").text(doc.componentName)
                    $("#Type_Operation_Detail").text(doc.actionTitle)
                    $("#Note_Detail").text(doc.note)
                    $("#Change_Log_Detail").text(doc.changeLogText)
                    $("#User_Operation_Detail").text(doc.createBy)
                    $(".Time_Operation_Detail").text(doc.createAt)

                    if (doc.envicenceImage?.length) {
                        doc.envicenceImage = `${AppUser.getApiGateway}/${doc.envicenceImage.replaceAll('\\','/')}`;
                    }

                    //Hiển thị ảnh kiểm kê
                    IsImageExist(doc.envicenceImage, (valid) => {
                        if (valid) {
                            $(".wrapper_image_inventory").show(0);
                            $("#img-inventory").attr("src", doc.envicenceImage)
                            $("#img-inventory").attr("alt", doc.envicenceImageTitle || "");

                            $(".history_detail_image").attr("src", "");
                            $(".history_detail_image").attr("src", doc.envicenceImage)
                            $("#image_title").attr("alt", doc.envicenceImageTitle || "");

                        } else {
                            $(".history_detail_image").attr("src", "");
                            $("#image_title").attr("alt",  "");
                            $(".wrapper_image_inventory").hide(0);
                        }
                    })

                    if (doc.historyOutputs.length) {
                        let resultHtml = doc.historyOutputs.map(item => {
                            return `
                                    <tr>
                                        <td class="quantityPerBom">${ValidateInputHelper.Utils.convertDecimalInventory(item.quantityPerBom)}</td>
                                        <td class="quantityOfBom">${ValidateInputHelper.Utils.convertDecimalInventory(item.quantityOfBom)}</td>
                                    </tr>`
                        })

                        let total = doc.historyOutputs.reduce((acc, curr) => {
                            acc += (curr.quantityPerBom * curr.quantityOfBom)
                            return acc
                        }, 0)

                        root.parentEl.find("#InventoryDetail_DataTable tbody").html(resultHtml);
                        root.parentEl.find("#totalValue").html(ValidateInputHelper.Utils.convertDecimalInventory(total));
                    }

                    //Nếu là phiếu C thì mới hiển thị
                    if (docType == 3) {
                        $(".wrapper_table_detail").show();
                    } else {
                        $(".wrapper_table_detail").hide();
                    }

                    if (doc.componentCDetail.data.length)
                    {
                        detailC_datatable = $(root.parentEl).find(".inventory_history_detailC_table").DataTable({
                            "bDestroy": true,
                            "processing": `<div class="spinner"></div>`,
                            pagingType: 'full_numbers',
                            'language': {
                                'loadingRecords': `<div class="spinner"></div>`,
                                'processing': '<div class="spinner"></div>',
                            },
                            //"scrollX": true,
                            scrollCollapse: true,
                            "serverSide": true,
                            "filter": true,
                            "searching": false,
                            responsive: true,
                            "lengthMenu": [10, 30, 50, 200],
                            dom: 'rt<"bottom"flp><"clear">',
                            "ordering": false,
                            "ajax": {
                                "url": `${host}/api/inventory/history/detail`,
                                "type": "POST",
                                "contentType": "application/x-www-form-urlencoded",
                                dataType: "json",
                                data: function (data) {
                                    detailFilterModel.searchTerm = root.parentEl.find(".input_search").val();

                                    Object.assign(data, detailFilterModel);
                                    return data;
                                },
                                dataFilter: function (data) {
                                    var json = jQuery.parseJSON(data);
                                    json.recordsTotal = json.data.componentCDetail.recordsTotal;
                                    json.recordsFiltered = json.data.componentCDetail.recordsTotal;
                                    json.data = json.data.componentCDetail.data;

                                    return JSON.stringify(json);
                                }
                            },
                            "drawCallback": function (settings) {
                                let totalPages = detailC_datatable.page.info().pages;
                                let totalRecords = detailC_datatable.page.info().recordsTotal;

                                let currPage = detailC_datatable.page() + 1;
                                if (currPage == 1) {
                                    root.parentEl.find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                                    root.parentEl.find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                                }
                                if (currPage == totalPages) {
                                    root.parentEl.find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                                    root.parentEl.find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                                }

                            },
                            "columnDefs": [
                                { "width": "15%", "targets": 0 },
                            ],
                            "columns": [
                                {
                                    "data": "",
                                    "name": "STT",
                                    "render": function (data, type, row, index) {
                                        let pagesize = index.settings._iDisplayLength;
                                        let currentRow = ++index.row;
                                        let currentPage = detailC_datatable.page() + 1;
                                        let STT = ((currentPage - 1) * pagesize) + currentRow;
                                        if (STT < 10) {
                                            STT = `0${STT}`;
                                        }
                                        return STT;
                                    },
                                    "autoWidth": true
                                },
                                {
                                    "data": "componentCode", "componentcode": "Mã linh kiện", render: function (data, type, row, index) {
                                        return data || row.modelCode
                                    }, "autoWidth": true
                                },
                                { "data": "quantityOfBom", "bom": "quantityOfBom", "autoWidth": true },
                                {
                                    "data": "quantityPerBom", "quantityPerBom": "", render: function (data, type, row, index) {
                                        if (data > 0) {
                                            return ValidateInputHelper.Utils.convertDecimalInventory(data);
                                        }
                                        return data;
                                    }, "autoWidth": true
                                },
                            ],
                            "initComplete": function (settings, json) {
                                detailC_datatable.columns.adjust();

                                let tableId = settings.sTableId;
                                let datatableLength = $(`#${tableId}_length`);

                                let optionValues = settings.aLengthMenu;
                                let length = settings._iDisplayLength;
                                let resultHtml = optionValues.map((val, i) => {
                                    return `<option value="${val}">Hiển thị ${val}</option>`
                                }).join('')

                                let selectElement = datatableLength.find("select");
                                selectElement.html(`${resultHtml}`)
                                selectElement.val(length).change();

                                let label = datatableLength.contents().eq(0);
                                $(label).contents().each((i, el) => {
                                    if ($(el).is("select") == false) {
                                        $(el).remove()
                                    }
                                })
                            }
                        });
                    }
                }

            }).catch(err => {
            }).finally(() => {
                loading(false);
            })
        })


        root.parentEl.delegate(".input_search", "keypress", ValidateInputHelper.FormEnter(ValidateInputHelper.Utils.debounce(() => {
            detailC_datatable.draw();
        }, 300)))

        //root.parentEl.delegate("#btnCloseViewImage", "click", function (e) {
        //    let imageModal = $(root.parentEl).find("#InventoryHistoryViewImage");
        //})

        root.parentEl.delegate(".input_search", "keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(10));
        root.parentEl.delegate(".input_search", "keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(10));
        //Chặn nhập khoảng trắng và cắt khoảng trắng khi paste
        $(".input_search").on("keypress", ValidateInputHelper.PreventWhiteSpace);
        $(".input_search").on("keyup", ValidateInputHelper.RemoveWhiteSpaceOnKeyup);


        root.parentEl.find("#btn_history_detail_search").click(ValidateInputHelper.Utils.debounce(function () {
            detailC_datatable.draw();
        }, 300));


        //Click ảnh lịch sử kiểm kê
        root.parentEl.delegate("#Inventory_ViewDetailModal #img-inventory", "click", function (e) {
            let imgSrc = $(this).attr("src")
            let imageModal = $(root.parentEl).find("#InventoryHistoryViewImage");
            imageModal.modal("show");

            imageModal.find(".img_inventory_zoom #image_title").text($(this).attr("alt"));

            document.querySelector(".history_detail_image").setAttribute("data-zoomist-src", imgSrc);
            setTimeout(() => {
                if (zoomistSelector) {
                    zoomistSelector.destroy();
                }
                const myZoomist = document.querySelector(".history_detail_image")
                zoomistSelector = new Zoomist(myZoomist, {
                    //src: imgSrc,
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

            }, 200);
        })
    }

    function PreLoad() {
        
    }

    function Init() {
        Cache();
        Events();
        PreLoad();
    }

    return {
        init: Init
    }
})();