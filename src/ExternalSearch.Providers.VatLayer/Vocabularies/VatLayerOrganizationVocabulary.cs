// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VatLayerOrganizationVocabulary.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the VatLayerOrganizationVocabulary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.VatLayer.Vocabularies
{
    /// <summary>The VatLayer organization vocabulary</summary>
    /// <seealso cref="CluedIn.Core.Data.Vocabularies.SimpleVocabulary" />
    public class VatLayerOrganizationVocabulary : SimpleVocabulary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VatLayerOrganizationVocabulary"/> class.
        /// </summary>
        public VatLayerOrganizationVocabulary()
        {
            this.VocabularyName = "VatLayer Organization";
            this.KeyPrefix      = "vatLayer.organization";
            this.KeySeparator   = ".";
            this.Grouping       = EntityType.Organization;

            this.AddGroup("Metadata", group => 
            {
                this.Name                               = group.Add(new VocabularyKey("name"));
                this.CountryCode                        = group.Add(new VocabularyKey("countryCode"));
                this.CvrNumber                          = group.Add(new VocabularyKey("cvrNumber",      VocabularyKeyDataType.Integer));
                this.FullVAT                            = group.Add(new VocabularyKey("fullVat"));
                this.Address                            = group.Add(new VocabularyKey("address"));

                this.AddMapping(this.Name,          Core.Data.Vocabularies.Vocabularies.CluedInOrganization.OrganizationName);
                this.AddMapping(this.CountryCode,   Core.Data.Vocabularies.Vocabularies.CluedInOrganization.AddressCountryCode);
                this.AddMapping(this.CvrNumber,     Core.Data.Vocabularies.Vocabularies.CluedInOrganization.CodesCVR);
                this.AddMapping(this.FullVAT,       Core.Data.Vocabularies.Vocabularies.CluedInOrganization.VatNumber);
                this.AddMapping(this.Address,       Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Address);

            });

        }

        public VocabularyKey Name { get; protected set; }
        public VocabularyKey CountryCode { get; protected set; }
        public VocabularyKey CvrNumber { get; protected set; }
        public VocabularyKey FullVAT { get; protected set; }
        public VocabularyKey Address { get; protected set; }



    }
}
