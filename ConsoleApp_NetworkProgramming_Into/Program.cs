using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

//HTTP 80  не защищенный, не нужну SSL сертификаты , не зашифрованный  
//HTTPS 443  защищенный , нужну SSL сертификаты б шифрует данные перед отправкой
namespace ConsoleApp_NetworkProgramming_Into
{
    class Root
    {
        public Person[] Porsens { get; set; }
    }

   public class Person
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string gender { get; set; }
        public string ip_address { get; set; }
    }

   public class PersonRepository
    {
        readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=PersonDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        public List<Person> GetPeople()
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "SELECT top (10) * FROM Persons";
            var people = db.Query<Person>(query);
            return people.ToList();
        }
        public List<Person> GetPeopleByName(string name)
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "SELECT * FROM Persons where first_name like @name+'%'";
            var people = db.Query<Person>(query, new {name});
            return people.ToList();
        }
    }

    class Program
    {
        public static PersonRepository repository;
        static void Main(string[] args)
        {
            //var server = new HttpListener();
            //server.Prefixes.Add("http://127.0.0.1:8080/");
            //server.Start();
            //Console.WriteLine("Server started...");
            //while (true)
            //{
            //    var context = server.GetContext();
            //    // context.Request;
            //    //  context.Response;
            //    Console.Write("Request from client: ");
            //    Console.WriteLine(context.Request.Url);

            //    using (var stream = new StreamWriter(context.Response.OutputStream))
            //    {
            //        Random random = new Random();

            //        stream.WriteLine("<h1 style = 'color:red'>Hello client! " + random.Next(0, 10000) + "</h1>");
            //    }

            //}
            //////////////////////////////////////////////////////////////////////////////////////
            ///
            repository = new PersonRepository();
            StartServer();
            Console.ReadKey();


        static async Task StartServer()
        {
            var server = new HttpListener();
            server.Prefixes.Add("http://127.0.0.1:8080/");
            server.Start();
            Console.WriteLine("Server started...");
            //int count = 0;
            while (true)
            {
                var context = await server.GetContextAsync();
                using var stream = new StreamWriter(context.Response.OutputStream);
                var request = context.Request;
                var response = context.Response;

                // context.Request;
                //  context.Response;
                Console.WriteLine($"{request.HttpMethod} - {request.RawUrl} - {DateTime.Now} Request from client: ");
                if (request.RawUrl == "/favicon.ico")
                {
                    

                        var bytes = File.ReadAllBytes("icon.ico");
                        stream.Write(bytes);
                    
                    continue;
                }
                if (request.RawUrl == "/GiveMeTime")
                {
                    
                        Random random = new Random();

                        await stream.WriteLineAsync("<h1 style = 'color:green'>Time " + DateTime.Now.ToShortTimeString() + "</h1>");
                    
                    continue;
                }
                if (request.RawUrl == "/wiki")
                {
                   
                        var html = await File.ReadAllTextAsync("index.html");
                        await stream.WriteLineAsync(html);
                    
                    continue;
                }

                    //if (request.RawUrl.StartsWith("/code"))
                    //{
                    //    StringBuilder stringBuilder = new StringBuilder(1000);
                    //    foreach (var item in request.QueryString)
                    //    {
                    //        stringBuilder.Append(item.ToString() + " " + request.QueryString.GetValues((string)item) + "\n");
                    //    }
                    //    await stream.WriteLineAsync(stringBuilder.ToString());
                    //}

                    if (request.RawUrl.Contains("/contact"))
                    {
                        if (request.RawUrl.Contains("Json"))
                        {

                            var json = await File.ReadAllTextAsync("data.json");
                            await stream.WriteLineAsync(json);

                            continue;
                        }
                        else if (request.RawUrl.Contains("DB"))
                        {
                            if (request.RawUrl.Contains("GetPeople"))
                            {
                                var json = repository.GetPeople();
                                StringBuilder stringBuilder = new StringBuilder(1000);
                                stringBuilder.Append("<h1 style = 'color:blue'>This is list of users: </h1>");
                                stringBuilder.Append("<ul>");
                                foreach (Person item in json)
                                {
                                    stringBuilder.Append($"<li>{item.id}) {item.first_name} {item.last_name} {item.gender}</li>");
                                }
                                stringBuilder.Append("</ul>");
                                await stream.WriteLineAsync(stringBuilder.ToString());

                                continue;
                            }
                           else if (request.QueryString.GetValues("search") != null)
                            {
                                var json = repository.GetPeopleByName(request.QueryString.GetValues("search").FirstOrDefault());
                                StringBuilder stringBuilder = new StringBuilder(1000);
                                stringBuilder.Append("<h1 style = 'color:blue'>This is list of users: </h1>");
                                stringBuilder.Append("<ul>");
                                foreach (Person item in json)
                                {
                                    stringBuilder.Append($"<li>{item.id}) {item.first_name} {item.last_name} {item.gender}</li>");
                                }
                                stringBuilder.Append("</ul>");
                                await stream.WriteLineAsync(stringBuilder.ToString());

                                continue;
                            }
                        }
                       else if (request.RawUrl.Contains("Tag"))
                        {

                            var json = await File.ReadAllTextAsync("data.json");
                            List<Person> root = JsonSerializer.Deserialize<List<Person>>(json);
                            Console.WriteLine(root.Count);
                            Console.WriteLine(root);
                            StringBuilder stringBuilder = new StringBuilder(1000);
                            stringBuilder.Append("<h1 style = 'color:blue'>This is list of users: </h1>");
                            stringBuilder.Append("<ul>");
                            foreach (Person item in root)
                            {
                                stringBuilder.Append($"<li>{item.id}) {item.first_name} {item.last_name} {item.gender}</li>");
                            }
                            stringBuilder.Append("</ul>");
                            await stream.WriteLineAsync(stringBuilder.ToString());

                            continue;
                        }
                    }

                //Console.WriteLine(context.Request.Url);

                    //Random random = new Random();

                    //await stream.WriteLineAsync("<h1 style = 'color:red'>Hello client! " + ++count + " = " + random.Next(0, 10000) + "</h1>");


                }
        }
    }
}
}
