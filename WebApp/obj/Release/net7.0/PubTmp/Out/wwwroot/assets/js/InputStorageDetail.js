$(function () {
    InputStorageDetailController.init();
});

; var InputStorageDetailController = (function () {
    let root = {
        parentEl: $("#InputStorageDetailModal"),
    }
    let dataTable;
    let cacheRow = {};
    let editing = false;

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

    function UpdateInputDetailAPI(inputId, model) {
        return new Promise(async (resolve, reject) => {
            host = App.ApiGateWayUrl;
            let url = `${host}/api/storage/input-storage/edit/${inputId}`

            try {
                const res = await $.ajax({
                    url: url,
                    type: 'PUT',
                    contentType: 'application/json',
                    data: JSON.stringify(model)
                });
                resolve(res)
            } catch (err) {
                reject(err)
            }
        })
    }

    function ChangeRemainingHandleAPI(inputDetailId, status) {
        host = App.ApiGateWayUrl;
        return new Promise(async (resolve, reject) => {
            let url = `${host}/api/storage/input-storage/update-remaininghandle/${inputDetailId}/${status}`
            try {
                const res = await $.ajax({
                    url: url,
                    type: 'PUT',
                    contentType: 'application/json',
                });
                resolve(res)
            } catch (err) {
                reject(err)
            }
        })
    }

    function ConfirmImportAPI(userId, inputId) {
        return new Promise(async (resolve, reject) => {
            host = App.ApiGateWayUrl;
            let url = `${host}/api/storage/input-storage/confirm/${inputId}/${userId}`

            try {
                const res = await $.ajax({
                    url: url,
                    type: 'POST',
                    contentType: 'application/json',
                });
                resolve(res)
            } catch (err) {
                reject(err)
            }
        })
    }
    function isValidNumber(str) {
        // Loại bỏ ký tự có dấu tiếng Việt
        str = str.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
        return !/[~`!@#$%\^&*()+=\-\[\]\\';./{}|\\":<>\?_a-zA-Z ]/g.test(str);
    }

    function Events() {
        jQuery.validator.addMethod("minQuantity", function (value, element) {
            let valid = true;
            let convertNumber = parseFloat(value);
            if (convertNumber <= 0) {
                valid = false;
            }
            return valid;
        }, 'Vui lòng nhập số lượng lớn hơn 0.');

        jQuery.validator.addMethod("maxQuantityCharacter", function (value, element, param) {
            let valid = true;

            let convertedValue = value.replaceAll(",", "");
            let length = convertedValue.length

            if (length > param) {
                valid = false;
            }
            return valid;
        }, `Số lượng nhập tối đa {0} ký tự.`);

        jQuery.validator.addMethod("validateQuantityOnlyNumber", function (value, element) {
            return this.optional(element) || isValidNumber(value)
        }, 'Số lượng không đúng định dạng.');

        root.$inputDetailForm.validate({
            rules: {
                Quantity: {
                    required: true,
                    //number: true,
                    minQuantity: true,
                    maxQuantityCharacter: numberConfig.maxCharacterLength
                    //validateQuantityOnlyNumber: true
                },
                Note: {
                    required: true,
                    maxlength: 50 
                }
            },
            messages: {
                Quantity: {
                    required: "Vui lòng nhập số lượng.",
                    //validateQuantityOnlyNumber: "Số lượng không đúng định dạng.",
                },
                Note: {
                    required: "Vui lòng nhập ghi chú.",
                    maxlength: "Tối đa {0} kí tự."
                }
            }
        })
        
        //Prevent form submit
        root.$inputDetailForm.submit((e) => {
            e.preventDefault();
        })

        root.$detailTable.delegate(".input_detail_edit", "click", function (e) {
            let target = $(e.target).closest(".input_detail_edit");
            let detailId = $(target).attr("id");
            let row = $(target).closest("tr");

            if (!editing) {
                editing = true;
                //Save clone row for cancle 
                let cloneRow = row.clone();
                cacheRow = cloneRow;

                let quantityColumn = row.find(`td:eq(4)`);
                let noteColumn = row.find(`td:eq(5)`);

                let quantityValue = quantityColumn.text();
                let noteValue = noteColumn.text();
                //Replace td to input
                quantityColumn.empty().append(`<input type="text" name="Quantity" value="${quantityValue}" />`);
                noteColumn.empty().append(`<textarea name="Note" style="height: 40px;">${noteValue}</textarea>`);

                let controls = $(`
                    <td>
                        <div class="d-flex w-50 justify-content-between">
                            <button class="btn_input_detail_cancle btn">Hủy</button>
                            <button class="btn_input_detail_change_row btn btn-danger" detailId="${detailId}">Lưu</button>
                        </div>
                    </td>
                `);

                target.closest("td").replaceWith(controls);

                numberConfig.initAutoNumeric("#input_storage_detail_table input[name='Quantity']");
            } else {
                toastr.warning("Đang thực hiện chỉnh sửa.");
            }
        })

        //Cancle
        root.$detailTable.delegate(".btn_input_detail_cancle", "click", function (e) {
            let target = $(e.target).closest(".btn_input_detail_cancle");

            let row = target.closest("tr");

            //let previousRow = root.$detailTable.data("tempRow");
            row.replaceWith(cacheRow);

            editing = false;
        })
        //Save
        root.$detailTable.delegate(".btn_input_detail_change_row", "click", function (e) {
            let target = e.target;
            let thisButton = $(target).closest(".btn_input_detail_change_row");
            let detailId = thisButton.attr("detailId");
            row = $(target).closest("tr");

            let isValidForm = root.$inputDetailForm.valid();
            if (isValidForm) {
                let quantity = row.find("input[name='Quantity']");
                let note = row.find("[name='Note']");

                //Update new value
                let newQuantity = quantity.val();
                let newNote = note.val();
                //cacheRow.find("td:eq(4)").text(newQuantity);
                //cacheRow.find("td:eq(5)").text(newNote);

                let inputId = root.$detailModal.data("id");
                //let detailId = root.$detailModal.
                let model = {
                    Quantity: newQuantity.replaceAll(",", ""),
                    Note: newNote,
                    UserId: App.User.UserId
                }
                //Call api check valid quantity
                UpdateInputDetailAPI(detailId, model).then((res) => {
                    LoadDatatable();
                    //row.replaceWith(cacheRow);
                    toastr.success(res?.message);

                    editing = false;
                }).catch((err) => {
                    Swal.fire({
                        //title: "<b>Quá sức chứa</b>",
                        text: err?.responseJSON?.message,
                        confirmButtonText: "Đã hiểu",
                        width: '30%'
                    })

                    editing = false;
                })
            }
        })

        //root.$detailTable.delegate("input[name='Quantity']", "keypress", ValidateInputHelper.OnlyNumerOnKeyPress);
        //root.$detailTable.delegate("input[name='Quantity']", "keypress", ValidateInputHelper.LimitInputLengthOnKeyPress(8));
        //root.$detailTable.delegate("input[name='Quantity']", "keyup", ValidateInputHelper.LimitInputLengthOnKeyPress(8));
        root.$detailTable.delegate("textarea[name='Note']", "keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50));

        root.$detailTable.delegate(".input_detail_select_remainingHandle", "change", function (e) {
            $("#checkbox-return-bwin").prop('checked', false);
            $("#checkbox-temporary-save").prop('checked', false);

            let target = e.target;
            let thisTarget = $(target).closest(".input_detail_select_remainingHandle");

            let status = thisTarget.val();
            let inputDetailId = thisTarget.attr("detailid");
            ChangeRemainingHandleAPI(inputDetailId, status).then((res) => {
                toastr.success(res?.message);
            }).catch((err) => {
                Swal.fire({
                    //title: "<b>Không thể xóa</b>",
                    text: err?.responseJSON?.message,
                    confirmButtonText: "Đã hiểu",
                    width: '30%'
                })
            })
        })

        root.$checkboxTemporarySave.click(function () {
            $("#checkbox-return-bwin").prop('checked', false);
            if ($(this).is(":checked")) {
                $(".input_detail_select_remainingHandle").val("1");
                
                $(".input_detail_select_remainingHandle").each(function (e) {
                    var inputDetailId = $(this).attr("detailid");
                    ChangeRemainingHandleAPI(inputDetailId, 1).then((res) => {
                        // toastr.success(res?.message);
                    }).catch((err) => {
                        Swal.fire({
                            //title: "<b>Không thể xóa</b>",
                            text: err?.responseJSON?.message,
                            confirmButtonText: "Đã hiểu",
                            width: '30%'
                        })
                    })
                })

                toastr.success("Cập nhật thành công");
            }
        })

        root.$checkboxReturnBwin.click(function () {
            $("#checkbox-temporary-save").prop('checked', false);
            if ($(this).is(":checked")) {
                $(".input_detail_select_remainingHandle").val("2");

                $(".input_detail_select_remainingHandle").each(function (e) {
                    var inputDetailId = $(this).attr("detailid");
                    ChangeRemainingHandleAPI(inputDetailId, 2).then((res) => {
                        // toastr.success(res?.message);
                    }).catch((err) => {
                        Swal.fire({
                            //title: "<b>Không thể xóa</b>",
                            text: err?.responseJSON?.message,
                            confirmButtonText: "Đã hiểu",
                            width: '30%'
                        })
                    })
                } )

                toastr.success("Cập nhật thành công");
            }
        })

        root.$btnConfirmImport.click(function (e) {
            let target = e.target;
            let inputId = root.$detailModal.data("id");
            let userId = App.User.UserId;

            Swal.fire({
                title: '<b>Xác nhận nhập kho</b>',
                text: "Bạn có chắc chắn muốn xác nhận lần phân bổ này ?",
                confirmButtonText: 'Đồng ý',
                showCancelButton: true,
                showLoaderOnConfirm: true,
                cancelButtonText: 'Hủy bỏ',
                reverseButtons: true,
                allowOutsideClick: false,
                customClass: {
                    actions: "swal_confirm_actions"
                }
            }).then((result, e) => {
                if (result.isConfirmed) {
                    loading(true);
                    ConfirmImportAPI(userId, inputId).then((res) => {
                        if (res.failCount) {
                            Swal.fire({
                                title: `<b>Thông báo</b>`,
                                text: `Import thành công ${res.successCount} dòng dữ liệu, có ${res.failCount} dòng dữ liệu lỗi. 
                                            Vui lòng nhấn nút "Đồng ý" để tải xuống file kết quả.`,
                                confirmButtonText: 'Đồng ý',
                                showCancelButton: true,
                                showLoaderOnConfirm: true,
                                cancelButtonText: 'Hủy bỏ',
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
                            });
                        } else {
                            root.$btnConfirmImport.attr("disabled", true);
                            root.$btnConfirmImport.hide();

                            toastr.success(res.message);
                        }
                    }).catch((err) => {
                        Swal.fire({
                            title: `<b>${err?.responseJSON?.message}</b>`,
                            confirmButtonText: "Đã hiểu",
                            width: '30%'
                        })
                    }).finally(() => {
                        loading(false);

                        $("#InputStorageDetailModal").modal("hide");
                        InputStorageController.reloadTable();
                    })
                }
            })
        })

        $("#InputStorageDetailModal").on("hidden.bs.modal", function (e) {
            editing = false;
        });
    }

    function Cache() {
        root.$detailModal = $(root.parentEl);
        root.$inputDetailForm = root.$detailModal.find("#input_storage_detail_form");
        root.$detailTable = $(root.parentEl.find("#input_storage_detail_table"));

        root.$btnConfirmImport = root.parentEl.find("#btnConfirmImport");
        root.$checkboxTemporarySave = root.parentEl.find("#checkbox-temporary-save");
        root.$checkboxReturnBwin = root.parentEl.find("#checkbox-return-bwin");
    }

    function PreLoad() {
    }

    function LoadDatatable() {
        let host = App.ApiGateWayUrl;
        let inputId = root.$detailModal.data("id");

        dataTable = root.$detailTable.DataTable({
            "bDestroy": true,
            fixedHeader: true,
            responsive: true,
            "processing": `<div class="spinner"></div>`,
            pagingType: 'full_numbers',
            'language': {
                'loadingRecords': `<div class="spinner"></div>`,
                'processing': '<div class="spinner"></div>',
            },
            select: true,
            "serverSide": true,
            "filter": true,
            scrollY: true,
            "searching": false,
            "lengthMenu": [10, 30, 50, 200],
            dom: 'rt<"bottom"flp><"clear">',
            "ordering": false,
            "ajax": {
                "url": host + `/api/storage/input-storage/details`,
                "type": "POST",
                "contentType": "application/x-www-form-urlencoded",
                dataType: "json",
                data: function (data) {
                    data.inputId = inputId;
                    data.userId = App.User.UserId;

                    let selectFactory = $(".Views_Storage_Index").find("#select_factory");
                    let factories = selectFactory.val();

                    data.factories = factories;

                    return data;
                },
                "dataSrc": function ({ data, totalNumber, remainingNumber }) {
                    $("#input_detail_total").data("totalNumber", totalNumber)
                    $("#input_detail_total").text(ValidateInputHelper.Utils.convertDecimalInventory(totalNumber))

                    $("#input_detail_remainingNumber").data("remainingNumber", remainingNumber)
                    $("#input_detail_remainingNumber").text(ValidateInputHelper.Utils.convertDecimalInventory(remainingNumber))

                    return data;
                }
            },
            "drawCallback": function (settings) {
                let totalPages = dataTable.page.info().pages;
                let totalRecords = dataTable.page.info().recordsTotal;

                let currPage = dataTable.page() + 1;
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

                editing = false;
            },
            "columnDefs": [
                { "width": "10%", "targets": [0,1,3,4,5,6,7] }, 
                { "width": "15%", "targets": 2 }, 
                //{ "width": "25%", "targets": 3 }, 
                //{ "width": "20%", "targets": 4 }, 
                //{ "width": "20%", "targets": 5 }, 
                //{ "width": "10%", "targets": 6 }, 
                //{ "width": "5%", "targets": 7 }, 
            ],
            "columns": [
                {
                    "data": "bwinOutputCode", "name": "Mã chỉ thị xuất kho",
                    render: function (data, type, row, index) {
                        return `<text id="${row.detailId}" inputId="${row.inputId}">${row.bwinOutputCode}</text>`
                    },
                    "autoWidth": true 
                },
                { "data": "componentCode", "name": "Mã linh kiện", "autoWidth": true },
                { "data": "suplierCode", "name": "Mã nhà cung cấp", "autoWidth": true },
                { "data": "positionCode", "name": "Vị trí", "autoWidth": true },
                {
                    "data": "quantity", "name": "Số lượng",
                    render: function (data, type, row, index) {
                        let convertNumber = ValidateInputHelper.Utils.convertDecimalInventory(data);
                        return convertNumber;
                    },
                    "autoWidth": true
                },
                { "data": "note", "name": "Ghi chú", "autoWidth": true },
                {
                    "data": "remainingHandle", "name": "remaining",
                    render: function (data, type, row, index) {
                        //type = 2 => remaining
                        let remainingHandle = row.remainingHandle;

                        if (row.type == 2) {
                            let remainingHandleSelect = `
                            <select class="input_detail_select_remainingHandle" detailId=${row.detailId}>
                                <option value="1" class="remaining_handle_green" ${remainingHandle == 1 ? "selected" : ""}>Lưu tạm thời</option>
                                <option value="2" class="remaining_handle_yellow" ${remainingHandle == 2 ? "selected" : ""}>Trả về Bwin</option>
                            </select>
                        `
                            return remainingHandleSelect;
                        }
                        return "";
                    },
                    "autoWidth": true 
                },
                {
                    "data": "detailId", "name": "",
                    render: function (data, type, row, index) {
                        let isEditable = row.type == 1;
                        let bwinStatus = $(root.parentEl).data("status");
                        if (bwinStatus == "0") {
                            return "";
                        }

                        if (isEditable) {
                            let editButtonHTML = `
                                                    <svg id="${row.detailId}" class="input_detail_edit" xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none">
                                                      <path fill-rule="evenodd" clip-rule="evenodd" d="M13.6962 5.47381C15.6862 3.48381 17.5862 3.53381 19.5262 5.47381C20.5262 6.46381 21.0062 7.42381 20.9962 8.41381C20.9962 9.37381 20.5162 10.3238 19.5262 11.3038L18.3262 12.5138C18.2462 12.5938 18.1462 12.6338 18.0362 12.6338C17.9962 12.6338 17.9562 12.6238 17.9162 12.6138C15.2662 11.8538 13.1462 9.73381 12.3862 7.08381C12.3462 6.94381 12.3862 6.78381 12.4862 6.68381L13.6962 5.47381ZM15.2762 13.0838C15.5462 13.2438 15.8262 13.3838 16.1162 13.5238C16.1551 13.5407 16.1935 13.5572 16.2315 13.5734C16.5691 13.7175 16.667 14.1629 16.4075 14.4225L10.6862 20.1438C10.5662 20.2738 10.3162 20.3938 10.1362 20.4238L6.29618 20.9638C6.17618 20.9838 6.05618 20.9938 5.93618 20.9938C5.39618 20.9938 4.89618 20.8038 4.53618 20.4538C4.11618 20.0238 3.92618 19.3838 4.02618 18.7038L4.56618 14.8738C4.59618 14.7038 4.71618 14.4538 4.84618 14.3238L10.5746 8.59534C10.8324 8.33758 11.267 8.43503 11.4146 8.76834C11.4345 8.81325 11.455 8.85841 11.4762 8.90381C11.6162 9.18381 11.7562 9.45381 11.9162 9.72381C12.0462 9.94381 12.1862 10.1638 12.3062 10.3138C12.4463 10.5286 12.6038 10.7172 12.7054 10.839C12.7126 10.8476 12.7196 10.8559 12.7262 10.8638C12.7403 10.882 12.7537 10.8994 12.7662 10.9157C12.8156 10.9801 12.8522 11.0279 12.8762 11.0438C13.2062 11.4438 13.5762 11.8038 13.9062 12.0838C13.9862 12.1638 14.0562 12.2238 14.0762 12.2338C14.2662 12.3938 14.4662 12.5538 14.6362 12.6638C14.8462 12.8138 15.0562 12.9538 15.2762 13.0838Z" fill="#87868C"/>
                                                    </svg>
                                                `
                            return editButtonHTML;
                        }

                        return "";
                    },
                    "autoWidth": true
                },
            ],
        });
    }

    function Init() {
        if (root.parentEl?.length > 0) {
            Cache()
            Events()

            PreLoad()
        } else {
            console.error("Không tìm thấy Input storage detail container")
        }
    }

    return {
        init: Init,
        loadDataTable: LoadDatatable
    }
})();