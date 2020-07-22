
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using CsvHelper;
using System.Globalization; //Needed for CultureInfo bit in CsvHelper
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ronadash_data.Models;

namespace ronadash_data
{
    class Program
    {
        /********************************************************************************/
        private static readonly int dbLoadInterval = 10000;
        private static readonly int millisPerHour = 60 * 60 * 1000;
        private static readonly string historicDataMountpoint = "/data";
        private static readonly HttpClient client = new HttpClient();
        private static readonly string root_endpoint = "https://corona.azure-api.net"; // Must not have trailing slash.
        /********************************************************************************/
        static async Task Main(string[] args)
        {
            DateTime start;
            List<Record> recs;
            bool alreadyLoaded, success;
            double time;
            string delim = "";
            for(int i=0; i<80; i++)
                delim += "*";

            /* Write delimiter to make new cycles easy to pick out in docker logs */
            BoxWrite(delim);
            BoxWrite(delim);
            BoxWrite(delim);
            BoxWrite("Begin:");
            
            while (true)
            {                
                start = DateTime.Now;
                recs = new List<Record>();
                time = 0;

                /* Check for any records. */
                alreadyLoaded = false;
                try
                {
                    using (RonaContext con = new RonaContext())
                    {
                        BoxWrite("Applying database migrations...");
                        await con.Database.MigrateAsync();
                        BoxWrite("Checking for existing records...");
                        alreadyLoaded = con.Records.Any();
                    }
                }
                catch (Exception ex)
                {
                    PrintException(ex, false, "Hit an exception when checking for existing data. Did you scaffold the database? (and are you trying the correct host?)");
                    return;
                }
                
                if (!alreadyLoaded)
                {
                    /* Looks like this is the first run. Grab old data and load the db. */
                    BoxWrite("No records found in database. Loading historical records from CSSE repo...");
                    recs = await GetHistoricalRecords();
                    if (recs == null)
                    {
                        BoxWrite($"Could not load historical data for initial database load.");
                    }
                    else if (recs.Count > 0)
                    {
                        BoxWrite("Loading records to database...");
                        success = LoadDbRecords(recs);
                        if (success)
                        {
                            BoxWrite($"Loaded {recs.Count():n0} historical records to the database.");
                            BoxWrite($"Current runtime: {(DateTime.Now - start).TotalMilliseconds / 1000:n3}s");
                        }
                        else
                            BoxWrite("There was a problem loading historical records to the database.");
                    }
                }

                /* Load today's data */
                BoxWrite("Loading list of ISO2 country codes...");
                List<string> countries = await GetCountries();
                BoxWrite($"Found {countries.Count:n0} country codes to process.");
                foreach(string country in countries)
                {
                    // BoxWrite($"Gathering today's data for {country}...");
                    // recs = await GetNewRecordsAsync("US");
                    recs = await GetNewRecordsAsync(country);
                    if (recs != null && recs.Count > 0)
                    {
                        BoxWrite($"Loading {recs.Count:n0} record{(recs.Count == 1 ? "" : "s")} for {country} to database...");
                        LoadDbRecords(recs);
                        // BoxWrite($"{country}'s data has been loaded.");
                    }
                    else if (recs == null)
                        BoxWrite($"!!! There was an error retrieving data using ISO2 code \"{country}\".");
                    else
                        BoxWrite($"Retrieved 0 records for {country}.");
                    await Task.Delay(1000); //Avoid sending too many calls too fast if my code borks.
                }
                
                /* Finish up */
                time = (DateTime.Now - start).TotalMilliseconds;
                BoxWrite($"Done. Runtime: {(time/1000.0):n3}s");
                BoxWrite($"Waiting {millisPerHour:n0}ms (1 hour) for the next data check.");
                await Task.Delay(millisPerHour);
            }
        }//Main
        /********************************************************************************/
        private static async Task<List<string>> GetCountries()
        {
            string endpoint = $"{root_endpoint}/country/";
            client.DefaultRequestHeaders.Accept.Clear();
            var task = client.GetStringAsync(endpoint);
            List<string> fin = new List<string>();
            List<CountryRecord> recs = JsonConvert.DeserializeObject<List<CountryRecord>>(await task);
            foreach(CountryRecord rec in recs)
                fin.Add(rec.ISO2);
            
            fin = fin
                .Where(x => !string.IsNullOrEmpty(x))
                .OrderBy(x => x)
                .ToList();
            
            return fin;
        }
        private static bool LoadDbRecords(List<Record> recs)
        {
            /************************************************************ 
             * Load Record objects to database.                         *
             * The weird new-context loop is to prevent timeouts that   *
             * can result from loading hundreds of thousands of records *
             * at once.                                                 *
             ************************************************************/
            bool success;
            List<string> existing;
            IEnumerable<DateTime> dates = from r in recs select r.Last_Update;
            DateTime minDate = dates.Min();
            DateTime maxDate = dates.Max();
            int startLen = 0, trimmedLen = 0, loadedSoFar = 0;
            RonaContext con;

            if (recs == null)
            {
                BoxWrite("Cannot load null list to database.");
                return false;
            }
            else if (recs.Count < 1)
            {
                BoxWrite("Cannot load empty list to database.");
                return false;
            }
            try
            {
                /* According to Some Guy On The Internet we should manually remove recs that conflict with db constraints. */
                con = new RonaContext();
                existing = (
                    from r in con.Records
                    where r.Last_Update >= minDate && r.Last_Update <= maxDate
                    select r.UniqueFields()
                ).ToList();
                startLen = recs.Count;
                recs = recs.Where(r => !existing.Contains(r.UniqueFields())).ToList();
                trimmedLen = recs.Count;

                if (trimmedLen < startLen)
                    BoxWrite($"After accounting for existing records, {trimmedLen:n0} records will be loaded.");

                /* Break into subsections of recs to load quicker. */
                var subLists = recs.Select((r, idx) => new {r, idx})
                    .GroupBy(a => a.idx/dbLoadInterval)
                    .Select((grp => grp.Select(g => g.r).ToList()))
                    .ToList();
                foreach(List<Record> subList in subLists)
                {
                    con = new RonaContext();
                    BoxWrite($"{loadedSoFar:n0} already loaded. Adding {subList.Count:n0} records to the database...");
                    con.Records.AddRange(subList);
                    con.SaveChanges();
                    loadedSoFar += subList.Count;
                }
                    success = true;                
            }
            catch (Exception ex)
            {
                success = false;
                PrintException(ex, true, "Encountered an exception while writing records to database.");
            }
            return success;
        }
        private static async Task<List<Record>> GetNewRecordsAsync(string countryCode)
        {
            string endpoint = $"{root_endpoint}/country/{countryCode}";
            client.DefaultRequestHeaders.Accept.Clear();
            var task = client.GetStringAsync(endpoint);
            List<Record> recs = new List<Record>();
            Country country = null;
            try
            {
                country = JsonConvert.DeserializeObject<Country>(await task);
            }
            catch (Exception ex)
            {
                BoxWrite($"Exception trying to deserialize data for {countryCode}:");
                Console.WriteLine($"\t\t{ex.Message}");
                return null;
            }

            if (country.States == null)
            {
                BoxWrite($"Got null states for {countryCode}");
                return null;
            }
            
            // BoxWrite($"country.States.Count for {countryCode} is {country.States.Count:n0}");
            recs.AddRange(country.ToRecords());
            // foreach(State state in country.States)
            //     recs.AddRange(state.ToRecords(country.Summary.Name));

            recs = (
                from r in recs
                group r by new { r.Country, r.Province_State, r.County, r.Last_Update } into grp
                select grp.First()
            ).ToList();

            return recs;
        }
        private static async Task<List<Record>> GetHistoricalRecords()
        {
            /* Import records from csv files */
            Regex regDateFile = new Regex(@"/\d{2}-\d{2}-\d{4}\.csv\s*$", RegexOptions.IgnoreCase);
            string[] files;
            List<Record> theList, recs;

            try
            {
                files = Directory
                    .GetFiles(historicDataMountpoint)
                    .Where(x => regDateFile.IsMatch(x))
                    .ToArray();
            }
            catch (Exception ex)
            {
                PrintException(ex, false, "Error retrieving list of files to search for historical data.");
                return null;
            }

            Array.Sort(files);

            theList = new List<Record>();
            
            BoxWrite($"Starting parsing tasks for {files.Count():n0} files...");
            // var tasks = files.Select(async file =>
            foreach(string file in files)
            {
                // recs = await Task.FromResult(ReadCSV(file));
                recs = ReadCSV(file);
                if (recs != null)
                    theList.AddRange(recs);
            }
            // });
            // BoxWrite($"Waiting for {tasks.Count()} tasks...");
            // await Task.WhenAll(tasks); //Seems this reruns all the tasks and gives us double records. I've gotta figure out async.
            BoxWrite("Tasks are complete.");

            BoxWrite($"Found {theList.Count():n0} records in {files.Length:n0} files.");

            theList = theList.Where(x => !string.IsNullOrEmpty(x.Province_State) && !string.IsNullOrEmpty(x.Country)).ToList();
            BoxWrite($"After removing entries without values for country or state, theList has {theList.Count:n0} records.");

            if (theList.Count < 1)
                return theList;
            
            /* Filter out fishy-looking records. I'm not sure how to determine which dupe to use, so remove all duplicates (on Country/Province/County/Date) */
            BoxWrite($"Filtering theList ({theList.Count:n0} records) to remove duplicate country+state+county+date entries.");
            theList = (
                from x in theList
                group x by new { x.Country, x.Province_State, x.County, x.Last_Update } into g
                where !string.IsNullOrEmpty(g.Key.Country)
                && !string.IsNullOrEmpty(g.Key.Province_State)
                && g.Count() == 1
                orderby g.Key.Country, g.Key.Province_State, g.Key.County
                select g.First()
                ).ToList();
            
            Console.WriteLine();
            BoxWrite($"theList has {theList.Count():n0} records after removing duplicates (by Country/Province_State/County/Date).");
            return theList;
        }
        private static List<Record> ReadCSV(string file)
        {
            List<Record> recs = new List<Record>();

            try
            {
                using (var sr = new StreamReader(file))
                using (var csv = new CsvReader(sr, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.PrepareHeaderForMatch = (string header, int index) => header.ToLower();
                    csv.Configuration.HeaderValidated = null;
                    csv.Configuration.MissingFieldFound = null;
                    csv.Configuration.RegisterClassMap<RecordMap>();
                    List<Record> records = csv.GetRecords<Record>().ToList();
                    foreach(Record rec in records)
                        rec.FixNames();
                    recs.AddRange(records);
                }
                return recs;
            }
            catch (Exception ex)
            {
                PrintException(ex, false, "Hit an exception in ReadCSV.");
                return null;
            }
        }
        /********************************************************************************/
        /* Misc Utils */
        private static void BoxWrite(string msg="")
        {
            if (!string.IsNullOrEmpty(msg))
                msg = $"{DateTime.Now.ToString("yyyy.MM.dd_HH:mm:ss.fff")} - {msg}".Trim();
            Console.WriteLine(msg);
        }
        public static string FixTheCase (string busted)
        {
            if (string.IsNullOrEmpty(busted))
                return busted;
            string fin = "";
            bool lastWasNotChar = true;
            char[] charArray = busted.Trim().ToLower().ToCharArray();
            foreach(char letter in charArray)
            {
                if (lastWasNotChar)
                    fin += letter.ToString().ToUpper();
                else
                    fin += letter.ToString();
                lastWasNotChar = !char.IsLetter(letter);
            }
            return fin;
        }
        private static void PrintException(Exception ex, bool writeInner, string msg)
        {
            string delim = "************************************************************";
            msg += $"\n\n{delim}\n{(writeInner ? ex.InnerException.Message : ex.Message)}\n{delim}\n\n";
            BoxWrite(msg);
        }
        /********************************************************************************/
    }//Program
}//namespace
