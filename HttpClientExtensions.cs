using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;


namespace KenHttpClientLibraryForInterfax
{
    internal static class HttpClientExtensions
    {
        public static void Initialize(this HttpClient httpClient, string username, string password, string baseUrlForInterfax)
        {
            // Authorization
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(username + ":" + password);
            string authToken = Convert.ToBase64String(plainTextBytes);
            //httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(baseUrlForInterfax);
            //HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        }
        /// <summary>
        /// Get account balance (prepaid card balance)
        /// </summary>
        public static decimal GetPrePaidCardBalance(this HttpClient httpClient)
        {
            decimal balance = 0;
            var response = httpClient.GetAsync("/accounts/self/ppcards/balance").Result;
            if (response.IsSuccessStatusCode)
            {
                string strBalance = response.Content.ReadAsStringAsync().Result;
                balance = Convert.ToDecimal(strBalance);
            }
            else
            {
                throw new Exception(response.StatusCode.ToString());
            }
            return balance;
        }
        /// <summary>
        /// Gets FaxDetailAll via Search for a list of faxIDs.  
        /// Using "Search" allows us to get a lot of results without worrying about Interfax' system limitations (6 request / minute).
        /// </summary>
        public static List<FaxDetailAll> GetFaxDetailsViaSearch(this HttpClient httpClient, List<string> listFaxIDs)
        {
            List<FaxDetailAll> listFaxDetails = new List<FaxDetailAll>();
            string commaDelimitedList = String.Join(",", listFaxIDs);
            var response = httpClient.GetAsync($"/outbound/search?ids={commaDelimitedList}").Result;
            if (response.IsSuccessStatusCode)
            {
                string content = response.Content.ReadAsStringAsync().Result;
                listFaxDetails = JsonSerializer.Deserialize<List<FaxDetailAll>>(content);
            }
            else
            {
                throw new Exception(response.StatusCode.ToString());
            }
            // decode each item
            foreach (var details in listFaxDetails)
            {
                // urldecode pieces that we urlencoded when we sent request to interfax
                details.senderCSID = HttpUtility.UrlDecode(details.senderCSID);
                details.contact = HttpUtility.UrlDecode(details.contact);
                details.subject = HttpUtility.UrlDecode(details.subject);

            }
            return listFaxDetails;
        }
        /// <summary>
        /// Gets FaxDetailSummary for each child fax in a batch. (Note that the batchID is also the faxID of one of the "child" faxes!)
        /// </summary>
        public static List<FaxDetailSummary> GetBatchChildFaxes(this HttpClient httpClient, long batchID)
        {
            List<FaxDetailSummary> listSummaryChildFaxes = new List<FaxDetailSummary>();
            var response = httpClient.GetAsync($"/outbound/batches/{batchID.ToString()}/children").Result;
            if (response.IsSuccessStatusCode)
            {
                string content = response.Content.ReadAsStringAsync().Result;
                listSummaryChildFaxes = JsonSerializer.Deserialize<List<FaxDetailSummary>>(content);
            }
            else
            {
                throw new Exception(response.StatusCode.ToString());
            }
            return listSummaryChildFaxes;
        }
        /// <summary>
        /// Gets a fax image (TIFF file).  
        /// </summary>
        public static Image GetFaxImage(this HttpClient httpClient, long faxID)
        {
            Image faxImage = null;
            var response = httpClient.GetAsync($"/outbound/faxes/{faxID.ToString()}/image").Result;
            if (response.IsSuccessStatusCode)
            {
                MemoryStream memoryStream = new MemoryStream(response.Content.ReadAsByteArrayAsync().Result);
                faxImage = Image.FromStream(memoryStream);
            }
            else
            {
                throw new Exception(response.StatusCode.ToString());
            }
            return faxImage;
        }
        /// <summary>
        /// Adds extra options to a requestUri which is already partially built. 
        /// The partially built requestURi will look like one of these (for normal fax or a batch):
        /// "/outbound/faxes?faxNumber={faxNumber}&contact={contact}"
        /// "/outbound/batches?list={jsonArrayFaxDestinations}"
        /// </summary>
        public static string AddOptions(this string requestUri, FaxSendOptions options)
        {
            if (options == null) return requestUri;

            var optionsDictionary = options.ToDictionary();
            if (optionsDictionary.Keys.Count == 0) return requestUri;

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            foreach (var key in optionsDictionary.Keys)
                queryString[key] = optionsDictionary[key];
            return $"{requestUri}&{queryString}";
        }

        /// <summary>
        /// Send a fax (size < 8 MB) to a single destination.
        /// A long value (faxID) is returned identifying the fax resource.
        /// </summary>
        public static long SendFaxToSingleDestination(this HttpClient httpClient, 
            string docFilepath, string contact, string faxNumber, FaxSendOptions faxSendOptions)
        {
            string requestUri = "/outbound/faxes?";
            requestUri += $"faxNumber={faxNumber}";
            requestUri += $"&contact={HttpUtility.UrlEncode(contact)}";
            requestUri = requestUri.AddOptions(faxSendOptions);
            return SendFax(httpClient, requestUri, docFilepath);
        }
        /// <summary>
        /// Send a fax (size < 8 MB) to multiple destinations.
        /// A long value (faxID) is returned identifying the fax resource.
        /// </summary>
        public static long SendFaxToMultipleDestinations(this HttpClient httpClient,
            string docFilepath, List<FaxDestination> faxDestinations, FaxSendOptions faxSendOptions)
        {
            // Tricky! Make sure we urlEncode all contact names
            foreach (FaxDestination faxDestination in faxDestinations)
            {
                faxDestination.Contact = HttpUtility.UrlEncode(faxDestination.Contact);
            }
            string jsonArrayFaxDestinations = JsonSerializer.Serialize(faxDestinations);
            string requestUri = "/outbound/batches?";
            requestUri += $"list={jsonArrayFaxDestinations}";
            requestUri = requestUri.AddOptions(faxSendOptions);
            return SendFax(httpClient, requestUri, docFilepath);
        }
        /// <summary>
        /// Gets called by SendFaxToSingleDestination and SendFaxToMultipleDestinations.
        /// </summary>
        private static long SendFax(HttpClient httpClient, string requestUri, string docFilepath)
        {
            long faxID = 0;
            // Tricky! The user specifies a threshold which determines if a fax must be uploaded in chunks.
            //         A document whose size is greater than 8 MB must always be uploaded in chunks.
            //         All documents can be uploaded in chunks so why don't we always do that?
            //         Good question!  The "upload in chunks" process is more complicated and is a multi-step process.
            //         There is less chance for errors when the file is uploaded "all-at-once".
            //         Performance may be worse when uploading in chunks.
            // The threshold is specified in the Settings class.  It would be better if settings could be saved elsewhere.
            FileInfo fileInfo = new FileInfo(docFilepath);
            if (fileInfo.Length > Settings.ThresholdToUploadInChunks)
            {
                faxID = UploadInChunks.SendFaxInChunks(httpClient, requestUri, docFilepath);
            }
            else
            {
                faxID = SendFaxAllAtOnce(httpClient, requestUri, docFilepath);
            }
            return faxID;
        }
        /// <summary>
        /// Send a fax all at once (not in chunks).  This works for sending to single destination or multiple destinations.
        /// </summary>
        private static long SendFaxAllAtOnce(HttpClient httpClient, string requestUri, string docFilepath)
        {
            long faxID = 0;
            ByteArrayContent httpContent = new ByteArrayContent(File.ReadAllBytes(docFilepath));
            httpContent.Headers.ContentType = new MediaTypeHeaderValue(Utils.GetMimeType(docFilepath));
            var response = httpClient.PostAsync(requestUri, httpContent).Result;

            if (response.IsSuccessStatusCode)
            {
                // sample HTTP Location header:
                // Location: https://rest.interfax.net/outbound/faxes/854759652 or
                // Location: https://rest.interfax.net/outbound/batches/854759652
                faxID = Convert.ToInt64(response.Headers.Location.Segments.Last());
            }
            else
            {
                throw new Exception(response.StatusCode.ToString());
            }
            // returns fax resource id or batch resource id
            return faxID;
        }
    }
}
