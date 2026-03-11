using SFB;
using System;
using System.IO;
using UnityEngine;
using TMPro;


public class UI_TCPClient : MonoBehaviour
{
    public int serverPort = 5555;
    public string serverAddress = "127.0.0.1";

    [SerializeField] private TCPClient clientReference;
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private ChatUIManager chatUI;

    private IClient _client;

    void Awake()
    {
        if (clientReference == null)
        {
            Debug.LogError("TCPClient reference not assigned!");
            return;
        }

        _client = clientReference;
    }

    void Start()
    {
        if (_client == null)
            return;

        _client.OnMessageReceived += HandleMessageReceived;
        _client.OnConnected += HandleConnection;
        _client.OnDisconnected += HandleDisconnection;

        Invoke(nameof(ConnectClient), 1.5f);
    }

    public void ConnectClient()
    {
        Debug.Log("[UI-Client] Connecting to server...");

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

        string base64 = Convert.ToBase64String(imageBytes);

        string message = "IMG|" + base64;

        await _client.SendMessageAsync(message);

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

    void HandleMessageReceived(string text)
    {
        Debug.Log("[UI-Client] Message received from server");

        if (text.StartsWith("IMG|"))
        {
            string base64 = text.Substring(4);

            byte[] imageBytes = Convert.FromBase64String(base64);

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
        Debug.Log("[UI-Client] Client Disconnect from Server");
    }
}
