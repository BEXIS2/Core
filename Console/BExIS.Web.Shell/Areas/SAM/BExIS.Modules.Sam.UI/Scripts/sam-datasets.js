$(".number-of-rows-bt").click(function () {
    var id = $(this).attr("entityid");
    var bt = this;
    console.log(id)

    $.get("/SAM/Datasets/CountRows", { id },
        function (data, textStatus, jqXHR) {
            var tr = $(bt).parents("tr")[0];
            var nuRowsTd = $(tr).find(".number-of-rows");
            $(nuRowsTd).text(data);
        },
        "json"
    );
})