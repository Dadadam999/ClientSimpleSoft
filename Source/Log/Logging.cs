namespace ClientSimpleSoft
{
    public static class Logging
    {
        private static string filePath
        {
            get
            {
                return $"log_{DateTime.Now:yyyyMMddHHmmss}.txt";
            }
        }

        public static void Create( Log log )
        {
            File.AppendAllText( filePath, string.Join( " ", log.List ) );
        }

        public static void DeleteFile()
        {
            if( File.Exists( filePath ) )
            {
                File.Delete( filePath );
            }
        }
    }
}
