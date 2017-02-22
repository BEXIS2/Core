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
        $(parent).find(".le-mapping-complex").remove();
        $(parent).append("<div id='emptySourceContainer'> <b>SOURCE</b></div>");

        changeAddFunctionOnSameSide("Source");

        if ($("#emptySourceContainer").length !== 0 || $("#emptyTargetContainer").length !== 0) {
            $("#newMapContainer .mapping-settings").hide();
        }
    }

    if ($(parent).hasClass("mapping_container_target")) {

        $(parent).find(".le-mapping-complex").remove();
        //add Target
        $(parent).append("<div id='emptyTargetContainer'> <b>Target</b></div>");

        changeAddFunctionOnSameSide("Target");

        if ($("#emptySourceContainer").length !== 0 || $("#emptyTargetContainer").length !== 0) {
            $("#newMapContainer .mapping-settings").hide();
        }
    }
}

function createRootElement(info) {
    
    //get Root source
    var id = $(info).find("#Id").text();
    var type = $(info).find("#Type").text();
    var elementid = $(info).find("#ElementId").text();
    var position = $(info).find("#Position").text();
    var name = $(info).find("#Name").text();
    var complexity = $(info).find("#Complexity").text();

    var obj =
    {
        "Id": id,
        "Name": name,
        "ElementId": elementid,
        "Type": type,
        "Position": position,
        "Complexity":complexity
    }

    return obj;
}

function createElement(info, element) {

    //get Root source
    var id = $(info).find("#Id").text();
    var type = $(info).find("#Type").text();
    var elementid = $(info).find("#ElementId").text();
    var position = $(info).find("#Position").text();
    var name = $(info).find("#Name").text();
    var complexity = $(info).find("#Complexity").text();

    var obj =
    {
        "Id": id,
        "Name": name,
        "ElementId": elementid,
        "Type": type,
        "Position": position,
        "Complexity": complexity,
        "Parent": element
    }

    return obj;
}

function createSimpleMapping(conn, sourceParent,targetParent) {

    console.log("create simple mappings");
    console.log(conn);
    console.log(conn.id);
    console.log(conn.sourceId);
    console.log(conn.targetId);

    
    var source = $("#" + conn.sourceId);
    var sourceInfo = $(source).find(".le-simple-info")[0];

    var target = $("#" + conn.targetId);
    var targetInfo = $(target).find(".le-simple-info")[0];

    var sourceObj = createElement(sourceInfo, sourceParent);
    var targetObj = createElement(targetInfo, targetParent);

    //var parent = $(source).parents(".mapping-container")[0];

    var obj =
    {
        "Source": sourceObj,
        "Target": targetObj
    }

    return obj;
}

function saveMapping(e, create) {

    //console.log(e);
    var parent = $(e).parents(".mapping-container")[0];
    //console.log(parent);


    //get Root source
    var rootInfo = $("#le-root-source").find(".le-root-info")[0];
    var rootSource = createRootElement(rootInfo);

    //GET SOURCE
    var sourceContainer = $(parent).find(".mapping_container_source")[0];
    var info = $(sourceContainer).find(".le-mapping-complex-info")[0];
    var source = createElement(info, rootSource);

    //Get RootTarget
    var rootTargetInfo = $("#le-root-target").find(".le-root-info")[0];
    var rootTarget = createRootElement(rootTargetInfo);


    //GET TARGET
    var targetContainer = $(parent).find(".mapping_container_target")[0];
    var targetInfo = $(targetContainer).find(".le-mapping-complex-info")[0];
    var target = createElement(targetInfo, rootTarget);

    // add simple mappings
    var simpleMappings = [];
    var parentMapping;
    // get mappingContainer Connection
    for (var i = 0; i < connections.length; i++) {
        if (connections[i].id === parent.id) {
            parentMapping = connections[i];
        }

    }

    if (parentMapping != null) {

        for (var i = 0; i < parentMapping.connections.length; i++) {
            var sm = createSimpleMapping(
                parentMapping.connections[i],
                source,
                target
            );
            simpleMappings.push(sm);
        }
    }

    var sendData =
    {
        "Source": source,
        "Target": target,
        "SimpleMappings":simpleMappings
    }
    //console.log(sendData);

    $.ajax({
        type: "POST",
        url: "/DIM/Mapping/SaveMapping",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        data: JSON.stringify(sendData),
        success: function(data) {

            $(parent).remove();
            $('#dim-mapping-middle').append(data);

            if (create) {

            //create empty
            $.get("/DIM/Mapping/LoadEmptyMapping",
                function(response) {
                    $('#dim-mapping-middle #newMapContainer').append(response);
                    changeAddFunctionOnSameSide("Target");
                    changeAddFunctionOnSameSide("Source");

                    //resetAllConnections();
                });
            }

            updateSaveOptions($(parent).attr("id"), false);

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

    //update save options
   if (connectionsChanged(parentId)) {
        updateSaveOptions(parentId, true);
    } else {
        updateSaveOptions(parentId, false);
    }
    console.log(connections.length);
}

function resetAllConnections() {

    //delete all connections
    console.log("RESET");
    console.log(connections.length);
    for (var i = 0; i < connections.length; i++)
    {
        var existParent = connections[i];
        console.log(existParent);
        var id = existParent.id;

        //jsPlumb.empty(id);

        //set all conncetions
        //initJSPLUMB(id);
    }
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

        instance.registerConnectionType("basic", { endpoint: ["Rectangle", { width: 5, height: 5 }], anchor: "Continuous", connector: ["Flowchart", { stub: [40, 60], gap: 10, cornerRadius: 5, alwaysRespectStubs: true }] });

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
                endpoint: ["Rectangle", { width: 5, height: 5 }],
                cssclass:"dim-mapping-anchor"
            });


            //set targets
            // get the list of ".le-mapping-simple-selector-source" elements.            
            var simpleTargets = jsPlumb.getSelector("#" + parentid + " .le-mapping-simple-selector-target");
            instance.makeTarget(simpleTargets,
            {
                anchor: "Left",
                endpoint: ["Rectangle", { width: 5, height: 5, color:"white" }]
                
            });


            // listen for new connections; initialise them the same way we initialise the connections at startup.
            instance.bind("connection", function (connInfo, originalEvent) {
                init(connInfo.connection);
            });


            //create Exiting connections
            addConnections(instance, parentid);

        });

        jsPlumb.fire("jsPlumbDemoLoaded", instance);
    });
};

function addConnections(jsPlumbInstance, parentid) {


    var parent = $("#" + parentid);
    var simpleMappings = parent.find(".mapping-container-simple-hidden-mapping");
    console.log("add connections *********************");

    console.log(simpleMappings);

    for (var i = 0; i < simpleMappings.length; i++) {

        //console.log(sm);
        //console.log($(sm).attr("sourceId"));
        //console.log($(sm).attr("targetId"));

        var sm = simpleMappings[i];

        var sourceid = $(sm).attr("sourceId");
        var targetid = $(sm).attr("targetId");

        var source= $("#" + parentid).find("#" + sourceid).parents(".le-simple-selector")[0];
        var target = $("#" + parentid).find("#" + targetid).parents(".le-simple-selector")[0];
   
       
        jsPlumbInstance.connect({
            source: source,
            target: target,
            type: "basic"
        });
    }

}

function getContainerSize(){
    return $(window).height() - $('.navbar').outerHeight() - $('#footer').outerHeight()-90;
}

function connectionsChanged(parentId) {
    
    var allConnections = [];

    for (var i = 0; i < connections.length; i++) {
        var existParent = connections[i];
        if (existParent.id === parentId) {
            allConnections = existParent.connections;
            //console.log(allConnections);
        }
    }

    var parent = $("#" + parentId);
    var startConnectionsList = $(parent).find(".mapping-container-simple-hidden-mapping");

    if (allConnections.length == startConnectionsList.length) {
        return false;
    }

    return true;
}

function countAllNewConnections(parentId) {

    //console.log("+++++++++++++++++++++++++++++++++++");
    //console.log("countAllNewConnections");

    var allNewConnections = [];
    var allConnections = [];

    for (var i = 0; i < connections.length; i++) {
        var existParent = connections[i];
        if (existParent.id === parentId) {
            allConnections = existParent.connections;
            //console.log(allConnections);
        }
    }

    var parent = $("#" + parentId);
    var startConnectionsList = $(parent).find(".mapping-container-simple-hidden-mapping");
    //console.log(startConnectionsList);

    for (var l = 0; l < allConnections.length; l++) {
        var newConn = allConnections[l];
        
        var isIn = false;
        for (var j = 0; j < startConnectionsList.length; j++) {
            var startConn = startConnectionsList[j];

            //console.log("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");

            var sourceContainer = $("#" + newConn.sourceId)[0];
            var source = $(sourceContainer).find(".le-mapping-simple-element")[0];
            var newConnSoureId = $(source).attr("id");

            var targetContainer = $("#" + newConn.targetId)[0];
            var target = $(targetContainer).find(".le-mapping-simple-element")[0];
            var newConnTargetId = $(target).attr("id");

            var startConnSoureId = $(startConn).attr("sourceId");
            var startConnTargetId = $(startConn).attr("targetId");

            //console.log(newConnSoureId);
            //console.log(newConnTargetId);
            //console.log("sT" + startConnSoureId);
            //console.log("sT" + startConnTargetId);

            if (newConnSoureId === startConnSoureId && newConnTargetId === startConnTargetId) {
                isIn = true;
            }
        }

        if (!isIn) {
            allNewConnections.push(newConn);
        }

        //console.log(allNewConnections);
    }

    return allNewConnections.length;
}

function updateSaveOptions(parentId, activate) {

    var saveBt = $("#" + parentId).find(".saveButton")[0];

    if (activate) {
        $(saveBt).removeAttr("disabled");
        $(saveBt).removeClass("bx-disabled");
    } else {
        $(saveBt).attr("disabled", "disabled");
        $(saveBt).addClass("bx-disabled");
    }
}
