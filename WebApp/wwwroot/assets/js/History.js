$(function () {
    HistoryController.init();

   
})

var HistoryController = (function () {
    let root = {
        parent: $(".Views_Storage_InOutHistory")
    }
    let datatable;
    let dataFilter = {

    }

    let dataFilterExport = {


    }

    function DetailAPI(userId, historyId) {
        return new Promise(async (resolve, reject) => {
            let url = `${App.ApiGateWayUrl}/api/storage/histories/${historyId}/${userId}`

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

    //Export Excel:
    function ExportExcelHistoryInOutStorage(dataFilterExport) {
        return new Promise(async (resolve, reject) => {
            let url = `/export/history`
            try {
                const res = await $.ajax({
                    url: url,
                    type: 'POST',
                    data: dataFilterExport,
                    xhrFields: {
                        responseType: 'blob'
                    },
                });
                resolve(res)
            } catch (err) {
                reject(err)
            }
        })
    }


    function Cache() {
        root.$historyTable = $(root.parent).find("#history_table")

        //Form 
        root.$filterForm = $(root.parent).find("#history_search_form")

        //Detail modal
        root.$detailModal = $(root.parent).find("#Views_Shared_Common__HistoryDetailModal")

        //Search Inputs
        root.$input_actor = root.parent.find("#input_actor")
        root.$input_date_from = $(root.parent).find("#input_date_from")
        root.$input_date_to = $(root.parent).find("#input_date_to")
        root.$input_componentcode = $(root.parent).find("#input_componentcode")
        root.$input_positioncode = $(root.parent).find("#input_positioncode")

        root.$input_quantity_range_from = $(root.parent).find("#input_quantity_range_from")
        root.$input_quantity_range_to = $(root.parent).find("#input_quantity_range_to")

        //Search select
        root.$select_activity_type = $(root.parent).find("#select_activity_type")
        root.$select_factories = $(root.parent).find("#select_factories")
        root.$select_areas = $(root.parent).find("#select_areas")
        root.$select_departments = $(root.parent).find("#select_departments")

        //Buttons
        root.$btnReset = root.parent.find("#btn-reset")
        root.$btnSearch = root.parent.find("#btn-search")

        //Display detail

        root.detail_actor = root.$detailModal.find("#detail_actor") 
        root.detail_activity_date = root.$detailModal.find("#detail_activity_date") 
        root.detail_department_name = root.$detailModal.find("#detail_department_name") 
        root.detail_business = root.$detailModal.find("#detail_business") 
        root.detail_component_code = root.$detailModal.find("#detail_component_code") 
        root.detail_component_name = root.$detailModal.find("#detail_component_name") 
        root.detail_supplier_code = root.$detailModal.find("#detail_supplier_code") 
        root.detail_supplier_name = root.$detailModal.find("#detail_supplier_name") 
        root.detail_supplier_short_name = root.$detailModal.find("#detail_supplier_short_name") 
        root.detail_position_code = root.$detailModal.find("#detail_position_code") 
        root.detail_quantity = root.$detailModal.find("#detail_quantity") 
        root.detail_inventory_number = root.$detailModal.find("#detail_inventory_number") 
        root.detail_note = root.$detailModal.find("#detail_note") 
    }


    function Events() {
        $("#input_date_from,#input_date_to").keydown(function (e) {
            e.preventDefault(); // Chặn nhập chữ và số
        });

        root.parent.delegate(".calendar_icon", "click", function (e) {
            let target = e.target;
            let thisButton = $(target).closest(".calendar_icon");

            $(thisButton).prevAll("input").datepicker("show");
        })

        //Reset search form
        root.$btnReset.click(ValidateInputHelper.Utils.debounce(function (e) {
            root.$input_actor.val("");

            root.$input_quantity_range_from.val("");
            root.$input_quantity_range_to.val("");

            root.$input_componentcode.val("");
            root.$input_positioncode.val("");

            root.$input_date_from.val("").datepicker('update');
            root.$input_date_to.val("").datepicker('update');

            $("#input_date_from-error").hide();
            $("#input_date_to-error").hide();
            $("#input_quantity_range_from-error").hide();
            $("#input_quantity_range_to-error").hide();

            //root.$select_activity_type.find("option").attr("selected", true);
            //root.$select_factories.find("option").attr("selected", true);
            //root.$select_areas.find("option").attr("selected", true);
            //root.$select_departments.find("option").attr("selected", true);

            root.$select_activity_type[0].reset();
            root.$select_factories[0].reset();
            root.$select_areas[0].reset();
            root.$select_departments[0].reset();

            root.$select_activity_type[0].toggleSelectAll(true);
            root.$select_factories[0].toggleSelectAll(true);
            root.$select_areas[0].toggleSelectAll(true);
            root.$select_departments[0].toggleSelectAll(true);


            datatable.draw();
        }, 200))

        //Search form
        root.$btnSearch.click(ValidateInputHelper.Utils.debounce(function (e) {
            let validForm = root.$filterForm.valid();
            if (validForm) {
                datatable.draw();
            }
        }, 200))

        //Detail
        root.$historyTable.delegate(".history_detail", "click", function (e) {
            let thisButton = $(this).closest(".history_detail");

            let userId = App.User.UserId;
            let historyId = $(thisButton).attr("data-id");

            loading(true);
            DetailAPI(userId, historyId).then((res) => {
                if (res.data) {
                    let detailModel = res.data;

                    root.detail_actor.text(detailModel.userName)

                    let convertedDate = moment(detailModel.createDate).format("DD/MM/YYYY HH:mm")
                    root.detail_activity_date.text(convertedDate)
                    root.detail_department_name.text(detailModel.departmentName)

                    let typeText = detailModel.type == 0 ? `<span class="history_type_input">Nhập kho</span>` : `<span class="history_type_input">Xuất kho</span>`;
                    root.detail_business.html(typeText)
                    root.detail_component_code.text(detailModel.componentCode)
                    root.detail_component_name.text(detailModel.componentName)
                    root.detail_supplier_code.text(detailModel.supplierCode)
                    root.detail_supplier_name.text(detailModel.supplierName)
                    root.detail_supplier_short_name.text(detailModel.supplierShortName)
                    root.detail_position_code.text(detailModel.positionCode)

                    let convertedQuantity = ValidateInputHelper.Utils.convertDecimalInventory(detailModel.quantity)
                    root.detail_quantity.text(convertedQuantity)

                    let convertedIventoryNumber = ValidateInputHelper.Utils.convertDecimalInventory(detailModel.inventoryNumber)
                    root.detail_inventory_number.text(convertedIventoryNumber)
                    root.detail_note.text(detailModel.note)

                    loading(false);
                    $(root.$detailModal).modal("show");
                }
            }).catch((err) => {
                toastr.error(err?.responseJSON?.message);
            })
        })

        root.$input_quantity_range_from.maskMoney({
            allowZero: true,
            defaultZero: false,
            allowEmpty: true,
            allowNegative: false,
            precision: 0,
            selectAllOnFocus: false,
            bringCaretAtEndOnFocus: false
        });
        root.$input_quantity_range_from.on("keypress", ValidateInputHelper.OnlyNumerOnKeyPress);
        root.$input_quantity_range_from.on("keypress", ValidateInputHelper.LimitInputLengthOnKeyPress(8));
        root.$input_quantity_range_from.on("keyup", ValidateInputHelper.LimitInputLengthOnKeyPress(8));
        //root.$input_quantity_range_from.on("keyup", ValidateInputHelper.Utils.RemoveVietnameseOnChange);
        //root.$input_quantity_range_from.on("keyup", ValidateInputHelper.Utils.NumberThousands);



        root.$input_quantity_range_to.maskMoney({
            allowZero: true,
            defaultZero: false,
            allowEmpty: true,
            allowNegative: false,
            precision: 0,
            selectAllOnFocus: false,
            bringCaretAtEndOnFocus: false
        });
        root.$input_quantity_range_to.on("keypress", ValidateInputHelper.OnlyNumerOnKeyPress);
        root.$input_quantity_range_to.on("keypress", ValidateInputHelper.LimitInputLengthOnKeyPress(8));
        root.$input_quantity_range_to.on("keyup", ValidateInputHelper.LimitInputLengthOnKeyPress(8));
        //root.$input_quantity_range_to.on("keyup", ValidateInputHelper.Utils.RemoveVietnameseOnChange);
        //root.$input_quantity_range_to.on("keyup", ValidateInputHelper.Utils.NumberThousands);

        //Enter to search
        root.$filterForm.on("keypress", ValidateInputHelper.FormEnter(function (e) {
            let validForm = root.$filterForm.valid();
            if (validForm) {
               datatable.draw();
            }
        }))

        //Export Excel:
        $(document).delegate("#export_file", "click", (e) => {
            dataFilterExport.UserId = App.User.UserId;

            let selectType = root.parent.find("#select_activity_type")
            let selectFactory = root.parent.find("#select_factories")
            let selectLayout = root.parent.find("#select_areas")
            let selectDepartment = root.parent.find("#select_departments")

            let userName = root.$input_actor.val();
            let componentCode = root.$input_componentcode.val()
            let positionCode = root.$input_positioncode.val()

            let quantityFrom = root.$input_quantity_range_from.val() == "" ? -1 : root.$input_quantity_range_from.val().replace(",", "");
            let quantityTo = root.$input_quantity_range_to.val() == "" ? -1 : root.$input_quantity_range_to.val().replace(",","");

            let dateFrom = root.$input_date_from.val() != "" ? root.$input_date_from.val().split("/").reverse().join("/") : "";
            let dateTo = root.$input_date_to.val() != "" ? root.$input_date_to.val().split("/").reverse().join("/") : "";

            let types;
            let isAllType;
            let factories;
            let isAllFactories;
            let layouts;
            let isAllLayouts;
            let departments;
            let isAllDepartments;

            if (selectType[0].isAllSelected()) {
                types = [],
                    isAllType = true
            } else {
                types = selectType.val(),
                    isAllType = false
            }

            if (selectFactory[0].isAllSelected()) {
                factories = [],
                    isAllFactories = true
            } else {
                factories = selectFactory.val(),
                    isAllFactories = false
            }

            if (selectLayout[0].isAllSelected()) {
                layouts = [],
                    isAllLayouts = true
            } else {
                layouts = selectLayout.val(),
                    isAllLayouts = false
            }

            if (selectDepartment[0].isAllSelected()) {
                departments = [],
                    isAllDepartments = true
            } else {
                departments = selectDepartment.val(),
                    isAllDepartments = false
            }
            dataFilterExport.userName = $.trim(userName)
            dataFilterExport.componentCode = $.trim(componentCode)
            dataFilterExport.positionCode = $.trim(positionCode)
            dataFilterExport.quantityFrom = quantityFrom
            dataFilterExport.quantityTo = quantityTo
            dataFilterExport.dateFrom = dateFrom
            dataFilterExport.dateTo = dateTo
            dataFilterExport.types = types
            dataFilterExport.factories = factories
            dataFilterExport.layouts = layouts
            dataFilterExport.departments = departments
            dataFilterExport.isAllType = isAllType
            dataFilterExport.isAllFactories = isAllFactories
            dataFilterExport.isAllLayouts = isAllLayouts
            dataFilterExport.isAllDepartments = isAllDepartments

            loading(true);
            ExportExcelHistoryInOutStorage(dataFilterExport).then((res) => {
                // Tạo một liên kết ẩn với dữ liệu tải xuống
                var blob = new Blob([res], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                var url = window.URL.createObjectURL(blob);
                var a = document.createElement('a');
                a.href = url;
                a.download = 'LichSuXuatNhapKho.xlsx';

                // Kích hoạt sự kiện nhấp chuột trên liên kết để tải xuống
                a.click();
                window.URL.revokeObjectURL(url);

                toastr.success(window.languageData[window.currentLanguage]["Xuất Excel thành công."]);
                loading(false)

            }).catch((err) => {
                loading(false)
                toastr.error(window.languageData[window.currentLanguage]['Xảy ra lỗi khi xuất Excel.']);
            })
        })

        $("#input_date_from, #input_date_to, #input_quantity_range_from, #input_quantity_range_to").change(function (e) {
            root.$filterForm.valid();
        });
    }

    function InitDatatable() {
        let host = App.ApiGateWayUrl;

        datatable = root.$historyTable.DataTable({
            "processing": `<span class="spinner"></span>`,
            scrollX: true,
            select: true,
            // scrollY: 500,
            fixedColumns: true,
            "serverSide": true,
            "paging": true,
            "pagingType": "full_numbers",
            "filter": true,
            "searching": false,
            responsive: true,
            "lengthMenu": [10, 30, 50, 200],
            dom: 'rt<"bottom"flp><"clear">',
            "ordering": false,
            "ajax": {
                "url": host + "/api/storage/histories",
                "type": "POST",
                "contentType": "application/x-www-form-urlencoded",
                dataType: "json",
                cache: false,
                data: function (data) {
                    dataFilter.UserId = App.User.UserId;

                    let selectType = root.parent.find("#select_activity_type")
                    let selectFactory = root.parent.find("#select_factories")
                    let selectLayout = root.parent.find("#select_areas")
                    let selectDepartment = root.parent.find("#select_departments")

                    let userName = root.$input_actor.val();
                    let componentCode = root.$input_componentcode.val()
                    let positionCode = root.$input_positioncode.val()

                    let quantityFrom = root.$input_quantity_range_from.val()
                    let quantityTo = root.$input_quantity_range_to.val()

                    let dateFrom = root.$input_date_from.val()
                    let dateTo = root.$input_date_to.val()

                    let types = selectType.val()
                    let factories = selectFactory.val()
                    let layouts = selectLayout[0].isAllSelected() ? "-1" : selectLayout.val()
                    let departments = selectDepartment[0].isAllSelected() ? "-1" : selectDepartment.val()

                    dataFilter.userName = userName
                    dataFilter.componentCode = componentCode
                    dataFilter.positionCode = positionCode
                    dataFilter.quantityFrom = quantityFrom.replace(",", "")
                    dataFilter.quantityTo = quantityTo.replace(",", "")
                    dataFilter.dateFrom = dateFrom
                    dataFilter.dateTo = dateTo
                    dataFilter.types = types
                    dataFilter.factories = factories
                    dataFilter.layouts = layouts
                    dataFilter.departments = departments 

                    Object.assign(data, dataFilter);
                    return data;
                },
                "dataSrc": function ({ data, recordsTotal, recordsFiltered }) {
                    datatable.page.info().recordsTotal = recordsTotal;
                    datatable.page.info().recordsDisplay = recordsFiltered;

                    return data;
                }
            },
            "drawCallback": function (settings) {
                let totalPages = datatable.page.info().pages;
                let totalRecords = datatable.page.info().recordsTotal;

                let currPage = datatable.page() + 1;
                if (currPage == 1) {
                    root.parent.find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                    root.parent.find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                }
                if (currPage == totalPages) {
                    root.parent.find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                    root.parent.find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                }

                root.parent.find(".pagination").before(`<div class="h5 datatable_total_records"> <b>${window.languageData[window.currentLanguage]['Tổng']}</b>: ${totalPages} <span class="total_record_seperate"> | </span>
                </div>`)
            },
            //initComplete: function (settings, data) {
            //    let tableId = settings.sTableId;
            //    let datatableLength = $(`#${tableId}_length`);

            //    let optionValues = settings.aLengthMenu;
            //    let length = settings._iDisplayLength;
            //    let resultHtml = optionValues.map((val, i) => {
            //        return `<option value="${val}">Hiển thị ${val}</option>`
            //    }).join('')

            //    let selectElement = datatableLength.find("select");
            //    selectElement.html(`
            //        ${resultHtml}
            //    `)
            //    selectElement.val(length).change();

            //    let label = datatableLength.contents().eq(0);
            //    $(label).contents().each((i, el) => {
            //        if ($(el).is("select") == false) {
            //            $(el).remove()
            //        }
            //    })
            //},
            //"columnDefs": [
            //    { "width": "50%", "targets": 0 },
            //],
            "columns": [
                {
                    "data": "",
                    "name": "STT",
                    "render": function (data, type, row, index) {
                        let pagesize = index.settings._iDisplayLength;
                        let currentRow = ++index.row;
                        let currentPage = datatable.page() + 1;

                        let STT = ((currentPage - 1) * pagesize) + currentRow;

                        if (STT < 10) {
                            STT = `0${STT}`;
                        }
                        return STT;
                    },
                    "autoWidth": true
                },
                { "data": "userCode", "name": "Mã nhân viên", "autoWidth": true },
                { "data": "userName", "name": "Người thao tác", "autoWidth": true },
                {
                    "data": "createDate", "name": "Thời gian thao tác",
                    render: function (data, type, row, index) {
                        let result;
                        result = moment(data).format("DD/MM/YYYY HH:mm");
                        return result;
                    },
                    "autoWidth": true
                },
                {
                    "data": "departmentName", "name": "Phòng ban",
                    render: function (data, type, row, index) {
                        return data;
                    },
                    "autoWidth": true
                },
                {
                    "data": "activityType", "name": "Loại nghiệp vụ",
                    render: function (data, type, row, index) {
                        let resultHTML = "";
                        if (data == 0) {
                            resultHTML = `<span class="history_type_input">${window.languageData[window.currentLanguage]['Nhập kho']}</span>`
                        } else if (data == 1) {
                            resultHTML = `<span class="history_type_output">${window.languageData[window.currentLanguage]['Xuất kho']}</span>`
                        }
                        return resultHTML;
                    },
                    "autoWidth": true
                },
                {
                    "data": "componentCode", "name": "Mã linh kiện",
                    render: function (data, type, row, index) {
                        return data;
                    },
                    "autoWidth": true
                },
                {
                    "data": "positionCode", "name": "Vị trí cố định",
                    render: function (data, type, row, index) {
                        return data;
                    },
                    "autoWidth": true
                },
                {
                    "data": "quantity", "name": "Số lượng",
                    render: function (data, type, row, index) {
                        let convertNumber = ValidateInputHelper.Utils.convertDecimalInventory(data);
                        return convertNumber;
                    },
                    "autoWidth": true
                },
                {
                    "data": "note", "name": "Ghi chú",
                    render: function (data, type, row, index) {
                        let resultHTML = `<div class="history_note">${data}</div>`;
                        return resultHTML;
                    },
                    "autoWidth": true
                },
                {
                    "data": "id",
                    "name": "Xem chi tiết",
                    render: function (data, type, row, index) {
                        let template = `<b class="history_detail" data-id="${row.id}" >${window.languageData[window.currentLanguage]['Xem chi tiết']}</b>`
                        return template
                    },
                    "autoWidth": true
                },
            ],
        });
    }

    function ValidateFilterForm() {
        jQuery.validator.addMethod("validateDateRange", function (value, element) {
            let valid = true;

            let fromDate = root.$filterForm.find("#input_date_from").val();
            let toDate = root.$filterForm.find("#input_date_to").val();

            if (fromDate && toDate) {
                let fromDateMoment = moment(fromDate, "DD/MM/YYYY");
                let toDateMoment = moment(toDate, "DD/MM/YYYY");

                if (fromDateMoment > toDateMoment) {
                    valid = false;
                }
            }
            return valid;
        }, 'Thời gian không đúng. Vui lòng chọn lại.');

        jQuery.validator.addMethod("quantityRangeValidate", function (value, element) {
            let valid = true;

            let quantityFrom = root.$filterForm.find("#input_quantity_range_from").val().replaceAll(",", "");
            let quantityTo = root.$filterForm.find("#input_quantity_range_to").val().replaceAll(",", "");

            if (quantityFrom && quantityTo) { 
                let convertQuantityFrom = Number(quantityFrom);
                let convertQuantityTo = Number(quantityTo);

                if (convertQuantityFrom > convertQuantityTo) {
                    valid = false;
                }
            }
            
            return valid;
        }, 'Sai định dạng, vui lòng nhập lại.');

        root.$filterForm.validate({
            rules: {
                QuantityRangeFrom: {
                    number: true,
                    quantityRangeValidate: true,
                },
                QuantityRangeTo: {
                    number: true,
                    quantityRangeValidate: true
                },
                DateFrom: {
                    validateDateRange: true,
                    validDateFormat: true
                },
                DateTo: {
                    validateDateRange: true,
                    validDateFormat: true
                }
            },
            messages: {
                
            }
        })
    }

    function Preload() {
        ValidateFilterForm();

        root.$input_date_from.datepicker({
            format: 'dd/mm/yyyy',
            autoclose: true,
            gotoCurrent: true,
            todayHighlight: true,
            todayBtn: "linked",
            clearBtn: true
        });
        root.$input_date_to.datepicker({
            format: 'dd/mm/yyyy',
            autoclose: true,
            gotoCurrent: true,
            todayHighlight: true,
            todayBtn: "linked",
            clearBtn: true
        });

        root.$select_activity_type.find("option").attr("selected", true);
        root.$select_factories.find("option").attr("selected", true);
        root.$select_areas.find("option").attr("selected", true);
        root.$select_departments.find("option").attr("selected", true);

        VirtualSelect.init({
            ele: '#select_activity_type, #select_factories, #select_areas, #select_departments',
            selectAllText: window.languageData[window.currentLanguage]['Tất cả'],
            noOptionsText: window.languageData[window.currentLanguage]['Không có kết quả'],
            noSearchResultsText: window.languageData[window.currentLanguage]['Không có kết quả'],
            searchPlaceholderText: window.languageData[window.currentLanguage]['Tìm kiếm'],
            allOptionsSelectedText: window.languageData[window.currentLanguage]['Tất cả'],
            optionsSelectedText: window.languageData[window.currentLanguage]["điều kiện đã được chọn"],
            selectAllOnlyVisible: true,
            hideClearButton: true,
        });

        root.$select_activity_type = $(root.parent).find("#select_activity_type");
        root.$select_factories = $(root.parent).find("#select_factories");
        root.$select_areas = $(root.parent).find("#select_areas");
        root.$select_departments = $(root.parent).find("#select_departments");

        $("#select_activity_type, #select_factories, #select_areas, #select_departments").show();

        InitDatatable();
    }

    function waitForInputOutHistoryLanguageData() {
        // Kiểm tra nếu dữ liệu đã sẵn sàng
        if (window.languageData && window.currentLanguage) {

            Cache();
            Preload();
            Events();

        } else {
            // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
            setTimeout(waitForInputOutHistoryLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
        }
    }


    function Init() {
        if (root.parent.length < 0) {
            console.error("Không tìm thấy màn hình lịch sử .Views_Storage_InOutHistory")
            return;
        }

        waitForInputOutHistoryLanguageData();
        
    }

    return {
        init: Init
    }
})();