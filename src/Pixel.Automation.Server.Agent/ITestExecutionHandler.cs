namespace pixel_execution_managers.Handlers;

/// <summary>
/// A handler to start execution of test cases
/// </summary>
internal interface ITestExecutionHandler
{
    /// <summary>
    /// Name of the test execution handler. Triggers should be configured to use required handlers.
    /// </summary>
    string Name { get; }
  
    /// <summary>
    /// Execute the test case for a given template by starting pixel-run.exe with required parameters
    /// </summary>
    /// <param name="templateName"></param>
    /// <returns></returns>
    Task ExecuteTestAsync(string templateName);
}
