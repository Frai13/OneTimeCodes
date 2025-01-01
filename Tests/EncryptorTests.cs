using NUnit.Framework;
using OneTimeCodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class EncryptorTests
    {
        private Rfc2898DeriveBytes RandomGenerator;
        private string defaultFileName = "examplefile";
        private string defaultContent = "This is an example of\ncontent";
        private byte[] saltDefault = Enumerable.Range(0, 16).Select(x => (byte)x).ToArray();
        private byte[] key;
        private byte[] iv;

        [SetUp]
        public void Setup()
        {
            File.Delete(defaultFileName);
            RandomGenerator = new Rfc2898DeriveBytes("", saltDefault, 1000, HashAlgorithmName.SHA256);

            key = RandomGenerator.GetBytes(32);
            iv = RandomGenerator.GetBytes(16);
        }

        [Test]
        public void TestEncryptFile()
        {
            File.WriteAllText(defaultFileName, defaultContent);
            Assert.IsTrue(Encryptor.Encrypt(key, iv, saltDefault, defaultFileName));

            string content = File.ReadAllText(defaultFileName);
            Assert.IsFalse(content == defaultContent);

            content = Encryptor.Decrypt(key, iv, defaultFileName);
            Assert.IsTrue(content == defaultContent);
        }

        [Test]
        public void TestEncryptFileDifferentPass()
        {
            File.WriteAllText(defaultFileName, defaultContent);
            Assert.IsTrue(Encryptor.Encrypt(key, iv, saltDefault, defaultFileName));

            RandomGenerator = new Rfc2898DeriveBytes("test", saltDefault, 1000, HashAlgorithmName.SHA256);
            key = RandomGenerator.GetBytes(32);
            iv = RandomGenerator.GetBytes(16);

            Assert.Throws<System.Security.Cryptography.CryptographicException>(() => { Encryptor.Decrypt(key, iv, defaultFileName); });
        }

        [Test]
        public void TestEncryptFileCreated()
        {
            File.WriteAllText(defaultFileName, defaultContent);
            File.WriteAllText($"{defaultFileName}_tmp", defaultContent);
            Assert.IsTrue(Encryptor.Encrypt(key, iv, saltDefault, defaultFileName));

            string content = Encryptor.Decrypt(key, iv, defaultFileName);
            Assert.IsTrue(content == defaultContent);
            File.Delete($"{defaultFileName}_tmp");
        }

        [Test]
        public void TestEncryptFileNotExists()
        {
            Assert.IsFalse(Encryptor.Encrypt(key, iv, saltDefault, ""));
            Assert.IsFalse(Encryptor.Encrypt(key, iv, saltDefault, "  "));
            Assert.IsFalse(Encryptor.Encrypt(key, iv, saltDefault, null));
            Assert.IsFalse(Encryptor.Encrypt(key, iv, saltDefault, "aaa"));

            Assert.IsTrue(Encryptor.Decrypt(key, iv, "") == "");
            Assert.IsTrue(Encryptor.Decrypt(key, iv, "  ") == "");
            Assert.IsTrue(Encryptor.Decrypt(key, iv, null) == "");
            Assert.IsTrue(Encryptor.Decrypt(key, iv, "aaa") == "");
        }

        [Test]
        public void TestEncryptFileNotValid()
        {
            // Path too long
            string path = string.Concat(Enumerable.Repeat("abcdefgh/", 100));
            Assert.IsFalse(Encryptor.Encrypt(key, iv, saltDefault, $"{path}example"));
            Assert.IsFalse(Encryptor.Encrypt(key, iv, saltDefault, $"afc?/example"));
            Assert.IsTrue(Encryptor.Decrypt(key, iv, $"{path}example") == "");
            Assert.IsTrue(Encryptor.Decrypt(key, iv, $"afc?/example") == "");

            using (FileStream fileStream = new FileStream(defaultFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                Assert.Throws<System.IO.IOException>(() => { Encryptor.Encrypt(key, iv, saltDefault, defaultFileName); });
                Assert.Throws<System.IO.IOException>(() => { Encryptor.Decrypt(key, iv, defaultFileName); });
            }
        }

        [Test]
        public void TestDecryptFileNotEncrypted()
        {
            File.WriteAllText(defaultFileName, defaultContent);

            Assert.Throws<System.Security.Cryptography.CryptographicException>(() => { Encryptor.Decrypt(key, iv, defaultFileName); });
        }
    }
}
