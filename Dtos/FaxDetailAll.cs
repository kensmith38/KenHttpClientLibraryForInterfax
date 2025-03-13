using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KenHttpClientLibraryForInterfax
{
    public class FaxDetailAll
    {
        public DateTime submitTime { get; set; }
        public string contact { get; set; }
        public string destinationFax { get; set; }
        public string replyEmail { get; set; }
        public string subject { get; set; }
        public int pagesSubmitted { get; set; }
        public string senderCSID { get; set; }
        public int attemptsToPerform { get; set; }
        public string pageSize { get; set; }
        public string pageResolution { get; set; }
        public string pageOrientation { get; set; }
        public string rendering { get; set; }
        public string pageHeader { get; set; }
        public string userId { get; set; }
        public int pagesSent { get; set; }
        public DateTime completionTime { get; set; }
        public string remoteCSID { get; set; }
        public int duration { get; set; }
        public int priority { get; set; }
        public decimal units { get; set; }
        public decimal costPerUnit { get; set; }
        public int attemptsMade { get; set; }
        public long id { get; set; }
        public string uri { get; set; }
        public int status { get; set; }
    }
}
