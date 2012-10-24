using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Tokens.Http;

namespace Client
{
    class Program
    {
        static Uri _address = new Uri("http://localhost:34980/api/greeting");
        
        static void Main(string[] args)
        {
            var client = new HttpClient { BaseAddress = _address };
            client.DefaultRequestHeaders.Authorization = 
                new BasicAuthenticationHeaderValue("alice", "alice");

            var response = client.GetAsync("").Result;

            Console.WriteLine("Hello {0}", response.Content.ReadAsStringAsync().Result);
            Console.ReadLine();
        }
    }
}
