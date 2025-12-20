var ValidateInputHelper = (function () {
    //Ham chan cac ky tu dac biet:
    function isValidText(str) {
        return !/[~`!@#$%\^&*()+=\-\[\]\\';.,/{}|\\":<>\?_]/g.test(str);
    }

    //Ham chi cho nhap so:
    function isValidNumber(str) {
        return /^\d$/g.test(str);
    }

    function OnlyNumerOnKeyPress(e) {
        let character = String.fromCharCode(e.keyCode);
        let isEnter = e.keyCode == 13;
        if (isEnter) {
            return true;
        } else {
            return isValidNumber(character);
        }
    }

    function RemoveVietnameseOnChange(e) {
        let validNumber = parseInt(e.target.value);
        if (!validNumber) {
            e.target.value = "";
        }
    }

    function PreventSepcialCharacterOnKeyPress(e) {
        let character = String.fromCharCode(e.keyCode)
        return isValidText(character)
    }

    function RemoveSpecialCharacter(e) {
        let val = e.target.value;
        e.target.value = val.replace(/[^\w ]/g, '');
    }

    function TrimWhiteSpaceOnBlur(e) {
        let trimVal = $.trim($(this).val())
        $(this).val(trimVal)
    }

    function formatNumberWithCommas(str) {
        // Chia chuỗi thành 2 phần: phần nguyên và phần thập phân
        var parts = str.toString().split('.');
        var integerPart = parts[0];
        var decimalPart = parts.length > 1 ? parts[1] : '';

        // Định dạng phần nguyên bằng cách chèn dấu ',' sau mỗi 3 số
        var formattedIntegerPart = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, ',');

        // Tạo chuỗi kết quả
        var formattedString;
        if (parts.length >= 2) {
            formattedString = formattedIntegerPart + '.' + decimalPart;
        } else {
            formattedString = formattedIntegerPart;
        }

        return formattedString;
    }

    function NumberThousands(event) {
        // remove any commas from earlier formatting
        const value = event.target.value.replace(/,/g, '');
        // try to convert to an integer
        const parsed = parseInt(value);
        // check if the integer conversion worked and matches the expected value
        if (!isNaN(parsed) && parsed == value) {
            // update the value
            event.target.value = new Intl.NumberFormat('en-US').format(value);
        }
    }

    function Debounce(func, delay = 600) {
        let timeout;

        return function executedFunc(...args) {
            if (timeout) {
                clearTimeout(timeout);
            }

            timeout = setTimeout(() => {
                func(...args);
                timeout = null;
            }, delay);
        };
    }

    function Enter(callback) {
        return function (e) {
            if (e.keyCode === 13) {
                callback(...arguments)
            }
        }
    }

    const maskDate = value => {
        let v = value.replace(/\D/g, '').slice(0, 10);
        if (v.length >= 5) {
            return `${v.slice(0, 2)}/${v.slice(2, 4)}/${v.slice(4)}`;
        }
        else if (v.length >= 3) {
            return `${v.slice(0, 2)}/${v.slice(2)}`;
        }
        return v
    }

    function LimitInputLengthOnKeyPress(maxLength) {
        return function (event) {
            const input = event.target;
            const inputValue = input.value.replaceAll(",", "");

            let valueLength = inputValue.length;
            if (valueLength > maxLength - 1) {
                event.preventDefault();
            }

            let sliceValue = inputValue.slice(0, maxLength);
            input.value = formatNumberWithCommas(sliceValue);
            return;
        };
    }

    function LimitNumber(maxLength) {
        return function (event) {
            const input = event.target;
            const inputValue = input.value.replaceAll(",", "");


            let valueLength = inputValue.length;
            if (valueLength > maxLength - 1) {
                event.preventDefault();
            }

            let isNumber = parseInt(inputValue);
            if (!isNumber) {
                input.value = "";
                return;
            }

            let sliceValue = inputValue.slice(0, maxLength);
            input.value = sliceValue;
            return;
        };
    }

    function LimitInputLengthOnKeyPressForText(maxLength) {
        return function (event) {
            const input = event.target;
            let inputValue = input.value;

            let valueLength = inputValue.length;
            if (valueLength > maxLength - 1) {
                event.preventDefault();
            }

            let sliceValue = inputValue.slice(0, maxLength);
            input.value = sliceValue;
        };
    }

    function PreventWhiteSpace(e) {
        let character = String.fromCharCode(e.keyCode);
        let isEnter = e.keyCode == 13;
        if (isEnter) {
            return true;
        } else {
            return !/[\s]/g.test(character);
        }
    }

    function RemoveWhiteSpaceOnKeyup(e) {
        let val = e.target.value;
        e.target.value = val.split(' ').join('');
    }

    function convertDecimalInventory(str, decimalNumber = 3) {
        // Chia chuỗi thành 2 phần: phần nguyên và phần thập phân
        var parts = str.toString().split('.');
        var integerPart = parts[0];
        var decimalPart = parts.length > 1 ? parts[1] : '';

        // Định dạng phần nguyên bằng cách chèn dấu ',' sau mỗi 3 số
        var formattedIntegerPart = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, ',');

        // Kiểm tra nếu phần thập phân toàn là số 0 hoặc rỗng
        if (!decimalPart || /^0+$/.test(decimalPart)) {
            return formattedIntegerPart;
        }

        // Lấy tối đa `decimalNumber` số sau dấu thập phân
        var formattedDecimalPart = decimalPart.slice(0, decimalNumber);

        // Tạo chuỗi kết quả
        return formattedIntegerPart + '.' + formattedDecimalPart;

    }

    return {
        PreventSepcialCharacterOnKeyPress: PreventSepcialCharacterOnKeyPress,
        TrimWhiteSpaceOnBlur: TrimWhiteSpaceOnBlur,
        OnlyNumerOnKeyPress: OnlyNumerOnKeyPress,
        FormEnter: Enter,
        LimitInputLengthOnKeyPress: LimitInputLengthOnKeyPress,
        RemoveVietnameseOnChange: RemoveVietnameseOnChange,
        LimitInputLengthOnKeyPressForText: LimitInputLengthOnKeyPressForText,
        LimitRawNumber: LimitNumber,
        RemoveSpecialCharacter: RemoveSpecialCharacter,
        PreventWhiteSpace: PreventWhiteSpace,
        RemoveWhiteSpaceOnKeyup: RemoveWhiteSpaceOnKeyup,
        Utils: {
            isValidText: isValidText,
            isValidNumber: isValidNumber,
            formatNumber: formatNumberWithCommas,
            debounce: Debounce,
            maskDate: maskDate,
            NumberThousands: NumberThousands,
            convertDecimalInventory: convertDecimalInventory
        }
    }
})()
