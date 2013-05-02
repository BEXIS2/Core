
//************************************************//
//*********** _previewDatastructure.cshtml *******//

function dataStructureGrid_OnRowDataBound(e) {

    //alert(e.item);
    var obj = e.row.childNodes;

    $.each(obj, function (i) {

        //$(this).trunk8();
        obj[i].title = obj[i].textContent;
        if (obj[i].textContent.length > 20) {
            obj[i].textContent = jQuery.trim(obj[i].textContent).substring(0, 20).trim(this) + "...";
        }
    });
}