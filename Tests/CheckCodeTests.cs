using NUnit.Framework;
using OneTimeCodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    internal class CheckCodeTests
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
        public void TestBlockCodes()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALL);
            generator.GenerateCodes(0, 3);

            Assert.IsFalse(generator.BlockCode("aa"));
            Assert.IsFalse(generator.BlockCode("aaa"));
            Assert.IsFalse(generator.BlockCode("aaaa"));

            Assert.IsTrue(generator.BlockCode("?N*uv?"));
            Assert.IsTrue(generator.BlockCode("{1q%vi"));
            Assert.IsTrue(generator.BlockCode("a5Tqid"));
            Assert.IsFalse(generator.BlockCode("?N*uv?"));
        }

        [Test]
        public void TestGetStoredCodes()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALL);
            generator.GenerateCodes(0, 3);

            Assert.IsFalse(generator.CheckCode("aa"));
            Assert.IsFalse(generator.CheckCode("aaa"));
            Assert.IsFalse(generator.CheckCode("aaaa"));

            Assert.IsTrue(generator.CheckCode("?N*uv?"));
            Assert.IsTrue(generator.CheckCode("{1q%vi"));
            Assert.IsTrue(generator.CheckCode("a5Tqid"));
            Assert.IsFalse(generator.CheckCode("?N*uv?"));

            Assert.IsFalse(generator.CheckCode("lU]SM1"));
            Assert.IsFalse(generator.CheckCode("|'!Sqp"));
            Assert.IsFalse(generator.CheckCode("oxoQ2T"));

            generator = new TokenGenerator("test", saltDefault, 1000, 6, CodeType.ALL);
            generator.GenerateCodes(0, 3);
            Assert.IsTrue(generator.CheckCode("lU]SM1"));
            Assert.IsTrue(generator.CheckCode("|'!Sqp"));
            Assert.IsTrue(generator.CheckCode("oxoQ2T"));
            Assert.IsFalse(generator.CheckCode("lU]SM1"));
        }

        [Test]
        public void TestGenerateCodesPath()
        {
            if (File.Exists("example"))
            {
                var dir = new DirectoryInfo("example");
                dir.Attributes = dir.Attributes & ~FileAttributes.ReadOnly;
                dir.Delete(true);
            }

            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALL);

            Directory.CreateDirectory("example/");
            Assert.IsTrue(generator.GenerateCodes(0, 3, "example/"));
            Assert.IsFalse(generator.CheckCode("aa"));
            Assert.IsFalse(generator.CheckCode("?N*uv?"));

            File.Move($"example/{TokenGenerator.fileName}", TokenGenerator.fileName);

            Assert.IsFalse(generator.CheckCode("aa"));
            Assert.IsFalse(generator.CheckCode("aaa"));
            Assert.IsFalse(generator.CheckCode("aaaa"));

            Assert.IsTrue(generator.CheckCode("?N*uv?"));
            Assert.IsTrue(generator.CheckCode("{1q%vi"));
            Assert.IsTrue(generator.CheckCode("a5Tqid"));
            Assert.IsFalse(generator.CheckCode("?N*uv?"));
        }
    }
}
