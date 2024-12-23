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

        public TokenGenerator(int seed, uint length, CodeType codeType = CodeType.ALL)
        {
            this.Random = new Random(seed);
            this.Length = length;
            this.CodeType = codeType;
            this.BytesSaved = new List<byte>();
        }

        public List<string> GetCodes(uint start, uint number)
        {
            uint endNumber = start + number;
            if (Length == 0) throw new ArgumentOutOfRangeException("Length", $"Length should be greater than 0.");

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

        public bool CheckCode(string code)
        {
            List<CodeContainer> codeList = Deserialize();
            if (codeList == null) return false;

            var codeContainer = codeList.Where(x => x.Code == code);
            if (!codeContainer.Any()) return false;

            if (GetCodes(codeContainer.First().Position, 1).First() != code) return false;
            
            return BlockCode(code);
        }

        public bool BlockCode(string code)
        {
            List<CodeContainer> codeList = Deserialize();
            if (codeList == null) return false;

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
