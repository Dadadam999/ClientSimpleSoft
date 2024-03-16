using System.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace ClientSimpleSoft
{
    public class DataBase
    {
        private string _connectionString { get; set; }
        private readonly SqlConnection _connection;
        private StringBuilder? _queryBuilder;

        public DataBase( string connectionString )
        {
            _connectionString = connectionString;
            _connection = new SqlConnection( _connectionString );
        }

        public void ConnectionClose() => _connection.Close();
        public void ConnectionOpen() => _connection.Open();

        public void Insert( string tableName, Dictionary<string, string> fields, string where )
        {
            ConnectionOpen();
            _queryBuilder = new StringBuilder();
            _queryBuilder.Append( $"INSERT INTO {tableName} (" );

            foreach( KeyValuePair<string, string> field in fields )
            {
                _queryBuilder.Append( $"{field.Key}, " );
            }

            _queryBuilder.Remove( _queryBuilder.Length - 2, 2 );
            _queryBuilder.Append( ") VALUES (" );

            foreach( KeyValuePair<string, string> field in fields )
            {
                _queryBuilder.Append( $"@{field.Key}, " );
            }

            _queryBuilder.Remove( _queryBuilder.Length - 2, 2 );
            _queryBuilder.Append( ')' );

            if( !string.IsNullOrEmpty( where ) )
            {
                _queryBuilder.Append( $" WHERE {where}" );
            }

            Logging.Create( new Log("MSSQL", _queryBuilder.ToString()) );
            using SqlCommand command = new( _queryBuilder.ToString(), _connection );

            foreach( KeyValuePair<string, string> field in fields )
            {
                DateTime dateValue;

                if( DateTime.TryParseExact( field.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue ) )
                {
                    command.Parameters.AddWithValue( $"@{field.Key}", dateValue );
                }
                else
                {
                    command.Parameters.AddWithValue( $"@{field.Key}", field.Value );
                }
            }

            command.ExecuteNonQuery();
        }

        public void Update( string tableName, Dictionary<string, string> fields, string where )
        {
            ConnectionOpen();
            _queryBuilder = new StringBuilder();
            _queryBuilder.Append( $"UPDATE {tableName} SET " );

            foreach( KeyValuePair<string, string> field in fields )
            {
                _queryBuilder.Append( $"{field.Key} = @{field.Key}, " );
            }

            _queryBuilder.Remove( _queryBuilder.Length - 2, 2 );

            if( !string.IsNullOrEmpty( where ) )
            {
                _queryBuilder.Append( $" WHERE {where}" );
            }

            Logging.Create( new Log( "MSSQL", _queryBuilder.ToString() ) );
            using SqlCommand command = new( _queryBuilder.ToString(), _connection );

            foreach( KeyValuePair<string, string> field in fields )
                command.Parameters.AddWithValue( $"@{field.Key}", field.Value );

            command.ExecuteNonQuery();
        }

        public SqlDataReader Select( string tableName, string where )
        {
            ConnectionOpen();
            _queryBuilder = new StringBuilder();
            _queryBuilder.Append( $"SELECT * FROM {tableName}" );

            if( !string.IsNullOrEmpty( where ) )
            {
                _queryBuilder.Append( $" WHERE {where}" );
            }

            SqlCommand command = new( _queryBuilder.ToString(), _connection );
            SqlDataReader reader = command.ExecuteReader();
            Logging.Create( new Log( "MSSQL", _queryBuilder.ToString() ) );
            return reader;
        }

        public SqlDataReader SelectLast( string tableName, string field, string where = "" )
        {
            ConnectionOpen();
            _queryBuilder = new StringBuilder();
            _queryBuilder.Append( $"SELECT TOP 1 * FROM {tableName}" );

            if( !string.IsNullOrEmpty( where ) )
                _queryBuilder.Append( $" WHERE {where}" );

            if( !string.IsNullOrEmpty( field ) )
                _queryBuilder.Append( $" ORDER BY {field} DESC" );

            Logging.Create( new Log( "MSSQL", _queryBuilder.ToString() ) );
            SqlCommand command = new( _queryBuilder.ToString(), _connection );
            SqlDataReader reader = command.ExecuteReader();
            return reader;
        }

        public SqlDataReader SelectGrouped( string tableName, string field )
        {
            ConnectionOpen();
            _queryBuilder = new StringBuilder();
            _queryBuilder.Append( $"SELECT {field} FROM {tableName} GROUP BY {field}" );
            Logging.Create( new Log( "MSSQL", _queryBuilder.ToString() ) );
            SqlCommand command = new( _queryBuilder.ToString(), _connection );
            SqlDataReader reader = command.ExecuteReader();
            return reader;
        }

        public SqlDataReader SelectWhereGrouped( string tableName, string field, string where )
        {
            ConnectionOpen();
            _queryBuilder = new StringBuilder();
            _queryBuilder.Append( $"SELECT {field} FROM {tableName} WHERE {where} GROUP BY {field}" );
            Logging.Create( new Log( "MSSQL", _queryBuilder.ToString() ) );
            SqlCommand command = new( _queryBuilder.ToString(), _connection );
            SqlDataReader reader = command.ExecuteReader();
            return reader;
        }
    }
}