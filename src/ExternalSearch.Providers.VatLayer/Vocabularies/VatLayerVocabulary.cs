
namespace CluedIn.ExternalSearch.Providers.VatLayer.Vocabularies
{
    /// <summary>The VatLayer vocabulary</summary>
    public static class VatLayerVocabulary
    {
        /// <summary>
        /// Initializes static members of the <see cref="VatLayerVocabulary" /> class.
        /// </summary>
        static VatLayerVocabulary()
        {
            Organization = new VatLayerOrganizationVocabulary();
        }

        /// <summary>Gets the organization.</summary>
        /// <value>The organization.</value>
        public static VatLayerOrganizationVocabulary Organization { get; private set; }
    }
}
