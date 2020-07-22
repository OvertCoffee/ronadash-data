using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ronadash_data.Models
{
    public class Country
    {
        public int Id { get; set; }
        public CountrySummary Summary { get; set; }
        [JsonProperty("State")]public ICollection<State> States { get; set; }
        // public List<Record> ToRecords()
        // {
        //     List<Record> recs = new List<Record>();
        //     foreach(State state in this.States)
        //     {
        //         foreach(County county in state.Counties)
        //         {
        //             var tmpRec = new Record(county);
        //             if (tmpRec != null)
        //                 recs.Add(tmpRec);
        //         }
        //     }
        //     return recs;
        // }

        public List<Record> ToRecords()
        {
            List<Record> recs = new List<Record>();
            Record rec;
            if (this.States == null || this.States.Count == 0)
            {
                rec = new Record();
                rec.Country = this.Summary.Name;
                rec.Province_State = "";
                rec.County = "";
                rec.Last_Update = this.Summary.Last_Update;
                rec.Lat = this.Summary.Latitude;
                rec.Long = this.Summary.Longitude;
                rec.Confirmed = this.Summary.Confirmed;
                rec.Deaths = this.Summary.Deaths;
                rec.Recovered = this.Summary.Recovered;
                rec.Active = this.Summary.Active;
                rec.Combined_Key = "";
                rec.Incidence_Rate = null;
                recs.Add(rec);
            }
            else
            {
                foreach(State state in this.States)
                    recs.AddRange(state.ToRecords(this.Summary.Name));
            }
            return recs;
        }//ToRecords
    }//Country
    public class CountrySummary
    {
        public int Id { get; set; }
        [JsonProperty("Country_Region")]public string Name { get; set; }
        public string Code { get; set; }
        public string Slug { get; set; }
        public DateTime Last_Update { get; set; }
        [JsonProperty("Lat")]public double Latitude { get; set; }
        [JsonProperty("Long_")]public double Longitude { get; set; }
        public int? Confirmed { get; set; }
        public int? Deaths { get; set; }
        public int? Recovered { get; set; }
        public int? NewConfirmed { get; set; }
        public int? NewDeaths { get; set; }
        public int? NewRecovered { get; set; }
        public int? Active { get; set; }
        public string Timeline { get; set; }
    }
}