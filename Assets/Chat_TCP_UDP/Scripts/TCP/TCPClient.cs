using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TCPClient : MonoBehaviour, IClient
{
    private TcpClient tcpClient;
    private NetworkStream networkStream;

    public bool isConnected { get; private set; }

    public event Action<string> OnMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public async Task ConnectToServer(string ip, int port)
    {
        if (isConnected)
            return;

        tcpClient = new TcpClient();

        Debug.Log("[Client] Connecting to server...");

        await tcpClient.ConnectAsync(ip, port);

        networkStream = tcpClient.GetStream();

        isConnected = true;

        Debug.Log("[Client] Connected to server");

        OnConnected?.Invoke();

        _ = ReceiveLoop();
    }

    private async Task ReceiveLoop()
{
    byte[] buffer = new byte[1024];
    StringBuilder messageBuilder = new StringBuilder();

    try
    {
        while (tcpClient != null && tcpClient.Connected)
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

                Debug.Log("[Client] Received: " + message);

                OnMessageReceived?.Invoke(message);
            }
        }
    }
    catch (Exception e)
    {
        Debug.Log("[Client] Error: " + e.Message);
    }
    finally
    {
        Disconnect();
    }
}

    public async Task SendMessageAsync(string message)
{
    if (!isConnected || networkStream == null)
        return;

    message += "\n"; 

    byte[] data = Encoding.UTF8.GetBytes(message);

    await networkStream.WriteAsync(data, 0, data.Length);
}

    

    public void Disconnect()
    {
        isConnected = false;

        networkStream?.Close();
        tcpClient?.Close();

        networkStream = null;
        tcpClient = null;

        Debug.Log("[Client] Disconnected");

        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}