$(function () {
    waitForInputStorageLanguageData();

});

function waitForInputStorageLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        InputStorageController.init();

    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForInputStorageLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}

;var InputStorageController = (function () {
    let root = {
        parentEl: $(".Views_Storage_Index"), 
    }
    let datatable;
    let dataFilter = {
        userName: "",
        fromDate: "",
        toDate: "",
        factories: "",
        statuses: ""
    };

    //function PostCreateDepartmentAPI(model) {
    //    return new Promise(async (resolve, reject) => {
    //        let url = `${host}/api/identity/department/create`

    //        try {
    //            const res = await $.ajax({
    //                url: url,
    //                type: 'POST',
    //                contentType: 'application/json',
    //                data: JSON.stringify(model)
    //            });
    //            resolve(res)
    //        } catch (err) {
    //            reject(err)
    //        }
    //    })
    //}

    function DeleteBwinAPI(inputId) {
        return new Promise(async (resolve, reject) => {
            let url = `${App.ApiGateWayUrl}/api/storage/input-storage/${inputId}`

            try {
                const res = await $.ajax({
                    url: url,
                    type: 'DELETE',
                    contentType: 'application/json'
                });
                resolve(res)
            } catch (err) {
                reject(err)
            }
        })
    }

    function PostImportFileAPI(userId, formFile) {
        return new Promise(async (resolve, reject) => {
            let host = App.ApiGateWayUrl;
            let url = `${host}/api/storage/import/${userId}`;

            $.ajax({
                type: 'POST',
                url: url,
                data: formFile,
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
        });
    }

    function PostExportFileAPI(userId, filterModel) {
        return new Promise(async (resolve, reject) => {
            let url = `${App.ApiGateWayUrl}/api/storage/input-storage/export/${userId}`;
            
            var xhr = new XMLHttpRequest();
            xhr.open('POST', url, true);
            xhr.setRequestHeader('Authorization', `Bearer ${App.Token}`);
            //xhr.processData = false;

            var filterFormData = new FormData();
            for (var key in filterModel) {
                if (filterModel.hasOwnProperty(key)) {
                    filterFormData.append(key, filterModel[key]);
                }
            }

            xhr.onreadystatechange = function (x) {
                if (xhr.readyState === 4) {
                    if (xhr.status == 200) {
                        resolve(xhr.response);
                    } else {
                        reject(xhr.responseText);
                    }
                } else if (xhr.readyState == 2) {
                    if (xhr.status == 200) {
                        xhr.responseType = 'blob';
                    } else {
                        xhr.responseType = 'text';
                    }
                }
            };

            xhr.send(filterFormData);
        })
    }

    function Events() {
        root.$searchForm.submit((e) => {
            e.preventDefault();
        })

        root.$searchForm.on("keypress", ValidateInputHelper.FormEnter(function (e) {
            let validForm = root.$searchForm.valid();
            if (validForm) {
                datatable.draw();
            }
        }))

        root.$input_bwin_table.delegate(".input_storage_detail", "click", function (e) {
            let inputId = $(e.target).closest(".input_storage_detail").data("id");
            root.$input_detail_modal.data("id", inputId)

            let target = e.target;
            let thisButton = $(target).closest(".input_storage_detail");

            let bwinStatus = thisButton.attr("status");

            let maNV = thisButton.closest("tr").find("td:eq(1)").text();
            let nguoiNhap = thisButton.closest("tr").find("td:eq(2)").text();
            let ngayNhap = thisButton.closest("tr").find("td:eq(3)").text();
            let tongSoLK = thisButton.closest("tr").find("td:eq(4)").text();
            let trangThai = thisButton.closest("tr").find("td:eq(5)").html();

            root.$input_detail_modal.find("#input_detail_inputUserName").text(nguoiNhap);
            root.$input_detail_modal.find("#input_detail_userCode").text(maNV);
            root.$input_detail_modal.find("#input_detail_createdAt").text(ngayNhap);
            root.$input_detail_modal.find("#input_detail_status").html(trangThai);

            if (bwinStatus == '1') {
                root.$btnConfirmModal.attr("disabled", false);
                root.$btnConfirmModal.show();
                $(".container_checkbox").show();
            } else {
                root.$btnConfirmModal.attr("disabled", true);
                root.$btnConfirmModal.hide();
                $(".container_checkbox").hide();
            }
            root.$input_detail_modal.data("status", thisButton.attr("status"));

            root.$input_detail_modal.modal("show");

            setTimeout(() => {
                InputStorageDetailController.loadDataTable();
            },100)

        })

        //Button click import
        root.$btnImportFile.click((e) => {
            let target = e.target;

            root.$importFileInput.val("");
            root.$importFileInput.trigger("click");
        })

        root.$importFileInput.change(function (e) {
            let target = e.target;
            let file = target.files[0];
            let userId = App.User.UserId;

            if (file) {
                var formData = new FormData();
                formData.append("file", file);

                loading(true);
                PostImportFileAPI(userId, formData).then((res) => {
                    //Nếu có dữ liệu lỗi thì cho tải về file
                    if (res.failCount > 0) {
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                            text: `${window.languageData[window.currentLanguage]["Import thành công"]} ${res.successCount} ${window.languageData[window.currentLanguage]["dòng dữ liệu và có"]} ${res.failCount } 
                            ${window.languageData[window.currentLanguage]['dòng dữ liệu lỗi không thể thực hiện import. Vui lòng ấn "Đồng ý" để xem dữ liệu lỗi.']}`,
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
                                let convertedBytes = FileTemplateHandler.utils.base64ToArrayBuffer(res.bytes);
                                FileTemplateHandler.utils.saveByteArr(convertedBytes, res.fileType, res.fileName);
                            }
                        })
                    }

                    //Nếu không có dòng lỗi là nhập kho thành công
                    if (res.failCount == 0) {
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                            text: `${window.languageData[window.currentLanguage]["Import thành công"]} ${res.successCount} ${window.languageData[window.currentLanguage]["dòng dữ liệu."]}`,
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        })
                    }
                }).catch((err) => {
                    Swal.fire({
                        title: `<b>${window.languageData[window.currentLanguage]["Thông báo"]}</b>`,
                        text: err?.responseJSON?.message || "",
                        confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                        width: '30%'
                    })
                }).finally(() => {
                    datatable.draw();
                    loading(false);
                })
            }
        })

        //Delete input from bwin
        root.$input_bwin_table.delegate(".btnDeleteInput", "click", (e) => {
            let target = e.target;
            let thisButton = $(target).closest('.btnDeleteInput');
            let inputId = thisButton.attr("inputid");

            Swal.fire({
                title: `<b>${window.languageData[window.currentLanguage]["Xác nhận xóa"]}</b>`,
                text: window.languageData[window.currentLanguage]["Bạn có chắc chắn muốn xóa lần nhập kho này ?"],
                confirmButtonText: window.languageData[window.currentLanguage]['Đồng ý'],
                showCancelButton: true,
                showLoaderOnConfirm: true,
                cancelButtonText: window.languageData[window.currentLanguage]['Hủy bỏ'],
                reverseButtons: true,
                allowOutsideClick: false,
                customClass: {
                    actions: "swal_confirm_actions"
                },
                preConfirm: () => {
                    loading(true);
                    DeleteBwinAPI(inputId).then((res) => {
                        toastr.success(window.languageData[window.currentLanguage]["Xóa lần nhập kho thành công."]);
                        datatable.draw();
                    }).catch((err) => {
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]["Không thể xóa"]}</b>`,
                            text: err?.responseJSON?.message,
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        })
                    }).finally(() => loading(false));
                }
            })
        })

        root.$btnReset.click((e) => {
            let target = e.target;
            let thisButotn = $(target);

            root.$inputUserName.val("");
            root.$inputDateFrom.val("").datepicker('update');
            root.$inputDateTo.val("").datepicker('update');

            $("#input_date_from-error").hide();
            $("#input_date_to-error").hide();

            root.$selectFactory[0].reset();
            root.$selectStatus[0].reset();

            root.$selectFactory[0].toggleSelectAll(true);
            root.$selectStatus[0].toggleSelectAll(true);

            datatable.draw();
        })

        //Search event
        root.$btnSearch.click(function (e) {
            let thisButton = $(this);

            let formValid = root.$searchForm.valid();
            if (formValid) {
                datatable.draw();
            }
        })

        //Export
        root.$btnExport.click(ValidateInputHelper.Utils.debounce(function (e) {
            let thisButton = $(this);
            let userId = App.User.UserId;

            dataFilter.userName = root.$inputUserName.val();
            dataFilter.fromDate = root.$inputDateFrom.val();
            dataFilter.toDate = root.$inputDateTo.val();
            dataFilter.factories = root.$selectFactory.val();
            dataFilter.statuses = root.$selectStatus.val();

            loading(true);
            PostExportFileAPI(userId, dataFilter).then((res) => {
                loading(false);

                let fileName = "OUTPUT_STORAGE_RESULT.xlsx";

                let blob = new Blob([res], { type: res.type });
                //let blob = new Blob([res], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
                let fileUrl = URL.createObjectURL(blob);

                var a = $("<a style='display: none;'/>");
                a.attr("href", fileUrl);
                a.attr("download", fileName);
                $("body").append(a);
                a[0].click();

                //Clear temp data
                window.URL.revokeObjectURL(fileUrl);
                a.remove();

            }).catch((err) => {
                loading(false);
                let parsedResponse = JSON.parse(err);
                Swal.fire({
                    title: `<b>${parsedResponse.message}</b>`,
                    confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                    width: '30%'
                })
            })
        }, 200))

        root.parentEl.find(".calendar_icon").click((e) => {
            let target = e.target;
            $(target).closest(".calendar_icon").prevAll("input").datepicker("show");
        })

        $("#input_date_from, #input_date_to").keydown(function (e) {
            e.preventDefault(); // Chặn nhập chữ và số
        });

        $("#input_date_from, #input_date_to").change(function (e) {
            root.$searchForm.valid();
        });

        $("#btnDownloadInputStorageTemplate").click((e) => {
            let fileKey = "TemplateImportInputStorage";
            $.ajax({
                type: 'GET',
                //url: '/file-template/input-storage',
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
                        a.download = 'Mẫu import nhập kho trên web.csv';

                        // Kích hoạt sự kiện nhấp chuột trên liên kết để tải xuống
                        a.click();
                        window.URL.revokeObjectURL(url);
                    }
                },
                error: function () {
                    toastr.error('Xảy ra lỗi khi tải biểu mẫu.');
                }
            });
        });
    }

    function Cache() {
        root.$input_bwin_table = root.parentEl.find("#input_bwin_table")
        root.$input_bwin_Detail = root.$input_bwin_table.find(".input_storage_detail")

        //Detail modal
        root.$input_detail_modal = $(root.parentEl.find("#InputStorageDetailModal"))

        root.$btnImportFile = root.parentEl.find("#btn_import_input_storage")
        root.$importFileInput = root.parentEl.find("#file_import_input_storage")

        root.$btnConfirmModal = root.$input_detail_modal.find("#btnConfirmImport")

        root.$inputDateFrom = $(root.parentEl.find("#input_date_from"))
        root.$inputDateTo = $(root.parentEl.find("#input_date_to"))
        root.$inputUserName = $(root.parentEl.find("#input_userName"))
        root.$selectFactory = $(root.parentEl.find("#select_factory"))
        root.$selectStatus = $(root.parentEl.find("#select_status"))

        root.$btnReset = root.parentEl.find("#btn-reset")
        root.$btnSearch = root.parentEl.find("#btn-search")

        root.$btnExport = root.parentEl.find("#export_file")

        root.$searchForm = $(root.parentEl.find("#input_storage_search_form"))
    }

    function PreLoad() {
        jQuery.validator.addMethod("validateDateRange", function (value, element) {
            let valid = true;

            let fromDate = root.parentEl.find("#input_date_from").val();
            let toDate = root.parentEl.find("#input_date_to").val();

            if (fromDate && toDate) {
                let fromDateMoment = moment(fromDate, "DD/MM/YYYY");
                let toDateMoment = moment(toDate, "DD/MM/YYYY");

                if (fromDateMoment > toDateMoment) {
                    valid = false;
                }
            }
            return valid;
        }, window.languageData[window.currentLanguage]['Thời gian không đúng. Vui lòng chọn lại.']);


        root.$searchForm.validate({
            rules: {
                FromDate: {
                    validateDateRange: true,
                    validDateFormat: true
                },
                ToDate: {
                    validateDateRange: true,
                    validDateFormat: true
                }
            }    
        })

        //root.$inputDateFrom.datepicker({
        //    format: 'dd/mm/yyyy',
        //    autoclose: true,
        //    gotoCurrent: true,
        //    todayHighlight: true,
        //    todayBtn: "linked",
        //    clearBtn: true
        //});

        root.$inputDateFrom.datepicker({
            format: 'dd/mm/yyyy',
            autoclose: true,
            gotoCurrent: true,
            todayHighlight: true,
            todayBtn: "linked",
            clearBtn: true
        }).on('show', function () {
            // Chỉ thay đổi text của nút "Today" và "Clear"
            $('.datepicker .clear').each(function () {
                if ($(this).text() === 'Xóa') {
                    $(this).text(window.languageData[window.currentLanguage]['Xóa']);
                }
            });

            $('.datepicker .today').each(function () {
                if ($(this).text() === 'Hôm nay') {
                    $(this).text(window.languageData[window.currentLanguage]['Hôm nay']);
                }
            });
        });


        root.$inputDateTo.datepicker({
            format: 'dd/mm/yyyy',
            autoclose: true,
            gotoCurrent: true,
            todayHighlight: true,
            todayBtn: "linked",
            clearBtn: true
        }).on('show', function () {
            // Chỉ thay đổi text của nút "Today" và "Clear"
            $('.datepicker .clear').each(function () {
                if ($(this).text() === 'Xóa') {
                    $(this).text(window.languageData[window.currentLanguage]['Xóa']);
                }
            });

            $('.datepicker .today').each(function () {
                if ($(this).text() === 'Hôm nay') {
                    $(this).text(window.languageData[window.currentLanguage]['Hôm nay']);
                }
            });
        });

        root.$selectFactory.find("option").attr("selected", true);
        root.$selectStatus.find("option").attr("selected", true);

        VirtualSelect.init({
            ele: '#select_factory, #select_status',
            selectAllText: window.languageData[window.currentLanguage]['Tất cả'],
            noOptionsText: window.languageData[window.currentLanguage]['Không có kết quả'],
            noSearchResultsText: window.languageData[window.currentLanguage]['Không có kết quả'],
            searchPlaceholderText: window.languageData[window.currentLanguage]['Tìm kiếm'],
            allOptionsSelectedText: window.languageData[window.currentLanguage]['Tất cả'],
            optionsSelectedText: window.languageData[window.currentLanguage]["điều kiện đã được chọn"],
            selectAllOnlyVisible: true,
            hideClearButton: true,
        });

        root.$selectFactory = root.parentEl.find("#select_factory");
        root.$selectStatus = root.parentEl.find("#select_status");

        setTimeout(() => {
            root.$selectFactory.show();
            root.$selectStatus.show();
        }, 500)
        

        InitImportListDatatable();
    }

    function InitImportListDatatable() {
        let host = App.ApiGateWayUrl;

        datatable = $('#input_bwin_table').DataTable({
            "bDestroy": true,
            "processing": `<div class="spinner"></div>`,
            pagingType: 'full_numbers',
            'language': {
                'loadingRecords': `<div class="spinner"></div>`,
                'processing': '<div class="spinner"></div>',
            },
            select: true,
            "serverSide": true,
            "filter": true,
            "searching": false,
            responsive: true,
            "lengthMenu": [10, 30, 50, 200],
            dom: 'rt<"bottom"flp><"clear">',
            "ordering": false,
            "ajax": {
                "url": host + "/api/storage/input-storage",
                "type": "POST",
                "contentType": "application/x-www-form-urlencoded",
                dataType: "json",
                data: function (data) {
                    dataFilter.userId = App.User.UserId;
                    dataFilter.userName = root.$inputUserName.val();
                    dataFilter.fromDate = root.$inputDateFrom.val();
                    dataFilter.toDate = root.$inputDateTo.val();
                    dataFilter.factories = root.$selectFactory.val();
                    dataFilter.statuses = root.$selectStatus.val();

                    Object.assign(data, dataFilter);
                    return data;
                },
                "dataSrc": function ({ data }) {
                    return data;
                }
            },
            "drawCallback": function (settings) {
                let totalPages = datatable.page.info().pages;
                let totalRecords = datatable.page.info().recordsTotal;

                let currPage = datatable.page() + 1;
                if (currPage == 1) {
                    root.parentEl.find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                    root.parentEl.find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                }
                if (currPage == totalPages) {
                    root.parentEl.find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                    root.parentEl.find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                }

                root.parentEl.find(".pagination").before(`<div class="h5 datatable_total_records"> <b>${window.languageData[window.currentLanguage]["Tổng"]}</b>: ${totalPages} <span class="total_record_seperate"> | </span>
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
            //    { "width": "25%", "targets": 7 }
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
                { "data": "userName", "name": "Người nhập", "autoWidth": true },
                {
                    "data": "createDate", "name": "Ngày nhập kho",
                    render: function (data, type, row, index) {
                        let result;
                        result = moment(data).format("DD/MM/YYYY HH:mm");
                        return result;
                    },
                    "autoWidth": true
                },
                {
                    "data": "total", "name": "Tổng số mã LK",
                    render: function (data, type, row, index) {
                        let convertNumber = ValidateInputHelper.Utils.convertDecimalInventory(data);
                        return convertNumber;
                    },
                    "autoWidth": true
                },
                {
                    "data": "status", "name": "Trạng thái",
                    render: function (data, type, row, index) {
                        let result;
                        let statusClass = "";

                        if (+data == 1) {
                            result = window.languageData[window.currentLanguage]["Tạm thời"];
                            statusClass = "input_storage_temporary";
                        } else {
                            result = window.languageData[window.currentLanguage]["Đã xác nhận"];
                            statusClass = "input_storage_todo";
                        }

                        let template = `<b class="input_storage_item_status ${statusClass}">${result}</b>`
                        return template;
                    },
                    "autoWidth": true
                },
                {
                    "data": "inputId",
                    "name": "Xem chi tiết",
                    render: function (data, type, row, index) {
                        let template = `<b class="input_storage_detail" data-id="${row.inputId}" status="${row.status}">${window.languageData[window.currentLanguage]["Xem chi tiết"]}</b>`
                        return template
                    },
                    "autoWidth": true
                },
                {
                    "data": "inputId", "name": "Controls",
                    render: function (data, type, row, index) {
                        if (row.status == 1) {
                            return `
                                    <div class="btnDeleteInput col-6" inputId="${row.inputId}">
                                        <label class="btn">
                                           <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18 " viewBox="0 0 25 24" fill="none">
                                              <path d="M21.57 5.23C19.96 5.07 18.35 4.95 16.73 4.86V4.85L16.51 3.55C16.36 2.63 16.14 1.25 13.8 1.25H11.18C8.84997 1.25 8.62997 2.57 8.46997 3.54L8.25997 4.82C7.32997 4.88 6.39997 4.94 5.46997 5.03L3.42997 5.23C3.00997 5.27 2.70997 5.64 2.74997 6.05C2.78997 6.46 3.14997 6.76 3.56997 6.72L5.60997 6.52C10.85 6 16.13 6.2 21.43 6.73C21.46 6.73 21.48 6.73 21.51 6.73C21.89 6.73 22.22 6.44 22.26 6.05C22.29 5.64 21.99 5.27 21.57 5.23Z" fill="#E60000"/>
                                              <path d="M19.73 8.14C19.49 7.89 19.16 7.75 18.82 7.75H6.17999C5.83999 7.75 5.49999 7.89 5.26999 8.14C5.03999 8.39 4.90999 8.73 4.92999 9.08L5.54999 19.34C5.65999 20.86 5.79999 22.76 9.28999 22.76H15.71C19.2 22.76 19.34 20.87 19.45 19.34L20.07 9.09C20.09 8.73 19.96 8.39 19.73 8.14ZM14.16 17.75H10.83C10.42 17.75 10.08 17.41 10.08 17C10.08 16.59 10.42 16.25 10.83 16.25H14.16C14.57 16.25 14.91 16.59 14.91 17C14.91 17.41 14.57 17.75 14.16 17.75ZM15 13.75H9.99999C9.58999 13.75 9.24999 13.41 9.24999 13C9.24999 12.59 9.58999 12.25 9.99999 12.25H15C15.41 12.25 15.75 12.59 15.75 13C15.75 13.41 15.41 13.75 15 13.75Z" fill="#E60000"/>
                                            </svg>
                                        </label>
                                    </div>
                                `
                        }
                        return "";
                    },
                    "autoWidth": true
                },
            ],
        });
    }
    function waitForInputStorageLanguageData() {
        // Kiểm tra nếu dữ liệu đã sẵn sàng
        if (window.languageData && window.currentLanguage) {
            
            Cache();
            PreLoad();
            Events();

        } else {
            // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
            setTimeout(waitForInputStorageLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
        }
    }
    function Init() {
        if (root.parentEl?.length < 0) {
            console.error("Không tìm thấy Input storage container");
            return;
        }

        waitForInputStorageLanguageData();
    }

    function RefreshInputStorageTable() {
        datatable.draw();
    }

    return {
        init: Init,
        reloadTable: RefreshInputStorageTable 
    }
})();