using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace dotnet_gist_retriever
{
    class Program
    {

        static HttpClient client = new HttpClient();

        static void Main()
        {
            Console.WriteLine("Enter the ID of the Gist required.");

            string gistId = Console.ReadLine();

            while (!Regex.IsMatch(gistId, "^[a-z0-9]*$"))
            {
                Console.WriteLine("The Gist ID entered was invalid. Please try again...");

                gistId = Console.ReadLine();
            }

            Console.WriteLine(@"
Enter a name for the destination file (including extension)");

            string destFileName = Console.ReadLine();

            while (string.IsNullOrEmpty(destFileName))
            {
                Console.WriteLine("Destination file name cannot be blank...");

                destFileName = Console.ReadLine();
            }

            Console.WriteLine(@"
Enter the destination path (forward slashes only and no trailing slash)");

            string destFilePath = Console.ReadLine();

            while (!Regex.IsMatch(destFilePath, "^(.+)/([^/]+)$"))
            {
                Console.WriteLine("The path entered was invalid, please try again...");

                destFilePath = Console.ReadLine();
            }

            Console.WriteLine(MakeRequest(gistId, destFileName, destFilePath)
                              .GetAwaiter()
                              .GetResult());

            Console.WriteLine("Completed");
        }

        static async Task<string> MakeRequest(string gistId, string destFileName, string destFilePath)
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Test");

            HttpResponseMessage response = await client.GetAsync($"https://api.github.com/gists/{gistId}");

            JObject objectResponse = JsonConvert
                                     .DeserializeObject<JObject>(response
                                                                 .Content
                                                                 .ReadAsStringAsync()
                                                                 .Result);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success!");

                JObject files = objectResponse.Value<JObject>("files");

                string fileContent = files.First.First.Value<string>("content");

                System.IO.File.WriteAllText($"{ destFilePath }/{ destFileName }", fileContent);

                return "Successfully created file";
            }
            else
            {
                Console.WriteLine("Failure");
                return response.ToString();
            }
        }
    }
}
