﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using PlainElastic.Net;
using PlainElastic.Net.IndexSettings;
using PlainElastic.Net.Mappings;
using PlainElastic.Net.Queries;
using PlainElastic.Net.Serialization;
using VirtoCommerce.Domain.Search.Filters;
using VirtoCommerce.Domain.Search.Model;
using VirtoCommerce.Domain.Search.Services;

namespace VirtoCommerce.SearchModule.Data.Providers.ElasticSearch
{
    public class ElasticSearchProvider : ISearchProvider
    {
        public const string SearchAnalyzer = "search_analyzer";
        private const string _indexAnalyzer = "index_analyzer";

        private readonly ISearchConnection _connection;
        private readonly Dictionary<string, List<ESDocument>> _pendingDocuments = new Dictionary<string, List<ESDocument>>();
        private readonly Dictionary<string, string> _mappings = new Dictionary<string, string>();

        private bool _settingsUpdated;

        #region Private Properties
        ElasticClient<ESDocument> _client;
        private ElasticClient<ESDocument> Client
        {
            get
            {
                if (_client == null)
                {
                    var url = ElasticServerUrl;

                    if (url.StartsWith("https://"))
                    {
                        ThrowException("https connection is not supported", null);
                    }

                    if (url.StartsWith("http://")) // remove http prefix
                    {
                        url = url.Substring("http://".Length);
                    }

                    if (url.EndsWith("/"))
                    {
                        url = url.Remove(url.LastIndexOf("/", StringComparison.Ordinal));
                    }

                    var arr = url.Split(':');
                    var host = arr[0];
                    var port = "8080";
                    if (arr.Length > 1)
                        port = arr[1];

                    _client = new ElasticClient<ESDocument>(host, int.Parse(port));
                }

                return _client;
            }
        }
        #endregion

        #region Public Properties
        public string DefaultIndex { get; set; }

        private ISearchQueryBuilder _queryBuilder = new ElasticSearchQueryBuilder();

        public ISearchQueryBuilder QueryBuilder
        {
            get { return _queryBuilder; }
            set { _queryBuilder = value; }
        }

        private bool _autoCommit = true;

        /// <summary>
        /// Gets or sets a value indicating whether [auto commit].
        /// </summary>
        /// <value><c>true</c> if [auto commit]; otherwise, <c>false</c>.</value>
        public bool AutoCommit
        {
            get { return _autoCommit; }
            set { _autoCommit = value; }
        }

        private int _autoCommitCount = 100;

        /// <summary>
        /// Gets or sets the auto commit count.
        /// </summary>
        /// <value>The auto commit count.</value>
        public int AutoCommitCount
        {
            get { return _autoCommitCount; }
            set { _autoCommitCount = value; }
        }

        private string _elasticServerUrl = string.Empty;

        /// <summary>
        /// Gets or sets the solr server URL without Core secified.
        /// </summary>
        /// <example>localhost:9200</example>
        /// <value>The solr server URL.</value>
        public string ElasticServerUrl
        {
            get { return _elasticServerUrl; }
            set { _elasticServerUrl = value; }
        }
        #endregion

        public ElasticSearchProvider()
        {
            Init();
        }

        public ElasticSearchProvider(ISearchQueryBuilder queryBuilder, ISearchConnection connection)
        {
            _queryBuilder = queryBuilder;
            _connection = connection;
            Init();
        }

        private bool _isInitialized;
        private void Init()
        {
            if (!_isInitialized)
            {
                if (_connection != null && !string.IsNullOrEmpty(_connection.DataSource))
                {
                    _elasticServerUrl = _connection.DataSource;
                }
                else
                {
                    _elasticServerUrl = "localhost:9200";
                }

                _isInitialized = true;
            }
        }

        public virtual ISearchResults Search(string scope, ISearchCriteria criteria)
        {
            var command = new SearchCommand(scope, criteria.DocumentType);

            command.Size(criteria.RecordsToRetrieve);
            command.From(criteria.StartingRecord);

            // Add spell checking
            // TODO: options.SpellCheck = new SpellCheckingParameters { Collate = true };

            // Build query
            var builder = (QueryBuilder<ESDocument>)_queryBuilder.BuildQuery(criteria);

            SearchResult<ESDocument> resultDocs;

            // Add some error handling
            try
            {
                resultDocs = Client.Search(command, builder);
            }
            catch (Exception ex)
            {
                throw new ElasticSearchException("Search using Elastic Search server failed, check logs for more details.", ex);
            }

            // Parse documents returned
            var documents = new ResultDocumentSet { TotalCount = resultDocs.hits.total };
            var docList = new List<ResultDocument>();
            foreach (var indexDoc in resultDocs.Documents)
            {
                var document = new ResultDocument();
                foreach (var field in indexDoc.Keys)
                    document.Add(new DocumentField(field, indexDoc[field]));

                docList.Add(document);
            }

            documents.Documents = docList.ToArray();

            // Create search results object
            var results = new SearchResults(criteria, new[] { documents });

            // Now add facet results
            var groups = new List<FacetGroup>();

            if (resultDocs.facets != null)
            {
                foreach (var filter in criteria.Filters)
                {
                    var groupCount = 0;

                    var group = new FacetGroup(filter.Key);

                    if (filter is AttributeFilter)
                    {
                        group.FacetType = FacetTypes.Attribute;
                        var attributeFilter = filter as AttributeFilter;
                        var myFilter = attributeFilter;
                        var values = myFilter.Values;

                        var key = filter.Key.ToLower();
                        if (!resultDocs.facets.ContainsKey(key))
                            continue;

                        var facet = resultDocs.facets[key] as TermsFacetResult;
                        if (facet != null)
                        {
                            if (values != null)
                            {
                                foreach (var value in values)
                                {
                                    //facet.terms
                                    var termCount = from f in facet.terms where f.term.Equals(value.Id, StringComparison.OrdinalIgnoreCase) select f.count;

                                    var enumerable = termCount as int[] ?? termCount.ToArray();
                                    if (!enumerable.Any())
                                        continue;

                                    //var facet = from resultFacet
                                    var newFacet = new Facet(@group, value.Id, GetDescription(value, criteria.Locale), enumerable.SingleOrDefault());
                                    @group.Facets.Add(newFacet);
                                }
                            }
                            else
                            {
                                foreach (var term in facet.terms)
                                {
                                    var newFacet = new Facet(@group, term.term, term.term, term.count);
                                    @group.Facets.Add(newFacet);
                                }
                            }

                            groupCount++;
                        }
                    }
                    else if (filter is PriceRangeFilter)
                    {
                        group.FacetType = FacetTypes.PriceRange;
                        var rangeFilter = filter as PriceRangeFilter;
                        if (rangeFilter != null
                            && rangeFilter.Currency.Equals(criteria.Currency, StringComparison.OrdinalIgnoreCase))
                        {
                            var myFilter = rangeFilter;
                            var values = myFilter.Values;
                            if (values != null)
                            {
                                values = rangeFilter.Values;

                                foreach (var value in values)
                                {
                                    var key = string.Format("{0}-{1}", myFilter.Key, value.Id).ToLower();

                                    if (!resultDocs.facets.ContainsKey(key))
                                        continue;

                                    var facet = resultDocs.facets[key] as FilterFacetResult;

                                    if (facet != null && facet.count > 0)
                                    {
                                        if (facet.count == 0)
                                            continue;

                                        var myFacet = new Facet(
                                            @group, value.Id, GetDescription(value, criteria.Locale), facet.count);
                                        @group.Facets.Add(myFacet);

                                        groupCount++;
                                    }
                                }
                            }
                        }
                    }
                    else if (filter is RangeFilter)
                    {
                        group.FacetType = FacetTypes.Range;
                        var myFilter = filter as RangeFilter;
                        if (myFilter != null)
                        {
                            var values = myFilter.Values;
                            if (values != null)
                            {
                                foreach (var value in values)
                                {
                                    var facet = resultDocs.facets[filter.Key] as FilterFacetResult;

                                    if (facet == null || facet.count <= 0)
                                    {
                                        continue;
                                    }

                                    var myFacet = new Facet(
                                        @group, value.Id, GetDescription(value, criteria.Locale), facet.count);
                                    @group.Facets.Add(myFacet);

                                    groupCount++;
                                }
                            }
                        }
                    }
                    else if (filter is CategoryFilter)
                    {
                        group.FacetType = FacetTypes.Category;
                        var myFilter = filter as CategoryFilter;
                        if (myFilter != null)
                        {
                            var values = myFilter.Values;
                            if (values != null)
                            {
                                foreach (var value in values)
                                {
                                    var key = string.Format("{0}-{1}", myFilter.Key.ToLower(), value.Id.ToLower()).ToLower();
                                    var facet = resultDocs.facets[key] as FilterFacetResult;

                                    if (facet == null || facet.count <= 0)
                                    {
                                        continue;
                                    }

                                    var myFacet = new Facet(
                                        @group, value.Id, GetDescription(value, criteria.Locale), facet.count);
                                    @group.Facets.Add(myFacet);

                                    groupCount++;
                                }
                            }
                        }
                    }

                    // Add only if items exist under
                    if (groupCount > 0)
                    {
                        groups.Add(group);
                    }
                }
            }

            results.FacetGroups = groups.ToArray();
            return results;
        }

        public virtual void Index(string scope, string documentType, IDocument document)
        {
            var core = GetCoreName(scope, documentType);
            if (!_pendingDocuments.ContainsKey(core))
            {
                _pendingDocuments.Add(core, new List<ESDocument>());
            }

            string mapping = null;
            if (!_mappings.ContainsKey(core))
            {
                // Get mapping info
                if (Client.IndexExists(new IndexExistsCommand(scope)))
                {
                    mapping = GetMappingFromServer(scope, documentType, core);
                }
            }
            else
            {
                mapping = _mappings[core];
            }

            var submitMapping = false;

            var properties = new Properties<ESDocument>();
            var localDocument = new ESDocument();

            for (var index = 0; index < document.FieldCount; index++)
            {
                var field = document[index];

                var key = field.Name.ToLower();

                if (localDocument.ContainsKey(key))
                {
                    var newValues = new List<object>();

                    var currentValue = localDocument[key];
                    var currentValues = currentValue as object[];

                    if (currentValues != null)
                    {
                        newValues.AddRange(currentValues);
                    }
                    else
                    {
                        newValues.Add(currentValue);
                    }

                    newValues.AddRange(field.Values);
                    localDocument[key] = newValues.ToArray();
                }
                else
                {
                    if (string.IsNullOrEmpty(mapping) || !mapping.Contains(string.Format("\"{0}\"", field.Name)))
                    {
                        var type = field.Value != null ? field.Value.GetType() : typeof(object);

                        if (type == typeof(decimal))
                        {
                            type = typeof(double);
                        }

                        var elasticType = ElasticCoreTypeMapper.GetElasticType(type);

                        var propertyMap = new CustomPropertyMap<ESDocument>(field.Name, elasticType)
                        .Store(field.ContainsAttribute(IndexStore.Yes))
                        .When(field.ContainsAttribute(IndexType.NotAnalyzed), p => p.Index(IndexState.not_analyzed))
                        .When(field.Name.StartsWith("__content", StringComparison.OrdinalIgnoreCase), p => p.Analyzer(_indexAnalyzer))
                        .When(Regex.Match(field.Name, "__content_en.*").Success, x => x.Analyzer("english"))
                        .When(Regex.Match(field.Name, "__content_de.*").Success, x => x.Analyzer("german"))
                        .When(Regex.Match(field.Name, "__content_ru.*").Success, x => x.Analyzer("russian"))
                        .When(field.ContainsAttribute(IndexType.No), p => p.Index(IndexState.no));

                        properties.CustomProperty(field.Name, p => propertyMap);

                        submitMapping = true;
                    }

                    if (field.Values.Length > 1)
                    {
                        localDocument.Add(key, field.Values);
                    }
                    else
                    {
                        localDocument.Add(key, field.Value);
                    }
                }
            }

            // submit mapping
            if (submitMapping)
            {
                //Use ngrams analyzer for search in the middle of word
                //http://www.elasticsearch.org/guide/en/elasticsearch/guide/current/ngrams-compound-words.html
                var settings = new IndexSettingsBuilder()
                    .Analysis(als => als
                        .Filter(f => f.NGram("trigrams_filter", ng => ng
                                .MinGram(3)
                                .MaxGram(20)))
                        .Analyzer(a => a
                            .Custom(_indexAnalyzer, custom => custom
                                .Tokenizer(DefaultTokenizers.standard)
                                .Filter(DefaultTokenFilters.lowercase.ToString(), "trigrams_filter"))
                            .Custom(SearchAnalyzer, custom => custom
                                .Tokenizer(DefaultTokenizers.standard)
                                .Filter(DefaultTokenFilters.lowercase.ToString()))))
                    .Build();

                if (!Client.IndexExists(new IndexExistsCommand(scope)))
                {
                    var response = Client.CreateIndex(new IndexCommand(scope), settings);
                    _settingsUpdated = true;
                    if (response.error != null)
                        throw new IndexBuildException(response.error);
                }
                else if (!_settingsUpdated)
                {
                    // We can't update settings on active index.
                    // So we need to close it, then update settings and then open index back.
                    Client.Close(new CloseCommand(scope));
                    Client.UpdateSettings(new UpdateSettingsCommand(scope), settings);
                    Client.Open(new OpenCommand(scope));
                    _settingsUpdated = true;
                }

                var mapBuilder = new MapBuilder<ESDocument>();
                var mappingNew = mapBuilder.RootObject(documentType, d => d.Properties(p => properties)).Build();

                var result = Client.PutMapping(new PutMappingCommand(scope, documentType), mappingNew);
                if (!result.acknowledged && result.error != null)
                    throw new IndexBuildException(result.error);

                GetMappingFromServer(scope, documentType, core);
            }

            _pendingDocuments[core].Add(localDocument);

            // Auto commit changes when limit is reached
            if (AutoCommit && _pendingDocuments[core].Count > AutoCommitCount)
            {
                Commit(scope);
            }
        }

        private string GetMappingFromServer(string scope, string documentType, string core)
        {
            string mapping = null;

            try
            {
                mapping = Client.GetMapping(new GetMappingCommand(scope, documentType));
            }
            catch (OperationException ex)
            {
                if (ex.HttpStatusCode != 404 || !ex.Message.Contains("TypeMissingException"))
                {
                    throw;
                }
            }

            if (mapping != null)
                _mappings[core] = mapping;

            return mapping;
        }

        public virtual int Remove(string scope, string documentType, string key, string value)
        {
            var result = Client.Delete(Commands.Delete(scope, documentType, value));

            if (result.error != null && !result.acknowledged)
                throw new IndexBuildException(result.error);

            return 1;
        }

        public virtual void RemoveAll(string scope, string documentType)
        {
            try
            {
                var result = Client.Delete(Commands.Delete(scope, documentType));

                if (result.error != null && !result.acknowledged)
                    throw new IndexBuildException(result.error);

                var core = GetCoreName(scope, documentType);
                _mappings.Remove(core);
            }
            catch (OperationException ex)
            {
                if (ex.HttpStatusCode == 404 && (ex.Message.Contains("TypeMissingException") || ex.Message.Contains("IndexMissingException")))
                {

                }
                else
                {
                    ThrowException("Failed to remove indexes", ex);
                }
            }
        }

        public virtual void Close(string scope, string documentType)
        {
        }

        public virtual void Commit(string scope)
        {
            var coreList = _pendingDocuments.Keys.ToList();

            foreach (var core in coreList)
            {
                var documents = _pendingDocuments[core];
                if (documents == null || documents.Count == 0)
                    continue;

                var coreArray = core.Split('.');
                var indexName = coreArray[0];
                var indexType = coreArray[1];

                var result = Client.IndexBulk(Commands.Bulk(indexName, indexType), documents);

                if (result == null)
                {
                    throw new IndexBuildException("no results");
                }

                foreach (var op in result.items)
                {
                    if (op.Result.error != null)
                    {
                        throw new IndexBuildException(op.Result.error);
                    }
                }

                // Remove documents
                _pendingDocuments[core].Clear();
            }
        }

        private string GetCoreName(string scope, string documentType)
        {
            return string.Format("{0}.{1}", scope.ToLower(), documentType);
        }

        private string GetDescription(ISearchFilterValue value, string locale)
        {
            if (value is AttributeFilterValue)
            {
                var v = value as AttributeFilterValue;
                return v.Value;
            }
            if (value is RangeFilterValue)
            {
                var v = value as RangeFilterValue;
                var returnVal = v.Displays.Where(d => d.Language.Equals(locale, StringComparison.OrdinalIgnoreCase)).Select(d => d.Value);

                if (!returnVal.Any())
                {
                    try
                    {
                        var localeShort = new CultureInfo(locale).TwoLetterISOLanguageName;
                        returnVal = v.Displays.Where(d => d.Language.Equals(localeShort, StringComparison.OrdinalIgnoreCase)).Select(d => d.Value);
                    }
                    catch
                    {
                    }
                }

                if (returnVal.Any())
                {
                    return returnVal.SingleOrDefault();
                }
                else
                {
                    return v.Id;
                }
            }
            if (value is CategoryFilterValue)
            {
                var v = value as CategoryFilterValue;
                return v.Name;
            }

            return string.Empty;
        }

        private void ThrowException(string message, Exception innerException)
        {
            throw new ElasticSearchException(string.Format("{0}. URL:{1}", message, ElasticServerUrl), innerException);
        }
    }
}
