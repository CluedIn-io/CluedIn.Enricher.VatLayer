using System.Collections.Generic;
using CluedIn.Core.Crawling;

namespace CluedIn.ExternalSearch.Providers.VatLayer
{
    public class VatLayerExternalSearchJobData : CrawlJobData
    {
        public VatLayerExternalSearchJobData(IDictionary<string, object> configuration)
        {
            ApiToken = GetValue<string>(configuration, Constants.KeyName.ApiToken);
            AcceptedEntityType = GetValue<string>(configuration, Constants.KeyName.AcceptedEntityType);
            AcceptedVocabKey = GetValue<string>(configuration, Constants.KeyName.AcceptedVocabKey);
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object> {
                { Constants.KeyName.ApiToken, ApiToken },
                { Constants.KeyName.AcceptedEntityType, AcceptedEntityType },
                { Constants.KeyName.AcceptedVocabKey, AcceptedVocabKey }
            };
        }

        public string ApiToken { get; set; }
        public string AcceptedEntityType { get; set; }
        public string AcceptedVocabKey { get; set; }
    }
}
