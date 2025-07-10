namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestSignalRService
{
    public bool IsConnected { get; private set; }
    public List<string> ReceivedMessages { get; } = new();

    public async Task ConnectAsync()
    {
        IsConnected = true;
    }

    public async Task DisconnectAsync()
    {
        IsConnected = false;
    }

    public void SimulateMessage(string message)
    {
        ReceivedMessages.Add(message);
    }
}