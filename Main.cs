using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;

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
            _output.Text += "Файл конфигурации считан.\n";
            Working();
        }

        public async void Working()
        {
            foreach( IntegrationModel integrationModel in _integration.Integrations )
            {
                _output.Text += $"Выполнение: {integrationModel.Name}.\n";
                _httpFetch = new HttpFetch( integrationModel.Domain );
                _dataBase = new DataBase( integrationModel.ConnectionString );

                SqlDataReader reader = _dataBase.SelectLastDate( integrationModel.TableName, integrationModel.DateField );

                DateTime dateValue = DateTime.Now;

                //if( reader.Read() )
                //    dateValue = reader.GetDateTime( reader.GetOrdinal( integrationModel.DateField ) );

                _httpFetch.PrepareData( new Dictionary<string, string>
                {
                    { "quickapi-secret", integrationModel.SecretKey },
                    { "quickapi-form-id", integrationModel.FormId },
                    { "quickapi-integration-id", integrationModel.IntegrationId },
                    { "quickapi-date-point", dateValue.ToString() },
                } );

                _output.Text += "Выполнение запроса на сервер...\n";
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
                                Dictionary<string, string> matching = integrationModel.FieldsMatching.ToDictionary( x => x.Item1, x => x.Item2 );
                                string fieldNameSql = string.Empty;

                                foreach( KeyValuePair<string, string> matchingField in matching )
                                {
                                    if( matchingField.Value == field.key )
                                    {
                                        fieldNameSql = matchingField.Key;
                                        break;
                                    }
                                }

                                if( !string.IsNullOrEmpty( fieldNameSql ) )
                                    fields.Add( fieldNameSql, (string)field.value );
                            }
                        }

                        fields.Add( integrationModel.DateField, (string) answer.date );
                        _dataBase.Insert( integrationModel.TableName, fields, "" );
                    }
                }
                _output.Text += "Интеграция выполнена.\n";
            }
        }
    }
}