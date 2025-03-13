using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KenHttpClientLibraryForInterfax
{
    public class FaxDestination
    {
        public string FaxNumber { get; set; }
        public string Contact { get; set; }
        public FaxDestination(string faxNumber, string contact)
        {
            FaxNumber = faxNumber;
            Contact = contact;
        }
    }
}
