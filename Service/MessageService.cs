using Microsoft.DotNet.MSIdentity.Shared;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Group5.Service
{
    public class MessageService: IMessageService
    {
        HttpClient client;

        public MessageService(HttpClient client)
        {
            this.client = client;   
        }

        public async Task<string> CheckSpelling1(string inputText)
        {
            return inputText;
        }

        // kiem tra chinh ta cua input text
        public async Task<string> CheckSpelling(string inputText)
        {
            string apiUrl = "https://fastest-spell-checker-api.p.rapidapi.com/spell_check";
            string token = "d8dcb0fe74mshdb955ebf2c55569p1537d0jsndcd5a04751d3";
            string host = "fastest-spell-checker-api.p.rapidapi.com";

            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", token);
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", host);

            var requestContent = new
            {
                text = inputText
            };


            var jsonRequest = JsonConvert.SerializeObject(requestContent);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var suggestions = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
                string correctedtext = suggestions.corrected_text;
                // Convert the suggestions to a string and return it
                return correctedtext;
            }
            else
            {
                // Handle API request failure (e.g., log the error).
                return null;
            }
        }
        public class Issue
        {
            public string corrected { get; set; }
            public string word { get; set; }
        }
        public class ApiResponse
        {
            public string corrected_text { get; set; }
            public int input_length { get; set; }
            public Issue[] issues { get; set; }
            public string original_text { get; set; }
            public bool success { get; set; }
        }
    }
}
