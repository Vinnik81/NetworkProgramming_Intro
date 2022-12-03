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

namespace ConsoleApp_Server
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

   public interface IPersonRepository
    {
        List<Person> GetAll();
        Person GetById(int id);
        List<Person> GetAllByName(string name);

    }
    public class PersonRepository: IPersonRepository
    {
        readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=PersonDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        public List<Person> GetAll()
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "SELECT * FROM Persons";
            var people = db.Query<Person>(query);
            return people.ToList();
        }
        public List<Person> GetAllByName(string name)
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "SELECT * FROM Persons where first_name like @name+'%'";
            var people = db.Query<Person>(query, new { name });
            return people.ToList();
        }

        public Person GetById(int id)
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "SELECT * FROM Person where id = @id";
            Person person = (Person)db.QueryFirstOrDefault<Person>(query, new { id });
            return person;
        }
    }
    class Program
    {
        public static IPersonRepository repository;
        static void Main(string[] args)
        {
            repository = new PersonRepository();
            StartServer();
            Console.Read();


            static async Task StartServer()
            {
                var server = new HttpListener();
                server.Prefixes.Add("http://127.0.0.1:8080/");
                server.Start();
                Console.WriteLine("Server started...");
                int count = 0;
                while (true)
                {
                    var context = await server.GetContextAsync();
                    using var stream = new StreamWriter(context.Response.OutputStream);
                    var request = context.Request;
                    var response = context.Response;

                    Console.WriteLine($"{request.HttpMethod} - {request.RawUrl} - {DateTime.Now}");
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
                                var id = request.QueryString["id"];
                                if (!string.IsNullOrWhiteSpace(id))
                                {
                                    Console.WriteLine("Get ID");
                                    var person = repository.GetById(int.Parse(id));

                                    StringBuilder stringBuilder = new StringBuilder(1000);
                                    stringBuilder.Append("<h1 style='color:blue'>This is list of users : </h1>");
                                    stringBuilder.Append("<ul>");
                                    stringBuilder.Append($"<li>{person.id}) {person.first_name} {person.last_name} {person.gender}</li>");


                                    stringBuilder.Append("</ul>");


                                    await stream.WriteLineAsync(stringBuilder.ToString());
                                    continue;
                                }
                                else
                                {
                                    var json = repository.GetAll();
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
                            else if (request.QueryString.GetValues("search") != null)
                            {
                                var json = repository.GetAllByName(request.QueryString.GetValues("search").FirstOrDefault());
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
                }
            }
        }
    }
}
