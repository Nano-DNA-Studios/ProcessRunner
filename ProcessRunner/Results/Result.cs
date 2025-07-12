namespace NanoDNA.ProcessRunner.Results
{
    /// <summary>
    /// Represents the result of an operation, provides a way to encapsulate the content and an optional message.
    /// </summary>
    /// <typeparam name="T">The Type of the Content to include in the Result</typeparam>
    public class Result<T> where T : class
    {
        /// <summary>
        /// Gets the message associated with the process execution result, if any.
        /// </summary>
        public string? Message { get; }

        /// <summary>
        /// Content or Additional Informaton of the Result, which can be any type specified by the generic parameter T.
        /// </summary>
        public T Content { get; }

        /// <summary>
        /// Initializes a new Instance of <see cref="Result{T}"/>, populates optional content and message properties.
        /// </summary>
        /// <param name="content">Content or Additional Information from the Result</param>
        /// <param name="message">Optional Message provifing context for the Result</param>
        public Result(T content, string? message = null)
        {
            Content = content;
            Message = message;
        }
    }
}
