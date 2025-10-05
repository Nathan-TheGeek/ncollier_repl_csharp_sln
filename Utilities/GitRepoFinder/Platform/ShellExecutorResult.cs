namespace GitRepoFinder.Platform;

public class ShellExecutorResult
{
    public ShellExecutorResult(int ExitCode, string StandardOutput, string StandardError)
    {
        this.ExitCode = ExitCode;
        this.StandardOutput = StandardOutput;
        this.StandardError = StandardError;
    }
    public int ExitCode { get; set; }
    public string StandardOutput { get; set; }
    public string StandardError { get; set; }
}