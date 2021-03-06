﻿function LoadFromCollectionPort(port) {
    if ((null == $("[data-collection='" + port + "']").attr("data-method") || "insert" != $("[data-collection='" + port + "']").attr("data-method")) && (null == CollectionLocks[port] && (CollectionLocks[port] = !1), !CollectionLocks[port])) {
        CollectionLocks[port] = !0, "flow" == $("[data-collection='" + port + "']").attr("data-expression") && (null == Collection[port] ? Collection[port] = {
            p: 0
        } : null == Collection[port].p && (Collection[port].p = 0));
        try {
            CollectionEvents[port].onLoading()
        } catch (e) { }
        $.getJSON("/yuuko/gets/" + port, Collection[port], function (data) {
            if (0 != data.length) $("[data-collection='" + port + "']").unbind().each(function () {
                for (var template = $(this).find(".template." + port.toLocaleLowerCase()), IdentifierField = $(this).attr("data-identifier"), i = 0; i < data.length; i++) {
                    var html = $(template)[0].outerHTML.toString();
                    for (var x in data[i]) "undefined" != typeof x && (html = html.replaceAll("\\$" + port + "." + x, data[i][x]));
                    var NewDom = $(html);
                    null != data[i][IdentifierField] && NewDom.attr("id", port.toLocaleLowerCase() + "-collection-" + data[i][IdentifierField]), NewDom.removeClass(port.toLocaleLowerCase()), NewDom.removeClass("template");
                    for (var x in data[i]) if ("undefined" != typeof x) {
                        if (null == NewDom.find("[data-field='" + port + "." + x + "']").val()) try {
                            NewDom.find("[data-field='" + port + "." + x + "']").val(data[i][x].toString())
                        } catch (e) { }
                        try {
                            NewDom.find("[data-field='" + port + "." + x + "']").text(data[i][x].toString())
                        } catch (e) { }
                        try {
                            NewDom.find("[data-field='" + port + "." + x + "']").html(data[i][x].toString())
                        } catch (e) { }
                        if (NewDom.attr("data-field") == port + "." + x) {
                            if (null == NewDom.val()) try {
                                NewDom.val(data[i][x].toString())
                            } catch (e) { }
                            if (null == NewDom.attr("data-collection") && null == NewDom.attr("data-detail")) {
                                try {
                                    NewDom.text(data[i][x].toString())
                                } catch (e) { }
                                try {
                                    NewDom.html(data[i][x].toString())
                                } catch (e) { }
                            }
                        }
                    }
                    $(this).append(NewDom)
                }
                CollectionLocks[port] = !1;
                try {
                    CollectionEvents[port].onLoaded()
                } catch (e) { }
            });
            else {
                CollectionLocks[port] = !0;
                try {
                    CollectionEvents[port].onEmptyCollection()
                } catch (e) { }
            }
        })
    }
}
function CollectionPageTo(port, page) {
    null == Collection[port] && (Collection[port] = {}, Collection[port].p = 0), null == Collection[port].p && (Collection[port].p = 0), Collection[port].p = page, $("[data-collection='" + port + "']").unbind().each(function () {
        var template = $(this).children(".template." + port).clone();
        $(this).html(""), $(this).append(template)
    }), LoadFromCollectionPort(port, page)
}
function CollectionFlowNext(port) {
    null == Collection[port] ? (Collection[port] = {}, Collection[port].p = 0) : null == Collection[port].p ? Collection[port].p = 0 : Collection[port].p++, LoadFromCollectionPort(port, Collection[port].p)
}
function ResetCollectionPort(port) {
    CollectionLocks[port] = !1, Collection[port] = {}, $("[data-collection='" + port + "']").unbind().each(function () {
        var template = $(this).find(".template." + port).clone();
        $(this).html(""), $(this).append(template)
    })
}
function LoadFromDetailPort(port) {
    if (null == DetailLocks[port] && (DetailLocks[port] = !1), !DetailLocks[port]) {
        DetailLocks[port] = !0;
        try {
            DetailEvents[port].onLoading()
        } catch (e) { }
        $.getJSON("/yuuko/gets/" + port, {
            k: Detail[port]
        },
        function (data) {
            var detail = DetailTemplates[port].toString();
            for (var x in data) "undefined" != typeof x && (detail = detail.replaceAll("\\$" + port + "." + x, data[x]));
            detail = $(detail);
            $("[data-detail='" + port + "']")[0].outerHTML = detail[0].outerHTML;
            detail = $("[data-detail='" + port + "']");
            for (var x in data) if ("undefined" != typeof x) {
                try {
                    detail.find("select[data-field='" + port + "." + x + "']").append($("<option>Current</option>").val(data[x]));
                }
                catch (e) { }
                try {
                    detail.find("[data-field='" + port + "." + x + "']").val(data[x].toString())
                } catch (e) { }
                if (null == detail.find("[data-field='" + port + "." + x + "']").attr("data-collection") && null == detail.find("[data-field='" + port + "." + x + "']").attr("data-detail")) {
                    try {
                        detail.find("[data-field='" + port + "." + x + "']").text(data[x].toString())
                    } catch (e) { }
                    try {
                        detail.find("[data-field='" + port + "." + x + "']").html(data[x].toString())
                    } catch (e) { }
                }
            }
            DetailLocks[port] = !1;
            try {
                DetailEvents[port].onLoaded()
            } catch (e) { }
        });
    }
}
function DetailKeyTo(port, key) {
    Detail[port] = key, DetailLocks[port] = !1, LoadFromDetailPort(port)
}
function EditPort(port) {
    try {
        DetailEvents[port].onEditing()
    } catch (e) { }
    $("[data-detail='" + port + "']").unbind().each(function () {
        if (null != $(this).attr("data-method") && "edit" == $(this).attr("data-method")) {
            var edit = {
                k: Detail[port],
                YuukoToken: YuukoToken
            };
            $(this).find("[data-field]").each(function () {
                $(this).attr("data-field").indexOf(port + ".") >= 0 && null != $(this).val() && (edit[$(this).attr("data-field").replace(port + ".", "")] = $(this).val())
            });
            $.post("/yuuko/sets/" + port + "/edit", edit, function (result) {
                try {
                    DetailEvents[port].onEdited(port, result);
                } catch (e) { }
            })
        }
    })
}
function DeletePort(port, key) {
    try {
        DetailEvents[port].onDeleting()
    } catch (e) { }
    $.post("/yuuko/sets/" + port + "/delete", {
        k: key,
        YuukoToken: YuukoToken
    }, function (result) {
        try {
            DetailEvents[port].onDeleted(key, result)
        } catch (e) { }
    })
}
function InsertPort(port) {
    try {
        DetailEvents[port].onInserting()
    } catch (e) { }
    $("[data-detail='" + port + "']").unbind().each(function () {
        if (null != $(this).attr("data-method") && "insert" == $(this).attr("data-method")) {
            var insert = {
                YuukoToken: YuukoToken
            };
            $(this).find("[data-field]").each(function () {
                $(this).attr("data-field").indexOf(port + ".") >= 0 && null != $(this).val() && (insert[$(this).attr("data-field").replace(port + ".", "")] = $(this).val())
            }), $.post("/yuuko/sets/" + port + "/insert", insert, function (result) {
                try {
                    DetailEvents[port].onInserted(result)
                } catch (e) { }
            })
        }
    })
}
var Collection = {},
	CollectionLocks = {},
	CollectionEvents = {},
	Detail = {},
	DetailLocks = {},
	DetailEvents = {},
	DetailTemplates = {};

String.prototype.replaceAll = function (str1, str2) {
    return this.replace(new RegExp(str1, "gm"), str2)
}, $(window).scroll(function () {
    totalheight = parseFloat($(window).height()) + parseFloat($(window).scrollTop()), $(document).height() <= totalheight && $("[data-expression='flow']").unbind().each(function () {
        CollectionFlowNext($(this).attr("data-collection"))
    })
}), $(document).ready(function () {
    $("[data-detail]").unbind().each(function () {
        if ($(this).attr("data-method") == null || $(this).attr("data-method") != "insert") {
            DetailTemplates[$(this).attr("data-detail")] = $(this)[0].outerHTML;
            LoadFromDetailPort($(this).attr("data-detail"));
        }
    });
    $("[data-collection]").unbind().each(function () {
        LoadFromCollectionPort($(this).attr("data-collection"))
    });
});