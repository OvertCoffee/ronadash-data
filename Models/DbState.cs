/********************************************************************************
 * Databse requires Counties be a list of foreign keys, data source returns a   *
 * list of County objects. This class bridges State objects from data source to *
 * a format our (SQL) database can work with.                                   *
 ********************************************************************************/

using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ronadash_data.Models
{
    public class DbState
    {
        public int Id { get; set; }
        public string Province_State {get; set;}
        public int Active {get; set;}
        public int Deaths {get; set;}
        public int Confirmed {get; set;}
        public int Recovered {get; set;}
        public DateTime Last_Update {get; set;}
        public List<int> Counties { get; set; }
        public DbState() { } 
        public DbState(State state)
        {
            /************************************************************
             * This only works right if state's counties have already   *
             * been loaded to the database (and assigned id values.)    *
             ************************************************************/
             
            this.Province_State = state.Province_State;
            this.Confirmed = state.Confirmed;
            this.Deaths = state.Deaths;
            this.Recovered = state.Recovered;
            this.Active = state.Active;
            this.Last_Update = state.Last_Update;
            this.Counties = new List<int>();
            if (state.Counties != null)
            {
                foreach (County guy in state.Counties)
                    this.Counties.Add(guy.CountyId);
            }
        } //DbState from State
        public DbState(string provname, DateTime theDate, int active, int deaths, int confs, int recovs, List<int> countyIDList)
        {
            this.Province_State = provname;
            this.Last_Update = theDate;
            this.Active = active;
            this.Deaths = deaths;
            this.Confirmed = confs;
            this.Recovered = recovs;
            this.Counties = countyIDList;
        }//DbState from aggregated Counties data
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
    }
}
