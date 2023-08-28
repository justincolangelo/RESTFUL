using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTFUL.Settings
{
    public class MongoDBSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        // populated at runtime by secret manager
        public string Password { get; set; }
        public string ConnectionString
        {
            get
            {
                //return $"mongodb://{Host}:{Port}";
                // if you've created a secret for password you can use this connection string
                // dotnet user-secrets init
                // dotnet user-secrets set MongoDBSettings:Password somepassword
                return $"mongodb://{User}:{Password}@{Host}:{Port}";
            }
        }
    }
}
