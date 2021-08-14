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
        public static void EnsureSuccess(this IRestResponse response)
        {
            if (!response.IsSuccessful)
            {
                throw new Exception(response.ErrorMessage, response.ErrorException);
            }
        }
    }
}
