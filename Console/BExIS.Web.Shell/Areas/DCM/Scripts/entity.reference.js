﻿/*TARGET TYPE to load the targets*/
$("#TargetType").on('change', function () {
    var id = $("#TargetType").val();

    $("#Target").empty();


    if (id !== "" && id > 0) {
        $.ajax({
            type: "GET",
            url: "/DCM/EntityReference/GetTargets",
            data: { id: id },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: successFunc,
            error: errorFunc
        })
    }
    else {
       
        $("#Target").prop("disabled", true);
        $("#Target").addClass("bx-disabled");
    }
});

function successFunc(data, status) {
    console.log(data);

    for (var i = 0; i < data.length; i++) {
        var option = data[i];
        $("#Target").append("<option value=" + option.Value + ">" + option.Text + "</option>");
    }

    $("#Target").prop("disabled", false);
    $("#Target").removeClass("bx-disabled");
}

function errorFunc() {
    alert('error');
}
/*END TARGET TYPE*/

/*TARGET to load the versions*/
$("#Target").on('change', function () {
    var id = $("#Target").val();
    var type = $("#TargetType").val();

    $("#TargetVersion").empty();

    if (id !== "" && id > 0) {
        $.ajax({
            type: "GET",
            url: "/DCM/EntityReference/GetTargetVersions",
            data: { id: id, type: type },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: successFuncTargetVersions,
            error: errorFuncTargetVersions
        })
    }
    else {
       
        $("#TargetVersion").prop("disabled", true);
        $("#TargetVersion").addClass("bx-disabled");
    }
});

function successFuncTargetVersions(data, status) {
    console.log(data);

    for (var i = 0; i < data.length; i++) {
        var option = data[i];
        $("#TargetVersion").append("<option value=" + option.Value + ">" + option.Text + "</option>");
    }

    $("#TargetVersion").prop("disabled", false);
    $("#TargetVersion").prop("selectedIndex", data.length - 1);
    $("#TargetVersion").removeClass("bx-disabled");
}

function errorFuncTargetVersions() {
    alert('error');
}
/*END TARGET VERSIONS*/

$("#button_createEntityreference_save").click(function () {
})

$("#button_createEntityreference_cancel").click(function () {
    $("#window_createReference").data('tWindow').destroy();
});

function onSuccess() {
    var id = $("#SourceId").val();
    var type = $("#SourceTypeId").val();
    reload(id, type);

    $("#window_createReference").data('tWindow').destroy();
}

function reload(id, type) {
    var parameters = {
        sourceId: id,
        sourceTypeId: type
    };

    $.ajax({
        type: 'GET',
        url: "/dcm/entityreference/show",
        data: parameters,
        dataType: 'html',
        success: function (htmlData) {
            $("#entity-reference-container").replaceWith(htmlData);
        }
    })
}