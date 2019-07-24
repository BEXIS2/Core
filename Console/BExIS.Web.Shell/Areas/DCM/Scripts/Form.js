var minimapOriginalTop = 0;
$(document).ready(function (e) {
    setTimeout(
        function () {
            //do something special
            //console.log("doc ready before autosize");
            //console.log($('textarea'));
            if ($('textarea') !== null) {
                $($('textarea')).each(function (index, element) {
                    // element == this
                    autosize($(this));
                    //console.log("done autosize");
                });
            }
        }, 10);

    //setTabIndex();
    resetAllTelerikIconTitles();
});

function setTabIndex() {
    var list = $(".metadataAttributeInput .t-input");
    //console.log(list);
    for (var i = 0; i < list.length; i++) {
        var input = list[i];
        //console.log(input);
        $(input).attr("tabindex", i);
    }
}

/******************************************
 ********* FORM    ************************
 ******************************************/

$(window).scroll(function () {
    bindMinimap();
});

var originalMinimapTop = 0;
var originalMiniRegionTop = 0;
function bindMinimap(create) {
    var scrollpostion = $(document).scrollTop();
    var topContainer = $('#root').position().top;
    var menubar = $(".navbar").height() + 20;

    var hContainer = $('#root').height();
    var hWindow = $(window).height();

    var hRatio = 1 - hWindow / hContainer;

    if (hRatio <= 0) hRatio = 0.1;

    if ($(".minimap").length === 0 || create) {
        var offset = getRatioHeight(menubar);

        $(".miniregion").remove();
        $(".minimap").remove();

        var previewBody = $('#root')
            .minimap(
                {
                    heightRatio: hRatio,
                    widthRatio: 0.095,
                    offsetHeightRatio: offset,
                    offsetWidthRatio: 0.02,
                    position: "right",
                    touch: true,
                    smoothScroll: false,
                    smoothScrollDelay: 100
                });
        var x = $(".minimap").css("top");
        originalMinimapTop = parseInt(x.split("px"));
        originalMiniRegionTop = $(".miniregion").position().top;

        $(".minimap").css("top", originalMinimapTop + (topContainer - menubar));
        //$(".miniregion").css("top", originalMiniRegionTop + (topContainer - menubar));

        //console.log("created");
    }

    var scrollmax = (topContainer - menubar) - scrollpostion;
    //console.log(topContainer);
    //console.log(menubar);
    //console.log(scrollpostion);
    //console.log(scrollmax);
    //console.log(originalMinimapTop);

    if ((topContainer - scrollpostion) <= menubar) {
        scrollmax = 0;
        //console.log("setty");
        $(".miniregion").removeClass("hidden");
    } else {
        $(".miniregion").addClass("hidden");
        //$(".miniregion").css("top", originalMiniRegionTop + (topContainer - menubar));
    }
    //console.log(scrollmax);

    ////var miniregionoffset = topContainer - originalMiniRegionTop;
    var positionMinimap = parseInt(originalMinimapTop) + parseInt(scrollmax);

    $(".minimap").css("top", positionMinimap);
}

function getRatioHeight(containerStart) {
    return (containerStart / $(window).height());
}

/******************************************
 ********* ATTIBUTE************************
 ******************************************/

function metadataAttributeOnLoad(e, hasErrors) {
    if (hasErrors)
        $('#' + e.id + "_input").AddClass("bx-input-error");
}

function OnKeyUpTextInput(e) {
    //console.log("OnKeyDownTextInput");
    //console.log(e.id);
    //console.log(e.value.length);
    //console.log(e.value);

    var length = e.value.length;

    if (length >= 60) {
        //console.log("start replace");
        var textarea = inputToTextArea(e);
        //console.log(textarea);
        $("#" + e.id).replaceWith(textarea);

        $(textarea).trigger("change");

        //set focus
        var tmp = $("#" + e.id).val();
        $("#" + e.id).val('');
        autosize($("#" + e.id));
        $("#" + e.id).val(tmp);
        $("#" + e.id).focus();

        //console.log("done");
    }
}

function inputToTextArea(input) {
    //<input    id="310_161_1_1_1_286_Input" name="310_161_1_1_1_286_Input" onkeyup="OnKeyUpTextInput(this)"                   title="" class="t-widget t-autocomplete t-input bx-metadataFormTextInput"  type="text" value="dassssss" autocomplete="off">
    //<textarea id="310_161_1_1_1_380_Input" name="Title"                   onchange="OnChange(this)" packageid="161"          title="" class="bx-textarea bx-metadataFormTextInput"                      cols="20" rows="2" style="overflow: hidden; word-wrap: break-word; resize: horizontal; height: 75px;">dassssssggggggggggggggggggggggggggggggggggggggggggggggggggggg</textarea>
    var textarea = "<textarea " +
        "id='" + $("#" + input.id).attr("id") + "'" +
        "name='" + $("#" + input.id).attr("name") + "'" +
        "onchange=\"OnChange(this)\"" +
        "onkeyup= \"OnKeyUpTextArea(this)\"" +
        "title='" + $("#" + input.id).attr("title") + "'" +
        "class=\"bx-textarea bx-metadataFormTextInput \"" +
        "cols=\"2\" rows=\"2\">" + input.value + "</textarea>";

    return textarea;
}

function OnKeyUpTextArea(e) {
    //console.log("OnKeyDownTextArea");
    //console.log(e.id);
    //console.log(e.value.length);
    //console.log(e.value);

    var length = e.value.length;

    if (length < 60) {
        //console.log("start replace");
        var input = textareaToInput(e);
        //console.log(input);
        $("#" + e.id).replaceWith(input);

        //set focus
        var tmp = $("#" + e.id).value;
        $("#" + e.id).value = '';
        $("#" + e.id).value = tmp;
        $("#" + e.id).focus();

        //console.log("done");

        autosize($('textarea'));
    }
}

function textareaToInput(textarea) {
    //<input    id="310_161_1_1_1_286_Input" name="310_161_1_1_1_286_Input" onkeyup="OnKeyUpTextInput(this)"                   title="" class="t-widget t-autocomplete t-input bx-metadataFormTextInput"  type="text" value="dassssss" autocomplete="off">
    //<textarea id="310_161_1_1_1_380_Input" name="Title"                   onchange="OnChange(this)" packageid="161"          title="" class="bx-textarea bx-metadataFormTextInput"                      cols="20" rows="2" style="overflow: hidden; word-wrap: break-word; resize: horizontal; height: 75px;">dassssssggggggggggggggggggggggggggggggggggggggggggggggggggggg</textarea>
    var input = "<input " +
        "id='" + $("#" + textarea.id).attr("id") + "'" +
        "name='" + $("#" + textarea.id).attr("name") + "'" +
        "onkeyup=\"OnKeyUpTextInput(this)\"" +
        "title='" + $("#" + textarea.id).attr("title") + "'" +
        "class=\"t-widget t-autocomplete t-input bx-metadataFormTextInput \"" +
        "value='" + $("#" + textarea.id).val() + "' " +
        "autocomplete=\"off\"></input>";

    return input;
}

var afterClosed = false;

function OnChangeTextInput(e) {
    var substr = e.target.id.split('_');
    var id = substr[0];
    var parentid = substr[1];
    var parentname = $("#" + e.id).attr("title");
    var number = substr[2];
    var ParentModelNumber = substr[3];
    var ParentStepID = substr[5];

    //alert(parentid);
    //alert(metadataStructureId);
    //alert(ParentStepID);
    //object value,  int id, int parentid,       string parentname,     int number, int parentModelNumber,                    int parentStepId)
    $.post('/DCM/Form/ValidateMetadataAttributeUsage',
        {
            value: e.value,
            id: id,
            parentid: parentid,
            parentname: parentname,
            number: number,
            parentModelNumber: ParentModelNumber,
            ParentStepId: ParentStepID
        },
        function (response) {
            var id = e.target.id;
            //console.log("OnChangeTextInput");
            //console.log(id);

            var index = id.lastIndexOf("_");
            var newId = id.substr(0, index);
            //console.log(newId);

            $("#" + newId).replaceWith(response);

            //alert("test");
            autosize($('textarea'));

            //check if the parent is set to a party
            console.log("after change");
            var parent = $("#" + ParentStepID)[0];
            console.log(parent);
            var partyid = $(parent).attr("partyid");
            console.log(partyid);

            var partyidConverted = TryParseInt(partyid, null)
            console.log("tryparse:" + partyidConverted)

            //delete party informations when a party was selected before
            if (partyidConverted !== null && partyidConverted > 0 && afterClosed === false) {
                console.log(ParentStepID);
                console.log(ParentModelNumber);

                UpdateWithParty(ParentStepID, ParentModelNumber, 0);
            }
            else {
                afterClosed = false;
            }
        })
}

function OnChange(e) {
    //console.log("OnChange");
    var substr = e.id.split('_');
    var id = substr[0];
    var parentid = substr[1];
    var parentname = $("#" + e.id).attr("title");
    var number = substr[2];
    var ParentModelNumber = substr[3];
    var ParentStepID = substr[5];

    //alert(parentid);
    //alert(metadataStructureId);
    //alert(ParentStepID);
    //object value,  int id, int parentid,       string parentname,     int number, int parentModelNumber,                    int parentStepId)
    $.post("/DCM/Form/ValidateMetadataAttributeUsage",
        {
            value: e.value,
            id: id,
            parentid: parentid,
            parentname: parentname,
            number: number,
            parentModelNumber: ParentModelNumber,
            ParentStepId: ParentStepID
        },
        function (response) {
            var index = e.id.lastIndexOf("_");
            var newId = e.id.substr(0, index);

            $("#" + newId).replaceWith(response);
            if ($('textarea') !== null) {
                autosize($('textarea'));
            }
        });
}

function OnChangeCheckBox(e) {
    var substr = e.id.split('_');
    var id = substr[0];
    var parentid = substr[1];
    var parentname = $("#" + e.id).attr("title");
    var number = substr[2];
    var ParentModelNumber = substr[3];
    var ParentStepID = substr[5];

    var value;

    if ($("#" + e.id).attr('checked')) {
        value = true;
    } else {
        value = false;
    }

    var data = {
        value: e.value,
        id: id,
        parentid: parentid,
        parentname: parentname,
        number: number,
        parentModelNumber: ParentModelNumber,
        ParentStepId: ParentStepID
    };

    //alert(parentid);
    //alert(metadataStructureId);
    //alert(ParentStepID);
    //object value,  int id, int parentid,       string parentname,     int number, int parentModelNumber,                    int parentStepId)
    $.post("/DCM/Form/ValidateMetadataAttributeUsage",
        {
            value: value,
            id: id,
            parentid: parentid,
            parentname: parentname,
            number: number,
            parentModelNumber: ParentModelNumber,
            ParentStepId: ParentStepID
        },
        function (response) {
            //alert(e.value);
            //alert(response);
            //var id = parentid;
            var index = e.id.lastIndexOf("_");
            var newId = e.id.substr(0, index);
            //alert(newId);

            $("#" + newId).replaceWith(response);
        })
}

function OnChangeDropDown(e) {
    var substr = e.target.id.split('_');
    var id = substr[0];
    var parentid = substr[1];
    var parentname = $("#" + e.id).attr("title");
    var number = substr[2];
    var ParentModelNumber = substr[3];
    var ParentStepID = substr[5];

    var data = {
        value: e.value,
        id: id,
        parentid: parentid,
        parentname: parentname,
        number: number,
        parentModelNumber: ParentModelNumber,
        ParentStepId: ParentStepID
    };

    //alert(parentid);
    //alert(metadataStructureId);
    //alert(ParentStepID);
    //object value,  int id, int parentid,       string parentname,     int number, int parentModelNumber,                    int parentStepId)
    $.post("/DCM/Form/ValidateMetadataAttributeUsage",
        {
            value: e.value,
            id: id,
            parentid: parentid,
            parentname: parentname,
            number: number,
            parentModelNumber: ParentModelNumber,
            ParentStepId: ParentStepID
        },
        function (response) {
        })
}

function OnChangeNumbers(e) {
    var substr = e.target.id.split('_');
    var id = substr[0];
    var parentid = substr[1];
    var parentname = ""//e.target.attr("title");
    var metadataStructureId = substr[2];
    var number = substr[3];
    var ParentModelNumber = substr[4];
    var ParentStepID = substr[5];

    var data = {
        value: e.target.value,
        id: id,
        parentid: parentid,
        parentname: parentname,
        number: number,
        ParentModelNumber: ParentModelNumber,
        ParentStepId: ParentStepID
    };

    //alert(id);
    //alert(parentid);
    //alert(metadataStructureId);
    //alert(number);

    $.ajax({
        url: '/DCM/Form/ValidateMetadataAttributeUsage',
        type: 'POST',
        data: JSON.stringify(data),
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        error: function (xhr) {
            //alert('Error: ' + xhr.statusText);
        },
        success: function (result) {
            //alert("success");
        },
        async: true,
        processData: false
    });
}

function OnChangeDatePicker(e) {
    var substr = e.target.id.split('_');
    var id = substr[0];
    var parentid = substr[1];
    var parentname = $("#" + e.id).attr("title");
    //var metadataStructureId = substr[2];
    var number = substr[2];
    var ParentModelNumber = substr[3];
    var ParentStepID = substr[5];

    var data = {
        value: e.value,
        id: id,
        parentid: parentid,
        parentname: parentname,
        number: number,
        ParentModelNumber: ParentModelNumber,
        ParentStepId: ParentStepID
    };

    //alert(id);
    //alert(parentid);
    //alert(metadataStructureId);
    //alert(number);

    $.ajax({
        url: '/DCM/Form/ValidateMetadataAttributeUsage',
        type: 'POST',
        data: JSON.stringify(data),
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        error: function (xhr) {
            //alert('Error: ' + xhr.statusText);
        },
        success: function (result) {
            //alert("success");
        },
        async: true,
        processData: false
    });
}

function OnClickAdd(e, max) {
    var substr = e.id.split('_');
    var id = substr[0];
    var parentid = substr[1];
    var number = substr[2];
    var ParentModelNumber = substr[3];
    var NumberOfSourceInPackage = substr[4];
    var ParentStepID = substr[5];

    var maxcardinality = max;

    if (NumberOfSourceInPackage <= parseInt(maxcardinality)) {
        $.post('/DCM/Form/AddMetadataAttributeUsage',
            {
                id: id,
                parentid: parentid,
                number: number,
                parentModelNumber: ParentModelNumber,
                ParentStepId: ParentStepID
            },
            function (response) {
                var id = ParentStepID;
                //alert(id);
                //alert(response);
                $("#" + id).replaceWith(response);

                resetAllTelerikIconTitles();
            });
    } else {
        alert("Maxium cardinality is reached.");
    }
}

function OnClickRemove(e) {
    var value = $("#" + e.id).closest(".ValueClass").value;

    if (value !== "") {
        var substr = e.id.split('_');
        var id = substr[0];
        var parentid = substr[1];
        var number = substr[2];
        var ParentModelNumber = substr[3];
        var NumberOfSourceInPackage = substr[4];
        var ParentStepID = substr[5];

        if (NumberOfSourceInPackage > 1) {
            var data = {
                value: value,
                id: id,
                parentid: parentid,
                number: number,
                ParentModelNumber: ParentModelNumber
            };

            $
                .post('/DCM/Form/RemoveMetadataAttributeUsage',
                    {
                        id: id,
                        parentid: parentid,
                        number: number,
                        parentModelNumber: ParentModelNumber,
                        ParentStepId: ParentStepID
                    },
                    function (response) {
                        $("#" + ParentStepID).replaceWith(response);
                    });
        } else {
            alert("You are not able to remove elements.");
        }
    }
}

function OnClickUp(e) {
    //alert(value);
    var value = $("#" + e.id).closest(".ValueClass").value;
    if (value !== "") {
        var substr = e.id.split('_');
        var id = substr[0];
        var parentid = substr[1];
        var number = substr[2];
        var ParentModelNumber = substr[3];
        var ParentStepID = substr[5];

        $.post('/DCM/Form/UpMetadataAttributeUsage',
            {
                id: id,
                parentid: parentid,
                number: number,
                ParentModelNumber: ParentModelNumber,
                ParentStepId: ParentStepID
            },
            function (response) {
                var id = ParentStepID;
                $("#" + id).replaceWith(response);
            });
    }
}

function OnClickDown(e) {
    var value = $("#" + e.id).closest(".ValueClass").value;
    if (value !== "") {
        var substr = e.id.split('_');
        var id = substr[0];
        var parentid = substr[1];
        var number = substr[2];
        var ParentModelNumber = substr[3];
        var ParentStepID = substr[5];

        var data = {
            value: value,
            id: id,
            parentid: parentid,
            number: number,
            ParentModelNumber: ParentModelNumber
        };

        $.post('/DCM/Form/DownMetadataAttributeUsage',
            {
                id: id,
                parentid: parentid,
                number: number,
                ParentModelNumber: ParentModelNumber,
                ParentStepId: ParentStepID
            },
            function (response) {
                var id = ParentStepID;
                $("#" + id).replaceWith(response);
            })
    }
}

function OnClose(e) {
    console.log(e.target.value);
    var value = e.target.value;

    var type = value.substring(value.lastIndexOf('_'));

    // if the autocomplete type a partycustm type
    if (type === "PartyCustomType") {
        if (~value.indexOf("(") && ~value.indexOf(")")) {
            var start = value.lastIndexOf("(") + 1;
            var end = value.lastIndexOf(")");
            var partyid = value.substr(start, end - start);

            console.log("partyid = " + partyid);

            if (partyid !== "0") {
                // find parent

                var parent = $(e.target).parents(".metadataCompountAttributeUsage")[0];
                console.log("parent");
                console.log(parent);

                if (parent !== null) {
                    var parentid = $(parent).attr("id");
                    var number = $(parent).attr("number");
                    UpdateWithParty(parentid, number, partyid);
                }

                afterClosed = true;
            }
        }
    }

    // if the autocomplete type a Entity
    if (type === "Entity") {
    }
}

/******************************************
 ********* Component************************
 ******************************************/

function UpdateWithParty(componentId, number, partyid) {
    console.log("update with party");
    console.log(componentId + "-" + number + "-" + partyid);

    $("#" + componentId).find(".metadataAttributeInput").each(function () {
        $(this).preloader(12, "...loading");
    })

    $.post('/DCM/Form/UpdateComplexUsageWithParty',
        {
            stepId: componentId,
            number: number,
            partyId: partyid
        },
        function (response) {
            console.log(componentId);
            //console.log(response);

            $("#" + componentId).replaceWith(response);
            // update party id to component
            $("#" + componentId).attr("partyid", partyid);
            //alert("test");
            autosize($('textarea'));
        })
}

function Add(e) {
    var temp = e.id;
    var parentId = temp.split("_")[0];
    var number = temp.split("_")[1];

    //alert(e.id);
    //alert(parentId);
    $.get('/DCM/Form/AddComplexUsage',
        { parentStepId: parentId, number: number },
        function (response) {
            //alert(parentId);
            $('#' + parentId).replaceWith(response);
            resetAllTelerikIconTitles();
            bindMinimap(true);
        })
}

function Remove(e) {
    var temp = e.id;
    var parentId = temp.split("_")[0];
    var number = temp.split("_")[1];

    $.get('/DCM/Form/RemoveComplexUsage',
        { parentStepId: parentId, number: number },
        function (response) {
            $('#' + parentId).replaceWith(response);
            bindMinimap(true);
        })
}

function Up(e) {
    var temp = e.id;
    var parentId = temp.split("_")[0];
    var number = temp.split("_")[1];

    $.get('/DCM/Form/UpComplexUsage',
        { parentStepId: parentId, number: number },
        function (response) {
            $('#' + parentId).replaceWith(response);
            bindMinimap(true);
        })
}

function Down(e) {
    var temp = e.id;
    var parentId = temp.split("_")[0];
    var number = temp.split("_")[1];

    $.get('/DCM/Form/DownComplexUsage',
        { parentStepId: parentId, number: number },
        function (response) {
            $('#' + parentId).replaceWith(response);
            bindMinimap(true);
        })
}

function Activate(e) {
    var temp = e.id;
    var stepid = temp.split("_")[0];
    var active = $(e).hasClass("bx-check-square-o");

    $.get('/DCM/Form/ActivateComplexUsage',
        {
            id: stepid
        },
        function (response) {
            console.log(response);

            $('#' + stepid).replaceWith(response);
            if (!active) {
                $('html, body').animate({
                    scrollTop: $('#' + stepid).offset().top - 80
                }, 'slow');
            }

            resetAllTelerikIconTitles();
            bindMinimap(true);
        });
}

function ActivateFromChoice(e) {
    var tmp = e.id;
    var v = tmp.split("_")[0];
    var temp = $(e).attr("name");
    var stepid = temp.split("_")[0];
    var active = $(e).hasClass("bx-dot-circle-o");

    $.get('/DCM/Form/ActivateComplexUsageInAChoice',
        {
            parentid: stepid, id: v
        },

        function (response) {
            $('#' + stepid).replaceWith(response);

            if (!active) {
                $('html, body').animate({
                    scrollTop: $('#' + stepid).offset().top - 70
                }, 'slow');
            }

            resetAllTelerikIconTitles();
            bindMinimap(true);
        });
}

function showHideClick(e) {
    var temp = e.id;
    var parentId = temp.split("_")[0];
    var number = temp.split("_")[1];

    var id = parentId + "_" + number + "_Container";
    var buttonId = parentId + "_" + number + "_ButtonView";
    $('#' + id).toggle();
    $('#' + buttonId).toggleClass("bx-angle-double-up bx-angle-double-down");
    bindMinimap(true);
}

function TryParseInt(str, defaultValue) {
    var retValue = defaultValue;
    if (str !== null && str !== undefined) {
        if (str.length > 0) {
            if (!isNaN(str)) {
                retValue = parseInt(str);
            }
        }
    }
    return retValue;
}

/******************************************
 ********* HELP ***************************
 ******************************************/

function showHelp(id) {
    $("#" + id).toggle();
}

function showHelpAll() {
    $(".help").show();
    $("#show_help_all").hide();
    $("#hide_help_all").show();
}

function hideHelpAll() {
    $(".help").hide();
    $("#show_help_all").show();
    $("#hide_help_all").hide();
}