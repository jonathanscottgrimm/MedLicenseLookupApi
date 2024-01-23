using PuppeteerSharp;

public class BrowserService : IDisposable
{
    private IBrowser _browser;
    private bool _disposed;


    public IBrowser Browser
    {
        get
        {
            if (_disposed) throw new ObjectDisposedException("BrowserService");
            return _browser;
        }
    }
    public async Task InitializeAsync()
    {
        if (_browser != null || _disposed)
            throw new InvalidOperationException("BrowserService is already initialized or disposed.");

        var launchOptions = new LaunchOptions
        {
            Headless = true,
            Args = new[]
        {
            "--no-sandbox"
        }
        };

        _browser = await Puppeteer.LaunchAsync(launchOptions);
    }

    public async Task<IPage> NewPageAsync()
    {
        if (_browser == null)
            throw new InvalidOperationException("BrowserService is not initialized.");

        return await _browser.NewPageAsync();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _browser?.Dispose();
            _disposed = true;
        }
    }
}
