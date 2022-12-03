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

namespace HomeWork_NetworkIntro
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
        List<Person> GetAllByName(string name);
        Person GetByID(int id);
        List<Person> GetByLastName(string lastname);
        List<Person> GetMale();
        List<Person> GetFemale();
        List<Person> GetByGender(string gender);
        Person GetByEmail(string email);

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

        public Person GetByEmail(string email)
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "SELECT * FROM Persons where email = @email";
            Person person = (Person)db.QueryFirstOrDefault<Person>(query, new { email });
            return person;
        }

        public List<Person> GetByGender(string gender)
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "SELECT * FROM Persons where gender = @gender";
            var people = db.Query<Person>(query, new { gender });
            return people.ToList();
        }

        public  Person GetByID(int id)
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "SELECT * FROM Persons where id = @id";
            Person person = (Person)db.QueryFirstOrDefault<Person>(query, new { id });
            return person;
        }

        public List<Person> GetByLastName(string lastname)
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "SELECT * FROM Persons where last_name = @lastname";
            var people = db.Query<Person>(query, new { lastname });
            return people.ToList();
        }

        public List<Person> GetFemale()
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "SELECT * FROM Persons where gender = 'Female'";
            var people = db.Query<Person>(query);
            return people.ToList();
        }

        public List<Person> GetMale()
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "SELECT * FROM Persons where gender = 'Male'";
            var people = db.Query<Person>(query);
            return people.ToList();
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

                while (true)
                {
                    var context = await server.GetContextAsync();
                    using var stream = new StreamWriter(context.Response.OutputStream);
                    var request = context.Request;
                    var response = context.Response;

                    
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
                    if (request.RawUrl == "/GiveMeDay")
                    {

                        Random random = new Random();

                        await stream.WriteLineAsync("<h1 style = 'color: red'> " + DateTime.Today.DayOfWeek + "</h1>");

                        continue;
                    }
                    if (request.RawUrl == "/GiveMeYear")
                    {

                        Random random = new Random();

                        await stream.WriteLineAsync("<h1 style = 'color: pink'> " + DateTime.Today.Year + "</h1>");

                        continue;
                    }
                    if (request.RawUrl.Contains("/SUM"))
                    {
                       
                        if (request.QueryString.GetValues("A") != null && request.QueryString.GetValues("B") != null)
                        {
                     
                            var A = request.QueryString["A"];
                            var B = request.QueryString["B"];
                            int SUM = int.Parse(A) + int.Parse(B);
                            await stream.WriteLineAsync("<h1 style = 'color: black'> " + A + " + " + B + " = " + SUM + "</h1>");
                        }
                    }
                    if (request.RawUrl.Contains("/MUL"))
                    {
                       
                        if (request.QueryString.GetValues("A") != null && request.QueryString.GetValues("B") != null)
                        {
                     
                            var A = request.QueryString["A"];
                            var B = request.QueryString["B"];
                            int MUL = int.Parse(A) * int.Parse(B);
                            await stream.WriteLineAsync("<h1 style = 'color: black'> " + A + " * " + B + " = " + MUL + "</h1>");
                        }
                    }
                    if (request.RawUrl.Contains("/DIV"))
                    {
                       
                        if (request.QueryString.GetValues("A") != null && request.QueryString.GetValues("B") != null)
                        {
                     
                            var A = request.QueryString["A"];
                            var B = request.QueryString["B"];
                            double DIV = double.Parse(A) / double.Parse(B);
                            await stream.WriteLineAsync("<h1 style = 'color: black'> " + A + " / " + B + " = " + DIV + "</h1>");
                        }
                    }
                    if (request.RawUrl.Contains("/SUB"))
                    {
                       
                        if (request.QueryString.GetValues("A") != null && request.QueryString.GetValues("B") != null)
                        {
                     
                            var A = request.QueryString["A"];
                            var B = request.QueryString["B"];
                            int MIN = int.Parse(A) - int.Parse(B);
                            await stream.WriteLineAsync("<h1 style = 'color: black'> " + A + " - " + B + " = " + MIN + "</h1>");
                        }
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
                                var lastname = request.QueryString["lastname"];
                                var gender = request.QueryString["gender"];
                                var email = request.QueryString["email"];
                                if (!string.IsNullOrWhiteSpace(id))
                                {
                                    //Get by Id
                                    Console.WriteLine("id");
                                    var json = repository.GetByID(int.Parse(id));
                                    StringBuilder stringBuilder = new StringBuilder(1000);
                                    stringBuilder.Append("<h1 style='color:blue'>This is list of users : </h1>");
                                    stringBuilder.Append("<ul>");
                                    stringBuilder.Append($"<li>{json.id}) {json.first_name} {json.last_name} {json.gender}</li>");
                                    stringBuilder.Append("</ul>");
                                    await stream.WriteLineAsync(stringBuilder.ToString());
                                    continue;
                                }
                                else if (!string.IsNullOrWhiteSpace(lastname))
                                {
                                    Console.WriteLine("lastname");
                                    var json = repository.GetByLastName(lastname);
                                    StringBuilder stringBuilder = new StringBuilder(1000);
                                    stringBuilder.Append("<h1 style = 'color:blue'>This is list of users: </h1>");
                                    stringBuilder.Append("<ul>");
                                    foreach(Person item in json)
                                    stringBuilder.Append($"<li>{item.id}) {item.first_name} {item.last_name} {item.gender}</li>");
                                    stringBuilder.Append("</ul>");
                                    await stream.WriteLineAsync(stringBuilder.ToString());

                                    continue;
                                }
                                else if (request.RawUrl.Contains("GetMale"))
                                {
                                    var json = repository.GetMale();
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
                                else if (request.RawUrl.Contains("GetFemale"))
                                {
                                    var json = repository.GetFemale();
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
                                else if (!string.IsNullOrWhiteSpace(gender))
                                {
                                    var json = repository.GetByGender(gender);
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
                                else if (!string.IsNullOrWhiteSpace(email))
                                {
                                    var person = repository.GetByEmail(email);
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
