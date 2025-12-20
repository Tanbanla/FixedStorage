
;App.User.isGrant = function (permission) {
    return this.RoleClaims.some(p => p.ClaimType === permission)
}

//Read only User
Object.freeze(App.User)
Object.freeze(App);

$.fn.StartLoading = function () {
    this.buttonLoader('start');
}
$.fn.StopLoading = function () {
    this.buttonLoader('stop');
}

