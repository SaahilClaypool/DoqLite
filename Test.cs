namespace DoqLite;

public static class Test
{
    public static async Task Run(SqliteConnection db)
    {
        var collection = new DoqCollection<MyClass>(db, "items");
        collection.Upsert(new MyClass() { X = 1, Y = "test" });
        collection.Upsert(new MyClass() { X = 2, Y = "test Me", Key = 1});
        var item = collection.Get(1);
        Console.WriteLine(item.ToJson());
    }

    class MyClass
    {
        static MyClass()
        {
        }
        public int Key { get; set; }
        public int X { get; set; }
        public string Y { get; set; }
    }
}