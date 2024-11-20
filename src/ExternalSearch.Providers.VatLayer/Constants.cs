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
        public const string Instruction = """
            [
              {
                "type": "bulleted-list",
                "children": [
                  {
                    "type": "list-item",
                    "children": [
                      {
                        "text": "Add the entity type to specify the golden records you want to enrich. Only golden records belonging to that entity type will be enriched."
                      }
                    ]
                  },
                  {
                    "type": "list-item",
                    "children": [
                      {
                        "text": "Add the vocabulary keys to provide the input for the enricher to search for additional information. For example, if you provide the website vocabulary key for the Web enricher, it will use specific websites to look for information about companies. In some cases, vocabulary keys are not required. If you don't add them, the enricher will use default vocabulary keys."
                      }
                    ]
                  },
                  {
                    "type": "list-item",
                    "children": [
                      {
                        "text": "Add the API key to enable the enricher to retrieve information from a specific API. For example, the Vatlayer enricher requires an access key to authenticate with the Vatlayer API."
                      }
                    ]
                  }
                ]
              }
            ]
            """;
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

        public static Guide Guide { get; set; } = new Guide
        {
            Instructions = Instruction
        };
        public static IntegrationType IntegrationType { get; set; } = IntegrationType.Enrichment;
    }
}
