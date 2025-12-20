const host = $("#APIGateway").val();
$(function () {
    handleButtonTabClick();

    InventoryAssignmentHandler.init();
    ActorHandler.init();
});

function handleButtonTabClick() {
    $(".changeTab").click(ValidateInputHelper.Utils.debounce(function (e) {
        // get id of element has class .changeTab
        var tabId = $(e.target).attr("id");
        // handle change tab
        handleChangeTab(tabId);
    }, 200));
}

function handleChangeTab(tabId) {
    // remove all class .active in element has class .changeTab
    $(".changeTab").removeClass("active");
    // add class .active in element has id = tabId
    $(`#${tabId}`).addClass("active");
    // hide all element has class .tab-content
    $(".tab-content").hide();
    // show element has id = tabId
    $(`#${tabId}-tab`).show();

    let activeTab = $(".changeTab.active");
    if (activeTab.is("#employee")) {
        ActorHandler.preLoad();
    } else if (activeTab.is("#area")) {
        InventoryAssignmentHandler.renderList();
    }
}


; var InventoryAssignmentHandler = (function () {
    let root = {
        parentEl: $("#Views_InventoryAssignment_Index"),
        event: {
            factoryItemChanged_AddModal: "addModal_factoryItem_changed",
            factoryItemChanged_EditModal: "editModal_factoryItem_changed"
        },
        checkBeforeUpdateResponseCache: null
    };
    let createFormValidator;
    let editFormValidator;

    function CreateLocationAPI(model) {
        return new Promise(async (resolve, reject) => {
            let url = `${host}/api/inventory/location`

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

    function UpdateLocationAPI(model) {
        return new Promise(async (resolve, reject) => {
            let url = `${host}/api/inventory/location`

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

    async function CheckBeforeEditLocationAPI(locationId) {
        return await $.ajax({
            url: `${host}/api/inventory/location/${locationId}/assigned-document`,
            type: 'GET',
        });
    }

    function GetLocationsAPI() {
        return new Promise(async (resolve, reject) => {
            $.ajax({
                url: `${host}/api/inventory/location`,
                type: 'GET',
                contentType: 'application/json',
                success: function (res) {
                    resolve(res);
                },
                error: function (err) {
                    reject(err);
                },
                complete: function (res, data) {
                    let itemCount = res?.responseJSON?.data?.length || 0;

                    $(window).trigger(GlobalEventName.inventory_location_mangament_response, itemCount);
                }
            });
        })
    }

    function DeleteAPI(locationId) {
        return new Promise(async (resolve, reject) => {
            let url = `${host}/api/inventory/location/${locationId}`

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

    async function LocationDetailAPI(locationId) {
        return await $.ajax({
            url: `${host}/api/inventory/location/${locationId}`,
            type: 'GET',
            contentType: 'application/json',
        });
    }


    function Cache() {
        //Modal thêm mới khu vực
        root.btnOpenAddLocationModal = root.parentEl.find("#addArea");
        root.Inventory_AddAreaModal = root.parentEl.find("#Inventory_AddAreaModal");
        root.form_create_location = root.Inventory_AddAreaModal.find("#form_create_location");
         
        root.btnApplyNewLocation = root.Inventory_AddAreaModal.find("#button_Apply_AddNewArea");
        root.btnCloseAddAreaModal = root.Inventory_AddAreaModal.find("#button_Cancel_AddNewArea");


        //Nút thêm mới nhà máy trong modal
        root.btn_add_factory = root.Inventory_AddAreaModal.find(".btn_add_factory");

        //Modal chỉnh sửa khu vực
        root.Inventory_EditAreaModal = root.parentEl.find("#Inventory_EditAreaModal");
        root.form_edit_location = root.Inventory_EditAreaModal.find("#form_edit_location");

        //Nút thêm mới nhà máy trong edit modal
        root.btn_add_factory_edit = root.Inventory_EditAreaModal.find(".btn_add_factory");
    }

    function Events() {
        $(window).on(GlobalEventName.inventory_location_mangament_response, function (e, itemCount) {
            if (itemCount > 0) {
                $(".location_empty_data")?.hide();
            } else {
                $(".location_empty_data")?.show();
            }
        });

        //Chặn input tạo khu vực
        root.Inventory_AddAreaModal.find("#Inventory_Area_Name").blur(ValidateInputHelper.TrimWhiteSpaceOnBlur)
        root.Inventory_AddAreaModal.find("#Inventory_Area_Name").on("keypress keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50))

        root.Inventory_AddAreaModal.find("#Inventory_Department_Name").blur(ValidateInputHelper.TrimWhiteSpaceOnBlur)
        root.Inventory_AddAreaModal.find("#Inventory_Department_Name").on("keypress keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(20))

        root.form_create_location.delegate("input[name^='FactoryNames']", "blur", ValidateInputHelper.TrimWhiteSpaceOnBlur)
        root.form_create_location.delegate("input[name^='FactoryNames']", "keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(10))
        root.form_create_location.delegate("input[name^='FactoryNames']", "keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(10))
        root.form_create_location.delegate("input[name^='FactoryNames']", "keypress", ValidateInputHelper.PreventSepcialCharacterOnKeyPress)
        root.form_create_location.delegate("input[name^='FactoryNames']", "keyup", ValidateInputHelper.RemoveSpecialCharacter)
           

        //Chặn input chỉnh sửa khu vực
        root.Inventory_EditAreaModal.find("#Inventory_Area_Name").blur(ValidateInputHelper.TrimWhiteSpaceOnBlur)
        root.Inventory_EditAreaModal.find("#Inventory_Area_Name").on("keypress keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(50))

        root.Inventory_EditAreaModal.find("#Inventory_Department_Name").blur(ValidateInputHelper.TrimWhiteSpaceOnBlur)
        root.Inventory_EditAreaModal.find("#Inventory_Department_Name").on("keypress keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(20))

        root.form_edit_location.delegate("input[name^='FactoryNames']", "blur", ValidateInputHelper.TrimWhiteSpaceOnBlur)
        root.form_edit_location.delegate("input[name^='FactoryNames']", "keypress", ValidateInputHelper.LimitInputLengthOnKeyPressForText(10))
        root.form_edit_location.delegate("input[name^='FactoryNames']", "keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(10))
        root.form_edit_location.delegate("input[name^='FactoryNames']", "keypress", ValidateInputHelper.PreventSepcialCharacterOnKeyPress)
        root.form_edit_location.delegate("input[name^='FactoryNames']", "keyup", ValidateInputHelper.RemoveSpecialCharacter)

        $("#addArea").click(function () {
            $("#Inventory_AddAreaModal").modal("show");
        });

        $("#area-tab").delegate(".editArea", "click", function (e) {
        })

        root.form_create_location.on("submit", e => e.preventDefault());

        root.btn_add_factory.click(function (e) {
            let factoryList = $(this).closest("#form_create_location").find(".factory_list");
            let childCount = factoryList.find(".factory_item").length;
            let index = ++childCount;

            let factoryItem = `<div class="factory_item">
                                    <div class="bold_component_label">${window.languageData[window.currentLanguage]['Nhà máy']}<span>*</span></div>
                                    <div class="factory_item_control">
                                        <div class="factory_input_container">
                                            <input id="" name="FactoryNames[${index}]" type="text" class="form-control background-color-grey" placeholder="${window.languageData[window.currentLanguage]['Nhập tên nhà máy']}...">
                                        </div>
                                        <div class="create_modal_btn_delete_factory">
                                            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                                <path d="M21.0699 5.23C19.4599 5.07 17.8499 4.95 16.2299 4.86V4.85L16.0099 3.55C15.8599 2.63 15.6399 1.25 13.2999 1.25H10.6799C8.34991 1.25 8.12991 2.57 7.96991 3.54L7.75991 4.82C6.82991 4.88 5.89991 4.94 4.96991 5.03L2.92991 5.23C2.50991 5.27 2.20991 5.64 2.24991 6.05C2.28991 6.46 2.64991 6.76 3.06991 6.72L5.10991 6.52C10.3499 6 15.6299 6.2 20.9299 6.73C20.9599 6.73 20.9799 6.73 21.0099 6.73C21.3899 6.73 21.7199 6.44 21.7599 6.05C21.7899 5.64 21.4899 5.27 21.0699 5.23Z" fill="#E60000"></path>
                                                <path d="M19.23 8.14C18.99 7.89 18.66 7.75 18.32 7.75H5.67999C5.33999 7.75 4.99999 7.89 4.76999 8.14C4.53999 8.39 4.40999 8.73 4.42999 9.08L5.04999 19.34C5.15999 20.86 5.29999 22.76 8.78999 22.76H15.21C18.7 22.76 18.84 20.87 18.95 19.34L19.57 9.09C19.59 8.73 19.46 8.39 19.23 8.14ZM13.66 17.75H10.33C9.91999 17.75 9.57999 17.41 9.57999 17C9.57999 16.59 9.91999 16.25 10.33 16.25H13.66C14.07 16.25 14.41 16.59 14.41 17C14.41 17.41 14.07 17.75 13.66 17.75ZM14.5 13.75H9.49999C9.08999 13.75 8.74999 13.41 8.74999 13C8.74999 12.59 9.08999 12.25 9.49999 12.25H14.5C14.91 12.25 15.25 12.59 15.25 13C15.25 13.41 14.91 13.75 14.5 13.75Z" fill="#E60000"></path>
                                            </svg>
                                        </div>
                                    </div>
                                </div>`

            factoryList.append(factoryItem);

            $(window).trigger(root.event.factoryItemChanged_AddModal)
        })

        root.btn_add_factory_edit.click(function (e) {
            let factoryList = $(this).closest("#form_edit_location").find(".factory_list");
            let childCount = factoryList.find(".factory_item").length;
            let index = ++childCount;

            let factoryItem = `<div class="factory_item">
                                    <div class="bold_component_label">${window.languageData[window.currentLanguage]['Nhà máy']}<span>*</span></div>
                                    <div class="factory_item_control">
                                        <div class="factory_input_container">
                                            <input id="" name="FactoryNames[${index}]" type="text" class="form-control background-color-grey" placeholder="${window.languageData[window.currentLanguage]['Nhập tên nhà máy']}...">
                                        </div>
                                        <div class="edit_modal_btn_delete_factory">
                                            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                                <path d="M21.0699 5.23C19.4599 5.07 17.8499 4.95 16.2299 4.86V4.85L16.0099 3.55C15.8599 2.63 15.6399 1.25 13.2999 1.25H10.6799C8.34991 1.25 8.12991 2.57 7.96991 3.54L7.75991 4.82C6.82991 4.88 5.89991 4.94 4.96991 5.03L2.92991 5.23C2.50991 5.27 2.20991 5.64 2.24991 6.05C2.28991 6.46 2.64991 6.76 3.06991 6.72L5.10991 6.52C10.3499 6 15.6299 6.2 20.9299 6.73C20.9599 6.73 20.9799 6.73 21.0099 6.73C21.3899 6.73 21.7199 6.44 21.7599 6.05C21.7899 5.64 21.4899 5.27 21.0699 5.23Z" fill="#E60000"></path>
                                                <path d="M19.23 8.14C18.99 7.89 18.66 7.75 18.32 7.75H5.67999C5.33999 7.75 4.99999 7.89 4.76999 8.14C4.53999 8.39 4.40999 8.73 4.42999 9.08L5.04999 19.34C5.15999 20.86 5.29999 22.76 8.78999 22.76H15.21C18.7 22.76 18.84 20.87 18.95 19.34L19.57 9.09C19.59 8.73 19.46 8.39 19.23 8.14ZM13.66 17.75H10.33C9.91999 17.75 9.57999 17.41 9.57999 17C9.57999 16.59 9.91999 16.25 10.33 16.25H13.66C14.07 16.25 14.41 16.59 14.41 17C14.41 17.41 14.07 17.75 13.66 17.75ZM14.5 13.75H9.49999C9.08999 13.75 8.74999 13.41 8.74999 13C8.74999 12.59 9.08999 12.25 9.49999 12.25H14.5C14.91 12.25 15.25 12.59 15.25 13C15.25 13.41 14.91 13.75 14.5 13.75Z" fill="#E60000"></path>
                                            </svg>
                                        </div>
                                    </div>
                                </div>
                                `

            factoryList.append(factoryItem);

            $(window).trigger(root.event.factoryItemChanged_EditModal);
        })

        root.form_create_location.delegate(".create_modal_btn_delete_factory", "click", function (e) {
            let factoryList = $(this).closest("#form_create_location").find(".factory_list");
            let childCount = factoryList.find(".factory_item");

            let factoryItem = $(this).closest(".factory_item");

            if (childCount.length > 1) {
                factoryItem.remove();
            }

            $(window).trigger(root.event.factoryItemChanged_AddModal);
        })

        //Delete factory in edit modal
        root.form_edit_location.delegate(".edit_modal_btn_delete_factory", "click", function (e) {
            let factoryList = $(this).closest(root.form_edit_location).find(".factory_list");
            let childCount = factoryList.find(".factory_item");

            let factoryItem = $(this).closest(".factory_item");

            if (childCount.length > 1) {
                factoryItem.remove();
            }

            $(window).trigger(root.event.factoryItemChanged_EditModal);
        })

        //On create modal show
        $(window).on('shown.bs.modal', function (e) {
            let target = e.target;
            let isCreateModal = $(target).is(root.Inventory_AddAreaModal);
            let isEditModal = $(target).is(root.Inventory_EditAreaModal);
            if (isCreateModal) {
                $(window).trigger(root.event.factoryItemChanged_AddModal);
            } else if (isEditModal) {
                $(window).trigger(root.event.factoryItemChanged_EditModal);
            }
        })

        $(window).on('hidden.bs.modal', function (e) {
            let target = e.target;
            let isCreateModal = $(target).is(root.Inventory_AddAreaModal);
            if (isCreateModal) {
                root.Inventory_AddAreaModal.find("#Inventory_Area_Name").val("");
                root.Inventory_AddAreaModal.find("#Inventory_Department_Name").val("");

                root.Inventory_AddAreaModal.find(".factory_item:not(:first-child)").remove();
                root.Inventory_AddAreaModal.find(".factory_item:eq(0)").find("input[name^='FactoryNames']").val("");

                createFormValidator.resetForm();
            }
        })

        root.Inventory_AddAreaModal.find("#button_Cancel_AddNewArea").click((e) => {
            root.Inventory_AddAreaModal.find("#Inventory_Area_Name").val("");
            root.Inventory_AddAreaModal.find("#Inventory_Department_Name").val("");

            root.Inventory_AddAreaModal.find(".factory_item:not(:first-child)").remove();
            root.Inventory_AddAreaModal.find(".factory_item:eq(0)").find("input[name^='FactoryNames']").val("");

            createFormValidator.resetForm();
        })

        //Create API
        root.btnApplyNewLocation.click(function (e) {
            $("#form_create_location input[name^='FactoryNames']").each(function () {
                $(this).rules("add", {
                    required: true,
                    maxlength: 10,
                    messages: {
                        required: window.languageData[window.currentLanguage]["Vui lòng nhập tên nhà máy."]
                    }
                });
            });  

            let validForm = root.form_create_location.valid();
            if (validForm) {
                let factoryNameArr = [];

                let factoryNameElements = root.form_create_location.find(".factory_list").find("input[name^='FactoryNames']");
                let factoryNames = factoryNameElements.map((i, el) => {
                    factoryNameArr.push($(el).val());
                })

                let model = {
                    Name: root.Inventory_AddAreaModal.find("#Inventory_Area_Name").val(),
                    DepartmentName: root.Inventory_AddAreaModal.find("#Inventory_Department_Name").val(),
                    FactoryNames: factoryNameArr
                };

                //Call API
                loading(true);
                CreateLocationAPI(model).then((res) => {
                    RenderList();
                    root.Inventory_AddAreaModal.modal("hide");

                    toastr.success(res.message);
                }).catch(err => {
                    if (err.responseJSON?.code == 40) {
                        createFormValidator.showErrors({ 'Name': err?.responseJSON?.message });
                    } else {
                        toastr.error(err?.responseJSON?.message);
                    }
                }).finally(() => {
                    loading(false);
                })
            }
        })

        root.parentEl.delegate(".btn_remove_location_item", "click", function (e) {
            let currDeleteButton = $(this).closest(".btn_remove_location_item");
            let locationId = $(currDeleteButton).closest(".location_item").attr("locationId");

            Swal.fire({
                title: `<b>${window.languageData[window.currentLanguage]['Xác nhận xóa']}</b>`,
                text: window.languageData[window.currentLanguage]["Bạn có chắc chắn muốn xóa khu vực này ?"],
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
                    loading(true);
                    DeleteAPI(locationId).then(res => {
                        RenderList();
                        toastr.success(res?.message);
                    }).catch(err => {
                        Swal.fire({
                            title: `<b>${window.languageData[window.currentLanguage]['Không thể xóa']}</b>`,
                            text: err?.responseJSON?.message,
                            confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                            width: '30%'
                        })
                    }).finally(() => {
                        loading(false);
                    })
                }
            })
        })

        root.parentEl.delegate(".editArea", "click", async function (e) {
            let thisBtn = $(e.target).closest(".editArea");
            let item = $(thisBtn).closest(".location_item");
            let locationId = item.attr("locationid");

            if (!locationId) return;

            let responseCheck;
            try {
                responseCheck = await CheckBeforeEditLocationAPI(locationId);
                root.checkBeforeUpdateResponseCache = responseCheck;
            } catch (err) {
                let response = err.responseJSON;

                Swal.fire({
                    title: `<b>${window.languageData[window.currentLanguage]['Thông báo']}</b>`,
                    text: window.languageData[window.currentLanguage][response?.message] || "",
                    confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                    width: '30%'
                });
            }

            if (responseCheck) {
                loading(true);
                try {
                    let response = await LocationDetailAPI(locationId);
                    if (!response.data) {
                        return;
                    }

                    let item = response.data;
                    root.Inventory_EditAreaModal.data("locationId", item.id);
                    root.Inventory_EditAreaModal.find("#Inventory_Area_Name").val(item.locationName);
                    root.Inventory_EditAreaModal.find("#Inventory_Department_Name").val(item.departmentName);
                    let factoryNames = item?.factoryNames || "";
                    if (factoryNames.length > 0) {
                        let displayDeleteButton = `<div class="edit_modal_btn_delete_factory">
                                            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                                    <path d="M21.0699 5.23C19.4599 5.07 17.8499 4.95 16.2299 4.86V4.85L16.0099 3.55C15.8599 2.63 15.6399 1.25 13.2999 1.25H10.6799C8.34991 1.25 8.12991 2.57 7.96991 3.54L7.75991 4.82C6.82991 4.88 5.89991 4.94 4.96991 5.03L2.92991 5.23C2.50991 5.27 2.20991 5.64 2.24991 6.05C2.28991 6.46 2.64991 6.76 3.06991 6.72L5.10991 6.52C10.3499 6 15.6299 6.2 20.9299 6.73C20.9599 6.73 20.9799 6.73 21.0099 6.73C21.3899 6.73 21.7199 6.44 21.7599 6.05C21.7899 5.64 21.4899 5.27 21.0699 5.23Z" fill="#E60000"></path>
                                                    <path d="M19.23 8.14C18.99 7.89 18.66 7.75 18.32 7.75H5.67999C5.33999 7.75 4.99999 7.89 4.76999 8.14C4.53999 8.39 4.40999 8.73 4.42999 9.08L5.04999 19.34C5.15999 20.86 5.29999 22.76 8.78999 22.76H15.21C18.7 22.76 18.84 20.87 18.95 19.34L19.57 9.09C19.59 8.73 19.46 8.39 19.23 8.14ZM13.66 17.75H10.33C9.91999 17.75 9.57999 17.41 9.57999 17C9.57999 16.59 9.91999 16.25 10.33 16.25H13.66C14.07 16.25 14.41 16.59 14.41 17C14.41 17.41 14.07 17.75 13.66 17.75ZM14.5 13.75H9.49999C9.08999 13.75 8.74999 13.41 8.74999 13C8.74999 12.59 9.08999 12.25 9.49999 12.25H14.5C14.91 12.25 15.25 12.59 15.25 13C15.25 13.41 14.91 13.75 14.5 13.75Z" fill="#E60000"></path>
                                                </svg>
                                            </div>`;

                        let resultHTML = factoryNames.split(',').map((name, i) => {
                            return `
                                <div class="factory_item">
                                    <div class="bold_component_label">${window.languageData[window.currentLanguage]['Nhà máy']}<span>*</span></div>
                                    <div class="factory_item_control">
                                        <div class="factory_input_container">
                                            <input id="" name="FactoryNames[${i}]" value="${name.trim()}" type="text" class="form-control background-color-grey" placeholder="${window.languageData[window.currentLanguage]['Nhập tên nhà máy']}...">
                                        </div>
                                        ${i != 0 ? displayDeleteButton : ""}
                                    </div>
                                </div>
                            `
                        });

                        root.Inventory_EditAreaModal.find(".factory_list").html(resultHTML);

                        $(window).on(root.event.factoryItemChanged_EditModal);
                        $("#Inventory_EditAreaModal").modal("show");
                    }
                } catch (err) {
                    let response = err.responseJSON;
                    toastr.error(response?.message || "Không tìm thấy thông tin khu vực.")
                } finally {
                    loading(false);
                }
            }
        })

        //Edit API
        root.Inventory_EditAreaModal.delegate(".btn_update_edit_modal", "click", async function (e) {
            $("#form_edit_location input[name^='FactoryNames']").each(function () {
                $(this).rules("add", {
                    required: true,
                    maxlength: 10,
                    messages: {
                        required: window.languageData[window.currentLanguage]["Vui lòng nhập tên nhà máy."]
                    }
                });
            }); 

            let validForm = root.form_edit_location.valid();
            if (!validForm) {
                return;
            }

            //Call API
            if (root.checkBeforeUpdateResponseCache && root.checkBeforeUpdateResponseCache.code == ServerResponseStatusCode.UpdateLocationTakeTime) {
                Swal.fire({
                    title: '<b>Xác nhận cập nhật</b>',
                    text: `${root.checkBeforeUpdateResponseCache.message}`,
                    confirmButtonText: 'Đồng ý',
                    showCancelButton: true,
                    showLoaderOnConfirm: true,
                    cancelButtonText: 'Hủy bỏ',
                    reverseButtons: true,
                    allowOutsideClick: false,
                    customClass: {
                        actions: "swal_confirm_actions"
                    }
                }).then((result) => {
                    if (result.isConfirmed) {
                        UpdateAction();
                    }
                })
            } else {
                UpdateAction();
            }
        })

        function UpdateAction() {
            let locationId = "";
            let factoryNameArr = [];

            let factoryNameElements = root.form_edit_location.find(".factory_list").find("input[name^='FactoryNames']");
            let factoryNames = factoryNameElements.map((i, el) => {
                factoryNameArr.push($(el).val());
            })

            let model = {
                Id: root.Inventory_EditAreaModal.data("locationId"),
                Name: root.Inventory_EditAreaModal.find("#Inventory_Area_Name").val(),
                DepartmentName: root.Inventory_EditAreaModal.find("#Inventory_Department_Name").val(),
                FactoryNames: factoryNameArr
            };

            //Call API
            loading(true);
            UpdateLocationAPI(model).then((res) => {
                RenderList();
                root.Inventory_EditAreaModal.modal("hide");

                toastr.success(res.message);
            }).catch(err => {
                //Nếu code 40: trùng tên khu vực
                if (err.responseJSON.code == 40) {
                    editFormValidator.showErrors({
                        'Inventory_Area_Name': err?.responseJSON?.message
                    })
                } else {
                    root.Inventory_EditAreaModal.modal("hide");
                    toastr.error(err?.responseJSON?.message);
                }

            }).finally(() => {
                loading(false);

                //refresh update response
                root.checkBeforeUpdateResponseCache = null;
            });
        }

        root.Inventory_EditAreaModal.delegate(".btn_cancle_edit_modal", "click", function (e) {
            let thisBtn = $(this).closest(".btn_cancle_edit_modal");
            root.Inventory_EditAreaModal.find(".factory_item:not(:first-child)").remove();
            root.Inventory_EditAreaModal.find(".factory_item:first-child").find("input").val("");

            editFormValidator.resetForm();
        })

        //Sub event Factory change on ADD
        $(window).on(root.event.factoryItemChanged_AddModal, function (e) {
            //Đếm số item để ẩn hiện thùng rác
            let factoryItems = root.Inventory_AddAreaModal.find(".factory_item");

            if (factoryItems.length > 1) {
                factoryItems.find(".create_modal_btn_delete_factory").show();
            } else if (factoryItems.length == 1) {
                factoryItems.eq(0).find(".create_modal_btn_delete_factory").hide();
            }
        })

        //Sub event Factory change on EDIT 
        $(window).on(root.event.factoryItemChanged_EditModal, function (e) {
            //Đếm số item để ẩn hiện thùng rác
            let factoryItems = root.Inventory_EditAreaModal.find(".factory_item");

            if (factoryItems.length > 1) {
                factoryItems.find(".edit_modal_btn_delete_factory").show();
            } else if (factoryItems.length == 1) {
                factoryItems.eq(0).find(".edit_modal_btn_delete_factory").hide();
            }
        })


    }

    async function RenderList() {
        root.parentEl.find(".Assignment_Layout_Content").empty();

        var canEditInventory = (App.User.AccountType == AccountType.TaiKhoanRieng && App.User.isGrant("EDIT_INVENTORY")) ||
                                isPromoter();

        loading(true);
        await GetLocationsAPI().then(res => {
            let data = res?.data || [];
            if (data) {
                let resultHTML = data.map((item, i) => {
                    return `
                        <div class="col-xl-3 col-lg-3 col-md-3 col-sm-3 Assignment_Layout_Content_Container mb-3 location_item" locationId="${item.id}">
                            <div class="Title">
                                <div class="row">
                                    <div class="col-xl-9 col-lg-9 col-md-9 col-sm-9">
                                        <h4>${item?.locationName || ""}</h4>
                                    </div>
                                    ${canEditInventory ? `<div class="col-xl-3 col-lg-3 col-md-3 col-sm-3 Image">
                                                        <div class="editArea" id="editArea">
                                                            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                                                <path fill-rule="evenodd" clip-rule="evenodd" d="M13.6962 5.47381C15.6862 3.48381 17.5862 3.53381 19.5262 5.47381C20.5262 6.46381 21.0062 7.42381 20.9962 8.41381C20.9962 9.37381 20.5162 10.3238 19.5262 11.3038L18.3262 12.5138C18.2462 12.5938 18.1462 12.6338 18.0362 12.6338C17.9962 12.6338 17.9562 12.6238 17.9162 12.6138C15.2662 11.8538 13.1462 9.73381 12.3862 7.08381C12.3462 6.94381 12.3862 6.78381 12.4862 6.68381L13.6962 5.47381ZM15.2762 13.0838C15.5462 13.2438 15.8262 13.3838 16.1162 13.5238C16.1551 13.5407 16.1935 13.5572 16.2315 13.5734C16.5691 13.7175 16.667 14.1629 16.4075 14.4225L10.6862 20.1438C10.5662 20.2738 10.3162 20.3938 10.1362 20.4238L6.29618 20.9638C6.17618 20.9838 6.05618 20.9938 5.93618 20.9938C5.39618 20.9938 4.89618 20.8038 4.53618 20.4538C4.11618 20.0238 3.92618 19.3838 4.02618 18.7038L4.56618 14.8738C4.59618 14.7038 4.71618 14.4538 4.84618 14.3238L10.5746 8.59534C10.8324 8.33758 11.267 8.43503 11.4146 8.76834C11.4345 8.81325 11.455 8.85841 11.4762 8.90381C11.6162 9.18381 11.7562 9.45381 11.9162 9.72381C12.0462 9.94381 12.1862 10.1638 12.3062 10.3138C12.4463 10.5286 12.6038 10.7172 12.7054 10.839C12.7126 10.8476 12.7196 10.8559 12.7262 10.8638C12.7403 10.882 12.7537 10.8994 12.7662 10.9157C12.8156 10.9801 12.8522 11.0279 12.8762 11.0438C13.2062 11.4438 13.5762 11.8038 13.9062 12.0838C13.9862 12.1638 14.0562 12.2238 14.0762 12.2338C14.2662 12.3938 14.4662 12.5538 14.6362 12.6638C14.8462 12.8138 15.0562 12.9538 15.2762 13.0838Z" fill="#87868C"></path>
                                                            </svg>
                                                        </div>
                                                        <div class="btn_remove_location_item">
                                                            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                                                <path d="M21.0699 5.23C19.4599 5.07 17.8499 4.95 16.2299 4.86V4.85L16.0099 3.55C15.8599 2.63 15.6399 1.25 13.2999 1.25H10.6799C8.34991 1.25 8.12991 2.57 7.96991 3.54L7.75991 4.82C6.82991 4.88 5.89991 4.94 4.96991 5.03L2.92991 5.23C2.50991 5.27 2.20991 5.64 2.24991 6.05C2.28991 6.46 2.64991 6.76 3.06991 6.72L5.10991 6.52C10.3499 6 15.6299 6.2 20.9299 6.73C20.9599 6.73 20.9799 6.73 21.0099 6.73C21.3899 6.73 21.7199 6.44 21.7599 6.05C21.7899 5.64 21.4899 5.27 21.0699 5.23Z" fill="#E60000"></path>
                                                                <path d="M19.23 8.14C18.99 7.89 18.66 7.75 18.32 7.75H5.67999C5.33999 7.75 4.99999 7.89 4.76999 8.14C4.53999 8.39 4.40999 8.73 4.42999 9.08L5.04999 19.34C5.15999 20.86 5.29999 22.76 8.78999 22.76H15.21C18.7 22.76 18.84 20.87 18.95 19.34L19.57 9.09C19.59 8.73 19.46 8.39 19.23 8.14ZM13.66 17.75H10.33C9.91999 17.75 9.57999 17.41 9.57999 17C9.57999 16.59 9.91999 16.25 10.33 16.25H13.66C14.07 16.25 14.41 16.59 14.41 17C14.41 17.41 14.07 17.75 13.66 17.75ZM14.5 13.75H9.49999C9.08999 13.75 8.74999 13.41 8.74999 13C8.74999 12.59 9.08999 12.25 9.49999 12.25H14.5C14.91 12.25 15.25 12.59 15.25 13C15.25 13.41 14.91 13.75 14.5 13.75Z" fill="#E60000"></path>
                                                            </svg>
                                                        </div>
                                                    </div>` 
                                            : ``
                                    }
                                </div>
                            </div>
                            <div class="Content">
                                <div class="row">
                                    <div class="col-xl-9 col-lg-9 col-md-9 col-sm-9">
                                        <h5>${window.languageData[window.currentLanguage]['Nhà máy']}:</h5>
                                    </div>
                                    <div class="col-xl-3 col-lg-3 col-md-3 col-sm-3">
                                        <p>${item?.factoryNames || ""}</p>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-xl-9 col-lg-9 col-md-9 col-sm-9">
                                        <h5>${window.languageData[window.currentLanguage]['Phòng ban']}:</h5>
                                    </div>
                                    <div class="col-xl-3 col-lg-3 col-md-3 col-sm-3">
                                        <p class="txt-breakline">${item?.departmentName || ""}</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `
                })

                root.parentEl.find(".Assignment_Layout_Content").html(resultHTML);
                Cache();
            }
        }).catch(err => {
            toastr.error(err?.responseJSON?.message);


        }).finally(() => {
            loading(false);
        })
    }

    async function PreLoad() {
        await RenderList();


        let validateCreateModel = {
            rules: {
                'Name': {
                    required: true,
                    maxlength: 50
                },
                'DepartmentName': {
                    required: true,
                    maxlength: 20
                },
                //'FactoryNames[]': {
                //    required: true
                //}
            },
            messages: {
                "Name": {
                    required: window.languageData[window.currentLanguage]["Vui lòng nhập tên khu vực."],
                    maxlength: "Hệ thống chỉ cho phép nhập tối đa {0} kí tự"
                },
                "DepartmentName": {
                    required: window.languageData[window.currentLanguage]["Vui lòng nhập tên phòng ban."],
                    maxlength: "Hệ thống chỉ cho phép nhập tối đa {0} kí tự"
                }
            },
        }

        let validateEditModel = {
            rules: {
                'Inventory_Area_Name': {
                    required: true,
                    maxlength: 50
                },
                'Inventory_Department_Name': {
                    required: true,
                    maxlength: 20
                },
                //'FactoryNames[0]': {
                //    required: true,
                //    maxlength: 10
                //},
            },
            messages: {
                "Inventory_Area_Name": {
                    required: window.languageData[window.currentLanguage]["Vui lòng nhập tên khu vực."],
                    maxlength: "Hệ thống chỉ cho phép nhập tối đa {0} kí tự"
                },
                "Inventory_Department_Name": {
                    required: window.languageData[window.currentLanguage]["Vui lòng nhập tên phòng ban."],
                    maxlength: "Hệ thống chỉ cho phép nhập tối đa {0} kí tự"
                }
            },
        }

        createFormValidator = root.form_create_location.validate(validateCreateModel);
        editFormValidator = root.form_edit_location.validate(validateEditModel);

        //$("#form_create_location input[name^='FactoryNames']").each(function () {
        //    $(this).rules("add", { required: true, maxlength: 10 });
        //});

        //$("#form_create_location input[name^='FactoryNames']").each(function () {
        //    $(this).rules("add", { required: true, maxlength: 10 });
        //}); 
    }
    function waitForAreaLanguageData() {
        // Kiểm tra nếu dữ liệu đã sẵn sàng
        if (window.languageData && window.currentLanguage) {

            Cache()
            PreLoad()
            Events()

        } else {
            // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
            setTimeout(waitForAreaLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
        }
    }
    function Init() {
        if (root.parentEl?.length <= 0) {
            console.error("Không tìm thấy container")
            return
        }

        waitForAreaLanguageData();
       
    }

    return {
        init: Init,
        getLocationsAPI: GetLocationsAPI,
        renderList: RenderList
    }
})();



; var ActorHandler = (function () {
    let root = {
        parentEl: $(".InventoryAssignment_Employee")    
    }
    let locations = [];
    let dataTable;
    let virtualSelectRole, virtualSelectLocation;

    let API = {
        ChangeRole: function (accountId, roleType) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/location/account/${accountId}/role/${roleType}`;

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
        },
        ChangeLocation: function (accountId, locationIds) {
            return new Promise(async (resolve, reject) => {
                let url = `${host}/api/inventory/location/account/${accountId}`;

                try {
                    const res = await $.ajax({
                        url: url,
                        type: 'PUT',
                        contentType: 'application/json',
                        data: JSON.stringify(locationIds)
                    });
                    resolve(res)
                } catch (err) {
                    reject(err)
                }
            })
        }
    }

    function Cache() {
        root.$actorTable = root.parentEl.find("#InventoryAssignment_DataTable");

        root.btnExportFile = root.parentEl.find(".InventoryAssignment_ExportExcel");
    }
    async function PreLoad() {
        //let response = await InventoryAssignmentHandler.getLocationsAPI();
        //locations = [...response.data];

        dataTable = root.$actorTable.DataTable({
            "bDestroy": true,
            stateSave: true,
            "processing": `<div class="spinner"></div>`,
            pagingType: 'full_numbers',
            'language': {
                'loadingRecords': `<div class="spinner"></div>`,
                'processing': '<div class="spinner"></div>',
                'emptyTable': `Chưa có người thao tác.Vui lòng thêm mới để quản lý.`
            },
            "serverSide": true,
            "filter": true,
            "searching": false,
            responsive: true,
            "lengthMenu": [10, 30, 50, 200],
            dom: 'rt<"bottom"flp><"clear">',
            "ordering": false,
            "ajax": {
                "url": `${host}/api/inventory/location/actors`,
                "type": "POST",
                "contentType": "application/x-www-form-urlencoded",
                dataType: "json",
                data: function (data) {
                    return data;
                },
                "dataSrc": function (json) {
                    return json.data;
                }
            },
            "drawCallback": async function (settings) {
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

                //Khóa nút xuất file nếu dữ liệu trống
                if (totalRecords == 0) {
                    root.parentEl.find(".InventoryAssignment_ExportExcel").prop("disabled", true);
                    root.parentEl.find(".InventoryAssignment_ExportExcel").removeClass("btn_disabled").addClass("btn_disabled");
                } else {
                    root.parentEl.find(".InventoryAssignment_ExportExcel").prop("disabled", false);
                    root.parentEl.find(".InventoryAssignment_ExportExcel").removeClass("btn_disabled");
                }


                //Nếu chỉ có quyền xem
                let canEdit = (App.User.AccountType == AccountType.TaiKhoanRieng && App.User.isGrant("EDIT_INVENTORY")) || isPromoter();

                $(".select_location").map((i, el) => {
                    let options = $(el).find("option");
                    let accountId = $(el).attr("accountid");
                    if (options.length > 1) {
                        VirtualSelect.init({
                            ele: `.select_location[accountId="${accountId}"]`,
                            selectAllText: window.languageData[window.currentLanguage]['Tất cả'],
                            alwaysShowSelectedOptionsCount: false,
                            alwaysShowSelectedOptionsLabel: false,
                            disableAllOptionsSelectedText: false,
                            selectAllOnlyVisible: false,
                            noOptionsText: window.languageData[window.currentLanguage]['Không có kết quả'],
                            noSearchResultsText: window.languageData[window.currentLanguage]['Không có kết quả'],
                            searchPlaceholderText: window.languageData[window.currentLanguage]['Tìm kiếm'],
                            allOptionsSelectedText: window.languageData[window.currentLanguage]['Tất cả'],
                            optionsSelectedText: window.languageData[window.currentLanguage]["điều kiện đã được chọn"],
                            selectAllOnlyVisible: true,
                            hideClearButton: true,
                            focusSelectedOptionOnOpen: true,
                            disableSelectAll: !canEdit,
                            disabled: !canEdit,
                            showSelectedOptionsFirst: !canEdit,
                            maxWidth: "250px"
                        });
                    } else {
                        VirtualSelect.init({
                            ele: `.location_options_${i}, .select_location[accountId="${accountId}"]`,
                            selectAllText: window.languageData[window.currentLanguage]['Tất cả'],
                            alwaysShowSelectedOptionsCount: false,
                            alwaysShowSelectedOptionsLabel: true,
                            disableAllOptionsSelectedText: true,
                            selectAllOnlyVisible: false,
                            noOptionsText: window.languageData[window.currentLanguage]['Không có kết quả'],
                            noSearchResultsText: window.languageData[window.currentLanguage]['Không có kết quả'],
                            searchPlaceholderText: window.languageData[window.currentLanguage]['Tìm kiếm'],
                            allOptionsSelectedText: window.languageData[window.currentLanguage]['Tất cả'],
                            optionsSelectedText: window.languageData[window.currentLanguage]["điều kiện đã được chọn"],
                            selectAllOnlyVisible: true,
                            hideClearButton: true,
                            focusSelectedOptionOnOpen: true,
                            disableSelectAll: !canEdit,
                            disabled: !canEdit,
                            showSelectedOptionsFirst: !canEdit,
                            maxWidth: "250px"
                        });
                    }
                })


            },
            "columnDefs": [
                { "width": "15%", "targets": 0 },
                { "width": "25%", "targets": [4,5] },
                { orderable: false, targets: 1 },
                { orderable: false, targets: [0,2,3,4,5] },
            ],
            "columns": [
                {
                    "data": "",
                    "name": "STT",
                    "render": function (data, type, row, index) {
                        let pagesize = index.settings._iDisplayLength;
                        let currentRow = ++index.row;
                        let currentPage = dataTable.page() + 1;

                        let STT = ((currentPage - 1) * pagesize) + currentRow;

                        if (STT < 10) {
                            STT = `0${STT}`;
                        }
                        return STT;
                    },
                    "autoWidth": true
                },
                { "data": "userName", "name": "userName", sortable: true, "autoWidth": true },
                {
                    "data": "roleType", "name": "roleType", "autoWidth": true, render: function (data, type, row, index) {
                        let roleTypes = AppInventoryRoleTypes;

                        if (row.accountType == 2) {
                            roleTypes = roleTypes.filter(x => x == AuditAccountTypeValue);
                        }

                        //Nếu chỉ có quyền xem
                        let canEdit = (App.User.AccountType == AccountType.TaiKhoanRieng && App.User.isGrant("EDIT_INVENTORY")) ||
                                                    isPromoter();

                        if (!canEdit) {
                            return roleTypes[data] || ``;
                        }

                        let optionsHTML = roleTypes.map((name, i) => {
                            let indexOfType = AppInventoryRoleTypes.indexOf(name);
                            let isSelected = data == indexOfType ? "selected" : "";

                            return `
                                    <option value="${indexOfType}" ${isSelected}>${window.languageData[window.currentLanguage][name]}</option>
                                `
                        });

                        let requiredSelect = `<option value="">${window.languageData[window.currentLanguage]['Chọn vai trò']}</option>`;

                        return `
                                <select class="select_role"
                                    data-search="true"
                                    data-silent-initial-value-set="true"
                                    accountId = "${row.userId}"
                                    locationId = "${row.locationId}">
                                    ${requiredSelect}
                                    ${optionsHTML}
                                </select>
                            `;
                    }
                },
                {
                    "data": "locationId", "name": "locationId", "autoWidth": true, render: function (data, type, row, index) {
                        locations = row.allLocations || [];

                        getAllLocations = row.locations || [];
                        let locationExcepts = [];
                        //Nếu chỉ có quyền xem
                        let canEdit = (App.User.AccountType == AccountType.TaiKhoanRieng && App.User.isGrant("EDIT_INVENTORY")) || isPromoter();
                        if (!canEdit) {
                            if (row.roleType == 1) {
                                let assignLocation = locations.some(x => x.isAssignedAudit == row.userId);
                                if (!assignLocation) return ``;
                            }

                            let assignLocation = locations.some(x => x.userInventoryId == row.userId);
                            if (!assignLocation) return ``;
                        }

                        if (locations.length > 0) {
                            let role = row.roleType;
                            let requiredSelect = !data ? `<option value="-1">Chọn khu vực</option>` : "";

                            //Nếu là xúc tiến thì tất cả khu vực
                            if (role == 2) {
                                return `${window.languageData[window.currentLanguage]['Tất cả']}`;
                            }

                            //Nếu role là giám sát thì phần select cho chọn nhiều khu vực
                            if (role == 1) {
                                let resultHtml = locations.map((location, i) => {
                                    if (location.isAssignedAudit && location.userAuditId == row.userId) {
                                        locationExcepts.push(location.name);
                                        return `<option userId="${location.userAuditId}" type="${row.roleType}" value="${location.id}" selected>${location.name}</option>`
                                    } 
                                }).join("");

                                let resultExceptHtml = getAllLocations.map((allLocation, i) => {
                                    if (!locationExcepts.includes(allLocation.locationName)) {
                                        return `<option userId="${row.userId}" type="${row.roleType}" value="${allLocation.locationId}">${allLocation.locationName}</option>`
                                    } 
                                   
                                }).join("");

                                return `<select class="select_location location_options_${index.row} ${row.id}" 
                                        accountId="${row.userId}" 
                                        locationId="${row.locationId}"
                                        multiple 
                                        data-search="true" 
                                        data-silent-initial-value-set="true"
                                        placeholder="Chọn khu vực"
                                        >
                                        ${resultHtml + resultExceptHtml} 
                                    </select>`;
                            }

                            let resultHtml = locations.map((location, i) => {
                                if (location.isAssignedInventory && location.userInventoryId == row.userId) {
                                    locationExcepts.push(location.name);
                                    return `<option userId="${location.userInventoryId}" type="${row.roleType}" value="${location.id}" selected>${location.name}</option>`
                                } 
                            }).join("");

                            let resultExceptHtml = getAllLocations.map((allLocation, i) => {
                                if (!locationExcepts.includes(allLocation.locationName)) {
                                    return `<option userId="${row.userId}" type="${row.roleType}" value="${allLocation.locationId}">${allLocation.locationName}</option>`
                                }

                            }).join("");

                            return `<select class="select_location location_options_${index.row} ${row.id}" 
                                        accountId="${row.userId}" 
                                        locationId="${row.locationId}"
                                        data-search="true" 
                                        data-maxValues="1"
                                        data-silent-initial-value-set="true"
                                        placeholder="Chọn khu vực"
                                         >
                                        ${requiredSelect}
                                        ${resultHtml + resultExceptHtml} 
                                    </select>`;
                        } else {
                            return ``;
                        }
                    }
                },
                {
                    "data": "factoryName", "name": "factoryName", render: function (data, type, row, index) {
                        let value = "";

                        //Nếu là xúc tiến thì tất cả khu vực
                        if (row.roleType == 2) {
                            return `${window.languageData[window.currentLanguage]['Tất cả']}`;
                        }

                        if (row.roleType == 1) {
                            value = row.allLocations.filter(x => x.userAuditId == row.userId).map(x => x.factoryName);
                            value = [...new Set(value)].join(', ');


                        } else {
                            value = row.allLocations.filter(x => x.userInventoryId == row.userId).map(x => x.factoryName);
                            value = [...new Set(value)].join(', ');
                        }

                        return value;
                    }, "autoWidth": true
                },
                {
                    "data": "departmentName", "name": "departmentName", orderable: false, render: function (data, type, row, index) {
                        let value = "";

                        //Nếu là xúc tiến thì tất cả khu vực
                        if (row.roleType == 2) {
                            return `${window.languageData[window.currentLanguage]['Tất cả']}`;
                        }

                        if (row.roleType == 1) {
                            value = row.allLocations.filter(x => x.userAuditId == row.userId).map(x => x.departmentName);
                            value = [...new Set(value)].join(', ');

                        } else {
                            value = row.allLocations.filter(x => x.userInventoryId == row.userId).map(x => x.departmentName);
                            value = [...new Set(value)].join(', ');
                        }
                        return value;
                    }, "autoWidth": true
                },
            ],
        });
    }

    function ExportAssignmentAPI() {
        return new Promise(async (resolve, reject) => {
            let host = App.ApiGateWayUrl;
            let url = `${host}/api/inventory/location/assignment/export`;

            var xhr = new XMLHttpRequest();
            xhr.open('GET', url, true);
            //xhr.setRequestHeader('Content-Type', false);
            xhr.setRequestHeader('Authorization', `Bearer ${App.Token}`);
            xhr.processData = false;

            xhr.onreadystatechange = function () {
                if (xhr.readyState === 4) {
                    if (xhr.status == 200) {
                        resolve(xhr.response);
                    } else {
                        reject(JSON.parse(xhr.responseText));
                    }
                } else if (xhr.readyState == 2) {
                    if (xhr.status == 200) {
                        xhr.responseType = 'blob';
                    } else {
                        xhr.responseType = 'text';
                    }
                }
            };

            xhr.send();
        })
    }

    function Events() {
        
        root.$actorTable.delegate(".select_role", "change", function (e) {
            let canEdit = (App.User.AccountType == AccountType.TaiKhoanRieng && App.User.isGrant("EDIT_INVENTORY")) || isPromoter();
            if (!canEdit) {
                toastr.error("Bạn không có quyền chỉnh sửa.")
                return;
            }

            let thisBtn = $(this).closest(".select_role");

            let accountId = $(thisBtn).attr("accountId");
            let roleType = $(thisBtn).find("option:selected").val();

            //console.log(roleType)

            loading(true);
            API.ChangeRole(accountId, roleType).then(res => {
                toastr.success(res.message);

                //Reload giữ nguyên trang hiển tại
                dataTable.ajax.reload(null, false);
                //dataTable.draw();
            }).catch(err => {
                toastr.error(err?.responseJSON?.message);
            }).finally(() => {
                loading(false);
            })
        })


        root.$actorTable.delegate(".select_location", "change", function (e) {
            let canEdit = (App.User.AccountType == AccountType.TaiKhoanRieng && App.User.isGrant("EDIT_INVENTORY")) || isPromoter();

            if (!canEdit) {
                let options = e.target.options;
                let selectedOption = e.target.getSelectedOptions()
                toastr.error("Bạn không có quyền chỉnh sửa.")
                return;
            }

            let thisBtn = $(this).closest(".select_location");

            let rowIndex = $(this).closest("tr").index();
            let rowDataModelValue = dataTable.row($(this).closest("tr")).data();

            let selectedValue = thisBtn.val();
            let roleType = rowDataModelValue.roleType;

            if (rowDataModelValue.roleType == null && selectedValue == "-1") {
                return;
            }

            if (rowDataModelValue.roleType == null) {
                toastr.error("Vui lòng gán vai trò cho người thao tác trước khi thực hiện gán khu vực.");
                thisBtn[0].setValue("-1");
                return;
            }

            let userId = rowDataModelValue.userId;
            let locationIds;
            if (typeof selectedValue == "string") {
                locationIds = [selectedValue];
            } else {
                locationIds = this.value;
            }

            //loading(true);
            API.ChangeLocation(userId, locationIds).then(res => {
                toastr.success(res.message);

                //Reload giữ nguyên trang hiển tại
                //dataTable.draw();
                dataTable.ajax.reload(null, false);
                //dataTable.api().ajax.reload();
            }).catch(err => {
                toastr.error(err?.responseJSON?.message);
            }).finally(() => {
                //loading(false);
            })
        })

        root.btnExportFile.click(function (e) {
            loading(true);
            ExportAssignmentAPI().then((res) => {
                let fileName = "ExportInventoryAssignment.xlsx";
                let blob = new Blob([res], { type: res.type });
                let fileUrl = URL.createObjectURL(blob);

                var a = $("<a style='display: none;'/>");
                a.attr("href", fileUrl);
                a.attr("download", fileName);
                $("body").append(a);
                a[0].click();

                //Clear temp data
                window.URL.revokeObjectURL(fileUrl);
                a.remove();

            }).catch(err => {
                Swal.fire({
                    title: `<b>Thông báo</b>`,
                    text: err?.message || "",
                    confirmButtonText: "Đã hiểu",
                    width: '30%'
                })
            }).finally(() => {
                loading(false);
            })
        })

        
    }

    function Init() {
        if (root.parentEl?.length <= 0) {
            console.error("Không tìm thấy container")
            return
        }

        Cache()
        Events()
    }

    return {
        init: Init,
        preLoad: PreLoad
    }
})();