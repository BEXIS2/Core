$(document).ready(function () {
    $("#TargetType").on('change', function () {
        var id = $("#TargetType").val();

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
            $("#Target").empty();
            $("#Target").prop("disabled", true);
            $("#Target").addClass("bx-disabled");
        }
    });
})

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