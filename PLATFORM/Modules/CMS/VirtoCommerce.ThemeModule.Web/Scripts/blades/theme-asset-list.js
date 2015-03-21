﻿angular.module('virtoCommerce.content.themeModule.blades.themeAssetList', [
    'virtoCommerce.content.themeModule.resources.themes',
	'virtoCommerce.content.themeModule.resources.themesStores',
	'virtoCommerce.content.themeModule.blades.editAsset',
	'virtoCommerce.content.themeModule.blades.editImageAsset'
])
.controller('themeAssetListController', ['$scope', 'themes', 'themesStores', 'bladeNavigationService', function ($scope, themes, themesStores, bladeNavigationService) {
	var blade = $scope.blade;

	$scope.selectedFolderId = undefined;
	$scope.selectedAssetId = undefined;

	blade.refresh = function () {
		blade.isLoading = true;
		themes.getAssets({ storeId: blade.choosenStoreId, themeId: blade.choosenThemeId }, function (data) {
			blade.currentEntities = data;
			themesStores.get({ id: blade.choosenStoreId }, function (data) {
				blade.store = data;
				blade.isLoading = false;
			});
		});
	}

	blade.folderClick = function (data) {
		//closeChildrenBlades();

		if (blade.checkFolder(data)) {
			$scope.selectedFolderId = undefined;
		}
		else {
			$scope.selectedFolderId = data.folderName;
		}
	}

	blade.checkFolder = function (data) {
		return $scope.selectedFolderId === data.folderName;
	}

	blade.checkAsset = function (data) {
		return $scope.selectedAssetId === data.id;
	}

	blade.assetClass = function (asset) {
		switch(asset.contentType)
		{
			case 'text/html':
			case 'application/json':
			case 'application/javascript':
				return 'fa-file-text';

			case 'image/png':
			case 'image/jpeg':
			case 'image/bmp':
			case 'image/gif':
				return 'fa-image';

			default:
				return 'fa-file';
		}
	}

	blade.setThemeAsActive = function () {
		blade.isLoading = true;
		if (_.where(blade.store.settings, { name: "DefaultThemeName" }).length > 0) {
			angular.forEach(blade.store.settings, function (value, key) {
				if (value.name === "DefaultThemeName") {
					value.value = blade.choosenThemeId;
				}
			});
		}
		else {
			blade.store.settings.push({ name: "DefaultThemeName", value: blade.choosenThemeId, valueType: "ShortText" })
		}

		themesStores.update({ storeId: blade.choosenStoreId }, blade.store, function (data) {
			blade.isLoading = false;
		});
	}

	blade.openBlade = function (asset) {
		$scope.selectedAssetId = asset.id;
		closeChildrenBlades();

		if (asset.contentType === 'text/html' || asset.contentType === 'application/json' || asset.contentType === 'application/javascript') {
			var newBlade = {
				id: 'editAssetBlade',
				choosenStoreId: blade.choosenStoreId,
				choosenThemeId: blade.choosenThemeId,
				choosenAssetId: asset.id,
				newAsset: false,
				title: asset.id,
				subtitle: 'Edit text asset',
				controller: 'editAssetController',
				template: 'Modules/$(VirtoCommerce.Theme)/Scripts/blades/edit-asset.tpl.html'
			};
			bladeNavigationService.showBlade(newBlade, blade);
		}
		else {
			var newBlade = {
				id: 'editImageAssetBlade',
				choosenStoreId: blade.choosenStoreId,
				choosenThemeId: blade.choosenThemeId,
				choosenAssetId: asset.id,
				newAsset: false,
				title: asset.id,
				subtitle: 'Edit image asset',
				controller: 'editImageAssetController',
				template: 'Modules/$(VirtoCommerce.Theme)/Scripts/blades/edit-image-asset.tpl.html'
			};
			bladeNavigationService.showBlade(newBlade, blade);
		}
	}

	function openBladeNew() {
		closeChildrenBlades();

		var contentType = blade.getContentType();

		if (contentType === 'text/html') {
			var newBlade = {
				id: 'addAsset',
				choosenStoreId: blade.choosenStoreId,
				choosenThemeId: blade.choosenThemeId,
				choosenFolder: $scope.selectedFolderId,
				newAsset: true,
				currentEntity: { id: undefined, content: undefined, contentType: undefined, assetUrl: undefined, name: undefined },
				title: 'New Asset',
				subtitle: 'Create new text asset',
				controller: 'editAssetController',
				template: 'Modules/$(VirtoCommerce.Theme)/Scripts/blades/edit-asset.tpl.html'
			};
			bladeNavigationService.showBlade(newBlade, blade);
		}
		else {
			var newBlade = {
				id: 'addImageAsset',
				choosenStoreId: blade.choosenStoreId,
				choosenThemeId: blade.choosenThemeId,
				choosenFolder: $scope.selectedFolderId,
				newAsset: true,
				currentEntity: { id: undefined, content: undefined, contentType: undefined, assetUrl: undefined, name: undefined },
				title: 'New Asset',
				subtitle: 'Create new image asset',
				controller: 'editImageAssetController',
				template: 'Modules/$(VirtoCommerce.Theme)/Scripts/blades/edit-image-asset.tpl.html'
			};
			bladeNavigationService.showBlade(newBlade, blade);
		}
	}

	blade.onClose = function (closeCallback) {
		closeChildrenBlades();
		closeCallback();
	};

	function closeChildrenBlades() {
		angular.forEach(blade.childrenBlades.slice(), function (child) {
			bladeNavigationService.closeBlade(child);
		});
		$scope.selectedNodeId = null;
	}

	blade.getContentType = function () {
		switch ($scope.selectedFolderId)
		{
			case 'layout':
			case 'templates':
			case 'snippets':
			case 'config':
			case 'locales':
				return 'text/html';

			default:
				return null;
		}
	}

    $scope.bladeHeadIco = 'fa fa-archive';

    $scope.bladeToolbarCommands = [
		{
			name: "Set Active", icon: 'fa fa-pencil-square-o',
			executeMethod: function () {
				blade.setThemeAsActive();
			},
			canExecuteMethod: function () {
				return !angular.isUndefined(blade.choosenThemeId);
			}
		},
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
        	name: "Add asset", icon: 'fa fa-plus',
        	executeMethod: function () {
        		openBladeNew();
        	},
        	canExecuteMethod: function () {
        		return !angular.isUndefined($scope.selectedFolderId);
        	}
        }
	];

	blade.refresh();
}]);
