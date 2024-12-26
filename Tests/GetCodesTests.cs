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

        [SetUp]
        public void Setup()
        {
            generator = null;
            File.Delete(TokenGenerator.fileName);
        }

        [Test]
        public void TestGetStoredCodes()
        {
            generator = new TokenGenerator(0, 5);

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
            generator = new TokenGenerator(0, 0);
            Assert.IsFalse(generator.GetCodes(0, 1).Any());
            Assert.IsFalse(generator.GetCodes(1, 1).Any());
        }

        [Test]
        public void TestSeed()
        {
            generator = new TokenGenerator(0, 6, CodeType.ALL);
            List<string> codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "emiU4U");
            Assert.That(codes[1] == "vJ|:<L");
            Assert.That(codes[2] == "\\M}#r~");

            generator = new TokenGenerator(1, 6, CodeType.ALL);
            codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "8+Li^I");
            Assert.That(codes[1] == "By*]#8");
            Assert.That(codes[2] == "?~a^;Z");
        }

        [Test]
        public void TestCodeTypes()
        {
            generator = new TokenGenerator(0, 6, CodeType.ALL);
            List<string> codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "emiU4U");
            Assert.That(codes[1] == "vJ|:<L");
            Assert.That(codes[2] == "\\M}#r~");

            generator = new TokenGenerator(0, 6, CodeType.ALPHABETIC_ONLY_LOWERCASE);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "svtofo");
            Assert.That(codes[1] == "xlzhhm");

            generator = new TokenGenerator(0, 6, CodeType.ALPHABETIC_ONLY_UPPERCASE);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "SVTOFO");
            Assert.That(codes[1] == "XLZHHM");

            generator = new TokenGenerator(0, 6, CodeType.ALPHABETIC_BOTH);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "kpmaLa");
            Assert.That(codes[1] == "uZyPQe");

            generator = new TokenGenerator(0, 6, CodeType.NUMERIC);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "787525");
            Assert.That(codes[1] == "949224");

            generator = new TokenGenerator(0, 6, CodeType.ALPHANUMERIC);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "fmiYYs");
            Assert.That(codes[1] == "QyDESS");
        }

        [Test]
        public void TestOkCharacters()
        {
            generator = new TokenGenerator(0, 1000, CodeType.ALL);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[!-~]+"));

            generator = new TokenGenerator(0, 1000, CodeType.ALPHABETIC_ONLY_LOWERCASE);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[a-z]+"));

            generator = new TokenGenerator(0, 1000, CodeType.ALPHABETIC_ONLY_UPPERCASE);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[A-Z]+"));

            generator = new TokenGenerator(0, 1000, CodeType.ALPHABETIC_BOTH);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[a-zA-Z]+"));

            generator = new TokenGenerator(0, 1000, CodeType.NUMERIC);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[0-9]+"));

            generator = new TokenGenerator(0, 1000, CodeType.ALPHANUMERIC);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[a-zA-Z0-9]+"));
        }
    }
}