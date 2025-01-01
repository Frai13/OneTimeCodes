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
    public class AntiHackTests
    {
        private TokenGenerator generator;
        private byte[] saltDefault = Enumerable.Range(0, 16).Select(x => (byte)x).ToArray();

        [SetUp]
        public void Setup()
        {
            generator = null;
            File.Delete(TokenGenerator.fileName);
        }

        [Test]
        public void TestDeleteFile()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALL);
            generator.GenerateCodes(0, 3);

            Assert.IsTrue(generator.CheckCode("9?:O)R"));
            File.Delete(TokenGenerator.fileName);
            Assert.IsFalse(generator.CheckCode(";oy@+w"));
            Assert.IsFalse(generator.CheckCode(",RXD~="));
        }

        [Test]
        public void TestFileEncrypted()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, CodeType.ALL);
            generator.GenerateCodes(0, 3);

            string content = File.ReadAllText(TokenGenerator.fileName);
            Assert.IsFalse(content.Contains("9?:O)R"));

            generator = new TokenGenerator("test", saltDefault, 1000, 6, CodeType.ALL);
            Assert.Throws<System.Security.Cryptography.CryptographicException>(() => { generator.CheckCode("9?:O)R"); });
        }
    }
}
