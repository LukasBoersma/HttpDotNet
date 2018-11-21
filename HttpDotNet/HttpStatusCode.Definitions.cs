using System.Collections.ObjectModel;
using System.Collections.Generic;
namespace HttpDotNet
{
    public static partial class HttpStatusCodes
    {
        public static readonly string Continue = "100 Continue";
        public static readonly string SwitchingProtocols = "101 Switching Protocols";
        public static readonly string Processing = "102 Processing";
        public static readonly string EarlyHints = "103 Early Hints";
        public static readonly string OK = "200 OK";
        public static readonly string Created = "201 Created";
        public static readonly string Accepted = "202 Accepted";
        public static readonly string NonAuthoritativeInformation = "203 Non-Authoritative Information";
        public static readonly string NoContent = "204 No Content";
        public static readonly string ResetContent = "205 Reset Content";
        public static readonly string PartialContent = "206 Partial Content";
        public static readonly string MultiStatus = "207 Multi-Status";
        public static readonly string AlreadyReported = "208 Already Reported";
        public static readonly string IMUsed = "226 IM Used";
        public static readonly string MultipleChoices = "300 Multiple Choices";
        public static readonly string MovedPermanently = "301 Moved Permanently";
        public static readonly string Found = "302 Found";
        public static readonly string SeeOther = "303 See Other";
        public static readonly string NotModified = "304 Not Modified";
        public static readonly string UseProxy = "305 Use Proxy";
        public static readonly string TemporaryRedirect = "307 Temporary Redirect";
        public static readonly string PermanentRedirect = "308 Permanent Redirect";
        public static readonly string BadRequest = "400 Bad Request";
        public static readonly string Unauthorized = "401 Unauthorized";
        public static readonly string PaymentRequired = "402 Payment Required";
        public static readonly string Forbidden = "403 Forbidden";
        public static readonly string NotFound = "404 Not Found";
        public static readonly string MethodNotAllowed = "405 Method Not Allowed";
        public static readonly string NotAcceptable = "406 Not Acceptable";
        public static readonly string ProxyAuthenticationRequired = "407 Proxy Authentication Required";
        public static readonly string RequestTimeout = "408 Request Timeout";
        public static readonly string Conflict = "409 Conflict";
        public static readonly string Gone = "410 Gone";
        public static readonly string LengthRequired = "411 Length Required";
        public static readonly string PreconditionFailed = "412 Precondition Failed";
        public static readonly string PayloadTooLarge = "413 Payload Too Large";
        public static readonly string URITooLong = "414 URI Too Long";
        public static readonly string UnsupportedMediaType = "415 Unsupported Media Type";
        public static readonly string RangeNotSatisfiable = "416 Range Not Satisfiable";
        public static readonly string ExpectationFailed = "417 Expectation Failed";
        public static readonly string MisdirectedRequest = "421 Misdirected Request";
        public static readonly string UnprocessableEntity = "422 Unprocessable Entity";
        public static readonly string Locked = "423 Locked";
        public static readonly string FailedDependency = "424 Failed Dependency";
        public static readonly string UpgradeRequired = "426 Upgrade Required";
        public static readonly string PreconditionRequired = "428 Precondition Required";
        public static readonly string TooManyRequests = "429 Too Many Requests";
        public static readonly string RequestHeaderFieldsTooLarge = "431 Request Header Fields Too Large";
        public static readonly string UnavailableForLegalReasons = "451 Unavailable For Legal Reasons";
        public static readonly string InternalServerError = "500 Internal Server Error";
        public static readonly string NotImplemented = "501 Not Implemented";
        public static readonly string BadGateway = "502 Bad Gateway";
        public static readonly string ServiceUnavailable = "503 Service Unavailable";
        public static readonly string GatewayTimeout = "504 Gateway Timeout";
        public static readonly string HTTPVersionNotSupported = "505 HTTP Version Not Supported";
        public static readonly string VariantAlsoNegotiates = "506 Variant Also Negotiates";
        public static readonly string InsufficientStorage = "507 Insufficient Storage";
        public static readonly string LoopDetected = "508 Loop Detected";
        public static readonly string NotExtended = "510 Not Extended";
        public static readonly string NetworkAuthenticationRequired = "511 Network Authentication Required";

        public static readonly ReadOnlyDictionary<int, string> ByCode = new ReadOnlyDictionary<int, string>(new Dictionary<int, string>()
        {
            {100, Continue},
            {101, SwitchingProtocols},
            {102, Processing},
            {103, EarlyHints},
            {200, OK},
            {201, Created},
            {202, Accepted},
            {203, NonAuthoritativeInformation},
            {204, NoContent},
            {205, ResetContent},
            {206, PartialContent},
            {207, MultiStatus},
            {208, AlreadyReported},
            {226, IMUsed},
            {300, MultipleChoices},
            {301, MovedPermanently},
            {302, Found},
            {303, SeeOther},
            {304, NotModified},
            {305, UseProxy},
            {307, TemporaryRedirect},
            {308, PermanentRedirect},
            {400, BadRequest},
            {401, Unauthorized},
            {402, PaymentRequired},
            {403, Forbidden},
            {404, NotFound},
            {405, MethodNotAllowed},
            {406, NotAcceptable},
            {407, ProxyAuthenticationRequired},
            {408, RequestTimeout},
            {409, Conflict},
            {410, Gone},
            {411, LengthRequired},
            {412, PreconditionFailed},
            {413, PayloadTooLarge},
            {414, URITooLong},
            {415, UnsupportedMediaType},
            {416, RangeNotSatisfiable},
            {417, ExpectationFailed},
            {421, MisdirectedRequest},
            {422, UnprocessableEntity},
            {423, Locked},
            {424, FailedDependency},
            {426, UpgradeRequired},
            {428, PreconditionRequired},
            {429, TooManyRequests},
            {431, RequestHeaderFieldsTooLarge},
            {451, UnavailableForLegalReasons},
            {500, InternalServerError},
            {501, NotImplemented},
            {502, BadGateway},
            {503, ServiceUnavailable},
            {504, GatewayTimeout},
            {505, HTTPVersionNotSupported},
            {506, VariantAlsoNegotiates},
            {507, InsufficientStorage},
            {508, LoopDetected},
            {510, NotExtended},
            {511, NetworkAuthenticationRequired},
        });

    }
}