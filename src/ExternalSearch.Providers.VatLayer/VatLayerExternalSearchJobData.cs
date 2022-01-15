using System.Collections.Generic;
using CluedIn.Core.Crawling;

namespace CluedIn.ExternalSearch.Providers.VatLayer
{
    public class VatLayerExternalSearchJobData : CrawlJobData
    {
        public VatLayerExternalSearchJobData(IDictionary<string, object> configuration)
        {
            ApiToken = GetValue<string>(configuration, Constants.KeyName.ApiToken);
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object> {
                { Constants.KeyName.ApiToken, ApiToken }
            };
        }

        public string ApiToken { get; set; }
    }
}
