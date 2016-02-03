$('body').attr('ng-app', 'treeRepeatApp');
if (!dataModels) {
    var dataModels = new Object();
    var App = angular.module('treeRepeatApp', ['sf.treeRepeat', 'ngDragDrop']);

    App.controller("SimpleCtrl", function ($scope, $attrs, $timeout) {
        //console.log($attrs);
        $scope.treeData = { name: "file root", sequence: dataModels[$attrs["model"]] };
        $scope.myDrop = function (event, ui) {
            $scope[$attrs["listmodel"]].push(ui.draggable)
        }
       $scope[$attrs["listmodel"]] = [];

        console.log($attrs);
        $scope.hideMe = function () {
            return $scope[$attrs["listmodel"]].length > 0;
        }
    });
}