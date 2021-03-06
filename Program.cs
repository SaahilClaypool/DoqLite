global using Microsoft.Data.Sqlite;
global using Dapper;
global using DoqLite;
using static System.Console;

SetupSqlite.Init();

using var db = new SqliteConnection($"Data Source=:memory:");
db.Open();

var collection = new DoqCollection<MyClass>(db);
var item1 = new MyClass() { X = 1, Y = "row one" };
collection.Upsert(item1);

var item2 = new MyClass() { X = 1, Y = "row two" };
collection.Upsert(item2);

item1.Y = "change row one";
collection.Upsert(item1);

collection.Delete(item2);

item1.Key = 10;
collection.Upsert(item1);

foreach (var item in collection.Where("t.body->>'y' = 'change row one'"))
{
    WriteLine(item.ToJson());
}

class MyClass : IEntity
{
    public int Key { get; set; }
    public int X { get; set; }
    public string Y { get; set; }
}
