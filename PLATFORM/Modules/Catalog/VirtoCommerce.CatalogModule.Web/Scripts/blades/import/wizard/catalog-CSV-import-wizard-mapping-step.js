﻿angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.catalogCSVimportWizardMappingStepController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
	var blade = $scope.blade;
	

	//Need automatically tracking 
	angular.forEach($scope.blade.importConfiguration.propertyMaps, function (x) {
		$scope.$watch(function ($scope) { return x.csvColumnName }, function (newValue, oldValue) {
			if (oldValue != newValue) {
				if (newValue) {
					var index = blade.importConfiguration.propertyCsvColumns.indexOf(newValue);
					if (index != -1) {
						blade.importConfiguration.propertyCsvColumns.splice(index, 1);
					}
				}
				if (oldValue) {
					var index = blade.importConfiguration.propertyCsvColumns.indexOf(oldValue);
					if (index == -1) {
						blade.importConfiguration.propertyCsvColumns.push(oldValue);
					}
				}
			}
		});
	});

	$scope.clearPropertyCsvColumns = function () {
		blade.importConfiguration.propertyCsvColumns.length = 0;

	};

	$scope.removePropertyCsvColumn = function (column) {
		var index = blade.importConfiguration.propertyCsvColumns.indexOf(column);
		if (index != -1) {
			blade.importConfiguration.propertyCsvColumns.splice(index, 1);
		}
	};

    $scope.setForm = function (form) {
        $scope.formScope = form;
    }

    $scope.isValid = function () {
    	return $scope.formScope && $scope.formScope.$valid;
    };

    $scope.saveChanges = function () {
    	$scope.bladeClose();
    };

    blade.isLoading = false;
}]);


