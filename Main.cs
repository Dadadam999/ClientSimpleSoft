using Newtonsoft.Json;
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
            Logging.DeleteFile();
            _integration = new Integration();
            _integration.Deserialize();
            _output.Text += "Файл конфигурации считан.\n";
            RunForArgs();
        }

        private async void RunForArgs()
        {
            await Test();
            return;

            string[] args = Environment.GetCommandLineArgs();

            if( args.Length < 2 )
            {
                await SyncAnswer();
                return;
            }

            foreach( string arg in args )
            {
                if( arg == "getanswer" )
                {
                    await SyncAnswer();
                }

                if( arg == "sendcycles" )
                {
                    await SendCycles();
                }

                if( arg == "exit" )
                {
                    Application.Exit();
                }
            }
        }

        private async Task SendCycles()
        {
            foreach( IntegrationModel integrationModel in _integration.Integrations )
            {
                if( !integrationModel.IsEnabled )
                {
                    continue;
                }

                if( integrationModel.TypeIntegration != "Quick Form" )
                {
                    continue;
                }

                _output.Text += $"Выполнение: {integrationModel.Name}.\n";
                _httpFetch = new HttpFetch( integrationModel.Domain );
                _dataBase = new DataBase( integrationModel.ConnectionString );
                using SqlDataReader reader = _dataBase.SelectWhereGrouped( integrationModel.TableNameCycles, integrationModel.CyclesListField, $"{integrationModel.CheckedCyclesField} != 0" );
                List<string?> cyclesArrow = new List<string?>();

                while( reader.Read() )
                {
                    cyclesArrow.Add( reader[0].ToString() );
                }

                _dataBase.ConnectionClose();
                _output.Text += $"Количество циклов: {cyclesArrow.Count}.\n";
                string cyclesJson = JsonConvert.SerializeObject( cyclesArrow );

                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "quickapi-secret", integrationModel.SecretKey },
                    { "quickapi-form-id", integrationModel.FormId },
                    { "quickapi-integration-id", integrationModel.IntegrationId },
                    { "quickapi-field-cycles", integrationModel.CycleFormField },
                    { "quickapi-cycles", cyclesJson }
                };

                Logging.Create( new Log( "API prepare", string.Join( " ", data ) ) );
                _httpFetch.PrepareDataAsFormData( data );
                _output.Text += "Выполнение запроса на сервер...\n";
                await _httpFetch.GetResponce( "/wp-json/quickapi/v1/sync-cycles-quickform" );
                _output.Text += "Интеграция выполнена.\n";
            }
        }

        public async Task SyncAnswer()
        {
            foreach( IntegrationModel integrationModel in _integration.Integrations )
            {
                if(!integrationModel.IsEnabled) 
                {
                    continue;
                }

                _output.Text += $"Выполнение: {integrationModel.Name}.\n";
                _httpFetch = new HttpFetch( integrationModel.Domain );
                _dataBase = new DataBase( integrationModel.ConnectionString );
                using SqlDataReader reader = _dataBase.SelectLast( integrationModel.TableNamePreview, integrationModel.OrderIdField, $"{integrationModel.FormIdField} = {integrationModel.FormId}" );
                string lastId = "0";

                if( reader != null && reader.HasRows && reader.Read() )
                {
                    int index = reader.GetOrdinal( integrationModel.OrderIdField );

                    if( !reader.IsDBNull( index ) )
                    {
                        lastId = reader.GetValue( index ).ToString();
                    }
                }

                _dataBase.ConnectionClose();

                if( !Regex.IsMatch( integrationModel.PeriodDate, @"^\d+$" ) )
                {
                    integrationModel.PeriodDate = "0";
                }

                DateTime dateValue = DateTime.Now.AddDays( -1 * Convert.ToInt32( integrationModel.PeriodDate ) );
                string formId = integrationModel.TypeIntegration == "Quick Form" ? "quickapi-form-id" : "quickapi-form-id-yandex";
                string endpoint = integrationModel.TypeIntegration == "Quick Form" ? "/wp-json/quickapi/v1/get-answers-quickform" : "/wp-json/quickapi/v1/get-answers-yandex";

                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "quickapi-secret", integrationModel.SecretKey },
                    { formId, integrationModel.FormId },
                    { "quickapi-integration-id", integrationModel.IntegrationId },
                    { "quickapi-date-point", dateValue.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "quickapi-last-answer", lastId }
                };

                Logging.Create( new Log( "API prepare", string.Join( " ", data ) ) );
                
                if( integrationModel.TypeIntegration  == "Quick Form" ) 
                {
                    _httpFetch.PrepareDataAsFormData( data );
                }
                else
                {
                    _httpFetch.PrepareDataAsJson( data );
                }
                
                _output.Text += "Выполнение запроса на сервер...\n";
                ResponceModel? responce = await _httpFetch.GetResponce( endpoint );

                if( responce == null )
                {
                    return;
                }

                dynamic? answers = JsonConvert.DeserializeObject<dynamic>( responce.Result );

                if( answers == null )
                {
                    return;
                }

                foreach( var answer in answers )
                {
                    if( answer == null )
                    {
                        continue;
                    }

                    Dictionary<string, string> fields = new Dictionary<string, string>();

                    foreach( var field in answer.fields )
                    {
                        if( field == null )
                        {
                            continue;
                        }

                        string fieldNameSql = string.Empty;

                        foreach( (string Key, string Value) matchingField in integrationModel.FieldsMatching )
                        {
                            if( matchingField.Value == (string) field.name )
                            {
                                fieldNameSql = matchingField.Key;
                                break;
                            }
                        }

                        if( !string.IsNullOrEmpty( fieldNameSql ) )
                        {
                            fields.Add( fieldNameSql, (string) field.value );
                        }
                    }

                    fields.Add( integrationModel.DateField, (string) answer.date );
                    fields.Add( integrationModel.OrderIdField, (string) answer.id );
                    fields.Add( integrationModel.FormIdField, integrationModel.FormId );
                    _dataBase.Insert( integrationModel.TableNamePreview, fields, "" );
                    _dataBase.ConnectionClose();

                }
                _output.Text += "Интеграция выполнена.\n";
            }
        }

        public async Task Test()
        {
            foreach( IntegrationModel integrationModel in _integration.Integrations )
            {
                _output.Text += $"Выполнение: {integrationModel.Name}.\n";
                _httpFetch = new HttpFetch( integrationModel.Domain );
                string formId = integrationModel.TypeIntegration == "Quick Form" ? "quickapi-form-id" : "quickapi-form-id-yandex";
                string endpoint = integrationModel.TypeIntegration == "Quick Form" ? "/wp-json/quickapi/v1/get-answers-quickform" : "/wp-json/quickapi/v1/get-answers-yandex";


                if( !Regex.IsMatch( integrationModel.PeriodDate, @"^\d+$" ) )
                {
                    integrationModel.PeriodDate = "0";
                }

                DateTime dateValue = DateTime.Now.AddDays( -1 * Convert.ToInt32( integrationModel.PeriodDate ) );

                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "quickapi-secret", integrationModel.SecretKey },
                    { formId, integrationModel.FormId },
                    { "quickapi-integration-id", integrationModel.IntegrationId },
                    { "quickapi-date-point", dateValue.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "quickapi-last-answer", "0" }
                };

                Logging.Create( new Log( "API prepare", string.Join( " ", data ) ) );

                if( integrationModel.TypeIntegration  == "Quick Form" )
                {
                    _httpFetch.PrepareDataAsFormData( data );
                }
                else
                {
                    _httpFetch.PrepareDataAsJson( data );
                }

                _output.Text += "Выполнение запроса на сервер...\n";
                ResponceModel? responce = await _httpFetch.GetResponce( endpoint );

                if( responce == null )
                {
                    return;
                }

                dynamic? answers = JsonConvert.DeserializeObject<dynamic>( responce.Result );

                if( answers == null )
                {
                    return;
                }

                foreach( var answer in answers )
                {
                    if( answer == null )
                    {
                        continue;
                    }

                    Dictionary<string, string> fields = new Dictionary<string, string>();

                    foreach( var field in answer.fields )
                    {
                        if( field == null )
                        {
                            continue;
                        }

                        string fieldNameSql = string.Empty;

                        foreach( (string Key, string Value) matchingField in integrationModel.FieldsMatching )
                        {
                            if( matchingField.Value == (string) field.name )
                            {
                                fieldNameSql = matchingField.Key;
                                break;
                            }
                        }

                        if( !string.IsNullOrEmpty( fieldNameSql ) )
                        {
                            fields.Add( fieldNameSql, (string) field.value );
                        }
                    }

                    _output.Text += $"\n{integrationModel.DateField} = {(string) answer.date}";
                    _output.Text += $"\n{integrationModel.OrderIdField} = {(string) answer.date}";
                    _output.Text += $"\n{integrationModel.FormIdField} = {(string) answer.formId}";
                }

                _output.Text += "Интеграция выполнена.\n";
            }
        }
    }
}