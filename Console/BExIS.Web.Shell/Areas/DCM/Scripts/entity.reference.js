$(document).ready(function () {
    $("#TargetType").on('change', function () {
        var type = $("#TargetType :selected").text();
        //var optGroup = $('optgroup[label="' + selected + '"]')[0].outerHTML;

        $.ajax({
            type: "POST",
            url: "'/DCM/EntityReference/GetTargets",
            data: type,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: successFunc,
            error: errorFunc
        });

        function successFunc(data, status) {
            alert(data);
        }

        function errorFunc() {
            alert('error');
        }
    });
})