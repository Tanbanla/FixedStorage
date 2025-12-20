$(document).ready(function () {
    //call api lấy ra ngôn ngữ:
    $.ajax({
        url: '/language/GetAllTranslatedKeys',  // URL của endpoint backend
        type: 'GET',
        success: function (data) {
            // biến toàn cục lưu trữ json ngôn ngữ:
            window.langData = data;

            let languageCookieName = '.AspNetCore.Culture'; // Tên cookie mặc định của CookieRequestCultureProvider
            let cookieValue = getCookie(languageCookieName);

            // biến toàn cục lấy ngôn ngữ hiện tại:
            let language = (cookieValue && cookieValue.includes("en-US")) ? "en-US" : "vi-VN";
            window.currentLang = language;

        },
        error: function (error) {
            console.error('Error fetching translated keys:', error);
        }
    });

    waitForLoginLanguageData();
    
    
});

function waitForLoginLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.langData && window.currentLang) {
        console.log('Mutil language ready')
        LoginFormValidation()
        ShowOrHidePassword_FormLogin()
        // Xử lý khi trường UserName_Login mất focus
        $("#UserName_Login , #Password_Login").blur(function () {
            // Loại bỏ khoảng trắng ở hai đầu ký tự
            $(this).val($.trim($(this).val()));
        });

        //Hàm cho nhap chu va so:
        //$('#UserName_Login').keypress(function (event) {
        //    var character = String.fromCharCode(event.keyCode);
        //    return isValidTextAndNumber(character);
        //});

        //Click button Login:
        const host = $("#APIGateway").val();
        $(document).delegate("#btnLogin_Form", "click", (e) => {
            let validForm = $("#BIVN_LoginForm").valid();
            if (validForm) {
                var username = $('#UserName_Login').val();
                var password = $('#Password_Login').val();
                let loginModel = {
                    Username: username,
                    Password: password,
                }

                Login(loginModel, 'NO').then((res) => {
                    window.location.href = "/";
                }).catch((error) => {
                    if (error != undefined) {
                        var response = JSON.parse(error.responseText);
                        switch (response.code) {
                            case 10:
                            case 11:
                            case 12:
                                {
                                    $('#Username_span').text('');
                                    Swal.fire({
                                        title: window.langData[window.currentLang]['Lỗi đăng nhập'],
                                        text: window.langData[window.currentLang]["Tài khoản không có quyền truy cập hệ thống."],
                                        confirmButtonText: window.langData[window.currentLang]['Đồng ý'],
                                        //showCancelButton: false,
                                        //showLoaderOnConfirm: true,
                                        //cancelButtonText: 'Hủy bỏ',
                                    })
                                    break;
                                }

                            case 13: {
                                $('#Username_span').text('');
                                Swal.fire({
                                    title: window.langData[window.currentLang]['Lỗi'],
                                    text: window.langData[window.currentLang]["Tài khoản không tồn tại trên hệ thống.Vui lòng thử lại."],
                                    confirmButtonText: window.langData[window.currentLang]['Đồng ý']
                                })
                                break;
                            }

                            case 14:
                                {
                                    $('#Username_span').text('');
                                    Swal.fire({
                                        title: window.langData[window.currentLang]['Lỗi đăng nhập'],
                                        text: window.langData[window.currentLang]["Thông tin đăng nhập không đúng. Vui lòng liên hệ với quản lý để được trợ giúp."],
                                        confirmButtonText: window.langData[window.currentLang]['Đồng ý']
                                    })
                                    break;
                                }

                            case 15:
                                {
                                    $('#Username_span').text('');
                                    Swal.fire({
                                        title: window.langData[window.currentLang]['Cảnh báo đăng nhập'],
                                        text: window.langData[window.currentLang]["Tài khoản đăng nhập của bạn đang được đăng nhập trên thiết bị khác. Bạn có muốn tiếp tục đăng nhập không?"],
                                        confirmButtonText: window.langData[window.currentLang]['Đồng ý'],
                                        showCancelButton: true,
                                        showLoaderOnConfirm: true,
                                        cancelButtonText: window.langData[window.currentLang]['Hủy bỏ'],
                                    }).then((result, e) => {
                                        if (result.isConfirmed) {
                                            var allowOverrideLoginPersonalAccount = 'YES';
                                            var username = $('#UserName_Login').val();
                                            var password = $('#Password_Login').val();
                                            let loginModel = {
                                                Username: username,
                                                Password: password
                                            }
                                            Login(loginModel, allowOverrideLoginPersonalAccount).then(res => {
                                                window.location.href = "inventory-reporting";
                                            }).catch(err => {
                                                Swal.fire({
                                                    title: window.langData[window.currentLang]["Không thể đăng nhập"],
                                                    text: err?.responseJSON?.message,
                                                    confirmButtonText: window.langData[window.currentLang]["Đóng"],
                                                    width: '30%'
                                                })
                                            })
                                        }
                                    })
                                    break;
                                }

                            case 16:
                                {
                                    $('#Username_span').text('');
                                    Swal.fire({
                                        title: window.langData[window.currentLang]['Lỗi'],
                                        text: window.langData[window.currentLang]["Tài khoản không tồn tại trên hệ thống.Vui lòng thử lại."],
                                        confirmButtonText: window.langData[window.currentLang]['Đồng ý']
                                    })
                                    break;
                                }

                            case 17:
                            case 18:
                            case 19:
                                {
                                    $('#Username_span').text('');
                                    Swal.fire({
                                        title: window.langData[window.currentLang]['Phiên đã hết hạn'],
                                        text: window.langData[window.currentLang]["Vui lòng đăng nhập lại."],
                                        confirmButtonText: window.langData[window.currentLang]['Đồng ý']
                                    })
                                    break;
                                }
                            case 21:
                                {
                                    $('#Username_span').text('');
                                    Swal.fire({
                                        title: window.langData[window.currentLang]['Lỗi'],
                                        text: window.langData[window.currentLang]["Tài khoản không tồn tại quyền."],
                                        confirmButtonText: window.langData[window.currentLang]['Đồng ý']
                                    })
                                    break;
                                }
                            case 22:
                                {
                                    $('#Username_span').text('');
                                    Swal.fire({
                                        title: window.langData[window.currentLang]['Lỗi'],
                                        text: window.langData[window.currentLang]["Tài khoản không tồn tại quyền chi tiết."],
                                        confirmButtonText: window.langData[window.currentLang]['Đồng ý']
                                    })
                                    break;
                                }
                            case 23:
                                {
                                    $('#Username_span').text('');
                                    Swal.fire({
                                        title: window.langData[window.currentLang]['Lỗi'],
                                        text: window.langData[window.currentLang]["Tạo Token không thành công."],
                                        confirmButtonText: window.langData[window.currentLang]['Đồng ý']
                                    })
                                    break;
                                }
                            case 24:
                                {
                                    $('#Username_span').text('');
                                    Swal.fire({
                                        title: window.langData[window.currentLang]['Lỗi'],
                                        text: window.langData[window.currentLang]["Client Id không hợp lệ."],
                                        confirmButtonText: window.langData[window.currentLang]['Đồng ý']
                                    })
                                    break;
                                }
                            case 25:
                                {
                                    $('#Username_span').text('');
                                    Swal.fire({
                                        title: window.langData[window.currentLang]['Lỗi'],
                                        text: window.langData[window.currentLang]["Client Secret không hợp lệ."],
                                        confirmButtonText: window.langData[window.currentLang]['Đồng ý']
                                    })
                                    break;
                                }
                            case 26:
                                {
                                    $('#Username_span').text('');
                                    Swal.fire({
                                        title: window.langData[window.currentLang]['Lỗi'],
                                        text: window.langData[window.currentLang]["Security Stamp không hợp lệ sau khi thay đổi mật khẩu thành công. Cần Logout để lấy ra Security Stamp mới nhất."],
                                        confirmButtonText: window.langData[window.currentLang]['Đồng ý']
                                    })
                                    break;
                                }
                            case 43:
                                {
                                    $('#Username_span').text('');
                                    Swal.fire({
                                        title: window.langData[window.currentLang]['Thông báo'],
                                        text: window.langData[window.currentLang]["Tài khoản đăng nhập chưa được gán vai trò thao tác. Vui lòng liên hệ quản lý để gán vai trò cho tài khoản này."],
                                        confirmButtonText: window.langData[window.currentLang]['Quay lại']
                                    })
                                    break;
                                }
                            case 44:
                                {
                                    $('#Username_span').text('');
                                    Swal.fire({
                                        title: window.langData[window.currentLang]['Thông báo'],
                                        text: window.langData[window.currentLang]["Tài khoản đăng nhập chưa được gán khu vực giám sát. Vui lòng liên hệ quản lý để gán khu vực giám sát cho tài khoản này."],
                                        confirmButtonText: window.langData[window.currentLang]['Quay lại']
                                    })
                                    break;
                                }

                            default:
                                {
                                    if (response.data != null) {
                                        if (response.data.username != null) {
                                            $('#Username_span').text('');
                                            Swal.fire({
                                                title: window.langData[window.currentLang]['Lỗi đăng nhập'],
                                                text: response.data.username,
                                                confirmButtonText: window.langData[window.currentLang]['Đồng ý']
                                            })
                                        }
                                        else {
                                            $('#Username_span').text('');
                                        }
                                        if (response.data.password != null) {
                                            $('#Password_span').text(response.data.password);
                                        }
                                        else {
                                            $('#Password_span').text('');
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                });
            }
        });

        function Login(model, allowOverrideLoginPersonalAccount) {
            return new Promise(async (resolve, reject) => {
                let url = `login`
                try {
                    const res = await $.ajax({
                        url: url,
                        type: 'POST',
                        data: model,
                        headers: {
                            'AllowOverrideLoginPersonalAccount': `${allowOverrideLoginPersonalAccount}`
                        }
                        //contentType: 'application/json',
                        //data: JSON.stringify(model)
                    });
                    resolve(res)
                } catch (err) {
                    reject(err)
                }
            });
        }

        KeyUpEnterToLogin()

    } else {
        console.log('Mutil language not ready')
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForLoginLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}

function getCookie(name) {
    let cookieArr = document.cookie.split(";");
    for (let i = 0; i < cookieArr.length; i++) {
        let cookiePair = cookieArr[i].trim();
        // Kiểm tra tên cookie
        if (cookiePair.startsWith(name + "=")) {
            return cookiePair.substring(name.length + 1);
        }
    }
    return null; // Không tìm thấy cookie
}
function KeyUpEnterToLogin() {
    
    $(document).on("keydown", "#BIVN_LoginForm", function (event) {
        if (event.keyCode === 13) {
            event.preventDefault();

            $("#btnLogin_Form").trigger("click");
        }
    });

}

//Validate Form Login:
function LoginFormValidation() {
    var inputUserName = window.langData[window.currentLang]["Vui lòng nhập tài khoản đăng nhập."];
    var lengthUserName = window.langData[window.currentLang]["Tài khoản đăng nhập phải có độ dài từ 1 đến 15 ký tự, không được chứa ký tự đặc biệt."];
    var noSpecialCharsUserName = window.langData[window.currentLang]["Tài khoản đăng nhập không đúng định dạng. Vui lòng thử lại."];
    var inputPassword = window.langData[window.currentLang]["Vui lòng nhập mật khẩu."];
    var lengthPassword = window.langData[window.currentLang]["Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng."];


    // Định nghĩa một phương thức kiểm tra tùy chỉnh
    jQuery.validator.addMethod("noSpecialChars", function (value, element) {
        return /^[a-zA-Z0-9_-]+$/.test(value);
    }, "Tài khoản đăng nhập không đúng định dạng. Vui lòng thử lại.");

    $("#BIVN_LoginForm").validate({
        rules: {
            UserName_Login: {
                required: true,
                minlength: 1,
                maxlength: 15,
                noSpecialChars: true
            },
            Password_Login: {
                required: true,
                minlength: 8,
                maxlength: 15,
                customPasswordValidation: true,
                noWhitespace: true,
            }
        },
        messages: {
            UserName_Login: {
                required: inputUserName,
                minlength: lengthUserName,
                maxlength: lengthUserName,
                noSpecialChars: noSpecialCharsUserName
            },
            Password_Login: {
                required: inputPassword,
                minlength: lengthPassword,
                maxlength: lengthPassword,
                customPasswordValidation: lengthPassword,
                noWhitespace: lengthPassword
            }
        }
    });
}

//Ẩn hiện Password:
function ShowOrHidePassword_FormLogin() {
    //Show Password:
    $("#BIVN_ShowPassword").click(() => {
        $("#BIVN_HidePassword").show();
        $("#BIVN_ShowPassword").hide();
        $("#Password_Login").attr("type", "text");
    })

    //Hide Password:
    $("#BIVN_HidePassword").click(() => {
        $("#BIVN_HidePassword").hide();
        $("#BIVN_ShowPassword").show();
        $("#Password_Login").attr("type", "password");
    })
}

// Thêm phương thức xác thực tùy chỉnh cho mật khẩu:
$.validator.addMethod("customPasswordValidation", function (value, element) {
    // Kiểm tra độ dài từ 8 đến 15 ký tự và chứa ít nhất một ký tự chữ và một ký tự số, không chứa khoảng trắng
    return this.optional(element) || /^(?=.*[a-zA-Z])(?=.*\d)(?!.*\s).{8,15}$/.test(value);    
}, "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.");

//Ham chi cho nhap chu va so:
function isValidTextAndNumber(str) {
    return !/[~`!@#$%\^&*()+=\-\[\]\\';.,/{}|\\":<>\?_]/g.test(str);
}

// Thêm quy tắc kiểm tra tùy chỉnh để kiểm tra khoảng trắng
$.validator.addMethod("noWhitespace", function (value, element) {
    return this.optional(element) || /^\S+$/i.test(value);
}, "Không được chứa khoảng trắng.");