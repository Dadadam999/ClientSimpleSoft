using Newtonsoft.Json;
using System.Text;

namespace ClientSimpleSoft
{
    public class HttpFetch
    {
        private string _domain { get; set; }
        private HttpContent? _content { get; set; }

        public HttpFetch( string domain )
        {
            _domain = domain;
        }

        public void PrepareDataAsJson( Dictionary<string, string> parameters )
        {
            var json = JsonConvert.SerializeObject( parameters );
            _content = new StringContent( json, Encoding.UTF8, "application/json" );
        }

        public void PrepareDataAsFormData( Dictionary<string, string> parameters )
        {
            var formData = new MultipartFormDataContent();
            foreach( var key in parameters.Keys )
            {
                formData.Add( new ByteArrayContent( Encoding.UTF8.GetBytes( parameters[key] ) ), key );
            }
            _content = formData;
        }

        public async Task<ResponceModel?> GetResponce( string endpoint )
        {
            using var client = new HttpClient();
            var response = await client.PostAsync( new Uri( _domain + endpoint ), _content );
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            if( responseBody == null )
                return new ResponceModel();

            Logging.Create( new Log( "API result", endpoint, responseBody ) );
            return JsonConvert.DeserializeObject<ResponceModel>( responseBody );
        }
    }
}
