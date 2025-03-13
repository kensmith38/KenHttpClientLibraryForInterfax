using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
// Design: 
// 1. Ken used a lot of the code, techniques, and concepts in Interfax's official Library (InterFAX.NET) which is found on GitHub.
// 2. KenHttpClentLibraryForInterfax supports only "outbound" faxes (and "batches"); there is no support for "inbound" faxes.
// 3. KenHttpClentLibraryForInterfax does not support sending multiple documents in a single fax.
// 4. KenHttpClentLibraryForInterfax supports faxing any file up to 20 MB in size.
// 4a. Files larger than 8 MB are uploaded in chunks automatically (transparent to the user).
// 4b. The actual limit is specified in the Settings.cs file (ThresholdToUploadInChunks)
// 5. KenHttpClentLibraryForInterfax retrieves many more details than InterFAX.NET library which retrieves a summary of details.
// 6. Ken's production application stores faxIDs (and batchIDs) in a database as faxes (and batches) are sent.
// 6a. KenHttpClentLibraryForInterfax does not include extensive search options because Ken's database has all the faxIDs and batchIDs and
//     that information is sufficient to get details for any fax.

// Summary: KenHttpClentLibraryForInterfax is almost a subset of the InterFAX.NET library with the following differences:
// 1. KenHttpClentLibraryForInterfax can send a fax to multiple destinations (batch)! 
// 2. KenHttpClentLibraryForInterfax supports outbound faxes (but not inbound).
// 3. KenHttpClentLibraryForInterfax automatically uses "document upload" for large faxes (files larger than 8 Mb; actually ThresholdToUploadInChunks).
// 3a. It is very convenient that a file of any size can be faxed using the "SendFax*" method without worrying about its size.

// Caveats:
// 1. While this code appears to function correctly based on initial testing, it has not been thoroughly tested under all possible scenarios,
//    and therefore, there's a possibility of unexpected behavior or errors in production.

// Companion program: KenHttpClentTestAppForInterfax also exists on GitHub.  It is a C# .Net Framework app to test KenHttpClentLibraryForInterfax.

namespace KenHttpClientLibraryForInterfax
{
    // An HttpClient is intended to be instantiated once and reused throughout the life of an application.
    //      See: https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-net-http-httpclient
    public class FaxClient
    {
        public const string baseUrlForInterfax = "https://rest.interfax.net";
        internal HttpClient HttpClient { get; private set; }
        public FaxClient(string username, string password)
        {
            HttpClient = new HttpClient();
            HttpClient.Initialize(username, password, baseUrlForInterfax);
        }
        /// <summary>
        /// Get account balance (prepaid card balance)
        /// </summary>
        public decimal GetAcctBalance()
        {
            decimal balance = HttpClient.GetPrePaidCardBalance();
            return balance;
        }
        /// <summary>
        /// Gets FaxDetailAll via Search for a list of faxIDs.  
        /// Using "Search" allows us to get a lot of results without worrying about Interfax' system limitations (6 request / minute).
        /// </summary>
        public List<FaxDetailAll> GetFaxDetailsViaSearch(List<string> listFaxIDs)
        {
            List<FaxDetailAll> listCompletedFaxDetails = HttpClient.GetFaxDetailsViaSearch(listFaxIDs);
            return listCompletedFaxDetails;
        }
        /// <summary>
        /// Gets batch child faxes. (Note that the batchID is also the faxID of one of the "child" faxes!)
        /// </summary>
        public List<FaxDetailSummary> GetBatchChildFaxes(long faxBatchID)
        {
            List<FaxDetailSummary> listSummaryChildFaxes = HttpClient.GetBatchChildFaxes(faxBatchID);
            return listSummaryChildFaxes;
        }
        /// <summary>
        /// Gets a fax image (TIFF file).
        /// </summary>
        public Image GetFaxImage(long faxID)
        {
            Image image = HttpClient.GetFaxImage(faxID);
            return image;
        }
        /// <summary>
        /// Send a fax to a single destination.
        /// A long value (faxID) is returned identifying the fax resource.
        /// </summary>
        public long SendFaxToSingleDestination(string docFilepath, string contact, string faxNumber, FaxSendOptions faxSendOptions)
        {
            long faxID = HttpClient.SendFaxToSingleDestination(docFilepath, contact, faxNumber, faxSendOptions);
            return faxID;
        }
        /// <summary>
        /// Send a fax to multiple destinations.
        /// A long value (batchID) is returned identifying the batch resource (fax sent to multiple destinations).
        /// </summary>
        public long SendFaxToMultipleDestinations(string docFilepath, List<FaxDestination> faxDestinations, FaxSendOptions faxSendOptions)
        {
            long batchID = HttpClient.SendFaxToMultipleDestinations(docFilepath, faxDestinations, faxSendOptions);
            return batchID;
        }
    }
}
