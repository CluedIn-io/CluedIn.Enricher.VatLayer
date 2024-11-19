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
            public const string AcceptedEntityType = "acceptedEntityType";
            public const string AcceptedVocabKey = "acceptedVocabKey";

        }

        public static string About { get; set; } = "VatLayer is an enricher for validating and cleaning VAT numbers";
        public static string Icon { get; set; } = "Resources.logo.svg";
        public static string Domain { get; set; } = "https://vatlayer.com/";

        public static AuthMethods AuthMethods { get; set; } = new AuthMethods
        {
            Token = new List<Control>()
            {
                new Control()
                {
                    DisplayName = "Api Key",
                    Type = "input",
                    IsRequired = true,
                    Name = KeyName.ApiToken,
                    Help = "The key to authenticate access to the Vatlayer API."
                },
                new Control()
                {
                    DisplayName = "Accepted Entity Type",
                    Type = "input",
                    IsRequired = true,
                    Name = KeyName.AcceptedEntityType,
                    Help = "The entity type that defines the golden records you want to enrich (e.g., /Organization)."
                },
                new Control()
                {
                    DisplayName = "Accepted Vocabulary Key",
                    Type = "input",
                    IsRequired = false,
                    Name = KeyName.AcceptedVocabKey,
                    Help = "The vocabulary key that contains the VAT numbers of companies you want to enrich (e.g., organization.vat)."
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
