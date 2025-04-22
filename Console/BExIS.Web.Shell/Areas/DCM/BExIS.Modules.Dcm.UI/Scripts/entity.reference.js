/*TARGET TYPE to load the targets*/
$("#TargetType").on('change', function () {
    var id = $("#TargetType").val();

    $("#Target").empty();
    $("#target_preloader").preloader(12, "");

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
    $("#Target").append("<option value='' selected disabled hidden>Select a entity</option>");

    for (var i = 0; i < data.length; i++) {
        var option = data[i];

        $("#Target").append("<option value=" + option.Value + ">" + option.Text + "</option>");
    }

    $("#Target").prop("disabled", false);
    $("#Target").removeClass("bx-disabled");
    $("#target_preloader").removePreloader();
}

function errorFunc() {
    alert('error');
    $("#target_preloader").removePreloader();
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
    //console.log(data);

    for (var i = 0; i < data.length; i++) {
        var option = data[i];
        //console.log(data);
        $("#TargetVersion").append("<option value=" + option.Value + ">" + option.Text + "</option>");
        //$("#TargetVersion").append("<option value=" + i + ">" + i + "</option>");

        console.log($("#TargetVersion"));
    }


    $("#TargetVersion").prop("disabled", false);
    $("#TargetVersion").prop("selectedIndex", 0);
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

function createEntityReference_OnSuccess(data) {
    //console.log("on success");
    //console.log(data);

    if (data === true) {
        console.log("on success data = true");

        var id = $("#Selected_Id").val();
        var type = $("#Selected_TypeId").val();
        var version = $("#Selected_Version").val();
        reload(id, type, version);

        $("#window_createReference").data('tWindow').destroy();
    }
}

function reload(id, type, version) {
    var parameters = {
        sourceId: id,
        sourceTypeId: type,
        sourceVersion: version
    };

    console.log(parameters);

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

function show_hide_help() {
    $("#help_ref_type").toggle();
}