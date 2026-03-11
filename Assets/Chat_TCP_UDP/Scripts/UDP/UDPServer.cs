using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UDPServer : MonoBehaviour, IServer
{
    private UdpClient udpServer;
    private IPEndPoint remoteEndPoint;

    public event Action<string> OnMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public bool isServerRunning { get; private set; }

    public Task StartServer(int port)
    {
        udpServer = new UdpClient(port);

        Debug.Log("[UDP Server] Server started. Waiting for messages...");

        isServerRunning = true;

        _ = ReceiveLoop();

        return Task.CompletedTask;
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (isServerRunning)
            {
                UdpReceiveResult result = await udpServer.ReceiveAsync();

                remoteEndPoint = result.RemoteEndPoint;

                string message = Encoding.UTF8.GetString(result.Buffer);

                Debug.Log("[UDP Server] Received: " + message);

                if (message == "CONNECT")
                {
                    OnConnected?.Invoke();
                    continue;
                }

                OnMessageReceived?.Invoke(message);
            }
        }
        catch (Exception e)
        {
            Debug.Log("[UDP Server] Error: " + e.Message);
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (!isServerRunning || remoteEndPoint == null)
        {
            Debug.Log("[UDP Server] No client connected");
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(message);

        await udpServer.SendAsync(data, data.Length, remoteEndPoint);

        Debug.Log("[UDP Server] Sent: " + message);
    }

    public void Disconnect()
    {
        if (!isServerRunning)
        {
            Debug.Log("[UDP Server] Server is not running");
            return;
        }

        isServerRunning = false;

        udpServer?.Close();
        udpServer = null;

        Debug.Log("[UDP Server] Disconnected");

        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}
