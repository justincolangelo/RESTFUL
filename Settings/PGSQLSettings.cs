
namespace RESTFUL.Settings
{
    public class PGSQLSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        // populated at runtime by secret manager
        public string Password { get; set; }
        public string DBName { get; set; }
        public string ConnectionString
        {
            get
            {
                return $"Host={Host};Username={User};Password={Password};Database={DBName}";
                // dotnet user-secrets init
                // dotnet user-secrets set PGSQLSettings:Password somepassword
            }
        }
    }
}
