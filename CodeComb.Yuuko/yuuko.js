var Collection = {};
var CollectionLocks = {};
var CollectionEvents = {};
var Detail = {};
var DetailLocks = {};
var DetailEvents = {};

String.prototype.replaceAll = function (reallyDo, replaceWith) {
    if (!RegExp.prototype.isPrototypeOf(reallyDo)) {
        return this.replace(new RegExp(reallyDo, "g"), replaceWith);
    } else {
        return this.replace(reallyDo, replaceWith);
    }
}

function LoadFromCollectionPort(port) {
    if (CollectionLocks[port] == null)
        CollectionLocks[port] = false;
    if (CollectionLocks[port])
        return;
    CollectionLocks[port] = true;
    try { CollectionEvents[port].onLoading(); } catch (e) { }
    $.getJSON("/yuuko/gets/" + port, Collection[port], function (data) {
        if (data.length == 0) {
            CollectionLocks[port] = true;
            try { CollectionEvents[port].onEmptyCollection(); } catch (e) { }
            return;
        }
        $("[data-collection='" + port + "']").unbind().each(function () {
            var template = $(this).children('.template');
            var IdentifierField = $(this).attr("data-identifier");
            for (var i = 0; i < data.length; i++) {
                var NewDom = template.clone();
                var html = $(NewDom).html();
                for (var x in data[i]) {
                    if (typeof (x) == "undefined") continue;
                    html = html.replaceAll("$" + port.toLocaleLowerCase() + "." + x, data[i][x]);
                }
                $(NewDom).html(html);
                $(NewDom).attr("id", port.toLocaleLowerCase() + "-collection-" + data[i][IdentifierField]);
                $(NewDom).removeClass(port.toLocaleLowerCase());
                $(NewDom).removeClass("template");
                for (var x in data[i]) {
                    if (typeof (x) == "undefined") continue;
                    try { $(NewDom).find("[data-field='" + port + "." + x + "']").val(data[i][x]); } catch (e) { }
                    try { $(NewDom).find("[data-field='" + port + "." + x + "']").html(data[i][x]); } catch (e) { }
                    try { $(NewDom).find("[data-field='" + port + "." + x + "']").text(data[i][x]); } catch (e) { }
                }
                $(this).append(NewDom);
            }
            CollectionLocks[port] = false;
            try { CollectionEvents[port].onLoaded(); } catch (e) { }
        });
    });
}

function CollectionPageTo(port, page) {
    if (Collection[port] == null) {
        Collection[port] = {};
        Collection[port].p = 0;
    }
    if (Collection[port].p == null)
        Collection[port].p = 0;
    Collection[port].p = page;
    $("[data-collection='" + port + "']").unbind().each(function () {
        var template = $(this).children(".template." + port).clone();
        $(this).html("");
        $(this).append(template);
    });
    LoadFromCollectionPort(port, page);
}

function CollectionFlowNext(port) {
    if (Collection[port] == null) {
        Collection[port] = {};
        Collection[port].p = 0;
    }
    else if (Collection[port].p == null)
        Collection[port].p = 0;
    else Collection[port].p++;
    LoadFromCollectionPort(port, Collection[port].p);
}

function ResetCollectionPort(port) {
    CollectionLocks[port] = false;
    Collection[port] = {};
    $("[data-collection='" + port + "']").unbind().each(function () {
        var template = $(this).children(".template." + port).clone();
        $(this).html("");
        $(this).append(template);
    });
}

function LoadFromDetailPort(port) {
    if (DetailLocks[port] == null)
        DetailLocks[port] = false;
    if (DetailLocks[port])
        return;
    DetailLocks[port] = true;
    try { DetailEvents[port].onLoading(); } catch (e) { }
    $.getJSON("/yuuko/gets/" + port, { k: Detail[port] }, function (data) {
        $("[data-detail='" + port + "']").unbind().each(function () {
            if ($(this).attr("data-method") == null || $(this).attr("data-method") != "insert") {
                var html = $(this).html();
                for (var x in data) {
                    if (typeof (x) == "undefined") continue;
                    html = html.replaceAll("$" + port.toLocaleLowerCase() + "." + x, data[x]);
                }
                $(this).html(html);
                for (var x in data) {
                    if (typeof (x) == "undefined") continue;
                    try { $(this).find("[data-field='" + port + "." + x + "']").val(data[x]); } catch (e) { }
                    try { $(this).find("[data-field='" + port + "." + x + "']").html(data[x]); } catch (e) { }
                    try { $(this).find("[data-field='" + port + "." + x + "']").text(data[x]); } catch (e) { }
                }
                DetailLocks[port] = false;
                try { DetailEvents[port].onLoaded(); } catch (e) { }
            }
        });
    });
}

function DetailKeyTo(port, key) {
    Detail[port] = key;
    DetailLocks[port] = false;
    LoadFromDetailPort(port);
}

function EditPort(port) {
    $("[data-detail='" + port + "']").unbind().each(function () {
        if ($(this).attr("data-method") != null && $(this).attr("data-method") == "edit") {
            var edit = { k: Detail[port], YuukoToken: YuukoToken };
            $(this).find("[data-field]").each(function () {
                if ($(this).attr("data-field").indexOf(port + ".") >= 0) {
                    if ($(this).val() != null)
                        edit[$(this).attr("data-field").replace(port + ".", "")] = $(this).val();
                }
            });
            $.post("/yuuko/sets/" + port + "/edit", edit, function (result) {
                try { Detail[port].onEdited(port, result); } catch (e) { }
            });
        }
    });
}

function DeletePort(port, key) {
    $.post("/yuuko/sets/" + port + "/delete", { k: key, YuukoToken: YuukoToken }, function (result) {
        try { Detail[port].onDeleted(key, result); } catch (e) { }
    });
}

function InsertPort(port) {
    $("[data-detail='" + port + "']").unbind().each(function () {
        if ($(this).attr("data-method") != null && $(this).attr("data-method") == "insert") {
            var insert = { YuukoToken: YuukoToken };
            $(this).find("[data-field]").each(function () {
                if ($(this).attr("data-field").indexOf(port + ".") >= 0) {
                    if ($(this).val() != null)
                        insert[$(this).attr("data-field").replace(port + ".", "")] = $(this).val();
                }
            });
            $.post("/yuuko/sets/" + port + "/insert", insert, function (result) {
                try { Detail[port].onInserted(result); } catch (e) { }
            });
        }
    });
}

$(window).scroll(function () {
    totalheight = parseFloat($(window).height()) + parseFloat($(window).scrollTop());
    if ($(document).height() <= totalheight) {
        $("[data-expression='flow']").unbind().each(function () {
            CollectionFlowNext($(this).attr("data-collection"));
        });
    }
});

$(document).ready(function () {
    $("[data-collection]").unbind().each(function () {
        LoadFromCollectionPort($(this).attr("data-collection"));
    });
    $("[data-detail]").unbind().each(function () {
        LoadFromDetailPort($(this).attr("data-detail"));
    });
});