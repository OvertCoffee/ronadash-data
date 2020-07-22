using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using CsvHelper.Configuration;

namespace ronadash_data.Models
{
    public sealed class RecordMap : ClassMap<Record>
    {
        public RecordMap()
        {
            Map(m => m.Id).Name("Id");
            Map(m => m.FIPS).Name("FIPS");
            Map(m => m.Country).Name("Country_Region");
            Map(m => m.Province_State).Name("Province_State");
            Map(m => m.County).Name("Admin2");
            Map(m => m.Last_Update).Name("Last_Update");
            Map(m => m.Lat).Name("Lat");
            Map(m => m.Long).Name("Long_");
            Map(m => m.Confirmed).Name("Confirmed");
            Map(m => m.Deaths).Name("Deaths");
            Map(m => m.Recovered).Name("Recovered");
            Map(m => m.Active).Name("Active");
            Map(m => m.Combined_Key).Name("Combined_Key");
            Map(m => m.Incidence_Rate).Name("Incidence_Rate");
            Map(m => m.Case_Fatality_Ratio).Name("Case-Fatality_Ratio");
        }
    }
    public class Record
    {
        public int Id { get; set; }
        public string FIPS { get; set; }
        [JsonProperty("Country_Region")]public string Country { get; set; }
        [JsonProperty("Province")]public string Province_State { get; set; }
        [JsonProperty("Admin2")] public string County { get; set; }
        public DateTime Last_Update { get; set; }
        public double? Lat { get; set; }
        public double? Long { get; set; }
        public int? Confirmed { get; set; }
        public int? Deaths { get; set; }
        public int? Recovered { get; set; }
        public int? Active { get; set; }
        public string Combined_Key { get; set; }
        public double? Incidence_Rate { get; set; }
        [JsonProperty("Case-Fatality_Ratio")]public double? Case_Fatality_Ratio { get; set; }
        public Record()
        {
            this.FIPS = null;
            this.Country = "";
            this.Province_State = "";
            this.County = "";
            this.Last_Update = DateTime.MinValue;
            this.Lat = null;
            this.Long = null;
            this.Confirmed = null;
            this.Deaths = null;
            this.Recovered = null;
            this.Active = null;
            this.Combined_Key = null;
            this.Incidence_Rate = null;
        }
        public Record(County guy)
        {
            double tmpDubs;
            this.FIPS = guy.FIPS;
            this.Country = string.IsNullOrEmpty(guy.Country_Region) ? "" : guy.Country_Region;
            this.Province_State = string.IsNullOrEmpty(guy.Province_State) ? "" : guy.Province_State;
            this.County = string.IsNullOrEmpty(guy.Name) ? "" : guy.Name;
            this.Last_Update = guy.Last_Update;
            this.Confirmed = guy.Confirmed;
            this.Deaths = guy.Deaths;
            this.Recovered = guy.Recovered;
            this.Active = guy.Active;
            this.Combined_Key = guy.Combined_Key;
            
            if (double.TryParse(guy.Lat, out tmpDubs))
                this.Lat = tmpDubs;
            else
                this.Lat = null;
            if (double.TryParse(guy.Long, out tmpDubs))
                this.Long = tmpDubs;
            else
                this.Long = null;
            if (double.TryParse(guy.Incidence_Rate, out tmpDubs))
                this.Incidence_Rate = tmpDubs;
            else
                this.Incidence_Rate = null;
        }
        public void FixNames()
        {
            this.Country = FixTheString(this.Country);
            this.Province_State = FixTheString(this.Province_State);
            this.County = FixTheString(this.County);
            
            /* Filler for missing "County" records. Looks like empty Admin2 is valid, but I don't like it. */
            if (string.IsNullOrEmpty(this.County) && !string.IsNullOrEmpty(Combined_Key) && !string.IsNullOrEmpty(this.FIPS))
            {
                string[] split = this.Combined_Key.Trim().Split(',');
                if (split.Length >= 1)
                    this.County = split[0];
            }
        }
        private string FixTheString(string guy)
        {
            if (string.IsNullOrEmpty(guy))
                return "";
            else
            {
                guy = guy.Trim();
                return Regex.Replace(guy, @"\s\s+", " ");
            }
        }
        public string ToPrettyString()
        {
            string fin = Environment.NewLine;
            fin += $"FIPS: {FIPS}" + Environment.NewLine;
            fin += $"County : {County }" + Environment.NewLine;
            fin += $"Province_State: {Province_State}" + Environment.NewLine;
            fin += $"Country: {Country}" + Environment.NewLine;
            fin += $"Last_Update: {Last_Update}" + Environment.NewLine;
            fin += $"Lat: {Lat}" + Environment.NewLine;
            fin += $"Long: {Long}" + Environment.NewLine;
            fin += $"Confirmed: {Confirmed}" + Environment.NewLine;
            fin += $"Deaths: {Deaths}" + Environment.NewLine;
            fin += $"Recovered: {Recovered}" + Environment.NewLine;
            fin += $"Active: {Active}" + Environment.NewLine;
            fin += $"Combined_Key: {Combined_Key}" + Environment.NewLine;
            fin += $"Incidence_Rate: {Incidence_Rate}" + Environment.NewLine;
            fin += $"Case_Fatality_Ratio: {Case_Fatality_Ratio}" + Environment.NewLine;
            return fin;
        }
        public string UniqueFields()
        {
            string fin = "";
            fin += this.Country + ":";
            fin += this.Province_State + ":";
            fin += this.County + ":";
            fin += this.Last_Update;
            return fin;
        }
    }
}