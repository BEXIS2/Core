function iconClick(e) {
    console.log(e);
    $(e).toggleClass("bx-angle-double-down bx-angle-double-up");
    var container = $(e).parents(".le-container");
    console.log(container);
    
    $(container).find(".le-container-content").slideToggle();

};

function leSimpleSelectorClick(e) {

        //console.log(e);
        var parent = $(e).parents(".le-simple")[0];
        //console.log(parent);
        var info = $(parent).find(".le-simple-info")[0];    
        //console.log(info);
        //console.log($(info).find("#Id"));

        var id = $(info).find("#Id").text();
        var type = $(info).find("#Type").text();
        var elementid = $(info).find("#ElementId").text();
        var position = $(info).find("#Position").text();
        var complexity = $(info).find("#Complexity").text();
        var name = $(info).find("#Name").text();
        var xpath = $(info).find("#XPath").text();

        var le =
        {
            "Id":id,
            "Name":name,
            "ElementId": elementid,
            "Type": type,
            "Position": position,
            "Complexity": complexity,
            "XPath":xpath
        }

        console.log(le);

    $.ajax({
        type: "POST",
        url: "/DIM/Mapping/AddMappingElement",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        data: JSON.stringify(le),
        success: function(data) {

            console.log("type : "+type);
            if (position.toLowerCase() === "source") {
                $("#emptySourceContainer").replaceWith(data);

                //deactivacte all source add icons
                changeAddFunctionOnSameSide("Source");

            } else {
                $("#emptyTargetContainer").replaceWith(data);

                //deactivacte all source add icons
                changeAddFunctionOnSameSide("Target");
            }


        },
        error: function(data) { alert("error") }


    });
};

function deactivateBts() {
    

}

function changeAddFunctionOnSameSide(key) {

    $("." + key)
        .find(".le-simple-selector")
        .each(function() {
            //console.log(this);
            $(this).toggleClass("bx-disabled function");

            if ($(this).attr("disabled")) {
                $(this).removeAttr("disabled");
            } else {
                $(this).attr("disabled", "disabled");
            }
        });

    if ($("#emptySourceContainer").length === 0 && $("#emptyTargetContainer").length === 0) {
        $("#newMapContainer .mapping-settings").show();
        //$(deleteBt).hide();
        initJSPLUMB("mapping_container_0");
    } else {
        //$(deleteBt).show();
    }
}

function deleteComplexMappingElement(e) {

    var parent = $(e).parents(".mapping_container_child")[0];
    console.log(parent);

    if ($(parent).hasClass("mapping_container_source")) {

        //add source
        $(parent).find(".le-mapping-container-header").remove();
        $(parent).find(".mapping-container-childrens").remove();
        $(parent).append("<div id='emptySourceContainer'> <b>SOURCE</b></div>");

        changeAddFunctionOnSameSide("Source");

        if ($("#emptySourceContainer").length !== 0 || $("#emptyTargetContainer").length !== 0) {
            $("#newMapContainer .mapping-settings").hide();
        }
    }

    if ($(parent).hasClass("mapping_container_target")) {

        $(parent).find(".le-mapping-container-header").remove();
        $(parent).find(".mapping-container-childrens").remove();
        //add Target
        $(parent).append("<div id='emptyTargetContainer'> <b>Target</b></div>");

        changeAddFunctionOnSameSide("Target");

        if ($("#emptySourceContainer").length !== 0 || $("#emptyTargetContainer").length !== 0) {
            $("#newMapContainer .mapping-settings").hide();
        }
    }
}

function createMapping(e) {

    console.log(e);
    var parent = $(e).parents(".mapping-container")[0];
    console.log(parent);


    //get Root source
    var rootInfo = $("#le-root-source").find(".le-root-info")[0];

    var rootSourceId = $(rootInfo).find("#Id").text();
    var rootSourceType = $(rootInfo).find("#Type").text();
    var rootSourceElementid = $(rootInfo).find("#ElementId").text();
    var rootSourcePosition = $(rootInfo).find("#Position").text();
    var rootSourceName = $(rootInfo).find("#Name").text();

    var rootSource =
    {
        "Id": rootSourceId,
        "Name": rootSourceName,
        "ElementId": rootSourceElementid,
        "Type": rootSourceType,
        "Position": rootSourcePosition,

    }

    //GET SOURCE
    var sourceContainer = $(parent).find(".mapping_container_source")[0];

    var info = $(sourceContainer).find(".le-mapping-complex-info")[0];

    console.log(info);


    var id = $(info).find("#Id").text();
    var type = $(info).find("#Type").text();
    var elementid = $(info).find("#ElementId").text();
    var position = $(info).find("#Position").text();
    var name = $(info).find("#Name").text();
    var xpath = $(info).find("#XPath").text();

    var source =
    {
        "Id": id,
        "Name": name,
        "ElementId": elementid,
        "Type": type,
        "Position": position,
        "XPath": xpath,
        "Parent": rootSource
    }

    //Get RootTarget

    var rootTargetInfo = $("#le-root-target").find(".le-root-info")[0];

    var rootTargetId = $(rootTargetInfo).find("#Id").text();
    var rootTargetType = $(rootTargetInfo).find("#Type").text();
    var rootTargetElementid = $(rootTargetInfo).find("#ElementId").text();
    var rootTargetPosition = $(rootTargetInfo).find("#Position").text();
    var rootTargetName = $(rootTargetInfo).find("#Name").text();

    var rootTarget =
    {
        "Id": rootTargetId,
        "Name": rootTargetName,
        "ElementId": rootTargetElementid,
        "Type": rootTargetType,
        "Position": rootTargetPosition
    }


    //GET TARGET
    var targetContainer = $(parent).find(".mapping_container_target")[0];

    var targetInfo = $(targetContainer).find(".le-mapping-complex-info")[0];

    console.log(targetInfo);

    var targetId = $(targetInfo).find("#Id").text();
    var targetType = $(targetInfo).find("#Type").text();
    var targetElementid = $(targetInfo).find("#ElementId").text();
    var targetPosition = $(targetInfo).find("#Position").text();
    var targetName = $(targetInfo).find("#Name").text();
    var targetXpath = $(targetInfo).find("#XPath").text();

    var target =
    {
        "Id": targetId,
        "Name": targetName,
        "ElementId": targetElementid,
        "Type": targetType,
        "Position": targetPosition,
        "XPath": targetXpath,
        "Parent": rootTarget
    }

    var mapping =
    {
        "Source": source,
        "Target": target
    }

    console.log(source);
    console.log(target);

    $.ajax({
        type: "POST",
        url: "/DIM/Mapping/SaveMapping",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        data: JSON.stringify(mapping),
        success: function (data) {

            $(parent).remove();
            $('#dim-mapping-middle').append(data);

            //create empty
            $.get("/DIM/Mapping/LoadEmptyMapping",
                function (response) {
                    $('#dim-mapping-middle').append(response);
                    changeAddFunctionOnSameSide("Target");
                    changeAddFunctionOnSameSide("Source");
                });

        },
        error: function (data) { alert("error") }


    });


}

function deleteMapping(e) {

    var parent = $(e).parents(".mapping-container")[0];
    console.log(parent);

    var idArray = $(parent).attr("id").split("_");
    console.log(idArray);

    var id = idArray[idArray.length - 1];
    console.log(id);

    $.post('/DIM/Mapping/DeleteMapping',
        { id: id },
        function(response) {

            if (response === true) {
                $(parent).remove();
            } else {
                alert(response);
            };

        });
}

var connections = [];
var connectionParent = {};

function updateParentConnection(conn, parentId) {

    var exist = false;

    for (var i = 0; i < connections.length; i++) {
        var existParent = connections[i];
        if (existParent.id === parentId) {
            exist = true;
            existParent.connections.push(conn);
        }
    }

    if (!exist) {

        connectionParent = {
            id: parentId,
            connections:[]
        }
        connectionParent.connections.push(conn);

        connections.push(connectionParent);
    } 

}

function removeParentConnection(conn, parentId) {

    for (var i = 0; i < connections.length; i++) {
        var existParent = connections[i];
        var idx;
        if (existParent.id === parentId) {

            for (var j = 0; j < existParent.connections.length; i++) {
                if (existParent.connections[i] == conn) {
                    idx = i;
                    break;
                }
            }

            if (idx != -1) {
                console.log("remove");
                existParent.connections.splice(idx, 1);
                break;
            }
        }
    }
 
}

function updateConnections(conn, remove, parentId) {

   if (!remove) {

        updateParentConnection(conn, parentId);

    }
    else {
        removeParentConnection(conn, parentId);
   }

   console.log("all");
   console.log(connections);

    if (connections.length > 0) {

        for (var i = 0; i < connections.length; i++) {
            var child = connections[i];

            console.log("-----");

            console.log("Mapping : " + child.id);
            console.log("Childrens : " + child.connections.length);
            console.log(child.connections);


        }

        //alert(s);
    } else {
        //hideConnectionInfo();
    }

    console.log(connections.length);
}

function initJSPLUMB(parentid) {

    jsPlumb.ready(function() {
        console.log("init jsplumb");

        var instance = window.instance = jsPlumb.getInstance({

            ConnectionOverlays: [
               ["Arrow", {
                   location: 1,
                   visible: true,
                   width: 11,
                   length: 11,
                   id: "ARROW",
                   events: {
                       click: function () { alert("you clicked on the arrow overlay") }
                   }
               }],
               ["Label", {
                   location: 0.1,
                   id: "label",
                   cssClass: "aLabel",
                   events: {
                       tap: function () { alert("hey"); }
                   },
                   text:"test"
               }]
                ],
            Connector: "StateMachine",
            // drag options
            //DragOptions: { cursor: "pointer", zIndex: 2000 },
            //// default to a gradient stroke from blue to green.
            //PaintStyle: {
            //    gradient: {
            //        stops: [
            //            [0, "#0d78bc"],
            //            [1, "#558822"]
            //        ]
            //    },
            //    stroke: "#558822",
            //    strokeWidth: 10
            //},
            Container: parentid
        });


        var init = function (connection) {
            connection.getOverlay("label").setLabel("Select");
        };

   
        // suspend drawing and initialise.
        instance.batch(function() {

            // bind to connection/connectionDetached events, and update the list of connections on screen.
            instance.bind("connection", function (info, originalEvent) {
                    
                updateConnections(info.connection, false, parentid);
            });

            instance.bind("connectionDetached", function (info, originalEvent) {
                console.log("connectionDetached");

                updateConnections(info.connection, true, parentid);
            });

            instance.bind("connectionMoved", function (info, originalEvent) {
                console.log("connectionMoved");
                //  only remove here, because a 'connection' event is also fired.
                // in a future release of jsplumb this extra connection event will not
                // be fired.
                updateConnections(info.connection, true, parentid);
            });


            //set source
            // get the list of ".le-mapping-simple-selector-source" elements.            
            var simpleSources = jsPlumb.getSelector("#" + parentid + " .le-mapping-simple-selector-source");

            instance.makeSource(simpleSources,
            {
                anchor: "Right",
                endpoint: ["Rectangle", { width: 10, height: 10 }],
                cssclass:"dim-mapping-anchor"
            });


            //set targets
            // get the list of ".le-mapping-simple-selector-source" elements.            
            var simpleTargets = jsPlumb.getSelector("#" + parentid + " .le-mapping-simple-selector-target");
            instance.makeTarget(simpleTargets,
            {
                anchor: "Left",
                endpoint: ["Rectangle", { width: 10, height: 10 }]
                
            });


            // listen for new connections; initialise them the same way we initialise the connections at startup.
            instance.bind("connection", function (connInfo, originalEvent) {
                init(connInfo.connection);
            });


        });

        jsPlumb.fire("jsPlumbDemoLoaded", instance);
    });
};


function getContainerSize(){
    return $(window).height() - $('.navbar').outerHeight() - $('#footer').outerHeight()-90;
}
