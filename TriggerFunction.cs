using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MainFunction
{
    public static class TriggerFunction
    {
        [FunctionName("TriggerFunction")]
        public static async Task Run([BlobTrigger("%BlobContainer%/{name}", Connection = "StorageAccount")] Stream myBlob, string name, ILogger log, CancellationToken cancellationToken)
        {
            log.LogWarning($"C# Blob trigger function on: {name}  Size: {myBlob.Length} Bytes");
            var hostName = Environment.GetEnvironmentVariable("Host");
            var api = Environment.GetEnvironmentVariable("Api_route");

            try
            {
                using (var httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    using (var httpClient = new HttpClient(httpClientHandler))
                    {
                        httpClient.Timeout = TimeSpan.FromMinutes(10);

                        var defaultHeaders = httpClient.DefaultRequestHeaders;
                        if (defaultHeaders.Accept == null || !defaultHeaders.Accept.Any(m => m.MediaType == "application/json"))
                        {
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        }

                        log.LogWarning("Sending the request.");
                        var responseMessage = await httpClient.GetAsync($"{hostName}{api}{name}", cancellationToken);

                        log.LogWarning($"Response received from API: {responseMessage.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.StackTrace);
            }
        }
    }
}