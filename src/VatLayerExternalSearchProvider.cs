using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.ExternalSearch;
using CluedIn.ExternalSearch.Providers.VatLayer.Models;
using CluedIn.ExternalSearch.Providers.VatLayer.Utility;
using CluedIn.ExternalSearch.Providers.VatLayer.Vocabularies;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Extensions.MonoHttp;

namespace CluedIn.ExternalSearch.Providers.VatLayer
{
    /// <summary>The vatlayer graph external search provider.</summary>
    /// <seealso cref="ExternalSearchProviderBase" />
    public class VatLayerExternalSearchProvider : ExternalSearchProviderBase
    {
        /**********************************************************************************************************
        * CONSTRUCTORS
        **********************************************************************************************************/

        public VatLayerExternalSearchProvider()
           : base(Constants.ExternalSearchProviders.VatLayerId, EntityType.Organization)
        {
            var nameBasedTokenProvider = new NameBasedTokenProvider("VatLayer");

            if (nameBasedTokenProvider.ApiToken != null)
                this.TokenProvider = new RoundRobinTokenProvider(nameBasedTokenProvider.ApiToken.Split(',', ';'));
        }

        public VatLayerExternalSearchProvider(IList<string> tokens)
            : this(true)
        {
            this.TokenProvider = new RoundRobinTokenProvider(tokens);
        }

        public VatLayerExternalSearchProvider(IExternalSearchTokenProvider tokenProvider)
            : this(true)
        {
            this.TokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        private VatLayerExternalSearchProvider(bool tokenProviderIsRequired)
            : this()
        {
            this.TokenProviderIsRequired = tokenProviderIsRequired;
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
                throw new InvalidOperationException("ApiToken for VatLayer must be provided.");

            if (!this.Accepts(request.EntityMetaData.EntityType))
                yield break;

            var existingResults = request.GetQueryResults<VatLayerResponse>(this).ToList();

            Func<string, bool> vatFilter = value => existingResults.Any(r => string.Equals(r.Data.VatNumber, value, StringComparison.InvariantCultureIgnoreCase));

            var entityType = request.EntityMetaData.EntityType;
            var vatNumber  = request.QueryParameters.GetValue(Core.Data.Vocabularies.Vocabularies.CluedInOrganization.VatNumber, new HashSet<string>());

            if (vatNumber == null)
                throw new ArgumentNullException();

            foreach (var value in vatNumber.Where(v => !vatFilter(v)))
            {
                var cleaner = new VatNumberCleaner();
                var sanitizedValue = cleaner.CheckVATNumber(value);
                //yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Identifier, value);
                    yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Identifier, sanitizedValue);
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
            var vat = query.QueryParameters[ExternalSearchQueryParameter.Identifier].FirstOrDefault();

            if (string.IsNullOrEmpty(vat))
                yield break;

            vat          = HttpUtility.UrlEncode(vat);
            var client   = new RestClient("http://www.apilayer.net/api");
            var request  = new RestRequest($"validate?access_key={this.TokenProvider.ApiToken}&vat_number={vat}&format=1", Method.GET);
            var response = client.ExecuteTaskAsync<VatLayerResponse>(request).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (response.Data != null && response.Data.Valid)
                    yield return new ExternalSearchQueryResult<VatLayerResponse>(query, response.Data);
                else
                {
                    var content = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    if (content.error != null)
                    {
                        throw new InvalidOperationException($"{content.error.info} - Type: {content.error.type} Code: {content.error.code}");
                    }
                }
            }
            else if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
                yield break;
            else if (response.ErrorException != null)
                throw new AggregateException(response.ErrorException.Message, response.ErrorException);
            else
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode + "; Content: " + response.Content);
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
        {
            var resultItem  = result.As<VatLayerResponse>();
            var code        = this.GetOriginEntityCode(resultItem);
            var clue        = new Clue(code, context.Organization);

            this.PopulateMetadata(clue.Data.EntityData, resultItem);

            return new[] { clue };
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
        {
            var resultItem = result.As<VatLayerResponse>();
            return this.CreateMetadata(resultItem);
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
            return null;
        }

        /// <summary>Creates the metadata.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The metadata.</returns>
        private IEntityMetadata CreateMetadata(IExternalSearchQueryResult<VatLayerResponse> resultItem)
        {
            var metadata = new EntityMetadataPart();

            this.PopulateMetadata(metadata, resultItem);

            return metadata;
        }

        /// <summary>Gets the origin entity code.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The origin entity code.</returns>
        private EntityCode GetOriginEntityCode(IExternalSearchQueryResult<VatLayerResponse> resultItem)
        {
            return new EntityCode(EntityType.Organization, this.GetCodeOrigin(), resultItem.Data.VatNumber);
        }

        /// <summary>Gets the code origin.</summary>
        /// <returns>The code origin</returns>
        private CodeOrigin GetCodeOrigin()
        {
            return CodeOrigin.CluedIn.CreateSpecific("vatlayer");
        }

        /// <summary>Populates the metadata.</summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="resultItem">The result item.</param>
        private void PopulateMetadata(IEntityMetadata metadata, IExternalSearchQueryResult<VatLayerResponse> resultItem)
        {
            var code = this.GetOriginEntityCode(resultItem);

            metadata.EntityType         = EntityType.Organization;
            metadata.Name               = resultItem.Data.CompanyName;
            metadata.OriginEntityCode   = code;

            metadata.Codes.Add(code);

            metadata.Properties[VatLayerVocabulary.Organization.Name]           = resultItem.Data.CompanyName;

            metadata.Properties[VatLayerVocabulary.Organization.CountryCode]    = resultItem.Data.CountryCode;

            if (resultItem.Data.CountryCode == "DK")
                metadata.Properties[VatLayerVocabulary.Organization.CvrNumber]  = resultItem.Data.VatNumber;

            metadata.Properties[VatLayerVocabulary.Organization.FullVAT]        = resultItem.Data.Query;
            
            metadata.Properties[VatLayerVocabulary.Organization.Address]        = resultItem.Data.CompanyAddress;
        }
    }
}
