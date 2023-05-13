using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

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
            RunForArgs();
        }

        private void RunForArgs()
        {
            string[] args = Environment.GetCommandLineArgs();

            if( args.Length > 1 )
            {
                string arg1 = args[1];
                
                if( arg1 == "/getanswer" )
                {
                    SyncAnswer();
                    return;
                }
                
                if( arg1 == "/sendcycles" )
                {
                    SyncCycles();
                    return;
                }
            }
            else
            {
                SyncAnswer();
            }

            Application.Exit();
        }

        private async void SyncCycles()
        {
            foreach( IntegrationModel integrationModel in _integration.Integrations )
            {
                _output.Text += $"Выполнение: {integrationModel.Name}.\n";
                _httpFetch = new HttpFetch( integrationModel.Domain );
                _dataBase = new DataBase( integrationModel.ConnectionString );
                SqlDataReader reader = _dataBase.SelectGrouped( integrationModel.TableNameCycles, integrationModel.CyclesListField);
                List<string?> cyclesArrow = new List<string?>();

                while( reader.Read() )
                    cyclesArrow.Add( reader[0].ToString() );

                string cyclesJson = JsonConvert.SerializeObject( cyclesArrow );

                _httpFetch.PrepareData( new Dictionary<string, string>
                {
                    { "quickapi-secret", integrationModel.SecretKey },
                    { "quickapi-form-id", integrationModel.FormId },
                    { "quickapi-integration-id", integrationModel.IntegrationId },
                    { "quickapi-field-cycles", integrationModel.CycleFormField },
                    { "quickapi-cycles", cyclesJson }
                } );

                _output.Text += "Выполнение запроса на сервер...\n";
                await _httpFetch.GetResponce( "/wp-json/quickapi/v1/syncfield" );
                _output.Text += "Интеграция выполнена.\n";
            }

            MessageBox.Show("Метод 2");
        }

        public async void SyncAnswer()
        {
            foreach( IntegrationModel integrationModel in _integration.Integrations )
            {
                _output.Text += $"Выполнение: {integrationModel.Name}.\n";
                _httpFetch = new HttpFetch( integrationModel.Domain );
                _dataBase = new DataBase( integrationModel.ConnectionString );

                SqlDataReader reader = _dataBase.SelectLast( integrationModel.TableNamePreview, integrationModel.OrderIdField );

                string lastId = "0";

                if( reader.Read() )
                    lastId = reader.GetString( reader.GetOrdinal( integrationModel.DateField ) );

                if( !Regex.IsMatch( integrationModel.OrderIdField, @"^\d+$" ) )
                    integrationModel.OrderIdField = "0";

                DateTime dateValue = DateTime.Now.AddDays( -1 * Convert.ToInt32( integrationModel.OrderIdField ) ); 

                _httpFetch.PrepareData( new Dictionary<string, string>
                {
                    { "quickapi-secret", integrationModel.SecretKey },
                    { "quickapi-form-id", integrationModel.FormId },
                    { "quickapi-integration-id", integrationModel.IntegrationId },
                    { "quickapi-date-point", dateValue.ToString() },
                    { "quickapi-last-answer", lastId }
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
                                string fieldNameSql = string.Empty;

                                foreach( (string Key, string Value) matchingField in integrationModel.FieldsMatching )
                                {
                                    _output.Text += $" {matchingField.Value} == {field.name} " + Environment.NewLine;
                                    if( matchingField.Value == (string)field.name )
                                    {
                                        fieldNameSql = matchingField.Key;
                                        _output.Text += matchingField.Key + Environment.NewLine;
                                        break;
                                    }
                                }

                                if( !string.IsNullOrEmpty( fieldNameSql ) )
                                    fields.Add( fieldNameSql, (string)field.value );
                            }
                        }

                        fields.Add( integrationModel.DateField, (string) answer.date );
                        _dataBase.Insert( integrationModel.TableNamePreview, fields, "" );
                    }
                }
                _output.Text += "Интеграция выполнена.\n";
            }
        }
    }
}