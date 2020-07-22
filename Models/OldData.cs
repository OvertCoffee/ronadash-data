using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ronadash_data.Models
{
    public class OldData
    {
        [JsonProperty("province")] public string Province { get; set; }
        [JsonProperty("county")] public string County { get; set; }
        [JsonProperty("timeline")] public Dictionary<string, Dictionary<DateTime, int>> TimeLine { get; set; }
        public string ToPrettyString()
        {
            string fin = "";
            fin += "********************************************************************************" + Environment.NewLine;
            fin += $"\tProvince: {Province}{Environment.NewLine}";
            fin += $"\tCounty: {County}{Environment.NewLine}";
            fin += $"\tTimeLine: {Environment.NewLine}";
            fin += $"\t\tCases: {TimeLine["cases"].Keys.Count}{Environment.NewLine}";
            fin += $"\t\tDeaths: {TimeLine["deaths"].Keys.Count}{Environment.NewLine}";
            fin += "********************************************************************************";
            return fin;
        }
        public List<County> GetCounties()
        {
            List<County> fin = new List<County>();
            County tmpCounty;
            int active, deaths;
            foreach(DateTime theDate in TimeLine["cases"].Keys)
            {
                active = TimeLine["cases"][theDate];
                deaths = TimeLine["deaths"].ContainsKey(theDate) ? TimeLine["deaths"][theDate] : -1;
                // tmpCounty = new County(this.Province, this.County, theDate, active, deaths);
                // fin.Add(tmpCounty);
                
                // // tmpCounty = new County();
                // // tmpCounty.Province_State = this.Province;
                // // tmpCounty.Name = this.County;
                // // tmpCounty.Active = active;
                // // tmpCounty.Deaths = deaths;

                fin.Add(new County(Program.FixTheCase(this.Province), Program.FixTheCase(this.County), theDate, active, deaths));
            }
            /* Pretty sure this'll never happen, but handle deaths with no corresponding active entry */
            foreach(DateTime theDate in TimeLine["deaths"].Keys)
            {
                /* Filter out recs we've already handled. */
                if (fin.Where(x => x.Last_Update.Date == theDate.Date).Count() > 0)
                    continue;
                active = -1; //If this had a matching active entry it would've hit that continue.
                deaths = TimeLine["deaths"][theDate];
                tmpCounty = new County(this.Province, this.County, theDate, active, deaths);
                
                // tmpCounty = new County();
                // tmpCounty.Province_State = this.Province;
                // tmpCounty.Name = this.County;
                // tmpCounty.Active = active;
                // tmpCounty.Deaths = deaths;

                fin.Add(tmpCounty);
            }
            return fin;
        }
    }
}