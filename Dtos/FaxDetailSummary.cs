using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KenHttpClientLibraryForInterfax
{
    public class FaxDetailSummary
    {
        public long id { get; set; }
        public int status { get; set; }
        public string userId { get; set; }
        public int pagesSent { get; set; }
        public DateTime completionTime { get; set; }
        public string remoteCSID { get; set; }
        public decimal duration { get; set; }
        public decimal cost { get; set; }
        public decimal units { get; set; }
        public decimal costPerUnit { get; set; }
        public int attemptsMade { get; set; }
        public string uri { get; set; }
    }
}
