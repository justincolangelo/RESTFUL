
namespace RESTFUL.Settings
{
    public class MSSQLSettings
    {
        public string DBName { get; set; }
        public string ConnectionString
        {
            get
            {
                return $"Server=.;Database={DBName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
                // dotnet user-secrets init
                // dotnet user-secrets set PGSQLSettings:Password somepassword
            }
        }
    }
}
