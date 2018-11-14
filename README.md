# HttpDotNet

A minimal HTTP server and client in pure managed .NET.

[![Build status](https://ci.appveyor.com/api/projects/status/hw8ag5y7txnpfjxv?svg=true)](https://ci.appveyor.com/project/LukasBoersma/httpdotnet)

## Classes

## Convenience classes

The following classes provide an easy-to-use interface that will cover most use cases.

* HttpClient maintains a single connection to a server. Use it to make HTTP requests to a server.
* HttpListener is a HTTP server that waits for incoming connections. It maintains a lis use open connections. Use the RequestParsed event to receive requests , and similar things.
* HttpStatusCodes

## Core classes

The following classes are needed when more low-level control is required.
The convenience classes above use these core classes to provide their services.

* HttpConnection represents an active connection, either to a client or a server.
It provides functions and events to send and receive messages.

* HttpMessage represents a single HTTP message, which is either a request or a response.
Messages are sent and received over a HttpConnection.

* HttpResponse and HttpRequest are HttpMessages, representing requests and responses.
* HttpParser is an internal helper class that knows how to read messages from a connection stream.
* HttpWriter is an internal helper class that knows how to write messages to a connection stream.