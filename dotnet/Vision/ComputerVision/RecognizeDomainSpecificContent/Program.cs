using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microsoft.Azure.CognitiveServices.Samples.ComputerVision.RecognizeDomainSpecificContent
{
    using Newtonsoft.Json.Linq;

    class Program
    {
        
        // Add your Azure Computer Vision subscription key and endpoint to your environment variables
        public static string subscriptionKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_SUBSCRIPTION_KEY");
        public static string endpoint = Environment.GetEnvironmentVariable("COMPUTER_VISION_ENDPOINT");
        
        static async Task Main(string[] args)
        {
            await RecognizeDomainSpecificContentSample.RunAsync(endpoint, subscriptionKey);

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }

    public class RecognizeDomainSpecificContentSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            Console.WriteLine("Recognize domain specific content (celebrities/landmarks) in images:");
            // See this repo's readme.md for info on how to get these images. Or, set the path to any appropriate image on your machine.
            string imageFilePath = @"Images\landmark.jpg"; 
            string remoteImageUrl = "https://github.com/Azure-Samples/cognitive-services-sample-data-files/raw/master/ComputerVision/Images/celebrities.jpg";

            await RecognizeDomainSpecificContentFromUrlAsync(remoteImageUrl, endpoint, key, "celebrities");
            await RecognizeDomainSpecificContentFromStreamAsync(imageFilePath, endpoint, key, "landmarks");
        }

        static async Task RecognizeDomainSpecificContentFromStreamAsync(string imageFilePath, string endpoint, string subscriptionKey, string specificDomain)
        {
            if (!File.Exists(imageFilePath))
            {
                Console.WriteLine("\nInvalid file path");
                return;
            }
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                string uri = $"{endpoint}/vision/v2.0/models/{specificDomain}/analyze";

                // Read the contents of the specified local image into a byte array.
                byte[] byteData = GetImageAsByteArray(imageFilePath);
                // Add the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses the "application /octet-stream" content type.
                    // The other content types you can use are "application/json" and "multipart/form-data".
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    // Asynchronously call the REST API method.
                    HttpResponseMessage response = await client.PostAsync(uri, content);
                    // Asynchronously get the JSON response.
                    string contentString = await response.Content.ReadAsStringAsync();
                    // Display the JSON response.
                    Console.WriteLine("\nResponse:\n\n{0}\n", JToken.Parse(contentString).ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }

        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            // Open a read-only file stream for the specified file.
            using (FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                // Read the file's contents into a byte array.
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }

        static async Task RecognizeDomainSpecificContentFromUrlAsync(string imageUrl, string endpoint, string subscriptionKey, string specificDomain)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remote image url:\n{0} \n", imageUrl);
                return;
            }
            try
            {
                HttpClient client = new HttpClient();
                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                string uri = $"{endpoint}/vision/v2.0/models/{specificDomain}/analyze";

                string requestBody = " {\"url\":\"" + imageUrl + "\"}";
                var content = new StringContent(requestBody);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // Post the request and display the result
                HttpResponseMessage response = await client.PostAsync(uri, content);
                string contentString = await response.Content.ReadAsStringAsync();
                Console.WriteLine("\nResponse:\n\n{0}\n", JToken.Parse(contentString).ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }
    }
}
