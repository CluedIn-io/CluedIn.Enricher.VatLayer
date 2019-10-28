using Microsoft.VisualStudio.TestTools.UnitTesting;
using CluedIn.ExternalSearch.Providers.VatLayer.Utility;

namespace VATCleanerTests
{
    [TestClass]
    public class VATUnitTests
    {
        [TestMethod]
        public void TestCheckVATNumber()
        {
            var VatNumber = "BE -/0888533955";
            var expected = "BE0888533955";
            var call = new VatNumberCleaner();
            var actual = call.CheckVATNumber(VatNumber);

            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void TestAUCheckDigit()
        {
            var VatNumber = "U12345678";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.ATVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestBECheckDigit()
        {
            var VatNumber = "0888533955";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.BEVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDenmarkCheckDigit()
        {
            var VatNumber = "10161614";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.DKVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestUKCheckDigit()
        {
            var VatNumber = "310031975";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.GBVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestFRCheckDigit()
        {
            var VatNumber = "02453829079";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.FRVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSECheckDigit()
        {
            var VatNumber = "556410328001";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.SEVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNLCheckDigit()
        {
            var VatNumber = "802353794B01";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.NLVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

       

        [TestMethod]
        public void TestBGCheckDigit()
        {
            var VatNumber = "101004508";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.BGVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCHEDigitCheck()
        {
            var VatNumber = "100416306IVA";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.CHEVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCYCheckDigit()
        {
            var VatNumber = "10111474A";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.CYVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCZCheckDigit()
        {
            var VatNumber = "5511061105";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.CZVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDECheckDigit()
        {
            var VatNumber = "113891176";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.DEVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestEECheckDigit()
        {
            var VatNumber = "100037342";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.EEVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestELCheckDigit()
        {
            var VatNumber = "094112730";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.ELVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestESCheckDigit()
        {
            var VatNumber = "00000019L";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.ESVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void TestFICheckDigit()
        {
            var VatNumber = "09853608";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.FIVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestIECheckDigit()
        {
            var VatNumber = "1409095C";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.IEVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestITCheckDigit()
        {
            var VatNumber = "00224320234";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.ITVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestLTCheckDigit()
        {
            var VatNumber = "100000031710";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.LTVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestLUCheckDigit()
        {
            var VatNumber = "26375245";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.LUVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestLVCheckDigit()
        {
            var VatNumber = "40003032949";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.LVVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMTCheckDigit()
        {
            var VatNumber = "10365719";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.MTVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void TestNOCheckDigit()
        {
            var VatNumber = "864234232";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.NOVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestPLCheckDigit()
        {
            var VatNumber = "5210088067";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.PLVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void TestROCheckDigit()
        {
            var VatNumber = "13182060";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.ROVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void TestRSCheckDigit()
        {
            var VatNumber = "100010812";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.RSVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestRUCheckDigit()
        {
            var VatNumber = "2901081545";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.RUVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSICheckDigit()
        {
            var VatNumber = "10830316";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.SIVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSKCheckDigit()
        {
            var VatNumber = "2020032377";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.SKVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestHRCheckDigit()
        {
            var VatNumber = "12385860076";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.HRVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void TestHUCheckDigit()
        {
            var VatNumber = "10766172";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.HUVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }


        /*Example Customer Tests*/

        [TestMethod]
        public void TestCHEPattern()
        {
            var VatNumber = "CHE-108.315.241";
            var call = new VatNumberCleaner();
            var actual = call.CHEVATCheckDigit(VatNumber);

            Assert.AreEqual("CHE108315241", actual);
        }

        [TestMethod]
        public void TestFRPattern()
        {
            var VatNumber = "FR955 4201 6951";
            var call = new VatNumberCleaner();
            var actual = call.CHEVATCheckDigit(VatNumber);

            Assert.AreEqual("FR95542016951", actual);
        }

        [TestMethod]
        public void TestNLCheckDigit2()
        {
            var VatNumber = "NL802353794B01";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.NLVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSECheckDigit2()
        {
            var VatNumber = "SE556410328001";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.NLVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestESBCheckDigit2()
        {
            var VatNumber = "ESB62131180";
            var expected = true;
            var call = new VatNumberCleaner();
            var actual = call.NLVATCheckDigit(VatNumber);

            Assert.AreEqual(expected, actual);
        }

    }
}
