using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KenHttpClientLibraryForInterfax
{
    // It would be better if settings could be saved elsewhere; perhaps in a database.
    public static class Settings
    {
        // Tricky! The user specifies a threshold which determines if a fax must be uploaded in chunks.
        //         A document whose size is greater than 8 MB must always be uploaded in chunks.
        //         All documents can be uploaded in chunks so why don't we always do that?
        //         Good question!  The "upload in chunks" process is more complicated and is a multi-step process.
        //         There is less chance for errors when the file is uploaded "all-at-once".
        //         Performance may be worse when uploading in chunks.

        public const int ThresholdToUploadInChunks = 7000000; // must always be less than 8 MB
        public const int ChunkSize = 50000;

        // These are for testing uploading in chunks without actually using a large file!
        //public const int ThresholdToUploadInChunks = 10000; // must always be less than 8 MB
        //public const int ChunkSize = 5000;

    }
}
