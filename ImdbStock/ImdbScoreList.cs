using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Xml;

namespace ImdbStock
{
    class ImdbScoreList
    {
        static void Main(string[] args)
        {
            ImdbScoreList list = new ImdbScoreList();
        }

        private readonly string[] companyIds = { "0040938", "0026840", "0005073", "0008970", "0173285", "0000756", "0005073" };
        
        public ImdbScoreList()
        {
            string rawHTML = RequestImdbHTML(companyIds[1]);
            List<string> movieIds = ParseHTML(rawHTML);
            
            

        }


        private string RequestImdbHTML(string companyId)
        {
            // Create a request for the URL. 
            WebRequest request = WebRequest.Create(
              "http://www.imdb.com/company/co" + companyId + "/");
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Clean up the streams and the response.
            reader.Close();
            response.Close();
            //Return the raw content
            return responseFromServer;
        }

        private string RequestMovieXML(string movieId)
        {
            // Create a request for the URL. 
            WebRequest request = WebRequest.Create(
              "http://www.omdbapi.com/?i=" + movieId + "&plot=full&r=xml");
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(responseFromServer)))
            {
            
            }

            return "";
        }

        private List<string> ParseHTML(string rawHTML)
        {
            List<string> movieIds = new List<string>();
            rawHTML = rawHTML.Substring(rawHTML.IndexOf("<ol style=\"margin-left: 50px\">"));
            rawHTML = rawHTML.Substring(0, rawHTML.LastIndexOf("<ol style=\"margin-left: 50px\">"));

            while (rawHTML.IndexOf("li>") > 0)
            {
                rawHTML = rawHTML.Substring(rawHTML.IndexOf("<a href=\"/title/") + ("<a href=\"/title/").Length);
                string movieId = rawHTML.Substring(rawHTML.IndexOf("<a href=\"/title/") + ("<a href=\"/title/").Length, 9);
                if (rawHTML.IndexOf("<a href=\"/title/") >= 0)
                {
                    movieIds.Add(movieId);
                }
                rawHTML = rawHTML.Substring(rawHTML.IndexOf("</li>") + ("</li>").Length);
            }


            return movieIds;
        }


    }
}
