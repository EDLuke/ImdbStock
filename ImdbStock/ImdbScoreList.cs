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
        struct Movie
        {
            public string id;
            public DateTime releaseDate;
            public float score;
        }

        Dictionary<string, int> months = new Dictionary<string, int>()
        {
            {"Jan", 1},
            {"Feb", 2},
            {"Mar", 3},
            {"Apr", 4},
            {"May", 5},
            {"Jun", 6},
            {"Jul", 7},
            {"Aug", 8},
            {"Sep", 9},
            {"Oct", 10},
            {"Nov", 11},
            {"Dec", 12}
        };


        static void Main(string[] args)
        {
            ImdbScoreList list = new ImdbScoreList();
        }

        private readonly string[] companyIds = { "0040938", "0026840", "0005073", "0008970", "0173285", "0000756"};
        private readonly Tuple<string, string, string>[] companies = 
        {
            Tuple.Create<string, string, string>("DreamWorks", "0040938", "DWA"),
            Tuple.Create<string, string, string>("Warner Bros", "0026840", "TWX"),
            Tuple.Create<string, string, string>("Universal Pictures", "0005073", "CMCSA"),
            Tuple.Create<string, string, string>("Walt Disney Pictures", "0008970", "DIS"),
            Tuple.Create<string, string, string>("Lionsgate", "0173285", "LGF"),
            Tuple.Create<string, string, string>("Twentieth Century Fox Film Corporation", "0000756", "FOX")
        };
        

        public ImdbScoreList()
        {
            string rawHTML = RequestImdbHTML(companyIds[1]);
            List<string> movieIds = ParseHTML(rawHTML);
            Movie movie = RequestMovieXML(movieIds[0]);
            YahooFinance finance = new YahooFinance();
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
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
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

        private Movie RequestMovieXML(string movieId)
        {
            // Create a request for the URL. 
            WebRequest request = WebRequest.Create(
              "http://www.omdbapi.com/?i=" + movieId + "&plot=full&r=xml");
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
            Movie movie = new Movie();
            movie.id = movieId;
            using (XmlReader xmlReader = XmlReader.Create(new StringReader(responseFromServer)))
            {
                DateTime releasedDate = new DateTime();
                xmlReader.ReadToFollowing("movie");
                xmlReader.MoveToAttribute("year");
                int year = Int32.Parse(xmlReader.Value);
                xmlReader.MoveToAttribute("released");
                int month = 0;
                int day = 0;
                if (xmlReader.Value.Equals("N/A"))
                {
                    day = 0;
                    month = 0;

                }
                else
                {
                    string[] date = (xmlReader.Value).Split(' ');
                    day = Int32.Parse(date[0]);
                    month = months[date[1]];
                }
                releasedDate = new DateTime(year, month, day);
                movie.releaseDate = releasedDate;
                xmlReader.MoveToAttribute("imdbRating");
                movie.score = (xmlReader.Value.Equals("N/A")) ? 0f : Single.Parse(xmlReader.Value);
            }

            return movie;
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
