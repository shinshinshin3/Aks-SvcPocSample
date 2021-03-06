﻿    using System;
    using System.IO;
    using Newtonsoft.Json;
    using GwClient01.Models;
    using System.Net.Http;
    using System.Collections.Generic;
    using System.Text;
    using Newtonsoft.Json.Converters;
    using System.Threading.Tasks;
    using System.Net.Http.Headers;

    namespace GwClient01
    {

        class Program
        {
            private static object lockobject = new object();
            private static HttpClient client = new HttpClient();
            private static List<RootObject> accidentLists;
            private static int dataCountLimit = 100000;
            private static int maxDegreeOfParallelism = 1;
            private static string jsonstr = "";

            static async Task Main(string[] args)
            {
                List<string> messageList = new List<string>()
                {
                    "Leave a window open",
                    "Leave a door open",
                    "Leave a refueling port open",
                    "with the bag left",
                    "with the key left",
                    "with the children left",
                    "Leave me alone",
                    "lost right arm",
                    "lost left arm",
                    "lost right leg",
                    "lost left leg",
                    "sheet belt broken",
                    "front door broken",
                    "right door broken",
                    "left door broken",
                    "car leave broken",
                    "a engine broken",
                    "engine oil used up",
                    "A Brake is stolen",
                    "A Handle is stolen"
                };

                using (StreamReader sr = new StreamReader(@"./Data/accidents_sample.json"))
                {
                    string accidentJsonString = sr.ReadToEnd();
                    try
                    {
                        accidentLists = JsonConvert.DeserializeObject<List<RootObject>>(accidentJsonString);
                        Console.WriteLine(accidentLists[0].Accidents[0].OccurenceDate);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("This data could not be deserialize:");
                        Console.WriteLine(e.Message);

                    }
                }

                /*
                var accept = new MediaTypeWithQualityHeaderValue("application/json");
                client.DefaultRequestHeaders.Accept.Add(accept);
                */

                var rand1 = new Random();
                var rand2 = new Random();

                ParallelOptions options = new ParallelOptions();
                options.MaxDegreeOfParallelism = maxDegreeOfParallelism;


                try
                {
                    Parallel.For(0,
                        dataCountLimit,
                        options,
                        async (int i, ParallelLoopState loopState) =>
                        {
                            System.Threading.Thread.Sleep(100);
                            var k = rand1.Next(0, 50000);
                            var n = rand2.Next(0, 19);
                            lock (lockobject)
                            {
                                accidentLists[k].dateTime = DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ssZ");
                                foreach (Accident_ a in accidentLists[k].Accidents)
                                {
                                    a.Message = messageList[rand2.Next(0, 19)];
                                    a.OccurenceDate = DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ssZ");
                                }
                                jsonstr = JsonConvert.SerializeObject(accidentLists[k]);
                        
                            }
                            //Console.WriteLine(jsonstr);

                            try
                            {
                                var content = new StringContent(jsonstr, Encoding.UTF8, "application/json");
                                var result = await client.PostAsync("http://localhost/api/AccidentList", content);
                                Console.WriteLine(@"time: {0}, statusCode: {1}", accidentLists[k].dateTime, (int)result.StatusCode);

                                //var response = result.Content.ReadAsStringAsync();
                                //Console.WriteLine(response);
                            }catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                            i++;
                        });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                /*
                for (int i = 0; i < N; i++)
                {
                    if (i > 0 && i % 25 == 0)
                    {
                        System.Threading.Thread.Sleep(100);
                    }

                    var k = rand1.Next(0, 50000);
                    var n = rand2.Next(0, 19);


                    accidentLists[k].dateTime = DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ssZ");
                    foreach (Accident_ a in accidentLists[k].Accidents)
                    {
                        a.Message = messageList[rand2.Next(0, 19)];
                        a.OccurenceDate = DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ssZ");
                    }

                    var json = JsonConvert.SerializeObject(accidentLists[k]);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    Console.WriteLine(json);
                    var result = await client.PostAsync("http://localhost/api/AccidentList", content);
                    var response = result.Content.ReadAsStringAsync();

                    Console.WriteLine(response.Result);
                    i++;
                }
                */
                }
        }
    }
