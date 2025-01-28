using Moq;
using NUnit.Framework;
using OneTimeCodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tests
{
    public class GetCodesTests
    {
        private TokenGenerator generator;
        private byte[] saltDefault = Enumerable.Range(0, 16).Select(x => (byte)x).ToArray();

        [SetUp]
        public void Setup()
        {
            generator = null;
            TestsUtils.FileDelete(TokenGenerator.CodesFileName);
        }

        [Test]
        public void TestGetStoredCodes()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 5);

            generator.BytesSaved = new List<byte> { 48, 49, 50, 51, 52 };
            List<string> codes = generator.GetCodes(0, 1);
            Assert.That(codes[0] == "01234");

            generator.BytesSaved.AddRange(new byte[] { 53, 54, 55, 56, 57 });
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "01234");
            Assert.That(codes[1] == "56789");
        }

        [Test]
        public void TestOutOfRange()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 0);
            Assert.IsFalse(generator.GetCodes(0, 1).Any());
            Assert.IsFalse(generator.GetCodes(1, 1).Any());
        }

        [Test]
        public void TestRngParams()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALL);
            List<string> codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "9?:O)R");
            Assert.That(codes[1] == ";oy@+w");
            Assert.That(codes[2] == ",RXD~=");
            generator = new TokenGenerator("", saltDefault, 100, 6, CodeType.ALL);
            codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "v1AB$&");
            Assert.That(codes[1] == "(.#Cb4");
            Assert.That(codes[2] == "r\\@n`Q");

            generator = new TokenGenerator("test", saltDefault, 1000, 6, CodeType.ALL);
            codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "S#exOP");
            Assert.That(codes[1] == "FML[Co");
            Assert.That(codes[2] == "]aR{>H");
            generator = new TokenGenerator("test", saltDefault, 100, 6, CodeType.ALL);
            codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "K>Aw<V");
            Assert.That(codes[1] == "q0cd;s");
            Assert.That(codes[2] == "jN7+wc");

            generator = new TokenGenerator("", Enumerable.Range(1, 16).Select(x => (byte)x).ToArray(), 1000, 6, CodeType.ALL);
            codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "7j\\Wlw");
            Assert.That(codes[1] == "P-oq?*");
            Assert.That(codes[2] == "o9&B+P");
            generator = new TokenGenerator("", Enumerable.Range(1, 16).Select(x => (byte)x).ToArray(), 100, 6, CodeType.ALL);
            codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "tg7O>)");
            Assert.That(codes[1] == "%maa=Y");
            Assert.That(codes[2] == "K+F@T3");
        }

        [Test]
        public void TestCodeTypes()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALL);
            List<string> codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "9?:O)R");
            Assert.That(codes[1] == ";oy@+w");
            Assert.That(codes[2] == ",RXD~=");

            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALPHABETIC_ONLY_LOWERCASE);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "oywcal");
            Assert.That(codes[1] == "ahukdr");

            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALPHABETIC_ONLY_UPPERCASE);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "ORRXDI");
            Assert.That(codes[1] == "CRFBJZ");

            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALPHABETIC_BOTH);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "ORoywR");
            Assert.That(codes[1] == "XDcaIl");

            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.NUMERIC);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "991583");
            Assert.That(codes[1] == "589604");

            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALPHANUMERIC);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "9ORoyw");
            Assert.That(codes[1] == "RXDcaI");
        }

        [Test]
        public void TestOkCharacters()
        {
            generator = new TokenGenerator("", saltDefault, 100, 1000, CodeType.ALL);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[!-~]+"));

            generator = new TokenGenerator("", saltDefault, 100, 1000, CodeType.ALPHABETIC_ONLY_LOWERCASE);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[a-z]+"));

            generator = new TokenGenerator("", saltDefault, 100, 1000, CodeType.ALPHABETIC_ONLY_UPPERCASE);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[A-Z]+"));

            generator = new TokenGenerator("", saltDefault, 100, 1000, CodeType.ALPHABETIC_BOTH);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[a-zA-Z]+"));

            generator = new TokenGenerator("", saltDefault, 100, 1000, CodeType.NUMERIC);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[0-9]+"));

            generator = new TokenGenerator("", saltDefault, 100, 1000, CodeType.ALPHANUMERIC);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[a-zA-Z0-9]+"));
        }
    }
}