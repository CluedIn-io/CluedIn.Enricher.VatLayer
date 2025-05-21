namespace CluedIn.ExternalSearch.Providers.VatLayer.Models
{
    public class VatLayerErrorResponse
    {
        public bool Success { get; set; }
        public VatLayerError Error { get; set; }
    }

    public class VatLayerError
    {
        public int Code { get; set; }
        public string Type { get; set; }
        public string Info { get; set; }
    }
}
