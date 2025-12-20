$(function () {
    waitForInventoriesLanguageData();

});

function waitForInventoriesLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        validateCreateInventory();
        cancelAddInventory();

        $("#btn_Apply_AddInventory").click(function () {
            if ($("#AddNewInventory_Form").valid()) {
                var link = $("#APIGateway").val();


                var userid = App.User.UserId;
                var inventoryDate = $("#Input_Date_Inventory").val();
                var auditFailPercentage = parseFloat($("#Ratio_Inventory").val());


                var filterData = {
                    UserId: userid,
                    InventoryDate: inventoryDate,
                    AuditFailPercentage: auditFailPercentage
                };

                $.ajax({
                    type: 'POST',
                    url: link + '/api/inventory/web/create',
                    data: JSON.stringify(filterData),
                    contentType: "application/json",
                    success: function (res) {
                        if (res.code == 200) {
                            toastr.success(window.languageData[window.currentLanguage]["Thêm mới đợt kiểm kê thành công."]);

                            $("#AddNewInventoryModal").modal("hide");
                            InputInventoryController.drawDatable();

                            //Reset input:
                            $("#input_date_inventory").val('')
                            $("#ratio_inventory").val('')

                            //Tắt datepicker đi:
                            $("#Input_Date_Inventory").datepicker('hide');

                        }
                        if (res.code == 90) {
                            Swal.fire({
                                title: `<b>${window.languageData[window.currentLanguage]["Không thể thêm mới đợt kiểm kê."]}</b>`,
                                text: `${window.languageData[window.currentLanguage]["Hiện đang có đợt kiểm kê chưa hoàn thành.Vui lòng không thêm đợt kiểm kê mới."]}`,
                                confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                                width: '30%'
                            }).then(() => {
                            })

                            $("#AddNewInventoryModal").modal("hide");

                            //Tắt datepicker đi:
                            $("#Input_Date_Inventory").datepicker('hide');
                        }
                        InputInventoryController.drawDatable()
                    },
                    error: function (error) {
                        //Tắt datepicker đi:
                        $("#Input_Date_Inventory").datepicker('hide');
                    }
                });

            }

        });

        $("#Input_Date_Inventory").keydown(function (e) {
            e.preventDefault(); // Chặn nhập chữ và số
        });

        $("#Input_Date_Inventory").on("change", function () {
            $("#Input_Date_Inventory-error").hide();
        })

        $("#AddNewInventoryModal")
            .find(".calendar_icon")
            .click(function (e) {
                let target = e.target;
                $(target).closest(".calendar_icon").prevAll("input").datepicker("show");
            });

        $("#Input_Date_Inventory").datepicker({
            format: "dd/mm/yyyy",
            autoclose: true,
            gotoCurrent: true,
            todayHighlight: true,
            todayBtn: "linked",
            clearBtn: true, // Thêm clearBtn như đoạn gốc
            startDate: "0d"
        }).on('show', function () {
            // Cập nhật text của nút "Clear" nếu cần
            $('.datepicker .clear').each(function () {
                if ($(this).text() === 'Xóa') {
                    $(this).text(window.languageData[window.currentLanguage]['Xóa']);
                }
            });

            // Cập nhật text của nút "Today" nếu cần
            $('.datepicker .today').each(function () {
                if ($(this).text() === 'Hôm nay') {
                    $(this).text(window.languageData[window.currentLanguage]['Hôm nay']);
                }
            });
        });


        $("#Ratio_Inventory").on("input", function () {
            if ($(this).val().length > maxLengthRatioInventory) {
                $(this).val($(this).val().slice(0, maxLengthRatioInventory));
            }
        });

    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForInventoriesLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}


var maxLengthRatioInventory = 2;

function validateCreateInventory() {
  $("#AddNewInventory_Form").validate({
    rules: {
      Input_Date_Inventory: {
        required: true,
      },
      Ratio_Inventory: {
        required: true,
      },
    },
    messages: {
      Input_Date_Inventory: {
            required: window.languageData[window.currentLanguage]["Vui lòng chọn ngày kiểm kê."],
      },
      Ratio_Inventory: {
          required: window.languageData[window.currentLanguage]["Vui lòng nhập tỉ lệ kiểm kê lại."],
      },
    },
  });
}

function cancelAddInventory () {
  $("#btn_Cancel_AddInventory").click(function () {
    $("#Input_Date_Inventory").val("");
    $("#Ratio_Inventory").val("");
    $("#Ratio_Inventory-error").hide();
    $("#Input_Date_Inventory-error").hide();
    $("#Input_Date_Inventory").val("").datepicker('update');
  })
}