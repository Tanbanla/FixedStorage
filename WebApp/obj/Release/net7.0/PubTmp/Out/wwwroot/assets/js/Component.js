var componentDataTable;

$(function () {

    waitForComponentLanguageData()
})

function waitForComponentLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        ListComponentsViewDetail()
        FirstLoadComponents();
        AddNewComponentViewDetail()
        Validation_AddComponent()

        //Ham chinh sua linh kien
        EditComponent()

        //Filter linh kiện:
        GetFilteredComponents()
        ResetSearch();
        CloseModal();

        //Hàm chặn các ký tự đặc biệt:
        $('#Add_Components_ComponentCode,#Edit_Components_ComponentCode').keypress(function (event) {
            var character = String.fromCharCode(event.keyCode);
            return componentIsValidText(character);
        });

        //Hàm chỉ cho nhập số, chữ, "-", "/":
        $('#Add_Components_PositionCode,#Edit_Components_PositionCode').keypress(function (event) {
            var character = String.fromCharCode(event.keyCode);
            return isValidSpecialCheck(character);
        });

        //Hàm chỉ cho nhập số:
        $('#Component_Inventory_Qty_Start,#Component_Inventory_Qty_End').keypress(function (event) {
            var character = String.fromCharCode(event.keyCode);
            return componentIsValidNumber(character);
        });

        $('#Component_Inventory_Qty_Start, #Component_Inventory_Qty_End').maskMoney({
            allowZero: true,
            defaultZero: false,
            allowEmpty: true,
            allowNegative: false,
            precision: 0,
            selectAllOnFocus: false,
            bringCaretAtEndOnFocus: false
        });
        $('#Component_Inventory_Qty_Start,#Component_Inventory_Qty_End').keypress(ValidateInputHelper.LimitInputLengthOnKeyPress(8));
        $('#Component_Inventory_Qty_Start,#Component_Inventory_Qty_End').keyup(ValidateInputHelper.LimitInputLengthOnKeyPress(8));

        //$('#Add_Components_InventoryMinQty,#Add_Components_InventoryMaxQty,#Add_Components_InventoryQty,#Edit_Components_InventoryMinQty,#Edit_Components_InventoryMaxQty,#Edit_Components_InventoryQty').keypress(function (event) {
        //    var inputValue = String.fromCharCode(event.keyCode); // Lấy ký tự vừa nhập

        //    // Sử dụng regex để kiểm tra xem ký tự vừa nhập có phải là số hay không
        //    var isNumber = /^[\d.]$/.test(inputValue);

        //    // Nếu ký tự không phải là số, ngăn người dùng nhập
        //    if (!isNumber) {
        //        event.preventDefault();
        //    }
        //});

        // var num = value.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        // Nhập khoảng số lượng tồn kho từ, số lượng tồn kho đến
        // Khoảng số lượng tồn kho đến phải lớn hơn số lượng tồn kho từ
        $(document).delegate('#Component_Inventory_Qty_Start', "change", (e) => {
            var inventoryQtyEnd = 0;
            var qtyEndNum = '';
            var qtyEndTxt = $('#Component_Inventory_Qty_End').val();
            var qtyStartTxt = e.currentTarget.value;

            if (qtyStartTxt.length <= 0 && qtyEndTxt.length <= 0) {
                $('#Component_Inventory_Qty_Start_span-error').hide();
                $('#Component_Inventory_Qty_End_span-error').hide();
                return;
            }

            if (qtyEndTxt != '') {
                qtyEndNum = qtyEndTxt.replaceAll(',', '');
                inventoryQtyEnd = parseInt(qtyEndNum);
            }

            var inventoryQtyStart = 0;
            var qtyStartNum = '';
            if (qtyStartTxt != '') {
                qtyStartNum = qtyStartTxt.replaceAll(',', '');
                inventoryQtyStart = parseInt(qtyStartNum);
            }
            if (inventoryQtyStart > 0 && inventoryQtyEnd > 0 && inventoryQtyStart > inventoryQtyEnd) {
                $('#Component_Inventory_Qty_Start_span-error').text('Sai định dạng, vui lòng nhập lại.');
            }
            else {
                $('#Component_Inventory_Qty_Start_span-error').text('');
                $('#Component_Inventory_Qty_End_span-error').text('');
            }
            //var qtyStartNumToView = qtyStartNum.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ",");
            //$('#Component_Inventory_Qty_Start').val(qtyStartNumToView);
            //var qtyEndNumToView = qtyEndNum.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ",");
            //$('#Component_Inventory_Qty_End').val(qtyEndNumToView);
        });
        $(document).delegate('#Component_Inventory_Qty_End', "change", (e) => {
            var inventoryQtyStart = 0;
            var qtyStartNum = '';

            var qtyStartTxt = $('#Component_Inventory_Qty_Start').val();
            var qtyEndTxt = e.currentTarget.value;

            if (qtyStartTxt.length <= 0 && qtyEndTxt.length <= 0) {
                $('#Component_Inventory_Qty_Start_span-error').hide();
                $('#Component_Inventory_Qty_End_span-error').hide();
                return;
            }

            if (qtyStartTxt != '') {
                qtyStartNum = qtyStartTxt.replaceAll(',', '');
                inventoryQtyStart = parseInt(qtyStartNum);
            }

            var inventoryQtyEnd = 0;
            var qtyEndNum = '';
            if (qtyEndTxt != '') {
                qtyEndNum = qtyEndTxt.replaceAll(',', '');
                inventoryQtyEnd = parseInt(qtyEndNum);
            }
            if (inventoryQtyStart > 0 && inventoryQtyEnd > 0 && inventoryQtyEnd < inventoryQtyStart) {
                $('#Component_Inventory_Qty_End_span-error').text('Sai định dạng, vui lòng nhập lại.');
            }
            else {
                $('#Component_Inventory_Qty_Start_span-error').text('');
                $('#Component_Inventory_Qty_End_span-error').text('');
            }
            //var qtyStartNumToView = qtyStartNum.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ",");
            //$('#Component_Inventory_Qty_Start').val(qtyStartNumToView);
            //var qtyEndNumToView = qtyEndNum.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ",");
            //$('#Component_Inventory_Qty_End').val(qtyEndNumToView);
        });

        CheckMaxLengthComponentCode();
        CheckMaxLengthComponentName();
        CheckMaxLengthComponentSupplierCode();
        CheckMaxLengthComponentSupplierName();
        CheckMaxLengthComponentShortSupplierName();
        CheckMaxLengthComponentPositionCode();
        CheckMaxLengthComponentInventoryMinQty();
        CheckMaxLengthComponentInventoryMaxQty();
        CheckMaxLengthComponentInventoryMaxQty();
        CheckMaxLengthComponentInventoryQty();
        CheckMaxLengthComponentInfo();
        CheckMaxLengthComponentNote();
        //ConvertNumberThousands();
        InputDoubleAddNewComponent();

        //Click nút đồng ý thêm linh kiện:
        ApplyAddNewComponent()

        //Click nút đồng ý chinh sua linh kiện:
        ApplyEditComponent()

        //Hàm checked all để xóa tất cả linh kiện trong 1 trang:
        CheckedAllDeteleComponent()

        //Bật tắt nút xóa linh kiện:
        ShowAndHideButtonDeleteComponent()

        //Click button Xóa linh kiện:
        ClickButtonDeleteComponent()

        //Click button Xuất Excel Linh kiện:
        ExportComponentList()

        // Import linh kiện:
        ImportComponentList();

        //Tải mẫu import linh kiện excel
        DownloadImportComponentTemplate();

    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForComponentLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}


var maxLengthComponentCode = 9;
var maxLengthComponentName = 150;
var maxLengthComponentSupplierCode = 50;
var maxLengthComponentSupplierName = 250;
var maxLengthComponentShortSupplierName = 250;
var maxLengthComponentPositionCode = 20;
var maxLengthComponentInventoryMinQty = 10;
var maxLengthComponentInventoryMaxQty = 10;
var maxLengthComponentInventoryQty = 10;
var maxLengthComponentComponentInfo = 250;
var maxLengthComponentComponentNote = 50;

// chặn nhập nếu mã linh kiện dài quá maxLength
function CheckMaxLengthComponentCode() {
    $("#Add_Components_ComponentCode, #Edit_Components_ComponentCode").on('input', function () {
        if ($(this).val().length > maxLengthComponentCode) {
            $(this).val($(this).val().slice(0, maxLengthComponentCode));
        }
    });
};

function CheckMaxLengthComponentName() {
    $("#Add_Components_ComponentName, #Edit_Components_ComponentName").on('input', function () {
        if ($(this).val().length > maxLengthComponentName) {
            $(this).val($(this).val().slice(0, maxLengthComponentName));
        }
    });
};

function CheckMaxLengthComponentSupplierCode() {
    $("#Add_Components_SupplierCode, #Edit_Components_SupplierCode").on('input', function () {
        if ($(this).val().length > maxLengthComponentSupplierCode) {
            $(this).val($(this).val().slice(0, maxLengthComponentSupplierCode));
        }
    });
};

function CheckMaxLengthComponentSupplierName() {
    $("#Add_Components_SupplierName, #Edit_Components_SupplierName").on('input', function () {
        if ($(this).val().length > maxLengthComponentSupplierName) {
            $(this).val($(this).val().slice(0, maxLengthComponentSupplierName));
        }
    });
};

function CheckMaxLengthComponentShortSupplierName() {
    $("#Add_Components_ShortSupplierName, #Edit_Components_ShortSupplierName").on('input', function () {
        if ($(this).val().length > maxLengthComponentShortSupplierName) {
            $(this).val($(this).val().slice(0, maxLengthComponentShortSupplierName));
        }
    });
};

function CheckMaxLengthComponentPositionCode() {
    $("#Add_Components_PositionCode, #Edit_Components_PositionCode").on('input', function () {
        if ($(this).val().length > maxLengthComponentPositionCode) {
            $(this).val($(this).val().slice(0, maxLengthComponentPositionCode));
        }
    });
};

function CheckMaxLengthComponentInventoryMinQty() {
    //$("#Add_Components_InventoryMinQty, #Edit_Components_InventoryMinQty").on('input', function () {
    //    if ($(this).val().length > maxLengthComponentInventoryMinQty) {
    //        $(this).val($(this).val().slice(0, maxLengthComponentInventoryMinQty));
    //    }
    //});
};

function CheckMaxLengthComponentInventoryMaxQty() {
    //$("#Add_Components_InventoryMaxQty, #Edit_Components_InventoryMaxQty").on('input', function () {
    //    if ($(this).val().length > maxLengthComponentInventoryMaxQty) {
    //        $(this).val($(this).val().slice(0, maxLengthComponentInventoryMaxQty));
    //    }
    //});
};

function CheckMaxLengthComponentInventoryQty() {
    //$("#Add_Components_InventoryQty, #Edit_Components_InventoryQty").on('input', function () {
    //    if ($(this).val().length > maxLengthComponentInventoryQty) {
    //        $(this).val($(this).val().slice(0, maxLengthComponentInventoryQty));
    //    }
    //});
};

function CheckMaxLengthComponentInfo() {
    $("#Add_Components_ComponentInfo, #Edit_Components_ComponentInfo").on('input', function () {
        if ($(this).val().length > maxLengthComponentComponentInfo) {
            $(this).val($(this).val().slice(0, maxLengthComponentComponentInfo));
        }
    });
};

function CheckMaxLengthComponentNote() {
    $("#Add_Components_Note, #Edit_Components_Note").on('input', function () {
        if ($(this).val().length > maxLengthComponentComponentNote) {
            $(this).val($(this).val().slice(0, maxLengthComponentComponentNote));
        }
    });
};

function InputDoubleAddNewComponent() {
    //$('#Add_Components_InventoryMinQty,#Add_Components_InventoryMaxQty,#Add_Components_InventoryQty,#Edit_Components_InventoryMinQty,#Edit_Components_InventoryMaxQty,#Edit_Components_InventoryQty,#Component_Inventory_Qty_Start,#Component_Inventory_Qty_End').on("input", function () {
    //    var inputValue = $(this).val();
    //    var sanitizedValue = inputValue.replace(/[^\d,]/g, '');
    //    sanitizedValue = sanitizedValue.replace(/,/g, '').replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    //    $(this).val(sanitizedValue);
    //});


    new AutoNumeric(`#Add_Components_InventoryMaxQty`,
        {
            allowDecimalPadding: false,
            decimalPlaces: 6,
            maximumValue: 99999999,
            //decimalPlacesShownOnBlur: AutoNumeric.options.decimalPlacesShownOnBlur.none,
        }
    );

    new AutoNumeric(`#Add_Components_InventoryQty`,
        {
            allowDecimalPadding: false,
            decimalPlaces: 6,
            maximumValue: 99999999,
            //decimalPlacesShownOnBlur: AutoNumeric.options.decimalPlacesShownOnBlur.none,
        }
    );

    new AutoNumeric(`#Add_Components_InventoryMinQty`,
        {
            allowDecimalPadding: false,
            decimalPlaces: 6,
            maximumValue: 99999999,
            //decimalPlacesShownOnBlur: AutoNumeric.options.decimalPlacesShownOnBlur.none,
        }
    );

}


var autoNumeric_EditComponent_InventoryMin;
var autoNumeric_EditComponent_InventoryMax;
var autoNumeric_EditComponent_Inventory;
function InputDoubleUpdateComponent() {
    autoNumeric_EditComponent_InventoryMin?.remove();
    autoNumeric_EditComponent_InventoryMin = new AutoNumeric(`#Edit_Components_InventoryMinQty`,
        {
            allowDecimalPadding: false,
            decimalPlaces: 6,
            maximumValue: 99999999,
        }
    );
    autoNumeric_EditComponent_InventoryMax?.remove();
    autoNumeric_EditComponent_InventoryMax = new AutoNumeric(`#Edit_Components_InventoryMaxQty`,
        {
            allowDecimalPadding: false,
            decimalPlaces: 6,
            maximumValue: 99999999,
        }
    );
    autoNumeric_EditComponent_Inventory?.remove();
    autoNumeric_EditComponent_Inventory = new AutoNumeric(`#Edit_Components_InventoryQty`,
        {
            allowDecimalPadding: false,
            decimalPlaces: 6,
            maximumValue: 99999999,
        }
    );
}

function FirstLoadComponents() {
    VirtualSelect.init({
        ele: '#Component_Layout',
        selectAllText: window.languageData[window.currentLanguage]['Tất cả'],
        noOptionsText: window.languageData[window.currentLanguage]['Không có kết quả'],
        noSearchResultsText: window.languageData[window.currentLanguage]['Không có kết quả'],
        searchPlaceholderText: window.languageData[window.currentLanguage]['Tìm kiếm'],
        allOptionsSelectedText: window.languageData[window.currentLanguage]['Tất cả'],
        optionsSelectedText: window.languageData[window.currentLanguage]["điều kiện đã được chọn"],
        selectAllOnlyVisible: true,
        hideClearButton: true,
    });

    var layoutValues = $('#txtSelectedLayout').val();
    if (layoutValues.length > 0) {
        $("#Component_Layout")[0].setValue(layoutValues);
    }
    else {
        $("#Component_Layout")[0].reset();
        $("#Component_Layout")[0].toggleSelectAll(true);
    }

    setTimeout(() => {
        GetComponents()

        //componentDataTable.draw();
    }, 1)
}

function GetComponents() {
    var link = $("#APIGateway").val();

    var tongText = window.languageData[window.currentLanguage]["Tổng"];

    componentDataTable = $('#ListComponent_DataTable').DataTable({
        destroy: true,
        "processing": `<span class="spinner"></span>`,
        "serverSide": true,
        "filter": true,
        //scrollY: 700,
        scrollX: true,
        "lengthMenu": [10, 30, 50, 200],
        dom: 'rt<"bottom"flp><"clear">',
        "ordering": false,
        
        "ajax": {
            "url": link + "/api/storage/get-component-list",
            "type": "POST",
            "contentType": "application/x-www-form-urlencoded",
            dataType: "json",
            data: function (data) {
                var componentCode = $("#Component_code").val();
                var componentName = $("#Component_Name").val();
                var supplierName = $("#Supplier_Name").val();
                var componentPosition = $("#Component_Position").val();
                var inventoryQtyStartVal = 0, inventoryQtyEndVal = 0;
                var componentInventoryQtyStart = $("#Component_Inventory_Qty_Start").val().replace(',', '');
                inventoryQtyStartVal = parseInt(componentInventoryQtyStart);
                var componentInventoryQtyEnd = $("#Component_Inventory_Qty_End").val().replace(',', '');
                inventoryQtyEndVal = parseInt(componentInventoryQtyEnd);
                if (inventoryQtyStartVal > 0 && inventoryQtyEndVal > 0 && inventoryQtyEndVal < inventoryQtyStartVal) {
                    $('#Component_Inventory_Qty_End_span-error').text('Sai định dạng, vui lòng nhập lại.');
                    return;
                }
                var inventoryStatus = $('#InventoryStatus').val();

                var allLayouts;
                var layoutIds;
                var checkAllLayouts = document.querySelector('#Component_Layout').isAllSelected();
                if (checkAllLayouts) {
                    allLayouts = -1;
                }
                else {
                    layoutIds = $('#Component_Layout').val();
                }

                let filterModel = {
                    ComponentCode: componentCode,
                    ComponentName: componentName,
                    SupplierName: supplierName,
                    ComponentPosition: componentPosition,
                    AllLayouts: allLayouts,
                    LayoutIds: layoutIds,
                    ComponentInventoryQtyStart: componentInventoryQtyStart == "" ? null : inventoryQtyStartVal,
                    ComponentInventoryQtyEnd: componentInventoryQtyEnd == "" ? null : inventoryQtyEndVal,
                    InventoryStatus: inventoryStatus
                };



                Object.assign(data, filterModel);
                return data;
            },
            "dataSrc": function (response) {
                // Cập nhật tổng số bản ghi trong DataTables
                componentDataTable.page.info().recordsTotal = response.totalRecords;
                componentDataTable.page.info().recordsDisplay = response.totalRecords;

                // Trả về dữ liệu để hiển thị trong DataTables, show if response.data > 0 else hide

                if (response.data.length > 0) {
                    $(".btnExportExcel_ListComponent").attr("disabled", false)
                    $(".btnExportExcel_ListComponent").addClass("export-color-blue")
                    $(".btnExportExcel_ListComponent").removeClass("export-color-grey")
                }
                else {
                    $(".btnExportExcel_ListComponent").attr("disabled", true)
                    $(".btnExportExcel_ListComponent").addClass("export-color-grey")
                    $(".btnExportExcel_ListComponent").removeClass("export-color-blue")
                }


                return response.data;
            }
        },
        "drawCallback": function (settings, data) {
            let totalPages = componentDataTable.page.info().pages;
            let totalRecords = componentDataTable.page.info().recordsTotal;

            let currPage = componentDataTable.page() + 1;
            if (currPage == 1) {
                $(".ListComponent_Container").find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $(".ListComponent_Container").find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }
            if (currPage == totalPages) {
                $(".ListComponent_Container").find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                $(".ListComponent_Container").find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
            }

            $(".ListComponent_Container").find(".pagination").before(`<div class="h5 datatable_total_records"> <b>${tongText}</b>: ${totalPages} <span class="total_record_seperate"> | </span>
                </div>`)

            let isNew = true;
            //let rowElement = $(`#ListComponent_DataTable tbody tr`).eq(index);
            //if (isNew) {
            //    rowElement.removeClass("new_component").addClass("new_component")
            //}
        },
        "columnDefs": [
            //{ "width": "80px", "targets": [3] },
            //{ "width": "80px", "targets": 4 },
            //{ "width": "5%", "targets": 2 }, 

        ],
        "columns": [
            {
                "data": "id",
                "name": "checkbox",
                "render": function (data, type, row, table) {
                    let aoData = table.settings.aoData;
                    let index = table.row;
                    let isNew = row?.isNewCreateAt || row?.isNewUpdateAt || false;

                    let tr = aoData[index].nTr;
                    if (isNew) {
                        $(tr).removeClass("new_component").addClass("new_component");
                    }
                    return `<input type="checkbox" class="Component_check" data-id="${data}" />`
                },
                "autoWidth": true
            },
            {
                "data": "",
                "name": "STT",
                "render": function (data, type, row, index) {
                    let pagesize = index.settings._iDisplayLength;
                    let currentRow = ++index.row;
                    let currentPage = componentDataTable.page() + 1;

                    let STT = ((currentPage - 1) * pagesize) + currentRow;

                    if (STT < 10) {
                        STT = `0${STT}`;
                    }
                    return STT;
                },
                "autoWidth": true
            },
            { "data": "componentCode", "name": "Mã linh kiện", "autoWidth": true },
            {
                "data": "componentName", "name": "Tên linh kiện",
                render: function (data) {
                    return `<div class="break-word-100">${data}</div>`
                },
                //"autoWidth": true,
                width: "200"
            },
            {
                "data": "supplierName", "name": "Tên nhà cung cấp",
                render: function (data) {
                    return `<div class="">${data}</div>`
                },
            },
            { "data": "componentPosition", "name": "Vị trí cố định", "autoWidth": true },
            {
                "data": "inventoryNumber", "name": "Tồn kho thực tế",
                "render": function (data, type, row) {
                    let convertNumber = ValidateInputHelper.Utils.convertDecimalInventory(data);
                    return convertNumber;
                },
                "autoWidth": true
            },
            {
                "data": "maxInventoryNumber", "name": "Tồn kho (lớn nhất)",
                "render": function (data, type, row) {
                    let convertNumber = ValidateInputHelper.Utils.convertDecimalInventory(data);
                    return convertNumber;
                },
                "autoWidth": true
            },
            {
                "data": "id",
                "name": "",
                "render": function (data, type, row) {
                    let isGrant = App.User.isGrant("MASTER_DATA_WRITE") == false ? "" : `<div class="EditListComponent_Controls mx-3" data-id="${data}">
                                                                                            <svg width="14" height="14" viewBox="0 0 14 14" fill="none" xmlns="http://www.w3.org/2000/svg">
                                                                                                <path fill-rule="evenodd" clip-rule="evenodd" d="M7.98948 3.19322C9.15031 2.03238 10.2586 2.06155 11.3903 3.19322C11.9736 3.77072 12.2536 4.33072 12.2478 4.90822C12.2478 5.46822 11.9678 6.02239 11.3903 6.59405L10.6903 7.29989C10.6436 7.34655 10.5853 7.36988 10.5211 7.36988C10.4978 7.36988 10.4745 7.36405 10.4511 7.35822C8.90531 6.91488 7.66864 5.67822 7.22531 4.13239C7.20198 4.05072 7.22531 3.95738 7.28364 3.89905L7.98948 3.19322ZM8.91115 7.63238C9.06865 7.72572 9.23198 7.80738 9.40114 7.88905C9.62883 7.98771 9.69475 8.28961 9.51929 8.46507L6.23364 11.7507C6.16364 11.8266 6.01781 11.8966 5.91281 11.9141L3.67281 12.2291C3.60281 12.2407 3.53281 12.2466 3.46281 12.2466C3.14781 12.2466 2.85614 12.1357 2.64614 11.9316C2.40114 11.6807 2.29031 11.3074 2.34864 10.9107L2.66364 8.67655C2.68114 8.57738 2.75114 8.43155 2.82698 8.35572L6.10721 5.07548C6.28625 4.89644 6.5874 4.9646 6.69448 5.19405C6.77614 5.35738 6.85781 5.51488 6.95114 5.67238C7.02698 5.80072 7.10864 5.92905 7.17864 6.01655C7.26035 6.14184 7.35224 6.25187 7.41154 6.32288C7.41575 6.32792 7.41979 6.33275 7.42365 6.33738C7.4319 6.348 7.43968 6.35813 7.44698 6.36765C7.47579 6.4052 7.49718 6.43308 7.51114 6.44238C7.70364 6.67572 7.91948 6.88572 8.11198 7.04905C8.15864 7.09572 8.19948 7.13072 8.21114 7.13655C8.32198 7.22988 8.43864 7.32322 8.53781 7.38738C8.66031 7.47488 8.78281 7.55655 8.91115 7.63238Z" fill="#87868C"/>
                                                                                            </svg>
                                                                                        </div>`;

                    let viewDetailText = window.languageData[window.currentLanguage]["Xem chi tiết"];
                    const selectHtmlSpecial = `
                        <div class="Controls_Container">
                            <div class="ViewDetail_Controls mx-3">
                                <a class="ListComponent_ViewDetail_Controls" data-id="${data}">${viewDetailText}</a>
                            </div>
                            ${isGrant}

                        </div>
                    `;
                    return selectHtmlSpecial;
                }
            }
        ],
    });

    //componentDataTable.draw()
    $('#ListComponent_DataTable_length label').contents().filter(function () {
        // Lọc ra các phần tử text và thay đổi nội dung của chúng
        return this.nodeType === 3;
    }).each(function () {
        // Thay đổi nội dung của phần tử text
        this.nodeValue = this.nodeValue.replace('Hiển thị ', '').replace(' bản ghi', '');
    });

    $('select[name="ListComponent_DataTable_length"] option').each(function () {
        // Thêm chữ "Hiển thị " vào trước nội dung của option
        $(this).text('Hiển thị ' + $(this).text());
    });
}

//Click nút đồng ý thêm linh kiện:
function ApplyAddNewComponent() {
    $(document).delegate("#button_Apply_AddNewComponent", "click", (e) => {
        if ($("#AddNewComponent_Form").valid()) {
            var link = $("#APIGateway").val();

            var userid = App.User.UserId;

            var componentCode = $("#Add_Components_ComponentCode").val();
            var componentName = $("#Add_Components_ComponentName").val();
            var supplierCode = $("#Add_Components_SupplierCode").val();
            var supplierName = $("#Add_Components_SupplierName").val();
            var supplierShortName = $("#Add_Components_ShortSupplierName").val();
            var positionCode = $("#Add_Components_PositionCode").val();
            var inventoryNumber = $("#Add_Components_InventoryQty").val();
            var minInventoryNumber = $("#Add_Components_InventoryMinQty").val();
            var maxInventoryNumber = $("#Add_Components_InventoryMaxQty").val();
            var componentInfo = $("#Add_Components_ComponentInfo").val();
            var note = $("#Add_Components_Note").val();

            var formData = {
                ComponentCode: componentCode,
                ComponentName: componentName,
                SupplierCode: supplierCode,
                SupplierName: supplierName,
                SupplierShortName: supplierShortName,
                PositionCode: positionCode,
                InventoryNumber: parseFloat(inventoryNumber.replace(/,/g, '')),
                MinInventoryNumber: parseFloat(minInventoryNumber.replace(/,/g, '')),
                MaxInventoryNumber: parseFloat(maxInventoryNumber.replace(/,/g, '')),
                ComponentInfo: componentInfo,
                Note: note,
                userId: userid
            };

            $.ajax({
                type: 'POST',
                url: link + '/api/storage/component/add',
                data: formData,
                dataType: "json",
                encode: true,
                success: function (res) {
                    if (res.code == 200) {
                        componentDataTable.draw();
                        toastr.success(res.message)
                        $("#AddNewComponentModal").modal("hide");
                        ResetCreateComponentModal();

                        //Reload
                        //setTimeout(() => {
                        //    location.reload();
                        //}, 1500)
                    }
                    else if (res.code == 56) {
                        toastr.error(res.message)
                    }

                    if (res.code === 59) {
                        $("#Add_Components_PositionCode-error").text(res.message)
                        $("#Add_Components_PositionCode").focus();
                        toastr.error(res.message)
                    }
                    else if (res.code == 63) {
                        toastr.error(res.message)
                    }
                    else {
                        $("#Add_Components_PositionCode-error").text()
                    }

                    //else if (res.code == 56) {
                    //    toastr.error(res.message)
                    //}
                    //else if (res.code == 57) {
                    //    toastr.error(res.message)
                    //}
                    //else if (res.code == 58) {
                    //    toastr.error(res.message)
                    //}
                },
                error: function (error) {
                    if (error.responseJSON.code === 59) {
                        $("#Add_Components_PositionCode-error").text(error.responseJSON.message)
                    } else {
                        $("#Add_Components_PositionCode-error").text('')
                    }

                    if (error.responseJSON.code === 61) {
                        $("#Add_Components_InventoryMinQty-error").text(error.responseJSON.message)
                    } else {
                        $("#Add_Components_InventoryMinQty-error").text('')
                    }

                    if (error.responseJSON.code === 62) {
                        $("#Add_Components_InventoryQty-error").text(error.responseJSON.message)
                    } else {
                        $("#Add_Components_InventoryQty-error").text('')
                    }
                    if (error.responseJSON.code === 400) {
                        toastr.error(error.responseJSON.message)
                    }
                }
            });
        }
    })
}

//Reset sau khi Thêm Mới Linh Kiện:
function ResetCreateComponentModal() {
    $("#Add_Components_ComponentCode").val('');
    $("#Add_Components_ComponentName").val('');
    $("#Add_Components_SupplierCode").val('');
    $("#Add_Components_SupplierName").val('');
    $("#Add_Components_ShortSupplierName").val('');
    $("#Add_Components_PositionCode").val('');
    $("#Add_Components_InventoryQty").val('');
    $("#Add_Components_InventoryMinQty").val('');
    $("#Add_Components_InventoryMaxQty").val('');
    $("#Add_Components_ComponentInfo").val('');
    $("#Add_Components_Note").val('');
}

//Reset sau khi Sửa Linh Kiện:
function ResetUpdateComponentModal() {
    $("#Edit_Components_ComponentCode").val('');
    $("#Edit_Components_ComponentName").val('');
    $("#Edit_Components_SupplierCode").val('');
    $("#Edit_Components_SupplierName").val('');
    $("#Edit_Components_PositionCode").val('');
    $("#Edit_Components_InventoryQty").val('');
    $("#Edit_Components_InventoryMinQty").val('');
    $("#Edit_Components_InventoryMaxQty").val('');
    $("#Edit_Components_ComponentInfo").val('');
    $("#Edit_Components_Note").val('');
}

function ResetCreateComponentModalError() {
    $("#Add_Components_ComponentCode-error").hide();
    $("#Add_Components_ComponentName-error").hide();
    $("#Add_Components_SupplierCode-error").hide();
    $("#Add_Components_SupplierName-error").hide();
    $("#Add_Components_ShortSupplierName-error").hide();
    $("#Add_Components_PositionCode-error").hide();
    $("#Add_Components_InventoryQty-error").hide();
    $("#Add_Components_InventoryMinQty-error").hide();
    $("#Add_Components_InventoryMaxQty-error").hide();
    $("#Add_Components_ComponentInfo-error").hide();
    $("#Add_Components_Note-error").hide();
}

function ResetEditComponentModalError() {
    $("#Edit_Components_ComponentCode-error").hide();
    $("#Edit_Components_ComponentName-error").hide();
    $("#Edit_Components_SupplierCode-error").hide();
    $("#Edit_Components_SupplierName-error").hide();
    $("#Edit_Components_ShortSupplierName-error").hide();
    $("#Edit_Components_PositionCode-error").hide();
    $("#Edit_Components_InventoryQty-error").hide();
    $("#Edit_Components_InventoryMinQty-error").hide();
    $("#Edit_Components_InventoryMaxQty-error").hide();
    $("#Edit_Components_ComponentInfo-error").hide();
    $("#Edit_Components_Note-error").hide();
}

function CloseModal() {
    $("#button_Delete_AddNewComponent").on("click", function () {
        ResetCreateComponentModal();
        ResetCreateComponentModalError();
    })

    $("#button_Delete_EditComponent").on("click", function () {
        ResetUpdateComponentModal();
        ResetEditComponentModalError();
    })
}

//Click nút đồng ý chinh sua linh kiện:
function ApplyEditComponent() {
    $(document).delegate("#button_Apply_EditComponent", "click", (e) => {
        if ($("#EditComponent_Form").valid()) {
            var link = $("#APIGateway").val();

            var userid = App.User.UserId;
            var componentId = $("#Edit_Components_ComponentId").val();

            var componentCode = $("#Edit_Components_ComponentCode").val();
            var componentName = $("#Edit_Components_ComponentName").val();
            var supplierCode = $("#Edit_Components_SupplierCode").val();
            var supplierName = $("#Edit_Components_SupplierName").val();
            var supplierShortName = $("#Edit_Components_ShortSupplierName").val();
            var positionCode = $("#Edit_Components_PositionCode").val();
            var inventoryNumber = $("#Edit_Components_InventoryQty").val();
            var minInventoryNumber = $("#Edit_Components_InventoryMinQty").val();
            var maxInventoryNumber = $("#Edit_Components_InventoryMaxQty").val();

            var componentInfo = $("#Edit_Components_ComponentInfo").val();
            var note = $("#Edit_Components_Note").val();

            var formData = {
                ComponentCode: componentCode,
                ComponentName: componentName,
                SupplierCode: supplierCode,
                SupplierName: supplierName,
                SupplierShortName: supplierShortName,
                PositionCode: positionCode,
                InventoryNumber: parseFloat(inventoryNumber.replace(/,/g, '')),
                MinInventoryNumber: parseFloat(minInventoryNumber.replace(/,/g, '')),
                MaxInventoryNumber: parseFloat(maxInventoryNumber.replace(/,/g, '')),
                ComponentInfo: componentInfo,
                Note: note,
                userId: userid,
                componentId: componentId
            };

            $.ajax({
                type: 'PUT',
                url: link + '/api/storage/component/update',
                data: formData,
                dataType: "json",
                encode: true,
                success: function (res) {
                    if (res.code == 200) {

                        componentDataTable.draw();

                        toastr.success(res.message)
                        $("#EditComponentModal").modal("hide");
                        ResetUpdateComponentModal();

                        //Reload:
                        //setTimeout(() => {
                        //    location.reload();
                        //}, 1500)
                    }
                    else if (res.code == 56) {
                        toastr.error(res.message)
                    }
                    else if (res.code == 59) {
                        toastr.error(res.message)
                    }
                    else if (res.code == 63) {
                        toastr.error(res.message)
                    }
                },
                error: function (error) {
                    if (error.responseJSON.code === 59) {
                        $("#Edit_Components_PositionCode-error").text(error.responseJSON.message)
                    } else {
                        $("#Edit_Components_PositionCode-error").text('')
                    }

                    if (error.responseJSON.code === 61) {
                        $("#Edit_Components_InventoryMinQty-error").text(error.responseJSON.message)
                    } else {
                        $("#Edit_Components_InventoryMinQty-error").text('')
                    }

                    if (error.responseJSON.code === 62) {
                        $("#Edit_Components_InventoryQty-error").text(error.responseJSON.message)
                    } else {
                        $("#Edit_Components_InventoryQty-error").text('')
                    }
                    if (error.responseJSON.code === 400) {
                        toastr.error(error.responseJSON.message)
                    }
                }
            });
        }
    })
}

//Ham chi cho nhap so:
function componentIsValidNumber(str) {
    // Loại bỏ ký tự có dấu tiếng Việt
    str = str.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
    return !/[~`!@#$%\^&*()+=\-\[\]\\';.,/{}|\\":<>\?_a-zA-Z ]/g.test(str);
}


//Hàm hiển thị số thập phân:
function convertDouble(str) {
    // Chia chuỗi thành 2 phần: phần nguyên và phần thập phân
    var parts = str.toString().split('.');
    var integerPart = parts[0];
    var decimalPart = parts.length > 1 ? parts[1] : '';

    // Định dạng phần nguyên bằng cách chèn dấu ',' sau mỗi 3 số
    var formattedIntegerPart = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, ',');

    // Tạo chuỗi kết quả
    var formattedString;
    if (parts.length >= 2) {
        formattedString = formattedIntegerPart + '.' + decimalPart;
    } else {
        formattedString = formattedIntegerPart;
    }
    
    return formattedString;
}

//Mở modal Thong tin linh kien:
function ListComponentsViewDetail() {
    $(document).delegate("a.ListComponent_ViewDetail_Controls", "click", (e) => {
        e.preventDefault();

        var link = $("#APIGateway").val();

        let target = e.target;
        var id = $(target).closest(".ListComponent_ViewDetail_Controls").data("id");
        $.ajax({
            type: "GET",
            url: link + `/api/storage/component/${id}`,
            dataType: "json",
            success: function (res) {
                if (res.code == 200) {
                    $(".Code_Content_Top_ListComponentViewDetail").text(res?.data?.componentCode)
                    $(".Name_Content_Top_ListComponentViewDetail").text(res?.data?.componentName)
                    $(".SuppilerCode_Content_Top_ListComponentViewDetail").text(res?.data?.supplierCode)
                    $(".SuppilerName_Content_Top_ListComponentViewDetail").text(res?.data?.supplierName)
                    $(".SuppilerShortName_Content_Top_ListComponentViewDetail").text(res?.data?.supplierShortName)
                    $(".PositionCode_Content_Bottom_ListComponentViewDetail").text(res?.data?.positionCode)
                    $(".InventoryQtyMin_Content_Bottom_ListComponentViewDetail").text(convertDouble(res?.data?.minInventoryNumber))
                    $(".InventoryQtyMax_Content_Bottom_ListComponentViewDetail").text(convertDouble(res?.data?.maxInventoryNumber))
                    $(".InventoryQty_Content_Bottom_ListComponentViewDetail").text(convertDouble(res?.data?.inventoryNumber))
                    $(".ComponentInfo_Content_Bottom_ListComponentViewDetail").text(res?.data?.componentInfo)
                    $(".ComponentNote_Content_Bottom_ListComponentViewDetail").text(res?.data?.note)
                    $(".CreatedBy_Content_Bottom_ListComponentViewDetail").text(res?.data?.createdName)
                    $(".CreatedAt_Content_Bottom_ListComponentViewDetail").text(res?.data?.createdAt)
                    $(".UpdatedBy_Content_Bottom_ListComponentViewDetail").text(res?.data?.updatedName)
                    $(".UpdatedAt_Content_Bottom_ListComponentViewDetail").text(res?.data?.updatedAt)

                    $("#ListComponent_ViewDetailUserModal").modal("show");
                    //toastr.success(res.message);

                }
                else if (res.code == 404) {
                    $(".Code_Content_Top_ListComponentViewDetail").text('')
                    $(".Name_Content_Top_ListComponentViewDetail").text('')
                    $(".SuppilerCode_Content_Top_ListComponentViewDetail").text('')
                    $(".SuppilerName_Content_Top_ListComponentViewDetail").text('')
                    $(".SuppilerShortName_Content_Top_ListComponentViewDetail").text('')
                    $(".PositionCode_Content_Bottom_ListComponentViewDetail").text('')
                    $(".InventoryQtyMin_Content_Bottom_ListComponentViewDetail").text('')
                    $(".InventoryQtyMax_Content_Bottom_ListComponentViewDetail").text('')
                    $(".InventoryQty_Content_Bottom_ListComponentViewDetail").text('')
                    $(".ComponentInfo_Content_Bottom_ListComponentViewDetail").text('')
                    $(".ComponentNote_Content_Bottom_ListComponentViewDetail").text('')
                    $(".CreatedBy_Content_Bottom_ListComponentViewDetail").text('')
                    $(".CreatedAt_Content_Bottom_ListComponentViewDetail").text('')
                    $(".UpdatedBy_Content_Bottom_ListComponentViewDetail").text('')
                    $(".UpdatedAt_Content_Bottom_ListComponentViewDetail").text('')

                    $("#ListComponent_ViewDetailUserModal").modal("show");
                    toastr.error(res.message);
                }
            },
            error: function (error) {
                toastr.error("Không có thông tin linh kiện.");
            }
        });
    })
}

//Mở modal Thêm mới linh kien:
function AddNewComponentViewDetail() {
    $(document).delegate("#btnCreate_Component", "click", (e) => {
        $("#AddNewComponentModal").modal("show")
    })
}

//Ham chan cac ky tu dac biet:
function componentIsValidText(str) {
    return !/[~`!@#$%\^&*()+=\-\[\]\\';.,/{}|\\":<>\?_]/g.test(str);
}

//Ham chi cho nhap so, chữ ko dấu, "-", "/":
function isValidSpecialCheck(str) {
    return !/[~`!@#$%\^&*()+=[\]\\';.,{}|\\":<>\?_]/g.test(str);
}

//Function Validate Thêm mới linh kiện:
function Validation_AddComponent() {
    // Thêm một quy tắc kiểm tra tùy chỉnh cho số 0 ở đầu
    $.validator.addMethod("notZeroAtStart", function (value, element) {
        return !/^0+$/.test(value);
    }, "Số lượng phải lớn hơn 0");

    jQuery.validator.addMethod("maxQuantityCharacter", function (value, element) {
        let maxChar = 8;
        let valid = true;

        let convertedValue = value.replaceAll(",", "");
        let length = convertedValue.length

        if (length > maxChar) {
            valid = false;
        }
        return valid;
    }, 'Số lượng nhập tối đa 8 ký tự.');


    jQuery.validator.addMethod("minQuantity", function (value, element) {
        let valid = true;
        let convertNumber = parseFloat(value);
        if (convertNumber < 0) {
            valid = false;
        }
        return valid;
    }, 'Vui lòng nhập số lượng >= 0.');

    //Them linh kien:
    $("#AddNewComponent_Form").validate({
        rules: {
            Add_Components_ComponentCode: {
                required: true,
                minlength: 9,
                maxlength: 9,
            },
            Add_Components_ComponentName: {
                required: true,
                minlength: 1,
                maxlength: 150,
            },
            Add_Components_SupplierCode: {
                required: true,
                minlength: 1,
                maxlength: 50,
            },
            Add_Components_SupplierName: {
                required: true,
                minlength: 1,
                maxlength: 250,
            },
            Add_Components_ShortSupplierName: {
                required: true,
                minlength: 1,
                maxlength: 250,
            },
            Add_Components_PositionCode: {
                required: true,
                minlength: 1,
                maxlength: 20,
            },
            Add_Components_InventoryMinQty: {
                required: true,
                minlength: 1,
                maxQuantityCharacter: true,
                minQuantity: true
                // maxlength: 10,
                //notZeroAtStart: true
            },
            Add_Components_InventoryMaxQty: {
                required: true,
                minlength: 1,
                maxQuantityCharacter: true,
                minQuantity: true
                //maxlength: 9,
                //notZeroAtStart: true
            }
            ,
            Add_Components_InventoryQty: {
                required: true,
                minlength: 1,
                maxQuantityCharacter: true,
                minQuantity: true
                //maxlength: 9,
                //notZeroAtStart: true
            }
            ,
            Add_Components_ComponentInfo: {
                //required: true,
                minlength: 1,
                maxlength: 250,
            }
            ,
            Add_Components_Note: {
                //required: true,
                minlength: 1,
                maxlength: 50,
            }
        },
        messages: {
            Add_Components_ComponentCode: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập mã linh kiện."],
                minlength: window.languageData[window.currentLanguage]["Vui lòng nhập đầy đủ 9 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Vui lòng nhập đầy đủ 9 ký tự."],
            },
            Add_Components_ComponentName: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập tên linh kiện."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 150 ký tự."],
            },
            Add_Components_SupplierCode: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập mã nhà cung cấp."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 50 ký tự."],
            },
            Add_Components_SupplierName: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập tên nhà cung cấp."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 250 ký tự."],
            },
            Add_Components_ShortSupplierName: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập tên nhà cung cấp rút gọn."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 250 ký tự."],
            },
            Add_Components_PositionCode: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập vị trí cố định."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 20 ký tự."],
            },
            Add_Components_InventoryMinQty: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập số lượng tồn kho nhỏ nhất."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                //maxlength: "Tối đa 9 ký tự",
                //notZeroAtStart: "Số lượng phải lớn hơn 0"
            },
            Add_Components_InventoryMaxQty: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập số lượng tồn kho lớn nhất."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                //maxlength: "Tối đa 9 ký tự",
                //notZeroAtStart: "Số lượng phải lớn hơn 0"
            },
            Add_Components_InventoryQty: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập số lượng tồn kho thực tế."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                //maxlength: "Tối đa 9 ký tự",
                //notZeroAtStart: "Số lượng phải lớn hơn 0"
            },
            Add_Components_ComponentInfo: {
                //required: "Vui lòng nhập thông tin linh kiện",
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 250 ký tự."],
            },
            Add_Components_Note: {
                //required: "Vui lòng nhập ghi chú",
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 50 ký tự."],
            }
        }
    });

    //Sua linh kien:
    $("#EditComponent_Form").validate({
        rules: {
            Edit_Components_ComponentCode: {
                required: true,
                minlength: 9,
                maxlength: 9,
            },
            Edit_Components_ComponentName: {
                required: true,
                minlength: 1,
                maxlength: 150,
            },
            Edit_Components_SupplierCode: {
                required: true,
                minlength: 1,
                maxlength: 50,
            },
            Edit_Components_SupplierName: {
                required: true,
                minlength: 1,
                maxlength: 250,
            },
            Edit_Components_ShortSupplierName: {
                required: true,
                minlength: 1,
                maxlength: 250,
            },
            Edit_Components_PositionCode: {
                required: true,
                minlength: 1,
                maxlength: 20,
            },
            Edit_Components_InventoryMinQty: {
                required: true,
                minlength: 1,
                maxQuantityCharacter: true,
                minQuantity: true
                //maxlength: 10,
                //notZeroAtStart: true
            },
            Edit_Components_InventoryMaxQty: {
                required: true,
                minlength: 1,
                maxQuantityCharacter: true,
                minQuantity: true
                //maxlength: 10,
                //notZeroAtStart: true
            }
            ,
            Edit_Components_InventoryQty: {
                required: true,
                minlength: 1,
                maxQuantityCharacter: true,
                minQuantity: true
                //maxlength: 10,
                //notZeroAtStart: true
            }
            ,
            Edit_Components_ComponentInfo: {
                //required: true,
                minlength: 1,
                maxlength: 250,
            }
            ,
            Edit_Components_Note: {
                //required: true,
                minlength: 1,
                maxlength: 50,
            }
        },
        messages: {
            Edit_Components_ComponentCode: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập mã linh kiện."],
                minlength: window.languageData[window.currentLanguage]["Vui lòng nhập đầy đủ 9 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Vui lòng nhập đầy đủ 9 ký tự."],
            },
            Edit_Components_ComponentName: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập tên linh kiện."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 150 ký tự."],
            },
            Edit_Components_SupplierCode: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập mã nhà cung cấp."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 50 ký tự."],
            },
            Edit_Components_SupplierName: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập tên nhà cung cấp."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 250 ký tự."],
            },
            Edit_Components_ShortSupplierName: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập tên nhà cung cấp rút gọn."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 250 ký tự."],
            },
            Edit_Components_PositionCode: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập vị trí cố định."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 20 ký tự."],
            },
            Edit_Components_InventoryMinQty: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập số lượng tồn kho nhỏ nhất."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                //maxlength: "Tối đa 10 ký tự",
                //notZeroAtStart: "Số lượng phải lớn hơn 0"
            },
            Edit_Components_InventoryMaxQty: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập số lượng tồn kho lớn nhất."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                //maxlength: "Tối đa 10 ký tự",
                //notZeroAtStart: "Số lượng phải lớn hơn 0"
            },
            Edit_Components_InventoryQty: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập số lượng tồn kho thực tế."],
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                //maxlength: "Tối đa 10 ký tự",
                //notZeroAtStart: "Số lượng phải lớn hơn 0"
            },
            Edit_Components_ComponentInfo: {
                //required: "Vui lòng nhập thông tin linh kiện",
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 250 ký tự."],
            },
            Edit_Components_Note: {
                //required: "Vui lòng nhập ghi chú",
                minlength: window.languageData[window.currentLanguage]["Ít nhất 1 ký tự."],
                maxlength: window.languageData[window.currentLanguage]["Tối đa 50 ký tự."],
            }
        }
    });

    //Check khong cho nhap so luong > 0 , luc filter linh kien:

    //$("#ListComponent_Search_Option_Form").validate({
    //    rules: {
    //        Component_Inventory_Qty_Start: {
    //            notZeroAtStart: true
    //        },
    //        Component_Inventory_Qty_End: {
    //            notZeroAtStart: true
    //        }
    //    },
    //    messages: {
    //        Component_Inventory_Qty_Start: {
    //            notZeroAtStart: "Số lượng phải lớn hơn 0"
    //        },
    //        Component_Inventory_Qty_End: {
    //            notZeroAtStart: "Số lượng phải lớn hơn 0"
    //        }
    //    }
    //});
}

//Hàm chỉnh sửa linh kiện:
function EditComponent() {
    $(document).delegate(".EditListComponent_Controls", "click", (e) => {
        let target = e.target;
        var id = $(target).closest(".EditListComponent_Controls").data("id");
        FillDataIntoEditComponentModal(id)
        $("#EditComponentModal").modal("show")
    })
}

//Hàm filter linh kiện:
function GetFilteredComponents() {
    $(document).delegate("#btn-search", "click", (e) => {
        //GetComponents()

        componentDataTable.draw();
    })
}

function ResetSearch() {
    $(document).delegate("#btn-reset", "click", (e) => {
        $("#Component_code").val("")
        $("#Component_Name").val("")
        $("#Supplier_Name").val("")

        $('#InventoryStatus').val(0).change();

        $("#Component_Layout")[0].reset();
        $("#Component_Layout")[0].toggleSelectAll(true);

        $("#Component_Position").val("")
        $("#Component_Inventory_Qty_Start").val("")
        $("#Component_Inventory_Qty_End").val("")
        $('#Component_Inventory_Qty_Start_span-error').text('');
        $('#Component_Inventory_Qty_End_span-error').text('');
        //GetComponents()

        componentDataTable.draw();
    })
}

//Hàm fill dữ liệu vào modal sửa linh kiện:
function FillDataIntoEditComponentModal(id) {
    var link = $("#APIGateway").val();

    //let target = e.target;

    $.ajax({
        type: "GET",
        url: link + `/api/storage/component/${id}`,
        dataType: "json",
        success: function (res) {
            if (res.code == 200) {
                $("#Edit_Components_ComponentId").val(res?.data?.id)
                $("#Edit_Components_ComponentCode").val(res?.data?.componentCode)
                $("#Edit_Components_ComponentName").val(res?.data?.componentName)
                $("#Edit_Components_SupplierCode").val(res?.data?.supplierCode)
                $("#Edit_Components_SupplierName").val(res?.data?.supplierName)
                $("#Edit_Components_ShortSupplierName").val(res?.data?.supplierShortName)
                $("#Edit_Components_PositionCode").val(res?.data?.positionCode)
                $("#Edit_Components_InventoryMinQty").val(convertDouble(res?.data?.minInventoryNumber))
                $("#Edit_Components_InventoryMaxQty").val(convertDouble(res?.data?.maxInventoryNumber))
                $("#Edit_Components_InventoryQty").val(convertDouble(res?.data?.inventoryNumber))
                $("#Edit_Components_ComponentInfo").val(res?.data?.componentInfo)
                $("#Edit_Components_Note").val(res?.data?.note)

                InputDoubleUpdateComponent();
            }
            else if (res.code == 404) {
                $("#Edit_Components_ComponentId").val('')
                $("#Edit_Components_ComponentCode").val('')
                $("#Edit_Components_ComponentName").val('')
                $("#Edit_Components_SupplierCode").val('')
                $("#Edit_Components_SupplierName").val('')
                $("#Edit_Components_ShortSupplierName").val('')
                $("#Edit_Components_PositionCode").val('')
                $("#Edit_Components_InventoryMinQty").val('')
                $("#Edit_Components_InventoryMaxQty").val('')
                $("#Edit_Components_InventoryQty").val('')
                $("#Edit_Components_ComponentInfo").val('')
                $("#Edit_Components_Note").val('')
            }
        },
        error: function (error) {
            //toastr.error("Không có thông tin linh kiện");
        }
    });

}

//Hàm checked all để xóa tất cả linh kiện trong 1 trang:
function CheckedAllDeteleComponent() {
    $(document).delegate("#Component_CheckAll", "change", (e) => {
        var isChecked = $(e.target).is(":checked");
        $(".Component_check").prop("checked", isChecked);

        var atLeastOneChecked = $(".Component_check:checked").length > 0;
        // Cập nhật trạng thái của nút "ListComponent_Delete"
        if (atLeastOneChecked) {
            $(".ListComponent_Delete").prop("disabled", !atLeastOneChecked);
            $(".ListComponent_Delete_Images img").attr("src", "./assets/images/icons/Delete_Red.svg")
            $(".ListComponent_Delete_Title").addClass("color-red")
        }
        else {
            $(".ListComponent_Delete").prop("disabled", atLeastOneChecked);
            $(".ListComponent_Delete_Images img").attr("src", "./assets/images/icons/Delete_Grey.svg")
            $(".ListComponent_Delete_Title").removeClass("color-red")
        }
    })
}

//Bật tắt nút xóa linh kiện:
function ShowAndHideButtonDeleteComponent() {
    $(document).delegate(".Component_check", "change", (e) => {
        var atLeastOneChecked = $(".Component_check:checked").length > 0;
        // Cập nhật trạng thái của nút "ListComponent_Delete"
        if (atLeastOneChecked) {
            $(".ListComponent_Delete").prop("disabled", !atLeastOneChecked);
            $(".ListComponent_Delete_Images img").attr("src", "./assets/images/icons/Delete_Red.svg")
            $(".ListComponent_Delete_Title").addClass("color-red")

            if ($(".Component_check:checked").length == $(".Component_check").length) {
                $('#Component_CheckAll').prop('checked', true);
            } else {
                $('#Component_CheckAll').prop('checked', false);
            }
        }
        else {
            $(".ListComponent_Delete").prop("disabled", atLeastOneChecked);
            $(".ListComponent_Delete_Images img").attr("src", "./assets/images/icons/Delete_Grey.svg")
            $(".ListComponent_Delete_Title").removeClass("color-red")
        }
    })
}

function ClickButtonDeleteComponent() {
    $(document).delegate(".ListComponent_Delete", "click", function () {
        Swal.fire({
            title: `<b>${window.languageData[window.currentLanguage]["Xóa linh kiện"]}</b>`,
            text: window.languageData[window.currentLanguage]["Bạn có chắc chắn muốn xóa dữ liệu?"],
            confirmButtonText: window.languageData[window.currentLanguage]['Đồng ý'],
            showCancelButton: true,
            showLoaderOnConfirm: true,
            cancelButtonText: window.languageData[window.currentLanguage]['Hủy bỏ'],
            reverseButtons: true,
            customClass: {
                actions: "swal_confirm_actions"
            }
        }).then((result, e) => {
            if (result.isConfirmed) {
                //lấy danh sách Id linh kiện muốn xóa:
                var selectedIds = [];

                $(".Component_check:checked").each(function () {
                    selectedIds.push($(this).data("id"));
                });

                //Call API Xóa Linh kiện:
                var link = $("#APIGateway").val();

                var formData = {
                    ids: selectedIds
                };

                $.ajax({
                    type: 'DELETE',
                    url: link + '/api/storage/component/delete',
                    data: formData,
                    dataType: "json",
                    encode: true,
                    success: function (res) {
                        if (res.code == 200) {
                            toastr.success(window.languageData[window.currentLanguage][res.message])

                            //Reload:
                            setTimeout(() => {
                                location.reload();
                            }, 1500)

                        }
                    },
                    error: function (error) {
                        if (error.responseJSON.code) {
                            toastr.error(error.responseJSON.message)
                        }
                    }
                });
            }
        })
    });
}

//Export List Components:
function ExportComponentList() {
    $(document).delegate("button.ListComponent_ExportExcel", "click", (e) => {
        var componentCode = $("#Component_code").val();
        var componentName = $("#Component_Name").val();
        var supplierName = $("#Supplier_Name").val();
        var componentPosition = $("#Component_Position").val();
        var componentInventoryQtyStart = $("#Component_Inventory_Qty_Start").val() == "" ? null : $("#Component_Inventory_Qty_Start").val().replace(',', '');
        var componentInventoryQtyEnd = $("#Component_Inventory_Qty_End").val() == "" ? null : $("#Component_Inventory_Qty_End").val().replace(',', '');
        var inventoryStatus = $('#InventoryStatus').val();
        //Check selected option Layout:
        var checkAllLayouts = document.querySelector('#Component_Layout').isAllSelected();
        var allLayouts;
        var layoutIds;
        if (checkAllLayouts) {
            allLayouts = -1;
        }
        else {
            layoutIds = $('#Component_Layout').val();
        }

        var filterData = {
            ComponentCode: componentCode,
            ComponentName: componentName,
            SupplierName: supplierName,
            ComponentPosition: componentPosition,
            ComponentInventoryQtyStart: componentInventoryQtyStart,
            ComponentInventoryQtyEnd: componentInventoryQtyEnd,
            AllLayouts: allLayouts,
            LayoutIds: layoutIds,
            InventoryStatus: inventoryStatus
        };

        $.ajax({
            type: 'POST',
            url: '/export/component/list',
            data: filterData,
            cache: false,
            xhrFields: {
                responseType: 'blob'
            },
            success: function (response) {
                if (response) {
                    // Tạo một liên kết ẩn với dữ liệu tải xuống
                    var blob = new Blob([response], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                    var url = window.URL.createObjectURL(blob);
                    var a = document.createElement('a');
                    a.href = url;
                    a.download = 'DanhSachLinhKien.xlsx';

                    // Kích hoạt sự kiện nhấp chuột trên liên kết để tải xuống
                    a.click();
                    window.URL.revokeObjectURL(url);

                    toastr.success(window.languageData[window.currentLanguage]["Xuất excel thành công."]);
                }
            },
            error: function () {
                toastr.error(window.languageData[window.currentLanguage]['Xảy ra lỗi khi xuất Excel.']);
            }
        });
    })
}



function ImportComponentList() {
    $(document).delegate("#btn_import_input_component", "click", (e) => {
        $("#inputComponentsFileExcel").val("");
        $("#inputComponentsFileExcel").trigger("click");
    });

    $("#inputComponentsFileExcel").change(function (e) {
        let target = e.target;
        let file = target.files[0];
        //let userId = App.User.UserId;
        if (file) {
            $('#frmComponentsImportExcel').validate({
                errorClass: 'red',
                ignore: [],
                lang: 'vi',
                rules: {
                    inputComponentsFileExcel: {
                        onlyExcels: true,
                    },
                },
                messages: {
                    inputComponentsFileExcel: {
                        onlyExcels: "Vui lòng chọn file Excel hợp lệ thuộc loại .xlsx, .xls, .csv"
                    },
                },
                errorPlacement: function (error, element) {
                    var response = JSON.parse(error.responseText);
                    toastr.error(response);
                    var response = JSON.parse(error.responseText);
                },
            });

            if ($('#frmComponentsImportExcel').valid()) {
                $("#overlay").css("display", "block");
                let url = 'import-component-list';
                var formData = new FormData();
                formData.append("file", file);
                $.ajax({
                    type: 'POST',
                    url: url,
                    data: formData,
                    processData: false, // Prevent jQuery from transforming the data
                    contentType: false, // Let jQuery set the content type
                    //xhrFields: {
                    //    responseType: 'blob'
                    //},
                    success: function (response) {
                        $("#overlay").css("display", "none");
                        if (response != undefined && response.code == 200) {
                            var successCount = response.data.successCount;
                            var failCount = response.data.failCount;
                            var successMessage = `${window.languageData[window.currentLanguage]["Import thành công"]} ` + successCount + ` ${window.languageData[window.currentLanguage]["dòng dữ liệu và có"]} ` + failCount + ` ${window.languageData[window.currentLanguage]["dòng dữ liệu lỗi không thể thực hiện import. Vui lòng ấn Đồng ý để xem dữ liệu lỗi."]}`;
                            if (response.data != undefined) {

                                componentDataTable.draw();

                                Swal.fire({
                                    title: window.languageData[window.currentLanguage]['Thông báo'],
                                    text: successMessage,
                                    confirmButtonText: window.languageData[window.currentLanguage]['Đồng ý']
                                }).then((result, e) => {
                                    if (result.isConfirmed) {
                                        var a = $("<a style='display: none;' id='downloadComponentsImportFileErrorExcel' />");
                                        a.attr("href", response.data.fileUrl);
                                        a.attr("download", response.data.fileName);
                                        $("body").append(a);
                                        a[0].click();

                                        //Clear temp data
                                        window.URL.revokeObjectURL(response.data.fileUrl);
                                        a.remove();
                                    }
                                })
                            }
                            else {

                                Swal.fire({
                                    title: window.languageData[window.currentLanguage]['Thông báo'],
                                    text: successMessage,
                                    confirmButtonText: window.languageData[window.currentLanguage]['Đồng ý']
                                })
                            }
                        }
                        else {
                            Swal.fire({
                                title: window.languageData[window.currentLanguage]['Thông báo'],
                                text: window.languageData[window.currentLanguage][response.message],
                                confirmButtonText: window.languageData[window.currentLanguage]['Đồng ý']
                            })
                        }
                    },
                    error: function (error) {
                        $("#overlay").css("display", "none");
                        if (error != undefined) {
                            var response = JSON.parse(error.responseText);
                            Swal.fire({
                                title: window.languageData[window.currentLanguage]['Lỗi Import linh kiện'],
                                text: window.languageData[window.currentLanguage][response.message],
                                confirmButtonText: window.languageData[window.currentLanguage]['Đồng ý']
                            })
                        }
                    }
                });
            }
        }
    })
}          

function DownloadImportComponentTemplate() {
    $("#btnDownloadImportComopnentTemplate").click(async (e) => {
        let fileKey = "TemplateImportComponentList";
        $.ajax({
            type: 'GET',
            url: `/file/template/${fileKey}`,
            xhrFields: {
                responseType: 'blob'
            },
            success: function (response) {
                if (response) {
                    // Tạo một liên kết ẩn với dữ liệu tải xuống
                    var blob = new Blob([response], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                    var url = window.URL.createObjectURL(blob);
                    var a = document.createElement('a');
                    a.href = url;
                    a.download = 'Mẫu import linh kiện.xlsx';

                    // Kích hoạt sự kiện nhấp chuột trên liên kết để tải xuống
                    a.click();
                    window.URL.revokeObjectURL(url);
                }
            },
            error: function () {
                toastr.error('Xảy ra lỗi khi tải biểu mẫu.');
            }
        });
    })
}