﻿angular.module('platformWebApp')
.directive('vaDynamicProperty', ['$compile', '$templateCache', '$http', 'platformWebApp.dynamicProperties.dictionaryItemsApi', function ($compile, $templateCache, $http, dictionaryItemsApi) {
    return {
        restrict: 'E',
        require: 'ngModel',
        replace: true,
        transclude: true,
        templateUrl: 'Scripts/app/dynamicProperties/directives/dynamicProperty.tpl.html',
        scope: { getPropValues: "&" },
        link: function (scope, element, attr, ngModelController, linker) {
            scope.currentEntity = ngModelController.$modelValue;
            var property = scope.currentEntity.property;

            scope.context = {};
            scope.context.currentPropValues = [];
            scope.context.allDictionaryValues = [];
            scope.context.langValuesMap = {};


            scope.$watch('context.langValuesMap', function (newValue, oldValue) {
                if (newValue != oldValue) {
                    scope.context.currentPropValues = [];
                    angular.forEach(scope.context.langValuesMap, function (langGroup, locale) {

                        angular.forEach(langGroup.currentPropValues, function (propValue) {
                            propValue.locale = locale;
                            scope.context.currentPropValues.push(propValue);
                        });
                    });
                }
            }, true);

            scope.$watch('context.currentPropValues', function (newValue) {
                //reflect only real changes
                if (newValue.length != scope.currentEntity.values.length || difference(newValue, scope.currentEntity.values).length > 0) {
                    //Prevent reflect changing when use null value for empty initial values
                    if (!(scope.currentEntity.values.length == 0 && newValue[0].value == null)) {
                        if (property.isDictionary && property.isMultilingual && !property.isArray) {
                            scope.currentEntity.values = _.where(scope.context.allDictionaryValues, { alias: newValue[0].alias });
                        } else {
                            scope.currentEntity.values = newValue;
                        }
                        ngModelController.$setViewValue(scope.currentEntity);
                    }
                }
            }, true);


            ngModelController.$render = function () {
                scope.currentEntity = ngModelController.$modelValue;
                property = scope.currentEntity.property;

                scope.context.currentPropValues = angular.copy(scope.currentEntity.values);
                if (needAddEmptyValue(scope.context.currentPropValues)) {
                    scope.context.currentPropValues.push({ value: null });
                }

                if (property.isDictionary) {
                    loadDictionaryValues();
                }
                if (property.isMultilingual) {
                    initLanguagesValuesMap();
                }

                chageValueTemplate();
            };

            var difference = function (one, two) {
                var containsEquals = function (obj, target) {
                    if (obj == null) return false;
                    return _.any(obj, function (value) {
                        return value.value == target.value;
                    });
                };
                return _.filter(one, function (value) { return !containsEquals(two, value); });
            }

            function needAddEmptyValue(values) {
                return !property.isArray && !property.isDictionary && values.length == 0;
            }

            function initLanguagesValuesMap() {
                //Group values by language 
                angular.forEach(property.catalog.languages, function (language) {
                    //Currently select values
                    var currentPropValues = _.where(scope.context.currentPropValues, { locale: language.locale });
                    //need add empty value for single  value type
                    if (needAddEmptyValue(currentPropValues)) {
                        currentPropValues.push({ value: null, locale: language.locale });
                    }
                    //All possible dict values
                    var allValues = _.where(scope.context.allDictionaryValues, { locale: language.locale });

                    var langValuesGroup = {
                        allValues: allValues,
                        currentPropValues: currentPropValues
                    };
                    scope.context.langValuesMap[language.locale] = langValuesGroup;
                });
            }

            function loadDictionaryValues() {
                var selectedValues = scope.currentEntity.values;
                dictionaryItemsApi.query({ id: property.objectType, propertyId: property.id }, function (result) {
                    //blade.origEntity = data;
                    //blade.currentEntities = angular.copy(data);

                    scope.context.allDictionaryValues = [];
                    scope.context.currentPropValues = [];

                    angular.forEach(result, function (dictValue) {
                        //Need select already selected values.
                        //Dictionary values are of same type like standard values
                        dictValue.selected = angular.isDefined(_.find(selectedValues, function (value) { return value.valueId == dictValue.valueId; }));
                        scope.context.allDictionaryValues.push(dictValue);
                        if (dictValue.selected) {
                            //add selected value
                            scope.context.currentPropValues.push(dictValue);
                        }
                    });

                    if (property.isMultilingual) {
                        initLanguagesValuesMap();
                    }
                }, function (error) { });
            };

            function getTemplateName() {
                var result = 'd' + property.valueType;

                if (property.isDictionary) {
                    result += '-dictionary';
                }
                if (property.isArray) {
                    result += '-multivalue';
                }
                if (property.isMultilingual) {
                    result += '-multilang';
                }
                result += '.html';
                return result;
            };

            function chageValueTemplate() {
                var templateName = getTemplateName();

                //load input template and display
                $http.get(templateName, { cache: $templateCache }).success(function (tplContent) {
                    //Need to add ngForm to isolate form validation into sub form
                    //var innerContainer = "<div id='innerContainer' />";

                    //We must destroy scope of elements we are removing from DOM to avoid angular firing events
                    var el = element.find('#valuePlaceHolder #innerContainer');
                    if (el.length > 0) {
                        el.scope().$destroy();
                    }
                    var container = element.find('#valuePlaceHolder');
                    var result = container.html(tplContent.trim());

                    //Create new scope, otherwise we would destroy our directive scope
                    var newScope = scope.$new();
                    $compile(result)(newScope);
                });
            };

            /* Datepicker */
            scope.datepickers = {
                DateTime: false
            }

            scope.showWeeks = true;
            scope.toggleWeeks = function () {
                scope.showWeeks = !scope.showWeeks;
            };

            scope.clear = function () {
                scope.currentEntity.values = [];
            };
            scope.today = new Date();

            scope.open = function ($event, which) {
                $event.preventDefault();
                $event.stopPropagation();

                scope.datepickers[which] = true;
            };

            scope.dateOptions = {
                formatYear: 'yyyy',
            };

            scope.formats = ['shortDate', 'dd-MMMM-yyyy', 'yyyy/MM/dd'];
            scope.format = scope.formats[0];


            linker(function (clone) {
                element.append(clone);
            });
        }
    }
}]);