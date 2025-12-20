$(function () {
    waitForUserInfoLanguageData();
})
function waitForUserInfoLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        ShowOrHide_UpdateUserInfoModal()
        ShowOrHide_ChangePassword_UpdateUserInfoModal()
        UploadImage_ViewDetailModal()
        ShowOrHidePassword_ViewUserInfo()
        CheckInputPasswordValidate()
        RemoveEmptyFirstAndLast()
        Validation_ChangePassword_UpdateUserInfoModal()
        HandlePreventChangePassword_UpdateUserInfoModal()
        Apply_ChangePassword_UpdateUserInfoModal()
        GetUserInfo()
        CloseChangePasswordModal()

    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForUserInfoLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}

//Open Modal ViewDetailUserModal:
function ShowOrHide_UpdateUserInfoModal() {
    $("#ClickToViewDetailModal").click((e) => {
        $("#ViewDetailUserModal").modal("show")
    })
}

//Open Modal ChangePassword_ViewDetailUserModal:
function ShowOrHide_ChangePassword_UpdateUserInfoModal() {
    $(document).delegate("#ClickToChangePassword_ViewDetailUser", "click", (e) => {
        $("#ViewDetailUserModal").modal("hide")
        
        $.ajax({
            type: "GET",
            url: "change-password",
            contentType: "application/json",                     
            success: function () {
                $("#ChangePassword_UpdateUserInfoModal").modal("show")                
            },
            error: function (error) {                
                if (error != undefined) {
                    toastr.error(JSON.parse(error.responseText).message);
                }
            }
        });     
        
    })
}

//Upload Image Xem chi tiết người dùng
function UploadImage_ViewDetailModal() {

    $(document).delegate("#btnUploadAvatar_ViewDetailUser", "click", (e) => {
        e.preventDefault()
        $("#UploadAvatar_ViewDetailUser").click()

    })

    $("#UploadAvatar_ViewDetailUser").change(function (ev) {
        let files = $(ev.target)[0].files[0];
       
        if (files != null) {
            var checkImgResult = checkImage(files);
            if (checkImgResult.code == 200) {

                var url = URL.createObjectURL(files);
                $('.Image_Left_Content_ViewDetalUser img').attr("src", url);


                //Xem thông tin người dùng - Cập nhật ảnh vào DB Identity:
                UpdateUserInfo(files).then((res) => {
                })
            }
            else if (checkImgResult.code == 413) {
                toastr.error(window.languageData[window.currentLanguage]["Ảnh quá dung lượng cho phép. Vui lòng chọn lại."]);
            }
            else {
                toastr.error(window.languageData[window.currentLanguage]["Không đúng định dạng hình ảnh."]);
            }
        }
        

    })

}

//Ham validate Image:
//return error code: 413 for too large , 400 for wrong format
function checkImage(file) {
    if (file) {
        if (file.size / (1024 * 1024) > 5) {
            //alert("File anh lon 2MB")
            return {code:413}
        }
        else if (!file.name.toLowerCase().match(/\.(jpeg|gif|jpg|png|bmp|webp|svg|tiff)/)) {
            //alert("La anh")
            return { code: 400 }
        }
        return { code: 200 }
    }
    return { code: 400 }
}

//Ẩn hiện Password:
function ShowOrHidePassword_ViewUserInfo() {    
    ////Xem chi tiết người dùng - Thay đổi mật khẩu - MK cũ:
    //Show Password:
    $("#OldPassword_ChangePassword_UpdateUserInfo_Show").click(() => {
        $("#OldPassword_ChangePassword_UpdateUserInfo_Hide").show();
        $("#OldPassword_ChangePassword_UpdateUserInfo_Show").hide();
        $("#OldPassword_ChangePassword_UpdateUserInfo").attr("type", "text");
    })

    //Hide Password:
    $("#OldPassword_ChangePassword_UpdateUserInfo_Hide").click(() => {
        $("#OldPassword_ChangePassword_UpdateUserInfo_Show").show();
        $("#OldPassword_ChangePassword_UpdateUserInfo_Hide").hide();
        $("#OldPassword_ChangePassword_UpdateUserInfo").attr("type", "password");
    })

    ////Xem chi tiết người dùng - Thay đổi mật khẩu - MK mới:
    //Show Password:
    $("#NewPassword_ChangePassword_UpdateUserInfo_Show").click(() => {
        $("#NewPassword_ChangePassword_UpdateUserInfo_Hide").show();
        $("#NewPassword_ChangePassword_UpdateUserInfo_Show").hide();
        $("#NewPassword_ChangePassword_UpdateUserInfo").attr("type", "text");
    })

    //Hide Password:
    $("#NewPassword_ChangePassword_UpdateUserInfo_Hide").click(() => {
        $("#NewPassword_ChangePassword_UpdateUserInfo_Show").show();
        $("#NewPassword_ChangePassword_UpdateUserInfo_Hide").hide();
        $("#NewPassword_ChangePassword_UpdateUserInfo").attr("type", "password");
    })

    ////Xem chi tiết người dùng - Thay đổi mật khẩu - Xác nhận MK mới:
    //Show Password:
    $("#ConfirmNewPassword_ChangePassword_UpdateUserInfo_Show").click(() => {
        $("#ConfirmNewPassword_ChangePassword_UpdateUserInfo_Hide").show();
        $("#ConfirmNewPassword_ChangePassword_UpdateUserInfo_Show").hide();
        $("#ConfirmNewPassword_ChangePassword_UpdateUserInfo").attr("type", "text");
    })

    //Hide Password:
    $("#ConfirmNewPassword_ChangePassword_UpdateUserInfo_Hide").click(() => {
        $("#ConfirmNewPassword_ChangePassword_UpdateUserInfo_Show").show();
        $("#ConfirmNewPassword_ChangePassword_UpdateUserInfo_Hide").hide();
        $("#ConfirmNewPassword_ChangePassword_UpdateUserInfo").attr("type", "password");
    })

}

//Function Bắt nhập 1 ký tự chữ và 1 ký tự số:

function CheckInputPasswordValidate() {
    // Thêm phương thức xác thực tùy chỉnh cho mật khẩu:
    $.validator.addMethod("customPasswordValidate", function (value, element) {
        // Kiểm tra độ dài từ 8 đến 15 ký tự và chứa ít nhất một ký tự chữ và một ký tự số, không chứa khoảng trắng
        return this.optional(element) || /^(?=.*[a-zA-Z])(?=.*\d)(?!.*\s).{8,15}$/.test(value);        
    }, "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.");
}

//Function bỏ khoảng trắng ở đầu và cuối:
function RemoveEmptyFirstAndLast() {
    // Xử lý khi trường UserName_Login mất focus
    $("#OldPassword_ChangePassword_ViewDetailUser , #NewPassword_ChangePassword_ViewDetailUser , #ConfirmNewPassword_ChangePassword_ViewDetailUser").blur(function () {
        // Loại bỏ khoảng trắng ở hai đầu ký tự
        $(this).val($.trim($(this).val()));
    });
}

//Function Validate Xem chi tiết người dùng - Thay đổi mật khẩu:
function Validation_ChangePassword_UpdateUserInfoModal() {
    $("#ChangePassword_UpdateUserInfo_Form").validate({
        rules: {
            OldPassword_ChangePassword_UpdateUserInfo: {
                required: true,
                minlength: 8,
                maxlength: 15,
                customPasswordValidate: true,
            },
            NewPassword_ChangePassword_UpdateUserInfo: {
                required: true,
                minlength: 8,
                maxlength: 15,
                customPasswordValidate: true,
            },
            ConfirmNewPassword_ChangePassword_UpdateUserInfo: {
                required: true,
                minlength: 8,
                maxlength: 15,
                customPasswordValidate: true,
            }
        },
        messages: {
            OldPassword_ChangePassword_UpdateUserInfo: {
                required: "Vui lòng nhập mật khẩu cũ. Nếu bạn không nhớ mật khẩu, hãy liên hệ admin để được hỗ trợ.",
                minlength: "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.",
                maxlength: "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.",
                customPasswordValidate: "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng."
            },
            NewPassword_ChangePassword_UpdateUserInfo: {
                required: "Vui lòng nhập mật khẩu mới.",
                minlength: "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.",
                maxlength: "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.",
                customPasswordValidate: "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng."
            },
            ConfirmNewPassword_ChangePassword_UpdateUserInfo: {
                required: "Vui lòng nhập xác nhận mật khẩu mới.",
                minlength: "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.",
                maxlength: "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.",
                customPasswordValidate: "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng."
            }

        }
    });
}

//Function Chặn submit Form: Xem chi tiết người dùng - Thay đổi mật khẩu
function HandlePreventChangePassword_UpdateUserInfoModal() {
    $("#ChangePassword_UpdateUserInfo_Form").submit((e) => {
        e.preventDefault()
    })
}

//Function Xem chi tiết người dùng - Thay đổi mật khẩu - Xác nhận:
function Apply_ChangePassword_UpdateUserInfoModal() {    
    $(document).delegate("#btnChangePassword_UpdateUserInfo", "click", (e) => {        
        if ($("#ChangePassword_UpdateUserInfo_Form").valid()) {
            var userId = $("#txtUserIdStr").val();
            var oldPassword = $("#OldPassword_ChangePassword_UpdateUserInfo").val();
            var newPassword = $("#NewPassword_ChangePassword_UpdateUserInfo").val();
            var newPasswordConfirm = $("#ConfirmNewPassword_ChangePassword_UpdateUserInfo").val();
            var formData = {
                UserId: userId,              
                OldPassword: oldPassword,
                NewPassword: newPassword,
                NewPasswordConfirm: newPasswordConfirm
            };                  
            $.ajax({
                type: 'POST',
                url: 'change-password',
                data: formData,
                dataType: "json",                
                success: function () {
                    Swal.fire({
                        title: 'Thông báo',
                        text: "Tài khoản của bạn đã thay đổi mật khẩu thành công",
                        confirmButtonText: 'Đồng ý',                       
                        showLoaderOnConfirm: true                       
                    }).then((result, e) => {
                        if (result.isConfirmed) {
                            Requester.RemoveToken();
                            $("#ChangePassword_UpdateUserInfoModal").modal("hide");
                            Reset_ChangePassword_UpdateUserInfo();
                            window.location.href = "/";                                                                                
                        }
                    })                    
                },
                error: function (error) {
                    if (error != undefined) {                        
                        var response = JSON.parse(error.responseText);                        
                        switch (response.code) {

                             // wrong old password
                            case 85: {
                                Swal.fire({
                                    title: 'Lỗi đăng nhập',
                                    text: response.message,
                                    confirmButtonText: 'Đồng ý',
                                })
                                break;
                            }

                            case 86: { 
                                Swal.fire({
                                    title: 'Lỗi thay đổi mật khẩu',
                                    text: response.message,
                                    confirmButtonText: 'Đã hiểu',
                                })
                                break;
                            }


                            default:
                                {
                                    if (response.data != null) {
                                        if (response.data.oldPassword != null) {
                                            $('#OldPassword_ChangePassword_UpdateUserInfo_span').text(response.data.oldPassword);
                                        }
                                        if (response.data.newPassword != null) {
                                            $('#NewPassword_ChangePassword_UpdateUserInfo_span').text(response.data.newPassword);
                                        }
                                        if (response.data.newPasswordConfirm != null) {
                                            $('#ConfirmNewPassword_ChangePassword_UpdateUserInfo_span').text(response.data.newPasswordConfirm);
                                        }
                                    }
                                    break;
                                }
                        }                                                                
                    }
                }
            });             
        }
    })
}

function CloseChangePasswordModal() {    
    $('#ChangePassword_UpdateUserInfoModal .btn-close').on('click', function () {
        $("#ChangePassword_UpdateUserInfoModal").modal("hide");
        Reset_ChangePassword_UpdateUserInfo();
    });    
}

function Reset_ChangePassword_UpdateUserInfo() {
    $("#OldPassword_ChangePassword_UpdateUserInfo").val('');
    $("#NewPassword_ChangePassword_UpdateUserInfo").val('');
    $("#ConfirmNewPassword_ChangePassword_UpdateUserInfo").val('');
    $('#OldPassword_ChangePassword_UpdateUserInfo_span').text('');
    $('#NewPassword_ChangePassword_UpdateUserInfo_span').text('');
    $('#ConfirmNewPassword_ChangePassword_UpdateUserInfo_span').text('');
    $('#NewPassword_ChangePassword_UpdateUserInfo-error').text('');
    $('#OldPassword_ChangePassword_UpdateUserInfo-error').text('');
    $('#ConfirmNewPassword_ChangePassword_UpdateUserInfo-error').text('');
}

//Hàm Xem thông tin người dùng - Cập nhật thông tin:
async function UpdateUserInfo(files) {
    return new Promise((res, resolve) => {
        var link = $("#APIGateway").val();
        var UserId = App.User.UserId;
        if (UserId != null) {
            //Ajax:
            var form = new FormData();
            form.append("userId", UserId);
            form.append("file", files);

            var settings = {
                "url": link + "/api/identity/update/user",
                "method": "PUT",
                "processData": false,
                "contentType": false,
                "mimeType": "multipart/form-data",
                "data": form
            }

            $.ajax(settings).done(function (response) {
                var res = JSON.parse(response)
                if (res.code == 200) {
                    var url = URL.createObjectURL(files);
                    $('.header-profile-user').attr("src", url);
                    toastr.success(res.message);

                }
                else {
                    toastr.error(res.message);
                }
            })
        }
        
    })
}

//Hàm Xem thông tin người dùng - Cập nhật thông tin:
function GetUserInfo() {
    var link = $("#APIGateway").val();
    var userId = App.User.UserId;

    if (userId != null) {
        //Ajax:
        var form = new FormData();
        form.append("userId", userId);

        var settings = {
            "url": link + `/api/identity/user/${userId}`,
            "method": "GET"
        }

        $.ajax(settings).done(function (response) {
            if (response.code == 200) {
                $("#ViewDetailUser_UserName").text(response?.data?.userName);
                $("#ViewDetailUser_FullName").text(response?.data?.fullName);
                $("#ViewDetailUser_Code").text(response?.data?.code);
                //$("#ViewDetailUser_Password").val(response?.data?.password);
                $("#ViewDetailUser_Department").text(response?.data?.department);
                $("#ViewDetailUser_Role").text(response?.data?.roleName);

                let avatarSrc = response?.data?.avatar;
                $(".Image_Left_Content_ViewDetalUser img").attr("src", avatarSrc && avatarSrc.length > 0 ? `${App.ApiGateWayUrl}${avatarSrc}` : "./assets/images/icons/AVT DEFAULT.svg");
                $(".header-profile-user").attr("src", avatarSrc && avatarSrc.length > 0 ? `${App.ApiGateWayUrl}${avatarSrc}` : "./assets/images/icons/AVT DEFAULT.svg");
                
                
            }
        })
    }
    
}

var Requester = (function () {
    function RemoveToken(accessToken) {
        $.ajaxSetup
            (
                {
                    cache: false,
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader('Authorization', ``);
                    }
                }
            );
    }

    return {
        RemoveToken: RemoveToken
    }
})()
