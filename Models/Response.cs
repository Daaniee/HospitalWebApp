using System.ComponentModel.DataAnnotations;

namespace hospitalwebapp.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }           // Indicates if the request was successful
        public int StatusCode { get; set; }         // HTTP status code
        public string Message { get; set; }
        public T Data { get; set; }              // Additional data
        public ApiResponse()
        { } 
        public ApiResponse(bool success, int statusCode, string message, T data)
        {
            Success = success;
            StatusCode = statusCode;
            Message = message;
            Data = data;
        }
    }

    public class ApiResponseNoData
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public ApiResponseNoData()
        {  }
        public ApiResponseNoData(bool success, int statusCode, string message)
        {
            Success = success;
            StatusCode = statusCode;
            Message = message;
        }
    }
}