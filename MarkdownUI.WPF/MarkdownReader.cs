using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownUI.WPF
{
    public static class MarkdownReader
    {

        public static string GetText(this Assembly assembly, string path)
        {
            var assemblyName = assembly.GetName();
            var justName = assemblyName.Name;
            string fullPath = justName + "." + path;
            Debug.WriteLine(fullPath);
            Stream stream = assembly.GetManifestResourceStream(fullPath);
            TextReader tr = new StreamReader(stream);

            return tr.ReadToEnd();
        }

    }
}
