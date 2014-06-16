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

//function PrimaryData_OnCommand(e) {

//    var grid = $("#PrimaryDataResultGrid").data("tGrid");

//    var columns = grid.columns;

//    var currentFilter = grid.filterBy;
//    var currentOrder = grid.orderBy;

//    var visibleColumns ="";
//    jQuery.each(columns, function (index) {
//        if (!this.hidden) {
//            if (visibleColumns != "")

//                visibleColumns += "," + this.member;
//            else
//                visibleColumns = this.member;
//        }
//    });

//    $.post('@Url.Action("SetGridCommand", "Data", new RouteValueDictionary { { "area", "DDM" } })', { filters: currentFilter, orders: currentOrder, columns: visibleColumns }, function (response) {

//    })

//}