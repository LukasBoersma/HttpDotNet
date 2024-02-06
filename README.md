# HttpDotNet (unmaintained!)

**⚠️ Warning: This project is outdated and not maintained. I wrote it to learn more about HTTP internals. You should not use it for your projects, it probably has security issues.**

A minimal HTTP server and client in pure managed .NET.

[![Build status](https://ci.appveyor.com/api/projects/status/hw8ag5y7txnpfjxv?svg=true)](https://ci.appveyor.com/project/LukasBoersma/httpdotnet)

## Basic Usage

If you just want to download some data or make a request to a server, these code snippets might be useful:

### Simple HTTP requests

```csharp

// The HttpClient class provides several convenience methods to make simple HTTP requests.
// Use these methods to get a HTTP response as a string or byte array:
string responseString = HttpClient.GetString("https://lukas-boersma.com/en");
byte[] responseBytes = HttpClient.GetBytes("https://lukas-boersma.com/en");

// If you need more than just the raw response data, you can get a HttpResponse object:
HttpResponse responseObject = HttpClient.GetResponse("https://lukas-boersma.com/en");

// This object has a stream that you can use to read the response data:
Stream responseStream = responseObject.BodyStream;

// You can access headers like this:
string contentType = responseObject["Content-Type"]; // Will be null if the header is not present in the response.


```

### Downloading a file

You could use `HttpClient.GetBytes` to download a file and then save the byte array into a file. But that would mean that the file has to be downloaded completely into memory before you can start writing it to the disk. Here is a better way to download a large file:

```csharp
var response = HttpClient.GetResponse("http://example.com/large-file.zip");
using(var fileStream = File.OpenWrite("large-file.zip"))
{
    response.BodyStream.CopyTo(fileStream);
}
```

### Hosting a HTTP server

```csharp
var server = new HttpListener();

// Register the RequestParsed event so we can react to incoming requests
server.RequestParsed += (sender, request) =>
{
    // respond to requests here. See the HttpDotnet.Samples.Server sample for details.
    [...]
};

// Listen on localhost (loopback). This way, only local connections will be accepted.
// Change to IPAddress.Any to allow external connections from any source.
var listeningEndpoint = new IPEndPoint(address: IPAddress.Loopback, port: 8888);
server.StartListening(listeningEndpoint);

// Start running the server. This method never returns!
// Use RunInBackground if you want to continue doing work in this thread.
server.RunBlocking();
```

## API Overview

## Convenience classes

The following classes provide an easy-to-use interface that will cover most use cases.

* HttpClient maintains a single connection to a server. Use it to make HTTP requests to a server.
* HttpListener is a HTTP server that waits for incoming connections. It maintains a lis use open connections. Use the RequestParsed event to receive requests , and similar things.
* HttpStatusCodes provides some utility methods for HTTP status codes, for example to check whether a status code represents an error.

## Core classes

The following classes are needed when more low-level control is required.
The convenience classes above use these core classes to provide their services.

* HttpConnectionStream represents an active connection, either to a client or a server.
It provides functions and events to send and receive messages.

* HttpMessage represents a single HTTP message, which is either a request or a response.
Messages are sent and received over a HttpConnectionStream.

* HttpResponse and HttpRequest are HttpMessages, representing requests and responses.
* HttpParser is an internal helper class that knows how to read messages from a connection stream.
* HttpWriter is an internal helper class that knows how to write messages to a connection stream.
