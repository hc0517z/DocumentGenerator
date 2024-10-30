namespace SpsDocumentGenerator.Base
{
    public class Result<T> : Result
    {
        public T Value { get; private set; }
        
        public static Result<T> Success(T value)
        {
            return new Result<T> { IsFailure = false, Value = value };
        }
        
        public new static Result<T> Failure(string errorMessage)
        {
            return new Result<T> { IsFailure = true, ErrorMessage = errorMessage };
        }
    }
}
