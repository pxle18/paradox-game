using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using VMP_CNR.Module.Configurations;

namespace VMP_CNR.Module.Tasks
{
    public abstract class SqlResultTask : SynchronizedTask
    {
        public override void Execute()
        {
            using (var connection = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = GetQuery();
                    using (var reader = command.ExecuteReader())
                    {
                        OnFinished(reader);
                    } 
                }
            }
        }

        public abstract string GetQuery();

        public abstract void OnFinished(MySqlDataReader reader);
    }

    public abstract class AsyncSqlResultTask : SynchronizedTask
    {
        public override async void Execute()
        {
            using (var connection = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = GetQuery();
                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        await OnFinished(reader);
                    }
                }
            }
        }

        public abstract string GetQuery();

        public abstract Task OnFinished(MySqlDataReader reader);
    }
}