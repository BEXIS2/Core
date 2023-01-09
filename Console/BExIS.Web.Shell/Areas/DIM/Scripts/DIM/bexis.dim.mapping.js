var connections = [];
var connectionParent = {};

$(window)
    .resize(function () {
        setTimeout(function () {
            reloadAllConnections();
        },
            100);
    });

$(document)
    .ready(function () {
        //create all connections in ui
        reloadAllConnections();
    });

function iconClick(e) {
    //console.log("CLICK");
    $(e).toggleClass("bx-angle-double-down bx-angle-double-up");
    var container = $(e).parents(".le-container");
    //console.log(container);
    $(container).find(".le-container-content").slideToggle();
    $(container).addClass("selected-mapping-element");
    setTimeout(reloadAllConnections, 100);
};

function iconTransformtionRuleClick(e) {
    ////console.log(e);

    $(e).toggleClass("bx-angle-double-down bx-angle-double-up");
    var container = $(e).parents(".mapping-container-transformation-rule")[0];
    ////console.log(container);

    $(container).find(".mapping-container-transformation-rule-content").slideToggle();
    $(container).toggleClass("selected-mapping-element");

    setTimeout(reloadAllConnections, 500);
};

function leSimpleSelectorClick(e) {
    ////console.log(e);
    var parent = $(e).parents(".le-simple")[0];
    ////console.log(parent);
    var info = $(parent).find(".le-simple-info")[0];
    ////console.log(info);
    ////console.log($(info).find("#Id"));

    var id = $(info).find("#Id").text();
    var type = $(info).find("#Type").text();
    var elementid = $(info).find("#ElementId").text();
    var position = $(info).find("#Position").text();
    var complexity = $(info).find("#Complexity").text();
    var name = $(info).find("#Name").text();
    var xpath = $(info).find("#XPath").text();

    ////console.log("xpath" + xpath);

    var le =
    {
        "Id": id,
        "Name": name,
        "ElementId": elementid,
        "Type": type,
        "Position": position,
        "Complexity": complexity,
        "XPath": xpath
    }

    ////console.log(le);

    $.ajax({
        type: "POST",
        url: "/DIM/Mapping/AddMappingElement",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        data: JSON.stringify(le),
        success: function (data) {
            ////console.log("type : "+type);
            if (position.toLowerCase() === "source") {
                $("#emptySourceContainer").replaceWith(data);

                //deactivacte all source add icons
                disableAddicons("Source");
            } else {
                $("#emptyTargetContainer").replaceWith(data);
                //deactivacte all source add icons
                disableAddicons("Target");
            }

            updateSaveOptionOnNewContainer();
            reloadAllConnections();

            var trashBT = $("#mapping_container_0").find(".bx-trash")
            console.log(trashBT);
            $(trashBT).show();
            console.log(trashBT);
        },
        error: function (data) { alert("error") }
    });
};

function disableAddicons(key) {
    $("." + key)
        .find(".le-simple-selector")
        .each(function () {
            ////console.log(this);
            $(this).addClass("bx-disabled");
            $(this).removeClass("function");

            if (!$(this).attr("disabled")) {
                $(this).attr("disabled", "disabled");
            }
        });
}

function enableAddicons(key) {
    $("." + key)
        .find(".le-simple-selector")
        .each(function () {
            ////console.log(this);
            $(this).removeClass("bx-disabled");
            $(this).addClass("function");

            $(this).removeAttr("disabled");
        });
}

function updateSaveOptionOnNewContainer() {
    //alert("updateSaveOptionOnNewContainer");
    if ($("#emptySourceContainer").length === 0 && $("#emptyTargetContainer").length === 0) {
        $("#newMapContainer .mapping-settings").show();
        //$(deleteBt).hide();

        //alert("updateSaveOptionOnNewContainer INSIDE");
        //initJSPLUMB("mapping_container_0");
    } else {
        //$(deleteBt).show();
    }
}

function deleteComplexMappingElement(e) {
    var parent = $(e).parents(".mapping_container_child")[0];
    ////console.log(parent);

    if ($(parent).hasClass("mapping_container_source")) {
        //add source
        $(parent).find(".le-mapping-complex").remove();
        $(parent).append("<div id='emptySourceContainer'> <b>SOURCE</b></div>");

        enableAddicons("Source");

        if ($("#emptySourceContainer").length !== 0 || $("#emptyTargetContainer").length !== 0) {
            $("#newMapContainer .mapping-settings").hide();
        }
    }

    if ($(parent).hasClass("mapping_container_target")) {
        $(parent).find(".le-mapping-complex").remove();
        //add Target
        $(parent).append("<div id='emptyTargetContainer'> <b>Target</b></div>");

        enableAddicons("Target");

        if ($("#emptySourceContainer").length !== 0 || $("#emptyTargetContainer").length !== 0) {
            $("#newMapContainer .mapping-settings").hide();
        }
    }

    reloadAllConnections();
    updateSaveOptionOnNewContainer();
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
        "Complexity": complexity
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
    var xpath = $(info).find("#XPath").text();

    var obj =
    {
        "Id": id,
        "Name": name,
        "ElementId": elementid,
        "Type": type,
        "Position": position,
        "Complexity": complexity,
        "Parent": element,
        "XPath": xpath
    }

    ////console.log("LINK ELEMENT");
    ////console.log(obj);

    return obj;
}

function createTransformationRule(id, regexPattern, mask) {
    /**
     * public long Id { get; set; }
        public string RegEx { get; set; }
     */
    var obj =
    {
        "Id": id,
        "RegEx": regexPattern,
        "Mask": mask
    }

    return obj;
}

function createSimpleMapping(conn, sourceParent, targetParent, parentMappingId) {
    ////console.log("create simple mappings");
    ////console.log(conn);
    ////console.log(conn.id);
    ////console.log(conn.sourceId);
    ////console.log(conn.targetId);

    var source = $("#" + conn.sourceId);
    var sourceInfo = $(source).find(".le-simple-info")[0];

    var target = $("#" + conn.targetId);
    var targetInfo = $(target).find(".le-simple-info")[0];

    var sourceObj = createElement(sourceInfo, sourceParent);
    var targetObj = createElement(targetInfo, targetParent);

    var mappingId = 0;

    // get Mask

    var trId = 0;
    var regexPattern = "";
    var mask = "";

    //get rull based on conn
    var rule = findRuleFromConn(conn);
    ////console.log("RULE");
    ////console.log(rule);

    var ruleId = $(rule).attr("id");
    ////console.log(ruleId);

    if (ruleId !== null && ruleId !== undefined) {
        trId = ruleId.split("_")[0];
    }

    regexPattern = $("#" + ruleId).find("#RegExPattern").val();
    mask = $("#" + ruleId).find("#Mask").val();

    ////console.log(regexPattern);
    var transformationRuleObj = createTransformationRule(trId, regexPattern, mask);

    ////console.log(transformationRuleObj);

    //var parent = $(source).parents(".mapping-container")[0];

    var obj =
    {
        "Id": mappingId,
        "ParentId": parentMappingId,
        "Source": sourceObj,
        "Target": targetObj,
        "TransformationRule": transformationRuleObj
    }

    return obj;
}

function saveMapping(e, create) {
    //console.log("mapping");
    //console.log("************************************");
    ////console.log(e);
    var parent = $(e).parents(".mapping-container")[0];

    var bothDirection = $(parent).find(".both-directions")[0];
    var isBothDirection = false;

    if ($(bothDirection).is(":checked")) {
        isBothDirection = true;
    }

   
    var mappingId = $(parent).attr("id").split("_")[2];
    var parentMappingId = $(parent).attr("parent");
    ////console.log(parent);

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

    for (var j = 0; j < connections.length; j++) {
        //console.log(connections[j].id + "===" + parent.id);

        if (connections[j].id === parent.id) {
            //console.log("-- >get connection");
            parentMapping = connections[j];
            break;
        }
    }

    if (parentMapping !== null) {
        ////console.log("save mapping ");
        ////console.log(parentMapping.connections.length);

        for (var i = 0; i < parentMapping.connections.length; i++) {
            ////console.log("create MAPPING");
            ////console.log(parent);
            ////console.log(parentMapping.connections[i]);
            ////console.log(source);
            ////console.log(target);

            var sm = createSimpleMapping(
                parentMapping.connections[i],
                source,
                target,
                mappingId
            );
            simpleMappings.push(sm);
        }
    }

    var newMapping = false;

    if ($(parent).attr("id") === "mapping_container_0") {
        newMapping = true;
    }

    var model =
    {
        "Id": mappingId,
        "ParentId": parentMappingId,
        "Source": source,
        "Target": target,
        "SimpleMappings": simpleMappings
    }

    var sendData =
    {
        model,
        both: isBothDirection
    }

    console.log(sendData);

    $.ajax({
        type: "POST",
        url: "/DIM/Mapping/SaveMapping",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        data: JSON.stringify(sendData),
        success: function (data) {
            //console.log("DATA");

            $(parent).remove();

            $('#dim-mapping-middle').prepend(data);

            var pid = $(parent).attr("id");

            //create empty
            $.get("/DIM/Mapping/LoadEmptyMapping",
                function (response) {
                    //if new mapping parent container id = 0
                    //but the new created container needs to update
                    // find the new id
                    if (newMapping) {
                        var allContainer = $('#dim-mapping-middle').find(".mapping-container");
                        //console.log("AllContainer");
                        //console.log(allContainer);
                        var newContainer = allContainer[0];
                        pid = $(newContainer).attr("Id");
                        //alert(pid);
                    }

                    removeParentFromConnections(pid);
                    //initJSPLUMB(pid);
                    //console.log(pid);
                    //alert(pid);

                    $('#dim-mapping-middle #newMapContainer').remove();
                    $('#dim-mapping-middle').prepend("<div id='newMapContainer'></div>");
                    $('#dim-mapping-middle #newMapContainer').append(response);

                    removeParentFromConnections("mapping_container_0");

                    //remove connection?
                    ////console.log("remove connections from 0 container");

                    ////console.log("RESET ALL CONNECTIONS");
                    reloadAllConnections();
                });

            enableAddicons("Target");
            enableAddicons("Source");
            updateSaveOptionOnNewContainer();
            updateSaveOptions($(parent).attr("id"), false);
        },
        error: function (data) {
            console.log(data);
            alert("error")
        }
    });
}

function deleteMapping(e) {
    var parent = $(e).parents(".mapping-container")[0];
    ////console.log(parent);

    var idArray = $(parent).attr("id").split("_");
    ////console.log(idArray);

    var id = idArray[idArray.length - 1];
    ////console.log(id);

    $.post('/DIM/Mapping/DeleteMapping',
        { id: id },
        function (response) {
            if (response === true) {
                $(parent).remove();

                ////console.log("RESET ALL CONNECTIONS");
                ////console.log("CONNECTIONS BEFORE DELETE MAPPINGS from GLOBAL Connections");
                ////console.log(connections);
                removeParentFromConnections($(parent).attr("id"));
                reloadAllConnections();

                enableAddicons("Target");
                enableAddicons("Source");
                updateSaveOptionOnNewContainer();
            } else {
                alert(response);
            };
        });
}

function updateParentConnection(conn, parentId, jsPlumbInstance) {
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
            connections: [],
            jsPlumbInstance: jsPlumbInstance
        }
        connectionParent.connections.push(conn);

        connections.push(connectionParent);
    }
}

function removeParentConnection(conn, parentId) {
    for (var i = 0; i < connections.length; i++) {
        var existParent = connections[i];
        var idx;
        console.log("parentId: " + parentId);
        if (existParent.id === parentId) {
            for (var j = 0; j < existParent.connections.length; j++) {
                if (existParent.connections[i] === conn) {
                    idx = i;
                    console.log("i: " + i);
                    break;
                }
            }

            console.log("idx: " + idx);

            if (idx !== -1) {
                console.log(existParent.connections);
                existParent.connections.splice(idx, 1);
                console.log(existParent.connections);
                break;
            }
        }
    }

    console.log("after remove parent");
}

function updateConnections(conn, remove, parentId, jsPlumbInstance) {
    if (!remove) {
        updateParentConnection(conn, parentId, jsPlumbInstance);
    } else {
        console.log("remove");
        console.log(parentId);
        removeParentConnection(conn, parentId);
    }

    //update save options
    if (connectionsChanged(parentId)) {
        updateSaveOptions(parentId, true);
    } else {
        updateSaveOptions(parentId, false);
    }

    ////console.log(connections);
    ////console.log(connections.length);
}

function removeParentFromConnections(parentId) {
    //console.log("INSIDE DELETE ");
    //console.log("parentid: " + parentId);

    ////console.log("------------------------------");
    ////console.log(connections);
    var deleteIndex = -1;

    for (var i = 0; i < connections.length; i++) {
        var existParent = connections[i];
        if (existParent.id === parentId) {
            deleteIndex = i;
        }
    }
    //console.log(deleteIndex);
    if (deleteIndex > -1) {
        connections.splice(deleteIndex, 1);
    }
}

function removeAllFromConnections() {
    //console.log("INSIDE DELETE ");
    //console.log("parentid: " + parentId);

    $("svg").remove();
    //$(".jtk-endpoint").remove();
    $(".jtk-overlay").remove();
    //$("path").remove();

    //console.log("------------------------------");
    //console.log(connections);
    if (connections.length > 0) {
        connections.forEach(
            function (c) {
                //console.log("parent connection");
                //console.log("------");

                if (c.connections.length > 0) {
                    //console.log("childs :" + c.connections.length);
                    c.connections.splice(0, c.connections.length);
                    //console.log("childs removed");

                    //console.log("childs :" + c.connections.length);
                }
            }
        )

        // connections.splice(0, connections.length - 1);
    }
}

function getInstance(parentid) {
    var instance = window.instance = jsPlumb.getInstance({
        ConnectionOverlays: [
            [
                "Arrow", {
                    location: 1,
                    visible: true,
                    width: 11,
                    length: 11,
                    id: "ARROW"
                }
            ],
            [
                "Label", {
                    id: "label",
                    cssClass: "aLabel",
                    events: {
                        tap: function (e) { }
                    },
                    text: "test"
                }
            ]
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

    return instance;
}

/**
 * Call this function when the connections need to set new because of postion
 * arrangements of the containers
 *
 */
function reloadAllConnections() {
    console.log("start reload:");
    console.log("*********************");
    removeAllFromConnections();

    $(".mapping-container")
        .each(function () {
            var id = $(this).attr("id");

            // delete before init

            var expandContainer = $("#" + id).find(".mapping-container-expand")[0];

            if ($(expandContainer).is(":visible")) {
                initJSPLUMB(id);
                console.log("reload:" + id);
            }
        });

    console.log("end");
}

function initJSPLUMB(parentid) {
    jsPlumb.ready(function () {
        //console.log("init jsplumb");
        //console.log("---------------------");
        //console.log("parent id :");
        //console.log(parentid);

        var instance = window.instance = getInstance(parentid);
        ////console.log("instance :");
        ////console.log(instance);

        var init = function (connection) {
            connection.getOverlay("label").setLabel("open");
        };

        instance.registerConnectionType("basic",
            {
                endpoint: ["Rectangle", { width: 5, height: 5 }],
                anchor: "Continuous",
                connector: ["Flowchart", { stub: [40, 60], gap: 10, cornerRadius: 5, alwaysRespectStubs: true }]
            });

        // suspend drawing and initialise.
        instance.batch(function () {
            // bind to connection/connectionDetached events, and update the list of connections on screen.
            instance.bind("connection",
                function (info, originalEvent) {
                    ////console.log("connection");

                    ////console.log(info);
                    ////console.log(instance);

                    updateConnections(info.connection, false, parentid, instance);
                });

            instance.bind("connectionDetached",
                function (info, originalEvent) {
                    console.log("connectionDetached");

                    updateConnections(info.connection, true, parentid, instance);
                });

            instance.bind("connectionMoved",
                function (info, originalEvent) {
                    ////console.log("connectionMoved");
                    //  only remove here, because a 'connection' event is also fired.
                    // in a future release of jsplumb this extra connection event will not
                    // be fired.
                    updateConnections(info.connection, true, parentid, instance);
                });

            instance.bind("click",
                function (component, originalEvent) {
                    //hide/Show TransformationRules
                    changeViewOfTransformationRule(component);
                });

            //set source
            // get the list of ".le-mapping-simple-selector-source" elements.
            var simpleSources = jsPlumb.getSelector("#" + parentid + " .le-mapping-simple-selector-source");

            if (simpleSources.length > 0) {
                //console.log("simpleSources :");
                //console.log(simpleSources);

                instance.makeSource(simpleSources,
                    {
                        anchor: "Right",
                        endpoint: ["Rectangle", { width: 5, height: 5 }],
                        cssclass: "dim-mapping-anchor"
                    });
            }

            //set targets
            // get the list of ".le-mapping-simple-selector-source" elements.
            var simpleTargets = jsPlumb.getSelector("#" + parentid + " .le-mapping-simple-selector-target");

            if (simpleTargets.length > 0) {
                //console.log("simpleTargets :");
                //console.log(simpleTargets);

                instance.makeTarget(simpleTargets,
                    {
                        anchor: "Left",
                        endpoint: ["Rectangle", { width: 5, height: 5, color: "white" }]
                    });
            }

            // listen for new connections; initialise them the same way we initialise the connections at startup.
            instance.bind("connection",
                function (connInfo, originalEvent) {
                    init(connInfo.connection);
                });

            //create Exiting connections
            addConnections(instance, parentid);
        });

        //jsPlumb.fire("jsPlumbDemoLoaded", instance);
        //console.log("---------------------");
    });
};

function changeViewOfTransformationRule(conn) {
    var rule = findRuleFromConn(conn);

    $(rule).find(".toogle-icon").trigger("click");

    //setTimeout(function () {
    //    var ruleContent = $(rule).find(".mapping-container-transformation-rule-content")[0];

    //    if (conn.getOverlay("label").getLabel() === "open") {
    //        conn.getOverlay("label").setLabel("close");
    //    } else {
    //        conn.getOverlay("label").setLabel("open");

    //    }
    //},100);
}

function findRuleFromConn(conn) {
    var sourceContainer = $("#" + conn.sourceId)[0];
    var source = $(sourceContainer).find(".le-mapping-simple-element")[0];
    var connSoureId = $(source).attr("id");

    //alert("test");

    //console.log("*FIND RULES FROM CONNECTIONS**");
    //console.log(conn);
    //console.log(sourceContainer);
    //console.log(source);
    //console.log(connSoureId);

    var targetContainer = $("#" + conn.targetId)[0];
    var target = $(targetContainer).find(".le-mapping-simple-element")[0];
    var connTargetId = $(target).attr("id");

    ////console.log(conn);
    connSoureId = connSoureId.replace("_MappingSimpleLinkElement", "");
    connTargetId = connTargetId.replace("_MappingSimpleLinkElement", "");

    ////console.log("FIND RULE");
    ////console.log(conn);

    //mapping_container_transformation
    var mapcontainer = $(sourceContainer).parents(".mapping-container")[0];
    var map_transformation_container = $(mapcontainer).find(".mapping-container-transformation-rule");

    var x = null;

    map_transformation_container.each(function () {
        var ruleSourceId = $(this).attr("sourceId").replace("_TransformationRuleItem", "");
        var ruleTargetId = $(this).attr("targetId").replace("_TransformationRuleItem", "");

        ////console.log(connSoureId);
        ////console.log(connTargetId);
        ////console.log(ruleSourceId);
        ////console.log(ruleTargetId);
        if (ruleSourceId === connSoureId && ruleTargetId === connTargetId) {
            ////console.log("match");
            ////console.log(this);

            x = this;
        }
    });

    return x;
}

function addConnections(jsPlumbInstance, parentid) {
    var parent = $("#" + parentid);
    var simpleMappings = parent.find(".mapping-container-simple-hidden-mapping");
    //console.log("add connections *********************");

    //console.log(simpleMappings);

    for (var i = 0; i < simpleMappings.length; i++) {
        ////console.log(sm);
        ////console.log($(sm).attr("sourceId"));
        ////console.log($(sm).attr("targetId"));

        var sm = simpleMappings[i];

        var sourceid = $(sm).attr("sourceId");
        var targetid = $(sm).attr("targetId");

        var source = $("#" + parentid).find("#" + sourceid).parents(".le-simple-selector")[0];
        var target = $("#" + parentid).find("#" + targetid).parents(".le-simple-selector")[0];

        jsPlumbInstance.connect({
            source: source,
            target: target,
            type: "basic"
        });
    }
}

function getContainerSize() {
    return $(window).height() - $('.navbar').outerHeight() - $('#footer').outerHeight() - 90;
}

function connectionsChanged(parentId) {
    var newConn = countAllNewConnections(parentId);
    var deletedConn = countAllDeletedConnections(parentId);

    ////console.log(newConn);
    ////console.log(deletedConn);

    if (newConn === 0 && deletedConn === 0) {
        return false;
    }

    return true;
}

function countAllNewConnections(parentId) {
    ////console.log("+++++++++++++++++++++++++++++++++++");
    ////console.log("countAllNewConnections");

    var allNewConnections = [];
    var allConnections = [];

    for (var i = 0; i < connections.length; i++) {
        var existParent = connections[i];
        if (existParent.id === parentId) {
            allConnections = existParent.connections;
            ////console.log(allConnections);
        }
    }

    var parent = $("#" + parentId);
    var startConnectionsList = $(parent).find(".mapping-container-simple-hidden-mapping");
    ////console.log(startConnectionsList);

    for (var l = 0; l < allConnections.length; l++) {
        var newConn = allConnections[l];

        var isIn = false;
        for (var j = 0; j < startConnectionsList.length; j++) {
            var startConn = startConnectionsList[j];

            ////console.log("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");

            var sourceContainer = $("#" + newConn.sourceId)[0];
            var source = $(sourceContainer).find(".le-mapping-simple-element")[0];
            var newConnSoureId = $(source).attr("id");

            var targetContainer = $("#" + newConn.targetId)[0];
            var target = $(targetContainer).find(".le-mapping-simple-element")[0];
            var newConnTargetId = $(target).attr("id");

            var startConnSoureId = $(startConn).attr("sourceId");
            var startConnTargetId = $(startConn).attr("targetId");

            ////console.log(newConnSoureId);
            ////console.log(newConnTargetId);
            ////console.log("sT" + startConnSoureId);
            ////console.log("sT" + startConnTargetId);

            if (newConnSoureId === startConnSoureId && newConnTargetId === startConnTargetId) {
                isIn = true;
            }
        }

        if (!isIn) {
            allNewConnections.push(newConn);
        }

        ////console.log(allNewConnections);
    }

    return allNewConnections.length;
}

function countAllDeletedConnections(parentId) {
    ////console.log("+++++++++++++++++++++++++++++++++++");
    ////console.log("countAllDeletedConnections");

    var allDeletedConnections = [];
    var allConnections = [];

    for (var i = 0; i < connections.length; i++) {
        var existParent = connections[i];
        if (existParent.id === parentId) {
            allConnections = existParent.connections;
            ////console.log(allConnections);
        }
    }

    var parent = $("#" + parentId);
    var startConnectionsList = $(parent).find(".mapping-container-simple-hidden-mapping");
    ////console.log(startConnectionsList);

    for (var j = 0; j < startConnectionsList.length; j++) {
        var startConn = startConnectionsList[j];

        var startConnSoureId = $(startConn).attr("sourceId");
        var startConnTargetId = $(startConn).attr("targetId");

        var newConn;

        var isIn = false;

        for (var l = 0; l < allConnections.length; l++) {
            newConn = allConnections[l];

            var targetContainer = $("#" + newConn.targetId)[0];
            var target = $(targetContainer).find(".le-mapping-simple-element")[0];
            var newConnTargetId = $(target).attr("id");

            var sourceContainer = $("#" + newConn.sourceId)[0];
            var source = $(sourceContainer).find(".le-mapping-simple-element")[0];
            var newConnSoureId = $(source).attr("id");

            ////console.log("--------1------");

            ////console.log(newConnSoureId);
            ////console.log(newConnTargetId);
            ////console.log("---------------");
            ////console.log(startConnSoureId);
            ////console.log(startConnTargetId);

            if (newConnSoureId === startConnSoureId && newConnTargetId === startConnTargetId) {
                isIn = true;
            }
        }

        ////console.log("round finished");
        ////console.log(isIn);

        if (!isIn) {
            ////console.log(newConn);
            if (newConn !== null) {
                allDeletedConnections.push(newConn);
            }
        }

        ////console.log("after add to delete list");
        ////console.log(allDeletedConnections);
    }

    return allDeletedConnections.length;
}

function updateSaveOptions(parentId, activate) {
    var saveBt = $("#" + parentId).find(".saveButton")[0];

    //if (true) {
    $(saveBt).removeAttr("disabled");
    $(saveBt).removeClass("bx-disabled");
    //} else {
    //    $(saveBt).attr("disabled", "disabled");
    //    $(saveBt).addClass("bx-disabled");
    //}
}

$(".mapping-container").dblclick(function () {
    var parent = $(this);
    var parentid = $(parent).attr("id");

    if (parentid !== "mapping_container_0") {
        $($("#" + parentid).find(".mapping-container-expand")).toggle();
        $($("#" + parentid).find(".mapping-container-collapse")).toggle();
        $($("#" + parentid).find(".jtk-overlay")).toggle();
        $($("#" + parentid).find(".jtk-connector")).toggle();
        $($("#" + parentid).find(".jtk-endpoint")).toggle();

        reloadAllConnections();
    }
})

function toggleArrows() {
    $(".jtk-endpoint").toggle();
    $(".jtk-connector").toggle();
}

/**
 * *****************************************************
 * @param {any} elems
 * @param {any} terms
 * @param {any} types
 */

function initIsotope(identiferSimple) {
    // init Isotope
    console.log("init isotope");

    $(identifer).isotope({
        itemSelector: identiferSimple,
        layoutMode: 'vertical',
        transitionDuration: '0.5s',
        getSortData: {
            name: '#Name',
            category: '[data-category]'
        }
    });
}

function concatValues(obj) {
    var value = '';
    for (var prop in obj) {
        if (prop == 0) {
            value = '.' + obj[prop];
        }
        else {
            value += ', .' + obj[prop];
        }
    }
    return value;
}

$('.le-search').keyup(function () {
    $('.le-search').trigger("change");
});

$('.le-search').change(function (e) {
    var parent = $(e.target).parents(".le-root")[0];

    //search input
    var element = $("#" + e.target.id);
    var targetid = e.target.id;

    //get position
    var tmpArray = targetid.split('-');
    var position = tmpArray[tmpArray.length - 1];

    //class for all simple elements
    var identiferSimple = ".le-simple-" + position;
    //class for container
    var currentidentifer = ".le-container-content-" + position;

    searchFilter = [];

    //get input field as a list
    var value = $('#' + targetid).val().trim();
    var terms = value.split(' ');

    //get checked types
    var types = [];
    $(parent).find(".prefilter:checked").each(function (index, element) {
        // element == this
        //console.log(element);
        var id = $(element).attr("id");

        types.push(id);
    });

    //get all simple elements

    var elems = $(identiferSimple);
    console.log("terms");
    console.log(terms);
    console.log("elems");
    console.log(elems);
    console.log("types");
    console.log(types);

    //filter based on terms and types
    searchFilter = filter(elems, terms, types);

    //console.log(searchFilter);
    console.log(concatValues(searchFilter));
    $(currentidentifer).isotope({ filter: concatValues(searchFilter) });
});

$(".prefilter").change(function (e) {
    //parent
    var parent = $(e.target).parents(".le-root")[0];
    console.log(parent);

    //search input
    var element = $(parent).find(".le-search")[0];
    var targetid = element.id;

    console.log(element);

    //get position
    var tmpArray = targetid.split('-');
    var position = tmpArray[tmpArray.length - 1];

    //class for all simple elements
    var identiferSimple = ".le-simple-" + position;
    console.log("identiferSimple : " + identiferSimple);
    //class for container
    var currentidentifer = ".le-container-content-" + position;

    searchFilter = [];

    //get input field as a list
    $('#' + targetid).val($('#' + targetid).val().trim());
    var terms = $('#' + targetid).val().split(' ');

    //get checked types
    console.log("start get types");
    console.log("****************");
    var types = []
    $(parent).find(".prefilter:checked").each(function (index, element) {
        // element == this
        console.log(element);
        var id = $(element).attr("id");

        types.push(id);
    });

    //get all simple elements
    var elems = $(identiferSimple);
    console.log("terms");
    console.log(terms);
    console.log("elems");
    console.log(elems);
    console.log("types");
    console.log(types);

    //console.log(elems);
    //filter based on terms and types
    searchFilter = filter(elems, terms, types);

    console.log(searchFilter);
    console.log(concatValues(searchFilter));
    $(currentidentifer).isotope({ filter: concatValues(searchFilter) });
})

function filter(elems, terms, types) {
    searchFilter = [];
    var temp = [];

    if (terms.length > 0 && terms[0] !== '') {
        for (var j = 0; j < terms.length; j++) {
            terms[j] = terms[j].toLowerCase();

            for (var i = 0; i < elems.length; i++) {
                var text = $(elems[i]).find('.le-simple-header').text().trim();
                var type = $(elems[i]).find('.fa-info').attr("type").trim();

                var id = $(elems[i]).attr("id");

                if (text.toLowerCase().indexOf(terms[j]) !== -1) {
                    //text is matched, now check the type
                    if (types.length > 0) {
                        for (var x = 0; x < types.length; x++) {
                            var t = types[x].toLowerCase();

                            if (type.toLowerCase().indexOf(t) !== -1) {
                                temp.push(id);
                            }
                            else if (t === "type" && type.toLowerCase().indexOf("complex") !== -1) {
                                temp.push(id);
                            }
                        }
                    }
                }
            }

            if (searchFilter.length === 0) {
                searchFilter = temp.slice();
            }

            if (temp.length === 0) {
                searchFilter.push("0");
            }
        }
    }
    //filter only types
    else if (types.length > 0) {
        for (var x = 0; x < types.length; x++) {
            for (var i = 0; i < elems.length; i++) {
                //var text = $(elems[i]).find('.le-simple-header').text().trim();
                var type = $(elems[i]).find('.fa-info').attr("type").trim();
                var id = $(elems[i]).attr("id");

                var t = types[x].toLowerCase();

                if (type.toLowerCase().indexOf(t) !== -1) {
                    console.log("yeah");
                    temp.push(id);
                } else if (t === "type" && type.toLowerCase().indexOf("complex") !== -1) {
                    temp.push(id);
                }
            }
        }

        if (searchFilter.length === 0) {
            searchFilter = temp.slice();
        }

        if (temp.length === 0) {
            searchFilter.push("0");
        }
    }
    else {
        searchFilter = [];
        searchFilter.push("0");
    }

    return searchFilter;
}