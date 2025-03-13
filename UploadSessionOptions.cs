using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KenHttpClientLibraryForInterfax
{
    public class UploadSessionOptions
    {
        /// <summary>
        /// Size of the document to be uploaded (in bytes)
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// The document file name, which can subsequently be queried. 
        /// The filename must end with an extension defining the file type, e.g. dailyrates.docx or newsletter.pdf, 
        /// and the file type must be in the list of supported file types.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Sets the retention policy of the uploaded document.
        ///   singleUse = can be used once
        ///   multiUse = deleted 60 minutes after the last usage
        ///   permanent = remains available until removed.
        /// </summary>
        public string Disposition { get; set; } = "singleUse";

        /// <summary>
        /// The sharing policy of the uploaded document. (private or shared)
        /// </summary>
        public string Sharing { get; set; } = "private";
        /// <summary>
        /// Create appropriate options for a single file
        /// </summary>
        /// <param name="docFilepath"></param>
        public UploadSessionOptions(string docFilepath)
        {
            FileInfo fileInfo = new FileInfo(docFilepath);
            Size = fileInfo.Length;
            Name = fileInfo.Name;
        }
    }
}
