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
using CluedIn.ExternalSearch.Providers.VatLayer.Models;
using CluedIn.ExternalSearch.Providers.VatLayer.Utility;
using CluedIn.ExternalSearch.Providers.VatLayer.Vocabularies;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using RestSharp;
//using RestSharp.Extensions.MonoHttp;
using EntityType = CluedIn.Core.Data.EntityType;

namespace CluedIn.ExternalSearch.Providers.VatLayer
{
    /// <summary>The VatLayer graph external search provider.</summary>
    /// <seealso cref="ExternalSearchProviderBase" />
    public class VatLayerExternalSearchProvider : ExternalSearchProviderBase, IExtendedEnricherMetadata
    {
        /**********************************************************************************************************
        * CONSTRUCTORS
        **********************************************************************************************************/

        public VatLayerExternalSearchProvider()
           : base(Constants.ExternalSearchProviders.VatLayerId, EntityType.Organization)
        {
            var nameBasedTokenProvider = new NameBasedTokenProvider("VatLayer");

            if (nameBasedTokenProvider.ApiToken != null)
            {
                TokenProvider = new RoundRobinTokenProvider(
                    nameBasedTokenProvider.ApiToken.Split(',', ';'));
            }
        }

        public VatLayerExternalSearchProvider(IEnumerable<string> tokens)
            : this(true)
        {
            TokenProvider = new RoundRobinTokenProvider(tokens);
        }

        public VatLayerExternalSearchProvider(IExternalSearchTokenProvider tokenProvider)
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

        /// <summary>Builds the queries.</summary>
        /// <param name="context">The context.</param>
        /// <param name="request">The request.</param>
        /// <returns>The search queries.</returns>
        public override IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request)
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
                if (string.IsNullOrEmpty(TokenProvider?.ApiToken))
                {
                    context.Log.LogError("ApiToken for VatLayer must be provided.");
                    yield break;
                }

                if (!Accepts(request.EntityMetaData.EntityType))
                {
                    context.Log.LogTrace("Unacceptable entity type from '{EntityName}', entity code '{EntityCode}'",request.EntityMetaData.DisplayName, request.EntityMetaData.EntityType.Code);

                    yield break;
                }

                context.Log.LogTrace("Starting to build queries for {EntityName}",request.EntityMetaData.DisplayName);

                var existingResults = request.GetQueryResults<VatLayerResponse>(this).ToList();

                bool vatFilter(string value) => existingResults.Any(r => string.Equals(r.Data.VatNumber, value, StringComparison.InvariantCultureIgnoreCase));

                var entityType = request.EntityMetaData.EntityType;
                var vatNumber = request.QueryParameters.GetValue(Core.Data.Vocabularies.Vocabularies.CluedInOrganization.VatNumber, new HashSet<string>());
                if (!vatNumber.Any())
                {
                    context.Log.LogTrace("No query parameter for '{VatNumber}' in request, skipping build queries",Core.Data.Vocabularies.Vocabularies.CluedInOrganization.VatNumber);
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
                                context.Log.LogTrace("Sanitized VAT number. Original '{OriginalValue}', Updated '{SanitizedValue}'",value,sanitizedValue);
                            }

                            context.Log.LogInformation("External search query produced, ExternalSearchQueryParameter: '{Identifier}' EntityType: '{EntityCode}' Value: '{SanitizedValue}'",ExternalSearchQueryParameter.Identifier,entityType.Code,sanitizedValue);

                            yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Identifier, sanitizedValue);
                        }
                    }

                    context.Log.LogTrace("Finished building queries for '{Name}'",request.EntityMetaData.Name);
                }
            }
        }

        /// <summary>Executes the search.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <returns>The results.</returns>
        public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query)
        {
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
                if (string.IsNullOrEmpty(TokenProvider?.ApiToken))
                {
                    throw new InvalidOperationException("ApiToken for VatLayer must be provided.");
                }

                context.Log.LogTrace("Starting external search for Id: '{Id}' QueryKey: '{QueryKey}'",query.Id,query.QueryKey);

                var vat = query.QueryParameters[ExternalSearchQueryParameter.Identifier].FirstOrDefault();

                if (string.IsNullOrEmpty(vat))
                {
                    context.Log.LogTrace("No parameter for '{Identifier}' in query, skipping execute search",ExternalSearchQueryParameter.Identifier);
                }
                else
                {
                    // TODO missing try { } catch { } block ...

                    vat = WebUtility.UrlEncode(vat);
                    var client = new RestClient("http://www.apilayer.net/api");
                    var request = new RestRequest($"validate?access_key={TokenProvider.ApiToken}&vat_number={vat}&format=1",
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

                    context.Log.LogTrace("Finished external search for Id: '{Id}' QueryKey: '{QueryKey}'",query.Id,query.QueryKey);
                }
            }
        }

        /// <summary>Builds the clues.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The clues.</returns>
        public override IEnumerable<Clue> BuildClues(ExecutionContext context,
            IExternalSearchQuery query,
            IExternalSearchQueryResult result,
            IExternalSearchRequest request)
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
                var code = GetOriginEntityCode(resultItem);
                var clue = new Clue(code, context.Organization);
                if (!string.IsNullOrEmpty(dirtyClue))
                    clue.Data.EntityData.Codes.Add(new EntityCode(EntityType.Organization, CodeOrigin.CluedIn.CreateSpecific("vatlayer"), dirtyClue));
                PopulateMetadata(clue.Data.EntityData, resultItem);

                context.Log.LogInformation("Clue produced, Id: '{Id}' OriginEntityCode: '{OriginEntityCode}' RawText: '{RawText}'",clue.Id,clue.OriginEntityCode,clue.RawText);

                return new[] {clue};
            }
        }

        /// <summary>Gets the primary entity metadata.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The primary entity metadata.</returns>
        public override IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context,
            IExternalSearchQueryResult result,
            IExternalSearchRequest request)
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
                var metadata =  CreateMetadata(result.As<VatLayerResponse>());

                context.Log.LogInformation("Primary entity meta data created, Name: '{Name}' OriginEntityCode: '{OriginEntityCode}'",metadata.Name,metadata.OriginEntityCode.Origin.Code);

                return metadata;
            }
        }

        /// <summary>Gets the preview image.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The preview image.</returns>
        public override IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
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

        private static IEntityMetadata CreateMetadata(IExternalSearchQueryResult<VatLayerResponse> resultItem)
        {
            var metadata = new EntityMetadataPart();

            PopulateMetadata(metadata, resultItem);

            return metadata;
        }

        private static EntityCode GetOriginEntityCode(IExternalSearchQueryResult<VatLayerResponse> resultItem)
        {
            return new EntityCode(EntityType.Organization, GetCodeOrigin(), resultItem.Data.CountryCode + resultItem.Data.VatNumber);
        }

        private static CodeOrigin GetCodeOrigin()
        {
            return CodeOrigin.CluedIn.CreateSpecific("vatlayer");
        }

        private static void PopulateMetadata(IEntityMetadata metadata, IExternalSearchQueryResult<VatLayerResponse> resultItem)
        {
            var code = GetOriginEntityCode(resultItem);

            metadata.EntityType         = EntityType.Organization;
            metadata.Name               = resultItem.Data.CompanyName;
            metadata.OriginEntityCode   = code;
            metadata.Codes.Add(code);

            metadata.Properties[VatLayerVocabulary.Organization.Name]           = resultItem.Data.CompanyName;

            metadata.Properties[VatLayerVocabulary.Organization.CountryCode]    = resultItem.Data.CountryCode;

            metadata.Properties[VatLayerVocabulary.Organization.CvrNumber]      = resultItem.Data.VatNumber;

            metadata.Properties[VatLayerVocabulary.Organization.FullVAT]        = resultItem.Data.Query;

            metadata.Properties[VatLayerVocabulary.Organization.Address]        = resultItem.Data.CompanyAddress;
        }

        public string Icon { get; } = "Resources.vatlayer.png";
        public string Domain { get; } = "https://vatlayer.com/";
        public string About { get; } = "VatLayer is enricher for validating and cleaning VAT numbers";
        public AuthMethods AuthMethods { get; } = null;
        public IEnumerable<Control> Properties { get; } = null;
        public Guide Guide { get; } = null;
        public IntegrationType Type { get; } = IntegrationType.Cloud;
    }
}
