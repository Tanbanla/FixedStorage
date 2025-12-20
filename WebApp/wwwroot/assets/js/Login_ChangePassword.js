$(document).ready(function () {
    Login_ChangePasswordFormValidation()
    ShowOrHidePassword_Login_ChangePasswordForm()
    // Xử lý khi trường UserName_Login mất focus
    $("#UserName_ChangePassword , #OldPassword_ChangePassword , #NewPassword_ChangePassword , #ConfirmNewPassword_ChangePassword").blur(function () {
        // Loại bỏ khoảng trắng ở hai đầu ký tự
        $(this).val($.trim($(this).val()));
    });


    //Click button Login:
    $("#btn_Apply_BIVN_ChangePassword_Form").click(function () {
        // Xác thực biểu mẫu
        if ($("#BIVN_ChangePassword_Form").valid()) {               
        }
    });


});

//Validate Form Login:
function Login_ChangePasswordFormValidation() {
    $("#BIVN_ChangePassword_Form").validate({
        rules: {
            UserName_ChangePassword: {
                required: true,
                minlength: 1,
                maxlength: 15,
            },
            OldPassword_ChangePassword: {
                required: true,
                minlength: 1,
                maxlength: 15,
                customPasswordValidation: true,
            },
            NewPassword_ChangePassword: {
                required: true,
                minlength: 1,
                maxlength: 15,
                customPasswordValidation: true,
            },
            ConfirmNewPassword_ChangePassword: {
                required: true,
                minlength: 1,
                maxlength: 15,
                customPasswordValidation: true,
                equalTo:"#NewPassword_ChangePassword"
            }
        },
        messages: {
            UserName_ChangePassword: {
                required: "Vui lòng nhập tài khoản đăng nhập.",
                minlength: "Ít nhất 1 ký tự",
                maxlength: "Tối đa 15 ký tự",
            },
            OldPassword_ChangePassword: {
                required: "Vui lòng nhập mật khẩu cũ. Nếu bạn không nhớ mật khẩu, hãy liên hệ admin để được hỗ trợ.",
                minlength: "Ít nhất 1 ký tự",
                maxlength: "Tối đa 15 ký tự",
                customPasswordValidation: "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng."
            },
            NewPassword_ChangePassword: {
                required: "Vui lòng nhập mật khẩu mới.",
                minlength: "Ít nhất 1 ký tự",
                maxlength: "Tối đa 15 ký tự",
                customPasswordValidation: "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng."
            },
            ConfirmNewPassword_ChangePassword: {
                required: "Vui lòng nhập xác nhận mật khẩu mới.",
                minlength: "Ít nhất 1 ký tự",
                maxlength: "Tối đa 15 ký tự",
                customPasswordValidation: "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.",
                equalTo: "Mật khẩu mới không trùng nhau."
            }

        }
    });
}

//Ẩn hiện Password:
function ShowOrHidePassword_Login_ChangePasswordForm() {
    ////Old Password
    //Show Password:
    $("#Show_OldPassword_ChangePassword").click(() => {
        $("#Hide_OldPassword_ChangePassword").show();
        $("#Show_OldPassword_ChangePassword").hide();
        $("#OldPassword_ChangePassword").attr("type", "text");
    })

    //Hide Password:
    $("#Hide_OldPassword_ChangePassword").click(() => {
        $("#Show_OldPassword_ChangePassword").show();
        $("#Hide_OldPassword_ChangePassword").hide();
        $("#OldPassword_ChangePassword").attr("type", "password");
    })

    ////New Password
    //Show Password:
    $("#Show_NewPassword_ChangePassword").click(() => {
        $("#Hide_NewPassword_ChangePassword").show();
        $("#Show_NewPassword_ChangePassword").hide();
        $("#NewPassword_ChangePassword").attr("type", "text");
    })

    //Hide Password:
    $("#Hide_NewPassword_ChangePassword").click(() => {
        $("#Show_NewPassword_ChangePassword").show();
        $("#Hide_NewPassword_ChangePassword").hide();
        $("#NewPassword_ChangePassword").attr("type", "password");
    })

    ////ConfirmNew Password
    //Show Password:
    $("#Show_ConfirmNewPassword_ChangePassword").click(() => {
        $("#Hide_ConfirmNewPassword_ChangePassword").show();
        $("#Show_ConfirmNewPassword_ChangePassword").hide();
        $("#ConfirmNewPassword_ChangePassword").attr("type", "text");
    })

    //Hide Password:
    $("#Hide_ConfirmNewPassword_ChangePassword").click(() => {
        $("#Show_ConfirmNewPassword_ChangePassword").show();
        $("#Hide_ConfirmNewPassword_ChangePassword").hide();
        $("#ConfirmNewPassword_ChangePassword").attr("type", "password");
    })

}

// Kiểm tra độ dài từ 8 đến 15 ký tự và chứa ít nhất một ký tự chữ và một ký tự số, không chứa khoảng trắng
$.validator.addMethod("customPasswordValidation", function (value, element) {       
    return this.optional(element) || /^(?=.*[a-zA-Z])(?=.*\d)(?!.*\s).{8,15}$/.test(value);
}, "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.");