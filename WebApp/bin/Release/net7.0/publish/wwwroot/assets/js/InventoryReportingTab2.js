$(function () {
    AuditReportingHandler.init();
});

; var AuditReportingHandler = (function () {
    let root = {
        parentEl: $("#supervision-report-tab"),
        events: {
            openAuditSettings: "openAuditSettings",
            departmentChanged: "departmentChanged",
            locationChanged: "locationChanged",
            auditorChanged: "auditorChanged"
        },
    };

    let AuditReportType = {
        Department: 0,
        Location: 1,
        Auditor: 2
    }

    let filterModel = {
        inventoryId: App?.User?.InventoryLoggedInfo?.InventoryModel?.InventoryId,
        //isCheckAllDepartment: "-1",
        //isCheckAllLocation: "-1"
        locations: []
    }
    let loadAuditReportTimer;

    const APIs = {
        AuditReportingAPI: async function (model) {
            return new Promise(async (resolve, reject) => {
                let url = `${App.ApiGateWayUrl}/api/inventory/report/audits`;

                $.ajax({
                    type: 'POST',
                    url: url,
                    contentType: 'application/json',
                    data: JSON.stringify(model),
                    success: function (response) {
                        resolve(response)

                        if (response.data?.progressReportLocations?.length == 0) {
                            $(window).trigger(GlobalEventName.reporting_audit_notfound);
                        }
                    },
                    error: function (error) {
                        reject(error)

                        //Kích hoạt sự kiện khi không tìm thấy dữ liệu
                        $(window).trigger(GlobalEventName.reporting_audit_notfound);
                    },
                });
            })
        },
        GetDepartmentsAPI: function () {
            return new Promise(async (resolve, reject) => {
                let url = `${App.ApiGateWayUrl}/api/inventory/location/departments`;

                $.ajax({
                    type: 'GET',
                    url: url,
                    contentType: 'application/json',
                    success: function (response) {
                        resolve(response)
                    },
                    error: function (error) {
                        reject(error)
                    },
                });
            })
        },
        GetLocationsAPI: async function (filterData) {
            return new Promise(async (resolve, reject) => {
                $.ajax({
                    type: "POST",
                    url: `${App.ApiGateWayUrl}/api/inventory/location/departmentname`,
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: JSON.stringify(filterData),
                    success: function (res) {
                        resolve(res);
                    },
                    error: function (err) {
                        reject(err)

                        //Kích hoạt sự kiện khi không tìm thấy dữ liệu
                        $(window).trigger(GlobalEventName.reporting_audit_notfound);
                    }
                });
            })
        },
        GetAuditorsAPI: async function (filterData) {
            return new Promise(async (resolve, reject) => {
                $.ajax({
                    type: "POST",
                    url: `${App.ApiGateWayUrl}/api/inventory/location/auditor`,
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: JSON.stringify(filterData),
                    success: function (res) {
                        resolve(res);
                    },
                    error: function (err) {
                        reject(err)

                        //Kích hoạt sự kiện khi không tìm thấy dữ liệu
                        $(window).trigger(GlobalEventName.reporting_audit_notfound);
                    }
                });
            })
        }
    }

    function Cache() {
        
    }
    function PreLoad() {
    }

    // Hàm chuyển đổi YYYYMMDD thành Date
    function convertToDate(yyyymmdd) {
        let year = parseInt(yyyymmdd.substring(0, 4), 10);
        let month = parseInt(yyyymmdd.substring(4, 6), 10) - 1;
        let day = parseInt(yyyymmdd.substring(6, 8), 10);
        return new Date(year, month, day);
    }

    // Lấy giá trị ngày từ select
    function shouldCallAPI() {
        let selectedDateStr = $("#ReportingAudit_InventoryName option:selected").text().trim();
        let selectedDate = convertToDate(selectedDateStr);
        let today = new Date();
        console.log(today <= selectedDate);
        return today <= selectedDate; // Nếu ngày hiện tại <= selectedDate thì tiếp tục gọi API
    }

    function Events() {
        //Kích hoạt sự kiện khi không tìm thấy dữ liệu
        $(window).on(GlobalEventName.reporting_audit_notfound, function (e) {
            $("#exportFile").hide();
        });

        $(window).on("report.tab.changed", function (e, data) {
            let tabTarget = data;

            let isActiveAuditTab = $(tabTarget).is("#supervision") && $(tabTarget).hasClass("active");

            if ($(tabTarget).is("#supervision")) {
                $("#reportType").hide();
                $("#exportFile").show();
                //$("#ReportingAudit_InventoryName").show();
                $("#ReportingAudit_Type").show();
            } else {
                $("#reportType").show();
                $("#exportFile").show();
                //$("#ReportingAudit_InventoryName").hide();
                $("#ReportingAudit_Type").hide();
            }

            //Nếu tab báo cáo giám sát đang active
            if (!isActiveAuditTab) {
                //Xóa tự động gọi API biểu đồ nếu không ở màn hình active
                if (loadAuditReportTimer) {
                    window.clearInterval(loadAuditReportTimer);
                }
            }

            //Start timer loading audit report
            if (loadAuditReportTimer) {
                window.clearInterval(loadAuditReportTimer);
            }

            //15s cập nhật lại chart một lần
            let failCount = 0;
            loadAuditReportTimer = window.setInterval(() => {
                // Nếu ngày hiện tại lớn hơn selectedDate => Dừng gọi API
                if (!shouldCallAPI()) {
                    console.log("Ngày hiện tại đã vượt quá ngày giới hạn, dừng gọi API.");
                    window.clearInterval(loadAuditReportTimer);
                    return;
                }

                //Nếu quá 5 lần không tìm thấy thì dừng tự động
                if (failCount == 5) {
                    window.clearInterval(loadAuditReportTimer);
                }

                //Nếu có dữ liệu reset lại failCount
                failCount = 0;

                let isDepartment = false;
                let isLocation = false;
                let isAuditor = false;

                $('.audit_department.spanBtn').each(function () {
                    if ($(this).hasClass('active')) {
                        isDepartment = true;
                        return false;
                    }
                });

                $('.audit_location.spanBtn').each(function () {
                    if ($(this).hasClass('active')) {
                        isLocation = true;
                        return false;
                    }
                });

                $('.audit_auditor.spanBtn').each(function () {
                    if ($(this).hasClass('active')) {
                        isAuditor = true;
                        return false;
                    }
                });

                //Chỉ lọc theo phòng ban hoặc không lọc theo điều kiện => mặc định là báo cáo theo phòng ban:
                if ((isDepartment || !isDepartment) && !isLocation && !isAuditor) {
                    let listDepartments = [];
                    let activeDepartments = root.parentEl.find("#pbList .audit_department.active");

                    activeDepartments.each(function () {
                        var value = $(this).attr('name');
                        listDepartments.push(value);
                    });

                    let departmentAuditReportModel = {
                        InventoryId: $("#ReportingAudit_InventoryName").val(),
                        AuditReportType: AuditReportType.Department,
                        Departments: listDepartments,
                        Locations: [],
                        Auditors: [],
                        ReportingAuditTypes: document.querySelector('#ReportingAudit_Type').isAllSelected() ? [] : $("#ReportingAudit_Type").val().map(Number)
                    };

                    APIs.AuditReportingAPI(departmentAuditReportModel).then(res => {
                        RenderChart(res.data)
                    }).catch(err => {
                        toastr.error(err?.responseJSON?.message || window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phù hợp"])
                        $(".chart_data_container").hide();
                        $(".chart_data_empty").show();
                        failCount++;
                    }).finally(() => {

                        let panel = $("#panelReporting2");
                        panel.removeClass("d-none").addClass("d-none");
                    })
                }
                else if (isDepartment && isLocation && !isAuditor) {
                    //Lọc theo phòng ban và khu vực => báo cáo theo khu vực
                    let listLocations = [];
                    let activeLocations = root.parentEl.find("#kvList .audit_location.active");

                    activeLocations.each(function () {
                        var value = $(this).attr('name');
                        listLocations.push(value);
                    });

                    let locationAuditReportModel = {
                        InventoryId: $("#ReportingAudit_InventoryName").val(),
                        AuditReportType: AuditReportType.Location,
                        Departments: [],
                        Locations: listLocations,
                        Auditors: [],
                        ReportingAuditTypes: document.querySelector('#ReportingAudit_Type').isAllSelected() ? [] : $("#ReportingAudit_Type").val().map(Number)
                    };

                    APIs.AuditReportingAPI(locationAuditReportModel).then(res => {
                        RenderChart(res.data)
                    }).catch(err => {
                        toastr.error(err?.responseJSON?.message || window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phù hợp"])
                        $(".chart_data_container").hide();
                        $(".chart_data_empty").show();
                    }).finally(() => {

                        let panel = $("#panelReporting2");
                        panel.removeClass("d-none").addClass("d-none");
                    })
                }
                else if (isDepartment && isLocation && isAuditor) {
                    //Lọc theo phòng ban, khu vực và người giám sát => báo cáo theo người giám sát
                    let listAuditors = [];
                    let activeAuditors = root.parentEl.find("#gsList .audit_auditor.active");

                    activeAuditors.each(function () {
                        var value = $(this).attr('name');
                        listAuditors.push(value);
                    });

                    let auditorAuditReportModel = {
                        InventoryId: $("#ReportingAudit_InventoryName").val(),
                        AuditReportType: AuditReportType.Auditor,
                        Departments: [],
                        Locations: [],
                        Auditors: listAuditors,
                        ReportingAuditTypes: document.querySelector('#ReportingAudit_Type').isAllSelected() ? [] : $("#ReportingAudit_Type").val().map(Number)
                    };

                    APIs.AuditReportingAPI(auditorAuditReportModel).then(res => {
                        RenderChart(res.data)
                    }).catch(err => {
                        toastr.error(err?.responseJSON?.message || window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phù hợp"])
                        $(".chart_data_container").hide();
                        $(".chart_data_empty").show();
                    }).finally(() => {

                        let panel = $("#panelReporting2");
                        panel.removeClass("d-none").addClass("d-none");
                    })
                }
                
            }, AuditReportInterval);
        })

        $(window).on("report.tab.changed", async function (e, data) {
            let tabTarget = data;
            let isActiveAuditTab = $(tabTarget).is("#supervision") && $(tabTarget).hasClass("active");

            let filterData = {
                Departments: [],
                Locations: []
            }

            //Nếu tab active không phải giám sát thì exit
            if (!isActiveAuditTab) return;

            //Load API phòng ban
            await APIs.GetDepartmentsAPI().then(res => {
                let data = res.data;


                if (data.length == 0) {
                    root.parentEl.find("#pbList").html(window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phòng ban"]);
                }

                if (data.length) {
                    let resultHTML = data.map(item => {
                        return `<span class="audit_department spanBtn active" name="${item.departmentName}">${item.departmentName}</span>`
                    });

                    root.parentEl.find("#pbList").html(resultHTML);
                    root.parentEl.find("#kvList").html(window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu khu vực"]);
                    root.parentEl.find("#gsList").html(window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu người giám sát"]);
                }
            });

            //Load lần đầu
            //root.parentEl.find("#pbList").find(".audit_department").removeClass("active").addClass("active");
            let activeDepartments = root.parentEl.find("#pbList .audit_department.active");
            if (activeDepartments.length == 0) {
                root.parentEl.find("#kvList").html(window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu khu vực"]);
            }
            let departmentNames = [...activeDepartments].map(item => {
                return $(item).attr("name")
            })

            //Cập nhật filter danh sách phòng ban
            filterData.Departments = departmentNames

            //Gọi API lấy danh sách khu vực theo danh sách filter vừa câp nhật
            APIs.GetLocationsAPI(filterData).then((res) => {
                let html = res?.data?.map((item) => {
                    return `<span class="audit_location spanBtn" name="${item.locationName}">${item.locationName}</span>`
                })

                root.parentEl.find("#kvList").html(html);
                //Tích hết các khu vực
                //root.parentEl.find("#kvList").find(".audit_location").removeClass("active").addClass("active");

                //Cập nhật filter khu vực
                filterData.Locations = res.data.map(item => item.locationName);
            });


            //Cập nhật id đợt kiểm kê:
            var listDepartments = [];
            activeDepartments.each(function () {
                var value = $(this).attr('name');
                listDepartments.push(value);
            });

            let auditReportModel = {
                InventoryId: $("#ReportingAudit_InventoryName").val(),
                AuditReportType: AuditReportType.Department,
                Departments: listDepartments,
                Locations: [],
                Auditors: [],
                ReportingAuditTypes: document.querySelector('#ReportingAudit_Type').isAllSelected() ? [] : $("#ReportingAudit_Type").val().map(Number)
            }

            APIs.AuditReportingAPI(auditReportModel).then(res => {
                RenderChart(res.data)
            }).catch(err => {
                toastr.error(err?.responseJSON?.message || "");

                $(".chart_data_container").hide();
                $(".chart_data_empty").show();
            }).finally(() => {
            })

            //loading(true);
            //Chạy nền
        })


        $(window).on(root.events.departmentChanged, async function (e, data) {
            //Nếu là tích chọn phòng ban
            let activeDepartments = root.parentEl.find("#pbList .audit_department.active");

            if (activeDepartments.length == 0) {
                root.parentEl.find("#kvList").html(window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu khu vực"]);
            }
            if (activeDepartments.length) {
                let departmentNames = [...activeDepartments].map(item => {
                    return $(item).attr("name")
                })

                let filterData = {
                    Departments: departmentNames
                }

                let res = await APIs.GetLocationsAPI(filterData);
                if (res?.data?.length) {
                    let html = res.data.map((item) => {
                        return `<span class="audit_location spanBtn" name="${item.locationName}">${item.locationName}</span>`
                    })

                    root.parentEl.find("#kvList").html(html);
                    root.parentEl.find("#gsList").html(window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu người giám sát"]);
                }
            }
        });

        $(window).on(root.events.auditorChanged, async function (e, data) {
            //Nếu là tích chọn phòng ban
            let activeLocations = root.parentEl.find("#kvList .audit_location.active");

            if (activeLocations.length == 0) {
                root.parentEl.find("#gsList").html(window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu người giám sát"]);
            }
            if (activeLocations.length) {
                let locationNames = [...activeLocations].map(item => {
                    return $(item).attr("name")
                })

                let filterData = {
                    Locations: locationNames
                }

                let res = await APIs.GetAuditorsAPI(filterData);
                if (res?.data?.length) {
                    let html = res.data.map((item) => {
                        return `<span class="audit_auditor spanBtn" name="${item.auditorName}">${item.auditorName}</span>`
                    })

                    root.parentEl.find("#gsList").html(html);
                }
            }
        });

        root.parentEl.find("#FixedAuditReporting .hideDetailTable").click(function (e) {


            root.parentEl.find("#Fixed_InventoryReporting2_DataTable").toggle();

            let isTableVisible = root.parentEl.find("#Fixed_InventoryReporting2_DataTable").is(":visible");
            let buttonText = isTableVisible ? window.languageData[window.currentLanguage]["Ẩn chi tiết bảng"] : window.languageData[window.currentLanguage]["Hiển thị chi tiết bảng"];
            $(this).text(buttonText);
        })

        root.parentEl.find("#FreeAuditReporting .hideDetailTable").click(function (e) {


            root.parentEl.find("#Free_InventoryReporting2_DataTable").toggle();

            let isTableVisible = root.parentEl.find("#Free_InventoryReporting2_DataTable").is(":visible");
            let buttonText = isTableVisible ? window.languageData[window.currentLanguage]["Ẩn chi tiết bảng"] : window.languageData[window.currentLanguage]["Hiển thị chi tiết bảng"];
            $(this).text(buttonText);
        })

        root.parentEl.delegate(".spanBtn", "click", function (e) {
            let target = e.target;
            let isActive = $(target).hasClass("active");
            if (isActive) {
                $(target).removeClass("active");
            } else {
                $(target).removeClass("active").addClass("active");
            }

            //Publish department changed
            if ($(target).is(".audit_department")) {
                $(window).trigger(root.events.departmentChanged, { target });
            }

            //Kích hoạt sự kiện khu vực thay đổi
            if ($(target).is(".audit_location")) {
                $(window).trigger(root.events.locationChanged, { target });
                $(window).trigger(root.events.auditorChanged, { target });
            }

            updateCheckboxState();
        })
        
        function updateCheckboxState() {
            let allDepartmentActive = $(".audit_department").length === $(".audit_department.active").length;
            $("#DepartmentAuditReportingToggleAll").prop("checked", allDepartmentActive);

            let allLocationActive = $(".audit_location").length === $(".audit_location.active").length;
            $("#LocationAuditReportingToggleAll").prop("checked", allLocationActive);

            let allAuditorActive = $(".audit_auditor").length === $(".audit_auditor.active").length;
            $("#AuditorAuditReportingToggleAll").prop("checked", allAuditorActive);
        }

        //Click chọn tất cả phòng ban trong bộ lọc:
        $("#DepartmentAuditReportingToggleAll").change(function () {
            if ($(this).is(":checked")) {
                $(".audit_department").addClass("active");
            } else {
                $(".audit_department").removeClass("active");
            }
        });

        //Click chọn tất cả khu vực trong bộ lọc:
        $("#LocationAuditReportingToggleAll").change(async function () {
            if ($(this).is(":checked")) {
                $(".audit_location").addClass("active");

                //Nếu là tích chọn phòng ban
                let activeLocations = root.parentEl.find("#kvList .audit_location.active");

                if (activeLocations.length == 0) {
                    root.parentEl.find("#gsList").html(window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu người giám sát"]);
                }
                if (activeLocations.length) {
                    let locationNames = [...activeLocations].map(item => {
                        return $(item).attr("name")
                    })

                    let filterData = {
                        Locations: locationNames
                    }

                    let res = await APIs.GetAuditorsAPI(filterData);
                    if (res?.data?.length) {
                        let html = res.data.map((item) => {
                            return `<span class="audit_auditor spanBtn" name="${item.auditorName}">${item.auditorName}</span>`
                        })

                        root.parentEl.find("#gsList").html(html);
                    }
                }


            } else {
                $(".audit_location").removeClass("active");

                //Nếu là tích chọn phòng ban
                let activeLocations = root.parentEl.find("#kvList .audit_location.active");

                if (activeLocations.length == 0) {
                    root.parentEl.find("#gsList").html(window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu người giám sát"]);
                }
            }
        });

        //Click chọn tất cả người giám sát trong bộ lọc:
        $("#AuditorAuditReportingToggleAll").change(function () {
            if ($(this).is(":checked")) {
                $(".audit_auditor").addClass("active");
            } else {
                $(".audit_auditor").removeClass("active");
            }
        });


        $("#filterReporting2").click((e) => {
            let panel = $("#panelReporting2");

            if (panel.hasClass("d-none")) {
                panel.removeClass("d-none");

                //$(window).trigger(root.events.openAuditSettings, panel)
            } else {
                panel.removeClass("d-none").addClass("d-none");
            }
        });

        //Thay đổi đợt kiểm kê, sẽ call lại api lấy ra danh sách báo cáo giám sát:
        $("#ReportingAudit_InventoryName").on("change", function () {
            let isAuditActiveTab = $("#supervision").hasClass("active");
            if (!isAuditActiveTab) {
                return;
            }
            let selectedInventoryId = $(this).val(); 
            ChangeInventoryCallAuditReportingAPI(selectedInventoryId);
        });

        //Thay đổi loại báo cáo giám sát, sẽ call API lấy ra danh sách báo cáo giám sát:
        let auditTypes = [];
        // Khi chọn nhiều option
        $("#ReportingAudit_Type").on("change", function () {
            auditTypes = $(this).val().map(Number); 
        });

        // Khi dropdown đóng lại thì gọi API
        $("#ReportingAudit_Type").on("focusout", function (event) {
            ChangeReportTypeCallAuditReportingAPI();
        });

        function ChangeReportTypeCallAuditReportingAPI() {
            let isDepartment = false;
            let isLocation = false;
            let isAuditor = false;

            $('.audit_department.spanBtn').each(function () {
                if ($(this).hasClass('active')) {
                    isDepartment = true;
                    return false;
                }
            });

            $('.audit_location.spanBtn').each(function () {
                if ($(this).hasClass('active')) {
                    isLocation = true;
                    return false;
                }
            });

            $('.audit_auditor.spanBtn').each(function () {
                if ($(this).hasClass('active')) {
                    isAuditor = true;
                    return false;
                }
            });

            //Chỉ lọc theo phòng ban hoặc không lọc theo điều kiện => mặc định là báo cáo theo phòng ban:
            if ((isDepartment || !isDepartment) && !isLocation && !isAuditor) {
                let listDepartments = [];
                let activeDepartments = root.parentEl.find("#pbList .audit_department.active");

                activeDepartments.each(function () {
                    var value = $(this).attr('name');
                    listDepartments.push(value);
                });

                let departmentAuditReportModel = {
                    InventoryId: $("#ReportingAudit_InventoryName").val(),
                    AuditReportType: AuditReportType.Department,
                    Departments: listDepartments,
                    Locations: [],
                    Auditors: [],
                    ReportingAuditTypes: auditTypes
                };

                APIs.AuditReportingAPI(departmentAuditReportModel).then(res => {
                    RenderChart(res.data)
                }).catch(err => {
                    toastr.error(err?.responseJSON?.message || window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phù hợp"])
                    $(".chart_data_container").hide();
                    $(".chart_data_empty").show();
                }).finally(() => {

                    let panel = $("#panelReporting2");
                    panel.removeClass("d-none").addClass("d-none");
                })
            }
            else if (isDepartment && isLocation && !isAuditor) {
                //Lọc theo phòng ban và khu vực => báo cáo theo khu vực
                let listLocations = [];
                let activeLocations = root.parentEl.find("#kvList .audit_location.active");

                activeLocations.each(function () {
                    var value = $(this).attr('name');
                    listLocations.push(value);
                });

                let locationAuditReportModel = {
                    InventoryId: $("#ReportingAudit_InventoryName").val(),
                    AuditReportType: AuditReportType.Location,
                    Departments: [],
                    Locations: listLocations,
                    Auditors: [],
                    ReportingAuditTypes: auditTypes
                };

                APIs.AuditReportingAPI(locationAuditReportModel).then(res => {
                    RenderChart(res.data)
                }).catch(err => {
                    toastr.error(err?.responseJSON?.message || window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phù hợp"])
                    $(".chart_data_container").hide();
                    $(".chart_data_empty").show();
                }).finally(() => {

                    let panel = $("#panelReporting2");
                    panel.removeClass("d-none").addClass("d-none");
                })
            }
            else if (isDepartment && isLocation && isAuditor) {
                //Lọc theo phòng ban, khu vực và người giám sát => báo cáo theo người giám sát
                let listAuditors = [];
                let activeAuditors = root.parentEl.find("#gsList .audit_auditor.active");

                activeAuditors.each(function () {
                    var value = $(this).attr('name');
                    listAuditors.push(value);
                });

                let auditorAuditReportModel = {
                    InventoryId: $("#ReportingAudit_InventoryName").val(),
                    AuditReportType: AuditReportType.Auditor,
                    Departments: [],
                    Locations: [],
                    Auditors: listAuditors,
                    ReportingAuditTypes: auditTypes
                };

                APIs.AuditReportingAPI(auditorAuditReportModel).then(res => {
                    RenderChart(res.data)
                }).catch(err => {
                    toastr.error(err?.responseJSON?.message || window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phù hợp"])
                    $(".chart_data_container").hide();
                    $(".chart_data_empty").show();
                }).finally(() => {

                    let panel = $("#panelReporting2");
                    panel.removeClass("d-none").addClass("d-none");
                })
            }
        }
        function ChangeInventoryCallAuditReportingAPI(selectedInventoryId) {
            let isDepartment = false;
            let isLocation = false;
            let isAuditor = false;

            $('.audit_department.spanBtn').each(function () {
                if ($(this).hasClass('active')) {
                    isDepartment = true;
                    return false;
                }
            });

            $('.audit_location.spanBtn').each(function () {
                if ($(this).hasClass('active')) {
                    isLocation = true;
                    return false;
                }
            });

            $('.audit_auditor.spanBtn').each(function () {
                if ($(this).hasClass('active')) {
                    isAuditor = true;
                    return false;
                }
            });

            //Chỉ lọc theo phòng ban hoặc không lọc theo điều kiện => mặc định là báo cáo theo phòng ban:
            if ((isDepartment || !isDepartment) && !isLocation && !isAuditor) {
                let listDepartments = [];
                let activeDepartments = root.parentEl.find("#pbList .audit_department.active");

                activeDepartments.each(function () {
                    var value = $(this).attr('name');
                    listDepartments.push(value);
                });

                let departmentAuditReportModel = {
                    InventoryId: selectedInventoryId,
                    AuditReportType: AuditReportType.Department,
                    Departments: listDepartments,
                    Locations: [],
                    Auditors: [],
                    ReportingAuditTypes: document.querySelector('#ReportingAudit_Type').isAllSelected() ? [] : $("#ReportingAudit_Type").val().map(Number)
                };

                APIs.AuditReportingAPI(departmentAuditReportModel).then(res => {
                    RenderChart(res.data)
                }).catch(err => {
                    toastr.error(err?.responseJSON?.message || window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phù hợp"])
                    $(".chart_data_container").hide();
                    $(".chart_data_empty").show();
                }).finally(() => {

                    let panel = $("#panelReporting2");
                    panel.removeClass("d-none").addClass("d-none");
                })
            }
            else if (isDepartment && isLocation && !isAuditor) {
                //Lọc theo phòng ban và khu vực => báo cáo theo khu vực
                let listLocations = [];
                let activeLocations = root.parentEl.find("#kvList .audit_location.active");

                activeLocations.each(function () {
                    var value = $(this).attr('name');
                    listLocations.push(value);
                });

                let locationAuditReportModel = {
                    InventoryId: selectedInventoryId,
                    AuditReportType: AuditReportType.Location,
                    Departments: [],
                    Locations: listLocations,
                    Auditors: [],
                    ReportingAuditTypes: document.querySelector('#ReportingAudit_Type').isAllSelected() ? [] : $("#ReportingAudit_Type").val().map(Number)
                };

                APIs.AuditReportingAPI(locationAuditReportModel).then(res => {
                    RenderChart(res.data)
                }).catch(err => {
                    toastr.error(err?.responseJSON?.message || window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phù hợp"])
                    $(".chart_data_container").hide();
                    $(".chart_data_empty").show();
                }).finally(() => {

                    let panel = $("#panelReporting2");
                    panel.removeClass("d-none").addClass("d-none");
                })
            }
            else if (isDepartment && isLocation && isAuditor) {
                //Lọc theo phòng ban, khu vực và người giám sát => báo cáo theo người giám sát
                let listAuditors = [];
                let activeAuditors = root.parentEl.find("#gsList .audit_auditor.active");

                activeAuditors.each(function () {
                    var value = $(this).attr('name');
                    listAuditors.push(value);
                });

                let auditorAuditReportModel = {
                    InventoryId: selectedInventoryId,
                    AuditReportType: AuditReportType.Auditor,
                    Departments: [],
                    Locations: [],
                    Auditors: listAuditors,
                    ReportingAuditTypes: document.querySelector('#ReportingAudit_Type').isAllSelected() ? [] : $("#ReportingAudit_Type").val().map(Number)
                };

                APIs.AuditReportingAPI(auditorAuditReportModel).then(res => {
                    RenderChart(res.data)
                }).catch(err => {
                    toastr.error(err?.responseJSON?.message || window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phù hợp"])
                    $(".chart_data_container").hide();
                    $(".chart_data_empty").show();
                }).finally(() => {

                    let panel = $("#panelReporting2");
                    panel.removeClass("d-none").addClass("d-none");
                })
            }
        }

        root.parentEl.find("#btnApplyFilterAuditReport").click(function (e) {
            var CaptureTimeType = $("#reportType").val();

            let isDepartment = false;
            let isLocation = false;
            let isAuditor = false;

            $('.audit_department.spanBtn').each(function () {
                if ($(this).hasClass('active')) {
                    isDepartment = true;
                    return false;
                }
            });

            $('.audit_location.spanBtn').each(function () {
                if ($(this).hasClass('active')) {
                    isLocation = true;
                    return false;
                }
            });

            $('.audit_auditor.spanBtn').each(function () {
                if ($(this).hasClass('active')) {
                    isAuditor = true;
                    return false;
                }
            });

            //Chỉ lọc theo phòng ban hoặc không lọc theo điều kiện => mặc định là báo cáo theo phòng ban:
            if ((isDepartment || !isDepartment) && !isLocation && !isAuditor) {
                let listDepartments = [];
                let activeDepartments = root.parentEl.find("#pbList .audit_department.active");

                activeDepartments.each(function () {
                    var value = $(this).attr('name');
                    listDepartments.push(value);
                });

                let departmentAuditReportModel = {
                    InventoryId: $("#ReportingAudit_InventoryName").val(),
                    AuditReportType: AuditReportType.Department,
                    Departments: listDepartments,
                    Locations: [],
                    Auditors: [],
                    ReportingAuditTypes: document.querySelector('#ReportingAudit_Type').isAllSelected() ? [] : $("#ReportingAudit_Type").val().map(Number)
                };

                APIs.AuditReportingAPI(departmentAuditReportModel).then(res => {
                    RenderChart(res.data)
                }).catch(err => {
                    toastr.error(err?.responseJSON?.message || window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phù hợp"])
                    $(".chart_data_container").hide();
                    $(".chart_data_empty").show();
                }).finally(() => {

                    let panel = $("#panelReporting2");
                    panel.removeClass("d-none").addClass("d-none");
                })
            }
            else if (isDepartment && isLocation && !isAuditor) {
                //Lọc theo phòng ban và khu vực => báo cáo theo khu vực
                let listLocations = [];
                let activeLocations = root.parentEl.find("#kvList .audit_location.active");

                activeLocations.each(function () {
                    var value = $(this).attr('name');
                    listLocations.push(value);
                });

                let locationAuditReportModel = {
                    InventoryId: $("#ReportingAudit_InventoryName").val(),
                    AuditReportType: AuditReportType.Location,
                    Departments: [],
                    Locations: listLocations,
                    Auditors: [],
                    ReportingAuditTypes: document.querySelector('#ReportingAudit_Type').isAllSelected() ? [] : $("#ReportingAudit_Type").val().map(Number)
                };

                APIs.AuditReportingAPI(locationAuditReportModel).then(res => {
                    RenderChart(res.data)
                }).catch(err => {
                    toastr.error(err?.responseJSON?.message || window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phù hợp"])
                    $(".chart_data_container").hide();
                    $(".chart_data_empty").show();
                }).finally(() => {

                    let panel = $("#panelReporting2");
                    panel.removeClass("d-none").addClass("d-none");
                })
            }
            else if (isDepartment && isLocation && isAuditor) {
                //Lọc theo phòng ban, khu vực và người giám sát => báo cáo theo người giám sát
                let listAuditors = [];
                let activeAuditors = root.parentEl.find("#gsList .audit_auditor.active");

                activeAuditors.each(function () {
                    var value = $(this).attr('name');
                    listAuditors.push(value);
                });

                let auditorAuditReportModel = {
                    InventoryId: $("#ReportingAudit_InventoryName").val(),
                    AuditReportType: AuditReportType.Auditor,
                    Departments: [],
                    Locations: [],
                    Auditors: listAuditors,
                    ReportingAuditTypes: document.querySelector('#ReportingAudit_Type').isAllSelected() ? [] : $("#ReportingAudit_Type").val().map(Number)
                };

                APIs.AuditReportingAPI(auditorAuditReportModel).then(res => {
                    RenderChart(res.data)
                }).catch(err => {
                    toastr.error(err?.responseJSON?.message || window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phù hợp"])
                    $(".chart_data_container").hide();
                    $(".chart_data_empty").show();
                }).finally(() => {

                    let panel = $("#panelReporting2");
                    panel.removeClass("d-none").addClass("d-none");
                })
            }
            
        })

        root.parentEl.find(".btnCloseSetting").click(function (e) {
            let panel = $("#panelReporting2");
            panel.removeClass("d-none").addClass("d-none");
        })


        async function ResetZoomChart() {
            return new Promise((resolve, reject) => {
                window?.auditReportChart?.resetZoom();
                setTimeout(() => {
                    resolve();
                }, 200);
            })
        }

        //Nhấn tải file pdf
        $("#exportFile").click(async function () {
            let isAuditActiveTab = $("#supervision").hasClass("active");
            if (!isAuditActiveTab) {
                return;
            }

            loading(true);
            //Reset zoom
            await ResetZoomChart();

            // Choose the element that your content will be rendered to.
            const element = document.querySelector('#supervision-report-tab');
            // Choose the element and save the PDF for your user.
            let currentDate = new moment().format("YYYY-MM-DD HH:mm:ss");

            let isOverFlowWidth = $("#InventoryReporting2_DataTable").outerWidth() > $('#supervision-report-tab').outerWidth(); 
            let tableWidth = $("#InventoryReporting2_DataTable").outerWidth() + 50;
            let body = document.body
            let html = document.documentElement
            let height = Math.max(body.scrollHeight, body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight)
            let target = document.querySelector('#supervision-report-tab')
            let heightCM = height / 38;

            let maxScreenWidth = Math.max(tableWidth);
            let cacheBodyScreen = $('#supervision-report-tab').outerWidth();
            $('#supervision-report-tab').width(maxScreenWidth);
            window.auditReportChart.resize();
            window.auditFreeReportChart.resize();
            $('#supervision-report-tab').css("margin", "auto");


            //Ẩn các nút thừa trên giao diện khi export file pdf
            $(target).find(".hidden_while_export").hide();
            html2pdf(target, {
                margin: 0,
                filename: `Baocaogiamsat_${currentDate}.pdf`,
                html2canvas: { dpi: 190, letterRendering: false, scale: 2 },
                jsPDF: {
                    orientation: 'portrait',
                    unit: 'cm',
                    //format: [heightCM, 60],
                    format: [isOverFlowWidth ? (heightCM + 30) : heightCM, isOverFlowWidth ? 100 : 60],
                    compress: true,
                    precision: 16
                }
            }).then(function () {
                //Hiển thị lại các nút bị ẩn khi xuất file pdf
                $(target).find(".hidden_while_export").show();

                $('#supervision-report-tab').width(cacheBodyScreen);
                window.auditReportChart.resize();
                window.auditFreeReportChart.resize();

                $('#supervision-report-tab').css("margin", "");

                loading(false);
            })
        })
    }


    function RenderChart(data) {
        
        $("#FixedAuditReporting").hide();
        $("#FreeAuditReporting").hide();

        let fixedAuditReporting = [];
        let freeAuditReporting = [];
        fixedAuditReporting = $.grep(data, function (item) {
            return item.reportingAuditType === 0;
        });

        freeAuditReporting = $.grep(data, function (item) {
            return item.reportingAuditType === 1;
        });

        if (fixedAuditReporting.length > 0) {
            $(".all_report_chart_data_empty").hide();
            FixedAuditReporting(fixedAuditReporting);
            
        }
        if (freeAuditReporting.length > 0) {
            $(".all_report_chart_data_empty").hide();
            FreeAuditReporting(freeAuditReporting);
           
        }
        
        if (fixedAuditReporting.length === 0 && freeAuditReporting.length === 0) {
            $(".all_report_chart_data_empty").show();
        }
        
    }

    function FixedAuditReporting(data) {
        $("#FixedAuditReporting").show();

        //Nếu không có dữ liệu thì hiển thị dữ liệu trống
        if (data.length == 0) {
            $("#FixedAuditReporting .chart_data_container").hide();
            $("#FixedAuditReporting .chart_data_empty").show();

            $("#exportFile").hide();
        } else {
            $("#FixedAuditReporting .chart_data_container").show();
            $("#FixedAuditReporting .chart_data_empty").hide();

            $("#exportFile").show();
        }

        //Mảng data 
        let arrDataNotAuditYet = data.map(item => {
            return item.totalTodo
        });
        let arrDataNotPassAudit = data.map(item => {
            return item.totalFail
        })
        let arrDataPass = data.map(item => {
            return item.totalPass
        })

        //Mảng data tính tỉ lệ %
        let arrDataNotAuditYetPercentage = data.map(item => {
            return item.totalDoc === 0 ? 0 : parseFloat((item.totalTodo / item.totalDoc) * 100).toFixed(2)
        });
        let arrDataNotPassAuditPercentage = data.map(item => {
            return item.totalDoc === 0 ? 0 : parseFloat((item.totalFail / item.totalDoc) * 100).toFixed(2)
        })
        let arrDataPassPercentage = data.map(item => {
            return item.totalDoc === 0 ? 0 : parseFloat((item.totalPass / item.totalDoc) * 100).toFixed(2)
        })


        //Labels
        let locationsNames = data.map(item => {
            return item.name
        });

        //Tổng tất cả các phiếu
        let totalDocCount = data.reduce((acc, curr) => {
            return acc + curr.totalDoc;
        }, 0)

        //Mảng tổng các phiếu
        let totalDocArray = data.reduce((acc, curr) => {
            return [...acc, curr.totalDoc]
        }, [])
        // create new element
        const newCtx = document.createElement("canvas");
        newCtx.id = "FixedAuditChart";

        let delayed;
        //if (window.auditReportChart && window.auditReportChart != null) {
        //    window.auditReportChart.destroy();
        //}

        //Mảng giá trị các bản ghi chưa đạt giám sát
        let auditNotYetValues = data.map(x => {
            return x.totalTodo
        })

        let auditFailValues = data.map(x => {
            return x.totalFail
        })

        let auditPassedValues = data.map(x => {
            return x.totalPass
        })

        if (!window.auditReportChart) {
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

            //modifierKey: 'ctrl',
            window.auditReportChart = new Chart(document.getElementById('FixedAuditChart'), {
                data: {
                    labels: locationsNames,
                    datasets: [
                        {
                            type: "line",
                            label: window.languageData[window.currentLanguage]["Tỉ lệ chưa giám sát"],
                            backgroundColor: "#50555C",
                            data: arrDataNotAuditYetPercentage, // phần trăm
                            borderColor: "#50555C",
                            pointBorderColor: "#FFFFFF",
                            borderWidth: 1.5,
                            pointRadius: 5,
                            pointHoverRadius: 10,
                            yAxisID: "y1",
                        },
                        {
                            type: "line",
                            label: window.languageData[window.currentLanguage]["Tỉ lệ không đạt giám sát"],
                            backgroundColor: "#DA8D00",
                            data: arrDataNotPassAuditPercentage, // phần trăm
                            borderColor: "#DA8D00",
                            pointBorderColor: "#FFFFFF",
                            borderWidth: 1.5,
                            pointRadius: 5,
                            pointHoverRadius: 10,
                            yAxisID: "y1",
                        },
                        {
                            type: "line",
                            label: window.languageData[window.currentLanguage]["Tỉ lệ giám sát"],
                            backgroundColor: "#17AE5C",
                            data: arrDataPassPercentage, // phần trăm
                            borderColor: "#17AE5C",
                            pointBorderColor: "#17AE5C",
                            borderWidth: 1.5,
                            pointRadius: 5,
                            pointHoverRadius: 10,
                            yAxisID: "y1",
                        },
                        {
                            type: "bar",
                            label: window.languageData[window.currentLanguage]["Đạt giám sát"],
                            backgroundColor: "#009543",
                            data: arrDataPass,
                            borderColor: "#009543",
                            pointBorderColor: "#FFFFFF",
                            borderWidth: 0,
                            pointRadius: 5,
                            pointHoverRadius: 10,
                            barThickness: 50,
                        },
                        {
                            type: "bar",
                            label: window.languageData[window.currentLanguage]["Không đạt giám sát"],
                            backgroundColor: "#DA8D00",
                            data: arrDataNotPassAudit, // phần trăm
                            borderColor: "#009543",
                            pointBorderColor: "#FFFFFF",
                            borderWidth: 0,
                            pointRadius: 5,
                            pointHoverRadius: 10,
                            barThickness: 50,

                        },
                        {
                            type: "bar",
                            label: window.languageData[window.currentLanguage]["Chưa giám sát"],
                            backgroundColor: "#C2C2C2",
                            data: arrDataNotAuditYet, // phần trăm
                            borderColor: "#50555C",
                            pointBorderColor: "#FFFFFF",
                            borderWidth: 0,
                            pointRadius: 5,
                            pointHoverRadius: 10,
                            barThickness: 50,
                        }
                    ],
                },
                options: {
                    onClick: async (e, activeEls) => {
                        let dataIndex = activeEls[0].index;
                        let label = e.chart.data.labels[dataIndex];
                        let listLocations = [];
                        let listAuditors = [];

                        //Lấy danh sách khu vực khi chọn 1 phòng ban:
                        let departmentFilterData = {
                            Departments: [label]
                        }

                        let resLocation = await APIs.GetLocationsAPI(departmentFilterData);
                        if (resLocation?.data?.length) {
                            root.parentEl.find("#pbList").find(".audit_department").removeClass("active").addClass("active");
                            //root.parentEl.find("#kvList").find(".audit_location").removeClass("active");
                            //root.parentEl.find("#gsList").find(".audit_auditor").removeClass("active");

                            resLocation.data.map((item) => {
                                listLocations.push(item.locationName);
                                //root.parentEl.find("#pbList").find(`.audit_department[name='${label}']`).addClass("active");
                                //root.parentEl.find("#kvList").find(`.audit_location[name='${item.locationName}']`).addClass("active");
                            })

                        }

                        //Lấy danh sách người giám sát khi chọn 1 khu vực:
                        let auditorFilterData = {
                            Locations: [label]
                        }

                        let resAuditor = await APIs.GetAuditorsAPI(auditorFilterData);
                        if (resAuditor?.data?.length) {
                            //root.parentEl.find("#kvList").find(".audit_location").removeClass("active");
                            //root.parentEl.find("#gsList").find(".audit_auditor").removeClass("active");
                            root.parentEl.find("#pbList").find(".audit_department").removeClass("active").addClass("active");

                            let html = resAuditor.data.map((item) => {
                                return `<span class="audit_auditor spanBtn" name="${item.auditorName}">${item.auditorName}</span>`
                            })

                            root.parentEl.find("#gsList").html(html);

                            resAuditor.data.map((item) => {
                                listAuditors.push(item.auditorName)
                                //root.parentEl.find("#kvList").find(`.audit_location[name='${label}']`).addClass("active");
                                //root.parentEl.find("#gsList").find(`.audit_auditor[name='${item.auditorName}']`).addClass("active");
                            })

                        }

                        if (listLocations.length !== 0 && listAuditors.length === 0) {
                            //Lọc theo phòng ban và khu vực => báo cáo theo khu vực


                            let locationAuditReportModel = {
                                InventoryId: $("#ReportingAudit_InventoryName").val(),
                                AuditReportType: AuditReportType.Location,
                                Departments: [],
                                Locations: listLocations,
                                Auditors: [],
                                ReportingAuditTypes: [0]
                            };

                            APIs.AuditReportingAPI(locationAuditReportModel).then(res => {
                                FixedAuditReporting(res.data)
                            }).catch(err => {
                                toastr.error(err?.responseJSON?.message || window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phù hợp"])
                                $("#FixedAuditReporting .chart_data_container").hide();
                                $("#FixedAuditReporting .chart_data_empty").show();
                            }).finally(() => {

                                let panel = $("#panelReporting2");
                                panel.removeClass("d-none").addClass("d-none");
                            })
                        }
                        else {
                            //Lọc theo phòng ban, khu vực và người giám sát => báo cáo theo người giám sát

                            let auditorAuditReportModel = {
                                InventoryId: $("#ReportingAudit_InventoryName").val(),
                                AuditReportType: AuditReportType.Auditor,
                                Departments: [],
                                Locations: [],
                                Auditors: listAuditors,
                                ReportingAuditTypes: [0]
                            };

                            APIs.AuditReportingAPI(auditorAuditReportModel).then(res => {
                                FixedAuditReporting(res.data)
                            }).catch(err => {
                                toastr.error(err?.responseJSON?.message || window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phù hợp"])
                                $("#FixedAuditReporting .chart_data_container").hide();
                                $("#FixedAuditReporting .chart_data_empty").show();
                            }).finally(() => {

                                let panel = $("#panelReporting2");
                                panel.removeClass("d-none").addClass("d-none");
                            })
                        }


                    },
                    // set style for label
                    plugins: {
                        legend: {
                            display: false,
                            position: "bottom",
                            align: "center",
                            fontFamily: "Arial",
                            onClick: function (e, legendItem, legend) {
                                var index = legendItem.datasetIndex;
                                var meta = window.auditReportChart.getDatasetMeta(index);

                                meta.hidden = meta.hidden === null ? !window.auditReportChart.data.datasets[index].hidden : null;

                                window.auditReportChart.update('none');
                            },
                            labels: {
                                usePointStyle: false,
                                //fontColor: "red",
                                generateLabels(chart) {
                                    const data = chart.data;
                                    const datasets = data.datasets;

                                    return data.datasets.map((item, i) => {
                                        const meta = chart.getDatasetMeta(0);
                                        const style = meta.controller.getStyle(i);

                                        return {
                                            text: item.label,
                                            fillStyle: item.borderColor,
                                            strokeStyle: 'red',
                                            lineWidth: style.borderWidth,
                                            hidden: !chart.getDataVisibility(i),
                                            datasetIndex: i
                                        }
                                    });
                                    return [];
                                }
                            }
                        },
                        zoom: zoomOptions
                    },
                    responsive: true,
                    animation: {
                        duration: 0
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
                            time: {
                                displayFormats: {
                                    hour: 'HH:mm',
                                    minute: 'HH:mm',
                                    second: 'HH:mm:ss'
                                }
                            }
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
                            max: 100,
                            min: 0,
                            position: "right",
                            ticks: {
                                stepSize: 10,
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
                },
            });
        } else {
            window.auditReportChart.data.labels = locationsNames;
            window.auditReportChart.data.datasets[0].data = arrDataNotAuditYetPercentage
            window.auditReportChart.data.datasets[1].data = arrDataNotPassAuditPercentage
            window.auditReportChart.data.datasets[2].data = arrDataPassPercentage
            window.auditReportChart.data.datasets[3].data = arrDataPass
            window.auditReportChart.data.datasets[4].data = arrDataNotPassAudit
            window.auditReportChart.data.datasets[5].data = arrDataNotAuditYet

            window.auditReportChart.update('none');
        }

        // replace canvas with newCtx
        //$("#auditChart").replaceWith(newCtx);

        //Render table
        let subHeader = ``
        let layoutHeaders = data.map(item => {
            subHeader += `<th>SL</th><th>${window.languageData[window.currentLanguage]["Tỉ lệ"]}</th>`
            return `<th colspan="2">${item.name} </th>`;
        })

        let tableHeader = `<thead><tr><th rowspan="2" class="table_first_header">${window.languageData[window.currentLanguage]["Hạng mục"]}</th>${layoutHeaders}</tr>
                                <tr>${subHeader}</tr></thead>`

        $("#Fixed_InventoryReporting2_DataTable thead").replaceWith(tableHeader);

        //Tổng phiếu cần giám sát
        let totalAuditDoc = `<td>${window.languageData[window.currentLanguage]["Tổng phiếu cần giám sát"]}</td>`;
        data.map(item => {
            //let percentage = parseFloat((item.totalPass / item.totalDoc) * 100).toFixed(2);
            let percentage = 100;
            totalAuditDoc += `<td>${ValidateInputHelper.Utils.convertDecimalInventory(item.totalDoc)}</td>
                               <td>${percentage}%</td>`
        })

        //Số phiếu chưa giám sát
        let todoDoc = `<td>${window.languageData[window.currentLanguage]["Số phiếu chưa giám sát"]}</td>`;
        data.map(item => {
            let percentage = item.totalDoc === 0 ? 0 : +(parseFloat((item.totalTodo / item.totalDoc) * 100)).toFixed(2);
            todoDoc += `<td>${ValidateInputHelper.Utils.convertDecimalInventory(item.totalTodo)}</td>
                               <td>${percentage}%</td>`
        })

        //Số phiếu giám sát không đạt
        let failedDoc = `<td>${window.languageData[window.currentLanguage]["Số phiếu giám sát không đạt"]}</td>`;
        data.map(item => {
            let percentage = item.totalDoc === 0 ? 0 : +(parseFloat((item.totalFail / item.totalDoc) * 100)).toFixed(2);
            failedDoc += `<td>${ValidateInputHelper.Utils.convertDecimalInventory(item.totalFail)}</td>
                               <td>${percentage}%</td>`
        })
        //Số phiếu giám sát đạt
        let passedDoc = `<td>${window.languageData[window.currentLanguage]["Số phiếu giám sát đạt"]}</td>`;
        data.map(item => {
            let percentage = item.totalDoc === 0 ? 0 : +(parseFloat((item.totalPass / item.totalDoc) * 100)).toFixed(2);
            passedDoc += `<td>${ValidateInputHelper.Utils.convertDecimalInventory(item.totalPass)}</td>
                               <td>${percentage}%</td>`
        })

        $("#Fixed_InventoryReporting2_DataTable tbody").replaceWith(`<tbody>
                                                    <tr>${totalAuditDoc}</tr>
                                                    <tr>${todoDoc}</tr>
                                                    <tr>${failedDoc}</tr>
                                                    <tr>${passedDoc}</tr>
                                                </tbody>`);

        //Ẩn hiện bảng chú giải màu
        $("#FixedAuditReporting .glossary_audit_report").show();
        $("#FixedAuditReporting .glossary_audit_report .HideArea").removeClass("HideArea");
        $("#FixedAuditReporting .headerDetailTable, .hideDetailTable").removeClass("HideArea").show();
    }

    function FreeAuditReporting(data) {
        $("#FreeAuditReporting").show();

        //Nếu không có dữ liệu thì hiển thị dữ liệu trống
        if (data.length == 0) {
            $("#FreeAuditReporting .chart_data_container").hide();
            $("#FreeAuditReporting .chart_data_empty").show();

            $("#exportFile").hide();
        } else {
            $("#FreeAuditReporting .chart_data_container").show();
            $("#FreeAuditReporting .chart_data_empty").hide();

            $("#exportFile").show();
        }

        //Mảng data 
        let arrDataNotAuditYet = data.map(item => {
            return item.totalTodo
        });
        let arrDataNotPassAudit = data.map(item => {
            return item.totalFail
        })
        let arrDataPass = data.map(item => {
            return item.totalPass
        })

        //Mảng data tính tỉ lệ %
        let arrDataNotAuditYetPercentage = data.map(item => {
            return item.totalDoc === 0 ? 0 : parseFloat((item.totalTodo / item.totalDoc) * 100).toFixed(2)
        });
        let arrDataNotPassAuditPercentage = data.map(item => {
            return item.totalDoc === 0 ? 0 : parseFloat((item.totalFail / item.totalDoc) * 100).toFixed(2)
        })
        let arrDataPassPercentage = data.map(item => {
            return item.totalDoc === 0 ? 0 : parseFloat((item.totalPass / item.totalDoc) * 100).toFixed(2)
        })


        //Labels
        let locationsNames = data.map(item => {
            return item.name
        });

        //Tổng tất cả các phiếu
        let totalDocCount = data.reduce((acc, curr) => {
            return acc + curr.totalDoc;
        }, 0)

        //Mảng tổng các phiếu
        let totalDocArray = data.reduce((acc, curr) => {
            return [...acc, curr.totalDoc]
        }, [])
        // create new element
        const newFreeAuditCtx = document.createElement("canvas");
        newFreeAuditCtx.id = "FreeAuditChart";

        let delayed;
        //if (window.auditReportChart && window.auditReportChart != null) {
        //    window.auditReportChart.destroy();
        //}

        //Mảng giá trị các bản ghi chưa đạt giám sát
        let auditNotYetValues = data.map(x => {
            return x.totalTodo
        })

        let auditFailValues = data.map(x => {
            return x.totalFail
        })

        let auditPassedValues = data.map(x => {
            return x.totalPass
        })

        if (!window.auditFreeReportChart) {
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

            //modifierKey: 'ctrl',
            window.auditFreeReportChart = new Chart(document.getElementById('FreeAuditChart'), {
                data: {
                    labels: locationsNames,
                    datasets: [
                        //{
                        //    type: "line",
                        //    label: window.languageData[window.currentLanguage]["Tỉ lệ chưa giám sát"],
                        //    backgroundColor: "#50555C",
                        //    data: arrDataNotAuditYetPercentage, // phần trăm
                        //    borderColor: "#50555C",
                        //    pointBorderColor: "#FFFFFF",
                        //    borderWidth: 1.5,
                        //    pointRadius: 5,
                        //    pointHoverRadius: 10,
                        //    yAxisID: "y1",
                        //},
                        {
                            type: "line",
                            label: window.languageData[window.currentLanguage]["Tỉ lệ không đạt giám sát"],
                            backgroundColor: "#DA8D00",
                            data: arrDataNotPassAuditPercentage, // phần trăm
                            borderColor: "#DA8D00",
                            pointBorderColor: "#FFFFFF",
                            borderWidth: 1.5,
                            pointRadius: 5,
                            pointHoverRadius: 10,
                            yAxisID: "y1",
                        },
                        {
                            type: "line",
                            label: window.languageData[window.currentLanguage]["Tỉ lệ giám sát"],
                            backgroundColor: "#17AE5C",
                            data: arrDataPassPercentage, // phần trăm
                            borderColor: "#17AE5C",
                            pointBorderColor: "#17AE5C",
                            borderWidth: 1.5,
                            pointRadius: 5,
                            pointHoverRadius: 10,
                            yAxisID: "y1",
                        },
                        {
                            type: "bar",
                            label: window.languageData[window.currentLanguage]["Đạt giám sát"],
                            backgroundColor: "#009543",
                            data: arrDataPass,
                            borderColor: "#009543",
                            pointBorderColor: "#FFFFFF",
                            borderWidth: 0,
                            pointRadius: 5,
                            pointHoverRadius: 10,
                            barThickness: 50,
                        },
                        {
                            type: "bar",
                            label: window.languageData[window.currentLanguage]["Không đạt giám sát"],
                            backgroundColor: "#DA8D00",
                            data: arrDataNotPassAudit, // phần trăm
                            borderColor: "#009543",
                            pointBorderColor: "#FFFFFF",
                            borderWidth: 0,
                            pointRadius: 5,
                            pointHoverRadius: 10,
                            barThickness: 50,

                        }
                        //,
                        //{
                        //    type: "bar",
                        //    label: window.languageData[window.currentLanguage]["Chưa giám sát"],
                        //    backgroundColor: "#C2C2C2",
                        //    data: arrDataNotAuditYet, // phần trăm
                        //    borderColor: "#50555C",
                        //    pointBorderColor: "#FFFFFF",
                        //    borderWidth: 0,
                        //    pointRadius: 5,
                        //    pointHoverRadius: 10,
                        //    barThickness: 50,
                        //}
                    ],
                },
                options: {
                    onClick: async (e, activeEls) => {
                        let dataIndex = activeEls[0].index;
                        let label = e.chart.data.labels[dataIndex];

                        let listLocations = [];
                        let listAuditors = [];

                        //Lấy danh sách khu vực khi chọn 1 phòng ban:
                        let departmentFilterData = {
                            Departments: [label]
                        }

                        let resLocation = await APIs.GetLocationsAPI(departmentFilterData);
                        if (resLocation?.data?.length) {
                            root.parentEl.find("#pbList").find(".audit_department").removeClass("active").addClass("active");
                            //root.parentEl.find("#kvList").find(".audit_location").removeClass("active");
                            //root.parentEl.find("#gsList").find(".audit_auditor").removeClass("active");

                            resLocation.data.map((item) => {
                                listLocations.push(item.locationName);
                                //root.parentEl.find("#pbList").find(`.audit_department[name='${label}']`).addClass("active");
                                //root.parentEl.find("#kvList").find(`.audit_location[name='${item.locationName}']`).addClass("active");
                            })

                        }

                        //Lấy danh sách người giám sát khi chọn 1 khu vực:
                        let auditorFilterData = {
                            Locations: [label]
                        }

                        let resAuditor = await APIs.GetAuditorsAPI(auditorFilterData);
                        if (resAuditor?.data?.length) {
                            //root.parentEl.find("#kvList").find(".audit_location").removeClass("active");
                            //root.parentEl.find("#gsList").find(".audit_auditor").removeClass("active");
                            root.parentEl.find("#pbList").find(".audit_department").removeClass("active").addClass("active");

                            let html = resAuditor.data.map((item) => {
                                return `<span class="audit_auditor spanBtn" name="${item.auditorName}">${item.auditorName}</span>`
                            })

                            root.parentEl.find("#gsList").html(html);

                            resAuditor.data.map((item) => {
                                listAuditors.push(item.auditorName)
                                //root.parentEl.find("#kvList").find(`.audit_location[name='${label}']`).addClass("active");
                                //root.parentEl.find("#gsList").find(`.audit_auditor[name='${item.auditorName}']`).addClass("active");
                            })

                        }

                        if (listLocations.length !== 0 && listAuditors.length === 0) {
                            //Lọc theo phòng ban và khu vực => báo cáo theo khu vực

                            let locationAuditReportModel = {
                                InventoryId: $("#ReportingAudit_InventoryName").val(),
                                AuditReportType: AuditReportType.Location,
                                Departments: [],
                                Locations: listLocations,
                                Auditors: [],
                                ReportingAuditTypes: [1]
                            };

                            APIs.AuditReportingAPI(locationAuditReportModel).then(res => {
                                FreeAuditReporting(res.data)
                            }).catch(err => {
                                toastr.error(err?.responseJSON?.message || window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phù hợp"])
                                $("#FreeAuditReporting .chart_data_container").hide();
                                $("#FreeAuditReporting .chart_data_empty").show();
                            }).finally(() => {

                                let panel = $("#panelReporting2");
                                panel.removeClass("d-none").addClass("d-none");
                            })
                        }
                        else {
                            //Lọc theo phòng ban, khu vực và người giám sát => báo cáo theo người giám sát

                            let auditorAuditReportModel = {
                                InventoryId: $("#ReportingAudit_InventoryName").val(),
                                AuditReportType: AuditReportType.Auditor,
                                Departments: [],
                                Locations: [],
                                Auditors: listAuditors,
                                ReportingAuditTypes: [1]
                            };

                            APIs.AuditReportingAPI(auditorAuditReportModel).then(res => {
                                FreeAuditReporting(res.data)
                            }).catch(err => {
                                toastr.error(err?.responseJSON?.message || window.languageData[window.currentLanguage]["Không tìm thấy dữ liệu phù hợp"])
                                $("#FreeAuditReporting .chart_data_container").hide();
                                $("#FreeAuditReporting .chart_data_empty").show();
                            }).finally(() => {

                                let panel = $("#panelReporting2");
                                panel.removeClass("d-none").addClass("d-none");
                            })
                        }


                    },
                    // set style for label
                    plugins: {
                        legend: {
                            display: false,
                            position: "bottom",
                            align: "center",
                            fontFamily: "Arial",
                            onClick: function (e, legendItem, legend) {
                                var index = legendItem.datasetIndex;
                                var meta = window.auditFreeReportChart.getDatasetMeta(index);

                                meta.hidden = meta.hidden === null ? !window.auditFreeReportChart.data.datasets[index].hidden : null;

                                window.auditFreeReportChart.update('none');
                            },
                            labels: {
                                usePointStyle: false,
                                //fontColor: "red",
                                generateLabels(chart) {
                                    const data = chart.data;
                                    const datasets = data.datasets;

                                    return data.datasets.map((item, i) => {
                                        const meta = chart.getDatasetMeta(0);
                                        const style = meta.controller.getStyle(i);

                                        return {
                                            text: item.label,
                                            fillStyle: item.borderColor,
                                            strokeStyle: 'red',
                                            lineWidth: style.borderWidth,
                                            hidden: !chart.getDataVisibility(i),
                                            datasetIndex: i
                                        }
                                    });
                                    return [];
                                }
                            }
                        },
                        zoom: zoomOptions
                    },
                    responsive: true,
                    animation: {
                        duration: 0
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
                            time: {
                                displayFormats: {
                                    hour: 'HH:mm',
                                    minute: 'HH:mm',
                                    second: 'HH:mm:ss'
                                }
                            }
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
                            max: 100,
                            min: 0,
                            position: "right",
                            ticks: {
                                stepSize: 10,
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
                },
            });
        } else {
            window.auditFreeReportChart.data.labels = locationsNames;
            //window.auditFreeReportChart.data.datasets[0].data = arrDataNotAuditYetPercentage
            window.auditFreeReportChart.data.datasets[0].data = arrDataNotPassAuditPercentage
            window.auditFreeReportChart.data.datasets[1].data = arrDataPassPercentage
            window.auditFreeReportChart.data.datasets[2].data = arrDataPass
            window.auditFreeReportChart.data.datasets[3].data = arrDataNotPassAudit
            //window.auditFreeReportChart.data.datasets[5].data = arrDataNotAuditYet

            window.auditFreeReportChart.update('none');
        }

        // replace canvas with newCtx
        //$("#auditChart").replaceWith(newCtx);

        //Render table
        let subHeader = ``
        let layoutHeaders = data.map(item => {
            subHeader += `<th>SL</th><th>${window.languageData[window.currentLanguage]["Tỉ lệ"]}</th>`
            return `<th colspan="2">${item.name} </th>`;
        })

        let tableHeader = `<thead><tr><th rowspan="2" class="table_first_header">${window.languageData[window.currentLanguage]["Hạng mục"]}</th>${layoutHeaders}</tr>
                                <tr>${subHeader}</tr></thead>`

        $("#Free_InventoryReporting2_DataTable thead").replaceWith(tableHeader);

        //Tổng phiếu cần giám sát
        let totalAuditDoc = `<td>${window.languageData[window.currentLanguage]["Tổng phiếu cần giám sát"]}</td>`;
        data.map(item => {
            //let percentage = parseFloat((item.totalPass / item.totalDoc) * 100).toFixed(2);
            let percentage = 100;
            totalAuditDoc += `<td>${ValidateInputHelper.Utils.convertDecimalInventory(item.totalDoc)}</td>
                               <td>${percentage}%</td>`
        })

        //Số phiếu chưa giám sát
        let todoDoc = `<td>${window.languageData[window.currentLanguage]["Số phiếu chưa giám sát"]}</td>`;
        data.map(item => {
            let percentage = item.totalDoc === 0 ? 0 : +(parseFloat((item.totalTodo / item.totalDoc) * 100)).toFixed(2);
            todoDoc += `<td>${ValidateInputHelper.Utils.convertDecimalInventory(item.totalTodo)}</td>
                               <td>${percentage}%</td>`
        })

        //Số phiếu giám sát không đạt
        let failedDoc = `<td>${window.languageData[window.currentLanguage]["Số phiếu giám sát không đạt"]}</td>`;
        data.map(item => {
            let percentage = item.totalDoc === 0 ? 0 : +(parseFloat((item.totalFail / item.totalDoc) * 100)).toFixed(2);
            failedDoc += `<td>${ValidateInputHelper.Utils.convertDecimalInventory(item.totalFail)}</td>
                               <td>${percentage}%</td>`
        })
        //Số phiếu giám sát đạt
        let passedDoc = `<td>${window.languageData[window.currentLanguage]["Số phiếu giám sát đạt"]}</td>`;
        data.map(item => {
            let percentage = item.totalDoc === 0 ? 0 : +(parseFloat((item.totalPass / item.totalDoc) * 100)).toFixed(2);
            passedDoc += `<td>${ValidateInputHelper.Utils.convertDecimalInventory(item.totalPass)}</td>
                               <td>${percentage}%</td>`
        })

        $("#Free_InventoryReporting2_DataTable tbody").replaceWith(`<tbody>
                                                    <tr>${totalAuditDoc}</tr>
                                                    <tr>${failedDoc}</tr>
                                                    <tr>${passedDoc}</tr>
                                                </tbody>`);

        //$("#Free_InventoryReporting2_DataTable tbody").replaceWith(`<tbody>
        //                                            <tr>${totalAuditDoc}</tr>
        //                                            <tr>${todoDoc}</tr>
        //                                            <tr>${failedDoc}</tr>
        //                                            <tr>${passedDoc}</tr>
        //                                        </tbody>`);

        //Ẩn hiện bảng chú giải màu
        $("#FreeAuditReporting .glossary_audit_report").show();
        $("#FreeAuditReporting .glossary_audit_report .HideArea").removeClass("HideArea");
        $("#FreeAuditReporting .headerDetailTable, .hideDetailTable").removeClass("HideArea").show();
    }

    function Init() {
        Cache();
        PreLoad();
        Events();
    }

    return {
        init: Init
    }
})();