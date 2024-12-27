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
        private byte[] saltDefault = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };

        [SetUp]
        public void Setup()
        {
            generator = null;
            File.Delete(TokenGenerator.fileName);
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
            Assert.That(codes[0] == "?N*uv?");
            Assert.That(codes[1] == "{1q%vi");
            Assert.That(codes[2] == "a5Tqid");
            generator = new TokenGenerator("", saltDefault, 100, 6, CodeType.ALL);
            codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "GKd9;~");
            Assert.That(codes[1] == "\"d*kNn");
            Assert.That(codes[2] == "igdpo_");

            generator = new TokenGenerator("test", saltDefault, 1000, 6, CodeType.ALL);
            codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "lU]SM1");
            Assert.That(codes[1] == "|'!Sqp");
            Assert.That(codes[2] == "oxoQ2T");
            generator = new TokenGenerator("test", saltDefault, 100, 6, CodeType.ALL);
            codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "In%v(B");
            Assert.That(codes[1] == "D{<{<=");
            Assert.That(codes[2] == "*cOWW_");

            generator = new TokenGenerator("", new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 1000, 6, CodeType.ALL);
            codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "A,*Uwu");
            Assert.That(codes[1] == "/pk+Zt");
            Assert.That(codes[2] == "5^o7]c");
            generator = new TokenGenerator("", new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 100, 6, CodeType.ALL);
            codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "/Bp+*k");
            Assert.That(codes[1] == ".-EL)a");
            Assert.That(codes[2] == "C.r6>Y");
        }

        [Test]
        public void TestCodeTypes()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALL);
            List<string> codes = generator.GetCodes(0, 3);
            Assert.That(codes[0] == "?N*uv?");
            Assert.That(codes[1] == "{1q%vi");
            Assert.That(codes[2] == "a5Tqid");

            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALPHABETIC_ONLY_LOWERCASE);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "uvqvia");
            Assert.That(codes[1] == "qidqcq");

            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALPHABETIC_ONLY_UPPERCASE);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "NTSBIK");
            Assert.That(codes[1] == "WWDEHA");

            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALPHABETIC_BOTH);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "Nuvqvi");
            Assert.That(codes[1] == "aTqidS");

            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.NUMERIC);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "151108");
            Assert.That(codes[1] == "058796");

            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALPHANUMERIC);
            codes = generator.GetCodes(0, 2);
            Assert.That(codes[0] == "Nuv1qv");
            Assert.That(codes[1] == "ia5Tqi");
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