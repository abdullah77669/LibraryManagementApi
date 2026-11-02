namespace LMS_APIs.Repository
{
    public record ServiceResult<T>(T? Value, int ErrorStatus = 0, string? Error = null)
    {
        public bool Succeeded => ErrorStatus == 0;
    }
}
