$(function () {
  CloseModal();
});

function CloseModal () {
  $("#btn-close").click(function () {
    $("#InventoryListDownloadFormModal").modal("hide");
  })
}


$(".container_item_download_form").click(function (e) {
    let fileKey = $(this).attr("filename");

    FileTemplateHandler.download(fileKey);
})