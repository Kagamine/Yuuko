var Lock = false;
var CurrentPage = 0;
var CurrentPort;
var CurrentConditions;
var Debug;

String.prototype.replaceAll = function (reallyDo, replaceWith) {
    if (!RegExp.prototype.isPrototypeOf(reallyDo)) {
        return this.replace(new RegExp(reallyDo, "g"), replaceWith);
    } else {
        return this.replace(reallyDo, replaceWith);
    }
}

function LoadFromCollectionPort(port, conditions)
{
    if (Lock) return;
    Lock = true;
    try { yuukoOnLoading(); } catch (e) { }
    $.getJSON("/yuuko/gets/" + port, conditions, function (data) {
        Debug = data;
        if (data.length == 0)
        {
            Lock = true;
            try { yuukoOnEmptyCollection(); } catch (e) { }
        }
        $("[data-collection='" + port + "']").unbind().each(function () {
            var template = $(this).children('.template');
            var IdentifierField = $(this).attr("data-identifier");
            for (var i = 0; i < data.length; i++)
            {
                var NewDom = template.clone();
                $(NewDom).attr("id", port.toLocaleLowerCase() + "-collection-" + data[i][IdentifierField]);
                $(NewDom).removeClass(port.toLocaleLowerCase());
                $(NewDom).removeClass("template");
                for (var x in data[i])
                {
                    if (typeof (x) == "undefined") continue;
                    try { $(NewDom).find("[data-field='" + x + "']").val(data[i][x]); } catch (e) { }
                    try { $(NewDom).find("[data-field='" + x + "']").html(data[i][x]); } catch (e) { }
                    try { $(NewDom).find("[data-field='" + x + "']").text(data[i][x]); } catch (e) { }
                }
                var html = $(NewDom).html();
                for (var x in data[i]) {
                    if (typeof (x) == "undefined") continue;
                    html = html.replaceAll("$" + x, data[i][x]);
                    console.log(html);
                }
                $(NewDom).html(html);
                $(this).append(NewDom);
            }
            try { yuukoOnLoaded(); } catch (e) { }
        });
    });
}