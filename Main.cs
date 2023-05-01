using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Text;

namespace ClientSimpleSoft
{
    public partial class Main : Form
    {
        private HttpFetch _httpFetch { get; set; }
        private Integration _integration { get; set; }
        private DataBase _dataBase { get; set; }

        public Main()
        {
            InitializeComponent();
            _integration = new Integration();
            _integration.Deserialize();
            _output.Text = "Файл конфигурации считан.\n";
            Working();           
        }

        public async void Working()
        {

            foreach( IntegrationModel integrationModel in _integration.Integrations )
            {
                _output.Text = $"Выполнение: {integrationModel.Name}.\n";
                _httpFetch = new HttpFetch( integrationModel.Domain );
                _dataBase = new DataBase( integrationModel.ConnectionString );

                SqlDataReader reader = _dataBase.SelectLastDate( integrationModel.TableName, integrationModel.DateField );

                DateTime dateValue = DateTime.Now;

                if( reader.Read() )
                    dateValue = reader.GetDateTime( reader.GetOrdinal( integrationModel.DateField ) );

                _httpFetch.PrepareData( new Dictionary<string, string>
                {
                    { "quickapi-secret", integrationModel.SecretKey },
                    { "quickapi-form-id", integrationModel.FormId },
                    { "quickapi-integration-id", integrationModel.IntegrationId },
                    { "quickapi-date-point", dateValue.ToString() },
                } );

                _output.Text = "Выполнение запроса на сервер...\n";
                ResponceModel? responce = await _httpFetch.GetResponce( "/wp-json/quickapi/v1/getanswers" );

                if( responce == null )
                    return;

                dynamic? answers = JsonConvert.DeserializeObject<dynamic>( responce.Result );

                if( answers == null )
                    return;

                foreach( var answer in answers )
                {
                    if( answer != null )
                    {

                        Dictionary<string, string> fields = new Dictionary<string, string>();

                        foreach( var field in answer.fields )
                        {
                            if( field != null )
                            {
                                string fieldNameSql = getFieldSql( field.name, integrationModel.FieldsMatching );

                                if( fieldNameSql != null )
                                    fields.Add( fieldNameSql, field.value);
                            }
                        }

                        fields.Add( integrationModel.DateField, answer.date );
                        _dataBase.Insert( integrationModel.TableName, fields, "" );
                    }
                }
            }
        }

        public string getFieldSql( string fieldNameWeb, (string, string)[] matchingFields )
        {
            foreach( (string key, string value) in matchingFields )
                if( value == fieldNameWeb )
                    return key;

            return string.Empty;
        }
    }
}