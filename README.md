# mock-json-api
ASPNet Core 3.x Webserver that can be used to mock any API request.  The web server supports most RESTful operations as well as standard http requests.  The mock server can also be confugured to respond to GraphQL request providing that the GraphQL endpoint is `/graphql`

## Getting started

Clone the repository and build the solution by running, command below, in the repository root.  Make sure all the nuget packages were restored and that the solution build successfully.
```
dotnet build
```

Start the `Mock-Api-Server` with 
```
dotnet run --project .\MockApiServer\MockApiServer.csproj
```

Execute ```https://localhost:5001/api/sample``` to test the with a provided sample document and verify that the below response was received.
```
{"Id":"SampleId","Name":"SampleName"}
```
