using System.Data;

namespace DoqLite
{
    public class DoqCollection<T> where T : IEntity
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
            var key = item.Key;

            var itemJson = item.ToJson();
            var values = $"(json('{itemJson}'))";
            var insertExpression = $"(body)";

            if (key > 0)
            {
                values = $"({key}, json('{itemJson}'))";
                insertExpression = $"(key, body)";
            }

            var command =
                $@"INSERT INTO {collectionName}
                {insertExpression}
                VALUES {values}
                {updateExpression}
                {returning}";
            Console.WriteLine(command);
            var id = conn.ExecuteScalar<int>(command);
            item.Key = id;
            return id;
        }

        public int Upsert(T item) => Insert(item, update: true);

        public int Delete(int key)
        {
            return conn.ExecuteScalar<int>($"delete from {collectionName} where key = {key} {returning}");
        }

        public int Delete(T item)
        {
            return Delete(item.Key);
        }

        public T Get(int key)
        {
            var (dbKey, item) = conn.QueryFirst<(int, T)>($"SELECT * FROM {collectionName} WHERE key = {key}");
            item.Key = dbKey;
            return item;
        }

        public T Get(T item)
        {
            return Get(item.Key);
        }

        public IEnumerable<T> Items => conn
            .Query<(int key, T value)>($"SELECT * FROM {collectionName} AS t")
            .Select(item =>
            {
                item.value.Key = item.key;
                return item.value;
            });

        /// <summary>
        /// Where clause in sql - collection is named `t`
        /// e.g.,: t.body->>'description' = 'filter on description'
        /// </summary>
        public IEnumerable<T> Where(string where) => conn
            .Query<(int key, T value)>($"SELECT * FROM {collectionName} AS t WHERE {where}")
            .Select(item =>
            {
                item.value.Key = item.key;
                return item.value;
            });
    }

    public class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
    {
        public static void Register()
        {
            SqlMapper.AddTypeHandler(new JsonTypeHandler<T>());
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
