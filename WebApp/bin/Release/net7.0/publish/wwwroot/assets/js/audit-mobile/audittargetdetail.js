$(function () {
    //Thêm mới dòng
    //AddNewRow();

    //Xóa bỏ dòng đã chọn
    //RemoveRow();

    //Focus input => Show button Delete
    //FocusInputThenShowButtonDelete();

    //Thêm ghi chú
    AddNote();
    ShowAndHideSuccessButton();
    ClickSuccessAudit();
    ClickFailAudit();
    ClickUpdateAudit();
})

function AddNewRow(){
    $(document).delegate(".AddNewRow_btn", "click", (e) => {
        var newRow = `<tr>
                    <td class="bt-bb-none AuditTargetDetail_DataTable_Delete opacity-0" style="border:none">
                    </td>
                    <td class="border_left border_is_delete">
                        <div class="QuantityInput_Container">
                            <input type="text" class="QuantityPerBom" placeholder="Nhập số lượng/thùng" />
                        </div>
                    </td>
                    <td class="border_right border_is_delete">
                        <div class="QuantityInput_Container">
                            <input type="text" class="QuantityOfBom" placeholder="Nhập số thùng" />
                        </div>
                    </td>
                    <td class="bt-bb-none" style="border:none">
                        <div class="d-flex mx-1">
                            <label class="form-check-label label"></label>
                            <input type="checkbox" class="form-check-input CheckBoxItem"/>
                        </div>
                    </td>
                </tr>`

        $("#AuditTargetDetail_DataTable tbody").append(newRow);
    })
}

function RemoveRow() {
    $(document).delegate(".AuditTargetDetail_DataTable_Delete", "click", (e) => {
        let isHideDelete = $(e.target).closest('tr').find(".AuditTargetDetail_DataTable_Delete").hasClass('opacity-0');
        if (isHideDelete) return;

        $(e.target).closest('tr').remove();
    })
}

function AddNote() {
    $("#icon-add-note").click(function(){
        var currentSrc = $(this).attr("src");
        if (currentSrc == "/assets/images/audit_target_mobile/minus.png") {
          $(this).attr("src", "/assets/images/audit_target_mobile/add.png");
          $(".AuditTargetDetail_Comment_TextArea").hide();
        } else {
          $(this).attr("src", "/assets/images/audit_target_mobile/minus.png");
          $(".AuditTargetDetail_Comment_TextArea").show();
        }
    });
}

function FocusInputThenShowButtonDelete() {
    //Khi không focus vào ô input thì ẩn nút xóa
    $('#AuditTargetDetail_DataTable').on('mouseleave', function () {
        $(".AuditTargetDetail_DataTable_Delete").removeClass("opacity-1");
        $(".AuditTargetDetail_DataTable_Delete").addClass("opacity-0");
    });

    //Khi focus vào nút xóa thì hiển thị nút Xóa:
    $(document).delegate("input.QuantityPerBom, input.QuantityOfBom", "focus", (e) => {

        //Hiện nút delete:
        $(".AuditTargetDetail_DataTable_Delete").removeClass("opacity-1");
        $(".AuditTargetDetail_DataTable_Delete").addClass("opacity-0");


        $(e.target).closest("tr").find(".AuditTargetDetail_DataTable_Delete").removeClass("opacity-0");
        $(e.target).closest("tr").find(".AuditTargetDetail_DataTable_Delete").addClass("opacity-1");

        ////Đổi border left:
        //$(e.target).closest("tr").find(".border_left").removeClass("border_is_delete");
        //$(e.target).closest("tr").find(".border_left").addClass("border_is_delete_left");

        ////Đổi border right:
        //$(e.target).closest("tr").find(".border_right").removeClass("border_is_delete");
        //$(e.target).closest("tr").find(".border_right").addClass("border_is_delete_right");

    })
}

function ShowAndHideSuccessButton() {
    $('#AuditTargetDetail_DataTable input.CheckBoxItem').change(function () {
        var checkedCount = $('#AuditTargetDetail_DataTable input.CheckBoxItem:checked').length;
        var totalCount = $('#AuditTargetDetail_DataTable input.CheckBoxItem').length;
        if (checkedCount === totalCount) {
            $('.SuccessAudit_Btn').prop('disabled', false);
            $('.SuccessAudit_Btn').removeClass('opacity-haftpart-1');

            $('.UpdateAudit_Btn').prop('disabled', false);
            $('.UpdateAudit_Btn').removeClass('opacity-haftpart-1');
        } else {
            $('.SuccessAudit_Btn').prop('disabled', true);
            $('.SuccessAudit_Btn').addClass('opacity-haftpart-1');

            $('.UpdateAudit_Btn').prop('disabled', true);
            $('.UpdateAudit_Btn').addClass('opacity-haftpart-1');
        }
    });
}

function SubmitAuditAPI(inventoryId, accountId, docId, actionType, filterData) {
    return new Promise(async (resolve, reject) => {
        var formData = new FormData();

        // Thêm dữ liệu vào FormData
        formData.append('UserCode', filterData.UserCode);
        formData.append('Comment', filterData.Comment);
        formData.append('IsAuditWebsite', filterData.IsAuditWebsite);
        filterData.DocOutputs.forEach((docOutput, index) => {
            formData.append(`DocOutputs[${index}].Id`, docOutput.Id);
            formData.append(`DocOutputs[${index}].QuantityOfBom`, docOutput.QuantityOfBom);
            formData.append(`DocOutputs[${index}].QuantityPerBom`, docOutput.QuantityPerBom);
        });

        $.ajax({
            type: "POST",
            url: `${AppUser.getApiGateway}/api/inventory/${inventoryId}/account/${accountId}/document/${docId}/action/${actionType}/submit-audit`,
            contentType: false,
            processData: false,
            data: formData,
            success: function (res) {
                resolve(res);
            },
            error: function (err) {
                reject(err)
            }
        });
    })
}
//Click nút Giám sát đạt:
function ClickSuccessAudit() {
    $(document).delegate(".SuccessAudit_Btn", "click", async (e) => {
        let inventoryInfo = await AppUser.getUser().inventoryLoggedInfo();
        let inventoryId = inventoryInfo.inventoryModel.inventoryId;
        let accountId = AppUser.getUser().userId();
        let userCode = AppUser.getUser().userCode;
        let comment = $(".AuditTargetDetail_Comment_TextArea .Comment_Here").val();
        let actionType = 2;

        let urlCurrent = window.location.href;
        var parts = urlCurrent.split('/');
        var docId = parts[parts.length - 1];

        var docOutputs = [];

        $("#AuditTargetDetail_DataTable .DocOutputs").map((index, item) => {
            var id = $(`.DocOutputs:eq(${index})`).attr("value");
            var quantityOfBom = $(`.DocOutputs:eq(${index})`).find(".QuantityOfBom").val();
            var quantityPerBom = $(`.DocOutputs:eq(${index})`).find(".QuantityPerBom").val();
            docOutputs.push({
                Id: id,
                QuantityOfBom: quantityOfBom,
                QuantityPerBom: quantityPerBom
            })
        })
        
        var isAuditWebsite = true;

        let filterData = {
            UserCode: userCode,
            Comment: comment,
            IsAuditWebsite: isAuditWebsite,
            DocOutputs: docOutputs

        }
        loading(true)

        SubmitAuditAPI(inventoryId, accountId, docId, actionType, filterData).then(res => {
            if (res.code == 200) {
                toastr.success(res.message);

                let statusName = {
                    0: "Chưa tiếp nhận",
                    1: "Không kiểm kê",
                    2: "Chưa kiểm kê",
                    3: "Chờ xác nhận",
                    4: "Cần chỉnh sửa",
                    5: "Đã xác nhận",
                    6: "Giám sát đạt",
                    7: "Giám sát không đạt",
                }

                let statusColor = {
                    0: "color-0D2EA0",
                    1: "color-333333",
                    2: "color-87868C",
                    3: "color-F3A600",
                    4: "color-ED7200",
                    5: "color-17AE5C",
                    6: "color-5092FC",
                    7: "color-E60000",
                }

                let oldStatus = $(".OldStatusDocDetail").attr("oldstatus");
                let newStatus = res?.data?.status;

                $(".OldStatusDocDetail").text(`${statusName[newStatus]}`);

                $(".OldStatusDocDetail").removeClass(`${statusColor[oldStatus]}`)
                $(".OldStatusDocDetail").addClass(`${statusColor[newStatus]}`)

                
                setTimeout((e) => {
                    loading(false);
                    window.location.href = "/auditmobile";
                }, 3000)
            }
            else if (res.code == 96) {
                toastr.error(res.message);
                loading(false);
            }
            else if (res.code == 64) {
                toastr.error(res.message);
                loading(false);
            }
            else if (res.code == 66) {
                toastr.error(res.message);
                loading(false);
            }
            else if (res.code == 403) {
                toastr.error(res.message);
                loading(false);
            }
        }).catch(err => {
            loading(false);
            toastr.error(err.responseJSON.message);
        })
    })
}


//Click nút Không đạt giám sát:
function ClickFailAudit() {
    $(document).delegate(".FailAudit_Btn", "click", async (e) => {
        let inventoryInfo = await AppUser.getUser().inventoryLoggedInfo();
        let inventoryId = inventoryInfo.inventoryModel.inventoryId;
        let accountId = AppUser.getUser().userId();
        let userCode = AppUser.getUser().userCode;
        let comment = $(".AuditTargetDetail_Comment_TextArea .Comment_Here").val();
        let actionType = 3;

        let urlCurrent = window.location.href;
        var parts = urlCurrent.split('/');
        var docId = parts[parts.length - 1];

        var docOutputs = [];

        $("#AuditTargetDetail_DataTable .DocOutputs").map((index, item) => {
            var id = $(`.DocOutputs:eq(${index})`).attr("value");
            var quantityOfBom = $(`.DocOutputs:eq(${index})`).find(".QuantityOfBom").val();
            var quantityPerBom = $(`.DocOutputs:eq(${index})`).find(".QuantityPerBom").val();
            docOutputs.push({
                Id: id,
                QuantityOfBom: quantityOfBom,
                QuantityPerBom: quantityPerBom
            })
        })



        var isAuditWebsite = true;

        let filterData = {
            UserCode: userCode,
            Comment: comment,
            IsAuditWebsite: isAuditWebsite,
            DocOutputs: docOutputs

        }
        loading(true)

        SubmitAuditAPI(inventoryId, accountId, docId, actionType, filterData).then(res => {
            if (res.code == 200) {
                toastr.error(res.message);
                let statusName = {
                    0: "Chưa tiếp nhận",
                    1: "Không kiểm kê",
                    2: "Chưa kiểm kê",
                    3: "Chờ xác nhận",
                    4: "Cần chỉnh sửa",
                    5: "Đã xác nhận",
                    6: "Giám sát đạt",
                    7: "Giám sát không đạt",
                }

                let statusColor = {
                    0: "color-0D2EA0",
                    1: "color-333333",
                    2: "color-87868C",
                    3: "color-F3A600",
                    4: "color-ED7200",
                    5: "color-17AE5C",
                    6: "color-5092FC",
                    7: "color-E60000",
                }

                let oldStatus = $(".OldStatusDocDetail").attr("oldstatus");
                let newStatus = res?.data?.status;

                $(".OldStatusDocDetail").text(`${statusName[newStatus]}`);

                $(".OldStatusDocDetail").removeClass(`${statusColor[oldStatus]}`)
                $(".OldStatusDocDetail").addClass(`${statusColor[newStatus]}`)

                setTimeout((e) => {
                    loading(false);
                    window.location.href = "/auditmobile";
                }, 3000)
            }
            else if (res.code == 96) {
                toastr.error(res.message);
                loading(false);
            }
            else if (res.code == 64) {
                toastr.error(res.message);
                loading(false);
            }
            else if (res.code == 66) {
                toastr.error(res.message);
                loading(false);
            }
            else if (res.code == 403) {
                toastr.error(res.message);
                loading(false);
            }
        }).catch(err => {
            loading(false);
            toastr.error(err.responseJSON.message);
        })
    })
}

//Click nút cập nhật giám sát:
function ClickUpdateAudit() {
    $(document).delegate(".UpdateAudit_Btn", "click", async (e) => {
        let inventoryInfo = await AppUser.getUser().inventoryLoggedInfo();
        let inventoryId = inventoryInfo.inventoryModel.inventoryId;
        let accountId = AppUser.getUser().userId();
        let userCode = AppUser.getUser().userCode;
        let comment = $(".AuditTargetDetail_Comment_TextArea .Comment_Here").val();
        let actionType = 4;

        let urlCurrent = window.location.href;
        var parts = urlCurrent.split('/');
        var docId = parts[parts.length - 1];

        var docOutputs = [];

        $("#AuditTargetDetail_DataTable .DocOutputs").map((index, item) => {
            var id = $(`.DocOutputs:eq(${index})`).attr("value");
            var quantityOfBom = $(`.DocOutputs:eq(${index})`).find(".QuantityOfBom").val();
            var quantityPerBom = $(`.DocOutputs:eq(${index})`).find(".QuantityPerBom").val();
            docOutputs.push({
                Id: id,
                QuantityOfBom: quantityOfBom,
                QuantityPerBom: quantityPerBom
            })
        })



        var isAuditWebsite = true;

        let filterData = {
            UserCode: userCode,
            Comment: comment,
            IsAuditWebsite: isAuditWebsite,
            DocOutputs: docOutputs

        }
        loading(true)

        SubmitAuditAPI(inventoryId, accountId, docId, actionType, filterData).then(res => {
            if (res.code == 200) {
                toastr.success(res.message);

                let statusName = {
                    0: "Chưa tiếp nhận",
                    1: "Không kiểm kê",
                    2: "Chưa kiểm kê",
                    3: "Chờ xác nhận",
                    4: "Cần chỉnh sửa",
                    5: "Đã xác nhận",
                    6: "Giám sát đạt",
                    7: "Giám sát không đạt",
                }

                let statusColor = {
                    0: "color-0D2EA0",
                    1: "color-333333",
                    2: "color-87868C",
                    3: "color-F3A600",
                    4: "color-ED7200",
                    5: "color-17AE5C",
                    6: "color-5092FC",
                    7: "color-E60000",
                }

                let oldStatus = $(".OldStatusDocDetail").attr("oldstatus");
                let newStatus = res?.data?.status;

                $(".OldStatusDocDetail").text(`${statusName[newStatus]}`);

                $(".OldStatusDocDetail").removeClass(`${statusColor[oldStatus]}`)
                $(".OldStatusDocDetail").addClass(`${statusColor[newStatus]}`)

                setTimeout((e) => {
                    loading(false);
                    window.location.href = "/auditmobile";
                }, 3000)
            }
            else if (res.code == 96) {
                toastr.error(res.message);
                loading(false);
            }
            else if (res.code == 64) {
                toastr.error(res.message);
                loading(false);
            }
            else if (res.code == 66) {
                toastr.error(res.message);
                loading(false);
            }
            else if (res.code == 403) {
                toastr.error(res.message);
                loading(false);
            }
        }).catch(err => {
            loading(false);
            toastr.error(err.responseJSON.message);
        })
    })
}