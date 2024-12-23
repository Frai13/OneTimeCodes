using Moq;
using NUnit.Framework;
using OneTimeCodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Tests
{
    public class GetCodesTests
    {
        private TokenGenerator generator;
        private Exception ex;

        [SetUp]
        public void Setup()
        {
            generator = null;
            File.Delete(TokenGenerator.fileName);
        }

        [Test]
        public void TestGetStoredCodes()
        {
            generator = new TokenGenerator(0, 5, 10);

            generator.BytesSaved = new List<byte> { 48, 49, 50, 51, 52 };
            List<string> codes = generator.GetCodes(0, 1);
            Assert.That(codes[0] == "01234");

            generator.BytesSaved.AddRange(new byte[] { 53, 54, 55, 56, 57 });
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "01234");
            Assert.That(codes[1] == "56789");
        }

        [Test]
        public void TestOutOfRangeException()
        {
            generator = new TokenGenerator(0, 0, 0);
            ex = Assert.Throws<ArgumentOutOfRangeException>(() => { generator.GetCodes(0, 1); });
            Assert.That(ex.Message.StartsWith("Length should be greater than 0."));
            ex = Assert.Throws<ArgumentOutOfRangeException>(() => { generator.GetCodes(1, 1); });
            Assert.That(ex.Message.StartsWith("Length should be greater than 0."));

            generator = new TokenGenerator(0, 1, 0);
            ex = Assert.Throws<ArgumentOutOfRangeException>(() => { generator.GetCodes(0, 1); });
            Assert.That(ex.Message.StartsWith("Maximum number of codes exceeded: 0."));
            ex = Assert.Throws<ArgumentOutOfRangeException>(() => { generator.GetCodes(1, 1); });
            Assert.That(ex.Message.StartsWith("Maximum number of codes exceeded: 0."));
            generator = new TokenGenerator(0, 1, 1);
            ex = Assert.Throws<ArgumentOutOfRangeException>(() => { generator.GetCodes(1, 1); });
            Assert.That(ex.Message.StartsWith("Maximum number of codes exceeded: 1."));
            ex = Assert.Throws<ArgumentOutOfRangeException>(() => { generator.GetCodes(2, 1); });
            Assert.That(ex.Message.StartsWith("Maximum number of codes exceeded: 1."));
        }

        [Test]
        public void TestCodeTypes()
        {
            generator = new TokenGenerator(0, 6, 3, CodeType.ALL);
            List<string> codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "emiU4U");
            Assert.That(codes[1] == "vJ|:<L");
            Assert.That(codes[2] == "\\M}#r~");

            generator = new TokenGenerator(0, 6, 2, CodeType.ALPHABETIC_ONLY_LOWERCASE);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "svtofo");
            Assert.That(codes[1] == "xlzhhm");

            generator = new TokenGenerator(0, 6, 2, CodeType.ALPHABETIC_ONLY_UPPERCASE);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "SVTOFO");
            Assert.That(codes[1] == "XLZHHM");

            generator = new TokenGenerator(0, 6, 2, CodeType.ALPHABETIC_BOTH);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "kpmaLa");
            Assert.That(codes[1] == "uZyPQe");

            generator = new TokenGenerator(0, 6, 2, CodeType.NUMERIC);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "787525");
            Assert.That(codes[1] == "949224");

            generator = new TokenGenerator(0, 6, 2, CodeType.ALPHANUMERIC);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "fmiYYs");
            Assert.That(codes[1] == "QyDESS");
        }

        [Test]
        public void TestOkCharacters()
        {
            generator = new TokenGenerator(0, 1000, 1, CodeType.ALL);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[!-~]+"));

            generator = new TokenGenerator(0, 1000, 1, CodeType.ALPHABETIC_ONLY_LOWERCASE);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[a-z]+"));

            generator = new TokenGenerator(0, 1000, 1, CodeType.ALPHABETIC_ONLY_UPPERCASE);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[A-Z]+"));

            generator = new TokenGenerator(0, 1000, 1, CodeType.ALPHABETIC_BOTH);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[a-zA-Z]+"));

            generator = new TokenGenerator(0, 1000, 1, CodeType.NUMERIC);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[0-9]+"));

            generator = new TokenGenerator(0, 1000, 1, CodeType.ALPHANUMERIC);
            Assert.True(Regex.IsMatch(generator.GetCodes(0, 1)[0], "[a-zA-Z0-9]+"));
        }
    }
}