namespace ClientSimpleSoft
{
    [Serializable]
    public class IntegrationModel
    {
        //main
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string Domain { get; set; }
        //preview tabel
        public string TableNamePreview { get; set; }
        public (string, string)[] FieldsMatching { get; set; }
        public string DateField { get; set; }
        public string OrderIdField { get; set; }
        //web
        public string SecretKey { get; set; }
        public string FormId { get; set; }
        public string IntegrationId { get; set; }
		public string PeriodDate { get; set; }
        //cycles table
        public string TableNameCycles { get; set; }
        public string CyclesListField { get; set; }
        public string CycleFormField { get; set; }
        public string CheckedCyclesField { get; set; }
        public string TypeIntegration { get; set; }
        public string FormIdField { get; set; }
        public bool IsEnabled { get; set; }
    }
}