$(function () {
    waitForInventoryListLanguageData();
});

function waitForInventoryListLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        InputInventoryController.init();
        $("#input_date_from,#input_date_to").keydown(function (e) {
            e.preventDefault(); // Chặn nhập chữ và số
        });

        //Validate Form Danh sách đợt kiểm kê:
        ValidateFormSearchInventory();

    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForInventoryListLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}


function ValidateFormSearchInventory() {
    $("#List_Inventory_User_Create").on("input", function () {
        var inputValue = $(this).val();

        // Giới hạn chỉ nhập đến ký tự thứ 50
        if (inputValue.length > 50) {
            $(this).val(inputValue.substr(0, 50));
        }

        // Loại bỏ dấu cách
        //$(this).val(function (index, value) {
        //    return value.replace(/\s/g, '');
        //});
    });
}

; async function CanDeleteAuditTarget() {
    let lastestUser = await AppUser.getUser();
    let lastestInventory = await lastestUser.inventoryLoggedInfo();
    let isGranEdit = await lastestUser.isGrant("EDIT_INVENTORY");

    let isInventoryDate = moment(lastestInventory?.inventoryModel?.inventoryDate || `1111-01-01T00:00:00`).diff(moment(), 'days') >= 0;
    let isValidPrivateAccount = isInventoryDate &&
        (isGranEdit && lastestUser.accountType == AccountType.TaiKhoanRieng) &&
        (lastestInventory?.inventoryModel?.status != InventoryStatus.Finish);

    let isPromoter = lastestInventory?.inventoryRoleType == 2;
    let canDeletePermission = isPromoter || isValidPrivateAccount;
    return canDeletePermission;
}

var maxLengthInputUsername = 50;

var InputInventoryController = (function () {
  let root = {
    parentEl: $(".Views_Inventory_Index"),
  };
    let datatable;

    let dataFilter = {
        CreatedBy: "",
        InventoryDateStart: "",
        InventoryDateEnd: "",
        Statuses: ""
    };

  function Events() {
    root.$searchForm.submit((e) => {
      e.preventDefault();
    });

    root.$searchForm.on(
      "keypress",
      ValidateInputHelper.FormEnter(function (e) {
        let validForm = root.$searchForm.valid();
        if (validForm) {
          datatable.draw();
        }
      })
    );

    $("#btnCreate_Inventory").on("click", function() {
      $("#AddNewInventoryModal").modal('show');
      //var today = moment();
      //var formattedDate = today.format("DD/MM/YYYY");
      //$("#Input_Date_Inventory").val(formattedDate).datepicker('update');
      $("#Input_Date_Inventory").val('');
      $("#Ratio_Inventory").val(5);
    })

    $(".btn-tab").click(async function() {
        var buttonId = $(this).attr('id');
        $('.btn-tab').removeClass('action');
        $(this).addClass('action');
        $(".tab-content").hide();
        $(`#tab-${buttonId}`).show();


        if (buttonId == "monitoring-create") {
            $.pub(GlobalEventName.inventory_tab_audittargetActived);
        }
    })

      $.sub(GlobalEventName.inventory_tab_audittargetActived, async (e) => {
          console.log(e)
      })

    //Xem chi tiết đợt kiểm kê:
    $(document).delegate(".detail-inventory", "click", (e) => {
          $(".breadcrumb li:nth-child(3) a").css({
            color: "#87868C"
          });
          var addBreadcrumbDetail = $(`
            <li>
              <img src="./assets/images/table_icons/arrow-right.svg" width="24px" height="24px" alt="arrow>">
            </li>
            <li>
            <a href="/inventory">${window.languageData[window.currentLanguage]["Xem chi tiết"]}</a>
            </li>
          `);
          $('.breadcrumb').append(addBreadcrumbDetail);
          
          var getInventoryId = $(e.target).data("id");

          $(".tab-content").hide();
          $("#tab-inventory-info").show();
          $(".Views_Inventory_Index").css("display", "none");
          $("#inventory-wrapper").css("display", "block");
          $(".page-content").addClass("custom_page_content");
          $(".container-fluid").addClass("custom_container_fluid");

          $("#inventory-wrapper").attr('data-id', `${getInventoryId}`)

          //Call Api Xem chi tiết:
          var link = $("#APIGateway").val();

          $.ajax({
              type: "GET",
              url: link + `/api/inventory/web/${getInventoryId}`,
              success: function (res) {
                  let getAccountType = App.User.AccountType;
                  let getInventoryRoleType = App.User.InventoryLoggedInfo.InventoryRoleType;
                  let currentDate = moment().format("YYYY-MM-DD");

                  if (res.code == 200) {
                      //Hiển thị Locked Đợt kiểm kê:
                      $("#IsLockedInventory").prop('checked', res?.data?.isLocked)

                      $('#InventoryDetail_input_date_from').val(res?.data?.inventoryDate);
                      $("#InventoryDetail_input_date_from").val(res?.data?.inventoryDate).datepicker('update');
                      $("#InventoryDetail_Status").val(res?.data?.status);
                      $("#InventoryDetail_input_inventory_ratio").val(res?.data?.auditFailPercentage);
                      $(".InventoryDetail_CreatedBy").text(res?.data?.createdBy);
                      $(".InventoryDetail_CreatedAt").text(res?.data?.createdAt);
                      $(".InventoryDetail_UpdatetedBy").text(res?.data?.updatedBy);
                      $(".InventoryDetail_UpdatetedAt").text(res?.data?.updatedAt);
                      $("span.InventoryDetail_InventoryName").text(res?.data?.inventoryName);

                      $("#inventory-wrapper").attr('data-aggregate-at', `${res?.data?.forceAggregateAt}`)
                      //Thêm trạng thái kiểm kê vào Tab:
                      $("#inventory-wrapper").attr('data-status', `${res?.data?.status}`)

                      //Thêm Ngày kiểm kê vào Tab:
                      var inventoryDate = moment(res?.data?.inventoryDate, "DD/MM/YYYY").format("YYYY-MM-DD");
                      $("#inventory-wrapper").attr('data-inventory-date', inventoryDate)
                      if (res?.data?.status == 3) {
                          $("#btn-update-inventory").hide();
                          $("#InventoryDetail_input_date_from").attr("disabled", true);
                          $("#InventoryDetail_Status").attr("disabled", true);
                          $("#InventoryDetail_input_inventory_ratio").attr("disabled", true);

                          //Ẩn icon đi:
                          $(".CalenderIcon").hide();
                          $(".PencilIcon").hide();
                          $("#InventoryDetail_Status").removeClass("form-select")
                          $("#InventoryDetail_Status").addClass("RemoveArrowDown")

                      } else {
                          //Logic phân quyền:
                          //TH1: Trạng thái: Hoàn thành thì chỉ cho xem dữ liệu:
                          //TH2: Trạng thái khác hoàn thành:
                            // + Nếu loại tài khoản là tài khoản chung(role: xúc tiến): Cho full chức năng:
                            // + Nếu loại tài khoản là tài khoản riêng: Phân quyền xem => cho xem dữ liệu, Phân quyền chỉnh sửa => Hiển thị các nút để chỉnh sửa dữ liệu.
                          if (getAccountType === "TaiKhoanChung" && getInventoryRoleType === 2) {
                              $("#btn-update-inventory").show();
                              $("#InventoryDetail_input_date_from").attr("disabled", true);
                              $("#InventoryDetail_Status").attr("disabled", false);
                              $("#InventoryDetail_input_inventory_ratio").attr("disabled", false);

                              //Ẩn icon đi:
                              $(".CalenderIcon").hide();
                              $(".PencilIcon").show();
                              $("#InventoryDetail_Status").addClass("form-select")
                              $("#InventoryDetail_Status").removeClass("RemoveArrowDown")

                          } else if (getAccountType === "TaiKhoanRieng") {
                              if (App.User.isGrant("VIEW_ALL_INVENTORY") || App.User.isGrant("VIEW_CURRENT_INVENTORY")) {
                                  $("#btn-update-inventory").hide();
                                  $("#InventoryDetail_input_date_from").attr("disabled", true);
                                  $("#InventoryDetail_Status").attr("disabled", true);
                                  $("#InventoryDetail_input_inventory_ratio").attr("disabled", true);

                                  //Ẩn icon đi:
                                  $(".CalenderIcon").hide();
                                  $(".PencilIcon").hide();
                                  $("#InventoryDetail_Status").removeClass("form-select")
                                  $("#InventoryDetail_Status").addClass("RemoveArrowDown")

                              }
                              if (App.User.isGrant("EDIT_INVENTORY")) {
                                  $("#btn-update-inventory").show();
                                  $("#InventoryDetail_input_date_from").attr("disabled", true);
                                  $("#InventoryDetail_Status").attr("disabled", false);
                                  $("#InventoryDetail_input_inventory_ratio").attr("disabled", false);

                                  //Ẩn icon đi:
                                  $(".CalenderIcon").hide();
                                  $(".PencilIcon").show();
                                  $("#InventoryDetail_Status").addClass("form-select")
                                  $("#InventoryDetail_Status").removeClass("RemoveArrowDown")

                              }

                              //Quá ngày kiểm kê:
                              if (moment(currentDate).isAfter(inventoryDate)) {
                                  $("#btn-update-inventory").hide();
                                  $("#InventoryDetail_input_date_from").attr("disabled", true);
                                  $("#InventoryDetail_Status").attr("disabled", true);
                                  $("#InventoryDetail_input_inventory_ratio").attr("disabled", true);

                                  //Ẩn icon đi:
                                  $(".CalenderIcon").hide();
                                  $(".PencilIcon").hide();
                                  $("#InventoryDetail_Status").removeClass("form-select")
                                  $("#InventoryDetail_Status").addClass("RemoveArrowDown")
                              }

                          }
                          
                      }
                  }

              },
              error: function (error) {
                  toastr.error(error.message)
              }
          });

      })

    root.$btnReset.click((e) => {
      let target = e.target;
      let thisButotn = $(target);

      root.$inputUserName.val("");
      root.$inputDateFrom.val("");
      root.$inputDateTo.val("");
      $("#input_date_from-error").hide();
      $("#input_date_to-error").hide();

      root.$selectStatus[0].reset();
      root.$selectStatus[0].toggleSelectAll(true);
      root.$inputDateFrom.val("").datepicker('update');
      root.$inputDateTo.val("").datepicker('update');

      datatable.draw();
    });

    //Search event
      root.$btnSearch.click(function (e) {
      let thisButton = $(this);

      let formValid = root.$searchForm.valid();
      if (formValid) {
        datatable.draw();
      }
    });

    //Export
      root.$btnExport.click(function (e) {

          var createdBy = $("#input_userName").val();
          var inventoryDateStart = $("#input_date_from").val();
          var inventoryDateEnd = $("#input_date_to").val();
          var statuses = $("#select_status").val();

          var filterData = {
              CreatedBy: createdBy,
              InventoryDateStart: inventoryDateStart,
              InventoryDateEnd: inventoryDateEnd,
              Statuses: statuses,
              
          };

          var url = '/export/inventory';
          $.ajax({
              type: 'POST',
              url: url,
              data: filterData,
              cache: false,
              xhrFields: {
                  responseType: 'blob'
              },
              success: function (response) {
                  if (response) {
                      var blob = new Blob([response], { type: response.type });
                      const fileURL = URL.createObjectURL(blob);
                      const link = document.createElement('a');
                      link.href = fileURL;

                      //Lấy ngày tháng hiện tại theo định dạng: yyyymmdd_hhmmss:
                      var currentTime = new Date();
                      var formattedTime = moment(currentTime).format("YYYYMMDD_HHmmss");

                      link.download = `DanhSachDotKiemKe_${formattedTime}.xlsx`;
                      link.click();
                  } else {
                      toastr.error(window.languageData[window.currentLanguage]["Không tìm thấy file."]);
                  }
                  toastr.success(window.languageData[window.currentLanguage]["Export danh sách đợt kiểm kê thành công."]);
              },
              error: function (error) {
                  if (error != undefined) {
                      toastr.error(error.message);
                  }
              }
          });
      });

    //Update Status:
      $(document).delegate("#InventoryStatus", "change", (e) => {
          e.preventDefault();

          var link = $("#APIGateway").val();

          var userId = App.User.UserId;

          let target = e.target;
          var inventoryId = $(target).data("id");
          var status = $(target).val();

          var currentStatus = $(target).attr("data-status");
          if (currentStatus == 3) {
              Swal.fire({
                  title: `<b>${window.languageData[window.currentLanguage]["Không thể thay đổi trạng thái"]}</b>`,
                  text: `${window.languageData[window.currentLanguage]["Bạn không thể thay đổi trạng thái của đợt kiểm kê đã hoàn thành."]}`,
                  confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                  width: '30%'
              });
              datatable.draw();
              return;
          }

          Swal.fire({
              title: `<b>${window.languageData[window.currentLanguage]["Xác nhận lưu"]}</b>`,
              text: `${window.languageData[window.currentLanguage]["Bạn có chắc chắn muốn cập nhật trạng thái của đợt kiểm kê."]}`,
              confirmButtonText: window.languageData[window.currentLanguage]["Đồng ý"],
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
                  $.ajax({
                      type: "PUT",
                      url: link + `/api/inventory/web/status/${inventoryId}/${status}/${userId}`,
                      success: function (res) {
                          if (res.code == 200) {
                              toastr.success(res.message)
                          }
                          if (res.code == 91) {
                              Swal.fire({
                                  title: `<b>${window.languageData[window.currentLanguage]["Không thể thay đổi trạng thái"]}</b>`,
                                  text: `${res?.message}`,
                                  confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                                  width: '30%'
                              });

                          }

                          if (res.code == 64) {
                              Swal.fire({
                                  title: `<b>${window.languageData[window.currentLanguage]["Không thể thay đổi trạng thái"]}</b>`,
                                  text: `${res?.message}`,
                                  confirmButtonText: window.languageData[window.currentLanguage]["Đã hiểu"],
                                  width: '30%'
                              });
                              
                          }

                          datatable.draw();

                      },
                      error: function (error) {
                          toastr.error(error.message)
                      }
                  });

              } else if (result.dismiss === Swal.DismissReason.cancel) {
                  datatable.draw();
              }
          });

          
      })

    root.parentEl.find(".calendar_icon").click((e) => {
      let target = e.target;
      $(target).closest(".calendar_icon").prevAll("input").datepicker("show");
    });
  }

  function Cache() {
    root.$inputDateFrom = $(root.parentEl.find("#input_date_from"));
    root.$inputDateTo = $(root.parentEl.find("#input_date_to"));
    root.$inputUserName = $(root.parentEl.find("#List_Inventory_User_Create"));
    root.$selectStatus = $(root.parentEl.find("#select_status"));

      root.$btnReset = root.parentEl.find("#btn-reset");
      root.$btnSearch = root.parentEl.find("#btn-search");
      root.$btnExport = root.parentEl.find("#export-file-listInventory");
      root.$updateStatus = root.parentEl.find("#InventoryStatus");


    root.$searchForm = $(root.parentEl.find("#input_inventory_search_form"));
  }

  function PreLoad() {
    jQuery.validator.addMethod(
      "validateDateRange",
      function (value, element) {
        let valid = true;

        let fromDate = root.parentEl.find("#input_date_from").val();
        let toDate = root.parentEl.find("#input_date_to").val();

        if (fromDate && toDate) {
          let fromDateMoment = moment(fromDate, "DD/MM/YYYY");
          let toDateMoment = moment(toDate, "DD/MM/YYYY");

          if (fromDateMoment > toDateMoment) {
            valid = false;
          }
        }
        return valid;
      },
      "Thời gian không đúng. Vui lòng chọn lại."
    );

    root.$searchForm.validate({
      rules: {
        FromDate: {
          validateDateRange: true,
          validDateFormat: true,
        },
        ToDate: {
          validateDateRange: true,
          validDateFormat: true,
        },
      },
    });

    root.$inputDateFrom.datepicker({
      format: "dd/mm/yyyy",
      autoclose: true,
      gotoCurrent: true,
      todayHighlight: true,
      todayBtn: "linked",
      clearBtn: true
    });

    root.$inputDateTo.datepicker({
      format: "dd/mm/yyyy",
      autoclose: true,
      gotoCurrent: true,
      todayHighlight: true,
      todayBtn: "linked",
      clearBtn: true
    });

    root.$selectStatus.find("option").attr("selected", true);

    VirtualSelect.init({
      ele: "#select_status",
        selectAllText: window.languageData[window.currentLanguage]["Tất cả"],
        noOptionsText: window.languageData[window.currentLanguage]["Không có kết quả"],
        noSearchResultsText: window.languageData[window.currentLanguage]["Không có kết quả"],
        searchPlaceholderText: window.languageData[window.currentLanguage]["Tìm kiếm"],
        allOptionsSelectedText: window.languageData[window.currentLanguage]["Tất cả"],
        optionsSelectedText: window.languageData[window.currentLanguage]["điều kiện đã được chọn"],
      selectAllOnlyVisible: true,
      hideClearButton: true,
    });

    root.$selectStatus = root.parentEl.find("#select_status");

    $("#input_date_from,#input_date_to").keydown(function (e) {
      e.preventDefault(); // Chặn nhập chữ và số
    });

    $("#input_date_from,#input_date_to").change(function (e) {
      root.$searchForm.valid();
    })

    $("#List_Inventory_User_Create").on("input", function () {
      if ($(this).val().length > maxLengthInputUsername) {
        $(this).val($(this).val().slice(0, maxLengthInputUsername));
      }
    });

    setTimeout(() => {
      root.$selectStatus.show();
    }, 500);

    InitImportListDatatable()
  }

    function InitImportListDatatable() {
        let host = App.ApiGateWayUrl;
        datatable = $('#list-inventory_table').DataTable({
            "bDestroy": true,
            "processing": `<div class="spinner"></div>`,
            pagingType: 'full_numbers',
            'language': {
                'loadingRecords': `<div class="spinner"></div>`,
                'processing': '<div class="spinner"></div>',
            },
            "serverSide": true,
            "filter": true,
            "searching": false,
            responsive: true,
            "lengthMenu": [10, 30, 50, 200],
            dom: 'rt<"bottom"flp><"clear">',
            "ordering": false,
            "ajax": {
                "url": host + "/api/inventory/web",
                "type": "POST",
                "contentType": "application/x-www-form-urlencoded",
                dataType: "json",
                data: function (data) {
                    dataFilter.CreatedBy = $("#List_Inventory_User_Create").val();
                    dataFilter.InventoryDateStart = $("#input_date_from").val();
                    dataFilter.InventoryDateEnd = $("#input_date_to").val();
                    dataFilter.Statuses = $("#select_status").val();

                    Object.assign(data, dataFilter);
                    return data;
                },
                "dataSrc": function ({ data }) {
                    return data;
                }
            },
            "drawCallback": function (settings) {
                let totalPages = datatable.page.info().pages;
                let totalRecords = datatable.page.info().recordsTotal;

                //Co du lieu thi show button xuat file excel:
                if (totalRecords > 0) {
                    $("#export-file-listInventory").attr("disabled", false);
                    $("#export-file-listInventory").removeClass("disable-button");

                } else {
                    $("#export-file-listInventory").attr("disabled", true);
                    $("#export-file-listInventory").addClass("disable-button");
                }

                let currPage = datatable.page() + 1;
                if (currPage == 1) {
                    root.parentEl.find(".pagination .first .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                    root.parentEl.find(".pagination .previous .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                }
                if (currPage == totalPages) {
                    root.parentEl.find(".pagination .next .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                    root.parentEl.find(".pagination .last .page-link").removeClass("datatable_disabled_color").addClass("datatable_disabled_color")
                }

                root.parentEl.find(".pagination").before(`<div class="h5 datatable_total_records"> <b>${window.languageData[window.currentLanguage]["Tổng"]}</b>: ${totalPages} <span class="total_record_seperate"> | </span>
                </div>`)

                if (totalRecords <= 10) {
                    $(".container-list-view .bottom").hide()
                }
                
            },
            //initComplete: function (settings, data) {
            //    let tableId = settings.sTableId;
            //    let datatableLength = $(`#${tableId}_length`);
            //    let optionValues = settings.aLengthMenu;
            //    let length = settings._iDisplayLength;

            //    let resultHtml = optionValues.map((val, i) => {
            //        return `<option value="${val}">Hiển thị ${val}</option>`
            //    }).join('')

            //    let selectElement = datatableLength.find("select");
            //    selectElement.html(`
            //        ${resultHtml}
            //    `)
            //    selectElement.val(length).change();

            //    let label = datatableLength.contents().eq(0);
            //    $(label).contents().each((i, el) => {
            //        if ($(el).is("select") == false) {
            //            $(el).remove()
            //        }
            //    })
            //},
            //"columnDefs": [
            //    { "width": "25%", "targets": 7 }
            //],
            "columns": [
                {
                    "data": "",
                    "name": "STT",
                    "render": function (data, type, row, index) {
                        let pagesize = index.settings._iDisplayLength;
                        let currentRow = ++index.row;
                        let currentPage = datatable.page() + 1;

                        let STT = ((currentPage - 1) * pagesize) + currentRow;

                        if (STT < 10) {
                            STT = `0${STT}`;
                        }
                        return STT;
                    },
                    "autoWidth": true
                },
                { "data": "inventoryName", "name": "Đợt kiểm kê", "autoWidth": true },
                {
                    "data": "inventoryDate", "name": "Ngày kiểm kê",
                    render: function (data, type, row, index) {
                        let result;
                        result = moment(data).format("DD/MM/YYYY");
                        return result;
                    },
                    "autoWidth": true
                },
                {
                    "data": "auditFailPercentage", "name": "Tỉ lệ kiểm kê lại(%)","autoWidth": true
                },
                {
                    "data": "status",
                    "name": "Trạng thái",
                    "render": function (data, type, row) {
                        let disabledStatus = "";
                        let showAndHideArrow = "";
                        let checkAccountTypeAndRole = App.User.AccountType == 'TaiKhoanChung' && App.User.InventoryLoggedInfo.InventoryRoleType == 2;
                        let RoleClaims = App.User.RoleClaims.map(x => x.ClaimType).includes("EDIT_INVENTORY");
                        if (checkAccountTypeAndRole) {
                            disabledStatus = "";
                            showAndHideArrow = "form-select form-select-lg";
                        } else {
                            if (RoleClaims) {
                                disabledStatus = "";
                                showAndHideArrow = "form-select form-select-lg";
                            } else {
                                disabledStatus = "disabled";
                                showAndHideArrow = "RemoveArrowDown";
                            }
                        }


                        const selectHtmlAccountType = `
                        <select id="InventoryStatus" data-id="${row.inventoryId}" data-status="${row.status}" class="${showAndHideArrow}" ${disabledStatus}>
                            <option value="0" ${data == 0 ? 'selected' : ''}>${window.languageData[window.currentLanguage]["Chưa kiểm kê"]}</option>
                            <option value="1" ${data == 1 ? 'selected' : ''}>${window.languageData[window.currentLanguage]["Đang kiểm kê"]}</option>
                            <option value="2" ${data == 2 ? 'selected' : ''}>${window.languageData[window.currentLanguage]["Đang giám sát"]}</option>
                            <option value="3" ${data == 3 ? 'selected' : ''}>${window.languageData[window.currentLanguage]["Hoàn thành"]}</option>
                        </select>
                        `;

                        return selectHtmlAccountType;
                    },
                    "autoWidth": true
                },
                {
                    "data": "fullName", "name": "Người tạo",
                    "autoWidth": true
                },
                {
                    "data": "createAt", "name": "Ngày tạo",
                    render: function (data, type, row, index) {
                        let result;
                        result = moment(data).format("DD/MM/YYYY HH:mm");
                        return result;
                    },
                    "autoWidth": true
                },
                {
                    "data": "",
                    "name": "",
                    "render": function (data, type, row) {
                        const selectHtmlSpecial = `
                        <div class="Controls_Container">
                            <div class="ViewDetail_Controls mx-3">
                                <a class="detail-inventory view_detail getInventoryId" data-id="${row.inventoryId}">${window.languageData[window.currentLanguage]["Xem chi tiết"]}</a>
                            </div>
                        </div>
                    `;
                        return selectHtmlSpecial;
                    },
                    "autoWidth": false,
                    "width": "115px"
                },
            ],
        });
    }

    function DrawDatable() {
        datatable.draw();
    }

  function Init() {
    if (root.parentEl?.length < 0) {
      //   console.error("Không tìm thấy Input storage container");
      return;
    }

    Cache();
    Events();
    PreLoad();
  }

  return {
      init: Init,
      drawDatable: DrawDatable
  };
})();
