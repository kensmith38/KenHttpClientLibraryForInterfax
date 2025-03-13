using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KenHttpClientLibraryForInterfax
{
    public static class Utils
    {
        public static string GetMimeType(string docFilepath)
        {
            string mimeType = "unknown";
            FileInfo fileInfo = new FileInfo(docFilepath);
            string fileExt = fileInfo.Extension.ToLower();
            if (fileExt.Equals(".txt")) { mimeType = "text/plain"; }
            else if (fileExt.Equals(".pdf")) { mimeType = "application/pdf"; }
            else if (fileExt.Equals(".doc")) { mimeType = "application/msword"; }
            else if (fileExt.Equals(".docx")) { mimeType = "application/msword"; }
            return mimeType;
        }
    }
}
