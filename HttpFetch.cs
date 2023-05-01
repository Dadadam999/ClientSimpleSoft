using Newtonsoft.Json;
using System.Text;

namespace ClientSimpleSoft
{
    public class HttpFetch
    {
        private string _domain { get; set; }
        private MultipartFormDataContent _formData { get; set; } = new MultipartFormDataContent();

        public HttpFetch( string domain )
        {
            _domain = domain;
        }

        public void PrepareData( Dictionary<string, string> parameters )
        {
            _formData = new MultipartFormDataContent();

            foreach( string key in parameters.Keys )
                _formData.Add( new ByteArrayContent( Encoding.UTF8.GetBytes( parameters[key] ) ), key );
        }

        public async Task<ResponceModel?> GetResponce( string endpoint )
        {
            using var client = new HttpClient();
            var response = await client.PostAsync( new Uri( _domain + endpoint ), _formData );
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            if( responseBody == null )
                return new ResponceModel();

            return JsonConvert.DeserializeObject<ResponceModel>( responseBody );
        }
    }
}