let loadInventoryReportTimer;

$(function () {
    waitForReportingTab1LanguageData();

});

function waitForReportingTab1LanguageData() {
    // Kiểm tra nếu dữ liệu đã sẵn sàng
    if (window.languageData && window.currentLanguage) {
        $("#filterReporting").click(() => {
            $("#panelReporting").removeClass("d-none");
        });

        // if panelReporting is visible, hide it when click outside
        $(document).mouseup(function (e) {
            const container = $("#panelReporting");
            if (!container.is(e.target) && container.has(e.target).length === 0) {
                container.addClass("d-none");
            }
        });

        ReportingAuditShowMutilDropdown();

        //Click Nut Xem Danh Sach Phieu Kiem Ke:
        ClickViewListInventoryDoc();

        //Change Thoi diem hien thi bao cao:
        ChangeViewReportTime();

        ShowAnhHideDoctypeReport()

        async function ResetZoomDepartmentChart() {
            return new Promise((resolve, reject) => {
                window?.departmentReportChart?.resetZoom();
                setTimeout(() => {
                    resolve();
                }, 200);
            })
        }


        $("#exportFile").click(async function () {
            let isInventoryTabActive = $("#inventory").hasClass("active");

            if (isInventoryTabActive) {
                //Reset zoom
                await ResetZoomDepartmentChart();

                //An cac nut thua di truoc khi xuat pdf:
                $(".btnViewListInventoryDoc").hide();
                $(".buttonChart-area").hide();
                $(".ChartDocTypeTitle_Datatable #hideDetailTable").hide();
                $("#filterReporting").hide();
                $(".headerDetailTable #showTableReport3").hide();

                //Lấy ngày tháng hiện tại theo định dạng: yyyymmdd_hhmmss:
                var currentTime = new Date();
                var formattedTime = moment(currentTime).format("YYYYMMDD_HHmmss");

                let isOverFlowWidth = $("#InventoryReporting3_DataTable").outerWidth() > $('#inventory-report-tab').outerWidth();
                let tableWidth = $("#InventoryReporting3_DataTable").outerWidth() + 50;

                let body = document.body
                let html = document.documentElement
                let height = Math.max(body.scrollHeight, body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight)
                let target = document.querySelector('#inventory-report-tab')
                let heightCM = height / 20;

                let maxScreenWidth = Math.max(tableWidth);
                let cacheBodyScreen = $('#inventory-report-tab').outerWidth();
                $('#inventory-report-tab').width(maxScreenWidth);
                var see = window;
                var see1 = window.departmentReportChart;
                window.departmentReportChart.resize();
                $('#inventory-report-tab').css("margin", "auto");


                loading(true);
                html2pdf(target, {
                    margin: 0,
                    filename: `Baocaotiendo_${formattedTime}.pdf`,
                    html2canvas: { dpi: 190, letterRendering: false, scale: 2 },
                    jsPDF: {
                        orientation: 'portrait',
                        unit: 'cm',
                        format: [isOverFlowWidth ? (heightCM + 30) : heightCM, isOverFlowWidth ? 100 : 60],
                        compress: true,
                        precision: 16
                    }
                }).then(function () {
                    loading(false);
                    //Hien lai cac nut thua sau khi xuat pdf:
                    $(".btnViewListInventoryDoc").show();
                    $(".buttonChart-area").show();
                    $(".ChartDocTypeTitle_Datatable #hideDetailTable").show();
                    $("#filterReporting").show();
                    $(".headerDetailTable #showTableReport3").show();

                    $('#inventory-report-tab').width(cacheBodyScreen);
                    window.departmentReportChart.resize();

                    $('#inventory-report-tab').css("margin", "");

                })
            }

        });

        ////Lấy ra danh sách phòng ban của filter:
        GetDepartmentAPI().then(res => {

            let data = res.data;
            if (data.length == 0) {
                $("#DepartmentProgressReportFilter").html(window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phòng ban"]);
                return;
            }

            if (data.length) {
                let resultHTML = data.map(item => {
                    return `<span class="progress_department spanBtn" name="${item.departmentName}">${item.departmentName}</span>`
                });

                $("#DepartmentProgressReportFilter").html(resultHTML);
                $("#LocationProgressReportFilter").html(``);

                //Filter Báo cáo phòng ban, khu vực: active tất cả các phòng ban
                //$("#DepartmentProgressReportFilter .progress_department").trigger("click");

            }
        })

        //Click active department filter:
        ClickActiveDepartment()

        //CLick active ku vuc filter:
        ClickActiveLocation()

        //Click button Ap dung trong filter:
        $(document).delegate("#btnApplyFilterDepartmentLocationReport", "click", (e) => {
            let isLocation = false;
            let isDepartment = false;
            let list_departments = [];
            let list_locations = [];
            let inventoryId = $("#ReportingAudit_InventoryName").val();

            var CaptureTimeType = $("#reportType").val();

            $('#LocationProgressReportFilter .spanBtn').each(function () {
                if ($(this).hasClass('active')) {
                    isLocation = true;
                    return false;
                }
            });

            $('#DepartmentProgressReportFilter .spanBtn').each(function () {
                if ($(this).hasClass('active')) {
                    isDepartment = true;
                    return false;
                }
            });

            $("#inventory-report-tab .filterPanel").addClass("d-none")
            //case 1: không chọn phòng ban => Load hết dữ liệu báo cáo phòng ban
            //case 2: có chọn phòng ban, không chọn khu vực => Load hết dữ liệu phòng ban đã chọn
            //case 3: chọn phòng ban, chọn khu vực => Load hết dữ liệu khu vực vừa chọn
            if (isDepartment == false) {

                RenderChartDepartment(isLocation, list_departments, list_locations, CaptureTimeType, inventoryId)

            } else if (isDepartment && isLocation == false) {

                $('#DepartmentProgressReportFilter .spanBtn.active').each(function () {
                    var value = $(this).attr('name');
                    list_departments.push(value);
                });
                RenderChartDepartment(isLocation, list_departments, list_locations, CaptureTimeType, inventoryId)

            } else if (isDepartment && isLocation) {
                isLocation = false;
                $('#DepartmentProgressReportFilter .spanBtn.active').each(function () {
                    var value = $(this).attr('name');
                    list_departments.push(value);
                });

                $('#LocationProgressReportFilter .spanBtn.active').each(function () {
                    var value = $(this).attr('name');
                    list_locations.push(value);
                });

                RenderChartDepartment(isLocation, list_departments, list_locations, CaptureTimeType, inventoryId)

            }


        })


        //Chọn nhiều loại phiếu trên báo cáo tiến độ theo loại phiếu:
        $(document).delegate("button.buttonChart", "click", (e) => {
            let target = e.target;
            let isActive = $(target).hasClass("active");
            if (isActive) {
                $(target).removeClass("active");
            } else {
                $(target).removeClass("active").addClass("active");
            }

            var CaptureTimeType = $("#reportType").val();

            // Tạo một mảng để lưu trữ dữ liệu data-doctype
            var DocType = [];

            // Lặp qua tất cả các button có class "buttonChart" và có class "active"
            $('.buttonChart.active').each(function () {
                // Lấy giá trị của thuộc tính data-doctype và thêm vào mảng
                var doctypeValue = $(this).data('doctype');
                DocType.push(doctypeValue);
            });
            if (DocType.length > 0) {
                //$("#container-chart").removeClass("HideArea")
                //$("h4.ProgressReportNote").removeClass("HideArea")
                //$(".descriptionArea").removeClass("HideArea")
                //$(".headerDetailTable").removeClass("HideArea")
                //$("#InventoryReporting_DataTable").removeClass("HideArea")

                $(".ChartDocType").removeClass("HideArea")
                $("h4.ChartDocTypeNote").removeClass("HideArea")
                $(".ChartDocTypeArea").removeClass("HideArea")
                $(".ChartDocTypeTitle_Datatable").removeClass("HideArea")
                $(".ChartDocType_Datatable").removeClass("HideArea")
                $(".text-error-report").prop("hidden", true);
                $(".buttonChart-area").show();

                RenderChartDocType(DocType, CaptureTimeType)
            }
            else {
                //$("#container-chart").addClass("HideArea")
                //$("h4.ProgressReportNote").addClass("HideArea")
                //$(".descriptionArea").addClass("HideArea")
                //$(".headerDetailTable").addClass("HideArea")
                //$("#InventoryReporting_DataTable").addClass("HideArea")
                //$(".text-error-report").prop("hidden", false);

                $(".ChartDocType").addClass("HideArea")
                $("h4.ChartDocTypeNote").addClass("HideArea")
                $(".ChartDocTypeArea").addClass("HideArea")
                $(".ChartDocTypeTitle_Datatable").addClass("HideArea")
                $(".ChartDocType_Datatable").addClass("HideArea")
                $(".text-error-report").prop("hidden", false);
                $(".buttonChart-area").hide();

                $(".exportArea #exportFile").hide();
                return;
            }
        })

        //Tab bao cao tien do active thi call API bao cao:
        let checkTabIsActive = $("#inventory.changeTab").hasClass("active");
        if (checkTabIsActive) {
            //$(".exportArea #exportFile").show();
            //chart1:
            RenderChartDocType();

            // chart 2
            RenderChartDepartment();

            //15s chay api bao cao 1 lan:
            //Start timer loading audit report
            if (loadInventoryReportTimer) {
                window.clearInterval(loadInventoryReportTimer);
            }

            //15s cập nhật lại chart một lần
            let totalTime = 15000
            loadInventoryReportTimer = window.setInterval(() => {
                //chart1:
                var CaptureTimeType = $("#reportType").val();
                // Tạo một mảng để lưu trữ dữ liệu data-doctype
                var DocType = [];
                // Lặp qua tất cả các button có class "buttonChart" và có class "active"
                $('.buttonChart.active').each(function () {
                    // Lấy giá trị của thuộc tính data-doctype và thêm vào mảng
                    var doctypeValue = $(this).data('doctype');
                    DocType.push(doctypeValue);
                });

                RenderChartDocType(DocType, CaptureTimeType);



                // chart 2
                let isLocation = false;
                let list_departments = [];
                let list_locations = [];

                $("#DepartmentProgressReportFilter .progress_department.active").each(function () {
                    let departmentValue = $(this).attr('name');
                    list_departments.push(departmentValue);
                });

                $("#LocationProgressReportFilter .progress_location.active").each(function () {
                    let locationValue = $(this).attr('name');
                    list_locations.push(locationValue);
                });

                //if (list_locations.length > 0) {
                //    isLocation = true;
                //}

                RenderChartDepartment(isLocation, list_departments, list_locations, CaptureTimeType)
            }, totalTime);

        }

        handleShowTable();

        //Clear Interval khi chuyển Tab Bao cáo giám sát:
        $(document).delegate("#supervision", "click", (e) => {
            clearInterval(loadInventoryReportTimer);
        })

        $(document).delegate("#inventory", "click", (e) => {

            //chart1:
            RenderChartDocType();

            // chart 2
            RenderChartDepartment();

            //15s chay api bao cao 1 lan:
            //Start timer loading audit report
            if (loadInventoryReportTimer) {
                window.clearInterval(loadInventoryReportTimer);
            }

            //15s cập nhật lại chart một lần
            let totalTime = 15000
            loadInventoryReportTimer = window.setInterval(() => {
                //chart1:
                var CaptureTimeType = $("#reportType").val();

                // Tạo một mảng để lưu trữ dữ liệu data-doctype
                var DocType = [];

                // Lặp qua tất cả các button có class "buttonChart" và có class "active"
                $('.buttonChart.active').each(function () {
                    // Lấy giá trị của thuộc tính data-doctype và thêm vào mảng
                    var doctypeValue = $(this).data('doctype');
                    DocType.push(doctypeValue);
                });
                RenderChartDocType(DocType, CaptureTimeType);

                // chart 2
                let isLocation = false;
                let list_departments = [];
                let list_locations = [];

                $("#DepartmentProgressReportFilter .progress_department.active").each(function () {
                    let departmentValue = $(this).attr('name');
                    list_departments.push(departmentValue);
                });

                $("#LocationProgressReportFilter .progress_location.active").each(function () {
                    let locationValue = $(this).attr('name');
                    list_locations.push(locationValue);
                });

                //if (list_locations.length > 0) {
                //    isLocation = true;
                //}

                RenderChartDepartment(isLocation, list_departments, list_locations, CaptureTimeType)
            }, totalTime);
        })

    } else {
        // Nếu chưa có dữ liệu, tiếp tục chờ với setTimeout
        setTimeout(waitForReportingTab1LanguageData, 70); // Chờ thêm 100ms rồi kiểm tra lại
    }
}
function ReportingAuditShowMutilDropdown() {
    var dropdownSelectors = [
        //'#ReportingAudit_InventoryName',
        `#ReportingAudit_Type`
    ]
    dropdownSelectors.map(selctor => {
        if ($(selctor).find("option").length > 1) {
            VirtualSelect.init({
                ele: selctor,
                selectAllText: window.languageData[window.currentLanguage]['Tất cả'],
                multiple: true,
                alwaysShowSelectedOptionsCount: false,
                alwaysShowSelectedOptionsLabel: false,
                disableAllOptionsSelectedText: false,
                selectAllOnlyVisible: false,
                noOptionsText: window.languageData[window.currentLanguage]['Không có kết quả'],
                noSearchResultsText: window.languageData[window.currentLanguage]['Không có kết quả'],
                searchPlaceholderText: window.languageData[window.currentLanguage]['Tìm kiếm'],
                allOptionsSelectedText: window.languageData[window.currentLanguage]['Tất cả'],
                optionsSelectedText: window.languageData[window.currentLanguage]["điều kiện đã được chọn"],
                optionSelectedText: window.languageData[window.currentLanguage]["điều kiện đã được chọn"],
                hideClearButton: true,
                autoSelectFirstOption: true,
            });
        } else {
            VirtualSelect.init({
                ele: selctor,
                selectAllText: window.languageData[window.currentLanguage]['Tất cả'],
                multiple: true,
                alwaysShowSelectedOptionsCount: false,
                alwaysShowSelectedOptionsLabel: true,
                disableAllOptionsSelectedText: true,
                noOptionsText: window.languageData[window.currentLanguage]['Không có kết quả'],
                noSearchResultsText: window.languageData[window.currentLanguage]['Không có kết quả'],
                searchPlaceholderText: window.languageData[window.currentLanguage]['Tìm kiếm'],
                allOptionsSelectedText: window.languageData[window.currentLanguage]['Tất cả'],
                optionsSelectedText: window.languageData[window.currentLanguage]["điều kiện đã được chọn"],
                optionSelectedText: window.languageData[window.currentLanguage]["điều kiện đã được chọn"],
                selectAllOnlyVisible: false,
                hideClearButton: true,
            });
        }
    })

    //$("#ReportingAudit_InventoryName")[0].reset();
    //let currInventory = App?.User?.InventoryLoggedInfo?.InventoryModel?.InventoryId;
    //let firstInventoryOption = $("#ReportingAudit_InventoryName")[0]?.options[0]?.value || "";
    //$("#ReportingAudit_InventoryName")[0].setValue(currInventory || firstInventoryOption);

    $("#ReportingAudit_Type")[0].reset();
    $("#ReportingAudit_Type")[0].toggleSelectAll(true);

    //$("#ReportingAudit_InventoryName").hide();
    $("#ReportingAudit_Type").hide();

}
function handleShowTable() {
    $("#showTableReport3").click(() => {
        
        const table = $("#InventoryReporting3_DataTable");
        const status = table.attr("data-status");
        switch (status) {
          case "hide":
            table.attr("data-status", "show");
                table.removeClass("d-none");
                $("#showTableReport3").text(window.languageData[window.currentLanguage]["Ẩn chi tiết bảng"]);
            break;
          case "show":
            table.attr("data-status", "hide");
            table.addClass("d-none");
                $("#showTableReport3").text(window.languageData[window.currentLanguage]["Hiển thị chi tiết bảng"]);
            break;
          default:
            break;
        }
  });
}

function RenderChartDocType(DocType, CaptureTimeType, inventoryId) {
    //Call API Bao Cao Tien Do Theo Loai Phieu:
    let InventoryId = (typeof inventoryId === 'undefined') ? $("#ReportingAudit_InventoryName").val() : inventoryId;
    let captureTimeType = (typeof CaptureTimeType === 'undefined') ? "0" : CaptureTimeType.toString();
    let departments = [];
    let locations = [];

    let filterData = {
        InventoryId: InventoryId,
        CaptureTimeType: captureTimeType,
        DocTypes: DocType,
        Departments: departments,
        Locations: locations
    };


    GetProgressReportAPI(filterData).then(res => {
        google.charts.load("current", { packages: ["corechart"] });
        google.charts.setOnLoadCallback(function () {
            if (res?.data?.progressReportDocTypes.length > 0) {
                $(".ChartDocType").removeClass("HideArea")
                $("h4.ChartDocTypeNote").removeClass("HideArea")
                $(".ChartDocTypeArea").removeClass("HideArea")
                $(".ChartDocTypeTitle_Datatable").removeClass("HideArea")
                $(".ChartDocType_Datatable").removeClass("HideArea")
                $(".text-error-report").prop("hidden", true);
                $(".buttonChart-area").show();

                $(".exportArea #exportFile").show();

                // Thêm cột cho mỗi header cho datatable
                let columnHeaders = `<thead>
                            <tr>
                                <th class="th-sm" rowspan="2">
                                    ${window.languageData[window.currentLanguage]["Hạng mục"]}
                                </th>`;

                //Clear lại button chọn loại phiếu:
                $('.buttonChartDoctypeA').removeClass('active')
                $('.buttonChartDoctypeB').removeClass('active')
                $('.buttonChartDoctypeE').removeClass('active')
                $('.buttonChartDoctypeC').removeClass('active')

                let titleArr = [];

                let titleSet = new Set();
                let docTypeMap = {
                    0: window.languageData[window.currentLanguage]["Phiếu A"],
                    1: window.languageData[window.currentLanguage]["Phiếu B"],
                    2: window.languageData[window.currentLanguage]["Phiếu E"],
                    3: window.languageData[window.currentLanguage]["Phiếu C"]
                };

                res.data.progressReportDocTypes.forEach(item => {
                    let DocType = ""

                    if (item.docType == 0) {
                        titleArr.push(window.languageData[window.currentLanguage]["Phiếu A"])
                        DocType = window.languageData[window.currentLanguage]["Phiếu A"];
                        $('.buttonChartDoctypeA').addClass('active')
                    } else if (item.docType == 1) {
                        titleArr.push(window.languageData[window.currentLanguage]["Phiếu B"])
                        DocType = window.languageData[window.currentLanguage]["Phiếu B"];
                        $('.buttonChartDoctypeB').addClass('active')
                    } else if (item.docType == 2) {
                        titleArr.push(window.languageData[window.currentLanguage]["Phiếu E"])
                        DocType = window.languageData[window.currentLanguage]["Phiếu E"];
                        $('.buttonChartDoctypeE').addClass('active')

                    } else if (item.docType == 3) {
                        titleArr.push(window.languageData[window.currentLanguage]["Phiếu C"])
                        DocType = window.languageData[window.currentLanguage]["Phiếu C"];
                        $('.buttonChartDoctypeC').addClass('active')

                    }
                    columnHeaders += `
                    <th class="th-sm">
                        ${DocType}
                    </th>`;

                    let docTypeKey = docTypeMap[item.docType];
                    if (docTypeKey) {
                        titleSet.add(window.languageData[window.currentLanguage][docTypeKey]);
                        
                    }
                });

                //Fill Cac loai phieu vao header:
                //if (titleArr.length == 4) {
                //    $('.ChartDocTypeTitle_Datatable_value').text(window.languageData[window.currentLanguage]["Tất cả"])
                //} else {
                //    $('.ChartDocTypeTitle_Datatable_value').text(titleArr.join(", ").trim())
                //}
                if (titleSet.size === 4) {
                    $('.ChartDocTypeTitle_Datatable_value').text(window.languageData[window.currentLanguage]["Tất cả"]);
                } else {
                    $('.ChartDocTypeTitle_Datatable_value').text([...titleSet].join(", ").trim());
                }

                columnHeaders += `<th class="th-sm" colspan="2">
                    ${window.languageData[window.currentLanguage]["Tất cả"]} </th> </tr>`;

                columnHeaders += `<tr>`;

                res.data.progressReportDocTypes.forEach(item => {
                    columnHeaders += `<td class="th-sm">SL</td>`;
                });

                columnHeaders += `<td class="th-sm">SL</td>
                    <td class="th-sm">${window.languageData[window.currentLanguage]["Tỉ lệ"]}</td>
                </tr> </thead>`;

                //Tong so phieu:
                let tongPhieu = `<tbody><tr><td>${window.languageData[window.currentLanguage]["Tổng phiếu"]}</td>`;

                let sumPhieu = 0;
                tongPhieu += res.data.progressReportDocTypes.map(item => {
                    sumPhieu += item.totalDoc;
                    return `<td>${convertDouble(item.totalDoc)}</td>`
                })
                tongPhieu += `<td>${convertDouble(sumPhieu)}</td>`
                tongPhieu += `<td>100%</td>`
                tongPhieu += `</tr>`;

                //So phieu chua kiem ke:
                let phieuChuaKiemKe = `<tr><td>${window.languageData[window.currentLanguage]["Số phiếu chưa kiểm kê"]}</td>`;

                let sumPhieuChuaKiemke = 0;
                let percentPhieuChuaKiemke = 0;
                phieuChuaKiemKe += res.data.progressReportDocTypes.map(item => {
                    sumPhieuChuaKiemke += item.totalTodo;
                    return `<td>${convertDouble(item.totalTodo)}</td>`
                })

                percentPhieuChuaKiemke = +(((parseFloat(sumPhieuChuaKiemke) / parseFloat(sumPhieu)) * 100)).toFixed(2)

                phieuChuaKiemKe += `<td>${convertDouble(sumPhieuChuaKiemke)}</td>`
                phieuChuaKiemKe += `<td>${percentPhieuChuaKiemke}%</td>`
                phieuChuaKiemKe += `</tr>`;

                //So phieu da kiem ke:
                let phieuDaKiemKe = `<tr><td>${window.languageData[window.currentLanguage]["Số phiếu đã kiểm kê"]}</td>`;

                let sumPhieuDaKiemKe = 0;
                let percentPhieuDaKiemKe = 0;
                phieuDaKiemKe += res.data.progressReportDocTypes.map(item => {
                    sumPhieuDaKiemKe += item.totalInventory;
                    return `<td>${convertDouble(item.totalInventory)}</td>`
                })

                percentPhieuDaKiemKe = +(((parseFloat(sumPhieuDaKiemKe) / parseFloat(sumPhieu)) * 100)).toFixed(2)

                phieuDaKiemKe += `<td>${convertDouble(sumPhieuDaKiemKe)}</td>`
                phieuDaKiemKe += `<td>${percentPhieuDaKiemKe}%</td>`
                phieuDaKiemKe += `</tr>`;

                //So phieu da xác nhận:
                let phieuDaXacNhan = `<tr><td>${window.languageData[window.currentLanguage]["Số phiếu đã xác nhận"]}</td>`;

                let sumPhieuDaXacNhan = 0;
                let percentPhieuDaXacNhan = 0;

                phieuDaXacNhan += res.data.progressReportDocTypes.map(item => {
                    sumPhieuDaXacNhan += item.totalConfirm;
                    return `<td>${convertDouble(item.totalConfirm)}</td>`
                })

                percentPhieuDaXacNhan = +(((parseFloat(sumPhieuDaXacNhan) / parseFloat(sumPhieu)) * 100)).toFixed(2)
                
                phieuDaXacNhan += `<td>${convertDouble(sumPhieuDaXacNhan)}</td>`
                phieuDaXacNhan += `<td>${percentPhieuDaXacNhan}%</td>`
                phieuDaXacNhan += `</tr></tbody>`;


                $("#InventoryReporting_DataTable").html(columnHeaders + tongPhieu + phieuChuaKiemKe + phieuDaKiemKe + phieuDaXacNhan);

                var data = google.visualization.arrayToDataTable([
                    ["Task", "Hours per Day"],
                    [window.languageData[window.currentLanguage]["Chưa kiểm kê"], { v: parseFloat(percentPhieuChuaKiemke), f: `${parseFloat(percentPhieuChuaKiemke)}%` }],
                    [window.languageData[window.currentLanguage]["Đã xác nhận"], { v: parseFloat(percentPhieuDaXacNhan), f: `${parseFloat(percentPhieuDaXacNhan)}%` }],
                    [window.languageData[window.currentLanguage]["Đã kiểm kê"], { v: parseFloat(percentPhieuDaKiemKe), f: `${parseFloat(percentPhieuDaKiemKe)}%` }],
                ]);

                var options = {
                    is3D: true,
                    colors: ["#C2C2C2", "#009543", "#F3A600"],
                    legend: {
                        position: "none",
                        textStyle: { color: 'black', fontSize: 16 }
                    },
                    tooltip: {
                        text: 'value'
                    },
                    pieSliceBorderColor: "white",
                    pieSliceText: 'value',
                    pieSliceTextStyle: 
                    {
                        color: "black"
                    },
                    slices: {
                        0: { border: '1px solid white' },
                    },
                    chartArea: {
                        left: 0,
                        top: 0,
                        width: "100%",
                        height: "100%",
                    },
                    plugins: {
                        pieSliceBorderColor: "white", 
                        datalabels: {
                            display: false
                        },
                        outlabels: {
                            display: false
                        }
                    }
                };

                var chart = new google.visualization.PieChart(
                    document.getElementById("container-chart")
                );
                chart.draw(data, options);

            }
            else {
                // Ẩn biểu đồ và hiển thị thông báo không có dữ liệu
                $(".ChartDocType").addClass("HideArea")
                $("h4.ChartDocTypeNote").addClass("HideArea")
                $(".ChartDocTypeArea").addClass("HideArea")
                $(".ChartDocTypeTitle_Datatable").addClass("HideArea")
                $(".ChartDocType_Datatable").addClass("HideArea")
                $(".text-error-report").prop("hidden", false);
                $(".buttonChart-area").hide();
                $(".exportArea #exportFile").hide();
            }


        });

    }).catch(() => {
        // Ẩn biểu đồ và hiển thị thông báo không có dữ liệu
        $(".ChartDocType").addClass("HideArea")
        $("h4.ChartDocTypeNote").addClass("HideArea")
        $(".ChartDocTypeArea").addClass("HideArea")
        $(".ChartDocTypeTitle_Datatable").addClass("HideArea")
        $(".ChartDocType_Datatable").addClass("HideArea")
        $(".text-error-report").prop("hidden", false);
        $(".buttonChart-area").hide();
        $(".exportArea #exportFile").hide();
    })
}

//Call API Doctype Report:
function GetProgressReportAPI(model) {
    return new Promise(async (resolve, reject) => {
        var link = $("#APIGateway").val();
        var url = link + `/api/inventory/report/progress`;
        $.ajax({
            type: 'POST',
            url: url,
            data: JSON.stringify(model),
            contentType: 'application/json',
            success: function (res) {
                resolve(res)
            },
            error: function (err) {
                reject(err)
            }
        });
    })
}

function ChangeViewReportTime() {
    //$(document).delegate("#reportType", "change", (e) => {
    //    var DocType = [];
    //    var CaptureTimeType = $("#reportType").val();

    //    //Render Báo cáo theo loại phiếu:
    //    RenderChartDocType(DocType, CaptureTimeType);

    //    //Render Báo cáo theo phòng ban, khu vực:

    //    var isLocation = false;
    //    var list_departments = [];
    //    var list_locations = [];

    //    RenderChartDepartment(isLocation, list_departments, list_locations, CaptureTimeType);
    //})

    //Thay đổi thời điểm báo cáo tiến độ:
    $("#reportType").on("change", function () {
        let isInventoryTabActive = $("#inventory").hasClass("active");
        if (!isInventoryTabActive) {
            return;
        }
        let CaptureTimeType = $(this).val();

        //Call API Bao Cao Tien Do Theo Loai Phieu:
        let InventoryId = $("#ReportingAudit_InventoryName").val();
        //let captureTimeType = $("#reportType").val();
        let DocType = [];

        let departments = [];
        let locations = [];
        let checkIsLocation = false;

        let filterData = {
            InventoryId: InventoryId,
            CaptureTimeType: CaptureTimeType,
            DocTypes: DocType,
            Departments: departments,
            Locations: locations
        };


        GetProgressReportAPI(filterData).then(res => {
            RenderChangedDocTypeReport(res);
            RenderChangedDepartmentLocationReport(checkIsLocation, res);

        }).catch(() => {
            //Biểu đồ báo cáo theo loại phiếu:
            // Ẩn biểu đồ và hiển thị thông báo không có dữ liệu:
            $(".ChartDocType").addClass("HideArea")
            $("h4.ChartDocTypeNote").addClass("HideArea")
            $(".ChartDocTypeArea").addClass("HideArea")
            $(".ChartDocTypeTitle_Datatable").addClass("HideArea")
            $(".ChartDocType_Datatable").addClass("HideArea")
            $(".text-error-report").prop("hidden", false);
            $(".buttonChart-area").hide();
            $(".exportArea #exportFile").hide();

            //Biểu đồ báo cáo theo phòng ban, khu vực:
            $(".Container-DepartmentLocation").hide();
            $(".text-error-department-report").prop("hidden", false);

            $(".exportArea #exportFile").hide();
        })

    });

    //Thay đổi đợt kiểm kê, sẽ call lại api lấy ra danh sách báo cáo giám sát:
    $("#ReportingAudit_InventoryName").on("change", function () {
        let isInventoryTabActive = $("#inventory").hasClass("active");
        if (!isInventoryTabActive) {
            return;
        }
        let selectedInventoryId = $(this).val();

        //Call API Bao Cao Tien Do Theo Loai Phieu:
        let InventoryId = selectedInventoryId;
        let captureTimeType = $("#reportType").val();
        let DocType = [];

        let departments = [];
        let locations = [];
        let checkIsLocation = false;

        let filterData = {
            InventoryId: InventoryId,
            CaptureTimeType: captureTimeType,
            DocTypes: DocType,
            Departments: departments,
            Locations: locations
        };


        GetProgressReportAPI(filterData).then(res => {
            RenderChangedDocTypeReport(res);
            RenderChangedDepartmentLocationReport(checkIsLocation, res);

        }).catch(() => {
            //Biểu đồ báo cáo theo loại phiếu:
            // Ẩn biểu đồ và hiển thị thông báo không có dữ liệu:
            $(".ChartDocType").addClass("HideArea")
            $("h4.ChartDocTypeNote").addClass("HideArea")
            $(".ChartDocTypeArea").addClass("HideArea")
            $(".ChartDocTypeTitle_Datatable").addClass("HideArea")
            $(".ChartDocType_Datatable").addClass("HideArea")
            $(".text-error-report").prop("hidden", false);
            $(".buttonChart-area").hide();
            $(".exportArea #exportFile").hide();

            //Biểu đồ báo cáo theo phòng ban, khu vực:
            $(".Container-DepartmentLocation").hide();
            $(".text-error-department-report").prop("hidden", false);

            $(".exportArea #exportFile").hide();
        })

    });

}

function RenderChangedDocTypeReport(res) {
    google.charts.load("current", { packages: ["corechart"] });
    google.charts.setOnLoadCallback(function () {
        // Kiểm tra dữ liệu và xử lý chỉ với một phần nhỏ nếu cần
        if (res?.data?.progressReportDocTypes.length > 0) {
            // Hiển thị các phần tử UI
            $(".ChartDocType").removeClass("HideArea");
            $("h4.ChartDocTypeNote").removeClass("HideArea");
            $(".ChartDocTypeArea").removeClass("HideArea");
            $(".ChartDocTypeTitle_Datatable").removeClass("HideArea");
            $(".ChartDocType_Datatable").removeClass("HideArea");
            $(".text-error-report").prop("hidden", true);
            $(".buttonChart-area").show();
            $(".exportArea #exportFile").show();

            // Khởi tạo các biến để tính toán
            let columnHeaders = `<thead><tr><th class="th-sm" rowspan="2">${window.languageData[window.currentLanguage]["Hạng mục"]}</th>`;
            let titleArr = [];
            let sumTotalDoc = 0;
            let sumTotalTodo = 0;
            let sumTotalInventory = 0;
            let sumTotalConfirm = 0;

            let titleSet = new Set();
            let docTypeMap = {
                0: window.languageData[window.currentLanguage]["Phiếu A"],
                1: window.languageData[window.currentLanguage]["Phiếu B"],
                2: window.languageData[window.currentLanguage]["Phiếu E"],
                3: window.languageData[window.currentLanguage]["Phiếu C"]
            };

            // Duyệt qua danh sách một lần duy nhất
            res.data.progressReportDocTypes.forEach(item => {
                let DocType = "";
                if (item.docType === 0) {
                    titleArr.push(window.languageData[window.currentLanguage]["Phiếu A"]);
                    DocType = window.languageData[window.currentLanguage]["Phiếu A"];
                    $('.buttonChartDoctypeA').addClass('active');
                } else if (item.docType === 1) {
                    titleArr.push(window.languageData[window.currentLanguage]["Phiếu B"]);
                    DocType = window.languageData[window.currentLanguage]["Phiếu B"];
                    $('.buttonChartDoctypeB').addClass('active');
                } else if (item.docType === 2) {
                    titleArr.push(window.languageData[window.currentLanguage]["Phiếu E"]);
                    DocType = window.languageData[window.currentLanguage]["Phiếu E"];
                    $('.buttonChartDoctypeE').addClass('active');
                } else if (item.docType === 3) {
                    titleArr.push(window.languageData[window.currentLanguage]["Phiếu C"]);
                    DocType = window.languageData[window.currentLanguage]["Phiếu C"];
                    $('.buttonChartDoctypeC').addClass('active');
                }

                // Xây dựng header cột
                columnHeaders += `<th class="th-sm">${DocType}</th>`;

                // Tính tổng các giá trị
                sumTotalDoc += item.totalDoc;
                sumTotalTodo += item.totalTodo;
                sumTotalInventory += item.totalInventory;
                sumTotalConfirm += item.totalConfirm;

                let docTypeKey = docTypeMap[item.docType];
                if (docTypeKey) {
                    titleSet.add(window.languageData[window.currentLanguage][docTypeKey]);

                }
            });

            if (titleSet.size === 4) {
                $('.ChartDocTypeTitle_Datatable_value').text(window.languageData[window.currentLanguage]["Tất cả"]);
            } else {
                $('.ChartDocTypeTitle_Datatable_value').text([...titleSet].join(", ").trim());
            }

            // Hoàn thiện header
            columnHeaders += `<th class="th-sm" colspan="2">${window.languageData[window.currentLanguage]["Tất cả"]}</th></tr>`;
            columnHeaders += `<tr>${res.data.progressReportDocTypes.map(() => `<td class="th-sm">SL</td>`).join("")}`;
            columnHeaders += `<td class="th-sm">SL</td><td class="th-sm">${window.languageData[window.currentLanguage]["Tỉ lệ"]}</td></tr></thead>`;

            // Tạo nội dung bảng
            const createRow = (label, values, total, percent) => {
                return `<tr><td>${label}</td>${values.join("")}<td>${convertDouble(total)}</td><td>${percent}%</td></tr>`;
            };

            const totalDocValues = res.data.progressReportDocTypes.map(item => `<td>${convertDouble(item.totalDoc)}</td>`);
            const totalTodoValues = res.data.progressReportDocTypes.map(item => `<td>${convertDouble(item.totalTodo)}</td>`);
            const totalInventoryValues = res.data.progressReportDocTypes.map(item => `<td>${convertDouble(item.totalInventory)}</td>`);
            const totalConfirmValues = res.data.progressReportDocTypes.map(item => `<td>${convertDouble(item.totalConfirm)}</td>`);

            const tongPhieu = createRow(
                window.languageData[window.currentLanguage]["Tổng phiếu"],
                totalDocValues,
                sumTotalDoc,
                "100"
            );

            const phieuChuaKiemKe = createRow(
                window.languageData[window.currentLanguage]["Số phiếu chưa kiểm kê"],
                totalTodoValues,
                sumTotalTodo,
                ((sumTotalTodo / sumTotalDoc) * 100).toFixed(2)
            );

            const phieuDaKiemKe = createRow(
                window.languageData[window.currentLanguage]["Số phiếu đã kiểm kê"],
                totalInventoryValues,
                sumTotalInventory,
                ((sumTotalInventory / sumTotalDoc) * 100).toFixed(2)
            );

            const phieuDaXacNhan = createRow(
                window.languageData[window.currentLanguage]["Số phiếu đã xác nhận"],
                totalConfirmValues,
                sumTotalConfirm,
                ((sumTotalConfirm / sumTotalDoc) * 100).toFixed(2)
            );

            // Gắn dữ liệu vào bảng
            $("#InventoryReporting_DataTable").html(columnHeaders + `<tbody>${tongPhieu}${phieuChuaKiemKe}${phieuDaKiemKe}${phieuDaXacNhan}</tbody>`);

            // Vẽ biểu đồ Google Chart
            const data = google.visualization.arrayToDataTable([
                ["Task", "Hours per Day"],
                [window.languageData[window.currentLanguage]["Chưa kiểm kê"], { v: parseFloat(((sumTotalTodo / sumTotalDoc) * 100).toFixed(2)), f: `${((sumTotalTodo / sumTotalDoc) * 100).toFixed(2)}%` }],
                [window.languageData[window.currentLanguage]["Đã xác nhận"], { v: parseFloat(((sumTotalConfirm / sumTotalDoc) * 100).toFixed(2)), f: `${((sumTotalConfirm / sumTotalDoc) * 100).toFixed(2)}%` }],
                [window.languageData[window.currentLanguage]["Đã kiểm kê"], { v: parseFloat(((sumTotalInventory / sumTotalDoc) * 100).toFixed(2)), f: `${((sumTotalInventory / sumTotalDoc) * 100).toFixed(2)}%` }],
            ]);

            const options = {
                is3D: true,
                colors: ["#C2C2C2", "#009543", "#F3A600"],
                legend: { position: "none", textStyle: { color: 'black', fontSize: 16 } },
                tooltip: { text: 'value' },
                pieSliceBorderColor: "white",
                pieSliceText: 'value',
                pieSliceTextStyle: { color: "black" },
                chartArea: { left: 0, top: 0, width: "100%", height: "100%" },
            };

            const chart = new google.visualization.PieChart(document.getElementById("container-chart"));
            chart.draw(data, options);
        }
        else {
            // Ẩn biểu đồ và hiển thị thông báo không có dữ liệu
            $(".ChartDocType").addClass("HideArea")
            $("h4.ChartDocTypeNote").addClass("HideArea")
            $(".ChartDocTypeArea").addClass("HideArea")
            $(".ChartDocTypeTitle_Datatable").addClass("HideArea")
            $(".ChartDocType_Datatable").addClass("HideArea")
            $(".text-error-report").prop("hidden", false);
            $(".buttonChart-area").hide();
            $(".exportArea #exportFile").hide();
        }


    });

}

function RenderChangedDepartmentLocationReport(checkIsLocation, res) {
    let departmentArray = [];
    let percentTotalTodo = [];
    let percentTotalInventory = [];
    let percentTotalConfirm = [];
    let totalTodo = [];
    let totalInventory = [];
    let totalConfirm = [];

    if (checkIsLocation == false) {
        if (res?.data?.progressReportDepartments.length > 0) {

            $(".Container-DepartmentLocation").show();
            $(".text-error-department-report").prop("hidden", true);

            $(".exportArea #exportFile").show();

            //Danh sach phong ban:
            res.data.progressReportDepartments.forEach(item => {
                departmentArray.push(item.department);
                totalConfirm.push(item.totalConfirm);
                totalInventory.push(item.totalInventory);
                totalTodo.push(item.totalTodo);

                percentTotalConfirm.push(parseFloat((item.totalConfirm / item.totalDoc) * 100).toFixed(2));
                percentTotalInventory.push(parseFloat((item.totalInventory / item.totalDoc) * 100).toFixed(2));
                percentTotalTodo.push(parseFloat((item.totalTodo / item.totalDoc) * 100).toFixed(2));
            });

            //Render table
            let subHeader = ``
            let layoutHeaders = res.data.progressReportDepartments.map(item => {
                subHeader += `<th>SL</th><th>${window.languageData[window.currentLanguage]["Tỉ lệ"]}</th>`
                return `<th colspan="2">${item.department} </th>`;
            })

            let tableHeader = `<thead><tr><th rowspan="2">${window.languageData[window.currentLanguage]["Hạng mục"]}</th>${layoutHeaders}</tr>
                                <tr>${subHeader}</tr></thead>`


            //Tổng phiếu:
            let textTotalDoc = `<td>${window.languageData[window.currentLanguage]["Tổng phiếu"]}</td>`;
            res.data.progressReportDepartments.map(item => {
                let tongPhieu = +(parseFloat((item.totalDoc / item.totalDoc) * 100)).toFixed(2);
                let tongPhieu_GT = tongPhieu;
                if (tongPhieu == 100.00) {
                    tongPhieu_GT = 100
                } else if (tongPhieu == 0.00) {
                    tongPhieu_GT = 0
                }

                textTotalDoc += `<td>${convertDouble(item.totalDoc)}</td>
                               <td>${tongPhieu_GT}%</td>`
            })

            //Số phiếu chưa kiểm kê:
            let textTotalTodo = `<td>${window.languageData[window.currentLanguage]["Số phiếu chưa kiểm kê"]}</td>`;
            res.data.progressReportDepartments.map(item => {
                let tongPhieu = +(parseFloat((item.totalTodo / item.totalDoc) * 100)).toFixed(2);
                textTotalTodo += `<td>${convertDouble(item.totalTodo)}</td>
                               <td>${tongPhieu}%</td>`
            })


            //Số phiếu đã kiểm kê:
            let textTotalInventory = `<td>${window.languageData[window.currentLanguage]["Số phiếu đã kiểm kê"]}</td>`;
            res.data.progressReportDepartments.map(item => {
                let tongPhieu = +(parseFloat((item.totalInventory / item.totalDoc) * 100)).toFixed(2);
                textTotalInventory += `<td>${convertDouble(item.totalInventory)}</td>
                               <td>${tongPhieu}%</td>`
            })

            //Số phiếu đã xác nhận
            let textTotalConfirm = `<td>${window.languageData[window.currentLanguage]["Số phiếu đã xác nhận"]}</td>`;
            res.data.progressReportDepartments.map(item => {
                let tongPhieu = +(parseFloat((item.totalConfirm / item.totalDoc) * 100)).toFixed(2);

                textTotalConfirm += `<td>${convertDouble(item.totalConfirm)}</td>
                               <td>${tongPhieu}%</td>`
            })

            let tableBody = `<tbody>
                                <tr>${textTotalDoc}</tr>
                                <tr>${textTotalTodo}</tr>
                                <tr>${textTotalInventory}</tr>
                                <tr>${textTotalConfirm}</tr>
                            </tbody>`
            $("#InventoryReporting3_DataTable").html(tableHeader + tableBody);

        }
        else {
            $(".Container-DepartmentLocation").hide();
            $(".text-error-department-report").prop("hidden", false);

            $(".exportArea #exportFile").hide();
        }
    }
    else {
        if (res?.data?.progressReportLocations.length > 0) {
            $(".Container-DepartmentLocation").show();
            $(".text-error-department-report").prop("hidden", true);

            $(".exportArea #exportFile").show();

            //Danh sach phong ban:
            res.data.progressReportLocations.forEach(item => {
                departmentArray.push(item.location);
                totalConfirm.push(item.totalConfirm);
                totalInventory.push(item.totalInventory);
                totalTodo.push(item.totalTodo);

                percentTotalConfirm.push(parseFloat((item.totalConfirm / item.totalDoc) * 100).toFixed(2));
                percentTotalInventory.push(parseFloat((item.totalInventory / item.totalDoc) * 100).toFixed(2));
                percentTotalTodo.push(parseFloat((item.totalTodo / item.totalDoc) * 100).toFixed(2));
            });

            //Render table
            let subHeader = ``
            let layoutHeaders = res.data.progressReportLocations.map(item => {
                subHeader += `<th>SL</th><th>${window.languageData[window.currentLanguage]["Tỉ lệ"]}</th>`
                return `<th colspan="2">${item.location} </th>`;
            })

            let tableHeader = `<thead><tr><th rowspan="2">${window.languageData[window.currentLanguage]["Hạng mục"]}</th>${layoutHeaders}</tr>
                                <tr>${subHeader}</tr></thead>`


            //Tổng phiếu
            let textTotalDoc = `<td>${window.languageData[window.currentLanguage]["Tổng phiếu"]}</td>`;
            res.data.progressReportLocations.map(item => {
                let tongPhieu = parseFloat((item.totalDoc / item.totalDoc) * 100).toFixed(2);
                let tongPhieu_GT = tongPhieu;
                if (tongPhieu == 100.00) {
                    tongPhieu_GT = 100
                } else if (tongPhieu == 0.00) {
                    tongPhieu_GT = 0
                }
                textTotalDoc += `<td>${item.totalDoc}</td>
                               <td>${tongPhieu_GT}%</td>`
            })

            //Số phiếu chưa kiểm kê:
            let textTotalTodo = `<td>${window.languageData[window.currentLanguage]["Số phiếu chưa kiểm kê"]}</td>`;
            res.data.progressReportLocations.map(item => {
                let tongPhieu = parseFloat((item.totalTodo / item.totalDoc) * 100).toFixed(2);
                let tongPhieu_GT = tongPhieu;
                if (tongPhieu == 100.00) {
                    tongPhieu_GT = 100
                } else if (tongPhieu == 0.00) {
                    tongPhieu_GT = 0
                }
                textTotalTodo += `<td>${item.totalTodo}</td>
                               <td>${tongPhieu_GT}%</td>`
            })


            //Số phiếu đã kiểm kê:
            let textTotalInventory = `<td>${window.languageData[window.currentLanguage]["Số phiếu đã kiểm kê"]}</td>`;
            res.data.progressReportLocations.map(item => {
                let tongPhieu = parseFloat((item.totalInventory / item.totalDoc) * 100).toFixed(2);
                let tongPhieu_GT = tongPhieu;
                if (tongPhieu == 100.00) {
                    tongPhieu_GT = 100
                } else if (tongPhieu == 0.00) {
                    tongPhieu_GT = 0
                }
                textTotalInventory += `<td>${item.totalInventory}</td>
                               <td>${tongPhieu_GT}%</td>`
            })

            //Số phiếu đã xác nhận
            let textTotalConfirm = `<td>${window.languageData[window.currentLanguage]["Số phiếu đã xác nhận"]}</td>`;
            res.data.progressReportLocations.map(item => {
                let tongPhieu = parseFloat((item.totalConfirm / item.totalDoc) * 100).toFixed(2);
                let tongPhieu_GT = tongPhieu;
                if (tongPhieu == 100.00) {
                    tongPhieu_GT = 100
                } else if (tongPhieu == 0.00) {
                    tongPhieu_GT = 0
                }
                textTotalConfirm += `<td>${item.totalConfirm}</td>
                               <td>${tongPhieu_GT}%</td>`
            })

            let tableBody = `<tbody>
                                <tr>${textTotalDoc}</tr>
                                <tr>${textTotalTodo}</tr>
                                <tr>${textTotalInventory}</tr>
                                <tr>${textTotalConfirm}</tr>
                            </tbody>`
            $("#InventoryReporting3_DataTable").html(tableHeader + tableBody);

        }
        else {
            $(".Container-DepartmentLocation").hide();
            $(".text-error-department-report").prop("hidden", false);

            $(".exportArea #exportFile").hide();
        }
    }
    //Render ra bieu do chart:
    // create new element
    const newCtx = document.createElement("canvas");
    newCtx.id = "myChartReporting";

    let delayed;

    const zoomOptions = {
        pan: {
            enabled: true,
            mode: 'x',
            speed: 0.1,
        },
        zoom: {
            wheel: {
                enabled: true
            },
            pinch: {
                enabled: true,
                speed: 0.1,
            },
            mode: 'x',
            speed: 0.1,
        },
    };


    window.departmentReportChart = new Chart(newCtx, {
        type: "bar",
        data: {
            labels: departmentArray,
            datasets: [
                {
                    label: window.languageData[window.currentLanguage]["Tỉ lệ chưa kiểm kê"],
                    backgroundColor: "#50555C",
                    data: percentTotalTodo, // phần trăm
                    borderColor: "#50555C",
                    pointBorderColor: "#FFFFFF",
                    borderWidth: 1.5,
                    type: "line",
                    pointRadius: 5,
                    pointHoverRadius: 10,
                    yAxisID: "y1",
                },
                {
                    label: window.languageData[window.currentLanguage]["Tỉ lệ đã xác nhận"],
                    backgroundColor: "#009543",
                    data: percentTotalConfirm, // phần trăm
                    borderColor: "#009543",
                    pointBorderColor: "#FFFFFF",
                    borderWidth: 1.5,
                    type: "line",
                    pointRadius: 5,
                    pointHoverRadius: 10,
                    yAxisID: "y1",
                },
                {
                    label: window.languageData[window.currentLanguage]["Tỉ lệ đã kiểm kê"],
                    backgroundColor: "#DA8D00",
                    data: percentTotalInventory, // phần trăm
                    borderColor: "#DA8D00",
                    pointBorderColor: "#FFFFFF",
                    borderWidth: 1.5,
                    type: "line",
                    pointRadius: 5,
                    pointHoverRadius: 10,
                    yAxisID: "y1",
                },
                {
                    label: window.languageData[window.currentLanguage]["Đã xác nhận"],
                    backgroundColor: "#009543",
                    data: totalConfirm,
                    type: "bar",
                    barThickness: 50,
                },
                {
                    label: window.languageData[window.currentLanguage]["Đã kiểm kê"],
                    backgroundColor: "#DA8D00",
                    data: totalInventory,
                    type: "bar",
                    barThickness: 50,
                },
                {
                    label: window.languageData[window.currentLanguage]["Chưa kiểm kê"],
                    backgroundColor: "#C2C2C2",
                    data: totalTodo,
                    type: "bar",
                    barThickness: 50,
                },
            ],
        },
        options: {
            onClick: (e, activeEls) => {
                //let datasetIndex = activeEls[0].datasetIndex;
                let dataIndex = activeEls[0].index;
                //let datasetLabel = e.chart.data.datasets[datasetIndex].label;
                //let value = e.chart.data.datasets[datasetIndex].data[dataIndex];
                let label = e.chart.data.labels[dataIndex];

                //$(`#DepartmentProgressReportFilter .progress_department`).removeClass("active");
                //$(`#DepartmentProgressReportFilter .progress_department[name="${label}"]`).trigger("click");
                let isLocation = true;
                let list_departments = [];
                let list_locations = [];
                var CaptureTimeType = $("#reportType").val();

                let list_locations1 = [];
                let list_locations2 = [];

                list_departments.push(label);

                let filterData = {
                    Departments: list_departments
                }
                $("#LocationProgressReportFilter .progress_location.active").each(function () {
                    let locationValue = $(this).attr('name');
                    list_locations2.push(locationValue);
                });

                GetLocationAPI(filterData).then(res => {
                    res.data.map(item => {
                        list_locations1.push(item.locationName);
                    })
                    if (list_locations2.length > 0) {

                        for (var i = 0; i < list_locations1.length; i++) {
                            if (list_locations2.indexOf(list_locations1[i]) !== -1) {
                                list_locations.push(list_locations1[i]);
                            }
                        }
                        RenderChartDepartment(isLocation, list_departments, list_locations, CaptureTimeType)
                    } else {
                        if (list_locations1.length > 0) {
                            RenderChartDepartment(isLocation, list_departments, list_locations1, CaptureTimeType)
                        }
                    }
                })


            },
            // set style for label
            plugins: {
                legend: false,
                zoom: zoomOptions
            },
            animation: {
                duration: 0,
                onComplete: () => {
                    delayed = true;
                },
                delay: (context) => {
                    let delay = 0;
                    if (
                        context.type === "data" &&
                        context.mode === "default" &&
                        !delayed
                    ) {
                        delay = context.dataIndex * 300 + context.datasetIndex * 100;
                    }
                    return delay;
                },
            },
            scales: {
                x: {
                    stacked: true,
                    grid: {
                        display: false,
                    },
                    ticks: {
                        font: {
                            weight: "600",
                            color: "#333333",
                            size: 14,
                        },
                        padding: 35,
                    },
                },
                y: {
                    stacked: true,
                    grid: {
                        display: false,
                    },
                    ticks: {
                        font: {
                            weight: "400",
                            color: "#333333",
                            size: 14,
                        },
                    },
                },
                // y1 is percents
                y1: {
                    position: "right",
                    min: 0,
                    max: 100,
                    ticks: {
                        callback: function (value) {
                            return value + "%";
                        },
                        font: {
                            weight: "400",
                            color: "#333333",
                            size: 14,
                        },
                    },
                },
            },
            responsive: true
        },
    });

    // replace canvas with newCtx
    $("#myChartReporting").replaceWith(newCtx);
}

function ClickViewListInventoryDoc() {
    $(document).delegate(".btnViewListInventoryDoc", "click", (e) => {
        window.location.href = "/inventory-document";
    })
}

function ShowAnhHideDoctypeReport() {
    $(document).delegate(".ShowAndHideDoctypeReport", "click", (e) => {

        var getDataTableStatus = $("#InventoryReporting_DataTable").attr("data-table-status");
        if (getDataTableStatus == "show") {
            $("#InventoryReporting_DataTable").attr("data-table-status", "hide");
            $("#InventoryReporting_DataTable").addClass("d-none");
            $(".ShowAndHideDoctypeReport").text(window.languageData[window.currentLanguage]["Hiển thị chi tiết bảng"]);
        } else {
            $("#InventoryReporting_DataTable").attr("data-table-status", "show");
            $("#InventoryReporting_DataTable").removeClass("d-none");
            $(".ShowAndHideDoctypeReport").text(window.languageData[window.currentLanguage]["Ẩn chi tiết bảng"]);
        }
    })
}

function GetDepartmentAPI() {
    return new Promise(async (resolve, reject) => {

        var link = $("#APIGateway").val();
        let url = `${link}/api/inventory/location/departments`;

        $.ajax({
            type: 'GET',
            url: url,
            contentType: 'application/json',
            success: function (response) {
                resolve(response)
            },
            error: function (error) {
                reject(error)
            }
        });
    })
}

function GetLocationAPI(filterData) {
    return new Promise(async (resolve, reject) => {
        var link = $("#APIGateway").val();

        $.ajax({
            type: "POST",
            url: `${link}/api/inventory/location/departmentname`,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify(filterData),
            success: function (res) {
                resolve(res);
            },
            error: function (err) {
                reject(err)
            }
        });
    })
}

function ClickActiveDepartment() {
    $(document).delegate("#DepartmentProgressReportFilter .spanBtn", "click", (e) => {
        let target = e.target;
        let isActive = $(target).hasClass("active");
        if (isActive) {
            $(target).removeClass("active");
        } else {
            $(target).removeClass("active").addClass("active");
        }

        //Publish department changed

        let activeDepartments = $("#DepartmentProgressReportFilter .progress_department.active");

        if (activeDepartments.length == 0) {
            $("#LocationProgressReportFilter").html(``);
        }
        if (activeDepartments.length) {
            let departmentNames = [...activeDepartments].map(item => {
                return $(item).attr("name")
            })

            let filterData = {
                Departments: departmentNames
            }

            GetLocationAPI(filterData).then(res => {
                if (res?.data?.length) {
                    let html = res.data.map((item) => {
                        return `<span class="progress_location spanBtn" name="${item.locationName}">${item.locationName}</span>`
                    })

                    $("#LocationProgressReportFilter").html(html);
                }
            })
        }
        updateCheckboxState();

    })

    //Click chọn tất cả phòng ban trong bộ lọc:
    $("#DepartmentProgressReportingToggleAll").change(function () {
        if ($(this).is(":checked")) {
            $(".progress_department").addClass("active");

            let activeDepartments = $("#DepartmentProgressReportFilter .progress_department.active");

            if (activeDepartments.length == 0) {
                $("#LocationProgressReportFilter").html(``);
            }
            if (activeDepartments.length) {
                let departmentNames = [...activeDepartments].map(item => {
                    return $(item).attr("name")
                })

                let filterData = {
                    Departments: departmentNames
                }

                GetLocationAPI(filterData).then(res => {
                    if (res?.data?.length) {
                        let html = res.data.map((item) => {
                            return `<span class="progress_location spanBtn" name="${item.locationName}">${item.locationName}</span>`
                        })

                        $("#LocationProgressReportFilter").html(html);
                    }
                }).finally(() => {
                    updateCheckboxState();
                });
            } else {
                updateCheckboxState();
            }
            
        } else {
            $(".progress_department").removeClass("active");
            let activeDepartments = $("#DepartmentProgressReportFilter .progress_department.active");

            if (activeDepartments.length == 0) {
                $("#LocationProgressReportFilter").html(``);
            }
        }
    });
}

function updateCheckboxState() {
    let allDepartmentActive = $(".progress_department").length === $(".progress_department.active").length;
    $("#DepartmentProgressReportingToggleAll").prop("checked", allDepartmentActive);

    let allLocationActive = $(".progress_location").length === $(".progress_location.active").length;
    $("#LocationProgressReportingToggleAll").prop("checked", allLocationActive);
}
function ClickActiveLocation() {
    $(document).delegate("#LocationProgressReportFilter .spanBtn", "click", (e) => {
        let target = e.target;
        let isActive = $(target).hasClass("active");
        if (isActive) {
            $(target).removeClass("active");
        } else {
            $(target).removeClass("active").addClass("active");
        }
        updateCheckboxState();
    })

    //Click chọn tất cả khu vực trong bộ lọc:
    $("#LocationProgressReportingToggleAll").change(function () {
        if ($(this).is(":checked")) {
            $(".progress_location").addClass("active");
        } else {
            $(".progress_location").removeClass("active");
        }
    });

}

function RenderChartDepartment(isLocation, list_departments, list_locations, CaptureTimeType, inventoryId) {
    //Call API Bao Cao Tien Do Theo Loai Phieu:
    let InventoryId = (typeof inventoryId === 'undefined') ? $("#ReportingAudit_InventoryName").val() : inventoryId;
    let captureTimeType = (typeof CaptureTimeType === 'undefined') ? $("#reportType").val() : CaptureTimeType.toString();
    let departments = (typeof list_departments === 'undefined') ? [] : list_departments;
    let locations = (typeof list_locations === 'undefined') ? [] : list_locations;
    let DocType = [];
    let checkIsLocation = (typeof isLocation === 'undefined') ? false : isLocation;

    let filterData = {
        InventoryId: InventoryId,
        CaptureTimeType: captureTimeType,
        DocTypes: DocType,
        Departments: departments,
        Locations: locations
    };


    GetProgressReportAPI(filterData).then(res => {
        let departmentArray = [];
        let percentTotalTodo = [];
        let percentTotalInventory = [];
        let percentTotalConfirm = [];
        let totalTodo = [];
        let totalInventory = [];
        let totalConfirm = [];

        if (checkIsLocation == false) {
            if (res?.data?.progressReportDepartments.length > 0) {

                $(".Container-DepartmentLocation").show();
                $(".text-error-department-report").prop("hidden", true);

                $(".exportArea #exportFile").show();

                //Danh sach phong ban:
                res.data.progressReportDepartments.forEach(item => {
                    departmentArray.push(item.department);
                    totalConfirm.push(item.totalConfirm);
                    totalInventory.push(item.totalInventory);
                    totalTodo.push(item.totalTodo);

                    percentTotalConfirm.push(parseFloat((item.totalConfirm / item.totalDoc) * 100).toFixed(2));
                    percentTotalInventory.push(parseFloat((item.totalInventory / item.totalDoc) * 100).toFixed(2));
                    percentTotalTodo.push(parseFloat((item.totalTodo / item.totalDoc) * 100).toFixed(2));
                });

                //Render table
                let subHeader = ``
                let layoutHeaders = res.data.progressReportDepartments.map(item => {
                    subHeader += `<th>SL</th><th>${window.languageData[window.currentLanguage]["Tỉ lệ"]}</th>`
                    return `<th colspan="2">${item.department} </th>`;
                })

                let tableHeader = `<thead><tr><th rowspan="2">${window.languageData[window.currentLanguage]["Hạng mục"]}</th>${layoutHeaders}</tr>
                                <tr>${subHeader}</tr></thead>`


                //Tổng phiếu:
                let textTotalDoc = `<td>${window.languageData[window.currentLanguage]["Tổng phiếu"]}</td>`;
                res.data.progressReportDepartments.map(item => {
                    let tongPhieu = +(parseFloat((item.totalDoc / item.totalDoc) * 100)).toFixed(2);
                    let tongPhieu_GT = tongPhieu;
                    if (tongPhieu == 100.00) {
                        tongPhieu_GT = 100
                    } else if (tongPhieu == 0.00) {
                        tongPhieu_GT = 0
                    }

                    textTotalDoc += `<td>${convertDouble(item.totalDoc)}</td>
                               <td>${tongPhieu_GT}%</td>`
                })

                //Số phiếu chưa kiểm kê:
                let textTotalTodo = `<td>${window.languageData[window.currentLanguage]["Số phiếu chưa kiểm kê"]}</td>`;
                res.data.progressReportDepartments.map(item => {
                    let tongPhieu = +(parseFloat((item.totalTodo / item.totalDoc) * 100)).toFixed(2);
                    textTotalTodo += `<td>${convertDouble(item.totalTodo)}</td>
                               <td>${tongPhieu}%</td>`
                })


                //Số phiếu đã kiểm kê:
                let textTotalInventory = `<td>${window.languageData[window.currentLanguage]["Số phiếu đã kiểm kê"]}</td>`;
                res.data.progressReportDepartments.map(item => {
                    let tongPhieu = +(parseFloat((item.totalInventory / item.totalDoc) * 100)).toFixed(2);
                    textTotalInventory += `<td>${convertDouble(item.totalInventory)}</td>
                               <td>${tongPhieu}%</td>`
                })

                //Số phiếu đã xác nhận
                let textTotalConfirm = `<td>${window.languageData[window.currentLanguage]["Số phiếu đã xác nhận"]}</td>`;
                res.data.progressReportDepartments.map(item => {
                    let tongPhieu = +(parseFloat((item.totalConfirm / item.totalDoc) * 100)).toFixed(2);
                    
                    textTotalConfirm += `<td>${convertDouble(item.totalConfirm)}</td>
                               <td>${tongPhieu}%</td>`
                })

                let tableBody = `<tbody>
                                <tr>${textTotalDoc}</tr>
                                <tr>${textTotalTodo}</tr>
                                <tr>${textTotalInventory}</tr>
                                <tr>${textTotalConfirm}</tr>
                            </tbody>`
                $("#InventoryReporting3_DataTable").html(tableHeader + tableBody);

            }
            else {
                $(".Container-DepartmentLocation").hide();
                $(".text-error-department-report").prop("hidden", false);

                $(".exportArea #exportFile").hide();
            }
        }
        else {
            if (res?.data?.progressReportLocations.length > 0) {
                $(".Container-DepartmentLocation").show();
                $(".text-error-department-report").prop("hidden", true);

                $(".exportArea #exportFile").show();

                //Danh sach phong ban:
                res.data.progressReportLocations.forEach(item => {
                    departmentArray.push(item.location);
                    totalConfirm.push(item.totalConfirm);
                    totalInventory.push(item.totalInventory);
                    totalTodo.push(item.totalTodo);

                    percentTotalConfirm.push(parseFloat((item.totalConfirm / item.totalDoc) * 100).toFixed(2));
                    percentTotalInventory.push(parseFloat((item.totalInventory / item.totalDoc) * 100).toFixed(2));
                    percentTotalTodo.push(parseFloat((item.totalTodo / item.totalDoc) * 100).toFixed(2));
                });

                //Render table
                let subHeader = ``
                let layoutHeaders = res.data.progressReportLocations.map(item => {
                    subHeader += `<th>SL</th><th>${window.languageData[window.currentLanguage]["Tỉ lệ"]}</th>`
                    return `<th colspan="2">${item.location} </th>`;
                })

                let tableHeader = `<thead><tr><th rowspan="2">${window.languageData[window.currentLanguage]["Hạng mục"]}</th>${layoutHeaders}</tr>
                                <tr>${subHeader}</tr></thead>`


                //Tổng phiếu
                let textTotalDoc = `<td>${window.languageData[window.currentLanguage]["Tổng phiếu"]}</td>`;
                res.data.progressReportLocations.map(item => {
                    let tongPhieu = parseFloat((item.totalDoc / item.totalDoc) * 100).toFixed(2);
                    let tongPhieu_GT = tongPhieu;
                    if (tongPhieu == 100.00) {
                        tongPhieu_GT = 100
                    } else if (tongPhieu == 0.00) {
                        tongPhieu_GT = 0
                    }
                    textTotalDoc += `<td>${item.totalDoc}</td>
                               <td>${tongPhieu_GT}%</td>`
                })

                //Số phiếu chưa kiểm kê:
                let textTotalTodo = `<td>${window.languageData[window.currentLanguage]["Số phiếu chưa kiểm kê"]}</td>`;
                res.data.progressReportLocations.map(item => {
                    let tongPhieu = parseFloat((item.totalTodo / item.totalDoc) * 100).toFixed(2);
                    let tongPhieu_GT = tongPhieu;
                    if (tongPhieu == 100.00) {
                        tongPhieu_GT = 100
                    } else if (tongPhieu == 0.00) {
                        tongPhieu_GT = 0
                    }
                    textTotalTodo += `<td>${item.totalTodo}</td>
                               <td>${tongPhieu_GT}%</td>`
                })


                //Số phiếu đã kiểm kê:
                let textTotalInventory = `<td>${window.languageData[window.currentLanguage]["Số phiếu đã kiểm kê"]}</td>`;
                res.data.progressReportLocations.map(item => {
                    let tongPhieu = parseFloat((item.totalInventory / item.totalDoc) * 100).toFixed(2);
                    let tongPhieu_GT = tongPhieu;
                    if (tongPhieu == 100.00) {
                        tongPhieu_GT = 100
                    } else if (tongPhieu == 0.00) {
                        tongPhieu_GT = 0
                    }
                    textTotalInventory += `<td>${item.totalInventory}</td>
                               <td>${tongPhieu_GT}%</td>`
                })

                //Số phiếu đã xác nhận
                let textTotalConfirm = `<td>${window.languageData[window.currentLanguage]["Số phiếu đã xác nhận"]}</td>`;
                res.data.progressReportLocations.map(item => {
                    let tongPhieu = parseFloat((item.totalConfirm / item.totalDoc) * 100).toFixed(2);
                    let tongPhieu_GT = tongPhieu;
                    if (tongPhieu == 100.00) {
                        tongPhieu_GT = 100
                    } else if (tongPhieu == 0.00) {
                        tongPhieu_GT = 0
                    }
                    textTotalConfirm += `<td>${item.totalConfirm}</td>
                               <td>${tongPhieu_GT}%</td>`
                })

                let tableBody = `<tbody>
                                <tr>${textTotalDoc}</tr>
                                <tr>${textTotalTodo}</tr>
                                <tr>${textTotalInventory}</tr>
                                <tr>${textTotalConfirm}</tr>
                            </tbody>`
                $("#InventoryReporting3_DataTable").html(tableHeader + tableBody);

            }
            else {
                $(".Container-DepartmentLocation").hide();
                $(".text-error-department-report").prop("hidden", false);

                $(".exportArea #exportFile").hide();
            }
        }
        //Render ra bieu do chart:
        // create new element
        const newCtx = document.createElement("canvas");
        newCtx.id = "myChartReporting";

        let delayed;

        const zoomOptions = {
            pan: {
                enabled: true,
                mode: 'x',
                speed: 0.1,
            },
            zoom: {
                wheel: {
                    enabled: true
                },
                pinch: {
                    enabled: true,
                    speed: 0.1,
                },
                mode: 'x',
                speed: 0.1,
            },
        };


        window.departmentReportChart = new Chart(newCtx, {
            type: "bar",
            data: {
                labels: departmentArray,
                datasets: [
                    {
                        label: window.languageData[window.currentLanguage]["Tỉ lệ chưa kiểm kê"],
                        backgroundColor: "#50555C",
                        data: percentTotalTodo, // phần trăm
                        borderColor: "#50555C",
                        pointBorderColor: "#FFFFFF",
                        borderWidth: 1.5,
                        type: "line",
                        pointRadius: 5,
                        pointHoverRadius: 10,
                        yAxisID: "y1",
                    },
                    {
                        label: window.languageData[window.currentLanguage]["Tỉ lệ đã xác nhận"],
                        backgroundColor: "#009543",
                        data: percentTotalConfirm, // phần trăm
                        borderColor: "#009543",
                        pointBorderColor: "#FFFFFF",
                        borderWidth: 1.5,
                        type: "line",
                        pointRadius: 5,
                        pointHoverRadius: 10,
                        yAxisID: "y1",
                    },
                    {
                        label: window.languageData[window.currentLanguage]["Tỉ lệ đã kiểm kê"],
                        backgroundColor: "#DA8D00",
                        data: percentTotalInventory, // phần trăm
                        borderColor: "#DA8D00",
                        pointBorderColor: "#FFFFFF",
                        borderWidth: 1.5,
                        type: "line",
                        pointRadius: 5,
                        pointHoverRadius: 10,
                        yAxisID: "y1",
                    },
                    {
                        label: window.languageData[window.currentLanguage]["Đã xác nhận"],
                        backgroundColor: "#009543",
                        data: totalConfirm,
                        type: "bar",
                        barThickness: 50,
                    },
                    {
                        label: window.languageData[window.currentLanguage]["Đã kiểm kê"],
                        backgroundColor: "#DA8D00",
                        data: totalInventory,
                        type: "bar",
                        barThickness: 50,
                    },
                    {
                        label: window.languageData[window.currentLanguage]["Chưa kiểm kê"],
                        backgroundColor: "#C2C2C2",
                        data: totalTodo,
                        type: "bar",
                        barThickness: 50,
                    },
                ],
            },
            options: {
                onClick: (e, activeEls) => {
                    //let datasetIndex = activeEls[0].datasetIndex;
                    let dataIndex = activeEls[0].index;
                    //let datasetLabel = e.chart.data.datasets[datasetIndex].label;
                    //let value = e.chart.data.datasets[datasetIndex].data[dataIndex];
                    let label = e.chart.data.labels[dataIndex];

                    //$(`#DepartmentProgressReportFilter .progress_department`).removeClass("active");
                    //$(`#DepartmentProgressReportFilter .progress_department[name="${label}"]`).trigger("click");
                    let isLocation = true;
                    let list_departments = [];
                    let list_locations = [];
                    var CaptureTimeType = $("#reportType").val();

                    let list_locations1 = [];
                    let list_locations2 = [];

                    list_departments.push(label);

                    let filterData = {
                        Departments: list_departments
                    }
                    $("#LocationProgressReportFilter .progress_location.active").each(function () {
                        let locationValue = $(this).attr('name');
                        list_locations2.push(locationValue);
                    });

                    GetLocationAPI(filterData).then(res => {
                        res.data.map(item => {
                            list_locations1.push(item.locationName);
                        })
                        if (list_locations2.length > 0) {

                            for (var i = 0; i < list_locations1.length; i++) {
                                if (list_locations2.indexOf(list_locations1[i]) !== -1) {
                                    list_locations.push(list_locations1[i]);
                                }
                            }
                            RenderChartDepartment(isLocation, list_departments, list_locations, CaptureTimeType)
                        } else {
                            if (list_locations1.length > 0) {
                                RenderChartDepartment(isLocation, list_departments, list_locations1, CaptureTimeType)
                            }
                        }
                    })


                },
                // set style for label
                plugins: {
                    legend: false,
                    zoom: zoomOptions
                },
                animation: {
                    duration: 0,
                    onComplete: () => {
                        delayed = true;
                    },
                    delay: (context) => {
                        let delay = 0;
                        if (
                            context.type === "data" &&
                            context.mode === "default" &&
                            !delayed
                        ) {
                            delay = context.dataIndex * 300 + context.datasetIndex * 100;
                        }
                        return delay;
                    },
                },
                scales: {
                    x: {
                        stacked: true,
                        grid: {
                            display: false,
                        },
                        ticks: {
                            font: {
                                weight: "600",
                                color: "#333333",
                                size: 14,
                            },
                            padding: 35,
                        },
                    },
                    y: {
                        stacked: true,
                        grid: {
                            display: false,
                        },
                        ticks: {
                            font: {
                                weight: "400",
                                color: "#333333",
                                size: 14,
                            },
                        },
                    },
                    // y1 is percents
                    y1: {
                        position: "right",
                        min: 0,
                        max: 100,
                        ticks: {
                            callback: function (value) {
                                return value + "%";
                            },
                            font: {
                                weight: "400",
                                color: "#333333",
                                size: 14,
                            },
                        },
                    },
                },
                responsive: true
            },
        });

        // replace canvas with newCtx
        $("#myChartReporting").replaceWith(newCtx);


    }).catch(() => {
        $(".Container-DepartmentLocation").hide();
        $(".text-error-department-report").prop("hidden", false);

        $(".exportArea #exportFile").hide();
    })
}

//Hàm hiển thị số thập phân:
function convertDouble(str) {
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