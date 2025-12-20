;(function ($) {
    const obj = $({});

    $.subcribe = function () {
        obj.on.apply(obj, arguments);
    }
    $.unsubcribe = function () {
        obj.off.apply(obj, arguments);
    }
    $.emit = function () {
        obj.trigger.apply(obj, arguments);
    }

    return obj;
})(jQuery)