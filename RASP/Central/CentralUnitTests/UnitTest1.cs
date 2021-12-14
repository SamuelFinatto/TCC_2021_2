using DataCommunicator.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace CentralUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private readonly string KeyTest = "d098q3dju329d92d8j239d82)(SIS)(Ada*&*(";
        private readonly string ExpectedString = "melhor TCC da PUCRS";

        [TestMethod]
        public void CapeHash_MultipleEntries_WorksOnlyWithCorrectKey()
        {
            byte[] bytes = Encoding.ASCII.GetBytes(KeyTest);
            Cape cape = new Cape(bytes, 0);

            byte[] text = Encoding.ASCII.GetBytes(ExpectedString);
            var decripted = cape.Hash(cape.Hash(text));
            var decriptedString = Encoding.ASCII.GetString(decripted);
            Assert.AreEqual(ExpectedString, decriptedString);

            byte[] wrongPass = Encoding.ASCII.GetBytes("da9wdi09ad90d-90id0-9ifs-");
            Cape wrongCape = new Cape(wrongPass, 0);
            var wrongDecripted = wrongCape.Hash(cape.Hash(text));
            var wrongDecriptedString = Encoding.ASCII.GetString(wrongDecripted);
            Assert.AreNotEqual(ExpectedString, wrongDecriptedString);

            byte[] correctPass = Encoding.ASCII.GetBytes(KeyTest);
            Cape correctCape = new Cape(correctPass, 0);
            var correctDecripted = correctCape.Hash(cape.Hash(text));
            var correctDecriptedString = Encoding.ASCII.GetString(correctDecripted);
            Assert.AreEqual(ExpectedString, correctDecriptedString);

        }

        [TestMethod]
        public void CapeHash_UTF8Conversion_ConvertSuccessfully()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(KeyTest);
            Cape cape = new Cape(bytes, 0);

            byte[] text = Encoding.UTF8.GetBytes(ExpectedString);
            var encriptedBytes = cape.Hash(text);
            var encriptedString = Encoding.UTF8.GetString(encriptedBytes);
            var toDecryptBytes = Encoding.UTF8.GetBytes(encriptedString);
            var decrypted = cape.Hash(toDecryptBytes);
            var decriptedString = Encoding.UTF8.GetString(decrypted);
            Assert.AreEqual(ExpectedString, decriptedString);
        }
    }
}
