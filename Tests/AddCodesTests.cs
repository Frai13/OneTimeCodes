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
    public class AddCodesTests
    {
        private TokenGenerator generator;
        private byte[] saltDefault = Enumerable.Range(0, 16).Select(x => (byte)x).ToArray();

        [SetUp]
        public void Setup()
        {
            generator = null;
            TestsUtils.FileDelete(TokenGenerator.CodesFileName);
            TestsUtils.FileDelete("codes_test");
        }

        [Test]
        public void TestAddCodesEmpty()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, TokenGenerator.CodesFileName, CodeType.ALL);
            generator.GenerateCodes(0, 3);

            Assert.IsTrue(generator.AddCodes(TokenGenerator.UserCodesFileName));
            Assert.IsTrue(generator.CheckCode("9?:O)R"));
            Assert.IsTrue(generator.CheckCode(";oy@+w"));
            Assert.IsTrue(generator.CheckCode(",RXD~="));
            Assert.IsFalse(generator.CheckCode("9?:O)R"));
        }

        [Test]
        public void TestFileNotExists()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, TokenGenerator.CodesFileName, CodeType.ALL);

            TestsUtils.FileDelete(TokenGenerator.UserCodesFileName);
            Assert.IsFalse(generator.AddCodes(TokenGenerator.UserCodesFileName));
        }

        [Test]
        public void TestAddCodesOverlap()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, TokenGenerator.CodesFileName, CodeType.ALL);
            generator.GenerateCodes(0, 3);

            Assert.IsTrue(generator.AddCodes(TokenGenerator.UserCodesFileName));
            Assert.IsTrue(generator.CheckCode("9?:O)R"));
            Assert.IsFalse(generator.CheckCode("/c!aI\""));

            generator.GenerateCodes(2, 3);
            Assert.IsTrue(generator.AddCodes(TokenGenerator.UserCodesFileName));
            Assert.IsFalse(generator.CheckCode("9?:O)R"));
            Assert.IsTrue(generator.CheckCode(";oy@+w"));
            Assert.IsTrue(generator.CheckCode(",RXD~="));
            Assert.IsFalse(generator.CheckCode(",RXD~="));
            Assert.IsTrue(generator.CheckCode("/c!aI\""));
            Assert.IsTrue(generator.CheckCode("laCh91"));

            generator.GenerateCodes(0, 3);

            Assert.IsTrue(generator.AddCodes(TokenGenerator.UserCodesFileName));
            Assert.IsTrue(generator.CheckCode(",RXD~="));
            Assert.IsFalse(generator.CheckCode(",RXD~="));

            generator.GenerateCodes(2, 3);
            Assert.IsTrue(generator.AddCodes(TokenGenerator.UserCodesFileName));
            Assert.IsTrue(generator.CheckCode("9?:O)R"));
            Assert.IsTrue(generator.CheckCode(";oy@+w"));
            Assert.IsTrue(generator.CheckCode(",RXD~="));
            Assert.IsTrue(generator.CheckCode("/c!aI\""));
            Assert.IsTrue(generator.CheckCode("laCh91"));
            Assert.IsFalse(generator.CheckCode(",RXD~="));
        }

        [Test]
        public void TestAddCodesNotEmpty()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, TokenGenerator.CodesFileName, CodeType.ALL);
            generator.GenerateCodes(0, 3);

            Assert.IsTrue(generator.AddCodes(TokenGenerator.UserCodesFileName));
            Assert.IsTrue(generator.CheckCode("9?:O)R"));
            Assert.IsFalse(generator.CheckCode("/c!aI\""));

            generator.GenerateCodes(3, 3);
            Assert.IsTrue(generator.AddCodes(TokenGenerator.UserCodesFileName));
            Assert.IsFalse(generator.CheckCode("9?:O)R"));
            Assert.IsTrue(generator.CheckCode(";oy@+w"));
            Assert.IsTrue(generator.CheckCode(",RXD~="));
            Assert.IsTrue(generator.CheckCode("/c!aI\""));
            Assert.IsTrue(generator.CheckCode("laCh91"));
            Assert.IsTrue(generator.CheckCode("?R=uk5"));
        }

        [Test]
        public void TestAddCodesDifferentPass()
        {
            TokenGenerator generatorTest = new TokenGenerator("test", saltDefault, 1000, 6, TokenGenerator.CodesFileName, CodeType.ALL);
            generatorTest.GenerateCodes(0, 3, "codes_test");
            generator = new TokenGenerator("", saltDefault, 1000, 6, TokenGenerator.CodesFileName, CodeType.ALL);
            generator.GenerateCodes(0, 3);

            Assert.Throws<System.Security.Cryptography.CryptographicException>(() => { generator.AddCodes("codes_test"); });
            Assert.IsTrue(generator.AddCodes(TokenGenerator.UserCodesFileName));
            Assert.IsFalse(generator.CheckCode("S#exOP"));
            Assert.IsFalse(generator.CheckCode("FML[Co"));
            Assert.IsFalse(generator.CheckCode("]aR{>H"));
            Assert.IsTrue(generator.CheckCode("9?:O)R"));
            Assert.IsTrue(generator.CheckCode(";oy@+w"));
            Assert.IsTrue(generator.CheckCode(",RXD~="));
            Assert.IsFalse(generator.CheckCode("9?:O)R"));
        }

        [Test]
        public void TestAddCodesReallocatedFile()
        {
            TestsUtils.FileDelete("path_to_file/codes_file");
            Directory.CreateDirectory("path_to_file");
            generator = new TokenGenerator("", saltDefault, 1000, 6, "path_to_file/codes_file");
            generator.GenerateCodes(0, 3);

            Assert.IsTrue(generator.AddCodes(TokenGenerator.UserCodesFileName));
            Assert.IsTrue(generator.CheckCode("9?:O)R"));
            Assert.IsTrue(generator.CheckCode(";oy@+w"));
            TestsUtils.FileDelete("path_to_file/codes_file");
            Assert.IsFalse(generator.CheckCode(",RXD~="));
            Assert.IsFalse(generator.CheckCode("9?:O)R"));
        }
    }
}
