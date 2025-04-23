using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.ExternalSearch;
using CluedIn.Core.Providers;
using CluedIn.Core.Connectors;
using CluedIn.ExternalSearch.Providers.VatLayer.Models;
using CluedIn.ExternalSearch.Providers.VatLayer.Utility;
using CluedIn.ExternalSearch.Providers.VatLayer.Vocabularies;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using EntityType = CluedIn.Core.Data.EntityType;

namespace CluedIn.ExternalSearch.Providers.VatLayer
{
    using System.Text.RegularExpressions;
    using CluedIn.ExternalSearch.Provider;

    /// <summary>The VatLayer graph external search provider.</summary>
    /// <seealso cref="ExternalSearchProviderBase" />
    public class VatLayerExternalSearchProvider : ExternalSearchProviderBase, IExtendedEnricherMetadata, IConfigurableExternalSearchProvider, IExternalSearchProviderWithVerifyConnection
    {
        /**********************************************************************************************************
         * FIELDS
         **********************************************************************************************************/

        private static readonly EntityType[] DefaultAcceptedEntityTypes = { EntityType.Organization };

        /**********************************************************************************************************
        * CONSTRUCTORS
        **********************************************************************************************************/

        public VatLayerExternalSearchProvider()
           : base(Constants.ProviderId, DefaultAcceptedEntityTypes)
        {
            var nameBasedTokenProvider = new NameBasedTokenProvider("VatLayer");

            if (nameBasedTokenProvider.ApiToken != null)
            {
                TokenProvider = new RoundRobinTokenProvider(
                    nameBasedTokenProvider.ApiToken.Split(',', ';'));
            }
        }

        private VatLayerExternalSearchProvider(IEnumerable<string> tokens)
            : this(true)
        {
            TokenProvider = new RoundRobinTokenProvider(tokens);
        }

        private VatLayerExternalSearchProvider(IExternalSearchTokenProvider tokenProvider)
            : this(true)
        {
            TokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        private VatLayerExternalSearchProvider(bool tokenProviderIsRequired)
            : this()
        {
            TokenProviderIsRequired = tokenProviderIsRequired;
        }

        /**********************************************************************************************************
         * METHODS
         **********************************************************************************************************/

        public IEnumerable<EntityType> Accepts(IDictionary<string, object> config, IProvider provider) => this.Accepts(config);

        private IEnumerable<EntityType> Accepts(IDictionary<string, object> config)
            => Accepts(new VatLayerExternalSearchJobData(config));

        private IEnumerable<EntityType> Accepts(VatLayerExternalSearchJobData config)
        {
            if (!string.IsNullOrWhiteSpace(config.AcceptedEntityType))
            {
                // If configured, only accept the configured entity types
                return new EntityType[] { config.AcceptedEntityType };
            }

            // Fallback to default accepted entity types
            return DefaultAcceptedEntityTypes;
        }

        private bool Accepts(VatLayerExternalSearchJobData config, EntityType entityTypeToEvaluate)
        {
            var configurableAcceptedEntityTypes = this.Accepts(config).ToArray();

            return configurableAcceptedEntityTypes.Any(entityTypeToEvaluate.Is);
        }

        public IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
            => InternalBuildQueries(context, request, new VatLayerExternalSearchJobData(config));

        private IEnumerable<IExternalSearchQuery> InternalBuildQueries(ExecutionContext context, IExternalSearchRequest request, VatLayerExternalSearchJobData config)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (context.Log.BeginScope($"{GetType().Name} BuildQueries: request {request}"))
            {
                if (string.IsNullOrEmpty(config.ApiToken))
                {
                    context.Log.LogError("ApiToken for VatLayer must be provided.");
                    yield break;
                }

                if (!this.Accepts(config, request.EntityMetaData.EntityType))
                {
                    context.Log.LogTrace("Unacceptable business domain from '{EntityName}', entity code '{EntityCode}'", request.EntityMetaData.DisplayName, request.EntityMetaData.EntityType.Code);
                    yield break;
                }

                context.Log.LogTrace("Starting to build queries for {EntityName}", request.EntityMetaData.DisplayName);

                var existingResults = request.GetQueryResults<VatLayerResponse>(this).ToList();

                bool vatFilter(string value) => existingResults.Any(r => string.Equals(r.Data.VatNumber, value, StringComparison.InvariantCultureIgnoreCase));

                var entityType = request.EntityMetaData.EntityType;

                var vatNumber = new HashSet<string>();
                if (!string.IsNullOrWhiteSpace(config.AcceptedVocabKey))
                {
                    vatNumber = request.QueryParameters.GetValue<string, HashSet<string>>(config.AcceptedVocabKey, new HashSet<string>());
                }
                else
                {
                    vatNumber = request.QueryParameters.GetValue(Core.Data.Vocabularies.Vocabularies.CluedInOrganization.VatNumber, new HashSet<string>());
                }

                if (!vatNumber.Any())
                {
                    context.Log.LogTrace("No query parameter for '{VatNumber}' in request, skipping build queries", Core.Data.Vocabularies.Vocabularies.CluedInOrganization.VatNumber);
                }
                else
                {
                    var filteredValues = vatNumber.Where(v => !vatFilter(v)).ToArray();

                    if (!filteredValues.Any())
                    {
                        context.Log.LogWarning("Filter removed all VAT numbers, skipping processing. Original '{Original}'", string.Join(",", vatNumber));
                    }
                    else
                    {
                        foreach (var value in filteredValues)
                        {
                            request.CustomQueryInput = vatNumber.ElementAt(0);
                            var cleaner = new VatNumberCleaner();
                            var sanitizedValue = cleaner.CheckVATNumber(value);

                            if (value != sanitizedValue)
                            {
                                context.Log.LogTrace("Sanitized VAT number. Original '{OriginalValue}', Updated '{SanitizedValue}'", value, sanitizedValue);
                            }

                            context.Log.LogInformation("External search query produced, ExternalSearchQueryParameter: '{Identifier}' EntityType: '{EntityCode}' Value: '{SanitizedValue}'", ExternalSearchQueryParameter.Identifier, entityType.Code, sanitizedValue);

                            yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Identifier, sanitizedValue);
                        }
                    }

                    context.Log.LogTrace("Finished building queries for '{Name}'", request.EntityMetaData.Name);
                }
            }
        }

        public IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query, IDictionary<string, object> config, IProvider provider)
        {
            var jobData = new VatLayerExternalSearchJobData(config);
            return InternalExecuteSearch(context, query, jobData.ApiToken);
        }

        private IEnumerable<IExternalSearchQueryResult> InternalExecuteSearch(ExecutionContext context, IExternalSearchQuery query, string apiToken)
        {
            if (string.IsNullOrEmpty(apiToken))
            {
                throw new InvalidOperationException("ApiToken for VatLayer must be provided.");
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (context.Log.BeginScope("{0} {1}: query {2}", GetType().Name, "ExecuteSearch", query))
            {
                context.Log.LogTrace("Starting external search for Id: '{Id}' QueryKey: '{QueryKey}'", query.Id, query.QueryKey);

                var vat = query.QueryParameters[ExternalSearchQueryParameter.Identifier].FirstOrDefault();

                if (string.IsNullOrEmpty(vat))
                {
                    context.Log.LogTrace("No parameter for '{Identifier}' in query, skipping execute search", ExternalSearchQueryParameter.Identifier);
                }
                else
                {
                    // TODO missing try { } catch { } block ...

                    vat = WebUtility.UrlEncode(vat);
                    var client = new RestClient("http://www.apilayer.net/api");
                    var request = new RestRequest($"validate?access_key={apiToken}&vat_number={vat}&format=1",
                        Method.GET);
                    var response = client.ExecuteAsync<VatLayerResponse>(request).Result;

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        if (response.Data != null && response.Data.Valid)
                        {
                            var diagnostic =
                                $"External search for Id: '{query.Id}' QueryKey: '{query.QueryKey}' produced results, CompanyName: '{response.Data.CompanyName}'  VatNumber: '{response.Data.VatNumber}'";

                            context.Log.LogInformation(diagnostic);

                            yield return new ExternalSearchQueryResult<VatLayerResponse>(query, response.Data);
                        }
                        else
                        {
                            var diagnostic =
                                $"Failed external search for Id: '{query.Id}' QueryKey: '{query.QueryKey}' - StatusCode: '{response.StatusCode}' Content: '{response.Content}'";

                            context.Log.LogError(diagnostic);

                            var content = JsonConvert.DeserializeObject<dynamic>(response.Content);
                            if (content.error != null)
                            {
                                throw new InvalidOperationException(
                                    $"{content.error.info} - Type: {content.error.type} Code: {content.error.code}");
                            }

                            // TODO else do what with content ? ...
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.NoContent ||
                             response.StatusCode == HttpStatusCode.NotFound)
                    {
                        var diagnostic =
                            $"External search for Id: '{query.Id}' QueryKey: '{query.QueryKey}' produced no results - StatusCode: '{response.StatusCode}' Content: '{response.Content}'";

                        context.Log.LogWarning(diagnostic);

                        yield break;
                    }
                    else if (response.ErrorException != null)
                    {
                        var diagnostic =
                            $"External search for Id: '{query.Id}' QueryKey: '{query.QueryKey}' produced no results - StatusCode: '{response.StatusCode}' Content: '{response.Content}'";

                        context.Log.LogError(diagnostic, response.ErrorException);

                        throw new AggregateException(response.ErrorException.Message, response.ErrorException);
                    }
                    else
                    {
                        var diagnostic =
                            $"Failed external search for Id: '{query.Id}' QueryKey: '{query.QueryKey}' - StatusCode: '{response.StatusCode}' Content: '{response.Content}'";

                        context.Log.LogError(diagnostic);

                        throw new ApplicationException(diagnostic);
                    }

                    context.Log.LogTrace("Finished external search for Id: '{Id}' QueryKey: '{QueryKey}'", query.Id, query.QueryKey);
                }
            }
        }

        public IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (context.Log.BeginScope("{0} {1}: query {2}, request {3}, result {4}", GetType().Name, "BuildClues", query, request, result))
            {
                var resultItem = result.As<VatLayerResponse>();
                var dirtyClue = request.CustomQueryInput.ToString();
                var code = new EntityCode(request.EntityMetaData.OriginEntityCode.Type, "vatlayer", $"{query.QueryKey}{request.EntityMetaData.OriginEntityCode}".ToDeterministicGuid());
                var clue = new Clue(code, context.Organization);

                PopulateMetadata(clue.Data.EntityData, resultItem, request);

                context.Log.LogInformation("Clue produced, Id: '{Id}' OriginEntityCode: '{OriginEntityCode}' RawText: '{RawText}'", clue.Id, clue.OriginEntityCode, clue.RawText);

                return new[] { clue };
            }
        }

        public IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (context.Log.BeginScope("{0} {1}: request {2}, result {3}", GetType().Name, "GetPrimaryEntityMetadata", request, result))
            {
                var metadata = CreateMetadata(result.As<VatLayerResponse>(), request);

                context.Log.LogInformation("Primary entity meta data created, Name: '{Name}' OriginEntityCode: '{OriginEntityCode}'", metadata.Name, metadata.OriginEntityCode.Origin.Code);

                return metadata;
            }
        }

        public override IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            // Note: This needs to be cleaned up, but since config and provider is not used in GetPrimaryEntityPreviewImage this is fine.
            var dummyConfig   = new Dictionary<string, object>();
            var dummyProvider = new DefaultExternalSearchProviderProvider(context.ApplicationContext, this);

            return GetPrimaryEntityPreviewImage(context, result, request, dummyConfig, dummyProvider);
        }

        public IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            using (context.Log.BeginScope("{0} {1}: request {2}, result {3}", GetType().Name, "GetPrimaryEntityPreviewImage", request, result))
            {
                context.Log.LogInformation("Primary entity preview image not produced, returning null");

                return null;
            }
        }


        public ConnectionVerificationResult VerifyConnection(ExecutionContext context, IReadOnlyDictionary<string, object> config)
        {
            IDictionary<string, object> configDict = config.ToDictionary(entry => entry.Key, entry => entry.Value);
            var jobData = new VatLayerExternalSearchJobData(configDict);

            var vat = WebUtility.UrlEncode("IE3539798LH");
            var client = new RestClient("http://www.apilayer.net/api");
            var request = new RestRequest($"validate?access_key={jobData.ApiToken}&vat_number={vat}&format=1", Method.GET);

            var response = client.ExecuteAsync<VatLayerResponse>(request).Result;

            return ConstructVerifyConnectionResponse(response);
        }

        private ConnectionVerificationResult ConstructVerifyConnectionResponse(IRestResponse<VatLayerResponse> response)
        {
            var isSuccessResponse = response.IsSuccessful;
            var errorMessageBase = $"{Constants.ProviderName} returned \"{(int)response.StatusCode} {response.StatusDescription}\".";
            if (response.ErrorException != null)
            {
                return new ConnectionVerificationResult(false, $"{errorMessageBase} {(!string.IsNullOrWhiteSpace(response.ErrorException.Message) ? response.ErrorException.Message : "This could be due to breaking changes in the external system")}.");
            }

            var responseData = response.Data;
            if (responseData?.Valid != true)
            {
                try
                {
                    var content = JsonConvert.DeserializeObject<VatLayerErrorResponse>(response.Content);
                    if (!string.IsNullOrWhiteSpace(content?.Error?.Type) && content.Error.Type.Equals("invalid_access_key", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return new ConnectionVerificationResult(false, $"{Constants.ProviderName} returned \"401 Unauthorized\". This could be due to an invalid API key.");
                    }
                }
                catch (Exception exception) {
                    return new ConnectionVerificationResult(false, $"Error deserializing request. The exception received was:\n {exception.Message}");
                }

                isSuccessResponse = false;
            }

            var regex = new Regex(@"\<(html|head|body|div|span|img|p\>|a href)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
            var isHtml = regex.IsMatch(response.Content);

            var errorMessage = isSuccessResponse ? string.Empty
                : string.IsNullOrWhiteSpace(response.Content) || isHtml
                    ? $"{errorMessageBase} This could be due to breaking changes in the external system."
                    : $"{errorMessageBase} {response.Content}.";

            return new ConnectionVerificationResult(isSuccessResponse, errorMessage);
        }

        private IEntityMetadata CreateMetadata(IExternalSearchQueryResult<VatLayerResponse> resultItem, IExternalSearchRequest request)
        {
            var metadata = new EntityMetadataPart();

            PopulateMetadata(metadata, resultItem, request);

            return metadata;
        }

        private void PopulateMetadata(IEntityMetadata metadata, IExternalSearchQueryResult<VatLayerResponse> resultItem, IExternalSearchRequest request)
        {
            var code = new EntityCode(request.EntityMetaData.OriginEntityCode.Type, "vatlayer", $"{request.Queries.FirstOrDefault()?.QueryKey}{request.EntityMetaData.OriginEntityCode}".ToDeterministicGuid());

            metadata.EntityType = request.EntityMetaData.EntityType;
            metadata.Name = request.EntityMetaData.Name;
            metadata.OriginEntityCode = code;
            metadata.Codes.Add(request.EntityMetaData.OriginEntityCode);

            metadata.Properties[VatLayerVocabulary.Organization.Name] = resultItem.Data.CompanyName;

            metadata.Properties[VatLayerVocabulary.Organization.CountryCode] = resultItem.Data.CountryCode;

            metadata.Properties[VatLayerVocabulary.Organization.CvrNumber] = resultItem.Data.VatNumber;

            metadata.Properties[VatLayerVocabulary.Organization.FullVAT] = resultItem.Data.Query;

            metadata.Properties[VatLayerVocabulary.Organization.Address] = resultItem.Data.CompanyAddress;
        }

        // Since this is a configurable external search provider, theses methods should never be called
        public override bool Accepts(EntityType entityType) => throw new NotSupportedException();
        public override IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request) => throw new NotSupportedException();
        public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query) => throw new NotSupportedException();
        public override IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request) => throw new NotSupportedException();
        public override IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request) => throw new NotSupportedException();

        /**********************************************************************************************************
         * PROPERTIES
         **********************************************************************************************************/

        public string Icon { get; } = Constants.Icon;
        public string Domain { get; } = Constants.Domain;
        public string About { get; } = Constants.About;

        public AuthMethods AuthMethods { get; } = Constants.AuthMethods;
        public IEnumerable<Control> Properties { get; } = Constants.Properties;
        public Guide Guide { get; } = Constants.Guide;
        public IntegrationType Type { get; } = Constants.IntegrationType;
    }
}
