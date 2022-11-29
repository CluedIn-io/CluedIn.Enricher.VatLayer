using System;
using System.Collections.Generic;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.Providers;

namespace CluedIn.ExternalSearch.Providers.VatLayer
{
    public static class Constants
    {
        public const string ComponentName = "VatLayer";
        public const string ProviderName = "Vat Layer";
        public static readonly Guid ProviderId = Core.Constants.ExternalSearchProviders.VatLayerId;

        public struct KeyName
        {
            public const string ApiToken = "apiToken";
            public const string AcceptedEntityTypes = "acceptedEntityTypes";
            public const string AcceptedVocabKeys = "acceptedVocabKeys";

        }

        public static string About { get; set; } = "VatLayer is an enricher for validating and cleaning VAT numbers";
        public static string Icon { get; set; } = "Resources.vatlayer.png";
        public static string Domain { get; set; } = "https://vatlayer.com/";

        public static AuthMethods AuthMethods { get; set; } = new AuthMethods
        {
            token = new List<Control>()
            {
                new Control()
                {
                    displayName = "Api Key",
                    type = "input",
                    isRequired = true,
                    name = KeyName.ApiToken
                },
                new Control()
                {
                    displayName = "Accepted Entity Types",
                    type = "input",
                    isRequired = true,
                    name = KeyName.AcceptedEntityTypes
                },
                new Control()
                {
                    displayName = "Accepted Vocab Keys",
                    type = "input",
                    isRequired = true,
                    name = KeyName.AcceptedVocabKeys
                },
            }
        };

        public static IEnumerable<Control> Properties { get; set; } = new List<Control>()
        {
            // NOTE: Leaving this commented as an example - BF
            //new()
            //{
            //    displayName = "Some Data",
            //    type = "input",
            //    isRequired = true,
            //    name = "someData"
            //}
        };

        public static Guide Guide { get; set; } = null;
        public static IntegrationType IntegrationType { get; set; } = IntegrationType.Enrichment;
    }
}
