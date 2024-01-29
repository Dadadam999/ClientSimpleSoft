namespace ClientSimpleSoft
{
    public class Log
    {
        public string Type { get; set; } = string.Empty;
        public string Request { get; set; } = string.Empty;
        public string Responce { get; set; } = string.Empty;
        public DateTime CreateAt
        {
            get => DateTime.Now;
        }

        public List<string> List
        {
            get => new List<string>{ CreateAt.ToString(), Type, Request, Responce, Environment.NewLine };
        }

        public Log(string type, string request = "", string responce = "" )
        {
            Type = type;
            Request = request;
            Responce = responce;
        }
    }
}