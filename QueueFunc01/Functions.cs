using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonLibrary.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QueueFunc01.Context;

namespace QueueFunc01
{
    public class Functions
    {
        private readonly DatabaseContext _context;
        private IConfiguration _configuration;
        private ILogger _logger;

        public Functions(DatabaseContext context,
            IConfiguration configuration, ILogger<Functions> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [FunctionName("Functions")]
        public async Task Run([QueueTrigger("%Queue_Name%", Connection = "Queue_ConnectionString")] string myQueueItem)
        {
            //Console.WriteLine(myQueueItem);

            /*
            // Base64url ⇒ UTF8
            int paddingNum = myQueueItem.Length % 4;
            if (paddingNum != 0)
            {
                paddingNum = 4 - paddingNum;
            }
            var queueItemUtf8 = Encoding.UTF8.GetString(Convert.FromBase64String(myQueueItem
                + new string('=', paddingNum)
                                .Replace('-', '+')                                //「-」⇒「+」
                                .Replace('_', '/')));                             //「_」)
            

            var m = new MemoryStream(Convert.FromBase64String(myQueueItem));
            var queueItemUtf8 = Encoding.UTF8.GetString(m.ToArray());

            Console.WriteLine(queueItemUtf8);
            */

            //var str = myQueueItem.TrimStart('"').Trim('\\').TrimEnd('"');
            var str = Regex.Unescape(myQueueItem).Trim();
            //str = str.Substring(1, str.Length - 2);

            Console.WriteLine(str);

            try
            {
                var locationList = JsonConvert.DeserializeObject<LocationList2>(str, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                foreach (var i in locationList.Locations)
                {
                    _context.location.Add(i);
                    _logger.LogDebug(String.Format("SQL Insert: {0}", JsonConvert.SerializeObject(i)), LogLevel.Debug);

                    Console.WriteLine(String.Format("SQL Insert: {0}", JsonConvert.SerializeObject(i)));
                }
                var sqlResult = await _context.SaveChangesAsync();
                Console.WriteLine(sqlResult.ToString());
;
                
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.StackTrace, LogLevel.Error);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }

        /*
        public static void Run([QueueTrigger("myqueue-items", Connection = "")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
        */
    }
}
