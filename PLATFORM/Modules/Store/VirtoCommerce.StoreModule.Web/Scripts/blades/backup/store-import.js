﻿angular.module('virtoCommerce.storeModule')
.controller('virtoCommerce.storeModule.storeImportController', ['$scope', 'FileUploader', 'platformWebApp.bladeNavigationService', 'virtoCommerce.storeModule.import', function ($scope, FileUploader, bladeNavigationService, importResource) {
	var blade = $scope.blade;
	blade.isLoading = false;
	blade.title = 'Importing store';
	$scope.blade.headIcon = 'fa-file-archive-o';

	$scope.$on("new-notification-event", function (event, notification) {
		if (blade.notification && notification.id == blade.notification.id) {
		    angular.copy(notification, blade.notification);

		    if (notification.finished) {
		        blade.isLoading = false;
		        if (notification.errorCount == 0) {
		            blade.parentBlade.refresh();
		        }
		    }
		}
	});

	$scope.startImport = function () {
	    blade.isLoading = true;
	    importResource.run(
            { FileUrl: $scope.blade.fileUrl, NewStoreId: $scope.blade.newStoreId, NewStoreName: $scope.blade.newStoreName },
            function (data) { blade.notification = data; },
            function (error) { bladeNavigationService.setError('Error ' + error.status, $scope.blade); });
	}

    //Upload
	if (!$scope.uploader) {
	    // Creates a uploader
	    var uploader = $scope.uploader = new FileUploader({
	        scope: $scope,
	        headers: { Accept: 'application/json' },
	        url: 'api/assets',
	        method: 'POST',
	        autoUpload: true,
	        removeAfterUpload: true
	    });

	    // //ADDING FILTERS
	    //uploader.filters.push({
	    //	name: 'zipFilter',
	    //	fn: function (i /*{File|FileLikeObject}*/, options) {
	    //		var type = '|' + i.type.slice(i.type.lastIndexOf('/') + 1) + '|';
	    //		return '|zip|'.indexOf(type) !== -1;
	    //	}
	    //});

	    uploader.onBeforeUploadItem = function (fileItem) {
	        blade.isLoading = true;
	    };

	    uploader.onSuccessItem = function (fileItem, asset, status, headers) {
	        $scope.blade.fileUrl = asset[0].relativeUrl;
	        blade.isLoading = false;
	    };
	    uploader.onErrorItem = function (item, response, status, headers) {
	        blade.isLoading = false;
	    }
	};

  
}]);
