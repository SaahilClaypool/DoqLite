# DoqLite

Document Storage on SQLite

Use SQLite3 (version 3.38.2 or higher) as a document database similar to the [cosmos sdk](https://github.com/Azure/azure-cosmos-dotnet-v3), but in-process.
This uses the [JSON1](https://www.sqlite.org/json1.html) extensions that are now part of the core SQLite library, and this will use the [FTS5](https://www.sqlite.org/fts5.html) extensions to provide full text search over those json documents.

[Example](./Program.cs)
```cs
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

foreach (var item in collection.Items)
{
    WriteLine(item.ToJson());
}

// {"key":1,"x":1,"y":"change row one"}
```

## Features

- [x] CRUD
- [ ] Linq
- [ ] Full text search

## Generated SQL

```cs
collection.Upsert(item1);
```

```sql
INSERT INTO myclass (key, body)
    VALUES (1, json('{"key":1,"x":1,"y":"change row one"}'))
    ON CONFLICT (key) DO UPDATE SET body = excluded.body
    RETURNING key
```

## References

- [SQLite as a document database](https://dgl.cx/2020/06/sqlite-json-support)
    - [hacker news discussion](https://news.ycombinator.com/item?id=25226260)
- [Using the SQLite JSON1 and FTS5 Extensions with Python](https://charlesleifer.com/blog/using-the-sqlite-json1-and-fts5-extensions-with-python/?utm_source=pocket_mylist)
- [Linq 2 Cosmos](https://github.com/Azure/azure-cosmos-dotnet-v3/blob/master/Microsoft.Azure.Cosmos/src/Linq/ExpressionToSQL.cs)