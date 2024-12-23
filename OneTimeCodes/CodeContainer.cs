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
}
