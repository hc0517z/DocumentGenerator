namespace SpsDocumentGenerator.Base
{
    public class Result
    {
        public bool IsSuccess => !IsFailure;

        public bool IsFailure { get; protected set; }

        public string ErrorMessage { get; protected set; }

        public static Result Success()
        {
            return new Result { IsFailure = false };
        }

        public static Result Failure(string errorMessage)
        {
            return new Result { IsFailure = true, ErrorMessage = errorMessage };
        }
    }
}
