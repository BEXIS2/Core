
$(document).ready(function () {
    if ($('textarea') != null) {
        autosize($('textarea'));
    }
    //setTabIndex();
    resetAllTelerikIconTitles();
});

function setTabIndex() {

    var list = $(".metadataAttributeInput .t-input");
    console.log(list);
    for (var i = 0; i < list.length ; i++) {
        var input = list[i];
        console.log(input);
        $(input).attr("tabindex", i);
    }
}


/******************************************
 ********* FORM    ************************
 ******************************************/

function bindMinimap() {

    //if (($('#root').height()+200) > ($(window).height())) {

    if ($(".miniregion")) {
        $(".miniregion").remove();
    }

    if ($(".minimap")) {
        $(".minimap").remove();
    }
    if ($('#root')) {

        var offset = getRatioHeight($('#root').position().top);

        var hRatio = 1 - $(window).height() / $('#root').height();
        if (hRatio <= 0) {
            hRatio = 0.1;
        }

        var previewBody = $('#root').minimap(
        {
            heightRatio: hRatio,
            widthRatio: 0.095,
            offsetHeightRatio: offset,
            offsetWidthRatio: 0.02,
            position: "right",
            touch: true,
            smoothScroll: true,
            smoothScrollDelay: 200
        });

        $(".minimap").css("z-index", "1000");
        $(".miniregion").css("z-index", "1000");

        $('#MetadataEditor').css("width", "89%");
    }



    //} else {

    //    if ($(".miniregion")) {
    //        $(".miniregion").remove();
    //    }

    //    if ($(".minimap")) {
    //        $(".minimap").remove();
    //    }

    //    $('#MetadataEditor').css("width", "100%");
    //}
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
    console.log("OnKeyDownTextInput");
    console.log(e.id);
    console.log(e.value.length);
    console.log(e.value);

       
    var length = e.value.length;

    if (length >= 60) {
        console.log("start replace");
        var textarea = inputToTextArea(e);
        console.log(textarea);
        $("#" + e.id).replaceWith(textarea);

        //set focus
        var tmp = $("#" + e.id).val();
        $("#" + e.id).val('');
        autosize($("#" + e.id));
        $("#" + e.id).val(tmp);
        $("#" + e.id).focus();
      
        console.log("done");
    }
}

function inputToTextArea(input) {
    //<input    id="310_161_1_1_1_286_Input" name="310_161_1_1_1_286_Input" onkeyup="OnKeyUpTextInput(this)"                   title="" class="t-widget t-autocomplete t-input bx-metadataFormTextInput"  type="text" value="dassssss" autocomplete="off">
    //<textarea id="310_161_1_1_1_380_Input" name="Title"                   onchange="OnChange(this)" packageid="161"          title="" class="bx-textarea bx-metadataFormTextInput"                      cols="20" rows="2" style="overflow: hidden; word-wrap: break-word; resize: horizontal; height: 75px;">dassssssggggggggggggggggggggggggggggggggggggggggggggggggggggg</textarea>
    var textarea = "<textarea " +
        "id='" + $("#" + input.id).attr("id") + "'" +
        "name='" + $("#" + input.id).attr("name") + "'" +
        "onchange=\"OnChange(this)\"" +
        "onkeyup= \"OnKeyUpTextArea(this)\""+
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
        $("#" + e.id).value='';
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
        "title='"+$("#" + textarea.id).attr("title")+"'" +
        "class=\"t-widget t-autocomplete t-input bx-metadataFormTextInput \"" +
        "value='" + $("#" + textarea.id).val() + "' " +
        "autocomplete=\"off\"></input>";

    return input;
}

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
            if ($('textarea') != null) {
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
        function(response) {

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
        error: function(xhr) {
            //alert('Error: ' + xhr.statusText);
        },
        success: function(result) {
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
        error: function(xhr) {
            //alert('Error: ' + xhr.statusText);
        },
        success: function(result) {
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
            function(response) {

                var id = ParentStepID;
                //alert(id);
                //alert(response);
                $("#" + id).replaceWith(response);
            });
    } else {
        alert("Maxium cardinality is reached.");
    }
}

function OnClickRemove(e) {

    var value = $("#" + e.id).closest(".ValueClass").value;

    if (value != "") {

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
                    function(response) {

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
    if (value != "") {

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
            function(response) {

                var id = ParentStepID;
                $("#" + id).replaceWith(response);
            });

    }
}

function OnClickDown(e) {
    //alert(value);
    var value = $("#" + e.id).closest(".ValueClass").value;
    if (value != "") {

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

        $.post('DCM/Form/DownMetadataAttributeUsage',
            {
                id: id,
                parentid: parentid,
                number: number,
                ParentModelNumber: ParentModelNumber,
                ParentStepId: ParentStepID
            },
            function(response) {

                var id = ParentStepID;
                $("#" + id).replaceWith(response);
            })

    }
}

/******************************************
 ********* Component************************
 ******************************************/

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
            bindMinimap();
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
            bindMinimap();
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
            bindMinimap();
        })
}

function Down(e) {
    var temp = e.id;
    var parentId = temp.split("_")[0];
    var number = temp.split("_")[1];

    $.get('/DCM/Form/DownComplexUsage',
        { parentStepId: parentId, number: number },
        function(response) {
            $('#' + parentId).replaceWith(response);
            bindMinimap();
        })
}

function Activate(e) {
    var temp = e.id;
    var stepid = temp.split("_")[0];
    var active = $(e).hasClass("bx-check-square-o");

    $.get('/DCM/Form/ActivateComplexUsage',
    {
        id : stepid
    },
        function (response) {
            $('#' + stepid).replaceWith(response);

            if (!active) {
                $('html, body').animate({
                    scrollTop: $('#' + stepid).offset().top - 80
                }, 'slow');
            }

            resetAllTelerikIconTitles();
            bindMinimap();
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

        if (!active)
        {
            $('html, body').animate({
                scrollTop: $('#' + stepid).offset().top - 70
            }, 'slow');
        }

        resetAllTelerikIconTitles();
        bindMinimap();
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
    bindMinimap();
}