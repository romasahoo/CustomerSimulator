using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CustomerSimulator
{
    class Program
    {
        private static readonly HttpClient _client = new HttpClient();
        private const string ServerUrl = "https://localhost:7018/api/Customers";
        private static readonly Random _random = new Random();

        static async Task Main(string[] args)
        {
            await SendParallelRequests();
        }

        private static async Task SendParallelRequests()
        {
            var tasks = new List<Task>();

            for (int i = 0; i < 10; i++) 
            {
                tasks.Add(Task.Run(async () =>
                {
                    var customers = GenerateRandomCustomers();
                    await SendPostRequest(customers);
                    await SendGetRequest();
                }));
            }

            await Task.WhenAll(tasks);
        }

        private static List<Customer> GenerateRandomCustomers()
        {
            var customers = new List<Customer>();
            for (int i = 0; i < 2; i++)
            {
                var customer = new Customer
                {
                    FirstName = GetRandomName(),
                    LastName = GetRandomName(),
                    Age = _random.Next(10, 91),
                    Id = Guid.NewGuid()
                };
                customers.Add(customer);
            }
            return customers;
        }

        private static async Task SendPostRequest(List<Customer> customers)
        {
            var json = JsonConvert.SerializeObject(customers);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(ServerUrl, content);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private static async Task SendGetRequest()
        {
            var response = await _client.GetAsync(ServerUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var customers = JsonConvert.DeserializeObject<List<Customer>>(json);
                Console.WriteLine($"Received {customers.Count} customers.");
            }
            else
            {
                Console.WriteLine($"Error fetching customers. Status code: {response.StatusCode}");
            }
        }

        private static string GetRandomName()
        {

            var firstNames = new List<string> {  "Leia", "Sadie", "Jose", "Sara", "Frank",
                "Dewey", "Tomas", "Joel", "Lukas", "Carlos" };
            var lastNames = new List<string> {"Liberty", "Ray", "Harrison", "Ronan", "Drew",
                "Powell", "Larsen", "Chan", "Anderson", "Lane"};

            
            var randomFirstNameIndex = _random.Next(firstNames.Count);
            var randomLastNameIndex = _random.Next(lastNames.Count);

            
            var randomName = $"{firstNames[randomFirstNameIndex]} {lastNames[randomLastNameIndex]}";

            return randomName;
        }

    }

    public class Customer
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }
}
