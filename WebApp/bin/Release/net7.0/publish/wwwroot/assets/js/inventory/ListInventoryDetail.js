$(function () {
  InputStorageDetailController.init();
});

var InputStorageDetailController = (function () {
  let root = {
    parentEl: $("#inventory-wrapper"),
  };

  function Cache() {
    root.$detailModal = $(root.parentEl);
    root.$inputDetailForm = root.$detailModal.find("#inventory-form");
    root.$inputDateFrom = $(
      root.parentEl.find("#InventoryDetail_input_date_from")
    );
    root.$btnConfirmImport = root.parentEl.find("#btnConfirmImport");
  }

  function Events() {
    root.parentEl.find(".calendar_icon").click((e) => {
      let target = e.target;
      $(target).closest(".calendar_icon").prevAll("input").datepicker("show");
    });

    $(".custom_icon_edit,#InventoryDetail_input_inventory_ratio").on("click", function (e) {
      // $("#InventoryDetail_input_inventory_ratio").prop("disabled", false);
      $("#InventoryDetail_input_inventory_ratio").removeClass("hidden-input");
    });

    $("#InventoryDetail_input_inventory_ratio").blur(function () {
      // $(this).prop("disabled", true);
      $("#InventoryDetail_input_inventory_ratio").addClass("hidden-input");
    });

      //Cập nhật chi tiết đợt kiểm kê:
      $(document).delegate("#btn-update-inventory","click", function () {
          if ($("#InventoryDetail_Form").valid()) {
            var getInventoryId = $("#inventory-wrapper").data("id");
            var userId = App.User.UserId;
            var getDate = $("#InventoryDetail_input_date_from").val();
            var status = parseInt($("#InventoryDetail_Status").val());
            var auditFailPercentage = parseFloat($("#InventoryDetail_input_inventory_ratio").val());
            var inventoryDate = moment(getDate, "DD/MM/YYYY").format("YYYY-MM-DD");
            var isLocked = $('#IsLockedInventory').is(':checked');

            var filterData = {
                UserId: userId,
                InventoryDate: inventoryDate,
                Status: status,
                AuditFailPercentage: auditFailPercentage,
                IsLocked: isLocked,
            };
  
            //Call Api Xem chi tiết:
            var link = $("#APIGateway").val();

            Swal.fire({
                title: '<b>Xác nhận lưu</b>',
                text: `Bạn có chắc chắn muốn cập nhật thông tin của đợt kiểm kê.`,
                confirmButtonText: 'Đồng ý',
                showCancelButton: true,
                showLoaderOnConfirm: true,
                cancelButtonText: 'Hủy bỏ',
                reverseButtons: true,
                allowOutsideClick: false,
                customClass: {
                    actions: "swal_confirm_actions"
                }
            }).then((result, e) => {
                if (result.isConfirmed) {

                    $.ajax({
                        type: 'PUT',
                        contentType: 'application/json',
                        data: JSON.stringify(filterData),
                        url: link + `/api/inventory/web/${getInventoryId}`,
                        success: function (res) {
                            if (res.code == 200) {
                                toastr.success(res.message);
                                $("#inventory-wrapper").attr('data-status', `${status}`)
                                $("#inventory-wrapper").attr('data-inventory-date', `${inventoryDate}`)
                            } else if (res.code == 64) {
                                Swal.fire({
                                    title: `<b>Không thể thay đổi trạng thái</b>`,
                                    text: `${res?.message}`,
                                    confirmButtonText: "Đã hiểu",
                                    width: '30%'
                                });
                            }

                        },
                        error: function (error) {
                            toastr.error(error.message)
                        }
                    });

                }
            });
        }

    });
  }

  function PreLoad() {
    $("#InventoryDetail_Form").validate({
      rules: {
        InventoryDetail_input_date_from: {
          required: true,
        },
        InventoryDetail_input_inventory_ratio: {
          required: true,
        },
      },
      messages: {
        InventoryDetail_input_date_from: {
          required: "Vui lòng chọn ngày kiểm kê.",
        },
        InventoryDetail_input_inventory_ratio: {
          required: "Vui lòng nhập tỉ lệ kiểm kê lại.",
        },
      },
    });
      $("#inventory-wrapper #InventoryDetail_input_date_from").datepicker({
          format: "dd/mm/yyyy",
          autoclose: true,
          gotoCurrent: true,
          todayHighlight: true,
          todayBtn: "linked",
          clearBtn: true
          //startDate: "0d",
      });

    $('#InventoryDetail_input_inventory_ratio').on('input', function() {
      var inputValue = $(this).val();  
      inputValue = inputValue.replace(/[^0-9]/g, '');
      inputValue = inputValue.substring(0, 2);
      $(this).val(inputValue);
    });

    $("#InventoryDetail_input_date_from").keydown(function (e) {
      e.preventDefault(); // Chặn nhập chữ và số
    });
  }

  function Init() {
    Cache();
    Events();
    PreLoad();
  }

  return {
    init: Init,
  };
})();
