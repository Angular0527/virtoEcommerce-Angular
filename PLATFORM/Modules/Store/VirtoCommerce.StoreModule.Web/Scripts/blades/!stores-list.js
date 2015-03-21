﻿angular.module('virtoCommerce.storeModule.blades', [
    'virtoCommerce.storeModule.resources.stores',
    'virtoCommerce.storeModule.wizards.newStore',
    // 'catalogModule.resources.catalogs'
    'ngSanitize'
])
.controller('storesListController', ['$scope', 'stores', 'bladeNavigationService', function ($scope, stores, bladeNavigationService) {
    $scope.selectedNodeId = null;

    $scope.blade.refresh = function () {
        $scope.blade.isLoading = true;
        stores.query({}, function (data) {
            $scope.blade.isLoading = false;
            $scope.blade.currentEntities = data;
        });
    }

    $scope.blade.openBlade = function (data) {
        $scope.selectedNodeId = data.id;

        var newBlade = {
            id: 'storeDetails',
            currentEntityId: data.id,
            // currentEntity: data,
            title: data.name,
            subtitle: 'Store details',
            controller: 'storeDetailController',
            template: 'Modules/$(VirtoCommerce.Store)/Scripts/blades/store-detail.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, $scope.blade);
    }

    function openBladeNew() {
        $scope.selectedNodeId = null;

        var newBlade = {
            id: 'storeDetails',
            // currentEntityId: data.id,
            currentEntity: {},
            title: 'New Store',
            subtitle: 'Create new Store',
            controller: 'newStoreWizardController',
            template: 'Modules/$(VirtoCommerce.Store)/Scripts/wizards/newStore/new-store-wizard.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, $scope.blade);
    }
    
    $scope.blade.onClose = function (closeCallback) {
        closeChildrenBlades();
        closeCallback();
    };

    function closeChildrenBlades() {
        angular.forEach($scope.blade.childrenBlades.slice(), function (child) {
            bladeNavigationService.closeBlade(child);
        });
    }

    $scope.bladeHeadIco = 'fa fa-archive';

    $scope.bladeToolbarCommands = [
        {
            name: "Refresh", icon: 'fa fa-refresh',
            executeMethod: function () {
                $scope.blade.refresh();
            },
            canExecuteMethod: function () {
                return true;
            }
        },
        {
            name: "Add", icon: 'fa fa-plus',
            executeMethod: function () {
                openBladeNew();
            },
            canExecuteMethod: function () {
                return true;
            }
        }
    ];

    $scope.blade.refresh();
}]);
