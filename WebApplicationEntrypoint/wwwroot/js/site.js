/// <reference path="../lib/angular.min.js" />
/// <reference path="../lib/linq.d.ts" />


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

        $timeout(function () {
            $scope.notifications.shift();
        }, 5000);
    });

});


rootApp.controller("PublisherController", function ($scope, $http, $interval) {

    $scope.total = 0;
    $scope.publishers = [];
    $scope.newPublisher = { messagesPerSecond: 1, size: 1 };

    $scope.getPublishers = (function () {
        $http.get("/api/Publisher").then(function (response) {
            $scope.publishers = response.data;

            $scope.total = Enumerable.from($scope.publishers).sum(function(it){ return it.messagesPerSecond; });
        })
    });

    $scope.deletePublisher = (function (publisher) {
        publisher.removing = true;
        $http.delete("/api/Publisher/" + publisher.id).then(function (response) {
            $scope.$parent.showNotification({ title: `Carga de trabalho ${publisher.id.split('-')[0]} deletada`, text: `A carga de trabalho ${publisher.id} foi deletada com sucesso`, icon: "check-circle" });
        }, function (response) {
            $scope.$parent.showNotification({ title: "Falha", text: "Não foi possível deletar", icon: "exclamation-circle" });
        })
    });

    $scope.add = (function (publisher) {

        $http.put("/api/Publisher/", {}, { params: publisher }).then(function (response) {

            var text = publisher.size === 1 ? "Uma carga de trabalho adicionada." : `${publisher.size} cargas de trabalho adicionadas.`;
            var title = publisher.size === 1 ? "Carga de trabalho adicionada" : "Cargas de trabalho adicionadas";

            $scope.$parent.showNotification({ title: title, text: text, icon: "check-circle" });

        }, function (response) {
            $scope.$parent.showNotification({ title: "Falha", text: "Não foi possível adicionar", icon: "exclamation-circle" });
        })
    });

    $interval(function () { $scope.getPublishers(); }, 2000);

    $scope.getPublishers();

});

rootApp.controller("ConsumerController", function ($scope, $http, $interval) {

    $scope.total = 0;
    $scope.consumers = [];
    
    $scope.newConsumer = { messagesPerSecond: 1, size: 1 };

    $scope.getConsumers = (function () {
        $http.get("/api/Consumer").then(function (response) {
            $scope.consumers = response.data;

            $scope.total = Enumerable.from($scope.consumers).sum(function(it){ return it.messagesPerSecond; });
        })
    });

    $scope.deleteConsumer = (function (consumer) {
        consumer.removing = true;
        $http.delete("/api/Consumer/" + consumer.id).then(function (response) {
            $scope.$parent.showNotification({ title: `Processador ${consumer.id.split('-')[0]} deletado`, text: `O processador ${consumer.id} foi deletado com sucesso`, icon: "check-circle" });
        }, function (response) {
            $scope.$parent.showNotification({ title: "Falha", text: "Não foi possível deletar", icon: "exclamation-circle" });
        })
    });

    $scope.add = (function (consumer) {

        $http.put("/api/Consumer/", {}, { params: consumer }).then(function (response) {

            var text = consumer.size === 1 ? "Um novo processador foi adicionado." : `${consumer.size} processadores foram adicionados.`;
            var title = consumer.size === 1 ? "Processador adicionado" : "Processadores adicionados";

            $scope.$parent.showNotification({ title: title, text: text, icon: "check-circle" });
        }, function (response) {
            $scope.$parent.showNotification({ title: "Falha", text: "Não foi possível adicionar", icon: "exclamation-circle" });
        })
    });

    $interval(function () { $scope.getConsumers(); }, 2000);

    $scope.getConsumers();

});