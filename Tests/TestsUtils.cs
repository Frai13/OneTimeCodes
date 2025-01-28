using OneTimeCodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    internal static class TestsUtils
    {
        internal static void FileDelete(string path)
        {
            if (File.Exists(path)) File.SetAttributes(path, FileAttributes.Normal);
            File.Delete(path);
        }
    }
}
