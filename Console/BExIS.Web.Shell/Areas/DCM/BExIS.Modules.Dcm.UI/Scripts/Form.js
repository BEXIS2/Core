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
    console.log("test");
    getId();
});


function getId() {
    console.log("load form.js");
    const metadataContainer = document.getElementById("MetadataEditor");
    console.log("metadataContainer", metadataContainer);
    if (metadataContainer) { // Check if the element exists
        entityId = metadataContainer.getAttribute("entityid"); // Get the value of the "data-value" attribute
        console.log("entityId", entityId); // Output the value (e.g., "123")
    }

    return entityId;
}

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
    if ($("#MetadataEditor").is(':visible')) {
        bindMinimap();
    }
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

    //console.log("position : " + positionMinimap + "(topContainer - scrollpostion) :" + (topContainer - scrollpostion) + "menubar : " + menubar);

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

// check wheter input field is to small for the incoming input
// if its to small change to text area
function OnKeyUpTextInput(e) {
    //console.log("OnKeyUpTextInput");
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

function OnChangeTextInput(e, ui) {
    console.log("change");

    var value;

    if (ui.item === null) {
        value = e.target.value;
    }
    else {
        value = ui.item.value;
    }

    //console.log(value);
    var substr = e.target.id.split('_');
    var id = substr[0];
    var parentid = substr[1];
    var parentname = $("#" + e.id).attr("title");
    var number = substr[2];
    var ParentModelNumber = substr[3];
    var ParentStepID = substr[5];

    var isMappingSelection = e.target.getAttribute("isMappingSelection");

    //console.log("substr", substr);
    //console.log("id", id);
    //console.log("parentid", parentid);
    //console.log("parentname", parentname);
    //console.log("number", number);
    //console.log("ParentModelNumber", ParentModelNumber);
    //console.log("ParentStepID", ParentStepID);
    console.log("isMappingSelection", isMappingSelection);

    // after close a autocomplete there is a id in the value,
    // this should be removed before send to the server
    if (afterClosed === true) {
        if (~value.indexOf("(") && ~value.indexOf(")")) {
            var start = value.lastIndexOf("(") + 1;
            value = value.substr(0, start - 2);

            console.log("--> after autocomplete the value from the selection needs to be cutted");
        }
    }

    //alert(parentid);
    //alert(metadataStructureId);
    //alert(ParentStepID);
    //object value,  int id, int parentid,       string parentname,     int number, int parentModelNumber,                    int parentStepId)
    $.post('/DCM/Form/ValidateMetadataAttributeUsage',
        {
            value: value,
            id: id,
            parentid: parentid,
            parentname: parentname,
            number: number,
            parentModelNumber: ParentModelNumber,
            ParentStepId: ParentStepID,
            entityId
        },
        function (response) {
            console.log(response);

            // after the on close event from the autocomplete component, the values change in the input fields
            // after this changes again this change event is triggered
            // to prevent this, a flag is set to check wheter this event is fired after a close event or not
            if (afterClosed === false) {
                //console.log("after validate value on server");
                //console.log("afterClosed : " + afterClosed);
                //console.log("if : " + (afterClosed === false));

                var id = e.target.id;
                //console.log("OnChangeTextInput");

                console.log("id", id);

                var index = id.lastIndexOf("_");
                var newId = id.substr(0, index);
                console.log("newId", newId);

                $("#" + newId).replaceWith(response);
                //updateHeader();

                //alert("test");
                autosize($('textarea'));

                //console.log("--> only runs when autocomplete is not used");

                //check if the parent is set to a party
                console.log("after change");
                var parent = $("#" + ParentStepID)[0];
                console.log(parent);
                var partyid = $(parent).attr("partyid");
                console.log(partyid);

                var partyidConverted = TryParseInt(partyid, null);
                console.log("tryparse:" + partyidConverted)
                console.log("partyid", partyid);
                console.log("partyidConverted", partyidConverted);

                //delete party information when a party was selected before
                if (partyidConverted !== null && partyidConverted > 0 && afterClosed === false && isMappingSelection !== null) {
                    console.log("go delete it");
                    //console.log(ParentStepID);
                    //console.log(ParentModelNumber);

                    UpdateWithParty(ParentStepID, ParentModelNumber, 0);
                }
                else {
                    afterClosed = false;
                }
            }
            else {
                afterClosed = false;
            }
        })

    // reset after close flag
    //afterClosed = false
}

function OnChange(e) {
    console.log("OnChange");
    var substr = e.id.split('_');
    var id = substr[0];
    var parentid = substr[1];
    var parentname = $("#" + e.id).attr("title");
    var number = substr[2];
    var ParentModelNumber = substr[3];
    var ParentStepID = substr[5];

    $.post("/DCM/Form/ValidateMetadataAttributeUsage",
        {
            value: e.value,
            id: id,
            parentid: parentid,
            parentname: parentname,
            number: number,
            parentModelNumber: ParentModelNumber,
            ParentStepId: ParentStepID,
            entityId
        },
        function (response) {
            var index = e.id.lastIndexOf("_");
            var newId = e.id.substr(0, index);

            console.log("OnChange", response);
            $("#" + newId).replaceWith(response);

            updateHeader();

            if ($('textarea') !== null) {
                autosize($('textarea'));
            }
        });
}

function OnChangeParameter(e) {
    //console.log("OnChange");
    var substr = e.id.split('_');
    var id = substr[0];
    var parentid = substr[1];
    var number = substr[2];
    var ParentModelNumber = substr[3];
    var ParentStepID = substr[5];

    $.post("/DCM/Form/ValidateMetadataParameterUsage",
        {
            value: e.value,
            id: id,
            attrUsageId: parentid,
            number: number,
            parentModelNumber: ParentModelNumber,
            ParentStepId: ParentStepID,
            entityId
        },
        function (response) {
            var index = e.id.lastIndexOf("_");
            var newId = e.id.substr(0, index);

            console.log("OnChangeParameter", response);

            $("#" + newId).replaceWith(response);

            updateHeader();

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

    $.post("/DCM/Form/ValidateMetadataAttributeUsage",
        {
            value: value,
            id: id,
            parentid: parentid,
            parentname: parentname,
            number: number,
            parentModelNumber: ParentModelNumber,
            ParentStepId: ParentStepID,
            entityId
        },
        function (response) {
            //alert(e.value);
            //alert(response);
            //var id = parentid;
            var index = e.id.lastIndexOf("_");
            var newId = e.id.substr(0, index);
            //alert(newId);

            $("#" + newId).replaceWith(response);
            updateHeader();
        });
}
function OnChangeParameterCheckBox(e) {
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

    $.post("/DCM/Form/ValidateMetadataParameterUsage",
        {
            value: value,
            id: id,
            attrUsageId: parentid,
            number: number,
            parentModelNumber: ParentModelNumber,
            ParentStepId: ParentStepID,
            entityId
        },
        function (response) {
            var index = e.id.lastIndexOf("_");
            var newId = e.id.substr(0, index);

            $("#" + newId).replaceWith(response);
            updateHeader();
        });
}

function OnChangeDropDown(e) {
    var idParentDiv = $(this).attr("id");
    var substr = e.target.id.split('_');
    var id = substr[0];
    var parentid = substr[1];
    var parentname = $("#" + e.id).attr("title");
    var number = substr[2];
    var ParentModelNumber = substr[3];
    var ParentStepID = substr[5];

    $.post("/DCM/Form/ValidateMetadataAttributeUsage",
        {
            value: e.value,
            id: id,
            parentid: parentid,
            parentname: parentname,
            number: number,
            parentModelNumber: ParentModelNumber,
            ParentStepId: ParentStepID,
            entityId
        },
        function (response) {
            var index = idParentDiv.lastIndexOf("_");
            var newId = idParentDiv.substr(0, index);

            $("#" + newId).replaceWith(response);

            updateHeader();
        });
};
function OnChangeParameterDropDown(e) {
    var idParentDiv = $(this).attr("id");
    var substr = e.target.id.split('_');
    var id = substr[0];
    var parentid = substr[1];
    var parentname = $("#" + e.id).attr("title");
    var number = substr[2];
    var ParentModelNumber = substr[3];
    var ParentStepID = substr[5];

    $.post("/DCM/Form/ValidateMetadataParameterUsage",
        {
            value: e.value,
            id: id,
            attrUsageId: parentid,
            number: number,
            parentModelNumber: ParentModelNumber,
            ParentStepId: ParentStepID,
            entityId
        },
        function (response) {
            var index = idParentDiv.lastIndexOf("_");
            var newId = idParentDiv.substr(0, index);

            $("#" + newId).replaceWith(response);

            updateHeader();
        });
};

function OnChangeNumbers(e) {
    var idParentDiv = $(this).attr("id");
    var value = $(e.currentTarget).val();
    var substr = e.target.id.split('_');
    var id = substr[0];
    var parentid = substr[1];
    var parentname = ""//e.target.attr("title");
    var metadataStructureId = substr[2];
    var number = substr[3];
    var ParentModelNumber = substr[4];
    var ParentStepID = substr[5];

    $.post("/DCM/Form/ValidateMetadataAttributeUsage",
        {
            value: value,
            id: id,
            parentid: parentid,
            parentname: parentname,
            number: number,
            parentModelNumber: ParentModelNumber,
            ParentStepId: ParentStepID,
            entityId
        },
        function (response) {
            var index = idParentDiv.lastIndexOf("_");
            var newId = idParentDiv.substr(0, index);

            $("#" + newId).replaceWith(response);

            updateHeader();
        });
}
function OnChangeParameterNumbers(e) {
    var idParentDiv = $(this).attr("id");
    var value = $(e.currentTarget).val();
    var substr = e.target.id.split('_');
    var id = substr[0];
    var parentid = substr[1];
    var parentname = ""//e.target.attr("title");
    var metadataStructureId = substr[2];
    var number = substr[3];
    var ParentModelNumber = substr[4];
    var ParentStepID = substr[5];

    $.post("/DCM/Form/ValidateMetadataParameterUsage",
        {
            value: value,
            id: id,
            attrUsageId: parentid,
            number: number,
            parentModelNumber: ParentModelNumber,
            ParentStepId: ParentStepID,
            entityId
        },
        function (response) {
            var index = idParentDiv.lastIndexOf("_");
            var newId = idParentDiv.substr(0, index);

            $("#" + newId).replaceWith(response);

            updateHeader();
        });
}

function OnChangeDatePicker(e) {
    //console.log(e.value);

    var value = $(e.currentTarget).val(); // data value as normal text string (not as DateTime string -> e.value)
    var idParentDiv = $(this).attr("id");
    var substr = e.target.id.split('_');
    var id = substr[0];
    var parentid = substr[1];
    var parentname = $("#" + e.id).attr("title");
    var number = substr[2];
    var ParentModelNumber = substr[3];
    var ParentStepID = substr[5];

    $.post("/DCM/Form/ValidateMetadataAttributeUsage",
        {
            value: value,
            id: id,
            parentid: parentid,
            parentname: parentname,
            number: number,
            parentModelNumber: ParentModelNumber,
            ParentStepId: ParentStepID,
            entityId
        },
        function (response) {
            var index = idParentDiv.lastIndexOf("_");
            var newId = idParentDiv.substr(0, index);

            $("#" + newId).replaceWith(response);

            updateHeader();
        });
}
function OnChangeParameterDatePicker(e) {
    //console.log(e.value);

    var value = $(e.currentTarget).val(); // data value as normal text string (not as DateTime string -> e.value)
    var idParentDiv = $(this).attr("id");
    var substr = e.target.id.split('_');
    var id = substr[0];
    var parentid = substr[1];
    var parentname = $("#" + e.id).attr("title");
    var number = substr[2];
    var ParentModelNumber = substr[3];
    var ParentStepID = substr[5];

    $.post("/DCM/Form/ValidateMetadataParameterUsage",
        {
            value: value,
            id: id,
            attrUsageId: parentid,
            number: number,
            parentModelNumber: ParentModelNumber,
            ParentStepId: ParentStepID,
            entityId
        },
        function (response) {
            var index = idParentDiv.lastIndexOf("_");
            var newId = idParentDiv.substr(0, index);

            $("#" + newId).replaceWith(response);

            updateHeader();
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
                ParentStepId: ParentStepID,
                entityId
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
                ParentModelNumber: ParentModelNumber,
                entityId
            };

            $
                .post('/DCM/Form/RemoveMetadataAttributeUsage',
                    {
                        id: id,
                        parentid: parentid,
                        number: number,
                        parentModelNumber: ParentModelNumber,
                        ParentStepId: ParentStepID,
                        entityId
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
                ParentStepId: ParentStepID,
                entityId
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
                ParentStepId: ParentStepID,
                entityId
            },
            function (response) {
                var id = ParentStepID;
                $("#" + id).replaceWith(response);
            })
    }
}

// Autocomplete
function OnClose(e, ui) {
    console.log("OnClose start");

    // after the on close event from the autocomplete component, the values change in the input fields
    // after this changes again a change event is triggered
    // to prevent this, a flag is set to check wheter this event is fired after a close event or not
    afterClosed = true;

    //console.log(ui);
    //var value = e.target.value;

    var uiid = e.target.id;
    var substr = e.target.id.split('_');
    var id = substr[0];
    // var tAutoComplete = $('#' + uiid).data("tAutoComplete");
    // console.log(tAutoComplete);
    var value = ui.item.value;

    var type = $('#' + uiid).attr("type");
    var start = 0;
    var end = 0;
    var partyid = 0;
    var entityid = 0;
    var entitytypeid = 0;
    var entityinfo;
    var number = 0;
    var parent;
    var parentid = 0;

    console.log(value);
    console.log(type);
    // if the autocomplete type a partycustm type
    if (type === "PartyCustomType") {
        console.log("partycustomtype");

        if (~value.indexOf("(") && ~value.indexOf(")")) {
            start = value.lastIndexOf("(") + 1;
            end = value.lastIndexOf(")");
            partyid = value.substr(start, end - start);

            var onlyValue = value.substr(0, start - 2);

            console.log("partyid = " + partyid);

            if (partyid !== "0") {
                // check if mapping to this metadata attribute is simple or complex.
                // complex means, that the attribute is defined in the context of the parent
                // e.g. name of User
                // simple means, that the attribute is not defined in the context of the
                // e.g. DataCreator Name in Contacts as list of contacts

                if ($(e.target).attr("simple") !== null) {
                    console.log("SIMPLE start");

                    var simple = $(e.target).attr("simple");
                    var xpath = $(e.target).attr("xpath");

                    if (simple === "True") {
                        console.log("UpdateSimpleMappingWithParty ");

                        UpdateSimpleMappingWithParty(uiid, xpath, partyid, onlyValue);
                    }
                }

                var complex;
                if ($(e.target).attr("complex") !== null) {
                    console.log("COMPLEX start");

                    complex = $(e.target).attr("complex");
                    if (complex === "True") {
                        parent = $(e.target).parents(".metadataCompountAttributeUsage")[0];
                        //console.log("parent");
                        //console.log(parent);

                        if (parent !== null) {
                            parentid = $(parent).attr("id");
                            number = $(parent).attr("number");
                            UpdateWithParty(parentid, number, partyid);
                        }
                    }
                }
            }
        }
    }

    // if the autocomplete type a Entity
    if (type === "Entity") {
        console.log("ENTITY STARTS");

        if (~value.indexOf("(") && ~value.indexOf(")")) {
            start = value.lastIndexOf("(") + 1;
            end = value.lastIndexOf(")");
            //(1/2)
            entityInfo = value.substr(start, end - start);
            entityid = entityInfo.split("_")[0];
            entitytypeid = entityInfo.split("_")[1];

            var title = value.substr(0, start - 2);

            parent = $(e.target).parents(".metadataCompountAttributeUsage")[0];

            if (parent !== null) {
                parentid = $(parent).attr("id");
                number = $(parent).attr("number");
                var attrnumber = $('#' + id).attr("number");

                UpdateWithEntity(parentid, number, id, attrnumber, entityid, entitytypeid, title);
            }
        }
    }
}

/******************************************
 ********* Component************************
 ******************************************/

function UpdateWithEntity(componentId, number, inputid, inputattrnumber, entityid, entitytypeid, value) {
    //console.log("update with entity");
    //console.log(componentId + "-" + number + "-" + entityid + "_" + entitytypeid);

    var attrId = inputid.split("_")[0];

    if (inputattrnumber === undefined) inputattrnumber = 1;

    //$("#" + componentId).find(".metadataAttributeInput").each(function () {
    //    $(this).preloader(12, "...loading");
    //})

    $.post('/DCM/Form/UpdateComplexUsageWithEntity',
        {
            stepId: componentId,
            number: number,
            inputattrid: attrId,
            inputAttrNumber: inputattrnumber,
            entityId: entityid,
            entityTypeId: entitytypeid,
            value: value,
            entityId
        },
        function (response) {
            //console.log(componentId);
            //console.log(response);

            $("#" + componentId).replaceWith(response);
            // update party id to component
            $($("#" + inputid)[inputattrnumber - 1]).attr("entityid", entityid);
            //alert("test");
            autosize($('textarea'));
        })
}

function UpdateWithParty(componentId, number, partyid) {
    console.log("update with complex mapping");
    //console.log(componentId + "-" + number + "-" + partyid);

    $("#" + componentId).find(".metadataAttributeInput").each(function () {
        $(this).preloader(12, "...loading");
    })

    $.post('/DCM/Form/UpdateComplexUsageWithParty',
        {
            stepId: componentId,
            number: number,
            partyId: partyid,
            entityId
        },
        function (response) {
            //console.log(componentId);
            //console.log(response);

            $("#" + componentId).replaceWith(response);
            // update party id to component
            $("#" + componentId).attr("partyid", partyid);
            // update linked icon
            $("#" + componentId + "_notLinked").hide();
            $("#" + componentId + "_linked").show();
            //alert("test");
            autosize($('textarea'));
        })
}

function UpdateSimpleMappingWithParty(componentId, xpath, partyid, value) {
    console.log("update with simple mapping");
    console.log(value);
    console.log($("#" + componentId));
    console.log("----------------------");

    $.post('/DCM/Form/UpdateSimpleUsageWithParty',
        {
            xpath: xpath,
            partyId: partyid,
            entityId
        },
        function (response) {
            if (response) {
                $("#" + componentId).attr("partyid", partyid);
                $("#" + componentId).val(value);
                $("#" + componentId).attr("value", value);
                // update linked icon
                $("#" + componentId + "_notLinked").hide();
                $("#" + componentId + "_linked").show();
            }
        });
}

function Add(e) {
    var temp = e.id;
    var parentId = temp.split("_")[0];
    var number = temp.split("_")[1];

    //alert(e.id);
    //alert(parentId);
    $.get('/DCM/Form/AddComplexUsage',
        { parentStepId: parentId, number: number, entityId },
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
        { parentStepId: parentId, number: number, entityId },
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
        { parentStepId: parentId, number: number, entityId },
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
        { parentStepId: parentId, number: number, entityId },
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
            id: stepid,
            entityId
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
            parentid: stepid, id: v, entityId
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
    $('#' + buttonId).toggleClass("fa-angle-double-down fa-angle-double-right");
    bindMinimap(true);
}

function showAllClick(e) {
    $('.first_level').show();
    $('.header-menu').css('background-color', '##bee1da');
    $(e).parent().css('background-color', '#a1bbb6');
    bindMinimap(true);
}
function showFirstLevelClick(e) {
    var temp = e.id;
    var parentId = temp.split("_")[0];
    var number = temp.split("_")[1];

    var id = parentId + "_" + number + "_Container";
    var buttonId = parentId + "_" + number + "_ButtonView";

    $('.header-menu').css('background-color', '##bee1da');
    $(e).parent().css('background-color', '#a1bbb6');

    $('.first_level').hide();
    $('#' + parentId).show();
    $('#' + id).show();
    bindMinimap(true);
}

function updateHeader() {
    bindMinimap(true);
    $('.header-menu').removeClass('bx-input-error')
    $('.bx-input-error').closest('.Metadata-Level-1:first-child').each(function () { $('#' + $(this).parent().parent().attr('id') + '_Menu').addClass('bx-input-error') });
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