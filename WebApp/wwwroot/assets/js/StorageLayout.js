$(function () {

    waitForStorageLayoutLanguageData();
})

function waitForStorageLayoutLanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        //Xoa Khu Vuc:
        DeleteLayout()
        GetComponentListFromLayoutDetail()

    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForStorageLayoutLanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}

//Xóa khu vực:
function DeleteLayout() {
    $(document).delegate(".ListStorage_Layout_Content .Image img", "click", (e) => {
        var getLayout = $(e.target).closest(".Image").data("id");

        Swal.fire({
            title: `<b>${window.languageData[window.currentLanguage]["Xác nhận xóa"]}</b>`,
            text: window.languageData[window.currentLanguage]["Nếu xóa khu vực này thì tất cả dữ liệu linh kiện có vị trí cố định thuộc khu vực sẽ bị xóa theo. Bạn có chắc chắn muốn xóa ?"],
            confirmButtonText: window.languageData[window.currentLanguage]['Đồng ý'],
            showCancelButton: true,
            showLoaderOnConfirm: true,
            cancelButtonText: window.languageData[window.currentLanguage]['Hủy bỏ'],
            customClass: {
                container: 'delete-layout-sweetalert'
            }
        }).then((result, e) => {
            if (result.isConfirmed) {
                var link = $("#APIGateway").val();

                var formData = {
                    layout: getLayout
                };

                $.ajax({
                    type: 'DELETE',
                    url: link + '/api/storage/layout/delete',
                    data: formData,
                    dataType: "json",
                    encode: true,
                    success: function (res) {
                        if (res.code == 200) {
                            toastr.success(window.languageData[window.currentLanguage][res.message])
                        }
                        setTimeout(() => {
                            window.location.reload();
                        }, 1500);
                    },
                    error: function (error) {
                        if (error.responseJSON.code) {
                            toastr.error(window.languageData[window.currentLanguage][error.responseJSON.message])
                        }
                    }
                });  

            }
        })

    })
}

function DeleteLayoutById(id) {
    
    Swal.fire({
        title: '<b>Xác nhận xóa</b>',
        text: "Nếu xóa khu vực này thì tất cả dữ liệu linh kiện có vị trí cố định thuộc khu vực sẽ bị xóa theo. Bạn có chắc chắn muốn xóa ?",
        confirmButtonText: 'Đồng ý',
        showCancelButton: true,
        showLoaderOnConfirm: true,
        cancelButtonText: 'Hủy bỏ',
        customClass: {
            container: 'delete-layout-sweetalert'
        }
    }).then((result, e) => {
        if (result.isConfirmed) {
            var link = $("#APIGateway").val();

            var formData = {
                layout: id
            };

            $.ajax({
                type: 'DELETE',
                url: link + '/api/storage/layout/delete',
                data: formData,
                dataType: "json",
                encode: true,
                success: function (res) {
                    if (res.code == 200) {
                        toastr.success(res.message)
                    }
                    setTimeout(() => {
                        window.location.reload();
                    }, 200);
                },
                error: function (error) {
                    if (error.responseJSON.code) {
                        toastr.error(error.responseJSON.message)
                    }
                }
            });

        }
    })

}


function GetComponentListFromLayoutDetail() {
    $(document).delegate(".layout-component", "click", (e) => {
        var form = $(e.target).closest(".frmGetComponentListFromLayoutDetail");
        form.trigger("submit");       
    });
    $(document).delegate(".layout-inventorystatus", "click", (e) => {
        var form = $(e.target).closest(".frmGetComponentListFromInventoryStatus");
        form.trigger("submit");
    });
}                          