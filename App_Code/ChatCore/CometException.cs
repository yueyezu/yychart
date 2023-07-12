/*
 * 没有什么特殊的异常类。
 * **/
using System;

namespace Commen.ChatCore
{
    /// <summary>
    /// Custom Exception class that contains information about exceptions thrown during
    /// the execution of code within this API
    /// </summary>
    public class CometException : Exception
    {
        /// <summary>
        /// MessageId For the CometClientAlreadyExists Exception
        /// </summary>
        public const int CometClientAlreadyExists = 1;
        /// <summary>
        /// MessageId for the CometClientDoesNotExist Exception
        /// </summary>
        public const int CometClientDoesNotExist = 2;
        /// <summary>
        /// MessageId for the CometHandlerParametersAreInvalid Exception
        /// </summary>
        public const int CometHandlerParametersAreInvalid = 3;

        private int messageId;

        /// <summary>
        /// Construct a new instance of the CometException object
        /// </summary>
        /// <param name="messageId">Specific message Id</param>
        /// <param name="message">Message Description</param>
        /// <param name="args">Arguments used when formatting the message description</param>
        public CometException(int messageId, string message, params object[] args)
            : base(string.Format(message, args))
        {
            this.messageId = messageId;
        }

        /// <summary>
        /// Gets the Specific message id
        /// </summary>
        public int MessageId
        {
            get { return this.messageId; }
        }

        /// <summary>
        /// Returns an exception initialized with the CometClientAlreadyExists exception
        /// </summary>
        /// <returns></returns>
        public static CometException CometClientAlreadyExistsException()
        {
            return new CometException(CometClientAlreadyExists, "CometClient already exists. Either the Private or Public Token is in use.");
        }

        /// <summary>
        /// Returns an exception initialized with the CometClientDoesNotExist Exception
        /// </summary>
        /// <returns></returns>
        public static CometException CometClientDoesNotExistException()
        {
            return new CometException(CometClientDoesNotExist, "CometClient does not exist.");
        }

        /// <summary>
        /// Returns an exception initialized with the CometHandlerParametersAreInvalid Exception
        /// </summary>
        /// <returns></returns>
        internal static CometException CometHandlerParametersAreInvalidException()
        {
            return new CometException(CometHandlerParametersAreInvalid, "Parameters passed to the BeginSubscribe method are invalid.  Please specify lastMessageId (long) and prviateToken (string) in the POST parameters.");
        }
    }
}
