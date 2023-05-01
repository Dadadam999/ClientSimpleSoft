using System.Text;
using System.Xml.Serialization;

namespace ClientSimpleSoft
{
    [Serializable]
    public class Integration
    {
        private XmlSerializer _serializer = new XmlSerializer( typeof( List<IntegrationModel> ) );

        [XmlElement( "IntegrationModel" )]
        public List<IntegrationModel> Integrations { get; set; } = new List<IntegrationModel>();

        public IntegrationModel? GetByName( string name )
        {
            IntegrationModel? model = Integrations.Find( x => x.Name == name );

            if( model == null )
                return null;

            return model;
        }

        public void Serialize()
        {
            try
            {
                using FileStream stream = new FileStream( "config_integration.xml", FileMode.Create );
                _serializer.Serialize( stream, Integrations );
            }
            catch( Exception ex )
            {
                MessageBox.Show( $"Ошибка при сохранении данных: {ex.Message}" );
            }
        }

        public void Deserialize()
        {
            if( !File.Exists( "config_integration.xml" ) )
                return;

            try
            {
                using FileStream stream = new FileStream( "config_integration.xml", FileMode.Open );
                Integrations = (List<IntegrationModel>) _serializer.Deserialize( stream );
            }
            catch( Exception ex )
            {
                MessageBox.Show( $"Ошибка при загрузке данных: {ex.Message}" );
            }
        }

        public (string, string)[] ParseMatchingFieldsFromString( RichTextBox text )
        {
            List<(string, string)> result = new List<(string, string)>();

            try
            {
                foreach( string line in text.Lines )
                {
                    string[] fields = line.Split( ',' );
                    result.Add( (fields[0], fields[1]) );
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( $"Данные поля соответствия полей введены неверно!\nОшибка:{ex.Message}" );
            }

            return result.ToArray();
        }

        public string MatchingFieldsToString( (string, string)[] fields )
        {
            if( fields == null || fields.Length == 0 )
                return string.Empty;

            StringBuilder result = new StringBuilder();

            foreach( (string key, string value) in fields )
                result.AppendLine( $"{key},{value}" );

            return result.ToString();
        }

    }
}
