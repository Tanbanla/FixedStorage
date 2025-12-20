/*dragula([document.getElementById("upcoming-task"),document.getElementById("inprogress-task"),document.getElementById("complete-task")]);*/
dragula([document.getElementById("kanban1"), document.getElementById("kanban2"), document.getElementById("kanban3"), document.getElementById("kanban4")]);


//dragula([document.querySelector('#sortable-listings')], {
//    direction: 'vertical',
//    revertOnSpill: true,
//}).on('drop', function (el, container) {
//    var Lists = $(container).find('.list');
//    var reOrder = [];
//    $.each(Lists, function (key, value) {
//        reOrder.push({ 'film_id': $(value).data('film-id'), 'trailer_id': $(value).data('trailer-id') });
//    });
//    _UpdateFetaureTrailerOdering(el, reOrder);
//});//-- end of dragular


//function _UpdateFetaureTrailerOdering(item, listing) {
//    $.ajax({
//        url: '/TelesalePools/ReOrder',
//        type: 'POST',
//        data: { new_order: listing },
//        success: function (res, sec) {
//            $(item).find('.response').addClass("success").delay(2000).queue(function () {
//                $(this).removeClass("success").dequeue();
//            });
//        },
//        error: function (res, sec) {
//            ///....
//        }
//    });
//}