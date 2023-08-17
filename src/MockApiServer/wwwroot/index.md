# Welcome
This is the Default Page for the [MockApiServer](https://github.com/jacodv/mock-api-server).  For detailed documentation on how to use the MockApiServer, 
please visit the [MockApiServer Wiki](https://github.com/jacodv/mock-api-server).

## Samples
To get the [POSTMAN collection](https://github.com/jacodv/mock-api-server/tree/develop/src/Samples/PostMan) visit here `https://github.com/jacodv/mock-api-server/tree/develop/src/Samples/PostMan`:

#### GET: `/api/sample`
This is a sample GET request.  It will return a JSON object with the following structure:
```json
{
Id: "SampleId",
Name: "SampleName"
}
```
```curl
curl https://localhost:5001/api/sample
```

#### OAuth Login POST: `/auth` - *(x-www-form-urlencoded)*
This is a sample POST typically used for OAuth login:
```json
{
    "UserName": "userName",
    "Token": "test-user-token",
    "Created": "current-date",
    "Expires": "current-date-plus-30-minues-in-secods",
    "ExpiresInSeconds": 1800,
    "access_token": "test-user-token",
    "token_type": "Bearer",
}
```
```curl
curl -d "username=my-user-name&password=pwd@grant_type=password@client_id=client1" -H "Content-Type: application/x-www-form-urlencoded" -X POST https://localhost:5001/auth
```
#### GraphQL POST: `/graphql`
This is a sample graphql query POST. Use the following query:
```json
{
	query: "query {
		sample {
			nodes {
			id
			}
		}
		}",
		variables: null,
		operationName: "SampleQuery"
}
```
```curl
curl --location 'https://localhost:5001/graphql' \
--header 'Content-Type: application/json' \
--data '{query: "query {
                sample {
                    nodes {
                        id
                }
           }
        }",
 variables:null,
 operationName: "SampleQuery"
 }'
```