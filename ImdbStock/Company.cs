using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImdbStock
{
    class Company
    {
        private string c_name;
        private string c_id;
        private string c_ticker; 
        private DateTime c_ipo;
        private Dictionary<Movie, string> c_movies;

        public Company(string name, string id, string ticker, DateTime ipo)
        {
            this.c_name = name;
            this.c_id = id;
            this.c_ticker = ticker;
            this.c_ipo = ipo;
            this.c_movies = new Dictionary<Movie, string>();
        }

        private void _parseRaw(Movie m)
        {

        }
    }
}
