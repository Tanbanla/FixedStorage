$(function () {
  handleButtonTabClick();
});

function handleButtonTabClick() {
    $(".changeTab").click(function (e) {
        var tabId = $(this).attr("id");

        //Prevent multiples click on current active tab
        let tabIsActive = $(this).hasClass("active");
        if (tabIsActive) return;

        handleChangeTab(tabId);
        $(window).trigger("report.tab.changed", e.target);
    });

}



function handleChangeTab(tabId) {
  // remove all class .active in element has class .changeTab
  $(".changeTab").removeClass("active");
  // add class .active in element has id = tabId
  $(`#${tabId}`).addClass("active");
  // hide all element has class .tab-content
  $(".tab-content").hide();
  // show element has id = tabId
    $(`#${tabId}-report-tab`).show();
}