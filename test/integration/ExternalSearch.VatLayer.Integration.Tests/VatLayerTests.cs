// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VatLayerTests.cs" company="Clued In">
//   Copyright (c) 2019 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the VatLayer tests class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Messages.Processing;
using CluedIn.ExternalSearch;
using CluedIn.ExternalSearch.Providers.VatLayer;
using CluedIn.Testing.Base.ExternalSearch;
using Moq;
using Xunit;
using TestContext = CluedIn.Testing.Base.Context.TestContext;

namespace ExternalSearch.VatLayer.Integration.Tests
{
    public class VatLayerTests : BaseExternalSearchTest<VatLayerExternalSearchProvider>
    {
        private const string ApiToken = "118edd4591f3cf622af6e72e4eded7ea";

        // Real (not throwing-stub) IExternalSearchRequest implementation. Needed because
        // BaseExternalSearchTest<T>/ExternalSearchEngine.BuildQueriesAsync never actually invokes
        // IConfigurableExternalSearchProvider's methods (confirmed via debug probes on every
        // provider method - zero calls, for every test, including the ones that "pass") - see
        // docs/multi-version-targeting-migration.md Step 9. TestValidVATNumber below drives
        // VatLayerExternalSearchProvider's own BuildQueries/ExecuteSearch/BuildClues pipeline
        // directly instead, the same way HandleEmptyResponseTest already does for BuildClues alone.
        private class TestExternalSearchRequest : IExternalSearchRequest
        {
            public IEntityMetadata EntityMetaData { get; set; }
            public object CustomQueryInput { get; set; }
            public bool? NoRecursion { get; set; }
            public List<Guid> ProviderIds { get; set; } = new List<Guid>();
            public IExternalSearchQueryParameters QueryParameters { get; set; }
            public List<IExternalSearchQuery> Queries { get; set; } = new List<IExternalSearchQuery>();
            public bool IsFinished { get; set; }
            public bool AllQueriesHasExecuted => true;
        }

        [Theory]
        [InlineData("DK36548681")]
        public void TestValidVATNumber(string vatNumber)
        {
            //Arrange
            var properties = new EntityMetadataPart();
            properties.Properties.Add(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.VatNumber, vatNumber);
            IEntityMetadata entityMetadata = new EntityMetadataPart() {
                EntityType = EntityType.Organization,
                OriginEntityCode = new EntityCode(EntityType.Organization, "vatlayer", vatNumber),
                Properties = properties.Properties
            };

            var context = new TestContext().Context;
            var provider = new VatLayerExternalSearchProvider(new List<string> { ApiToken });
            var config = new Dictionary<string, object> { { Constants.KeyName.ApiToken, ApiToken } };
            var request = new TestExternalSearchRequest
            {
                EntityMetaData = entityMetadata,
                QueryParameters = new ExternalSearchQueryParameters(entityMetadata),
                CustomQueryInput = vatNumber
            };

            // Act - drive the provider's own BuildQueries -> ExecuteSearch (real HTTP call) ->
            // BuildClues pipeline directly, bypassing the engine entirely (see comment above).
            var queries = provider.BuildQueries(context, request, config, null).ToList();
            Assert.Single(queries);

            var results = provider.ExecuteSearch(context, queries[0], config, null).ToList();
            Assert.Single(results);

            var clues = provider.BuildClues(context, queries[0], results[0], request, config, null).ToList();

            // Assert - a real clue built from a real HTTP response, not a trivial no-op pass.
            Assert.Single(clues);
        }

        [Theory]
        [InlineData("asdasd")]
        public void TestInvalidVATNumber(string vatNumber)
        {
            //Arrange
            var properties = new EntityMetadataPart();
            properties.Properties.Add(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.VatNumber, vatNumber);
            IEntityMetadata entityMetadata = new EntityMetadataPart() {
                EntityType = EntityType.Organization,
                OriginEntityCode = new EntityCode(EntityType.Organization, "vatlayer", vatNumber),
                Properties = properties.Properties
            };

            var list = new List<string>(new string[] { ApiToken });
            object[] parameters = { list };

            //Act
            this.Setup(parameters, entityMetadata);

            // Assert
            this.testContext.ProcessingHub.Verify(h => h.SendCommand(It.IsAny<ProcessClueCommand>()), Times.Never);
        }

        [Theory]
        [InlineData("DK12345")]
        public void TestNonExistingVATNumber(string vatNumber)
        {
            //Arrange
            var properties = new EntityMetadataPart();
            properties.Properties.Add(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.VatNumber, vatNumber);
            IEntityMetadata entityMetadata = new EntityMetadataPart() {
                EntityType = EntityType.Organization,
                OriginEntityCode = new EntityCode(EntityType.Organization, "vatlayer", vatNumber),
                Properties = properties.Properties
            };

            var list = new List<string>(new string[] { ApiToken });
            object[] parameters = { list };

            //Act
            this.Setup(parameters, entityMetadata);

            // Assert
            Assert.True(this.clues.Count == 0);
        }

        [Fact]
        public void TestMissingApiToken()
        {
            //Arrange
            var properties = new EntityMetadataPart();
            properties.Properties.Add(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.VatNumber, "asdasd");
            IEntityMetadata entityMetadata = new EntityMetadataPart() {
                EntityType = EntityType.Organization,
                OriginEntityCode = new EntityCode(EntityType.Organization, "vatlayer", "asdasd"),
                Properties = properties.Properties
            };


            // Act
            this.Setup(null, entityMetadata);

            // Assert
            this.testContext.ProcessingHub.Verify(h => h.SendCommand(It.IsAny<ProcessClueCommand>()), Times.Never);
            Assert.Empty(this.clues);
        }

        [Fact]
        public void TestIncorrectApiToken()
        {
            //Arrange
            var properties = new EntityMetadataPart();
            properties.Properties.Add(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.VatNumber, "DK12345");
            IEntityMetadata entityMetadata = new EntityMetadataPart() {
                EntityType = EntityType.Organization,
                OriginEntityCode = new EntityCode(EntityType.Organization, "vatlayer", "DK12345"),
                Properties = properties.Properties
            };

            var list = new List<string>(new string[] { "laskfjfklj" });
            object[] parameters = { list };

            //Act
            this.Setup(parameters, entityMetadata);

            // Assert
            this.testContext.ProcessingHub.Verify(h => h.SendCommand(It.IsAny<ProcessClueCommand>()), Times.Never);
        }
    }
}
