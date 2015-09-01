using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ImdbStock
{
    class YahooFinance
    {
        private string companyId;


        public YahooFinance(string companyId)
        {
            this.companyId = companyId;
        }

        public string RequestFinanceQuotesCSV(DateTime time, TimeSpan duration)
        {
            //First parse the date time
            int startMonth = time.Month - 1;
            int startDay = time.Day;
            int startYear = time.Year;

            DateTime endDate = time.Add(duration);
            int endMonth = endDate.Month - 1;
            int endDay = endDate.Day;
            int endYear = endDate.Year;

            //Console.WriteLine(companyId + "\t" + startDay + "\t" + startMonth + "\t" + startYear);
            //Console.WriteLine(companyId + "\t" + endDay + "\t" + endMonth + "\t" + endYear);

            // Create a request for the URL. 
            WebRequest request = WebRequest.Create(
              "http://ichart.finance.yahoo.com/table.csv?s=" + companyId + "&a=" + startMonth + "&b=" + startDay + "&c=" + startYear + "&d=" + endMonth + "&e=" + endDay + "&f=" + endYear + "&g=d&ignore=.csv");
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            Console.WriteLine(responseFromServer);
            // Clean up the streams and the response.
            reader.Close();
            response.Close();
            //Return the raw content
            return responseFromServer;
        }
    }
}
