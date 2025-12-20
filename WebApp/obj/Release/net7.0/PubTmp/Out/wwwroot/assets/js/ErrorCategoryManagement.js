let ErrorInvestigationDatatable;
let ErrorInvestigationDocumentDatatable;

let maxLengthSearchDetailTypeC = 10;
let maxLengthCreateInventoryPlant = 4;
let maxLengthCreateInventoryWhLoc = 4;
let maxLengthCreateInventoryQuantityFrom = 10;
let maxLengthCreateInventoryQuantityTo = 10;
let maxLengthCreateInventoryComponentCode = 12;
let maxLengthCreateInventoryModelCode = 11;
let maxLengthCreateInventoryUserDistribution = 20;
let maxLengthErrorInvestigationConfirmErrorQuantity = 9;

$(function () {
    waitForErrorCategoryMamangementLanguageData();
});

function waitForErrorCategoryMamangementLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        GetErrorCategoryManagementData();
        ValidateErrorCategoryManagement();
        AddNewErrorCategoryManagement();
        RemoveErrorCategoryManagement();
        EditErrorCategoryManagement();

    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForErrorCategoryMamangementLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}

function RemoveErrorCategoryManagement() {
    $(document).off("click", ".btnRemove_ErrorCategoryManagement").on("click", ".btnRemove_ErrorCategoryManagement", (e) => {
        e.preventDefault();
        let host = App.ApiGateWayUrl;
        var inventoryId = App.User.InventoryLoggedInfo.InventoryModel.InventoryId;
        let errorCategoryId = $(e.target).closest(".btnRemove_ErrorCategoryManagement").attr("errorcategoryid");

        Swal.fire({
            title: `<b>${window.languageData[window.currentLanguage]['Xác nhận xóa']}</b>`,
            text: `${window.languageData[window.currentLanguage]['Bạn có chắc chắn muốn xóa phân loại lỗi này?']}`,
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
                loading(true)
                var url = host + `/api/error-investigation/web/management/error-category/${errorCategoryId}/remove`;
                $.ajax({
                    type: 'DELETE',
                    url: url,
                    contentType: 'application/json',
                    cache: false,
                    success: function (response) {
                        toastr.success(window.languageData[window.currentLanguage]["Xóa phân loại lỗi thành công."]);
                        GetErrorCategoryManagementData();
                    },
                    error: function (error) {
                        console.log(error);
                    },
                    complete: function () {
                        loading(false);
                    }
                });

            }
        });
    })

}

function EditErrorCategoryManagement() {
    $(document).off("click", "#btnUpdate_ErrorCategoryManagement").on("click", "#btnUpdate_ErrorCategoryManagement", (e) => {
        e.preventDefault();
        let host = App.ApiGateWayUrl;
        let errorCategoryId = $(e.target).closest(".btnUpdate_ErrorCategoryManagement").attr("errorcategoryid");
        $.ajax({
            type: 'GET',
            url: host + `/api/error-investigation/web/management/error-category/${errorCategoryId}`,
            success: function (response) {
                let data = response?.data;
                $("#EditInputErrorCategoryName").val(data?.errorCategoryName ?? "");
                $("#btnConfirmEditErrorCategory").attr("errorcategoryid", data?.id);
            },
            error: function (error) {
                var err = error?.responseJSON;
                toastr.error(window.languageData[window.currentLanguage][err?.message]);
            }
        });

        $("#EditErrorManagementModal").modal("show");
    })
    $(document).off("click", "#btnConfirmEditErrorCategory").on("click", "#btnConfirmEditErrorCategory", (e) => {
        e.preventDefault();
        let host = App.ApiGateWayUrl;
        var inventoryId = App.User.InventoryLoggedInfo.InventoryModel.InventoryId;
        let errorCategoryId = $(e.target).attr("errorcategoryid");
        var valid = $("#EditErrorCategoryManagementForm").valid();
        if (valid) {

            let dataFilter = {
                Name: ""
            };
            dataFilter.Name = $("#EditInputErrorCategoryName").val();
            loading(true)
            var url = host + `/api/error-investigation/web/management/error-category/${errorCategoryId}/edit`;
            $.ajax({
                type: 'PUT',
                url: url,
                data: JSON.stringify(dataFilter),
                contentType: 'application/json',
                cache: false,
                success: function (response) {
                    $("#EditErrorManagementModal").modal("hide");
                    toastr.success(window.languageData[window.currentLanguage]["Chỉnh sửa phân loại lỗi thành công."]);
                    GetErrorCategoryManagementData();
                },
                error: function (error) {
                    var err = error?.responseJSON;
                    toastr.error(window.languageData[window.currentLanguage][err?.message]);
                },
                complete: function () {
                    loading(false);
                }
            });
        }

    })

}

function AddNewErrorCategoryManagement() {
    $(document).off("click", "#btnCreate_ErrorCategoryManagement").on("click", "#btnCreate_ErrorCategoryManagement", (e) => {
        e.preventDefault();
        $("#InputErrorCategoryName").val('');
        $("#AddNewErrorManagementModal").modal("show");
    })
    $(document).off("click", "#btnConfirmCreateErrorCategory").on("click", "#btnConfirmCreateErrorCategory", (e) => {
        e.preventDefault();
        let host = App.ApiGateWayUrl;
        var inventoryId = App.User.InventoryLoggedInfo.InventoryModel.InventoryId;
        var valid = $("#newErrorCategoryManagementForm").valid();
        if (valid) {
            
            let dataFilter = {
                Name: ""
            };
            dataFilter.Name = $("#InputErrorCategoryName").val();
            loading(true)
            var url = host + `/api/error-investigation/web/management/error-category/add`;
            $.ajax({
                type: 'POST',
                url: url,
                data: JSON.stringify(dataFilter),
                contentType: 'application/json',
                cache: false,
                success: function (response) {
                    $("#AddNewErrorManagementModal").modal("hide");
                    toastr.success(window.languageData[window.currentLanguage]["Thêm mới phân loại lỗi thành công."]);
                    GetErrorCategoryManagementData();
                },
                error: function (error) {
                    var err = error?.responseJSON;
                    toastr.error(window.languageData[window.currentLanguage][err?.message]);
                },
                complete: function () {
                    loading(false);
                }
            });
        }
        
    })
    
}

function ValidateErrorCategoryManagement() {
    $("#newErrorCategoryManagementForm").validate({
        rules: {
            InputErrorCategoryName: {
                required: true
            }
        },
        messages: {
            InputErrorCategoryName: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập tên phân loại."]
            }
        }
    });

    $("#EditErrorCategoryManagementForm").validate({
        rules: {
            EditInputErrorCategoryName: {
                required: true
            }
        },
        messages: {
            EditInputErrorCategoryName: {
                required: window.languageData[window.currentLanguage]["Vui lòng nhập tên phân loại."]
            }
        }
    });
}

function GetErrorCategoryManagementData() {
    let host = App.ApiGateWayUrl;
    $.ajax({
        type: 'GET',
        url: host + `/api/error-investigation/web/management/error-category`,
        success: function (response) {
            let data = response?.data; 
            let option = "";
            if (data.length > 0) {
                data.forEach(item => {
                    option += `<div class="col-xl-3 col-lg-3 col-md-3 col-sm-3 ErrorCategoryManagement_Item mb-3 location_item">
                                <div class="Title">
                                    <div class="row">
                                        <div class="col-xl-9 col-lg-9 col-md-9 col-sm-9">
                                            <h4>${item.errorCategoryName}</h4>
                                        </div>
                                        <div class="col-xl-3 col-lg-3 col-md-3 col-sm-3 Image">
                                            <!--
                                            //20250404: Không cho phép chỉnh sửa phân loại lỗi:
                                            <div class="btnUpdate_ErrorCategoryManagement" id="btnUpdate_ErrorCategoryManagement" ErrorCategoryId="${item.id}">
                                                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                                    <path fill-rule="evenodd" clip-rule="evenodd" d="M13.6962 5.47381C15.6862 3.48381 17.5862 3.53381 19.5262 5.47381C20.5262 6.46381 21.0062 7.42381 20.9962 8.41381C20.9962 9.37381 20.5162 10.3238 19.5262 11.3038L18.3262 12.5138C18.2462 12.5938 18.1462 12.6338 18.0362 12.6338C17.9962 12.6338 17.9562 12.6238 17.9162 12.6138C15.2662 11.8538 13.1462 9.73381 12.3862 7.08381C12.3462 6.94381 12.3862 6.78381 12.4862 6.68381L13.6962 5.47381ZM15.2762 13.0838C15.5462 13.2438 15.8262 13.3838 16.1162 13.5238C16.1551 13.5407 16.1935 13.5572 16.2315 13.5734C16.5691 13.7175 16.667 14.1629 16.4075 14.4225L10.6862 20.1438C10.5662 20.2738 10.3162 20.3938 10.1362 20.4238L6.29618 20.9638C6.17618 20.9838 6.05618 20.9938 5.93618 20.9938C5.39618 20.9938 4.89618 20.8038 4.53618 20.4538C4.11618 20.0238 3.92618 19.3838 4.02618 18.7038L4.56618 14.8738C4.59618 14.7038 4.71618 14.4538 4.84618 14.3238L10.5746 8.59534C10.8324 8.33758 11.267 8.43503 11.4146 8.76834C11.4345 8.81325 11.455 8.85841 11.4762 8.90381C11.6162 9.18381 11.7562 9.45381 11.9162 9.72381C12.0462 9.94381 12.1862 10.1638 12.3062 10.3138C12.4463 10.5286 12.6038 10.7172 12.7054 10.839C12.7126 10.8476 12.7196 10.8559 12.7262 10.8638C12.7403 10.882 12.7537 10.8994 12.7662 10.9157C12.8156 10.9801 12.8522 11.0279 12.8762 11.0438C13.2062 11.4438 13.5762 11.8038 13.9062 12.0838C13.9862 12.1638 14.0562 12.2238 14.0762 12.2338C14.2662 12.3938 14.4662 12.5538 14.6362 12.6638C14.8462 12.8138 15.0562 12.9538 15.2762 13.0838Z" fill="#87868C"></path>
                                                </svg>
                                            </div> 
                                            -->
                                            <!--
                                            <div class="btnRemove_ErrorCategoryManagement" ErrorCategoryId="${item.id}">
                                                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                                    <path d="M21.0699 5.23C19.4599 5.07 17.8499 4.95 16.2299 4.86V4.85L16.0099 3.55C15.8599 2.63 15.6399 1.25 13.2999 1.25H10.6799C8.34991 1.25 8.12991 2.57 7.96991 3.54L7.75991 4.82C6.82991 4.88 5.89991 4.94 4.96991 5.03L2.92991 5.23C2.50991 5.27 2.20991 5.64 2.24991 6.05C2.28991 6.46 2.64991 6.76 3.06991 6.72L5.10991 6.52C10.3499 6 15.6299 6.2 20.9299 6.73C20.9599 6.73 20.9799 6.73 21.0099 6.73C21.3899 6.73 21.7199 6.44 21.7599 6.05C21.7899 5.64 21.4899 5.27 21.0699 5.23Z" fill="#E60000"></path>
                                                    <path d="M19.23 8.14C18.99 7.89 18.66 7.75 18.32 7.75H5.67999C5.33999 7.75 4.99999 7.89 4.76999 8.14C4.53999 8.39 4.40999 8.73 4.42999 9.08L5.04999 19.34C5.15999 20.86 5.29999 22.76 8.78999 22.76H15.21C18.7 22.76 18.84 20.87 18.95 19.34L19.57 9.09C19.59 8.73 19.46 8.39 19.23 8.14ZM13.66 17.75H10.33C9.91999 17.75 9.57999 17.41 9.57999 17C9.57999 16.59 9.91999 16.25 10.33 16.25H13.66C14.07 16.25 14.41 16.59 14.41 17C14.41 17.41 14.07 17.75 13.66 17.75ZM14.5 13.75H9.49999C9.08999 13.75 8.74999 13.41 8.74999 13C8.74999 12.59 9.08999 12.25 9.49999 12.25H14.5C14.91 12.25 15.25 12.59 15.25 13C15.25 13.41 14.91 13.75 14.5 13.75Z" fill="#E60000"></path>
                                                </svg>
                                            </div>
                                            -->
                                        </div>
                                    </div>
                                </div>
                            </div>`;
                });
                $(".ErrorCategoryManagement_List").html(option);
            } else {
                $(".ErrorCategoryManagement_List").html(option);
            }
        },
        error: function (error) {
            console.log(error);
        }
    });
}

