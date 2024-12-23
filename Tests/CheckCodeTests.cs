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
        private Exception ex;

        [SetUp]
        public void Setup()
        {
            generator = null;
            File.Delete(TokenGenerator.fileName);
        }

        [Test]
        public void TestBlockCodes()
        {
            generator = new TokenGenerator(0, 6, 3, CodeType.ALL);
            generator.GenerateCodes(0, 3);

            Assert.IsFalse(generator.BlockCode("aa"));
            Assert.IsFalse(generator.BlockCode("aaa"));
            Assert.IsFalse(generator.BlockCode("aaaa"));

            Assert.IsTrue(generator.BlockCode("emiU4U"));
            Assert.IsTrue(generator.BlockCode("vJ|:<L"));
            Assert.IsTrue(generator.BlockCode("\\M}#r~"));
            Assert.IsFalse(generator.BlockCode("emiU4U"));
        }

        [Test]
        public void TestGetStoredCodes()
        {
            generator = new TokenGenerator(0, 6, 3, CodeType.ALL);
            generator.GenerateCodes(0, 3);

            Assert.IsFalse(generator.CheckCode("aa"));
            Assert.IsFalse(generator.CheckCode("aaa"));
            Assert.IsFalse(generator.CheckCode("aaaa"));

            Assert.IsTrue(generator.CheckCode("emiU4U"));
            Assert.IsTrue(generator.CheckCode("vJ|:<L"));
            Assert.IsTrue(generator.CheckCode("\\M}#r~"));
            Assert.IsFalse(generator.CheckCode("emiU4U"));
        }

        [Test]
        public void TestGenerateCodesPath()
        {
            var dir = new DirectoryInfo("example");
            dir.Attributes = dir.Attributes & ~FileAttributes.ReadOnly;
            dir.Delete(true);

            generator = new TokenGenerator(0, 6, 3, CodeType.ALL);
            Assert.IsFalse(generator.GenerateCodes(0, 3, "example/"));

            Directory.CreateDirectory("example/");
            Assert.IsTrue(generator.GenerateCodes(0, 3, "example/"));
            Assert.IsFalse(generator.CheckCode("aa"));
            Assert.IsFalse(generator.CheckCode("emiU4U"));

            File.Move($"example/{TokenGenerator.fileName}", TokenGenerator.fileName);

            Assert.IsFalse(generator.CheckCode("aa"));
            Assert.IsFalse(generator.CheckCode("aaa"));
            Assert.IsFalse(generator.CheckCode("aaaa"));

            Assert.IsTrue(generator.CheckCode("emiU4U"));
            Assert.IsTrue(generator.CheckCode("vJ|:<L"));
            Assert.IsTrue(generator.CheckCode("\\M}#r~"));
            Assert.IsFalse(generator.CheckCode("emiU4U"));
        }
    }
}
