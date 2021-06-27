/// <reference path="../lib/angular.min.js" />
var rootApp = angular.module('gaGOApp', []);

rootApp.controller("RootController", function ($scope) {

    $scope.showSidebar = true;

    $scope.showProfile = false;

});

rootApp.controller("NotificationController", function ($scope, $timeout) {

    $scope.notifications = [];

/*
        { title: "A", text: "B", icon: "check-circle" },
        { title: "A", text: "B", icon: "annotation" },
        { title: "A", text: "B", icon: "chat-alt-2" },
        { title: "A", text: "B", icon: "cube-transparent" },
        { title: "A", text: "B", icon: "cube" },
        { title: "A", text: "B", icon: "exclamation" },
        { title: "A", text: "B", icon: "exclamation-circle" }

        Forma de uso
        $scope.$parent.showNotification();
*/

    $scope.$parent.showNotification = (function (notification) {
        $scope.notifications.push(notification);

        $timeout(function(){
            $scope.notifications.shift();
        }, 5000);
    });

});


rootApp.controller("HomeController", function ($scope, $timeout) {

    $scope.teste = "A";



});