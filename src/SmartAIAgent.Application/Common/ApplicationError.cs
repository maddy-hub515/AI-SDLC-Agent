namespace SmartAIAgent.Application.Common;

public sealed class ApplicationError : Exception
{
    public ApplicationError(string code, string message) : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
