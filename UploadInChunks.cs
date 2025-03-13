using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static System.Net.WebRequestMethods;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.IO;
using System.Net;

namespace KenHttpClientLibraryForInterfax
{
    // Design: Sending in chunks is a 4-step process.
    // 1. Create UploadSession
    // 2. Upload chunks in a loop until entire document has been uploaded
    // 3. Get the UploadSession (to get the session.Uri)
    // 4. Send the fax using session.Uri
    public static class UploadInChunks
    {
        public static long SendFaxInChunks(HttpClient httpClient, string requestUri, string docFilepath)
        {
            long faxID = 0;
            // 1. Create UploadSession
            UploadSessionOptions uploadSessionOptions = new UploadSessionOptions(docFilepath);
            string sessionID = CreateUploadSession(httpClient, uploadSessionOptions);
            // 2. Upload chunks
            using (var fileStream = System.IO.File.OpenRead(docFilepath))
            {
                UploadFileStreamToSession(httpClient, sessionID, fileStream);
            }
            // 4. Send the fax using session.Uri
            var content = new ByteArrayContent(new byte[0]);
            content.Headers.ContentLocation = new Uri($"https://rest.interfax.net/outbound/documents/{sessionID}");
            content.Headers.ContentLength = 0;
            var response = httpClient.PostAsync(requestUri, content).Result;

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

        /// <summary>
        /// Create a document upload session.
        /// </summary>
        /// <param name="options"></param>
        /// <returns>The id of the created session.</returns>
        public static string CreateUploadSession(HttpClient httpClient, UploadSessionOptions options)
        {
            string sessionID = null;
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            string requestUri = "/outbound/documents?";
            requestUri += $"size={options.Size}";
            requestUri += $"&name={options.Name}";
            requestUri += $"&disposition={options.Disposition}";
            requestUri += $"&sharing={options.Sharing}";
            //
            var response = httpClient.PostAsync(requestUri, null).Result;

            if (response.IsSuccessStatusCode)
            {
                // If successful, the HTTP Location header contains the fully-qualifed URI of the newly-created fax resource
                // e.g.Location: https://rest.interfax.net/outbound/documents/8547z59r65q2
                sessionID = response.Headers.Location.Segments.Last();
            }
            else
            {
                throw new Exception(response.StatusCode.ToString());
            }
            // returns sessionID
            return sessionID;
        }
        /// Uploads a FileStream to the given document upload session.
        /// </summary>
        /// <param name="sessionId">The id of an already existing upload session.</param>
        /// <param name="fileStream">The FileStream to upload.</param> 
        public static void UploadFileStreamToSession(HttpClient httpClient, string sessionID, FileStream fileStream)
        {
            string errmssg = null;
            int MaxChunkSize = Settings.ChunkSize;
            var buffer = new byte[MaxChunkSize];
            int len;
            while ((len = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                var data = new byte[len];
                Array.Copy(buffer, data, len);
                var response = UploadDocumentChunk(httpClient, sessionID, fileStream.Position - len, data);
                if (response.StatusCode == HttpStatusCode.Accepted) continue;
                if (response.StatusCode == HttpStatusCode.NoContent) continue;
                if (response.StatusCode == HttpStatusCode.OK) break;
                errmssg = $"Code = {(int)response.StatusCode}" +
                    $"\nMessage = {response.ReasonPhrase}" +
                    $"\nMoreInfo = {response.Content.ReadAsStringAsync().Result}";
                throw new Exception(errmssg);
            }
        }
        /// <summary>
        /// Uploads a chunk of data to the given document upload session.
        /// </summary>
        /// <param name="sessionId">The id of an alread existing upload session.</param>
        /// <param name="offset">The starting position of <paramref name="data"/> in the document.</param>
        /// <param name="data">The data to upload.s</param>
        /// <returns>An HttpResponseMessage</returns>
        public static HttpResponseMessage UploadDocumentChunk(HttpClient httpClient, string sessionID, long offset, byte[] data)
        {
            int MaxChunkSize = Settings.ChunkSize;
            if (data.Length > MaxChunkSize)
                throw new ArgumentOutOfRangeException(nameof(data), $"Cannot upload more than {MaxChunkSize} bytes at once.");

            ByteArrayContent httpContent = new ByteArrayContent(data);
            var range = new RangeHeaderValue(offset, offset + data.Length - 1);
            string requestUri = $"/outbound/documents/{sessionID}";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = httpContent;
            request.Headers.Range = range;
            return httpClient.SendAsync(request).Result;
        }
    }
}
