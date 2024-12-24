using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
        protected Random Random;
        protected uint Length;
        protected CodeType CodeType;

        internal List<byte> BytesSaved;
        internal static string fileName = "codes";

        /// <summary>
        /// TokenGenerator constructor
        /// </summary>
        /// <param name="seed">Pseudorandom generator</param>
        /// <param name="length">Length of the generated code</param>
        /// <param name="codeType"><see cref="CodeType"/> enum</param>
        public TokenGenerator(int seed, uint length, CodeType codeType = CodeType.ALL)
        {
            this.Random = new Random(seed);
            this.Length = length;
            this.CodeType = codeType;
            this.BytesSaved = new List<byte>();
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

            for (int i = BytesSaved.Count; i < endFirstByte + Length; i++)
            {
                if (this.CodeType == CodeType.ALL) BytesSaved.Add((byte)Random.Next('!', 127));
                else if (this.CodeType == CodeType.ALPHABETIC_ONLY_LOWERCASE) BytesSaved.Add((byte)Random.Next('a', '{'));
                else if (this.CodeType == CodeType.ALPHABETIC_ONLY_UPPERCASE) BytesSaved.Add((byte)Random.Next('A', '['));
                else if (this.CodeType == CodeType.NUMERIC) BytesSaved.Add((byte)Random.Next('0', ':'));
                else if (this.CodeType == CodeType.ALPHABETIC_BOTH)
                {
                    byte value = 0;
                    while (!Regex.IsMatch(((char)value).ToString(), "[a-zA-Z]+"))
                    {
                        value = (byte)Random.Next('A', '{');
                    }
                    BytesSaved.Add(value);
                }
                else if(this.CodeType == CodeType.ALPHANUMERIC)
                {
                    byte value = 0;
                    while (!Regex.IsMatch(((char)value).ToString(), "[0-9a-zA-Z]+"))
                    {
                        value = (byte)Random.Next('0', '{');
                    }
                    BytesSaved.Add(value);
                }
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
        public bool GenerateCodes(uint start, uint number, string path = "")
        {
            path = path == "" ? "./" : path;
            if (!Directory.Exists(path))
            {
                return false;
            }
            path = !path.EndsWith("/") && !path.EndsWith("\\") ? path + "/" : path;

            List<string> codes = GetCodes(start, number);
            List<CodeContainer> codeList = new List<CodeContainer>();
            for (int i = 0; i < number; i++)
            {
                codeList.Add(new CodeContainer((uint)i + start, codes.ElementAt(i)));
            }

            Serialize(codeList, path);
            
            return true;
        }

        /// <summary>
        /// Check if <paramref name="code"/> is still available
        /// </summary>
        /// <param name="code">Code to be checked</param>
        /// <returns>True if available, False otherwise</returns>
        public bool CheckCode(string code)
        {
            List<CodeContainer> codeList = Deserialize();
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
        public bool BlockCode(string code)
        {
            List<CodeContainer> codeList = Deserialize();
            if (!codeList.Any()) return false;

            var codeContainer = codeList.Select((v, i) => new { v, i }).Where(x => x.v.Code == code);
            if (!codeContainer.Any()) return false;

            codeList.RemoveAt(codeContainer.First().i);

            Serialize(codeList, "");

            return true;
        }
        

        internal virtual void Serialize(List<CodeContainer> codeList, string path)
        {
            string jsonString = JsonConvert.SerializeObject(codeList);

            File.WriteAllText($"{path}{fileName}", jsonString);
        }

        internal List<CodeContainer> Deserialize()
        {
            List<CodeContainer> codeList = new List<CodeContainer>();
            if (!File.Exists(fileName)) return codeList;
            string jsonString = File.ReadAllText(fileName);
            codeList = JsonConvert.DeserializeObject<List<CodeContainer>>(jsonString);
            return codeList;
        }
    }
}
