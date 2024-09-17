using CasualNetwork;
using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class Client : MonoBehaviour
{
    private SampleClientSSL clientSSL;
    private SampleClientNOSSL clientNoSSL;
    public TMPro.TMP_InputField bytesField;
    public UnityEvent onConnecting, onConnectedSSL, onConnectedNOSSL, onDisconnected;

    private void Start()
    {
        bytesField.text = PlayerPrefs.GetString("bytesLength", "2000");
    }

   
    public void ConnectSSL()
    {
        int bytesLength = int.Parse(bytesField.text);
        PlayerPrefs.SetString("bytesLength", bytesLength.ToString());
        onConnecting?.Invoke();
        Task.Run(async () =>
        {
            var ip = "34.140.226.0";

            var context = new SslContext(SslProtocols.Tls12 ,(sender, certificate, chain, sslPolicyErrors) => true);

            clientSSL = new SampleClientSSL(this, context, bytesLength, ip, 1234);
            clientSSL.ConnectAsync();
            await WaitUntilTrueAsync(() => clientSSL.IsConnected, 50);

            Printer.Log("Is connected successfully");
            Dispatcher.RunOnMainThread(() => onConnectedSSL?.Invoke());
        });
    }

    public void ConnectNOSSL()
    {
        int bytesLength = int.Parse(bytesField.text);
        PlayerPrefs.SetString("bytesLength", bytesLength.ToString());
        onConnecting?.Invoke();
        Task.Run(async () =>
        {
            var ip = "34.140.226.0";

            clientNoSSL = new SampleClientNOSSL(this, bytesLength, ip, 1235);
            clientNoSSL.ConnectAsync();
            await WaitUntilTrueAsync(() => clientNoSSL.IsConnected, 50);

            Printer.Log("Is connected successfully");
            Dispatcher.RunOnMainThread(() => onConnectedNOSSL?.Invoke());
        });
    }

    public void DisconnectSSL()
    {
        if (clientSSL == null)
            return;

        clientSSL.Disconnect();
        CallDCEVent();
        Printer.Log("Disconnected SSL");
    }

    public void DisconnectNOSSL()
    {
        if (clientNoSSL == null)
            return;

        clientNoSSL.Disconnect();
        CallDCEVent();
        Printer.Log("Disconnected NO SSL");
    }

    private void OnApplicationQuit()
    {
        DisconnectSSL();
        DisconnectNOSSL();
    }

    public void CallDCEVent()
    {
        Dispatcher.RunOnMainThread(() => onDisconnected?.Invoke());
    }

    private async Task WaitUntilTrueAsync(Func<bool> condition, int delayMilliseconds = 100)
    {
        while (!condition())
        {
            await Task.Delay(delayMilliseconds);
        }
    }
}

public static class Printer
{
    public static void Log(object @object)
    {
        Debug.Log($"{DateTime.UtcNow}----{@object}");
    }
    public static void LogError(object @object)
    {
        Debug.LogError($"{DateTime.UtcNow}----{@object}");
    }
}

public class SampleClientSSL : SslClient
{
    private Client client;
    private int bytesLength;
    public SampleClientSSL(Client client, SslContext context, int bytesLength, string address, int port) : base(context, address, port)
    {
        this.client = client;
        this.bytesLength = bytesLength;
    }

    public void DisconnectAndStop()
    {
        DisconnectAsync();
    }

    protected override void OnHandshaked()
    {
        Printer.Log($"SSL client handshaked a new session with Id {Id}");
    }

    protected override void OnConnecting()
    {
        Printer.Log($"Trying to connect to server SSL");
    }

    protected override void OnConnected()
    {
        Printer.Log($"SSL client connected a new session with Id {Id}");

        var data = new byte[bytesLength];

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 1;
        }

        Task.Run(async ()=>
        {
            while (true)
            {
                Send(data, 0 , data.Length);
                await Task.Delay(50);
            }

        });
    }

    protected override void OnDisconnected()
    {
        Printer.Log($"SSLclient disconnected a session with Id {Id}");
        client.CallDCEVent();
    }

    DateTime lastTimePacketReceived = DateTime.Now;
    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        int diff = (int)(DateTime.Now - lastTimePacketReceived).TotalMilliseconds;
        Printer.Log($"Received data {size}, interval time ms: {diff}");
        lastTimePacketReceived = DateTime.Now;
    }

    protected override void OnError(System.Net.Sockets.SocketError error)
    {
        Printer.Log($"SSL client caught an error with code {error}");
    }
}

public class SampleClientNOSSL : TcpClient
{
    private Client client;

    private int bytesLength;
    public SampleClientNOSSL(Client client, int bytesLength, string address, int port) : base(address, port)
    {
        this.client = client;
        this.bytesLength = bytesLength;
    }

    public void DisconnectAndStop()
    {
        DisconnectAsync();
    }

    protected override void OnConnecting()
    {
        Printer.Log($"Trying to connect to server no ssl");
    }

    protected override void OnConnected()
    {
        Printer.Log($"no ssl client connected a new session with Id {Id}");

        var data = new byte[bytesLength];

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 1;
        }

        Task.Run(async () =>
        {
            while (true)
            {
                Send(data, 0, data.Length);
                await Task.Delay(50);
            }

        });
    }

    protected override void OnDisconnected()
    {
        Printer.Log($"no ssl client disconnected a session with Id {Id}");
        client.CallDCEVent();
    }

    DateTime lastTimePacketReceived = DateTime.Now;
    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        int diff = (int)(DateTime.Now - lastTimePacketReceived).TotalMilliseconds;
        Printer.Log($"Received data {size}, interval time ms: {diff}");
        lastTimePacketReceived = DateTime.Now;
    }

    protected override void OnError(System.Net.Sockets.SocketError error)
    {
        Printer.Log($"no ssl client caught an error with code {error}");
    }
}