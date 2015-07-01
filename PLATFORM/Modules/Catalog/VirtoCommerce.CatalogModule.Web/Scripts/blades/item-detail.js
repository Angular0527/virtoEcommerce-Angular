﻿angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.itemDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.settings', 'virtoCommerce.catalogModule.items', 'platformWebApp.dialogService', function ($scope, bladeNavigationService, settings, items, dialogService) {
    var blade = $scope.blade;
    blade.origItem = {};
    blade.item = {};
    blade.currentEntityId = blade.itemId;

    blade.refresh = function (parentRefresh) {
        blade.isLoading = true;

        return items.get({ id: blade.itemId }, function (data) {
            blade.itemId = data.id;
            blade.title = data.code;
            if (!data.productType) {
                data.productType = 'Physical';
            }
            blade.subtitle = data.productType + ' item details';
            $scope.isTitular = data.titularItemId == null;
            $scope.isTitularConfirmed = $scope.isTitular;

            blade.item = angular.copy(data);
            blade.origItem = data;
            blade.isLoading = false;
            if (parentRefresh) {
                blade.parentBlade.refresh();
            }
        },
        function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
    }

    //$scope.onTitularChange = function () {
    //    $scope.isTitular = !$scope.isTitular;
    //    if ($scope.isTitular) {
    //        blade.item.titularItemId = null;
    //    } else {
    //        blade.item.titularItemId = blade.origItem.titularItemId;
    //    }
    //};

    $scope.codeValidator = function (value) {
        var pattern = /[$+;=%{}[\]|\\\/@ ~#!^*&()?:'<>,]/;
        return !pattern.test(value);
    };

    function isDirty() {
        var retVal = !angular.equals(blade.item, blade.origItem);
        return retVal;
    };

    function saveChanges() {
        blade.isLoading = true;
        items.update({}, blade.item, function () {
            blade.refresh(true);
        },
        function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
    };

    blade.onClose = function (closeCallback) {
        if (isDirty()) {
            var dialog = {
                id: "confirmItemChange",
                title: "Save changes",
                message: "The item has been modified. Do you want to save changes?"
            };
            dialog.callback = function (needSave) {
                if (needSave) {
                    saveChanges();
                }
                closeCallback();
            };
            dialogService.showConfirmationDialog(dialog);
        }
        else {
            closeCallback();
        }
    };

    var formScope;
    $scope.setForm = function (form) {
        formScope = form;
    }

    blade.headIcon = blade.productType === 'Digital' ? 'fa fa-file-archive-o' : 'fa fa-truck';
    blade.toolbarCustomTemplates = ["Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-detail-toolbar.tpl.html"];

    blade.toolbarCommands = [
	 {
	     name: "Save", icon: 'fa fa-save',
	     executeMethod: function () {
	         saveChanges();
	     },
	     canExecuteMethod: function () {
	         return isDirty() && formScope && formScope.$valid;
	     },
	     permission: 'catalog:items:manage'
	 },
        {
            name: "Reset", icon: 'fa fa-undo',
            executeMethod: function () {
                angular.copy(blade.origItem, blade.item);
                $scope.isTitular = blade.item.titularItemId == null;
            },
            canExecuteMethod: function () {
                return isDirty();
            },
            permission: 'catalog:items:manage'
        }
    ];

    // datepicker
    $scope.datepickers = {}
    $scope.open = function ($event, which) {
        $event.preventDefault();
        $event.stopPropagation();
        $scope.datepickers[which] = true;
    };
    // $scope.dateOptions = { 'year-format': "'yyyy'" };

    $scope.openCoreSettingsManagement = function () {
        var newBlade = {
            id: 'moduleSettingsSection',
            moduleId: 'VirtoCommerce.Core',
            title: 'Platform settings',
            controller: 'platformWebApp.settingsDetailController',
            template: 'Scripts/app/settings/blades/settings-detail.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, blade);
    };

    $scope.taxTypes = settings.getValues({ id: 'VirtoCommerce.Core.General.TaxTypes' });
    blade.refresh(false);
}]);
