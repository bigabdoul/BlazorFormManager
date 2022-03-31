
if (!Date.prototype.addDays) {
    Date.prototype.addDays = function (days) {
        var date = new Date(this.valueOf());
        date.setDate(date.getDate() + days);
        return date;
    }
}

function toIsoDateString(date) {
    if (!!date && typeof date === 'string') date = new Date(date);
    const d = date || new Date();
    return new Date(d.getTime() - (d.getTimezoneOffset() * 60 * 1000)).toISOString().split('T')[0];
}

function replaceQueryParam(name, value, uri) {
    uri || (uri = window.location.href);

    const re = new RegExp("([?&])" + name + "=.*?(&|$)", "i");
    const separator = uri.indexOf('?') !== -1 ? "&" : "?";

    return uri.match(re)
        ? uri.replace(re, '$1' + name + "=" + value + '$2')
        : uri + separator + name + "=" + value;
}
