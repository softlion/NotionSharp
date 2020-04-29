using System;

namespace NotionSharp.Lib.ApiV3.Results
{
    public class NotionErrorResult
    {
        public Guid ErrorId { get; set; }
        /// <summary>
        /// UnauthorizedError
        /// </summary>
        public string Name { get; set; }
        public string Message { get; set; }
        /// <summary>
        /// ExpiredToken
        /// </summary>
        public string Status { get; set; }
    }
}
