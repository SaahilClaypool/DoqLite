using System.Data;

namespace DoqLite
{
    public class DoqCollection<T>
    {
        private readonly SqliteConnection conn;
        private readonly string collectionName;
        public readonly string returning = "RETURNING key";

        public DoqCollection(SqliteConnection conn, string? name = null)
        {
            name ??= typeof(T).Name;
            JsonTypeHandler<T>.Register();
            name = name.ToLower();
            conn.ExecuteScalar($@"CREATE TABLE IF NOT EXISTS {name} (key integer primary key, body JSON );");

            this.conn = conn;
            collectionName = name;
        }

        public int Insert(T item, bool update = false)
        {
            var updateExpression = update ? "ON CONFLICT (key) DO UPDATE SET body = excluded.body" : "";
            bool hasKey = false;
            var keyProp = item.GetProperty("Key");
            var key = keyProp.GetValue(item);

            var itemJson = item.ToJson();
            var values = $"(json('{itemJson}'))";
            var insertExpression = $"(body)";

            if (key.GetType().IsAssignableTo(typeof(int)))
            {
                hasKey = true;
                if ((int)key > 0)
                {
                    values = $"({key}, json('{itemJson}'))";
                    insertExpression = $"(key, body)";
                }
            }

            var command =
                $@"INSERT INTO {collectionName}
                {insertExpression}
                VALUES {values}
                {updateExpression}
                {returning}";
            Console.WriteLine(command);
            var id = conn.ExecuteScalar<int>(command);
            if (hasKey)
            {
                keyProp.SetValue(item, id);
            }
            return id;
        }

        public int Upsert(T item) => Insert(item, update: true);

        public int Delete(int key)
        {
            return conn.ExecuteScalar<int>($"delete from {collectionName} where key = {key} {returning}");
        }

        public int Delete(T item)
        {
            var key = (int)item.GetProperty("Key").GetValue(item, null);
            return Delete(key);
        }

        public T Get(int key)
        {
            return conn.QueryFirst<T>($"select * from {collectionName} where key = {key}");
        }

        public T Get(T item)
        {
            var key = (int)item.GetProperty("Key").GetValue(item, null);
            return Get(key);
        }

        public IEnumerable<T> Items => conn.Query<T>($"select body from {collectionName}");
    }

    public class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
    {
        public static void Register()
        {
            SqlMapper.AddTypeHandler<T>(new JsonTypeHandler<T>());
        }

        public override void SetValue(IDbDataParameter parameter, T value)
        {
            parameter.Value = value.ToJson();
        }

        public override T Parse(object value)
        {
            return ((string)value).ToObject<T>();
        }
    }
}