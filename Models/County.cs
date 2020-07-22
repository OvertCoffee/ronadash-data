using System;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace ronadash_data.Models
{
    public class County
    {
        public int CountyId { get; set; }
        public string FIPS { get; set; }
        [JsonProperty("Admin2")] public string Name { get; set; }
        public string Province_State { get; set; }
        public string Country_Region { get; set; }
        public DateTime Last_Update { get; set; }
        // public string Last_Update { get; set; }
        public string Lat { get; set; }
        public string Long { get; set; }
        public int Confirmed { get; set; }
        public int Deaths { get; set; }
        public int Recovered { get; set; }
        public int Active { get; set; }
        public string Combined_Key { get; set; }
        public string Incidence_Rate { get; set; }
        public County () { }
        public County (string province_state, string name , DateTime last_update, int cases, int deaths)
        {
            this.Province_State = province_state;
            this.Name = name;
            this.Last_Update = last_update;
            this.Active = -1;
            this.Deaths = deaths;
            this.Recovered = -1;
            this.Confirmed = cases;
            this.FIPS = "";
            this.Country_Region = "";
            this.Lat = "";
            this.Long = "";
            this.Incidence_Rate = "";
        }
        public string ToPrettyString()
        {
            string fin = Environment.NewLine;
            fin += "************************************************************";
            fin += Environment.NewLine;
            fin += $"County: {this.Name}{Environment.NewLine}\t";
            fin += $"FIPS: {this.FIPS}{Environment.NewLine}\t";
            fin += $"Province_State: {this.Province_State}{Environment.NewLine}\t";
            fin += $"Country_Region: {this.Country_Region}{Environment.NewLine}\t";
            fin += $"Last_Update: {this.Last_Update}{Environment.NewLine}\t";
            fin += $"Lat: {this.Lat}{Environment.NewLine}\t";
            fin += $"Long: {this.Long}{Environment.NewLine}\t";
            fin += $"Active: {this.Active}{Environment.NewLine}";
            fin += $"Deaths: {this.Deaths}{Environment.NewLine}\t";
            fin += $"Confirmed: {this.Confirmed}{Environment.NewLine}\t";
            fin += $"Recovered: {this.Recovered}{Environment.NewLine}\t";
            fin += "************************************************************";
            return fin;
        }
    }
}
