using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;

namespace ImdbStock
{
    class ImdbScoreList
    {
        public const int DAYS = 5;

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
        private readonly Tuple<string, string, string, DateTime>[] companies = 
        {
            Tuple.Create<string, string, string, DateTime>("DreamWorks", "0040938", "DWA", new DateTime(2004, 10, 28)),
            Tuple.Create<string, string, string, DateTime>("Warner Bros", "0026840", "TWX", new DateTime(1992, 3, 19)),
            Tuple.Create<string, string, string, DateTime>("Universal Pictures", "0005073", "CMCSA", new DateTime(1988, 7, 7)),
            Tuple.Create<string, string, string, DateTime>("Walt Disney Pictures", "0008970", "DIS", new DateTime(1962, 1, 2)),
            Tuple.Create<string, string, string, DateTime>("Lionsgate", "0173285", "LGF", new DateTime(1998, 11, 17)),
            Tuple.Create<string, string, string, DateTime>("Twentieth Century Fox Film Corporation", "0000756", "FOX", new DateTime(1987, 12, 30))
        };

        private List<Company> companyList = new List<Company>();
        

        public ImdbScoreList()
        {
            DateTime now = DateTime.Now;
            LoadCompanies();
            foreach (Tuple<string, string, string, DateTime> c in companies)
            {
                string rawHTML = RequestImdbHTML(companies[1].Item2);
                List<string> movieIds = ParseHTML(rawHTML);
                foreach (string movieId in movieIds)
                {
                    Movie movie = RequestMovieXML(movieId);
                    if (movie.m_releaseDate.CompareTo(companies[1].Item4) <= 0)
                    {
                        break;
                    }
                    if (movie.m_releaseDate.CompareTo(now) < 0)
                    {
                        YahooFinance finance = new YahooFinance(companies[1].Item3);
                        //Console.WriteLine(movie.m_title + "\t" + movie.m_score);
                        finance.RequestFinanceQuotesCSV(movie.m_releaseDate, new TimeSpan(DAYS, 0, 0, 0));
                    }
                }

            }

            SaveCompanies();
        }

        private void LoadCompanies()
        {
            if (File.Exists("company.bin"))
            {
                using (Stream stream = File.Open("company.bin", FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    companyList = bin.Deserialize(stream) as List<Company>;
                }
            }            
        }

        private void SaveCompanies()
        {
            using (Stream stream = File.Open("company.bin", FileMode.Create))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, companyList);
            }
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
            string m_id = movieId;
            string m_title;
            DateTime m_releaseDate;
            float m_score;
            using (XmlReader xmlReader = XmlReader.Create(new StringReader(responseFromServer)))
            {
                DateTime releasedDate = new DateTime();
                xmlReader.ReadToFollowing("movie");
                xmlReader.MoveToAttribute("title");
                m_title = xmlReader.Value;
                xmlReader.MoveToAttribute("year");
                //cleaning year string
                string yearString = xmlReader.Value;
                if (yearString.Length > 4) {
                    yearString = yearString.Substring(0, 4);
                }
                int year = Int32.Parse(yearString);
                //make sure there is attribute 'released'
                if (xmlReader.GetAttribute("released") != "N/A") {
                    xmlReader.MoveToAttribute("released");
                    int month = 0;
                    int day = 0;
                    if (xmlReader.Value.Equals("N/A")) {
                        day = 0;
                        month = 0;

                    }
                    else {
                        string[] date = (xmlReader.Value).Split(' ');
                        day = Int32.Parse(date[0]);
                        month = months[date[1]];
                    }
                    releasedDate = new DateTime(year, month, day);
                    m_releaseDate = releasedDate;
                }
                //if no release date is set yet, use today's (days ago) date and the provided year
                else {
                    DateTime durationAgo = DateTime.Now.Subtract(new TimeSpan(DAYS, 0, 0, 0));
                    Console.WriteLine(durationAgo);
                    m_releaseDate = new DateTime(year, durationAgo.Month, durationAgo.Day);
                }
                
                xmlReader.MoveToAttribute("imdbRating");
                m_score = (xmlReader.Value.Equals("N/A")) ? 0f : Single.Parse(xmlReader.Value);
            }

            return new Movie(m_id, m_title, m_releaseDate, m_score);
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
