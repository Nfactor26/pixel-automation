using RestSharp;
using System;

namespace Pixel.Persistence.Services.Client
{
    public static class RestExtensions
    {
        /// <summary>
        /// Check whether the respnose is successful. If response is not success, throw exception
        /// </summary>
        /// <param name="response"></param>
        public static void EnsureSuccess(this RestResponse response)
        {
            if (!response.IsSuccessful)
            {
                switch(response.StatusCode)
                {
                    case System.Net.HttpStatusCode.Conflict:
                        throw new Exception(response.Content, response.ErrorException);
                    default:
                        throw new Exception(response.ErrorMessage, response.ErrorException);
                }
            }
        }
    }
}
