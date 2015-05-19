﻿angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.categoriesItemsListController', [
        '$rootScope', '$scope', '$filter', 'virtoCommerce.catalogModule.categories', 'virtoCommerce.catalogModule.items', 'virtoCommerce.catalogModule.listEntries', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.authService', function ($rootScope, $scope, $filter, categories, items, listEntries, bladeNavigationService, dialogService, authService) {
        	//pagination settings
        	$scope.pageSettings = {};
        	$scope.pageSettings.totalItems = 0;
        	$scope.pageSettings.currentPage = 1;
        	$scope.pageSettings.numPages = 5;
        	$scope.pageSettings.itemsPerPageCount = 20;

        	$scope.filter = { searchKeyword: undefined };

        	$scope.selectedAll = false;
        	$scope.selectedItem = null;
        	var preventCategoryListingOnce; // prevent from unwanted additional actions after command was activated from context menu

        	var blade = $scope.blade;

        	blade.refresh = function () {
        		blade.isLoading = true;
        		listEntries.listitemssearch(
                    {
                    	catalog: blade.catalogId,
                    	category: blade.categoryId,
                    	q: $scope.filter.searchKeyword,
                    	// propertyValues: ,
                    	respGroup: 'withCategories, withProducts',
                    	start: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    	count: $scope.pageSettings.itemsPerPageCount
                    },
                    function (data, headers) {
                    	blade.isLoading = false;
                    	$scope.pageSettings.totalItems = angular.isDefined(data.totalCount) ? data.totalCount : 0;
                    	$scope.items = data.listEntries;
                    	$scope.selectedAll = false;

                    	if ($scope.selectedItem != null) {
                    		$scope.selectedItem = $scope.findItem($scope.selectedItem.id);
                    	}

                    	//Set navigation breadcrumbs
                    	setBreadcrumps();
                    	setCheckedEntries();
                    }, function (error) {
                    	blade.isLoading = false;
                    	bladeNavigationService.setError('Error ' + error.status, blade);
                    });
        	}

        	//Breadcrumps
        	function setBreadcrumps() {
        		//Clone array (angular.copy leave a same reference)
        		blade.breadcrumbs = blade.breadcrumbs.slice(0);

        		//catalog breadcrump by default
        		var breadCrumb = {
        			id: blade.catalogId,
        			name: blade.catalog.name,
        			blade: blade
        		};

        		//if category need change to category breadcrumb
        		if (angular.isDefined(blade.category)) {

        			breadCrumb.id = blade.categoryId;
        			breadCrumb.name = blade.category.name;
        		}

        		//prevent duplicate items
        		if (!_.some(blade.breadcrumbs, function (x) { return x.id == breadCrumb.id })) {
        			blade.breadcrumbs.push(breadCrumb);
        		}

        		breadCrumb.navigate = function (breadcrumb) {
        			bladeNavigationService.closeBlade(blade,
                        function () {
                        	blade.disableOpenAnimation = true;
                        	bladeNavigationService.showBlade(blade, blade.parentBlade);
                        	blade.refresh();
                        });
        		};
        	}

        	$scope.$watch('pageSettings.currentPage', function (newPage) {
        		blade.refresh();
        	});

        	$scope.edit = function (listItem) {
        		if (listItem.type === 'category') {
        			preventCategoryListingOnce = true;
        		}
        		edit(listItem);
        	};

        	function edit(listItem) {
        		closeChildrenBlades();

        		$scope.selectedItem = listItem;
        		if (listItem.type === 'category') {
        			blade.showCategoryBlade(listItem.id, null, listItem.name);
        		}
        		// else do nothing as item is opened on selecting it.
        	};

        	blade.showCategoryBlade = function (id, data, title) {
        		var newBlade = {
        			id: "listCategoryDetail",
        			currentEntityId: id,
        			currentEntity: data,
        			title: title,
        			subtitle: 'Category details',
        			controller: 'virtoCommerce.catalogModule.categoryDetailController',
        			template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/category-detail.tpl.html',
        		};
        		bladeNavigationService.showBlade(newBlade, blade);
        	};

        	blade.showItemBlade = function (id, title) {
        		var newBlade = {
        			id: "listItemDetail",
        			itemId: id,
        			title: title,
        			subtitle: 'Item details',
        			controller: 'virtoCommerce.catalogModule.itemDetailController',
        			template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-detail.tpl.html'
        		};
        		bladeNavigationService.showBlade(newBlade, blade);
        	};

        	blade.showNewItemWizard = function (inMemoryItem) {
        		var newBlade = {
        			id: "newProductWizard",
        			item: inMemoryItem,
        			title: "New product",
        			subtitle: 'Fill all product information',
        			controller: 'virtoCommerce.catalogModule.newProductWizardController',
        			template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/wizards/newProduct/new-product-wizard.tpl.html'
        		};
        		bladeNavigationService.showBlade(newBlade, blade);
        	};

        	blade.showNewVariationWizard = function (inMemoryItem) {
        		var newBlade = {
        			id: "newVariationWizard",
        			item: inMemoryItem,
        			title: "New variation",
        			subtitle: 'Fill variation information',
        			controller: 'virtoCommerce.catalogModule.newProductWizardController',
        			template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/wizards/newProduct/new-variation-wizard.tpl.html'
        		};
        		bladeNavigationService.showBlade(newBlade, blade);
        	};

        	$scope.delete = function () {
        		if (isItemsChecked()) {
        			deleteChecked();
        		} else {
        			var dialog = {
        				id: "notifyNoTargetCategory",
        				title: "Message",
        				message: "Nothing selected. Check some Categories or Items first."
        			};
        			dialogService.showNotificationDialog(dialog);
        		}

        		preventCategoryListingOnce = true;
        	};

        	function isItemsChecked() {
        		return $scope.items && _.any($scope.items, function (x) { return x.selected; });
        	}

        	function deleteChecked() {
        		var dialog = {
        			id: "confirmDeleteItem",
        			title: "Delete confirmation",
        			message: "Are you sure you want to delete selected Categories or Items?",
        			callback: function (remove) {
        				if (remove) {
        					closeChildrenBlades();

        					var selection = $filter('filter')($scope.items, { selected: true }, true);

        					var listEntryLinks = [];
        					var categoryIds = [];
        					var itemIds = [];
        					angular.forEach(selection, function (listItem) {
        						var deletingLink = false;

        						if (listItem.type === 'category') {
        							if (blade.catalog.virtual && _.some(listItem.links, function (x) { return x.categoryId === blade.categoryId; })) {
        								deletingLink = true;
        							} else {
        								categoryIds.push(listItem.id);
        							}
        						} else {
        							if (blade.catalog.virtual) {
        								deletingLink = true;
        							} else {
        								itemIds.push(listItem.id);
        							}
        						}

        						if (deletingLink)
        							listEntryLinks.push({
        								listEntryId: listItem.id,
        								listEntryType: listItem.type,
        								catalogId: blade.catalogId,
        								categoryId: blade.categoryId,
        							});
        					});

        					if (listEntryLinks.length > 0) {
        						listEntries.deletelinks(listEntryLinks, function (data, headers) {
        							blade.refresh();
        							if (blade.mode === 'mappingSource')
        								blade.parentBlade.refresh();
        						}, function (error) {
        							bladeNavigationService.setError('Error ' + error.status, blade);
        						});
        					}
        					if (categoryIds.length > 0) {
        						categories.remove({ ids: categoryIds }, function (data, headers) {
        							blade.refresh();
        						}, function (error) {
        							bladeNavigationService.setError('Error ' + error.status, blade);
        						});
        					}
        					if (itemIds.length > 0) {
        						items.remove({ ids: itemIds }, function (data, headers) {
        							blade.refresh();
        						}, function (error) {
        							bladeNavigationService.setError('Error ' + error.status, blade);
        						});
        					}
        				}
        			}
        		}
        		dialogService.showConfirmationDialog(dialog);
        	}

        	function mapChecked() {
        		//var dialog = {
        		//    id: "confirmDeleteItem",
        		//    title: "Map confirmation",
        		//    message: "Are you sure you want to map selected Categories or Items?",
        		//    callback: function (confirmed) {
        		//        if (confirmed) {
        		// blade.parentBlade.catalog.virtual....
        		closeChildrenBlades();

        		var selection = _.where($scope.items, { selected: true });
        		var listEntryLinks = [];
        		angular.forEach(selection, function (listItem) {
        			listEntryLinks.push({
        				listEntryId: listItem.id,
        				listEntryType: listItem.type,
        				catalogId: blade.parentBlade.catalogId,
        				categoryId: blade.parentBlade.categoryId,
        			});
        		});

        		listEntries.createlinks(listEntryLinks, function () {
        			blade.refresh();
        			blade.parentBlade.refresh();
        		});
        		//}
        		//    }
        		//}
        		//dialogService.showConfirmationDialog(dialog);
        	}

        	blade.setSelectedItem = function (listItem) {
        		$scope.selectedItem = listItem;
        	};

        	$scope.selectItem = function (e, listItem) {
        		blade.setSelectedItem(listItem);

        		if (listItem.type === 'category') {
        			if (preventCategoryListingOnce) {
        				preventCategoryListingOnce = false;
        			} else {
        				var newBlade = {
        					id: 'itemsList' + (blade.level + (e.ctrlKey ? 1 : 0)),
        					level: blade.level + (e.ctrlKey ? 1 : 0),
        					mode: blade.mode,
        					breadcrumbs: blade.breadcrumbs,
        					title: 'Categories & Items',
        					subtitle: 'Browsing "' + listItem.name + '"',
        					catalogId: blade.catalogId,
        					catalog: blade.catalog,
        					categoryId: listItem.id,
        					category: listItem,
        					disableOpenAnimation: true,
        					controller: 'virtoCommerce.catalogModule.categoriesItemsListController',
        					template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/categories-items-list.tpl.html'
        				};

        				if (e.ctrlKey) {
        					bladeNavigationService.showBlade(newBlade, blade);
        				} else {
        					bladeNavigationService.closeBlade(blade, function () {
        						bladeNavigationService.showBlade(newBlade, blade.parentBlade);
        					});
        				}
        			}
        		} else {
        			blade.showItemBlade(listItem.id, listItem.name);
        		}

        		blade.currentItemId = $scope.selectedItem.type === 'product' ? $scope.selectedItem.id : undefined;
        	};


        	$scope.findItem = function (id) {
        		var retVal;
        		angular.forEach($scope.items, function (item) {
        			if (item.id == id)
        				retVal = item;
        		});

        		return retVal;
        	}

        	$scope.hasLinks = function (listEntry) {
        		return blade.catalog.virtual && listEntry.links && (listEntry.type === 'category' ? listEntry.links.length > 0 : listEntry.links.length > 1);
        		// return listEntry.links && listEntry.links.length > 0;
        	}

        	blade.onClose = function (closeCallback) {
        		closeChildrenBlades();
        		closeCallback();
        	};

        	function closeChildrenBlades() {
        		angular.forEach(blade.childrenBlades.slice(), function (child) {
        			bladeNavigationService.closeBlade(child);
        		});
        	}

        	$scope.bladeToolbarCommands = [
                {
                	name: "Refresh",
                	icon: 'fa fa-refresh',
                	executeMethod: function () {
                		blade.refresh();
                	},
                	canExecuteMethod: function () {
                		return true;
                	}
                },
                {
                	name: "Manage",
                	icon: 'fa fa-edit',
                	executeMethod: function () {
                		// selected OR the first checked listItem
                		edit($scope.selectedItem || _.find($scope.items, function (x) { return x.selected; }));
                	},
                	canExecuteMethod: function () {
                		return $scope.selectedItem || isItemsChecked();
                	}
                },
				  {
				  	name: "Import", icon: 'fa fa-download',
				  	executeMethod: function () {
				  		$scope.import(selectedNode);
				  	},
				  	canExecuteMethod: function () {
				  		return true;
				  	}
				  },
				 {
				 	name: "Export", icon: 'fa fa-upload',
				 	executeMethod: function () {
				 		var newBlade = {
				 			id: 'exports',
				 			title: 'Data export',
				 			catalogId: blade.catalogId,
				 			subtitle: 'Choose data format & start export',
				 			controller: 'virtoCommerce.catalogModule.exporterListController',
				 			template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/export/exporter-list.tpl.html'
				 		};
				 		bladeNavigationService.showBlade(newBlade, $scope.blade);
				 	},
				 	canExecuteMethod: function () {	return true; }
				 },
                {
                	name: "Delete",
                	icon: 'fa fa-trash-o',
                	executeMethod: function () {
                		deleteChecked();
                	},
                	canExecuteMethod: function () {
                		return isItemsChecked();
                	}
                }
                //{
                //    name: "Advanced search", icon: 'fa fa-search',
                //    executeMethod: function () {
                //        var newBlade = {
                //            id: 'listItemChild',
                //            title: 'Advanced search',
                //            subtitle: 'Searching within...',
                //            controller: 'virtoCommerce.catalogModule.advancedSearchController',
                //            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/advanced-search.tpl.html'
                //        };
                //        bladeNavigationService.showBlade(newBlade, blade.parentBlade);
                //    },
                //    canExecuteMethod: function () {
                //        return true;
                //    }
                //}
        	];

        	if (angular.isDefined(blade.mode)) {
        		// mappingSource
        		if (blade.mode === 'mappingSource') {
        			var mapCommand = {
        				name: "Map", icon: 'fa fa-link',
        				executeMethod: function () {
        					mapChecked();
        				},
        				canExecuteMethod: function () {
        					return isItemsChecked();
        				}
        			}
        			$scope.bladeToolbarCommands.splice(1, 2, mapCommand);
        		} else if (blade.mode === 'newAssociation') {
        			$scope.bladeToolbarCommands.splice(1, 2);
        		}
        	} else if (authService.checkPermission('catalog:categories:manage')
                   || (authService.checkPermission('catalog:virtual_catalogs:manage') && blade.catalog.virtual)
                   || (authService.checkPermission('catalog:items:manage') && !blade.catalog.virtual)) {
        		$scope.bladeToolbarCommands.splice(0, 0, {
        			name: "Add", icon: 'fa fa-plus',
        			executeMethod: function () {
        				closeChildrenBlades();

        				var newBlade = {
        					id: 'listItemChild',
        					title: 'New category item',
        					subtitle: 'choose new item type',
        					controller: 'virtoCommerce.catalogModule.categoriesItemsAddController',
        					template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/categories-items-add.tpl.html'
        				};
        				bladeNavigationService.showBlade(newBlade, blade);
        			},
        			canExecuteMethod: function () {
        				return true;
        			}
        		});
        	}

        	blade.onAfterCatalogSelected = function (selectedNode) {
        		var newBlade = {
        			id: 'itemsList' + (blade.level + 1),
        			level: blade.level + 1,
        			mode: 'mappingSource',
        			breadcrumbs: [],
        			title: 'Choose Categories & Items for mapping',
        			subtitle: 'Creating a Link inside virtual catalog',
        			catalogId: selectedNode.id,
        			catalog: selectedNode,
        			disableOpenAnimation: true,
        			controller: 'virtoCommerce.catalogModule.categoriesItemsListController',
        			template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/categories-items-list.tpl.html'
        		};
        		bladeNavigationService.showBlade(newBlade, blade);
        	};

        	$scope.checkAll = function (selected) {
        		angular.forEach($scope.items, function (item) {
        			item.selected = selected;
        			$scope.checkOne(item);
        		});
        	};

        	$scope.checkOne = function (listItem) {
        		if (blade.mode === 'newAssociation') {
        			blade.parentBlade.updateSelection(listItem);
        		}
        	}

        	$scope.showCheck = function (listItem) {
        		var retVal = true;
        		if (blade.mode === 'newAssociation') {
        			retVal = listItem.type !== 'category';
        		}
        		return retVal;
        	}

        	function setCheckedEntries() {
        		if (blade.mode === 'newAssociation') {
        			_.each(blade.parentBlade.selection, function (selectedEntry) {
        				var foundItem = _.findWhere($scope.items, { id: selectedEntry.id });
        				if (foundItem) {
        					foundItem.selected = true;
        				}
        			});
        		}
        	}

        	//No need to call this because page 'pageSettings.currentPage' is watched!!! It would trigger subsequent duplicated req...
        	//blade.refresh();
        }]);
