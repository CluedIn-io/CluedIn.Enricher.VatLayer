namespace CluedIn.ExternalSearch.VatLayer.Unit.Tests.Utility.VatNumberCleaner
{
    public abstract class TestBase
    {
        protected readonly Providers.VatLayer.Utility.VatNumberCleaner Sut;

        protected TestBase()
        {
            Sut = new Providers.VatLayer.Utility.VatNumberCleaner();
        }
    }
}
