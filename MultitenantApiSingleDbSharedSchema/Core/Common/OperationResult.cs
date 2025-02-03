namespace MultitenantApiSingleDbSharedSchema.Core.Common;

public class OperationResult
{
    private static readonly IReadOnlyList<OperationResultError> EmptyErrors = new List<OperationResultError>().AsReadOnly();

    public bool Succeeded { get; }
    public IReadOnlyList<OperationResultError> Errors { get; }

    private OperationResult(bool succeeded, IEnumerable<OperationResultError>? errors)
    {
        Succeeded = succeeded;
        Errors = errors?.ToList().AsReadOnly() ?? EmptyErrors;
    }

    public static OperationResult Success() => new(true, null);
    public static OperationResult Failure(IEnumerable<OperationResultError> errors) => new(false, errors);
}

public class OperationResultError
{
    public string Code { get; set; } = null!;
    
    public string Description { get; set; } = null!;
}

