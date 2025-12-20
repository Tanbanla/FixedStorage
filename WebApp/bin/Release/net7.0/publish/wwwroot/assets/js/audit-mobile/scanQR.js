//$(".top_bar").hide();

$(function () {
    AuditMobileHomeController.init();
})

var AuditMobileHomeController = (function () {
    let root = {
        el: $(".Views_AuditMobile_Index"),
        Apis: {
            ScanQR: async function (inventoryId, userId, componentCode) {
                return new Promise(async (resolve, reject) => {
                    $.ajax({
                        type: 'GET',
                        url: `${AppUser.getApiGateway}/api/inventory/${inventoryId}/account/${userId}/audit/scan/${componentCode}`,
                        success: function (response) {
                            resolve(response)
                        },
                        error: function (error) {
                            reject(error)
                        }
                    });
                })
            },
            LogoutMobile: async function (model) {
                return new Promise(async (resolve, reject) => {
                    $.ajax({
                        type: 'POST',
                        url: `${AppUser.getApiGateway}/api/identity/logout`,
                        contentType: "application/json",
                        data: JSON.stringify(model),
                        success: function (response) {
                            resolve(response)
                        },
                        error: function (error) {
                            reject(error)
                        }
                    });
                })
            },
        }
    }
    
    //const html5QrCode = new Html5Qrcode("reader");

    //async function StartCamera(onDetectCallback, onErrCallback) {
    //    let timeoutHandler;
    //    let isScanned = false;

    //    try {
    //        const devices = await Html5Qrcode.getCameras();
    //        debugger
    //        if (devices && devices.length) {
    //            const cameraId = devices[0].id;

    //            $("#dummyQRCode").hide();

    //            // Dừng sau 20 giây nếu không quét được
    //            if (timeoutHandler) {
    //                clearTimeout(timeoutHandler);
    //            }
    //            timeoutHandler = setTimeout(() => {
    //                if (!isScanned) {
    //                    StopCamera();

    //                    Swal.fire({
    //                        title: `<b>Lỗi</b>`,
    //                        text: `Hệ thống không nhận dạng được mã linh kiện. Vui lòng nhập mã linh kiện.`,
    //                        confirmButtonText: "Đồng ý",
    //                        width: "90%",
    //                    });
    //                }
    //            }, ScanQRTimeout);

    //            await html5QrCode.start(
    //                { deviceId: cameraId }, // Sử dụng object thay vì chỉ string
    //                {
    //                    fps: 10,
    //                    qrbox: 250,
    //                    videoConstraints: {
    //                        facingMode: "environment", // Cải thiện tương thích với iOS
    //                    },
    //                },
    //                (decodedText, decodedResult) => {
    //                    onDetectCallback(decodedText, decodedResult);
    //                    isScanned = true;
    //                    StopCamera();
    //                },
    //                (err) => {
    //                    console.error("Lỗi khi quét mã QR:", err);
    //                    if (onErrCallback) {
    //                        onErrCallback(err);
    //                    }
    //                }
    //            );
    //            debugger
    //            // Lấy stream và track video
    //            const stream = await navigator.mediaDevices.getUserMedia({
    //                video: {
    //                    deviceId: cameraId,
    //                    advanced: [{ zoom: true }]
    //                },
    //            });
    //            const videoTrack = stream.getVideoTracks()[0];
    //            const capabilities = videoTrack.getCapabilities();

    //            // Nếu hỗ trợ zoom, thêm logic zoom
    //            if (capabilities.zoom) {
    //                const zoomSlider = document.getElementById("zoomSliderQRCode");
    //                zoomSlider.min = capabilities.zoom.min;
    //                zoomSlider.max = capabilities.zoom.max;
    //                zoomSlider.step = capabilities.zoom.step || 1;
    //                zoomSlider.value = videoTrack.getSettings().zoom || capabilities.zoom.min;

    //                zoomSlider.addEventListener("input", () => {
    //                    const zoomLevel = parseFloat(zoomSlider.value);
    //                    videoTrack.applyConstraints({
    //                        advanced: [{ zoom: zoomLevel }],
    //                    });
    //                });
    //            } else {
    //                console.warn("Camera không hỗ trợ chức năng zoom.");
    //            }
    //        } else {
    //            Swal.fire({
    //                title: "Lỗi",
    //                text: "Không tìm thấy camera nào trên thiết bị này.",
    //                confirmButtonText: "OK",
    //            });
    //        }
    //    } catch (err) {
    //        console.error("Không thể khởi động camera:", err);
    //        Swal.fire({
    //            title: "Lỗi",
    //            text: "Không thể khởi động camera. Vui lòng kiểm tra quyền hoặc cấu hình.",
    //            confirmButtonText: "OK",
    //        });
    //        if (onErrCallback) {
    //            onErrCallback(err);
    //        }
    //    }
    //}

    //function StopCamera() {
    //    $("#dummyQRCode").show();
    //    html5QrCode.stop().then(() => {
    //        console.log("Camera stopped successfully.");
    //    }).catch((err) => {
    //        console.error("Lỗi khi dừng camera:", err);
    //    });
    //}


    async function StartCamera(onDetectCallback, onErrCallback) {
        let timeoutHandler;
        let isScanned = false;

        try {
            const devices = await Html5Qrcode.getCameras();
            if (devices && devices.length) {
                // Sử dụng camera sau nếu có
                const cameraId = devices.find(device => device.label.toLowerCase().includes("back"))?.id || devices[0].id;

                $("#dummyQRCode").hide();

                // Cấu hình scanner
                const scannerConfig = {
                    fps: 10,
                    qrbox: 250,
                    videoConstraints: {
                        facingMode: { ideal: "environment" }, // Yêu cầu camera sau
                    },
                };

                // Tạo scanner
                const scanner = new Html5QrcodeScanner("reader", scannerConfig, false);

                // Render scanner
                scanner.render(
                    (decodedText, decodedResult) => {
                        isScanned = true;
                        onDetectCallback(decodedText, decodedResult);
                        scanner.clear(); // Dừng scanner khi quét thành công
                    },
                    (err) => {
                        console.error("Lỗi khi quét mã QR:", err);
                        if (onErrCallback) {
                            onErrCallback(err);
                        }
                    }
                );

                // Loại bỏ nút "Stop Scanning"
                const removeStopButton = () => {
                    const stopScanningButton = document.querySelector("#html5-qrcode-button-camera-stop");
                    if (stopScanningButton) {
                        stopScanningButton.style.display = "none"; // Ẩn nút "Stop Scanning"
                    } else {
                        setTimeout(removeStopButton, 150); // Chờ nếu nút chưa sẵn sàng
                    }
                };
                removeStopButton();

                // Tự động dừng quét sau 20 giây nếu không quét được
                if (timeoutHandler) {
                    clearTimeout(timeoutHandler);
                }
                timeoutHandler = setTimeout(() => {
                    if (!isScanned) {
                        StopCamera(scanner);

                        Swal.fire({
                            title: `<b>Lỗi</b>`,
                            text: `Hệ thống không nhận dạng được mã linh kiện. Vui lòng nhập mã linh kiện.`,
                            confirmButtonText: "Đồng ý",
                            width: "90%",
                        });
                    }
                }, ScanQRTimeout);
                const stream = await navigator.mediaDevices.getUserMedia({
                    video: {
                        deviceId: { exact: cameraId },
                        facingMode: { ideal: "environment" },
                        advanced: [{ zoom: true }],
                    },
                });
                const videoTrack = stream.getVideoTracks()[0];

                if ("getCapabilities" in videoTrack) {
                    const capabilities = videoTrack.getCapabilities();

                    if (capabilities && capabilities.zoom) {
                        const zoomSlider = document.getElementById("zoomSliderQRCode");
                        if (zoomSlider) {
                            zoomSlider.min = capabilities.zoom.min;
                            zoomSlider.max = capabilities.zoom.max;
                            zoomSlider.step = capabilities.zoom.step || 1;
                            zoomSlider.value = videoTrack.getSettings().zoom || capabilities.zoom.min;

                            zoomSlider.addEventListener("input", async () => {
                                const zoomLevel = parseFloat(zoomSlider.value);
                                try {
                                    await videoTrack.applyConstraints({
                                        advanced: [{ zoom: zoomLevel }],
                                    });
                                } catch (err) {
                                    console.error("Lỗi khi áp dụng zoom:", err);
                                }
                            });
                        }
                    } else {
                        handleRangeCameraCapability(videoTrack);
                    }
                } else {
                    console.warn("Không thể lấy capabilities từ video track.");
                    handleRangeCameraCapability(videoTrack);
                }
                
            }
            //else {
            //    Swal.fire({
            //        title: "Lỗi",
            //        text: "Không tìm thấy camera nào trên thiết bị này.",
            //        confirmButtonText: "OK",
            //    });
            //}
        } catch (err) {
            console.error("Không thể khởi động camera:", err);
            //Swal.fire({
            //    title: "Lỗi",
            //    text: "Không thể khởi động camera. Vui lòng kiểm tra quyền hoặc cấu hình.",
            //    confirmButtonText: "OK",
            //});
            if (onErrCallback) {
                onErrCallback(err);
            }
        }


        function handleRangeCameraCapability(videoTrack) {
            const zoomSlider = document.getElementById("zoomSliderQRCode");
            if (zoomSlider) {
                zoomSlider.disabled = false;

                zoomSlider.addEventListener("input", async () => {
                    const zoomValue = parseFloat(zoomSlider.value);
                    try {
                        await videoTrack.applyConstraints({
                            advanced: [{ zoom: zoomValue }],
                        });
                    } catch (err) {
                        console.error("Lỗi khi điều chỉnh zoom với RangeCameraCapability:", err);
                    }
                });
            }
        }

    }

    function StopCamera(scanner) {
        $("#dummyQRCode").show();
        if (scanner) {
            scanner.clear().then(() => {
                console.log("Camera stopped successfully.");
            }).catch((err) => {
                console.error("Lỗi khi dừng camera:", err);
            });
        }
    }



    function Cache() {
        root.$form = root.el.find("#scanQRForm");
        root.$btnScanQR = root.el.find("#btnScanQR");
    }

    function Events() {
        $("#scanQRForm").submit((e) => {
            e.preventDefault();
        })

        $("#dummyQRCode").click(function (e) {
            StartCamera(async (data) => {
                //Gọi API
                let inventoryInfo = await AppUser.getUser().inventoryLoggedInfo();
                let inventoryId = inventoryInfo.inventoryModel.inventoryId;
                let accountId = AppUser.getUser().userId();
                
                loading(true);
                root.Apis.ScanQR(inventoryId, accountId, data).then(res => {
                    res?.data?.length == 1 ? RedirectToDocDetail(res.data[0].id) : ChoosePosition(res.data);

                }).catch(err => {
                    const { code, data, message } = err.responseJSON;

                    Swal.fire({
                        title: `<b>Lỗi</b>`,
                        text: `${message}`,
                        confirmButtonText: "Đã hiểu",
                        width: '90%'
                    });
                }).finally(() => {
                    loading(false);
                });
            }, (err) => {
                //Swal.fire({
                //    title: `<b>Lỗi</b>`,
                //    text: `Không tìm thấy thiết bị Camera.`,
                //    confirmButtonText: "Đã hiểu",
                //    width: '80%'
                //});
            })
        })

        root.$btnScanQR.click(async function (e) {
            let validForm = root.$form.valid();
            if (!validForm) return;

            let componentCode = $("#ComponentCode").val();
            //Gọi API
            let inventoryInfo = await AppUser.getUser().inventoryLoggedInfo();
            let inventoryId = inventoryInfo.inventoryModel.inventoryId;
            let accountId = AppUser.getUser().userId();

            loading(true);
            root.Apis.ScanQR(inventoryId, accountId, componentCode).then(res => {
                res?.data?.length == 1 ? RedirectToDocDetail(res.data[0].id) : ChoosePosition(res.data);
            }).catch(err => {
                const { code, data, message } = err.responseJSON;

                Swal.fire({
                    title: `<b>Lỗi</b>`,
                    text: `${message}`,
                    confirmButtonText: "Đã hiểu",
                    width: '90%'
                });
            }).finally(() => {
                loading(false);
            })
        })

        $("#btnLogout").click(async function (e) {
            window.location.href = "/logout";
        })


        $("body").delegate(".modal-backdrop", "click", function (e) {
            let modal = $("#auditMobile_sidebar_menu").modal("hide");
        })

        $("#ComponentCode").blur(ValidateInputHelper.TrimWhiteSpaceOnBlur);
        $("#ComponentCode").on("keypress keyup", ValidateInputHelper.LimitInputLengthOnKeyPressForText(20));

        $("#ComponentCode").blur(function () {
            root.$formValidator.resetForm();
        })
    }

    function RedirectToDocDetail(docId) {
        window.location.href = `${window.location.href}/documentdetail/${docId}`;
    }

    function ChoosePosition(data) {
        let resultHTML = data.map(item => {
            let docStatusColorClass = InventoryDocStatus_CSS[item.status];
            let docTitle = InventoryDocStatus[item.status];

            return `
                    <div class="form-check py-2 d-flex align-items-center gap-1">
                        <input class="form-check-input option_position my-auto" type="radio" name="documentId" value="${item.id}">
                        <text class="form-check-label txt-bolder txt-14" for="documentId">${item.positionCode}</text>: <text class="${docStatusColorClass} txt-13">${docTitle}</text>
                    </div>
                `
        }).join('');

        Swal.fire({
            title: '<b>Chọn vị trí</b>',
            html: `
                    <div class="select_position_container d-flex flex-column gap-2 py-1">
                        ${resultHTML}
                    </div>
            `,
            confirmButtonText: 'Đồng ý',
            showCancelButton: true,
            showLoaderOnConfirm: true,
            cancelButtonText: 'Hủy bỏ',
            reverseButtons: true,
            allowOutsideClick: false,
            customClass: {
                actions: "swal_confirm_actions"
            },
            preConfirm: (e) => {
                let selectedEl = $(".option_position:checked");
                if (!selectedEl?.length) {
                    toastr.error("Vui lòng chọn một vị trí.");

                    return false;
                } else {
                    return true;
                }
            }
        }).then((response) => {
            if (response.isConfirmed) {
                let docId = $(".option_position:checked").val();
                let docStatus = $(".option_position:checked").val();
                RedirectToDocDetail(docId);
            }
        })
    }

    function ValidateForm() {
        //jQuery.validator.addMethod("ComponentCodePattern", function (value, element) {
        //    return /^*$/.test(value);
        //}, 'Mã linh kiện không hợp lệ.');

        let validateModel = {
            rules: {
                ComponentCode: {
                    required: true,
                    noSpace: true,
                    //maxlength: 12
                    //ComponentCodePattern: true
                }
            },
            messages: {
                ComponentCode: {
                    required: "Vui lòng nhập mã linh kiện.",
                    //maxlength: "Độ dài ký tự không hợp lệ."
                },
            }
        }

        root.$formValidator = root.$form.validate(validateModel);
    }

    function PreLoad() {
        ValidateForm();
    }

    function Init() {
        Cache();
        Events();

        PreLoad();
    }

    return {
        init: Init
    }
})();