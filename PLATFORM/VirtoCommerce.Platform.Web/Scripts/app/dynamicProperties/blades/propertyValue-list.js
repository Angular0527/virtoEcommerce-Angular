﻿angular.module('platformWebApp')
.controller('platformWebApp.propertyValueListController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.settings', 'platformWebApp.dynamicProperties.dictionaryItemsApi', function ($scope, bladeNavigationService, dialogService, settings, dictionaryItemsApi) {
    var blade = $scope.blade;
    blade.headIcon = 'fa-plus-square-o';
    blade.title = "Properties values";
    blade.subtitle = "Edit properties values";
    $scope.languages = [];

    blade.refresh = function () {
        settings.getValues({ id: 'VirtoCommerce.Core.General.Languages' }, function (data) {
            $scope.languages = data;
        });
        blade.origEntity = blade.currentEntity;
        blade.currentEntity = angular.copy(blade.currentEntity);
        blade.isLoading = false;
    };

    function isDirty() {
        return !angular.equals(blade.currentEntity.dynamicProperties, blade.origEntity.dynamicProperties);
    }

    $scope.cancelChanges = function () {
        angular.copy(blade.origEntity.dynamicProperties, blade.currentEntity.dynamicProperties);
        $scope.bladeClose();
    };

    $scope.saveChanges = function () {
        if (isDirty()) {
            angular.copy(blade.currentEntity.dynamicProperties, blade.origEntity.dynamicProperties);
        }
        $scope.bladeClose();
    };

    var formScope;
    $scope.setForm = function (form) {
        formScope = form;
    }

    $scope.editDictionary = function (property) {
        var newBlade = {
            id: "propertyDictionary",
            isApiSave: true,
            currentEntity: property,
            controller: 'platformWebApp.propertyDictionaryController',
            template: '$(Platform)/Scripts/app/dynamicProperties/blades/property-dictionary.tpl.html',
            onChangesConfirmedFn: function () {
                blade.currentEntity.dynamicProperties = angular.copy(blade.currentEntity.dynamicProperties);
            }
        };
        bladeNavigationService.showBlade(newBlade, blade);
    };

    blade.onClose = function (closeCallback) {
        if (isDirty()) {
            var dialog = {
                id: "confirmItemChange",
                title: "Save changes",
                message: "The properties have been modified. Do you want to confirm changes?",
                callback: function (needSave) {
                    if (needSave) {
                        $scope.saveChanges();
                    }
                    closeCallback();
                }
            };
            dialogService.showConfirmationDialog(dialog);
        }
        else {
            closeCallback();
        }
    };

    blade.toolbarCommands = [
        {
            name: "Reset", icon: 'fa fa-undo',
            executeMethod: function () {
                angular.copy(blade.origEntity.dynamicProperties, blade.currentEntity.dynamicProperties);
            },
            canExecuteMethod: function () {
                return isDirty();
            }
        },
		{
		    name: "Manage type properties", icon: 'fa fa-edit',
		    executeMethod: function () {
		        var newBlade = {
		            id: 'dynamicPropertyList',
		            objectType: blade.currentEntity.objectType,
		            controller: 'platformWebApp.dynamicPropertyListController',
		            template: '$(Platform)/Scripts/app/dynamicProperties/blades/dynamicProperty-list.tpl.html'
		        };
		        bladeNavigationService.showBlade(newBlade, blade);
		    },
		    canExecuteMethod: function () {
		        return angular.isDefined(blade.currentEntity.objectType);
		    }
		}
    ];

    $scope.getDictionaryValues = function (property, callback) {
        dictionaryItemsApi.query({ id: property.objectType, propertyId: property.id }, callback);
    }

    blade.refresh();
}]);
