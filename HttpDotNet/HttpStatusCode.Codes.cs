using System;

namespace HttpDotNet
{
    public static partial class StatusCodes
    {
        public static int GetCodeNumber(string status)
        {
            if(String.IsNullOrEmpty(status)
                || status.Length < 3
                || status[0] < '0' || status[0] > '9'
                || status[1] < '0' || status[1] > '9'
                || status[2] < '0' || status[2] > '9')
            {
                return -1;
            }
            else
            {
                //Performance-optimized code parsing
                int d1 = status[0] - '0';
                int d2 = status[1] - '0';
                int d3 = status[2] - '0';
                return d1*100 + d2*10 + d3;
            }
        }
        
        public static bool IsSuccess(int code) => code >= 100 && code < 400;
        public static bool IsSuccess(string status) => IsSuccess(GetCodeNumber(status));
        public static bool IsRedirect(int code) => code >= 300 && code < 400;
        public static bool IsRedirect(string status) => IsRedirect(GetCodeNumber(status));
        public static bool IsError(int code) => code >= 400 && code < 600;
        public static bool IsError(string status) => IsError(GetCodeNumber(status));
        public static bool IsClientError(int code) => code >= 400 && code < 500;
        public static bool IsClientError(string status) => IsClientError(GetCodeNumber(status));
        public static bool IsServerError(int code) => code >= 500 && code < 600;
        public static bool IsServerError(string status) => IsServerError(GetCodeNumber(status));
        public static bool IsValidResponse(int code) => code >= 100 && code < 500;
        public static bool IsValidResponse(string status) => IsValidResponse(GetCodeNumber(status));
    }
}