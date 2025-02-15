﻿using NUnit.Framework;
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
            TestsUtils.FileDelete(TokenGenerator.CodesFileName);
            TestsUtils.FileDelete($"{TokenGenerator.CodesFileName}_tmp");
        }

        [Test]
        public void TestDeleteFile()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, TokenGenerator.CodesFileName, CodeType.ALL);
            generator.GenerateCodes(0, 3);
            File.Move(TokenGenerator.UserCodesFileName, TokenGenerator.CodesFileName);

            Assert.IsTrue(generator.CheckCode("9?:O)R"));
            TestsUtils.FileDelete(TokenGenerator.CodesFileName);
            Assert.IsFalse(generator.CheckCode(";oy@+w"));
            Assert.IsFalse(generator.CheckCode(",RXD~="));
        }

        [Test]
        public void TestFileEncrypted()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, TokenGenerator.CodesFileName, CodeType.ALL);
            generator.GenerateCodes(0, 3);
            File.Move(TokenGenerator.UserCodesFileName, TokenGenerator.CodesFileName);

            string content = File.ReadAllText(TokenGenerator.CodesFileName);
            Assert.IsFalse(content.Contains("9?:O)R"));

            generator = new TokenGenerator("test", saltDefault, 1000, 6, TokenGenerator.CodesFileName, CodeType.ALL);
            Assert.Throws<System.Security.Cryptography.CryptographicException>(() => { generator.CheckCode("9?:O)R"); });
        }

        [Test]
        public void TestFileDifferentPass()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, TokenGenerator.CodesFileName, CodeType.ALL);
            generator.GenerateCodes(0, 3);
            File.Copy(TokenGenerator.UserCodesFileName, $"{TokenGenerator.CodesFileName}_tmp");

            generator = new TokenGenerator("test", saltDefault, 1000, 6, TokenGenerator.CodesFileName, CodeType.ALL);
            generator.GenerateCodes(0, 3);
            TestsUtils.FileDelete(TokenGenerator.UserCodesFileName);
            File.Move($"{TokenGenerator.CodesFileName}_tmp", TokenGenerator.CodesFileName);
            Assert.Throws<System.Security.Cryptography.CryptographicException>(() => { generator.CheckCode("S#exOP"); });
        }

        [Test]
        public void TestFileCopy()
        {
            generator = new TokenGenerator("", saltDefault, 1000, 6, TokenGenerator.CodesFileName, CodeType.ALL);
            generator.GenerateCodes(0, 3);
 
            Assert.Throws<UnauthorizedAccessException>(() =>
            {
                File.WriteAllText(TokenGenerator.UserCodesFileName, "Overwrite text.");
            });
        }
    }
}
