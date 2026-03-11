using SFB;
using System;
using System.IO;
using UnityEngine;
using TMPro;
using System.Collections.Generic;


public class UdpClientUI: MonoBehaviour
{
    public int serverPort = 7777;
    public string serverAddress = "127.0.0.1";
    [SerializeField] private UDPClient clientReference;
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private ChatUIManager chatUI;

    private IClient _client;
    void Awake()
    {
        _client = clientReference;
    }
    void Start()
    {
        _client.OnMessageReceived += HandleMessageReceived;
        _client.OnConnected += HandleConnection;
        _client.OnDisconnected += HandleDisconnection;
    }
    public void ConnectClient()
    {
        _client.ConnectToServer(serverAddress, serverPort);
    }
   public async void SendClientMessage()
    {
        if (_client == null)
        {
            Debug.LogError("Client reference missing");
            return;
        }

        if (!_client.isConnected)
        {
            Debug.Log("The client is not connected");
            return;
        }

        if (messageInput == null)
        {
            Debug.LogError("Message Input not assigned");
            return;
        }

        if (messageInput.text == "")
        {
            Debug.Log("The chat entry is empty");
            return;
        }

        string message = messageInput.text;

        await _client.SendMessageAsync(message);

        if (chatUI != null)
        {
            chatUI.AddMessage(message, true);
        }

        messageInput.text = "";
    }

     public async void SendImage(string path)
{
    byte[] imageBytes = File.ReadAllBytes(path);

    int maxSize = 3; 

    if (imageBytes.Length > maxSize)
    {
        Debug.Log("Image too large for UDP");

        if (chatUI != null)
        {
            float kb = imageBytes.Length / 1024f;
            chatUI.AddMessage($"The image could not be sent ({kb:F1} KB). too large for UDP.", true);
        }

        return; 
    }


    int chunkSize = 1000;
    int totalChunks = Mathf.CeilToInt((float)imageBytes.Length / chunkSize);

    await _client.SendMessageAsync("IMG_START|" + totalChunks);

    for (int i = 0; i < totalChunks; i++)
    {
        int start = i * chunkSize;
        int length = Mathf.Min(chunkSize, imageBytes.Length - start);

        byte[] chunk = new byte[length];
        Array.Copy(imageBytes, start, chunk, 0, length);

        string base64 = Convert.ToBase64String(chunk);

        string message = "IMG_PART|" + i + "|" + base64;

        await _client.SendMessageAsync(message);
    }

    await _client.SendMessageAsync("IMG_END");

    chatUI.AddImage(imageBytes, true);
}

    public async void SendPDF(string path)
{
    string fileName = Path.GetFileName(path);

    string message = "PDF|" + fileName;

    await _client.SendMessageAsync(message);

    chatUI.AddPDF(fileName, true);
}

    public void SelectImage()
    {
        var extensions = new[]
        {
        new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
        new ExtensionFilter("PNG", "png"),
        new ExtensionFilter("JPG", "jpg", "jpeg")
    };

        var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", extensions, false);

        if (paths.Length > 0)
        {
            SendImage(paths[0]);
        }
    }

    public void SelectPDF()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(
            "Select PDF",
            "",
            "pdf",
            false
        );

        if (paths.Length > 0)
        {
            SendPDF(paths[0]);
        }
    }

List<byte> imageBuffer = new List<byte>();

    void HandleMessageReceived(string text)
{
    Debug.Log("[UI-Client] Message received from server");

    if (text.StartsWith("IMG_START"))
    {
        imageBuffer.Clear();
    }
    else if (text.StartsWith("IMG_PART"))
    {
        string[] parts = text.Split('|');

        byte[] chunk = Convert.FromBase64String(parts[2]);

        imageBuffer.AddRange(chunk);
    }
    else if (text.StartsWith("IMG_END"))
    {
        byte[] imageBytes = imageBuffer.ToArray();

        chatUI.AddImage(imageBytes, false);
    }
    else if (text.StartsWith("PDF|"))
    {
        string fileName = text.Substring(4);

        chatUI.AddPDF(fileName,false);
    }
    else
    {
        chatUI.AddMessage(text, false);
    }
}

void HandleConnection()
{
    Debug.Log("[UI-Client] Client Connected to Server");
}

void HandleDisconnection()
{
    Debug.Log("[UI-Client] Client Disconnected from Server");
}
}
