using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneTimeCodes
{
    internal class CodeContainer
    {
        public uint Position { get; set; }
        public string Code { get; set; }

        internal CodeContainer() { }
        internal CodeContainer(uint position, string code)
        {
            Position = position;
            Code = code;
        }
    }

    internal class CodeContainerEquality : IEqualityComparer<CodeContainer>
    {
        public CodeContainerEquality() { }

        public bool Equals(CodeContainer x, CodeContainer y)
        {
            return x.Code == y.Code;
        }

        public int GetHashCode(CodeContainer obj)
        {
            return obj.Code.GetHashCode();
        }
    }
}
