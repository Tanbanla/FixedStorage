
$(function () {
    CreatRoleController.init()
})


var CreatRoleController = (function () {
    let root = {}
    function CreatRoleAPI(model) {
        return new Promise(async (resolve, reject) => {
            //let url = `${host}/api/identity/role/create`
            let url = `role/create`;

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

    function Cache() {
        root.$container = $("#Views_Shared_Common_Role__CreateRoleView");
        root.$btn_modal_create_role = root.$container.find("#btn_modal_create_role");
        root.$createRoleForm = root.$container.find("#createRoleForm");

        root.$websiteTitle = root.$createRoleForm.find(".website_title");
        root.$mobileTitle = root.$createRoleForm.find(".mobile_title");

        root.$webAccessToggle = root.$createRoleForm.find(".WebAccessToggle");
        root.$mobileAccessToggle = root.$createRoleForm.find(".MobileAccessToggle");
    }

    function Events() {
        //Toggle web access events
        root.$createRoleForm.find(".WebAccessToggle").click((e) => {
            let target = $(e.target).closest(".WebAccessToggle")
            let isChecked = target.is(':checked')

            if (!isChecked) {
                root.$createRoleForm.find("#WebAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").prop("checked", false)
            }
            root.$createRoleForm.find("#WebAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").attr("disabled", !isChecked)

            if (!isChecked) {
                root.$createRoleForm.find("#WebAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").closest(".form-check").removeClass("role_uncheck").addClass("role_uncheck")
                root.$websiteTitle.removeClass("txt-green")
            } else {
                root.$createRoleForm.find("#WebAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").closest(".form-check").removeClass("role_uncheck")
                root.$websiteTitle.removeClass("txt-green").addClass("txt-green")
            }

            DisabledWebsiteAndMobile();
        })

        root.$createRoleForm.find(".MobileAccessToggle").click((e) => {
            let target = $(e.target).closest(".MobileAccessToggle")
            let isChecked = target.is(':checked')

            if (!isChecked) {
                root.$createRoleForm.find("#MobileAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").prop("checked", false)
            }
            root.$createRoleForm.find("#MobileAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").attr("disabled", !isChecked)

            if (!isChecked) {
                root.$createRoleForm.find("#MobileAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").closest(".form-check").removeClass("role_uncheck").addClass("role_uncheck")
                root.$mobileTitle.removeClass("txt-green")
            } else {
                root.$createRoleForm.find("#MobileAccess input[name='permission[]']:not(input[name='permission[]']:eq(0))").closest(".form-check").removeClass("role_uncheck")
                root.$mobileTitle.removeClass("txt-green").addClass("txt-green")
            }

            DisabledWebsiteAndMobile();
        })

        root.$btn_modal_create_role.click((e) => {
            var checkOpenMsg = $("#toast-container .toast-error .toast-message").text();
            let webAccessCreateRole = root.$createRoleForm.find(".WebAccessToggle")
            let mobileAccessRole = root.$createRoleForm.find(".MobileAccessToggle")

            let webAccessChecked = webAccessCreateRole.is(":checked")
            let mobileAccessChecked = mobileAccessRole.is(":checked")

            let webPermissionCount = root.$createRoleForm.find("#WebAccess .permission_list input[name='permission[]']:checked").length;
            let mobilePermissionCount = root.$createRoleForm.find("#MobileAccess .permission_list input[name='permission[]']:checked").length;

            let validForm = root.$createRoleForm.valid()
            //let toastOptions = {
            //    maxOpened: 1,
            //    preventDuplicates: 1,
            //    autoDismiss: true
            //};
           
            if (!webAccessChecked && !mobileAccessChecked) {
                toastr.error(window.languageData[window.currentLanguage]["Vui lòng lựa chọn quyền truy cập."])
                return;
            } else if (webAccessChecked && mobileAccessChecked && webPermissionCount == 0 && mobilePermissionCount == 0) {
                toastr.error(window.languageData[window.currentLanguage]["Vui lòng lựa chọn quyền truy cập trên website và mobile."])
                //toastr.error("Vui lòng lựa chọn quyền truy cập trên mobile.")
                return;
            } else if (webAccessChecked && webPermissionCount == 0) {
                toastr.error(window.languageData[window.currentLanguage]["Vui lòng lựa chọn quyền truy cập trên website."])
                return;
            } else if (mobileAccessChecked && mobilePermissionCount == 0) {
                toastr.error(window.languageData[window.currentLanguage]["Vui lòng lựa chọn quyền truy cập trên mobile."])
                return;
            }

            if (validForm) {
                let checkedPermissions = root.$createRoleForm.find("input[name='permission[]']:checked")

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
                        Name: root.$createRoleForm.find("input[name='Name']").val(),
                        UserId: $("#App_UserId").val(),
                        Permissions: permissionsModel
                    }

                    //Call API
                    root.$btn_modal_create_role.buttonLoader('start');
                    loading(true);
                    CreatRoleAPI(prepareModel).then(res => {
                        ReloadPage(0);
                    }).catch(err => {
                        toastr.error(err?.responseJSON?.message)
                        root.$btn_modal_create_role.buttonLoader('stop');
                    }).finally(() => {
                        loading(false);
                    })
                }
            }
        })

        //Validate input
        //root.$createRoleForm.delegate("input[name='Name']", "keypress", ValidateInputHelper.PreventSepcialCharacterOnKeyPress)
        root.$createRoleForm.delegate("input[name='Name']", "blur", ValidateInputHelper.TrimWhiteSpaceOnBlur)

        root.$createRoleForm.delegate("input[name='Name']", "keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50))
        root.$createRoleForm.delegate("input[name='Name']", "change", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50))

        //On create modal show
        $(window).on('shown.bs.modal', function (e) {
            let target = e.target;
            let isCreateModal = $(target).is("#Views_Shared_Common_Role__CreateRoleView");
            if (isCreateModal) {
                root.$createFormValidator.resetForm();
            }

            //enable website access toggle
            root.$webAccessToggle.prop("checked", true).change();
            //enable mobile access toggle
            root.$mobileAccessToggle.prop("checked", true).change();
        });

        //Bật tắt xem sửa dữ liệu master data
        $("#createRoleForm input[value='MASTER_DATA_READ']").change((e) => {
            let isChecked = $(e.target).is(":checked");
            if (!isChecked) {
                $("#createRoleForm input[value='MASTER_DATA_WRITE']").prop("checked", false);
            }
        })
        $("#createRoleForm input[value='MASTER_DATA_WRITE']").change((e) => {
            let isChecked = $(e.target).is(":checked");
            $("#createRoleForm input[value='MASTER_DATA_READ']").prop("checked", isChecked);
        })

        //Phần tick kiểm kê
        $("#createRoleForm input[value='VIEW_ALL_INVENTORY']").change(function (e) {
            let isChecked = $(e.target).is(":checked");
            $("#createRoleForm input[value='VIEW_CURRENT_INVENTORY']").prop("checked", isChecked);

            if (!isChecked) {
                $("#createRoleForm input[value='EDIT_INVENTORY']").prop("checked", false);
            }
        })

        $("#createRoleForm input[value='VIEW_CURRENT_INVENTORY']").change(function (e) {
            let isChecked = $(e.target).is(":checked");

            if (!isChecked) {
                $("#createRoleForm input[value='VIEW_ALL_INVENTORY']").prop("checked", false);
            }
        })

        $("#createRoleForm input[value='EDIT_INVENTORY']").change(function (e) {
            let isChecked = $(e.target).is(":checked");

            if (isChecked) {
                $("#createRoleForm input[value='VIEW_CURRENT_INVENTORY']").prop("checked", isChecked);
            }
        })

        $("#createRoleForm input[value='VIEW_CURRENT_INVENTORY']").change(function (e) {
            let isChecked = $(e.target).is(":checked");

            if (!isChecked) {
                $("#createRoleForm input[value='EDIT_INVENTORY']").prop("checked", isChecked);
            }
        })

        $("#createRoleForm input[value='SUBMIT_INVESTIGATION_DETAIL']").change(function (e) {
            let isChecked = $(e.target).is(":checked");

            if (isChecked) {
                $("#createRoleForm input[value='VIEW_INVESTIGATION_DETAIL']").prop("checked", isChecked);
            }
        })

        $("#createRoleForm input[value='CONFIRM_INVESTIGATION_DETAIL']").change(function (e) {
            let isChecked = $(e.target).is(":checked");

            if (isChecked) {
                $("#createRoleForm input[value='VIEW_INVESTIGATION_DETAIL']").prop("checked", isChecked);
            }
        })

    }

    function DisabledWebsiteAndMobile() {
        let websiteToggleChecked = root.$webAccessToggle.is(":checked");
        let mobileToggleChecked = root.$mobileAccessToggle.is(":checked");

        if (!websiteToggleChecked && !mobileToggleChecked) {
            //Tắt các checkbox xem dữ liệu theo phòng ban
            root.$createRoleForm.find(`input[data-category="DEPARTMENT_DATA_INQUIRY"]`).prop("checked", false).attr("disabled", true);
            root.$createRoleForm.find(`input[data-category="DEPARTMENT_DATA_INQUIRY"]`).closest(".form-check").removeClass("role_uncheck").addClass("role_uncheck");

            //Tắt các checkbox xem dữ liệu theo nhà máy
            root.$createRoleForm.find(`input[data-category="FACTORY_DATA_INQUIRY"]`).prop("checked", false).attr("disabled", true);
            root.$createRoleForm.find(`input[data-category="FACTORY_DATA_INQUIRY"]`).closest(".form-check").removeClass("role_uncheck").addClass("role_uncheck");
        } else {
            //Nếu bật website / mobile thì bật lại các checkbox xem dữ liệu phòng ban
            root.$createRoleForm.find(`input[data-category="DEPARTMENT_DATA_INQUIRY"]`).attr("disabled", false);
            root.$createRoleForm.find(`input[data-category="DEPARTMENT_DATA_INQUIRY"]`).closest(".form-check").removeClass("role_uncheck");
            //Nếu bật website / mobile thì bật lại các checkbox xem dữ liệu nhà máy
            root.$createRoleForm.find(`input[data-category="FACTORY_DATA_INQUIRY"]`).attr("disabled", false);
            root.$createRoleForm.find(`input[data-category="FACTORY_DATA_INQUIRY"]`).closest(".form-check").removeClass("role_uncheck");
        }
    }

    function PreLoad() {
        jQuery.validator.addMethod("requireRoleName", function (value, element) {
            // reset value of label#Name-error
            $("span[data-valmsg-for='Name']").text("");
            return value.length > 0
        }, "Vui lòng nhập tên nhóm quyền.");

        jQuery.validator.addMethod("existRoleName", function (value, element) {
            var host = $("#APIGateway").val()
            var url = `${host}/api/identity/role/exist-name/${value}`
            var valid = true

            if (value.length > 0) {
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
        }, "Tên nhóm quyền đã tồn tại. Vui lòng thử lại.");

        jQuery.validator.addMethod("Create_RequiredPermissionAtLeast", function (value, element) {
            let valid = true
            let webAccessCreateRole = root.$createRoleForm.find(".WebAccessToggle")
            let mobileAccessRole = root.$createRoleForm.find(".MobileAccessToggle")

            let webAccessChecked = webAccessCreateRole.is(":checked")
            let mobileAccessChecked = mobileAccessRole.is(":checked")
            if (webAccessChecked) {
                let atLeastWebChecked = root.$createRoleForm.find("#WebAccess [name='permission[]']:not(.WebAccessToggle):checked").length >= 1
                if (atLeastWebChecked == false) {
                    valid = false
                } 
            }
            if (mobileAccessChecked) {
                let atLeastMobileChecked = root.$createRoleForm.find("#MobileAccess [name='permission[]']:not(.MobileAccessToggle):checked").length >= 1
                if (atLeastMobileChecked == false) {
                    valid = false
                } 
            }

            if (!webAccessChecked && !mobileAccessChecked) {
                valid = false
            }

            return valid
        }, () => {
            /* var checkOpenMsg = $("#toast-container .toast-error .toast-message").text();
            if (checkOpenMsg) {
                return
            }
            let webAccessCreateRole = root.$createRoleForm.find(".WebAccessToggle")
            let mobileAccessRole = root.$createRoleForm.find(".MobileAccessToggle")

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

        let validateCreateModel = {
            ignore: [],
            rules: {
                'Name': {
                    requireRoleName: true,
                    existRoleName: true,
                },
                'permission[]': {
                    Create_RequiredPermissionAtLeast: true
                }
            },
            messages: {
                'Name': {
                    requireRoleName: window.languageData[window.currentLanguage]["Vui lòng nhập tên nhóm quyền."],
                    existRoleName: window.languageData[window.currentLanguage]["Tên nhóm quyền đã tồn tại. Vui lòng thử lại."]
                }
            },
            errorLabelContainer: $('span[data-valmsg-for="Name"]'),
            wrapper: "li"
        }

        //Vaidate form
        root.$createFormValidator = root.$createRoleForm.validate(validateCreateModel);
    }
    function waitForAddRoleLanguageData() {
        // Kiểm tra nếu dữ liệu đã sẵn sàng
        if (window.languageData && window.currentLanguage) {
            Cache()
            PreLoad()

            Events()

        } else {
            // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
            setTimeout(waitForAddRoleLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
        }
    }
    function Init() {
        waitForAddRoleLanguageData()
    }

    return {
        init: Init
    }
})()
