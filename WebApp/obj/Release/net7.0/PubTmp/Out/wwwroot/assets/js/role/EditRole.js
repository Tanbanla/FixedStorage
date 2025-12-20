
$(function () {
    EditRoleController.init()
})


var EditRoleController = (function () {
    let root = {}

    function EditRoleAPI(model) {
        return new Promise(async (resolve, reject) => {
            //let url = `${host}/api/identity/role/edit`
            let url = `role/edit`

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

    function Events() {
        //Toggle web access events
        root.$editRoleForm.find("#WebAccess input[name='permission[]']:eq(0)").click((e) => {
            let target = $(e.target)
            let isChecked = target.is(':checked')

            if (!isChecked) {
                root.$editRoleForm.find("#WebAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").prop("checked", false)
            }
            root.$editRoleForm.find("#WebAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").attr("disabled", !isChecked)

            if (!isChecked) {
                root.$editRoleForm.find("#WebAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").closest(".form-check").removeClass("role_uncheck").addClass("role_uncheck")
                root.$websiteTitle.removeClass("txt-green")
            } else {
                root.$editRoleForm.find("#WebAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").closest(".form-check").removeClass("role_uncheck")
                root.$websiteTitle.removeClass("txt-green").addClass("txt-green")
            }

            DisabledWebsiteAndMobile();
        })

        root.$editRoleForm.find("#MobileAccess input[name='permission[]']:eq(0)").click((e) => {
            let target = $(e.target)
            let isChecked = target.is(':checked')

            if (!isChecked) {
                root.$editRoleForm.find("#MobileAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").prop("checked", false)
            }
            root.$editRoleForm.find("#MobileAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").attr("disabled", !isChecked)

            if (!isChecked) {
                root.$editRoleForm.find("#MobileAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").closest(".form-check").removeClass("role_uncheck").addClass("role_uncheck")
                root.$mobileTitle.removeClass("txt-green")
            } else {
                root.$editRoleForm.find("#MobileAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").closest(".form-check").removeClass("role_uncheck")
                root.$mobileTitle.removeClass("txt-green").addClass("txt-green")
            }

            DisabledWebsiteAndMobile();
        })

        root.$btn_modal_edit_role.click((e) => {
            var checkOpenMsg = $("#toast-container .toast-error .toast-message").text();
            let webAccessCreateRole = root.$editRoleForm.find(".WebAccessToggle")
            let mobileAccessRole = root.$editRoleForm.find(".MobileAccessToggle")

            let webAccessChecked = webAccessCreateRole.is(":checked")
            let mobileAccessChecked = mobileAccessRole.is(":checked")

            let webPermissionCount = root.$editRoleForm.find("#WebAccess .permission_list input[name='permission[]']:checked").length;
            let mobilePermissionCount = root.$editRoleForm.find("#MobileAccess .permission_list input[name='permission[]']:checked").length;

            if (!webAccessChecked && !mobileAccessChecked) {
                toastr.error(window.languageData[window.currentLanguage]["Vui lòng lựa chọn quyền truy cập."])
                return;
            } else if (webAccessChecked && mobileAccessChecked && webPermissionCount == 0 && mobilePermissionCount == 0) {
                //toastr.error("Vui lòng lựa chọn quyền truy cập trên website.")
                //toastr.error("Vui lòng lựa chọn quyền truy cập trên mobile.")
                toastr.error(window.languageData[window.currentLanguage]["Vui lòng lựa chọn quyền truy cập trên website và mobile."])
                return;
            } else if (webAccessChecked && webPermissionCount == 0) {
                toastr.error(window.languageData[window.currentLanguage]["Vui lòng lựa chọn quyền truy cập trên website."])
                return;
            } else if (mobileAccessChecked && mobilePermissionCount == 0) {
                toastr.error(window.languageData[window.currentLanguage]["Vui lòng lựa chọn quyền truy cập trên mobile."])
                return;
            }

            let validForm = root.$editRoleForm.valid()
            let roleId = root.$container.data("roleId")
            
            if (validForm) {
                let checkedPermissions = root.$editRoleForm.find("input[name='permission[]']")

                if (checkedPermissions?.length > 0) {
                    let permissionsModel = []
                    let preparePermissions = checkedPermissions.map((i, el) => {
                        permissionsModel.push({
                            Category: $(el).data("category"),
                            Name: $(el).val(),
                            Active: $(el).is(":checked")
                        })
                    })

                    var prepareModel = {
                        Name: root.$editRoleForm.find("input[name='Name']").val(),
                        RoleId: roleId,
                        UserId: $("#App_UserId").val(),
                        Permissions: permissionsModel
                    }

                    //Call API
                    root.$btn_modal_edit_role.buttonLoader('start');
                    loading(true);
                    EditRoleAPI(prepareModel).then(res => {
                        //RoleController.render();
                        //toastr.success(res?.message)
                        ReloadPage(0);
                    }).catch(err => {
                        root.$btn_modal_edit_role.buttonLoader('stop');
                       toastr.error(window.languageData[window.currentLanguage][err?.responseJSON?.message])
                    }).finally(() => {
                        loading(false);
                    })
                }
            }
        })

        //Validate input
        root.$editRoleForm.delegate("input[name='Name']", "keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50))
        root.$editRoleForm.delegate("input[name='Name']", "change", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50))

        root.$editRoleForm.delegate("input[name='Name']", "blur", ValidateInputHelper.TrimWhiteSpaceOnBlur)

        //On create modal show
        $(window).on('shown.bs.modal', function (e) {
            let target = e.target;
            let isEditModal = $(target).is("#Views_Shared_Common_Role__EditRoleView");
            if (isEditModal) {
                root.$editFormValidator.resetForm();
            }
        });

        //Bật tắt xem sửa dữ liệu master data
        $("#editRoleForm input[value='MASTER_DATA_READ']").change((e) => {
            let isChecked = $(e.target).is(":checked");
            if (!isChecked) {
                $("#editRoleForm input[value='MASTER_DATA_WRITE']").prop("checked", false);
            }
        })
        $("#editRoleForm input[value='MASTER_DATA_WRITE']").change((e) => {
            let isChecked = $(e.target).is(":checked");
            $("#editRoleForm input[value='MASTER_DATA_READ']").prop("checked", isChecked);
        })

        //Phần tick kiểm kê
        $("#editRoleForm input[value='VIEW_ALL_INVENTORY']").change(function (e) {
            let isChecked = $(e.target).is(":checked");
            $("#editRoleForm input[value='VIEW_CURRENT_INVENTORY']").prop("checked", isChecked);

            if (!isChecked) {
                $("#editRoleForm input[value='EDIT_INVENTORY']").prop("checked", false);
            }
        })

        $("#editRoleForm input[value='VIEW_CURRENT_INVENTORY']").change(function (e) {
            let isChecked = $(e.target).is(":checked");

            if (!isChecked) {
                $("#editRoleForm input[value='VIEW_ALL_INVENTORY']").prop("checked", false);
            }
        })

        $("#editRoleForm input[value='EDIT_INVENTORY']").change(function (e) {
            let isChecked = $(e.target).is(":checked");

            if (isChecked) {
                $("#editRoleForm input[value='VIEW_CURRENT_INVENTORY']").prop("checked", isChecked);
            }
        })

        $("#editRoleForm input[value='VIEW_CURRENT_INVENTORY']").change(function (e) {
            let isChecked = $(e.target).is(":checked");

            if (!isChecked) {
                $("#editRoleForm input[value='EDIT_INVENTORY']").prop("checked", isChecked);
            }
        })

        $("#editRoleForm input[value='SUBMIT_INVESTIGATION_DETAIL']").change(function (e) {
            let isChecked = $(e.target).is(":checked");

            if (isChecked) {
                $("#editRoleForm input[value='VIEW_INVESTIGATION_DETAIL']").prop("checked", isChecked);
            }
        })

        $("#editRoleForm input[value='CONFIRM_INVESTIGATION_DETAIL']").change(function (e) {
            let isChecked = $(e.target).is(":checked");

            if (isChecked) {
                $("#editRoleForm input[value='VIEW_INVESTIGATION_DETAIL']").prop("checked", isChecked);
            }
        })

    }

    function DisabledWebsiteAndMobile() {
        let websiteToggleChecked = root.$webAccessToggle.is(":checked");
        let mobileToggleChecked = root.$mobileAccessToggle.is(":checked");

        if (!websiteToggleChecked && !mobileToggleChecked) {
            //Tắt các checkbox xem dữ liệu theo phòng ban
            root.$editRoleForm.find(`input[data-category="DEPARTMENT_DATA_INQUIRY"]`).prop("checked", false).attr("disabled", true);
            root.$editRoleForm.find(`input[data-category="DEPARTMENT_DATA_INQUIRY"]`).closest(".form-check").removeClass("role_uncheck").addClass("role_uncheck");

            //Tắt các checkbox xem dữ liệu theo nhà máy
            root.$editRoleForm.find(`input[data-category="FACTORY_DATA_INQUIRY"]`).prop("checked", false).attr("disabled", true);
            root.$editRoleForm.find(`input[data-category="FACTORY_DATA_INQUIRY"]`).closest(".form-check").removeClass("role_uncheck").addClass("role_uncheck");
        } else {
            //Nếu bật website / mobile thì bật lại các checkbox xem dữ liệu phòng ban
            root.$editRoleForm.find(`input[data-category="DEPARTMENT_DATA_INQUIRY"]`).attr("disabled", false);
            root.$editRoleForm.find(`input[data-category="DEPARTMENT_DATA_INQUIRY"]`).closest(".form-check").removeClass("role_uncheck");
            //Nếu bật website / mobile thì bật lại các checkbox xem dữ liệu nhà máy
            root.$editRoleForm.find(`input[data-category="FACTORY_DATA_INQUIRY"]`).attr("disabled", false);
            root.$editRoleForm.find(`input[data-category="FACTORY_DATA_INQUIRY"]`).closest(".form-check").removeClass("role_uncheck");
        }
    }

    function DisablePermission() {
        root.$editRoleForm.find("#WebAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").closest(".form-check").removeClass("role_uncheck").addClass("role_uncheck")
        root.$editRoleForm.find("#MobileAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").closest(".form-check").removeClass("role_uncheck").addClass("role_uncheck")
    }

    function Cache() {
        root.$container = $("#Views_Shared_Common_Role__EditRoleView");
        root.$btn_modal_edit_role = root.$container.find("#btn_modal_edit_role");
        root.$editRoleForm = root.$container.find("#editRoleForm");

        root.$websiteTitle = root.$editRoleForm.find(".website_title");
        root.$mobileTitle = root.$editRoleForm.find(".mobile_title");

        root.$webAccessToggle = root.$editRoleForm.find(".WebAccessToggle");
        root.$mobileAccessToggle = root.$editRoleForm.find(".MobileAccessToggle");
    }
    function Preload() {
        DisablePermission()

        jQuery.validator.addMethod("edit_role_existname", function (value, element) {
            var host = $("#APIGateway").val()
            var url = `${host}/api/identity/role/exist-name/${value}`
            var valid = true

            let isNameChanged = value.toLowerCase() !== $(element).attr("data-root").toLowerCase()
            if (isNameChanged) {
                $.ajax({
                    type: "GET",
                    url: url,
                    async: false,
                    success: function (res) {
                        valid = !res.data
                    }
                });
            }
            return valid
        }, 'Tên nhóm quyền đã tồn tại. Vui lòng thử lại');

        jQuery.validator.addMethod("Edit_RequiredPermissionAtLeast", function (value, element) {
            let valid = true
            let webAccessCreateRole = root.$editRoleForm.find(".WebAccessToggle")
            let mobileAccessRole = root.$editRoleForm.find(".MobileAccessToggle")

            let webAccessChecked = webAccessCreateRole.is(":checked")
            let mobileAccessChecked = mobileAccessRole.is(":checked")
            if (webAccessChecked) {
                let atLeastWebChecked = root.$editRoleForm.find("#WebAccess [name='permission[]']:not(.WebAccessToggle):checked").length >= 1
                if (atLeastWebChecked == false) {
                    valid = false
                }
            }
            if (mobileAccessChecked) {
                let atLeastMobileChecked = root.$editRoleForm.find("#MobileAccess [name='permission[]']:not(.MobileAccessToggle):checked").length >= 1
                if (atLeastMobileChecked == false) {
                    valid = false
                }
            }

            if (!webAccessChecked && !mobileAccessChecked) {
                valid = false
            }

            return valid
        }, () => {
            /* let webAccessCreateRole = root.$editRoleForm.find(".WebAccessToggle")
            let mobileAccessRole = root.$editRoleForm.find(".MobileAccessToggle")

            let webAccessChecked = webAccessCreateRole.is(":checked")
            let mobileAccessChecked = mobileAccessRole.is(":checked")

            if (!webAccessChecked && !mobileAccessChecked) {
                toastr.error("Vui lòng lựa chọn quyền truy cập.")
                return
            }else if(webAccessChecked && mobileAccessChecked ) {
                toastr.error("Vui lòng lựa chọn quyền truy cập trên website.")
                toastr.error("Vui lòng lựa chọn quyền truy cập trên mobile.")
                return
            } else if (webAccessChecked) {
                toastr.error("Vui lòng lựa chọn quyền truy cập trên website.")
                return 
            } else if (mobileAccessChecked) {
                toastr.error("Vui lòng lựa chọn quyền truy cập trên mobile.")
                return
            } */
        });

        let validateEditModel = {
            ignore: [],
            rules: {
                'Name': {
                    required: true,
                    edit_role_existname: true
                },
                'permission[]': {
                    Edit_RequiredPermissionAtLeast: true
                }
            },
            messages: {
                "Name": {
                    required: window.languageData[window.currentLanguage]["Vui lòng nhập tên nhóm quyền."]
                },
            },
            errorLabelContainer: $('span[data-valmsg-for="Name"]'),
            wrapper: "li"
        }

        //Vaidate form
        root.$editFormValidator = root.$editRoleForm.validate(validateEditModel);
    }

    function waitForEditRoleLanguageData() {
        // Kiểm tra nếu dữ liệu đã sẵn sàng
        if (window.languageData && window.currentLanguage) {
            Cache()
            Preload()

            Events()

        } else {
            // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
            setTimeout(waitForEditRoleLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
        }
    }

    function Init() {
        waitForEditRoleLanguageData();
    }
    return {
        init: Init
    }
})()




