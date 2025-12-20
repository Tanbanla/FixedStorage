if ($("#Views_AuditMobile_AuditTargetList").length > 0) {
    $("#audittarget_filter_bar").show();
}

$(function () {
    AuditTargetController.Init();
})

const AuditTargetController = {
    el: $("#Views_AuditMobile_AuditTargetList"),
    Cache: function () {
        this.$btnOpenFilterModal = $(".setting_container");
        this.$filterModal = $(this.el.find("#modal_filter_audittargetlist"));
    },
    Apis: {
        Departments: async function (inventoryId, userId) {
            return new Promise(async (resolve, reject) => {
                $.ajax({
                    type: 'GET',
                    url: `${AppUser.getApiGateway}/api/inventory/${inventoryId}/account/${userId}/dropdown/department`,
                    success: function (response) {
                        resolve(response)
                    },
                    error: function (error) {
                        reject(error)
                    }
                });
            })
        },
        Locations: async function (inventoryId, userId, departmentName = DropdownOption.All) {
            return new Promise(async (resolve, reject) => {
                $.ajax({
                    type: 'GET',
                    url: `${AppUser.getApiGateway}/api/inventory/${inventoryId}/account/${userId}/dropdown/department/${departmentName}/location`,
                    success: function (response) {
                        resolve(response)
                    },
                    error: function (error) {
                        reject(error)
                    }
                });
            })
        },
        ComponentCodes: async function (inventoryId, userId, departmentName = DropdownOption.All, locationName = DropdownOption.All) {
            return new Promise(async (resolve, reject) => {
                $.ajax({
                    type: 'GET',
                    url: `${AppUser.getApiGateway}/api/inventory/${inventoryId}/account/${userId}/dropdown/department/${departmentName}/location/${locationName}/component`,
                    success: function (response) {
                        resolve(response)
                    },
                    error: function (error) {
                        reject(error)
                    }
                });
            })
        },
        GetAuditTargets: async function (model) {
            return new Promise(async (resolve, reject) => {
                $.ajax({
                    type: 'POST',
                    url: `${AppUser.getApiGateway}/api/inventory/list-audit`,
                    data: JSON.stringify(model),
                    contentType: "application/json",
                    success: function (response) {
                        resolve(response)

                        AuditTargetController.tempData = response.data;
                    },
                    error: function (error) {
                        reject(error)
                    }
                });
            })
        }
    },
    RenderAuditTargetList: function (data) {
        if (!data?.auditInfoModels?.length) {
            $(".audit_target_list").html(`<div class="text-center">${NotFounData}</div>`)
            $(".audit_list_status").hide();
        } else {
            $(".audit_list_status").show();
            $(".audit_target_list").text("")

            $("#auditList_statusCount").text(`${data.finishCount}/${data.totalCount} phiếu`);

            let resultHtml = data.auditInfoModels.map((item, i) => {
                let status = InventoryDocStatus[item.status];

                let docStatusColorClass = InventoryDocStatus_CSS[item.status];

                return `
                    <div class="d-flex justify-content-between w-100 align-items-center">
                        <div class="w-100">
                            <div class="row">
                                <div class="col-5">Phòng ban</div>
                                <div class="col-6"><label>${item.departmentName}</label></div>
                            </div>
                            <div class="row">
                                <div class="col-5">Khu vực</div>
                                <div class="col-6"><label>${item.locationName}</label></div>
                            </div>
                            <div class="row">
                                <div class="col-5">Mã linh kiện</div>
                                <div class="col-6"><label>${item.componentCode}</label></div>
                            </div>
                            <div class="row">
                                <div class="col-5">Vị trí</div>
                                <div class="col-6"><label>${item.positionCode}</label></div>
                            </div>
                            <div class="row">
                                <div class="col-5">Trạng thái</div>
                                <div class="col-6"><label class="${docStatusColorClass} txt-bolder">${status}</label></div>
                            </div>
                        </div>
                        <div>
                            <div href="/AuditMobile/Documentdetail/${item.id}" status=${item.status} class="audit_item_navigation"> 
                                <div> <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
                                        <path d="M5.93994 13.2787L10.2866 8.93208C10.7999 8.41875 10.7999 7.57875 10.2866 7.06542L5.93994 2.71875" stroke="#333333" stroke-width="1.5" stroke-miterlimit="10" stroke-linecap="round" stroke-linejoin="round"/>
                                    </svg>
                                </div>
                            </div>
                        </div>
                    </div>
                    <hr />
                `
            }).join('')

            $(".audit_target_list").html(resultHtml)
        }

    },
    Events: function () {
        $.sub(GlobalEventName.audit_mobile_filter_change, async function (e) {
            let selectedDepartment = $("#select_department option:selected").val();
            let selectedLocation = $("#select_location option:selected").val();
            let selectedComponent = $("#select_componentCode option:selected").val();

            let isValidFilter = AuditTargetController.$filterFormValidator.checkForm();
            if (isValidFilter) {
                $(".audit_list_status").show();
                $(".audit_target_list").show();
            } else {
                $(".audit_list_status").hide();
                $(".audit_target_list").hide();
            }

            //Render list of audit 
            if (isValidFilter) {
                let inventoryInfo = await AppUser.getUser().inventoryLoggedInfo();
                let inventoryId = inventoryInfo.inventoryModel.inventoryId;
                let accountId = AppUser.getUser().userId();

                let queryModel = {
                    inventoryId: inventoryId,
                    accountId: accountId,
                    departmentName: selectedDepartment,
                    locationName: selectedLocation,
                    componentCode: selectedComponent
                }
                
                AuditTargetController.Apis.GetAuditTargets(queryModel).then(res => {
                    AuditTargetController.RenderAuditTargetList(res.data);
                }).catch(err => {
                })
            }
        })

        $("#filter_audit_form").submit(function (e) {
            e.preventDefault();
        })

        this.$btnOpenFilterModal.click(function (e) {
            AuditTargetController.$filterModal.modal("show");
        })

        $("#select_department").change(async function (e) {
            let selectedOption = $(this).find("option:selected");
            let selectedValue = selectedOption.val();

            if (selectedValue == "") {
                $("#select_location").val("").change();
                return;
            }

            let inventoryInfo = await AppUser.getUser().inventoryLoggedInfo();
            let inventoryId = inventoryInfo.inventoryModel.inventoryId;
            let accountId = AppUser.getUser().userId();

            AuditTargetController.Apis.Locations(inventoryId, accountId, selectedValue).then(res => {
                AuditTargetController.RenderLocation(res.data);

                $.pub(GlobalEventName.audit_mobile_location_change);
            })
        })

        $("#select_location").change(async function (e) {
            let selectedDepartmentValue = $("#select_department").find("option:selected").val();
            let selectedLocationValue = $("#select_location").find("option:selected").val();


            if (selectedLocationValue == "") {
                $("#select_componentCode").val("").change();
                return;
            }

            let inventoryInfo = await AppUser.getUser().inventoryLoggedInfo();
            let inventoryId = inventoryInfo.inventoryModel.inventoryId;
            let accountId = AppUser.getUser().userId();

            AuditTargetController.Apis.ComponentCodes(inventoryId, accountId, selectedDepartmentValue, selectedLocationValue).then(res => {
                AuditTargetController.RenderComponent(res.data);

                $.pub(GlobalEventName.audit_mobile_component_change);
            })
        })

        //Filter
        $("#btnConfirmFilter").click(function (e) {
            let filterFormValid = AuditTargetController.$filterFormValidator.valid();
            if (!filterFormValid) return;

            let selectedDepartment = $("#select_department option:selected").val();
            let selectedLocation = $("#select_location option:selected").val();
            let selectedComponent = $("#select_componentCode option:selected").val();

            selectedDepartment = selectedDepartment == DropdownOption.All ? DropdownOption.DisplayName.All : selectedDepartment;
            selectedLocation = selectedLocation == DropdownOption.All ? DropdownOption.DisplayName.All : selectedLocation;
            selectedComponent = selectedComponent == DropdownOption.All ? DropdownOption.DisplayName.All : selectedComponent;

            $("#filter_label_department").text(selectedDepartment);
            $("#filter_label_location").text(selectedLocation);
            $("#filter_label_component").text(selectedComponent);

            $.pub(GlobalEventName.audit_mobile_filter_change);
        })

        //Reset filter
        $("#btnResetFilter").click(function (e) {
            $("#select_department").val(DropdownOption.All);
            $("#select_location").val(DropdownOption.All);
            $("#select_componentCode").val(DropdownOption.All);

            $("#filter_label_department").text(DropdownOption.DisplayName.All);
            $("#filter_label_location").text(DropdownOption.DisplayName.All);
            $("#filter_label_component").text(DropdownOption.DisplayName.All);

            setTimeout(() => {
                AuditTargetController.$filterFormValidator.resetForm();
            })

            $.pub(GlobalEventName.audit_mobile_filter_change);
        })

        AuditTargetController.el.delegate(".audit_item_navigation", "click", function (e) {
            let index = $(".audit_item_navigation").index($(this));
            let { id, status } = AuditTargetController?.tempData?.auditInfoModels[index];

            let validStatus = false;
            let message = ``;

            if (InventoryDocStatus[status] === InventoryDocStatusTitle.NotInventoryYet) {
                message = "Phiếu này chưa được thực hiện kiểm kê. Vui lòng thử lại.";
            } else if (InventoryDocStatus[status] === InventoryDocStatusTitle.WaitingConfirm ||
                InventoryDocStatus[status] === InventoryDocStatusTitle.MustEdit) {
                message = "Phiếu này chưa được thực hiện xác nhận kiểm kê. Vui lòng thử lại.";
            } else {
                validStatus = true;
            }

            if (!validStatus) {
                Swal.fire({
                    title: `<b>Lỗi</b>`,
                    text: `${message}`,
                    confirmButtonText: "Đã hiểu",
                    width: '100%'
                });

                return;
            } else {
                window.location.href = `/AuditMobile/DocumentDetail/${id}`;
            }
        })
    },
    RenderDepartment: function (data) {
        if (!data?.length) {
            return;
        }
        let resultHtml = data.map(item => {
            return `<option value="${item}">${item}</option>`
        }).join('');
        let wrapper = `<option value="${DropdownOption.All}">${DropdownOption.DisplayName.All}</option>`;

        $("#select_department").html(wrapper + resultHtml);
        $("#select_department").change();
    },
    RenderLocation: function (data) {
        if (!data?.length) {
            return;
        }
        let resultHtml = data.map(item => {
            return `<option value="${item}">${item}</option>`
        }).join('');
        let wrapper = `<option value=${DropdownOption.All}>${DropdownOption.DisplayName.All}</option>`;

        $("#select_location").html(wrapper + resultHtml);
        $("#select_location").change();

    },
    RenderComponent: function (data) {
        if (!data?.length) {
            return;
        }
        let resultHtml = data.map(item => {
            return `<option value="${item}">${item}</option>`
        }).join('');
        let wrapper = `<option value=${DropdownOption.All}>${DropdownOption.DisplayName.All}</option>`;

        $("#select_componentCode").html(wrapper + resultHtml);
    },
    Validate: function () {
        let validateModel = {
            rules: {
                Department: {
                    required: true,
                },
                Location: {
                    required: true
                },
                ComponentCode: {
                    required: true
                }
            },
            messages: {
                Department: {
                    required: "Vui lòng chọn phòng ban."
                },
                Location: {
                    required: "Vui lòng chọn khu vực."
                },
                ComponentCode: {
                    required: "Vui lòng chọn mã linh kiện."
                },
            }
        }

        AuditTargetController.$filterFormValidator = AuditTargetController.el.find("#filter_audit_form").validate(validateModel);
    },
    InitFilter: async function () {
        let inventoryInfo = await AppUser.getUser().inventoryLoggedInfo();
        let userCode = AppUser.getUser().userCode;
        let inventoryId = inventoryInfo.inventoryModel.inventoryId;
        let accountId = AppUser.getUser().userId();


        let promiseDepartments = AuditTargetController.Apis.Departments(inventoryId, accountId);
        let promiseLocations = AuditTargetController.Apis.Locations(inventoryId, accountId, DropdownOption.All);
        let promiseComponents = AuditTargetController.Apis.ComponentCodes(inventoryId, accountId, DropdownOption.All, DropdownOption.All);

        Promise.all([promiseDepartments, promiseLocations, promiseComponents]).then((values) => {
            let departments = values[0].data;
            let locations = values[1].data;
            let components = values[2].data;

            AuditTargetController.RenderDepartment(departments);
            AuditTargetController.RenderLocation(locations);
            AuditTargetController.RenderComponent(components);
        }).finally(() => {
            $("#btnConfirmFilter").trigger("click");

            loading(false);
        })
    },
    PreLoad: async function () {
        loading(true);
        let inventoryInfo = await AppUser.getUser().inventoryLoggedInfo();
        let userCode = AppUser.getUser().userCode;
        let inventoryId = inventoryInfo.inventoryModel.inventoryId;
        let accountId = AppUser.getUser().userId();

        $.ajaxSetup({
            headers: { 'DeviceId': `${userCode}`}
        });

        //Init filter
        AuditTargetController.InitFilter();

        //Init validate 
        AuditTargetController.Validate();
    },
    Init: function () {
        this.Cache();
        this.Events();


        this.PreLoad();
    },
}