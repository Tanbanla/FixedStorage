 const host = $("#APIGateway").val();

//toastr.success("Đánh dấu đã đọc");
//Swal.fire(
//    'Good job!',
//    'You clicked the button!',
//    'success'
//)

$(function () {
    DepertmentController.init();

})

var DepertmentController = (function () {
    let root = {
        parentEl: $(".Views_Departments_Index"),
    }

    let createValidator;
    let editValidator;
    const selectPlaceHolderText = "Tìm kiếm...";

    function PrepareEditAPI() {
        return new Promise(async (resolve, reject) => {
            try {
                var res = await $.get("edit/prepare")
                resolve(res);
            } catch {
            }
        })
    }

    function PostCreateDepartmentAPI(model) {
        return new Promise(async (resolve, reject) => {
            let url = `${host}/api/identity/department/create`

            try {
                const res = await $.ajax({
                    url: url,
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(model)
                });
                resolve(res)
            } catch (err) {
                reject(err)
            }
        })
    }

    function EditDepartmentAPI(model) {
        return new Promise(async (resolve, reject) => {
            let url = `${host}/api/identity/department/edit`

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
    function DepartmentListAPI() {
        return new Promise(async (resolve, reject) => {
            try {
                let url = `${host}/api/identity/department`
                var res = await $.get(url)
                resolve(res)
            } catch {
                reject()
            }
        })
    }
    function PepareDeleteAPI(deparmentId) {
        return new Promise(async (resolve, reject) => {
            let url = `${host}/api/identity/department/delete/prepare/${deparmentId}`
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

    function DeleteAPI(deparmentId, userId) {
        return new Promise(async (resolve, reject) => {
            let url = `${host}/api/identity/department/delete/${deparmentId}/${userId}`
            try {
                const res = await $.ajax({
                    url: url,
                    type: 'DELETE',
                    contentType: 'application/json',
                });
                resolve(res)
            } catch (err) {
                reject(err)
            }
        })
    }

    function Events() {
        //Delete department
        root.parentEl.delegate(".btnDeleteDepartment", "click", (e) => {
            /*root.deleteItemModal.modal("show")*/
            let departmentId = $(e.target).closest(".btnDeleteDepartment").attr("data-value")
            let userId = $("#App_UserId").val();

            Swal.fire({
                title: `<b>${window.languageData[window.currentLanguage]["Xác nhận xóa"]}</b>`,
                text: window.languageData[window.currentLanguage]["Bạn có chắc chắn muốn xóa phòng ban này?"],
                confirmButtonText: window.languageData[window.currentLanguage]["Đồng ý"],
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
                    DeleteAPI(departmentId, userId).then(res => {
                        RenderList();
                        //ReloadPage(2500);
                        toastr.success(window.languageData[window.currentLanguage][res?.message]);
                    }).catch(err => {
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]["Không thể xóa"]}</b>`,
                            text: err?.responseJSON?.message,
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        })
                    })
                }
            })
        })

        root.btnConfirmDelete.click((e) => {
            //goi API delete
            //PrepareDelete()
            //root.deleteItemModal.modal("hide")
        })

        root.parentEl.delegate(".btnEditDepartment", "click", (e) => {
            PrepareEditAPI().then(() => {
                root.editItemModal.modal("show")

                let itemCardContainer = $(e.target).closest(".department_item")

                //root.editItemModal.find("select[name='ManagerId']").val("").trigger("change");

                //Fill Id
                let departmentId = itemCardContainer.find("[name='DepartmentId']").val()
                root.editItemForm.find("input[name='DepartmentId']").val(departmentId)
                //Fill Name
                let departmentName = itemCardContainer.find("[name='Name']").attr("data-value")
                root.editItemForm.find("input[name='Name']").attr("data-root", departmentName)
                root.editItemForm.find("input[name='Name']").val(departmentName)
                //Fill select
                let managerId = itemCardContainer.find("[name='ManagerId']").attr("data-value")
                root.editItemForm.find("select[name='ManagerId']").val(managerId || "").change()
            })
        })

        root.btnEditModal.click((e) => {
            let validForm = root.editItemForm.valid()
            if (validForm) {
                let model = {
                    DepartmentId: root.editItemForm.find("input[name='DepartmentId']").val(),
                    UserId: $("#App_UserId").val(),
                    Name: root.editItemForm.find("input[name='Name']").val(),
                    ManagerId: root.editItemForm.find("select[name='ManagerId']").val()
                }

                root.btnEditModal.buttonLoader('start');
                loading(true);
                EditDepartmentAPI(model).then((res) => {
                    root.btnEditModal.buttonLoader('stop');
                    root.editItemModal.modal("hide");

                    RenderList();
                    toastr.success(window.languageData[window.currentLanguage][res.message])
                    //ReloadPage(2000)
                }).catch(err => {
                    // toastr.error(err?.responseJSON?.message)
                    root.btnEditModal.buttonLoader('stop');
                    if (editValidator && typeof editValidator.showErrors === "function") {
                        if (err?.responseJSON?.data.hasOwnProperty("Name")) {
                            editValidator.showErrors({
                                "Name": window.languageData[window.currentLanguage][err?.responseJSON?.data.Name]
                            });
                        }
                    }
                    
                }).finally(() => {
                    loading(false);
                })
            }
        })

        root.btnCreateModal.click((e) => {
            let validForm = root.createForm.valid();
            if (validForm) {
                let createDepartmentModel = {
                    UserId: $("#App_UserId").val(),
                    Name: root.createForm.find("input[name='Name']").val(),
                    ManagerId: root.createForm.find("select[name='ManagerId']").val()
                }

                root.btnCreateModal.StartLoading();
                loading(true);
                PostCreateDepartmentAPI(createDepartmentModel).then((res) => {
                    root.btnCreateModal.StopLoading();
                    root.creatItemModal.modal("hide");

                    RenderList();
                    toastr.success(window.languageData[window.currentLanguage][res?.message]);
                    //ReloadPage(2500);
                }).catch((err) => {
                    root.btnCreateModal.StopLoading();
                    if (createValidator && typeof createValidator.showErrors === "function") {
                        if (err?.responseJSON?.data.hasOwnProperty("Name")) {
                            createValidator.showErrors({
                                "Name": window.languageData[window.currentLanguage][err?.responseJSON?.data.Name]
                            });
                        }
                    }
                    
                }).finally(() => {
                    loading(false);
                })
            }
        })
        //Validate input
        //root.createForm.delegate("input[name='Name']", "keypress", ValidateInputHelper.PreventSepcialCharacterOnKeyPress)
        root.createForm.delegate("input[name='Name']", "blur", ValidateInputHelper.TrimWhiteSpaceOnBlur);

        //root.editItemForm.delegate("input[name='Name']", "keypress", ValidateInputHelper.PreventSepcialCharacterOnKeyPress)
        root.editItemForm.delegate("input[name='Name']", "blur", ValidateInputHelper.TrimWhiteSpaceOnBlur);

        //On create modal show
        $(window).on('shown.bs.modal', function (e) {
            let target = e.target;
            let isCreateModal = $(target).is(root.creatItemModal);
            let isEditModal = $(target).is(root.editItemModal);
            if (isCreateModal) {
                root.creatItemModal.find("#Name").val("");
                root.creatItemModal.find("#ManagerId").val("").change();

                if (createValidator && typeof createValidator.resetForm === "function") {
                    createValidator.resetForm();
                }

            } else if (isEditModal) {
                if (editValidator && typeof editValidator.resetForm === "function") {
                    editValidator.resetForm();
                }
            }
        });

        root.createForm.delegate("input[name='Name']", "keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50));
        root.createForm.delegate("input[name='Name']", "change", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50));

        root.editItemForm.delegate("input[name='Name']", "keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50));
        root.editItemForm.delegate("input[name='Name']", "change", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50));

        root.editItemForm.delegate("input[name='Name']", "keyup", (e) => {
            root.editItemForm.valid();
        });

        root.createForm.delegate("input[name='Name']", "keyup", (e) => {
            root.createForm.valid();
        });
    }

    function RenderList() {
        DepartmentListAPI().then((res) => {
            let departments = res?.data || [];
            if (departments.length > 0) {
                let resultHtml = departments.map(item => {
                    return `
                        <div class="department_item h100 rounded p-3">
                            <div class="row">
                                <input hidden name="DepartmentId" value="${item?.id}" />
                                <div class="col-9">
                                    <h3 class="h4 txt-blue txt-heading txt-breakline" name="Name" data-value="${item?.name}">${item?.name}</h3>
                                </div>
                                <div class="col-3">
                                    <div class="row">
                                        <div class="col-6">
                                            <a data-href="#" class="btnEditDepartment">
                                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none">
                                                    <path fill-rule="evenodd" clip-rule="evenodd" d="M13.6962 5.47381C15.6862 3.48381 17.5862 3.53381 19.5262 5.47381C20.5262 6.46381 21.0062 7.42381 20.9962 8.41381C20.9962 9.37381 20.5162 10.3238 19.5262 11.3038L18.3262 12.5138C18.2462 12.5938 18.1462 12.6338 18.0362 12.6338C17.9962 12.6338 17.9562 12.6238 17.9162 12.6138C15.2662 11.8538 13.1462 9.73381 12.3862 7.08381C12.3462 6.94381 12.3862 6.78381 12.4862 6.68381L13.6962 5.47381ZM15.2762 13.0838C15.5462 13.2438 15.8262 13.3838 16.1162 13.5238C16.1551 13.5407 16.1935 13.5572 16.2315 13.5734C16.5691 13.7175 16.667 14.1629 16.4075 14.4225L10.6862 20.1438C10.5662 20.2738 10.3162 20.3938 10.1362 20.4238L6.29618 20.9638C6.17618 20.9838 6.05618 20.9938 5.93618 20.9938C5.39618 20.9938 4.89618 20.8038 4.53618 20.4538C4.11618 20.0238 3.92618 19.3838 4.02618 18.7038L4.56618 14.8738C4.59618 14.7038 4.71618 14.4538 4.84618 14.3238L10.5746 8.59534C10.8324 8.33758 11.267 8.43503 11.4146 8.76834C11.4345 8.81325 11.455 8.85841 11.4762 8.90381C11.6162 9.18381 11.7562 9.45381 11.9162 9.72381C12.0462 9.94381 12.1862 10.1638 12.3062 10.3138C12.4463 10.5286 12.6038 10.7172 12.7054 10.839C12.7126 10.8476 12.7196 10.8559 12.7262 10.8638C12.7403 10.882 12.7537 10.8994 12.7662 10.9157C12.8156 10.9801 12.8522 11.0279 12.8762 11.0438C13.2062 11.4438 13.5762 11.8038 13.9062 12.0838C13.9862 12.1638 14.0562 12.2238 14.0762 12.2338C14.2662 12.3938 14.4662 12.5538 14.6362 12.6638C14.8462 12.8138 15.0562 12.9538 15.2762 13.0838Z" fill="#87868C"/>
                                                </svg>
                                            </a>
                                        </div>
                                        <div class="col-6">
                                            <a class="btnDeleteDepartment" data-value="${item?.id}">
                                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none">
                                                    <path d="M21.07 5.23C19.46 5.07 17.85 4.95 16.23 4.86V4.85L16.01 3.55C15.86 2.63 15.64 1.25 13.3 1.25H10.68C8.35001 1.25 8.13 2.57 7.97 3.54L7.76 4.82C6.83001 4.88 5.9 4.94 4.97 5.03L2.93001 5.23C2.51001 5.27 2.21 5.64 2.25 6.05C2.29 6.46 2.65 6.76 3.07 6.72L5.11001 6.52C10.35 6 15.63 6.2 20.93 6.73C20.96 6.73 20.98 6.73 21.01 6.73C21.39 6.73 21.72 6.44 21.76 6.05C21.79 5.64 21.49 5.27 21.07 5.23Z" fill="#E60000"/>
                                                    <path d="M19.23 8.14C18.99 7.89 18.66 7.75 18.32 7.75H5.67999C5.33999 7.75 4.99999 7.89 4.76999 8.14C4.53999 8.39 4.40999 8.73 4.42999 9.08L5.04999 19.34C5.15999 20.86 5.29999 22.76 8.78999 22.76H15.21C18.7 22.76 18.84 20.87 18.95 19.34L19.57 9.09C19.59 8.73 19.46 8.39 19.23 8.14ZM13.66 17.75H10.33C9.91999 17.75 9.57999 17.41 9.57999 17C9.57999 16.59 9.91999 16.25 10.33 16.25H13.66C14.07 16.25 14.41 16.59 14.41 17C14.41 17.41 14.07 17.75 13.66 17.75ZM14.5 13.75H9.49999C9.08999 13.75 8.74999 13.41 8.74999 13C8.74999 12.59 9.08999 12.25 9.49999 12.25H14.5C14.91 12.25 15.25 12.59 15.25 13C15.25 13.41 14.91 13.75 14.5 13.75Z" fill="#E60000"/>
                                                </svg>
                                            </a>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-6">
                                    <label class="txt-bolder">${window.languageData[window.currentLanguage]["Số lượng nhân viên"]}: </label>
                                </div>
                                <div class="col-6 txt-green ">${item?.members || 0}</div>
                            </div>
                            <div class="row" name="ManagerId" data-value="${item?.managerId || ""}">
                                <div class="col-6">
                                    <label class="txt-bolder">${window.languageData[window.currentLanguage]["Trưởng phòng"]}: </label>
                                </div>
                                <div class="col-6 txt-light-blue ">${item?.managerName || ""}</div>
                            </div>
                        </div>
                    `
                }).join("");
                root.department_list.html(resultHtml);
                Cache();
            }
        });
    }

    function Cache() {
        root.btnDeleteItem = root.parentEl.find(".btnDeleteDepartment")
        root.btnEditItem = root.parentEl.find(".btnEditDepartment")

        root.creatItemModal = $("#Views_Shared_Common__NewDepartmentModal")
        root.btnCreateModal = root.creatItemModal.find("#btnConfirmCreateDepartment")
        root.createForm = root.creatItemModal.find("#newDepartmentForm")

        root.deleteItemModal = $("#Views_Shared_Common__DeleteDepartmentModal")
        root.btnConfirmDelete = root.deleteItemModal.find("#btnConfirmDelete")

        root.editItemModal = $("#Views_Shared_Common__EditDepartmentModal")
        root.btnEditModal = root.editItemModal.find("#btnEdit")
        root.editItemForm = root.editItemModal.find("#editItemForm")

        root.editModal_select_manager = root.editItemModal.find("select#ManagerId")
        root.createModal_select_manager = root.creatItemModal.find("select#ManagerId")

        root.department_list = root.parentEl.find(".department_list")
    }

    function PreLoad() {
        jQuery.validator.addMethod("existDepartmentName", function (value, element) {
            var host = $("#APIGateway").val()
            var url = `${host}/api/identity/department/exist-department/${value}`
            var valid = true
            $.ajax({
                type: "GET",
                url: url,
                async: false,
                success: function (res) {
                    valid = !res.data
                }
            });
            return valid
        }, 'Phòng ban này đã tồn tại, vui lòng nhập lại.');


        //jQuery.validator.addMethod("existNameForEditDepartment", function (value, element) {
        //    var host = $("#APIGateway").val()
        //    var url = `${host}/api/identity/department/exist-department/${value}`
        //    var valid = true

        //    let isNameChanged = value.toLowerCase() !== $(element).attr("data-root").toLowerCase()
        //    if (isNameChanged) {
        //        $.ajax({
        //            type: "GET",
        //            url: url,
        //            async: false,
        //            success: function (res) {
        //                valid = !res.data
        //            }
        //        });
        //    }
        //    return valid
        //}, 'Tên phòng ban đã tồn tại');
        let validateCreateModel = {
            rules: {
                Name: {
                    required: true,
                    //existDepartmentName: true,
                    maxlength: 50
                }
            },
            messages: {
                "Name": {
                    required: window.languageData[window.currentLanguage]["Vui lòng nhập tên phòng ban."],
                    maxlength: window.languageData[window.currentLanguage]["Hệ thống chỉ cho phép nhập tối đa 50 kí tự"]
                }
            }
        }


        let validateEditModel = {
            rules: {
                "Name": {
                    required: true,
                    //existNameForEditDepartment: true,
                    maxlength: 50,
                }
            },
            messages: {
                "Name": {
                    required: window.languageData[window.currentLanguage]["Vui lòng nhập tên phòng ban."],
                    maxlength: window.languageData[window.currentLanguage]["Hệ thống chỉ cho phép nhập tối đa 50 kí tự"]
                }
            }
        }

        //Vaidate form
        createValidator = root.createForm.validate(validateCreateModel);
        editValidator = root.editItemForm.validate(validateEditModel);


        root.createModal_select_manager?.select2({
            dropdownParent: root.creatItemModal,
        });
        //Init select2
        root.editModal_select_manager?.select2({
            //dropdownParent: root.editItemModal,
            dropdownParent: root.editItemForm,
        });

        root.editModal_select_manager.on("select2:open", (e) => {
            $(".select2-search__field").attr("placeholder", selectPlaceHolderText)
            //let offset = $(".edit_department_select_container").offset().top;
            let offset = $(e.currentTarget).offset().top;
            $(".select2-dropdown--below").offset({ top: offset + 10 });

            let selectMagnify = $(`<div class='select_magnify'>
                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none">
                                      <path d="M11.5 21C16.7467 21 21 16.7467 21 11.5C21 6.25329 16.7467 2 11.5 2C6.25329 2 2 6.25329 2 11.5C2 16.7467 6.25329 21 11.5 21Z" stroke="#87868C" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
                                      <path d="M22 22L20 20" stroke="#87868C" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
                                    </svg>
                                </div>`)
            $(".select_magnify").remove()
            $(".select2-search__field").after(selectMagnify)
        })

        root.createModal_select_manager.on("select2:open", () => {
            $(".select2-search__field").attr("placeholder", selectPlaceHolderText)

            let selectMagnify = $(`<div class='select_magnify'>
                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none">
                                      <path d="M11.5 21C16.7467 21 21 16.7467 21 11.5C21 6.25329 16.7467 2 11.5 2C6.25329 2 2 6.25329 2 11.5C2 16.7467 6.25329 21 11.5 21Z" stroke="#87868C" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
                                      <path d="M22 22L20 20" stroke="#87868C" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
                                    </svg>
                                </div>`)
            $(".select_magnify").remove()
            $(".select2-search__field").after(selectMagnify)
        })

    }

    function waitForDepartmentLanguageData() {
        // Kiểm tra nếu dữ liệu đã sẵn sàng
        if (window.languageData && window.currentLanguage) {
            Cache()
            PreLoad()
            Events()

        } else {
            // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
            setTimeout(waitForDepartmentLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
        }
    }

    function Init() {
        if (root.parentEl?.length <= 0) {
            return
        }
        
        waitForDepartmentLanguageData()
    }

    return {
        init: Init
    }
})()

