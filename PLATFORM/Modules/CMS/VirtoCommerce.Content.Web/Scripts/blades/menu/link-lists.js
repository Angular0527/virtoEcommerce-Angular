﻿angular.module('virtoCommerce.contentModule')
.controller('virtoCommerce.contentModule.linkListsController', ['$scope', 'virtoCommerce.contentModule.menus', 'virtoCommerce.contentModule.stores', 'platformWebApp.bladeNavigationService', function ($scope, menus, menusStores, bladeNavigationService) {
	$scope.selectedNodeId = null;

	var blade = $scope.blade;

	blade.initialize = function () {
		blade.isLoading = true;
		menus.get({ storeId: blade.storeId }, function (data) {
			blade.currentEntities = data;
			blade.isLoading = false;
			blade.parentBlade.refresh(blade.storeId, 'menus');
		},
        function (error) { bladeNavigationService.setError('Error ' + error.status, $scope.blade); });
	}

	blade.openBlade = function (data) {
		$scope.selectedNodeId = data.id;
		closeChildrenBlades();

		var newBlade = {
			id: 'editMenuLinkListBlade',
			choosenStoreId: blade.storeId,
			choosenListId: data.id,
			newList: false,
			title: 'Edit ' + data.name + ' list',
			subtitle: 'Link list edit',
			controller: 'virtoCommerce.contentModule.menuLinkListController',
			template: 'Modules/$(VirtoCommerce.Content)/Scripts/blades/menu/menu-link-list.tpl.html'
		};
		bladeNavigationService.showBlade(newBlade, blade);
	}

	function openBladeNew() {
		$scope.selectedNodeId = null;
		closeChildrenBlades();

		var newBlade = {
			id: 'addMenuLinkListBlade',
			choosenStoreId: blade.storeId,
			newList: true,
			title: 'Add new list',
			subtitle: 'Create new list',
			controller: 'virtoCommerce.contentModule.menuLinkListController',
			template: 'Modules/$(VirtoCommerce.Content)/Scripts/blades/menu/menu-link-list.tpl.html'
		};
		bladeNavigationService.showBlade(newBlade, $scope.blade);
	}

	blade.onClose = function (closeCallback) {
		closeChildrenBlades();
		closeCallback();
	};

	blade.getFlag = function (lang) {
		switch (lang) {
			case 'ru-RU':
				return 'ru';

			case 'en-US':
				return 'us';

			case 'fr-FR':
				return 'fr';

			case 'zh-CN':
				return 'ch';

			case 'ru-RU':
				return 'ru';

			case 'ja-JP':
				return 'jp';

			case 'de-DE':
				return 'de';
		}
	}

	function closeChildrenBlades() {
		angular.forEach(blade.childrenBlades.slice(), function (child) {
			bladeNavigationService.closeBlade(child);
		});
	}

	$scope.blade.headIcon = 'fa-archive';

	$scope.blade.toolbarCommands = [
        {
        	name: "Add list", icon: 'fa fa-plus',
        	executeMethod: function () {
        		openBladeNew();
        	},
        	canExecuteMethod: function () {
        		return true;
        	},
        	permission: 'content:manage'
        }
	];

	blade.initialize();
}]);
