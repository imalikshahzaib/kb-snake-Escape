namespace ArrowMaze.Common
{
    public sealed class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T Value { get; private set; }
        public string Error { get; private set; }

        private Result(bool isSuccess, T value, string error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, string.Empty);
        }

        public static Result<T> Failure(string error)
        {
            return new Result<T>(false, default(T), error);
        }
    }
}
