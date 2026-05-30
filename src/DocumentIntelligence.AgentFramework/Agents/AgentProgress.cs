namespace DocumentIntelligence.AgentFramework.Agents;

public static class AgentProgress
{
    private static readonly AsyncLocal<IProgress<string>?> _stepReporter = new();

    public static async Task<T> WithProgressAsync<T>(IProgress<string> progress, Func<Task<T>> action)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(action);

        IProgress<string>? previous = _stepReporter.Value;
        _stepReporter.Value = progress;
        try
        {
            return await action().ConfigureAwait(false);
        }
        finally
        {
            _stepReporter.Value = previous;
        }
    }

    internal static void Report(string description)
    {
        _stepReporter.Value?.Report(description);
    }
}
