using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Tests"), InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace OneTimeCodes
{
    /// <summary>
    /// Enum that describes the different characteres that can be found on the generated code
    /// </summary>
    public enum CodeType
    {
        ALL = 0,
        ALPHABETIC_ONLY_LOWERCASE = 1,
        ALPHABETIC_ONLY_UPPERCASE = 2,
        ALPHABETIC_BOTH = 3,
        NUMERIC = 4,
        ALPHANUMERIC = 5
    }

    public class TokenGenerator
    {
        protected Rfc2898DeriveBytes RandomGenerator;
        protected uint Length;
        protected CodeType CodeType;

        internal List<byte> BytesSaved;
        internal static string CodesFileName = "_codes_";
        internal const string UserCodesFileName = "codes";

        private byte[] Salt;
        private byte[] EncryptionKey;
        private const int EncryptionKeyLength = 32;
        private byte[] EncryptionIv;
        private const int EncryptionIvLength = 16;

        /// <summary>
        /// TokenGenerator constructor
        /// </summary>
        /// <param name="password">Password used to generate random values</param>
        /// <param name="salt">Salt used to generate random values. Length must be 16.</param>
        /// <param name="iterations">Iterations used to generate random values</param>
        /// <param name="length">Length of the generated code</param>
        /// <param name="codeType"><see cref="CodeType"/> enum</param>
        /// <exception cref="System.ArgumentException">Thrown if salt length is not 16 and by Rfc2898DeriveBytes</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown by Rfc2898DeriveBytes</exception>
        public TokenGenerator(string password, byte[] salt, int iterations, uint length, CodeType codeType = CodeType.ALL)
        {
            if (salt.Length != 16) throw new System.ArgumentException("Salt length must be 16");

            this.Salt = salt;
            this.RandomGenerator = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            this.Length = length;
            this.CodeType = codeType;
            this.BytesSaved = new List<byte>();

            this.EncryptionKey = RandomGenerator.GetBytes(EncryptionKeyLength);
            this.EncryptionIv = RandomGenerator.GetBytes(EncryptionIvLength);
        }

        /// <summary>
        /// TokenGenerator destructor
        /// </summary>
        ~TokenGenerator()
        {
            this.RandomGenerator.Dispose();
        }

        /// <summary>
        /// Get a list of <paramref name="number"/> codes from <paramref name="start"/> index
        /// </summary>
        /// <param name="start">Start index</param>
        /// <param name="number">Number of codes to generate</param>
        /// <returns>List of codes generated</returns>
        public List<string> GetCodes(uint start, uint number)
        {
            uint endNumber = start + number;
            if (Length == 0) return new List<string>();

            uint endFirstByte = endNumber * Length;

            if (BytesSaved.Count >= endFirstByte)
            {
                return GetStoredCodes(start, number);
            }

            string regex = "";
            if (this.CodeType == CodeType.ALL) regex = @"[!-~]+";
            else if (this.CodeType == CodeType.ALPHABETIC_ONLY_LOWERCASE) regex = "[a-z]+";
            else if (this.CodeType == CodeType.ALPHABETIC_ONLY_UPPERCASE) regex = "[A-Z]+";
            else if (this.CodeType == CodeType.NUMERIC) regex = "[0-9]+";
            else if (this.CodeType == CodeType.ALPHABETIC_BOTH) regex = "[a-zA-Z]+";
            else if (this.CodeType == CodeType.ALPHANUMERIC) regex = "[0-9a-zA-Z]+";

            for (int i = BytesSaved.Count; i < endFirstByte + Length; i++)
            {
                byte value = 0;
                while (!Regex.IsMatch(((char)value).ToString(), regex))
                {
                    value = RandomGenerator.GetBytes(1).First();
                }
                BytesSaved.Add(value);
            }

            return GetStoredCodes(start, number);
        }

        internal virtual List<string> GetStoredCodes(uint start, uint number)
        {
            uint startByte = start * Length;

            List<string> result = new List<string>();
            for (int i = 0; i < number; i++)
            {
                result.Add(new string(BytesSaved.Skip((int)startByte).Take((int)Length).Select(b => (char)b).ToArray()));
                startByte += Length;
            }

            return result;
        }

        /// <summary>
        /// Create a file at <paramref name="path"/> containing <paramref name="number"/> codes from <paramref name="start"/> index
        /// </summary>
        /// <param name="start">Start index</param>
        /// <param name="number">Number of codes to generate</param>
        /// <returns>True if success</returns>
        /// <exception cref="System.IO.IOException">Thrown if file is in use</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown if file is encrypted with different parameters</exception>
        public bool GenerateCodes(uint start, uint number, string filePath = UserCodesFileName)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return false;

            List<string> codes = GetCodes(start, number);
            List<CodeContainer> codeList = new List<CodeContainer>();
            for (int i = 0; i < number; i++)
            {
                codeList.Add(new CodeContainer((uint)i + start, codes.ElementAt(i)));
            }

            return Serialize(codeList, filePath);
        }

        /// <summary>
        /// Check if <paramref name="code"/> is still available
        /// </summary>
        /// <param name="code">Code to be checked</param>
        /// <returns>True if available, False otherwise</returns>
        /// <exception cref="System.IO.IOException">Thrown if file is in use</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown if file is encrypted with different parameters</exception>
        public bool CheckCode(string code)
        {
            List<CodeContainer> codeList = Deserialize(CodesFileName);
            if (!codeList.Any()) return false;

            var codeContainer = codeList.Where(x => x.Code == code);
            if (!codeContainer.Any()) return false;

            if (GetCodes(codeContainer.First().Position, 1).First() != code) return false;
            
            return BlockCode(code);
        }

        /// <summary>
        /// Blocks <paramref name="code"/>
        /// </summary>
        /// <param name="code">Code to be blocked</param>
        /// <returns>True if success, False otherwise</returns>
        /// <exception cref="System.IO.IOException">Thrown if file is in use</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown if file is encrypted with different parameters</exception>
        public bool BlockCode(string code)
        {
            List<CodeContainer> codeList = Deserialize(CodesFileName);
            if (!codeList.Any()) return false;

            var codeContainer = codeList.Select((v, i) => new { v, i }).Where(x => x.v.Code == code);
            if (!codeContainer.Any()) return false;

            codeList.RemoveAt(codeContainer.First().i);

            return Serialize(codeList, CodesFileName);
        }

        /// <summary>
        /// Add new codes to already allowed ones from file <paramref name="filePath"/>
        /// </summary>
        /// <param name="filePath">Path to the file that contains new codes</param>
        /// <returns>True if success, False otherwise</returns>
        /// <exception cref="System.IO.IOException">Thrown if file is in use</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown if file is encrypted with different parameters</exception>
        public bool AddCodes(string filePath)
        {
            List<CodeContainer> codeListStored = Deserialize(CodesFileName);

            List<CodeContainer> codeListNew = Deserialize(filePath);
            if (!codeListNew.Any()) return false;

            List<CodeContainer> codeList = codeListStored.Union(codeListNew, new CodeContainerEquality()).ToList();

            return Serialize(codeList, CodesFileName);
        }

        internal bool Serialize(List<CodeContainer> codeList, string filePath)
        {
            string jsonString = JsonConvert.SerializeObject(codeList);

            if (File.Exists(filePath)) File.SetAttributes(filePath, File.GetAttributes(filePath) & ~FileAttributes.ReadOnly & ~FileAttributes.Hidden);
            File.WriteAllText(filePath, jsonString);
            Encryptor.Encrypt(EncryptionKey, EncryptionIv, Salt, filePath);
            File.SetAttributes(filePath, FileAttributes.ReadOnly | FileAttributes.Hidden);

            return true;
        }

        internal List<CodeContainer> Deserialize(string filePath)
        {
            List<CodeContainer> codeList = new List<CodeContainer>();
            if (!File.Exists(filePath)) return codeList;
            string jsonString = Encryptor.Decrypt(EncryptionKey, EncryptionIv, filePath);
            codeList = JsonConvert.DeserializeObject<List<CodeContainer>>(jsonString);
            return codeList;
        }
    }
}
