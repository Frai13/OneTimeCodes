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
        private byte[] saltDefault = Enumerable.Range(0, 16).Select(x => (byte)x).ToArray();

        [SetUp]
        public void Setup()
        {
            generator = null;
            File.Delete(TokenGenerator.CodesFileName);
        }

        [Test]
        public void TestBlockCodes()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALL);
            generator.GenerateCodes(0, 3);
            File.Move(TokenGenerator.UserCodesFileName, TokenGenerator.CodesFileName);

            Assert.IsFalse(generator.BlockCode("aa"));
            Assert.IsFalse(generator.BlockCode("aaa"));
            Assert.IsFalse(generator.BlockCode("aaaa"));

            Assert.IsTrue(generator.BlockCode("9?:O)R"));
            Assert.IsTrue(generator.BlockCode(";oy@+w"));
            Assert.IsTrue(generator.BlockCode(",RXD~="));
            Assert.IsFalse(generator.BlockCode("9?:O)R"));
        }

        [Test]
        public void TestGetStoredCodes()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALL);
            generator.GenerateCodes(0, 3);
            File.Move(TokenGenerator.UserCodesFileName, TokenGenerator.CodesFileName);

            Assert.IsFalse(generator.CheckCode("aa"));
            Assert.IsFalse(generator.CheckCode("aaa"));
            Assert.IsFalse(generator.CheckCode("aaaa"));

            Assert.IsTrue(generator.CheckCode("9?:O)R"));
            Assert.IsTrue(generator.CheckCode(";oy@+w"));
            Assert.IsTrue(generator.CheckCode(",RXD~="));
            Assert.IsFalse(generator.CheckCode("9?:O)R"));

            Assert.IsFalse(generator.CheckCode("S#exOP"));
            Assert.IsFalse(generator.CheckCode("FML[Co"));
            Assert.IsFalse(generator.CheckCode("]aR{>H"));

            generator = new TokenGenerator("test", saltDefault, 1000, 6, CodeType.ALL);
            generator.GenerateCodes(0, 3);
            File.Delete(TokenGenerator.CodesFileName);
            File.Move(TokenGenerator.UserCodesFileName, TokenGenerator.CodesFileName);
            Assert.IsTrue(generator.CheckCode("S#exOP"));
            Assert.IsTrue(generator.CheckCode("FML[Co"));
            Assert.IsTrue(generator.CheckCode("]aR{>H"));
            Assert.IsFalse(generator.CheckCode("S#exOP"));
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
            Assert.IsTrue(generator.GenerateCodes(0, 3, "example/codes"));
            Assert.IsFalse(generator.CheckCode("aa"));
            Assert.IsFalse(generator.CheckCode("9?:O)R"));

            File.Move($"example/codes", TokenGenerator.CodesFileName);

            Assert.IsFalse(generator.CheckCode("aa"));
            Assert.IsFalse(generator.CheckCode("aaa"));
            Assert.IsFalse(generator.CheckCode("aaaa"));

            Assert.IsTrue(generator.CheckCode("9?:O)R"));
            Assert.IsTrue(generator.CheckCode(";oy@+w"));
            Assert.IsTrue(generator.CheckCode(",RXD~="));
            Assert.IsFalse(generator.CheckCode("9?:O)R"));
        }
    }
}
