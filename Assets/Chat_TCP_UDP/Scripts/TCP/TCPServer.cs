using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TCPServer : MonoBehaviour, IServer
{
    private TcpListener tcpListener;
    private TcpClient connectedClient;
    private NetworkStream networkStream;

    public bool isServerRunning { get; private set; }

    public event Action<string> OnMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public async Task StartServer(int port)
    {
        if (isServerRunning)
            return;

        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();

        isServerRunning = true;

        Debug.Log("[Server] Waiting for client...");

        connectedClient = await tcpListener.AcceptTcpClientAsync();

        Debug.Log("[Server] Client connected");

        networkStream = connectedClient.GetStream();

        OnConnected?.Invoke();

        _ = ReceiveLoop();
    }

    private async Task ReceiveLoop()
    {
        byte[] buffer = new byte[1024];
        StringBuilder messageBuilder = new StringBuilder();

        try
        {
            while (connectedClient != null && connectedClient.Connected)
            {
                int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                    break;

                string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                messageBuilder.Append(chunk);

                while (messageBuilder.ToString().Contains("\n"))
                {
                    string fullMessage = messageBuilder.ToString();
                    int index = fullMessage.IndexOf("\n");

                    string message = fullMessage.Substring(0, index);

                    messageBuilder.Remove(0, index + 1);

                    Debug.Log("[Server] Received: " + message);

                    OnMessageReceived?.Invoke(message);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("[Server] Error: " + e.Message);
        }
        finally
        {
            Disconnect();
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (connectedClient == null || !connectedClient.Connected || networkStream == null)
        {
            Debug.Log("[Server] No client connected");
            return;
        }

        message += "\n";

        byte[] data = Encoding.UTF8.GetBytes(message);

        await networkStream.WriteAsync(data, 0, data.Length);
    }

    public void Disconnect()
    {
        networkStream?.Close();
        connectedClient?.Close();
        tcpListener?.Stop();

        networkStream = null;
        connectedClient = null;
        tcpListener = null;

        isServerRunning = false;

        Debug.Log("[Server] Disconnected");

        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}
