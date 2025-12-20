var commonJs = {
    configs: {
        pageIndex: 1,
        pageSize: 10,
    },
    
    confirm: function (message, okCallback) {
        bootbox.confirm({
            message: message,
            buttons: {
                confirm: {
                    label: 'Đồng ý',
                    className: 'btn-success'
                },
                cancel: {
                    label: 'Hủy',
                    className: 'btn-danger'
                }
            },
            callback: function (result) {
                if (result === true) {
                    okCallback();
                }
            }
        });
    },
    
    formatNumber: function (number, precision) {
        if (!isFinite(number)) {
            return number.toString();
        }

        var a = number.toFixed(precision).split('.');
        a[0] = a[0].replace(/\d(?=(\d{3})+$)/g, '$&,');
        return a.join('.');
    },

    toCamel: function (o) {
        var newO, origKey, newKey, value;
        if (o instanceof Array) {
            newO = [];
            for (origKey in o) {
                value = o[origKey];
                newO.push(value);
            }
        } else {
            newO = {}
            for (origKey in o) {
                if (o.hasOwnProperty(origKey)) {
                    newKey = (origKey.charAt(0).toLowerCase() + origKey.slice(1) || origKey).toString();
                    value = o[origKey];
                    newO[newKey] = value;
                }
            }
        }
        return newO;
    },
    
    hasWhiteSpace: function (s) {
        return s.indexOf(' ') >= 0;
    },

    getDropDownList: function (name, selected, optionalParam1, optionalParam2) {        
        var dropDownList = $(name);
        var url = '';
        var inputData;
        dropDownList.html('');        
        switch (name)
        {
                case "#ddlDepartmentsSearch":           
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "Department",
                        selected
                    };                   
                    dropDownList.append($('<option></option>').val('0').html('Tất cả'));
                    break;
                }
            
                case "#ddlDepartments":
                case "#ddlDepartmentsCreate":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "Department",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Chọn phòng ban'));                   
                    break;                    
                }
                
            case "#ddlRolesSearch":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "Role",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Tất cả'));                    
                    break;
                }

            case "#ddlRoles":
            case "#ddlRolesCreate":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "Role",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Chọn quyền'));
                    break;
                }

            case "#ddlPositionsSearch":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "Position",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Tất cả'));
                    break;
                }

            case "#ddlPositions":
            case "#ddlPositionsCreate":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "Position",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Chọn chức vụ'));
                    break;
                }

            case "#ddlStaffStatusesSearch":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "StaffStatus",
                        selected
                    };                    
                    dropDownList.append($('<option></option>').val('0').html('Tất cả'));
                    break;                    
                }
           
            case "#ddlStaffStatuses":
            case "#ddlStaffStatusesCreate":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "StaffStatus",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Chọn trạng thái'));
                    break;                   
                }

            case "#ddlStatusesSearch":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "Status",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Tất cả'));
                    break;
                }

            case "#ddlStatuses":
            case "#ddlStatusesCreate":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "Status",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Chọn trạng thái'));
                    break;
                }

            case "#ddlGendersSearch":
                {
                    url = '/Common/GetDropDownList';                    
                    inputData = {
                        type: "Gender",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Tất cả'));
                    break;                    
                }

            case "#ddlGenders":
            case "#ddlGendersCreate":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "Gender",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Chọn giới tính'));
                    break;                    
                }
            
            case "#ddlDirectLeadersSearch":
                {
                    url = '/Common/GetDirectLeaderBySearch';
                    var department = optionalParam1;
                    var position = parseInt(optionalParam2);
                    //var departmentId = optionalParam1 != undefined ? parseInt(optionalParam1) : null;
                    //var position = optionalParam2 != undefined ? parseInt(optionalParam2) : null;
                    inputData =
                    {
                        type: "DirectLeadersSearch",
                        directLeaderId: selected,
                        departmentId: departmentId,
                        position: position,                        
                    };
                    dropDownList.append($('<option></option>').val('0').html('Tất cả'));
                    break;                    
                }

            case "#ddlDirectLeaders":
            case "#ddlDirectLeadersCreate":
                {                    
                    url = '/Common/GetDirectLeader';
                    //var department = optionalParam1 != undefined ? department : null;
                    //var position = optionalParam2 != undefined ? parseInt(optionalParam2) : null;
                    //var directLeaderId = selected != undefined ? parseInt(selected) : null;
                    var department = optionalParam1;
                    var position = parseInt(optionalParam2);
                    var directLeaderId = parseInt(selected);
                    inputData =
                    {
                        directLeaderId: directLeaderId,
                        department: optionalParam1,
                        position: position,
                    };
                    dropDownList.append($('<option></option>').val('0').html('Chọn cấp trên trực tiếp'));
                    break;                    
                }

            case "#ddlAreasSearch":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "Area",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Tất cả'));
                    break;                    
                }

            case "#ddlAreas":
            case "#ddlAreasCreate":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "Area",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Chọn khu vực'));
                    break;                    
                }

            case "#ddlProvincesSearch":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "Province",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Tất cả'));
                    break;                    
                }

            case "#ddlProvinces":
            case "#ddlProvincesCreate":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "Province",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Chọn Tỉnh/Thành Phố'));
                    break;                    
                }

            case "#ddlDistrictsSearch":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "District",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Tất cả'));                    
                    break;                    
                }

            case "#ddlDistricts":
            case "#ddlDistrictsCreate":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "District",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Chọn Quận/Huyện'));
                    break;
                }

            case "#ddlConsultants":
            case "#ddlConsultantsCreate":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "Consultant",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Chọn tư vấn'));
                    break;
                }
           
            case "#ddlPromotersSearch":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "Promoter",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Tất cả'));
                    break;
                }

            case "#ddlPromotersCreate":
            case "#ddlPromotersUpdate":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "Promoter",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Chọn Promoter quản lý'));
                    break;
                }

            case "#ddlTelesales":
            case "#ddlTelesalesCreate":
                {                    
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "TelesalePool",
                        selected
                    };
                    dropDownList.append($('<option></option>').val('0').html('Chọn Telesale'));                    
                    break;
                }

            case "#ddlScholarshipExamStatus_NE":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "ScholarshipExamStatus",
                        selected
                    };
                    /*dropDownList.append($('<option></option>').val('').html('Chọn tình trạng'));  */                 
                    break;
                }
           
            case "#ddlBankAccountDorm":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "AccountNumber",
                        selectedStr: selected
                    };
                    /*dropDownList.append($('<option></option>').val('').html('Chọn số tài khoản'));*/
                    break;
                }

            case "#ddlFeePaymentDorm":
                {
                    url = '/Common/GetDropDownList';
                    inputData = {
                        type: "FeePaymentDorm",
                        selected
                    };
                    /*dropDownList.append($('<option></option>').val('').html('Chọn hình thức nộp'));*/
                    break;
                }

        }        
        return $.ajax({
            type: "GET",
            url: url,
            /*dataType: "json",*/
            data: inputData,
            success: function (response) {
                if (name == "#ddlScholarshipExamStatus_NE" || name == "#ddlBankAccountDorm" || name == "#ddlFeePaymentDorm") {
                    dropDownList.html('');
                }                
                $.each(response, function (i, item) {
                    if (selected != undefined && item.Value == selected)
                    {
                        dropDownList.append($('<option selected=\"selected\"></option>').val(item.Value).html(item.Text));
                    }                   
                    else
                    {
                        if (name == '#ddlStatuses' || name == '#ddlStatusesCreate' || name == '#ddlStaffStatuses' || name == '#ddlStaffStatusesCreate')
                        {
                            if (item.Value == '1')
                            {
                                dropDownList.append($('<option selected=\"selected\"></option>').val(item.Value).html(item.Text));
                            }
                            else
                            {
                                dropDownList.append($('<option></option>').val(item.Value).html(item.Text));
                            }
                        }
                        else
                        {
                            dropDownList.append($('<option></option>').val(item.Value).html(item.Text));
                        }                        
                    }
                });
                
                switch (name) {
                        case "#ddlDepartmentsCreate":
                        {
                            if (selected == null) {
                                $("#ddlDepartmentsCreate option:contains('Chọn phòng ban')").attr("selected", true);
                            }
                            break;
                        }

                        case "#ddlDepartments":                 
                        {
                            if (selected == null) {
                                $("#ddlDepartments option:contains('Chọn phòng ban')").attr("selected", true);
                            }                           
                            break;
                        }                        
                }                
            },            
            error: function (error) {                
                if (error != undefined) {
                    toastr.error('Danh sách hiển thị thất bại.');
                    commonJs.handleErrors(error);
                }
            }
        });
    },    

    handleErrors: function (error) {
        if (error != undefined && error != '') {
            var response = JSON.parse(error.responseText);
            var elements = $("span.text-danger");

            elements.map(function (index, element) {
                var name = $(element).attr("name");
                if (!name) {
                    return;
                }
                var error = response.Errors[name];
                if (error) {
                    $(element).text(error);
                    $(element).css("display", "block");
                }
                else {
                    $(element).css("display", "none");
                }
            });
        }        
    },

    displayError: function (data) {
        if (data != null) {
            toastr.error(data);
        }
    },

    viewTextErrors: function (errorElement) {        
        errorElement.addClass("text-danger");        
        var errorId = errorElement.attr("id");
        var wrapDiv = "#" + errorId + "-div";
        /*$(wrapDiv).html('');*/
        errorElement.appendTo($(wrapDiv));        
    },

    appendErrorToWrapDiv: function (errorElement) {        
        var errorId = errorElement.attr("id");
        //$('#' + errorId).prop("name", errorId);       
        //var wrapDiv = "#" + errorId + "-div";       
        //errorElement.appendTo($(wrapDiv));        
        var textError = errorElement.text();
        if ($('span[name="' + errorId + '"]').text() == '') {
            $('span[name="' + errorId + '"]').text(textError);
        }        
    },

    resetTextErrors: function () {
        $('.text-error').children('label').remove();
        $('.text-error').children('span').text('');
    },

    getError: function (selector, errorContent) {
        if (errorContent == undefined) {
            $(selector).text("Vui lòng điền đầy đủ thông tin.");
        }
        else {
            $(selector).text(errorContent);
        }
    },

    calculateAge: function(birthday) { // birthday is a date
        var ageDifMs = Date.now() - birthday.getTime();
        var ageDate = new Date(ageDifMs); // miliseconds from epoch
        return Math.abs(ageDate.getUTCFullYear() - 1970);
    },

    isPositiveNumber: function (n) {
        return Number(n) === n && Number(n) >= 0;
    },

    isPositiveFloat: function (n) {
        return Number(n) === n && n % 1 !== 0 && Number(n) >= 0;
    },

    isFloat: function (n) {
        return Number(n) === n && n % 1 !== 0;
    },

    isInt: function (n) {
        return Number(n) === n && n % 1 === 0;
    },

    /* ------------------------------------------------------------- DATE ------------------------------------------------------------- */

    // "dd-MM-yyyy"c is converted from ISO Date String
    getddmmyyyyDateStringFromDateISOString: function (isoDateStr, sign) {               
        var arr = isoDateStr.split('-');
        var year = arr[0];
        var month = arr[1];
        var day = arr[2].substring(0, 2);       
        var date = day + "" + sign + "" + month + "" + sign + "" + year;
        return date;
    },

    // "dd-MM-yyyy"c is converted from Date object
    getddmmyyyyDateStringFromDateObject: function (dateObj, sign) {
        var date = '';
        var month = dateObj.getMonth() + 1;
        var day = dateObj.getDate();
        var year = dateObj.getFullYear();
        if (month < 10)
            month = "0" + month;
        if (day < 10)
            day = "0" + day;
        date = day + "" + sign + "" + month + "" + sign + "" + year;
        return date;
    },

    // Date object is converted from 'dd-MM-yyyy'
    getDateObjectFromDdmmyyyy: function(ddmmyyyy, sign1, sign2) {
        var arr = ddmmyyyy.split(sign1);
        var mmddyyyy = '"' + arr[1] + '"' + sign2 + '"' + arr[0] + '"' + sign2 + '"' + arr[2] + '"';
        return new Date(mmddyyyy);        
    },

    // "dd/MM/yyyy" string
    dateFormatddmmyyyyJson: function (datetime) {
        if (datetime == null || datetime == '')
            return '';
        var newdate = new Date(parseInt(datetime.substr(6)));
        var month = newdate.getMonth() + 1;
        var day = newdate.getDate();
        var year = newdate.getFullYear();
        var hh = newdate.getHours();
        var mm = newdate.getMinutes();
        if (month < 10)
            month = "0" + month;
        if (day < 10)
            day = "0" + day;
        if (hh < 10)
            hh = "0" + hh;
        if (mm < 10)
            mm = "0" + mm;
        return day + "/" + month + "/" + year;
    },

    // dd/MM/yyy hh:mm:ss
    dateTimeFormatddmmyyyyhhhmmssJson: function (datetime) {
        if (datetime == null || datetime == '')
            return '';
        var newdate = new Date(datetime);
        //var newdate = new Date(parseInt(datetime.substr(6)));
        var month = newdate.getMonth() + 1;
        var day = newdate.getDate();
        var year = newdate.getFullYear();
        var hh = newdate.getHours();
        var mm = newdate.getMinutes();
        var ss = newdate.getSeconds();
        if (month < 10)
            month = "0" + month;
        if (day < 10)
            day = "0" + day;
        if (hh < 10)
            hh = "0" + hh;
        if (mm < 10)
            mm = "0" + mm;
        if (ss < 10)
            ss = "0" + ss;
        return day + "/" + month + "/" + year + " " + hh + ":" + mm + ":" + ss;
    },

    // Get 'yyyy-MM-ddThh:mm:zz.416Z'
    getDateISOString: function (dateStr, sign) {
        var dateParts = dateStr.split(sign);
        var dateJsObj = new Date(parseInt(dateParts[2]), dateParts[1] - 1, dateParts[0], 1, 0, 0, 0);
        var dateISOString = dateJsObj.toISOString();
        return dateISOString;
    },

    //  Convert a "dd/MM/yyyy" string into a Date object
    getDateObject: function (ddMMyyyySlashDateStr) {
        let dat = '';
        if (ddMMyyyySlashDateStr != undefined && ddMMyyyySlashDateStr != '') {
            let d = ddMMyyyySlashDateStr.split("-");
            dat = new Date(d[2] + '/' + d[1] + '/' + d[0]);
        }        
        return dat;
    },
    
    //  "yyyy-MM-dd" is converted from "dd-MM-yyyy"
    getyyyymmddDateString: function (dateStr) {        
        let dat = '';
        if (dateStr != undefined && dateStr != '') {
            let d = dateStr.split("-");
            dat = d[2] + "-" + d[1] + "-" + d[0];
        }
        return dat;
    },

    //  "dd-MM-yyyy" is converted from "yyyy-MM-dd"
    getddmmyyyyDateString: function (dateStr) {

        dateStr = dateStr.replace("'", "");
        dateStr = dateStr.replace("'", "");
        let dateArr = dateStr.split("-");
        let date = dateArr[2] + "-" + dateArr[1] + "-" + dateArr[0];
        return date;
    },

    //  "yyyy-MM-dd" is converted from "dd/MM/yyyy"
    formatDate: function (strDate) {
        var newDate = strDate.split('/');
        var d = newDate[0];
        var m = newDate[1];
        var y = newDate[2];
        return y + "-" + m + "-" + d;
    },

    // get 'dd-MM-yyyy' from 'dd/MM/yyyy'
    getddmmyyyySlashSeperatedString_fromddmmyyyyHyphenSeperatedString: function (strDate) {
        var newDate = strDate.split('/');
        var d = newDate[0];
        var m = newDate[1];
        var y = newDate[2];
        return d + "-" + m + "-" + y;
    },

    // get 'yyyy' from 'dd/MM/yyyy'
    getYear_fromddmmyyyyHyphenSeperatedString: function (strDate) {
        var newDate = strDate.split('/');
        var d = newDate[0];
        var m = newDate[1];
        var y = newDate[2];
        return y;
    },

    getYears: function (from, to) {
        var d1 = new Date(from),
            d2 = new Date(to),
            yr = [];

        for (var i = d1.getFullYear(); i <= d2.getFullYear(); i++) {
            yr.push(i);
        }
        return yr;
    },

    previewFile: function (imgId, evt) {
        /*loading(true);*/
        var files = evt.target.files; // FileList object

        // Loop through the FileList and render image files as thumbnails.
        for (var i = 0, f; f = files[i]; i++) {
            // Only process image files.
            if (!f.type.match('image.*')) {
                continue;
            }

            var reader = new FileReader();

            // Closure to capture the file information.
            reader.onload = (function (theFile) {
                return function (e) {
                    // Render thumbnail.
                    //$('#avatarImg').attr('src', e.target.result);
                    //document.getElementById('avatarImg').removeChild(document.getElementById('avatarImg').childNodes[0])
                    resize(e.target.result).then(url => {
                        crop(url, 1 / 1).then(data => {
                            //document.getElementById('avatarImg').appendChild(canvas);
                            document.getElementById(imgId).setAttribute('src', data);
                        });
                    })

                    //var span = document.createElement('span');
                    //span.innerHTML = ['<img class="thumb" src="', e.target.result,
                    //    '" title="', escape(theFile.name), '"/>'].join('');
                    //document.getElementById('list').insertBefore(span, null);
                };
            })(f);

            // Read in the image file as a data URL.
            reader.readAsDataURL(f);
        }
        /*loading(false);*/
    },

    resize: function (url) {
        // we return a Promise that gets resolved with our canvas element
        return new Promise((resolve) => {
            // this image will hold our source image data
            const image = new Image();

            // we want to wait for our image to load
            image.onload = () => {
                // Resize the image
                var canvas = document.createElement('canvas'),
                    max_size = 500;
                let width = image.width;
                let height = image.height;
                if (width > height) {
                    if (width > max_size) {
                        height *= max_size / width;
                        width = max_size;
                    }
                } else {
                    if (height > max_size) {
                        width *= max_size / height;
                        height = max_size;
                    }
                }
                canvas.width = width;
                canvas.height = height;
                canvas.getContext('2d').drawImage(image, 0, 0, width, height);
                var dataUrl = canvas.toDataURL('image/jpeg');
                resolve(dataUrl);
            };

            // start loading our image
            image.src = url;
        });

    },

    crop: function (url, aspectRatio) {
        // we return a Promise that gets resolved with our canvas element
        return new Promise((resolve) => {
            // this image will hold our source image data
            const inputImage = new Image();

            // we want to wait for our image to load
            inputImage.onload = () => {
                // let's store the width and height of our image
                const inputWidth = inputImage.naturalWidth;
                const inputHeight = inputImage.naturalHeight;

                // get the aspect ratio of the input image
                const inputImageAspectRatio = inputWidth / inputHeight;

                // if it's bigger than our target aspect ratio
                let outputWidth = inputWidth;
                let outputHeight = inputHeight;
                if (inputImageAspectRatio > aspectRatio) {
                    outputWidth = inputHeight * aspectRatio;
                } else if (inputImageAspectRatio < aspectRatio) {
                    outputHeight = inputWidth / aspectRatio;
                }

                // calculate the position to draw the image at
                const outputX = (outputWidth - inputWidth) * 0.5;
                const outputY = (outputHeight - inputHeight) * 0.5;

                // create a canvas that will present the output image
                const outputImage = document.createElement("canvas");

                // set it to the same size as the image
                outputImage.width = outputWidth;
                outputImage.height = outputHeight;

                // draw our image at position 0, 0 on the canvas
                const ctx = outputImage.getContext("2d");
                ctx.drawImage(inputImage, outputX, outputY);
                var dataUrl = outputImage.toDataURL('image/jpeg');
                resolve(dataUrl);
            };

            // start loading our image
            inputImage.src = url;
        });
    },

    unsignVietnameseString: function (str) {
        str = str.replace(/A|Á|À|Ã|Ạ|Â|Ấ|Ầ|Ẫ|Ậ|Ă|Ắ|Ằ|Ẵ|Ặ/g, "A");
        str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
        str = str.replace(/E|É|È|Ẽ|Ẹ|Ê|Ế|Ề|Ễ|Ệ/g, "E");
        str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
        str = str.replace(/I|Í|Ì|Ĩ|Ị/g, "I");
        str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
        str = str.replace(/O|Ó|Ò|Õ|Ọ|Ô|Ố|Ồ|Ỗ|Ộ|Ơ|Ớ|Ờ|Ỡ|Ợ/g, "O");
        str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
        str = str.replace(/U|Ú|Ù|Ũ|Ụ|Ư|Ứ|Ừ|Ữ|Ự/g, "U");
        str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
        str = str.replace(/Y|Ý|Ỳ|Ỹ|Ỵ/g, "Y");
        str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
        str = str.replace(/Đ/g, "D");
        str = str.replace(/đ/g, "d");
        // Some system encode vietnamese combining accent as individual utf-8 characters
        str = str.replace(/\u0300|\u0301|\u0303|\u0309|\u0323/g, ""); // Huyền sắc hỏi ngã nặng
        str = str.replace(/\u02C6|\u0306|\u031B/g, ""); // Â, Ê, Ă, Ơ, Ư
        return str;
    },    

    updateFeesDisplayFormatVietnamese: function(name) {
        var txtFeeSelector = $(name);
        txtFeeSelector.on('keyup', function () {
            if (txtFeeSelector.val() != null && txtFeeSelector.val() != " VND") {
                $(this).val(function (index, value) {
                    var num = value.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ",");
                    var vnd = num + " VND";
                    return vnd;
                });
            }
        });
    },
}

const LEAD_NAME_COLUMN = "lead.name";
const LEAD_GENDER_COLUMN = "citizenindentifications.gender";
const LEAD_MOBILE_NUMBER_COLUMN = "lead.mobile_number";
const LEAD_SECOND_MOBILE_NUMBER_COLUMN = "lead.second_mobile_number";
const LEAD_EMAIL_COLUMN = "lead.email";
const LEAD_SUB_MAJOR_COLUMN = "submajors.name";
const LEAD_CLASS_COLUMN = "lead.class";
const LEAD_FACEBOOK_COLUMN = "lead.facebook";
const LEAD_SOURCE_COLUMN = "lead.source";
const LEAD_STATUS_COLUMN = "lead.status";
const LEAD_MARKETING_CHANNEL_COLUMN = "lead.marketing_channel";
const LEAD_CAMPAIGN_COLUMN = "lead.campaign_name";
const LEAD_DUPLICATED_COLUMN = "lead.duplicate_number";
const LEAD_UPDATED_BY_COLUMN = "lead.updated_by"; 
const LEAD_ASSIGNEE_COLUMN = "leaddistributions.staff_id";
/*const LEAD_ASSIGNEE_COLUMN = "lead.assignee";*/
const LEAD_CREATED_BY_COLUMN = "lead.created_by";
const PROVINCE_OF_LEAD_COLUMN = "provinceLead.name";
const SCHOOL_COLUMN = "school.name";
const LEAD_LIVING_ADDRESS_COLUMN = "citizenindentifications.living_address";
const PROVINCE_OF_SCHOOl_COLUMN = "provinceSchool.name";
const DISTRICT_OF_LEAD_COLUMN = "districtLead.name";
const DISTRICT_OF_SCHOOL_COLUMN = "districtSchool.name";
const LEAD_NATIONAL_COLUMN = "citizenindentifications.national";
const LEAD_ETHNIC_COLUMN = "nedocuments.nation";
const LEAD_RELIGION_COLUMN = "nedocuments.religion";
const LEAD_PROGRESS_STEP_COLUMN = "lead.progress_step";
const LEAD_DROPOUT_STEP_COLUMN = "lead.dropout_step";
const LEAD_NE_STEP_COLUMN = "lead.ne_step";
const LEAD_NB_STEP_COLUMN = "lead.nb_step";
const LEAD_REG_STEP_COLUMN = "lead.reg_step";
const LEAD_CONFIRM_PENDING_STEP_COLUMN = "lead.confirm_pending_step";
const LEAD_SUBMISSION_PENDING_STEP_COLUMN = "lead.submission_pending_step";
const LEAD_APPROVE_PENDING_STEP_COLUMN = "lead.approve_pending_step";
const LEAD_BIRTHDAY_COLUMN = "lead.birthday";
const LEAD_REGISTER_YEAR_COLUMN = "lead.register_year";
const LEAD_CREATED_AT_COLUMN = "lead.CreatedAt";
const LEAD_UPDATED_AT_COLUMN = "lead.UpdatedAt";
const LEAD_ASSIGNED_AT_COLUMN = "leaddistributions.created_at";
const LEAD_NOTE_COLUMN = "leadhistories.content";
const LEAD_REG_CODE_COLUMN = "lead.reg_code";
const LEAD_NE_CODE_COLUMN = "lead.ne_code";
const LEAD_NB_CODE_COLUMN = "lead.nb_code";

const EQUAL_OPERATOR = "Equal";
const NOT_EQUAL_OPERATOR = "NotEqual";
const IS_NULL_OPERATOR = "IsNull";
const NOT_NULL_OPERATOR = "NotNull";
const BEFORE_OPERATOR = "Before";
const AFTER_OPERATOR = "After";

const LAST_YEAR_OPERATOR = "LastYear";
const THIS_YEAR_OPERATOR = "ThisYear";
const LAST_QUARTER_OPERATOR = "LastQuarter";
const THIS_QUARTER_OPERATOR = "ThisQuarter";
const LAST_MONTH_OPERATOR = "LastMonth";
const THIS_MONTH_OPERATOR = "ThisMonth";
const LAST_WEEK_OPERATOR = "LastWeek";
const THIS_WEEK_OPERATOR = "ThisWeek";
const YESTERDAY_OPERATOR = "Yesterday";
const TODAY_OPERATOR = "Today";

const BETWEEN_OPERATOR = "Between";
const LEAD_STATUS_CARE_COLUMN = "lead.status_care_lead";//lead.status_care_lead
$(document).ajaxSend(function (e, xhr, options) {
    if (options.type.toUpperCase() == "POST" || options.type.toUpperCase() == "PUT") {
        var token = $('form').find("input[name='__RequestVerificationToken']").val();
        xhr.setRequestHeader("RequestVerificationToken", token);
    }
});