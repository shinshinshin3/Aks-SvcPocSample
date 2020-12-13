using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using GwClient02.Models;
using System.Text;

namespace GwClient02
{
    class Program
    {

        private static object lockobject = new object();
        private static HttpClient client = new HttpClient();
        private static RootObject accidentList;
        private static LocationList2 locationList;
        private static int dataCountLimit = 100000;
        private static int maxDegreeOfParallelism = 1;
        private static string accidentjsonstr = "";
        private static string locationsjsonstr = "";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            using (StreamReader sr = new StreamReader(@"./Data/accidentListSample.json"))
            {
                string accidentJsonString = sr.ReadToEnd();
                try
                {
                    accidentList = JsonConvert.DeserializeObject<RootObject>(accidentJsonString);
                    //Console.WriteLine(accidentLists[0].Accidents[0].OccurenceDate);
                }
                catch (Exception e)
                {
                    Console.WriteLine("This data could not be deserialize:");
                    Console.WriteLine(e.Message);

                }
            }

            using (StreamReader sr = new StreamReader(@"./Data/locationSample.json"))
            {
                string locationJsonString = sr.ReadToEnd();
                try
                {
                    locationList = JsonConvert.DeserializeObject<LocationList2>(locationJsonString);
                    //Console.WriteLine(accidentLists[0].Accidents[0].OccurenceDate);
                }
                catch (Exception e)
                {
                    Console.WriteLine("This data could not be deserialize:");
                    Console.WriteLine(e.Message);

                }
            }

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = maxDegreeOfParallelism;

            var rand = new Random();

            try
            {
                Parallel.For(0,
                    dataCountLimit,
                    options,
                    async (int i, ParallelLoopState loopState) =>
                    {
                        System.Threading.Thread.Sleep(300);

                        var k = rand.Next(0, 50000);

                        var now = DateTime.UtcNow;
                        accidentList.dateTime = now.ToString("yyyy-MM-ddThh:mm:ssZ");
                        accidentList.CountryCode = "EU" + k.ToString();

                        for (int n = 0; n <= accidentList.Accidents.Length - 1; n++)
                        {
                            accidentList.Accidents[n].OccurenceDate = now.ToString("yyyy-MM-ddThh:mm:ssZ");
                            accidentList.Accidents[n].Address = "address" + (k + n).ToString();
                            accidentList.Accidents[n].City = "city" + (k + n).ToString();
                            //accidentList.Accidents[n].VehicleId = accidentList.Accidents[n].VehicleId + (k + n).ToString();
                            //accidentList.Accidents[n].VehicleId += (k + n).ToString();
                            accidentList.Accidents[n].VehicleId = (k + n).ToString();
                            accidentList.Accidents[n].Message = "Message" + (k + n).ToString();
                        }

                        for (int n = 0; n <= locationList.Locations.Length -1; n++)
                        {
                            locationList.Locations[n].DateTime = now.ToString("yyyy-MM-ddThh:mm:ssZ");
                            locationList.Locations[n].Address = "address" + (k + n).ToString();
                            locationList.Locations[n].City = "city" + (k + n).ToString();
                            //locationList.Locations[n].VehicleId = locationList.Locations[n].VehicleId + (k + n).ToString();
                            //locationList.Locations[n].VehicleId += (k + n).ToString();

                            //string vehicleId = locationList.Locations[n].VehicleId;
                            //locationList.Locations[n].VehicleId = vehicleId + "h";
                            locationList.Locations[n].VehicleId = (k + n).ToString();

                        }

                        accidentjsonstr = JsonConvert.SerializeObject(accidentList);
                        locationsjsonstr = JsonConvert.SerializeObject(locationList);

                        //Console.WriteLine(accidentjsonstr);
                        //Console.WriteLine(locationsjsonstr);

                        var accidentContent = new StringContent(accidentjsonstr, Encoding.UTF8, "application/json");
                        var accidentResult = await client.PostAsync("http://localhost/api/AccidentList", accidentContent);
                        Console.WriteLine(@"AccientListApi: time: {0}, statusCode: {1}", accidentList.dateTime, (int)accidentResult.StatusCode);

                        var locationContent = new StringContent(locationsjsonstr, Encoding.UTF8, "application/json");
                        var locationResult = await client.PostAsync("http://localhost/api/LocationList", locationContent);
                        Console.WriteLine(@"LocationListApi: time: {0}, statusCode: {1}", locationList.Locations[0].DateTime, (int)locationResult.StatusCode);

                    }
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
