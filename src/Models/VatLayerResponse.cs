using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.VatLayer.Models
{

    public class VatLayerResponse
    {
        [JsonProperty("valid")]
        public bool Valid { get; set; }

        [JsonProperty("format_valid")]
        public bool FormatValid { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("vat_number")]
        public string VatNumber { get; set; }

        [JsonProperty("company_name")]
        public string CompanyName { get; set; }

        [JsonProperty("company_address")]
        public string CompanyAddress { get; set; }
    }

}
