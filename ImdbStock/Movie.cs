using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImdbStock
{
    class Movie
    {
        public string m_id {  get; private set; }
        public string m_title {  get; private set; }
        public DateTime m_releaseDate {  get; private set; }
        public float m_score {  get; private set; }

        public Movie(string id, string title, DateTime releaseDate, float score)
        {
            this.m_id = id;
            this.m_title = title;
            this.m_releaseDate = releaseDate;
            this.m_score = score;
        }
    }
}
