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
    //class Root
    //{
    //    public Person[] Porsens { get; set; }
    //}

    public class Person: ICloneable 
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string gender { get; set; }
        public string ip_address { get; set; }

        public override string? ToString()
        {
            base.ToString();
           return ($"{id}) {first_name}, {last_name}, {email}, {gender}, {ip_address}");
        }


       public object Clone()
        {
            return new Person
            {
                id = id,
                first_name = first_name,
                last_name = last_name,
                email = email,
                gender = gender,
                ip_address = ip_address
            };
        }
    }

   public interface IPersonRepository
    {
        List<Person> GetAll();
        Person GetById(int id);
        List<Person> GetByIdFromTo(int from, int to);
        List<Person> GetAllByName(string name);
        void Create(Person person);
        void Create(params Person[] person);
        void Update (Person person);
        void Update (params Person[] person);
        void Delete (Person person);
        int GetCount ();

    }
    public class PersonRepository: IPersonRepository
    {
        readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=PersonDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public void Create(Person person)
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "insert into Persons (id, first_name, last_name, email, gender, ip_address) values (@id, @first_name, @last_name, @email, @gender, @ip_address)";

            db.Execute(query, person);
            
            //var parametr = new DynamicParameters();
            //parametr.Add("@id", person.id, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            //parametr.Add("@first_name", person.first_name, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            //parametr.Add("@last_name", person.last_name, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            //parametr.Add("@email", person.email, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            //parametr.Add("@gender", person.gender, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            //parametr.Add("@ip_address", person.ip_address, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            //db.Execute(query, parametr);
        }

        public void Create(params Person[] person)
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "insert into Persons (id, first_name, last_name, email, gender, ip_address) values (@id, @first_name, @last_name, @email, @gender, @ip_address)";

            db.Execute(query, person);
        }

        public void Delete(Person person)
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "delete from Persons where id = @id ";

            db.Execute(query, person);
        }

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
            var query = "SELECT * FROM Persons where id = @id";
            Person person = (Person)db.QueryFirstOrDefault<Person>(query, new { id });
            return person;
        }

        public List<Person> GetByIdFromTo(int from, int to)
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "SELECT * FROM Persons where id >= @from and id <= @to";
           var people = db.Query<Person>(query, new { from, to });
            return people.ToList();
        }

        public int GetCount()
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "SELECT COUNT(*) FROM Persons";
            var count = db.ExecuteScalar<int>(query);
            return count;
        }

        public void Update(Person person)
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "update Persons set first_name = @first_name, last_name = @last_name, email = @email, gender = @gender, ip_address = @ip_address where id = @id";

            db.Execute(query, person);
        }
        public void Update(params Person[] person)
        {
            using var db = new SqlConnection(connectionString);
            db.Open();
            var query = "update Persons set first_name = @first_name, last_name = @last_name, email = @email, gender = @gender, ip_address = @ip_address where id = @id";

            db.Execute(query, person);
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

                    StringBuilder builder = new StringBuilder(1000);

                    builder.Append(@"<p style='color:green'>/GiveMeTime   </p>");
                    builder.Append(@"<p style='color:green'>/wiki   </p>");
                    builder.Append(@"<p style='color:green'>/contact   </p>");
                    builder.Append(@"<p style='color:green'>--/Json   </p>");
                    builder.Append(@"<p style='color:green'>--/Db   </p>");
                    builder.Append(@"<p style='color:green'>----/GetPeople   </p>");
                    builder.Append(@"<p style='color:green'  >------/Create ?id,name,surname,email,gender,ip    </p>");
                    builder.Append(@"<p style='color:green'  >------/Creates ?id,name,surname,email,gender,ip    </p>");
                    builder.Append(@"<p style='color:green'  >------/Update ?id,name,surname,email,gender,ip    </p>");
                    builder.Append(@"<p style='color:green'  >------/Delete  ?id    </p>");
                    builder.Append(@"<p style='color:red'  >------/Count      </p>");
                    builder.Append(@"<p style='color:green'>------/   </p>");
                    builder.Append(@"<p style='color:green'>------/   </p>");
                    builder.Append(@"<p style='color:green'>------/   </p>");
                    builder.Append(@"<p style='color:green'>------/id   </p>");
                    builder.Append(@"<p style='color:green'>------/from and /to   </p>");
                    builder.Append(@"<p style='color:green'>----/search   </p>");
                    builder.Append(@"<p style='color:green'>--/Tag   </p>");
                    await stream.WriteLineAsync(builder.ToString());

                    Console.WriteLine($"{request.HttpMethod} - {request.RawUrl} - {DateTime.Now}");
                    if (request.RawUrl == "/favicon.ico")
                    {


                        var bytes = File.ReadAllBytes("icon.ico");
                        stream.Write(bytes);

                        continue;
                    }
                    if (request.HttpMethod == "POST" && request.RawUrl == "/GiveMeTime")
                    {

                        Random random = new Random();

                        await stream.WriteLineAsync("<h1 style = 'color:red'>Time " + DateTime.Now.ToShortTimeString() + "</h1>");

                        continue;
                    }
                    if (request.HttpMethod == "GET" && request.RawUrl == "/GiveMeTime")
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
                                var from = request.QueryString["from"];
                                var to = request.QueryString["to"];
                                if (request.RawUrl.Contains("Count"))
                                {
                                   var count2 = repository.GetCount();
                                    StringBuilder stringBuilder = new StringBuilder(1000);
                                    stringBuilder.Append("<h1 style='color:green'>Count persons: "+ count2 +" </h1>");
                                    await stream.WriteLineAsync(stringBuilder.ToString());
                                    continue;
                                }
                                else if (request.RawUrl.Contains("Delete"))
                                {
                                    Person person = new Person();
                                    person.id = int.Parse(request.QueryString["id"]);
                                    
                                 
                                    repository.Delete(person);
                                    StringBuilder stringBuilder = new StringBuilder(1000);
                                    stringBuilder.Append("<h1 style='color:blue'>This is person delete: </h1>");
                                    stringBuilder.Append("<h1 style='color:blue'>"+ person.ToString() + "</h1>");
                                    await stream.WriteLineAsync(stringBuilder.ToString());
                                    continue;
                                }
                                else if (request.RawUrl.Contains("Update"))
                                {
                                    Person person = new Person();
                                    person.id = int.Parse(request.QueryString["id"]);
                                    person.first_name = request.QueryString["name"];
                                    person.last_name = request.QueryString["surname"];
                                    person.email = request.QueryString["email"];
                                    person.gender = request.QueryString["gender"];
                                    person.ip_address = request.QueryString["ip"];

                                 
                                    repository.Update(person);
                                    StringBuilder stringBuilder = new StringBuilder(1000);
                                    stringBuilder.Append("<h1 style='color:blue'>This is person update: </h1>");
                                    stringBuilder.Append("<h1 style='color:blue'>"+ person.ToString() + "</h1>");
                                    await stream.WriteLineAsync(stringBuilder.ToString());
                                    continue;
                                }
                                else if (request.RawUrl.Contains("Create"))
                                {
                                    Person person = new Person();
                                    person.id = int.Parse(request.QueryString["id"]);
                                    person.first_name = request.QueryString["name"];
                                    person.last_name = request.QueryString["surname"];
                                    person.email = request.QueryString["email"];
                                    person.gender = request.QueryString["gender"];
                                    person.ip_address = request.QueryString["ip"];

                                    List<Person> people = new List<Person>();
                                    for (int i = 0; i < 10; i++)
                                    {
                                        person.id = 1002 + i;
                                        people.Add((Person)person.Clone());
                                    }

                                    repository.Create(people.ToArray()); //Вставляет массив элементов
                                   // repository.Create(person); // добавляет один элемент
                                    StringBuilder stringBuilder = new StringBuilder(1000);
                                    stringBuilder.Append("<h1 style='color:blue'>This is person add: </h1>");
                                    stringBuilder.Append("<h1 style='color:blue'>"+ person.ToString() + "</h1>");
                                    await stream.WriteLineAsync(stringBuilder.ToString());
                                    continue;
                                }
                               else if (!string.IsNullOrWhiteSpace(id))
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
                               else if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
                                {
                                    var people = repository.GetByIdFromTo(int.Parse(from), int.Parse(to));
                                    StringBuilder stringBuilder = new StringBuilder(1000);
                                    stringBuilder.Append($"<h1 style = 'color:blue'>This is list of users from {from} to {to}: </h1>");
                                    stringBuilder.Append("<ul>");
                                    foreach (Person item in people)
                                    {
                                        stringBuilder.Append($"<li>{item.id}) {item.first_name} {item.last_name} {item.gender}</li>");
                                    }
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
