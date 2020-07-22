using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ronadash_data.Models
{
    public class State
    {
        public int Id { get; set; }
        public string Province_State {get; set;}
        public int Active {get; set;}
        public int Deaths {get; set;}
        public int Confirmed {get; set;}
        public int Recovered {get; set;}
        public DateTime Last_Update {get; set;}
        // [JsonProperty("City")] public County[] Counties { get; set; }
        [JsonProperty("City")] public ICollection<County> Counties { get; set; }
        public State (string provState, int active, int deaths, int confs, int recovs, DateTime theDate)
        {
            this.Province_State = provState;
            this.Active = active;
            this.Deaths = deaths;
            this.Confirmed = confs;
            this.Recovered = recovs;
            this.Last_Update = theDate;
            this.Counties = null;
        }
        public State() { }
        public State (County county)
        {
            this.Province_State = county.Province_State;
            this.Active = county.Active;
            this.Deaths = county.Deaths;
            this.Confirmed = county.Confirmed;
            this.Recovered = county.Recovered;
            this.Last_Update = county.Last_Update;
            this.Counties = new County[]{};
        }
        public string ToPrettyString()
        {
            string fin = "";
            fin += "********************************************************************************" + Environment.NewLine;
            fin += $"\tProvince_State: {this.Province_State}{Environment.NewLine}";
            fin += $"\tActive: {this.Active}{Environment.NewLine}";
            fin += $"\tDeaths: {this.Deaths}{Environment.NewLine}";
            fin += $"\tConfirmed: {this.Confirmed}{Environment.NewLine}";
            fin += $"\tRecovered: {this.Recovered}{Environment.NewLine}";
            fin += $"\tLast_Update: {this.Last_Update}{Environment.NewLine}";
            fin += "********************************************************************************" + Environment.NewLine;
            return fin;
        }
        public List<Record> ToRecords(string countryName)
        {
            List<Record> recs = new List<Record>();
            if (this.Counties != null)
            {
                foreach(County county in this.Counties)
                    recs.Add(new Record(county));
            }
            else
            {
                Record rec = new Record();
                rec.Country = countryName;
                rec.Province_State = this.Province_State;
                rec.Last_Update = this.Last_Update;
                rec.Confirmed = this.Confirmed;
                rec.Deaths = this.Deaths;
                rec.Recovered = this.Recovered;
                rec.Active = this.Active;
                recs.Add(rec);
            }
            return recs;
        }
    }
}
