namespace OrderBookClientHW.Services;

public class ConnectionStatusService
{
    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            _isConnected = value;
            NotifyStateChanged();
        }
    }

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}