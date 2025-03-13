using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;

namespace KenHttpClientLibraryForInterfax
{
    // Reference: https://docs.uplandsoftware.com/interfax/documentation/api/rest/rest-api-reference/sending-faxes/send-fax/
    public class FaxSendOptions
    {
        // These are initialized to values that Ken wants to use for all faxes.
        // If you set a value to null, then the value in the interfax portal control panel will be used.

        // Ken never uses postponement
        public DateTime? PostponeTime { get; set; } = null;
        public int? RetriesToPerform { get; set; } = 4;
        public string Csid { get; set; } = "Ken's CSID";
        public string PageHeader { get; set; } = "To: {To} From: {From} Pages: {TotalPages}";
        public string Reference { get; set; }
        // Ken set Delivery Status via Web Service = Never (thus ReplyAddress is not needed).
        public string ReplyAddress { get; set; } = null;
        public string PageSize { get; set; } = "letter";
        public string FitToPage { get; set; } = "noscale";
        public string PageOrientation { get; set; } = "portrait";
        public string Resolution { get; set; } = "fine";
        public string Rendering { get; set; } = "grayscale";
        /// <summary>
        /// FaxSendOptions are used for both single destinations and multiple destinations.
        /// </summary>
        public FaxSendOptions(string subject)
        {
            Reference = subject;
        }
        public Dictionary<string, string> ToDictionary()
        {
            // urlencode pieces that may have special characters
            string urlEncodedCsid = HttpUtility.UrlEncode(Csid);
            string urlEncodedReference = HttpUtility.UrlEncode(Reference);
            //
            var options = new Dictionary<string, string>();
            if (PostponeTime.HasValue) options.Add("postponeTime", PostponeTime.Value.ToString("s") + "Z");
            if (RetriesToPerform != null) options.Add("retriesToPerform", RetriesToPerform.ToString());
            if (!string.IsNullOrEmpty(Csid)) options.Add("csid", urlEncodedCsid);
            if (!string.IsNullOrEmpty(PageHeader)) options.Add("pageHeader", PageHeader);
            if (!string.IsNullOrEmpty(Reference)) options.Add("reference", urlEncodedReference);
            if (!string.IsNullOrEmpty(ReplyAddress)) options.Add("replyAddress", ReplyAddress);
            if (!string.IsNullOrEmpty(PageSize)) options.Add("pageSize", PageSize);
            if (!string.IsNullOrEmpty(FitToPage)) options.Add("fitToPage", FitToPage);
            if (!string.IsNullOrEmpty(PageOrientation)) options.Add("pageOrientation", PageOrientation);
            if (!string.IsNullOrEmpty(Resolution)) options.Add("resolution", Resolution);
            if (!string.IsNullOrEmpty(Rendering)) options.Add("rendering", Rendering);
            return options;
        }
    }
}
