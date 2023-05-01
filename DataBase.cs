using System.Data.SqlClient;
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
            _connection.Open();
        }

        public void ConnectionClose() => _connection.Close();

        public void Insert( string tableName, Dictionary<string, string> fields, string where )
        {
            _queryBuilder = new StringBuilder();
            _queryBuilder.Append( $"INSERT INTO {tableName} (" );

            foreach( KeyValuePair<string, string> field in fields )
                _queryBuilder.Append( $"{field.Key}, " );


            _queryBuilder.Remove( _queryBuilder.Length - 2, 2 );
            _queryBuilder.Append( ") VALUES (" );

            foreach( KeyValuePair<string, string> field in fields )
                _queryBuilder.Append( $"@{field.Key}, " );

            _queryBuilder.Remove( _queryBuilder.Length - 2, 2 );
            _queryBuilder.Append( ')' );

            if( !string.IsNullOrEmpty( where ) )
                _queryBuilder.Append( $" WHERE {where}" );

            using SqlCommand command = new( _queryBuilder.ToString(), _connection );
            foreach( KeyValuePair<string, string> field in fields )
                command.Parameters.AddWithValue( $"@{field.Key}", field.Value );

            command.ExecuteNonQuery();
        }

        public void Update( string tableName, Dictionary<string, string> fields, string where )
        {
            _queryBuilder = new StringBuilder();
            _queryBuilder.Append( $"UPDATE {tableName} SET " );

            foreach( KeyValuePair<string, string> field in fields )
                _queryBuilder.Append( $"{field.Key} = @{field.Key}, " );

            _queryBuilder.Remove( _queryBuilder.Length - 2, 2 );

            if( !string.IsNullOrEmpty( where ) )
                _queryBuilder.Append( $" WHERE {where}" );

            using SqlCommand command = new( _queryBuilder.ToString(), _connection );
            foreach( KeyValuePair<string, string> field in fields )
                command.Parameters.AddWithValue( $"@{field.Key}", field.Value );

            command.ExecuteNonQuery();
        }

        public SqlDataReader Select( string tableName, string where )
        {
            _queryBuilder = new StringBuilder();
            _queryBuilder.Append( $"SELECT * FROM {tableName}" );

            if( !string.IsNullOrEmpty( where ) )
                _queryBuilder.Append( $" WHERE {where}" );

            SqlCommand command = new( _queryBuilder.ToString(), _connection );
            SqlDataReader reader = command.ExecuteReader();
            return reader;
        }

        public SqlDataReader SelectLastDate( string tableName, string TimeCode )
        {
            _queryBuilder = new StringBuilder();
            _queryBuilder.Append( $"SELECT TOP 1 * FROM {tableName}" );

            if( !string.IsNullOrEmpty( TimeCode ) )
                _queryBuilder.Append( $" ORDER BY {TimeCode} DESC" );

            SqlCommand command = new( _queryBuilder.ToString(), _connection );
            SqlDataReader reader = command.ExecuteReader();
            return reader;
        }
    }
}