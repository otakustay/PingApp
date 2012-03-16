$(function() {
    $('#doSearch').click(function() {
        var path = location.pathname;
        var items = path.split('/');
        var keywords = encodeURIComponent($.trim($('#keywords').val()));

        if (!keywords) {
            $('#keywords').focus();
            return false;
        }

        // 已经在搜索页面
        if (path.indexOf('/search') == 0) {
            items[items.length - 1] = keywords;
            location.href = items.join('/') + location.search;
        }
        else {
            location.href = '/search/' + keywords;
        }
        return false;
    });
});