using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ChatUIManager : MonoBehaviour
{
    public GameObject messageLeftPrefab;
    public GameObject messageRightPrefab;
    public GameObject imagePrefab;
    public GameObject pdfPrefab;

    public Transform content;

    [SerializeField] private ScrollRect scrollRect;

    public void AddMessage(string message, bool isClient)
    {
        if (content == null)
        {
            Debug.LogError("Content not assigned in ChatUIManager");
            return;
        }

        GameObject prefab = isClient ? messageRightPrefab : messageLeftPrefab;

        if (prefab == null)
        {
            Debug.LogError("Message prefab missing");
            return;
        }

        GameObject newMessage = Instantiate(prefab, content);

        TMP_Text text = newMessage.GetComponentInChildren<TMP_Text>();

        if (text != null)
        {
            text.text = message;
        }
        else
        {
            Debug.LogError("TMP_Text not found inside prefab");
        }

        Canvas.ForceUpdateCanvases();
        StartCoroutine(ScrollToBottom());
    }

    public void AddImage(byte[] imageData, bool isClient)
{
    GameObject imgObj = Instantiate(imagePrefab, content);

    Texture2D tex = new Texture2D(2,2);
    tex.LoadImage(imageData);

    Sprite sprite = Sprite.Create(
        tex,
        new Rect(0,0,tex.width,tex.height),
        new Vector2(0.5f,0.5f)
    );

    Image img = imgObj.GetComponent<Image>();
    img.sprite = sprite;

    RectTransform rt = img.GetComponent<RectTransform>();
    rt.sizeDelta = new Vector2(200, 200);

    Canvas.ForceUpdateCanvases();
    StartCoroutine(ScrollToBottom());
}

    public void AddPDF(string fileName, bool isClient)
{
    GameObject newPDF = Instantiate(pdfPrefab, content);

    TMP_Text text = newPDF.GetComponentInChildren<TMP_Text>();

    text.text = "" + fileName;

    Canvas.ForceUpdateCanvases();
    StartCoroutine(ScrollToBottom());
}


    IEnumerator ScrollToBottom()
    {
        yield return null;

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}