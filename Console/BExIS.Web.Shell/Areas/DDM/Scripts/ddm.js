$("#fullDatasetDownload").change(function () {
    var subset = $('#fullDatasetDownload').is(":checked");

    alert(subset);

    $.post('@Url.Action("SetFullDatasetDownload","Data", new RouteValueDictionary { { "area", "DDM" } })', { subset: subset }, function (response) {
        alert(response);
    })
});

function PrimaryDataResultGrid_OnLoad(e) {
    $('.t-grid .t-status').hide();
}

function PrimaryData_OnColumnChange(e)
{
    alert(e.name);

}