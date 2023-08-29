# RESTFUL API example

This currently targets dotnet 7 and was created using VS2022 Community. 
It uses dotnet secret manager for passwords instead of putting them in appsettings.
It could be adapted to use a keyvault or some other external provider.
Depending on what type of database you want to use there is an appsetting for either postgres or mongo.
Dockerfile is provided to build a conatinerized version of the app for use in a Kubernetes cluster.
The mongo db used locally was a containerized version.
Postgres is version 15, but shouldn't be an issue for other recent versions. The schema is RESTFUL.

### Migrations weren't used in this example for postgres
```
CREATE TABLE IF NOT EXISTS "RESTFUL".items
(
    "Id" uuid NOT NULL,
    "Name" character varying(200) COLLATE pg_catalog."default" NOT NULL,
    "Price" numeric NOT NULL,
    "CreatedDate" date NOT NULL,
    CONSTRAINT "Item_pkey" PRIMARY KEY ("Id")
)
```

### Add secret manager
(secrets can typically be found here %APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json)
`dotnet user-secrets init`

### Add a secret (following the app settings structure)
`dotnet user-secrets set MongoDBSettings:Password somepassword`
`dotnet user-secrets set PGSQLSettings:Password somepassword`

### Build the docker image
`docker build -t yourdockerhub/restful:v1.0.0 .`

### Create a new network locally if you need
`docker network create restfulnet`

### Run Mongo container if using mongodb
`docker run -d --rm --name mongo -p 27017:27017 -v mongodbdata:/data/db -e MONGO_INITDB_ROOT_USERNAME=mongoadmin -e MONGO_INITDB_ROOT_PASSWORD=password --network=restfulnet mongo`

### Run the container/app
`docker run -it --rm -p 2800:80 -e MongoDBSettings:Host=mongo -e MongoDBSettings:Password=password --network=restfulnet restful:v1.0.0`
`docker run -it --rm -p 2800:80 -e PGSQLSettings:Host=somenetworkwithyourpostgresdb -e PGSQLSettings:Password=password --network=restfulnet restful:v1.0.0`

## Using Kubernetes

### Create the secret in kubectl for mongo pw
`kubectl create secret generic restful-secrets --from-literal=mongodb-password='somepassword'`

### Create the services/apps for your cluster (if not using default namespace append -n yournamespace)
`kubectl apply -f mongodb.yml`
`kubectl apply -f restful.yml`

### Check the pods (append -n yournamespace if needed)
`kubectl get po`