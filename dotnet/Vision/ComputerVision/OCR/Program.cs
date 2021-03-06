using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microsoft.Azure.CognitiveServices.Samples.ComputerVision.OCR
{
    using Newtonsoft.Json.Linq;

    class Program
    {
        // Add your Azure Computer Vision subscription key and endpoint to your environment variables
        public static string subscriptionKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_SUBSCRIPTION_KEY");
        public static string endpoint = Environment.GetEnvironmentVariable("COMPUTER_VISION_ENDPOINT");
        
        static async Task Main(string[] args)
        {
            await OCRSample.RunAsync(endpoint, subscriptionKey);

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }

    public class OCRSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            Console.WriteLine("Performing OCR on the images:");
            // See this repo's readme.md for info on how to get these images. Or, set the path to any appropriate image on your machine.
            string imageFilePath = @"Images\printed_text.jpg";  
            string remoteImageUrl = "https://github.com/Azure-Samples/cognitive-services-sample-data-files/raw/master/ComputerVision/Images/printed_text.jpg";
            // OCR works poorly for non-printed text. Look at the ExtractText sample for using the read operation that can work for both handwritten and printed text
            await OCRFromStreamAsync(imageFilePath, endpoint, key);
            await OCRFromUrlAsync(remoteImageUrl, endpoint, key);
        }

        static async Task OCRFromStreamAsync(string imageFilePath, string endpoint, string subscriptionKey)
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

                // Request parameters. 
                // The language parameter doesn't specify a language, so the method detects it automatically.
                // The detectOrientation parameter is set to true, so the method detects and and corrects text orientation before detecting text.
                string requestParameters = "language=unk&detectOrientation=true";

                //Assemble the URI and content header for the REST API request
                string uri = $"{endpoint}/vision/v2.0/ocr?{requestParameters}";
                // Read the contents of the specified local image into a byte array.
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                // Add the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This particular example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json" (used in OCRFromUrlAsync) and "multipart/form-data".
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

        static async Task OCRFromUrlAsync(string imageUrl, string endpoint, string subscriptionKey)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remote image url:\n{0} \n", imageUrl);
                return;
            }
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                //Assemble the URI and content header for the REST API request
                string requestParameters = "language=unk&detectOrientation=true";
                string uri = $"{endpoint}/vision/v2.0/ocr?{requestParameters}";
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
