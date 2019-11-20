using CluedIn.ExternalSearch.Providers.VatLayer.Utility;

using Xunit;

namespace CluedIn.ExternalSearch.VatLayer.Unit.Tests.Utility
{
    public class VatNumberCleanerTests
    {
        private readonly VatNumberCleaner _sut;

        public VatNumberCleanerTests()
        {
            _sut = new VatNumberCleaner();
        }

        [Theory]
        [InlineData("BE -/0888533955", "BE0888533955")]
        public void TestCheckVatNumber(string vatNumber, string expected)
        {
            Assert.Equal(expected, _sut.CheckVATNumber(vatNumber));
        }

        [Theory]
        [InlineData("U12345678")]
        public void TestAuCheckDigit(string vatNumber)
        {
            Assert.True(_sut.ATVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("0888533955")]
        public void TestBeCheckDigit(string vatNumber)
        {
            Assert.True(_sut.BEVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("10161614")]
        public void TestDenmarkCheckDigit(string vatNumber)
        {
            Assert.True(_sut.DKVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("310031975")]
        public void TestUkCheckDigit(string vatNumber)
        {
            Assert.True(_sut.GBVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("02453829079")]
        public void TestFrCheckDigit(string vatNumber)
        {
            Assert.True(_sut.FRVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("556410328001")]
        public void TestSeCheckDigit(string vatNumber)
        {
            Assert.True(_sut.SEVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("802353794B01")]
        public void TestNlCheckDigit(string vatNumber)
        {
            Assert.True(_sut.NLVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("101004508")]
        public void TestBgCheckDigit(string vatNumber)
        {
            Assert.True(_sut.BGVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("100416306IVA")]
        public void TestCheDigitCheck(string vatNumber)
        {
            Assert.True(_sut.CHEVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("10111474A")]
        public void TestCyCheckDigit(string vatNumber)
        {
            Assert.True(_sut.CYVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("5511061105")]
        public void TestCzCheckDigit(string vatNumber)
        {
            Assert.True(_sut.CZVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("113891176")]
        public void TestDeCheckDigit(string vatNumber)
        {
            Assert.True(_sut.DEVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("100037342")]
        public void TestEeCheckDigit(string vatNumber)
        {
            Assert.True(_sut.EEVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("094112730")]
        public void TestElCheckDigit(string vatNumber)
        {
            Assert.True(_sut.ELVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("00000019L")]
        public void TestEsCheckDigit(string vatNumber)
        {
            Assert.True(_sut.ESVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("09853608")]
        public void TestFiCheckDigit(string vatNumber)
        {
            Assert.True(_sut.FIVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("1409095C")]
        public void TestIeCheckDigit(string vatNumber)
        {
            Assert.True(_sut.IEVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("00224320234")]
        public void TestItCheckDigit(string vatNumber)
        {
            Assert.True(_sut.ITVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("100000031710")]
        public void TestLtCheckDigit(string vatNumber)
        {
            Assert.True(_sut.LTVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("26375245")]
        public void TestLuCheckDigit(string vatNumber)
        {
            Assert.True(_sut.LUVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("40003032949")]
        public void TestLvCheckDigit(string vatNumber)
        {
            Assert.True(_sut.LVVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("10365719")]
        public void TestMtCheckDigit(string vatNumber)
        {
            Assert.True(_sut.MTVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("864234232")]
        public void TestNoCheckDigit(string vatNumber)
        {
            Assert.True(_sut.NOVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("5210088067")]
        public void TestPlCheckDigit(string vatNumber)
        {
            Assert.True(_sut.PLVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("13182060")]
        public void TestRoCheckDigit(string vatNumber)
        {
            Assert.True(_sut.ROVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("100010812")]
        public void TestRsCheckDigit(string vatNumber)
        {
            Assert.True(_sut.RSVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("2901081545")]
        public void TestRuCheckDigit(string vatNumber)
        {
            Assert.True(_sut.RUVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("10830316")]
        public void TestSiCheckDigit(string vatNumber)
        {
            Assert.True(_sut.SIVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("2020032377")]
        public void TestSkCheckDigit(string vatNumber)
        {
            Assert.True(_sut.SKVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("12385860076")]
        public void TestHrCheckDigit(string vatNumber)
        {
            Assert.True(_sut.HRVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("10766172")]
        public void TestHuCheckDigit(string vatNumber)
        {
            Assert.True(_sut.HUVATCheckDigit(vatNumber));
        }

        /*Example Customer Tests*/

        // If you want to test for the whole VatNumber format(including country code) you call the CheckVATNumber function - returns string
        // If you want to test for only the digit part of VatNumber, you can each individual country check digit function - returns bool

        [Theory]
        [InlineData("CHE-108.315.241", "CHE108315241")]
        public void TestChePattern(string vatNumber, string expected)
        {
            Assert.Equal(expected, _sut.CheckVATNumber(vatNumber));
        }

        [Theory]
        [InlineData("FR955 4201 6951", "FR95542016951")]
        public void TestFrPattern(string vatNumber, string expected)
        {
            Assert.Equal(expected, _sut.CheckVATNumber(vatNumber));
        }

        [Theory]
        [InlineData("NL802353794B01", "NL802353794B01")]
        public void TestNlCheckDigit2(string vatNumber, string expected)
        {
            Assert.Equal(expected, _sut.CheckVATNumber(vatNumber));
        }

        [Theory]
        [InlineData("556410328001")]
        public void TestSeCheckDigit2(string vatNumber)
        {
            Assert.True(_sut.SEVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("B62131180")]
        public void TestEsbCheckDigit2(string vatNumber)
        {
            Assert.True(_sut.ESVATCheckDigit(vatNumber));
        }

        [Theory]
        [InlineData("GB000 0000 11")]
        [InlineData("GB000 0000 12")]
        [InlineData("GB222 5607 89")]
        [InlineData("GB776 5310 13")]
        [InlineData("BE0888533955" )]
        [InlineData("GB417 1157 75")]
        [InlineData("GB702 4610 79")]
        [InlineData("GB272 7221 11")]
        [InlineData("GB000 0000 01")]
        [InlineData("GB735 1406 54")]
        [InlineData("GB000 0000 05")]
        [InlineData("GB168 8372 61")]
        [InlineData("GB000 0000 23")]
        [InlineData("GB747 8798 54")]
        [InlineData("SE556410328001")]
        [InlineData("GB108 3046 44")]
        [InlineData("DK10161614")]
        [InlineData("GB945 7518 87")]
        [InlineData("GB000 0000 31")]
        [InlineData("FR02453829079")]
        [InlineData("GB503 1018 15")]
        [InlineData("NL802353794B01")]
        [InlineData("GB000 0000 17")]
        [InlineData("GB923 4281 36")]
        [InlineData("GB731 7507 42")]
        [InlineData("GB000 0000 02")]
        [InlineData("GB883 8175 78")]
        [InlineData("GB000 0000 06")]
        [InlineData("GB000 0000 27")]
        [InlineData("GB000 0000 00")]
        [InlineData("GB000 0000 03")]
        [InlineData("GB597 2654 89")]
        [InlineData("GB704 2101 01")]
        [InlineData("GB000 0000 25")]
        [InlineData("GB108 2409 39")]
        [InlineData("GB703 5643 53")]
        [InlineData("GB202 1441 76")]
        [InlineData("GB724 6179 27")]
        [InlineData("GB439 4758 08")]
        [InlineData("GB336 3492 51")]
        [InlineData("GB000 0000 08")]
        [InlineData("GB135 5557 06")]
        [InlineData("GB000 0000 09")]
        [InlineData("GB559 0978 89")]
        [InlineData("GB000 0000 07")]
        [InlineData("GB000 0000 04")]
        [InlineData("GB000 0000 10")]
        [InlineData("GB506 6026 70")]
        [InlineData("GB814 2577 34")]
        [InlineData("GB000 0000 21")]
        [InlineData("GB108 2470 36")]
        [InlineData("GB243 1640 91")]
        [InlineData("GB997 3273 64")]
        [InlineData("GB919 2934 95")]
        [InlineData("GB892 1637 03")]
        [InlineData("GB000 0000 14")]
        [InlineData("GB000 0000 15")]
        [InlineData("GB160 0904 45")]
        [InlineData("GB297 9364 87")]
        [InlineData("GB000 0000 28")]
        [InlineData("GB434 9713 35")]
        [InlineData("GB494 5524 16")]
        [InlineData("GB000 0000 19")]
        [InlineData("GB000 0000 24")]
        [InlineData("GB000 0000 26")]
        [InlineData("GB223 7100 10")]
        [InlineData("GB000 0000 13")]
        [InlineData("GB804 1163 73")]
        [InlineData("GB394 1212 63")]
        [InlineData("GB178 3069 80")]
        [InlineData("GB239 2074 64")]
        [InlineData("GB801 7806 45")]
        [InlineData("GB000 0000 16")]
        [InlineData("GB000 0000 18")]
        [InlineData("GB732 3615 53")]
        [InlineData("GB298 4484 96")]
        [InlineData("GB505 4652 55")]
        [InlineData("GB694 1366 12")]
        [InlineData("GB000 0000 20")]
        [InlineData("GB000 0000 22")]
        [InlineData("GB223 4728 76")]
        [InlineData("GB670 7557 13")]
        [InlineData("GB989 1122 90")]
        [InlineData("GB795 9294 54")]
        [InlineData("GB746 6441 14")]
        [InlineData("GB000 0000 29")]
        [InlineData("GB823 8535 19")]
        [InlineData("GB772 1252 45")]
        [InlineData("GB310 0319 75")]
        [InlineData("GB000 0000 54")]
        public void CheckIssVatNumber(string vatNumber)
        {
            Assert.NotEmpty(
                _sut.CheckVATNumber(vatNumber));
        }
    }
}
