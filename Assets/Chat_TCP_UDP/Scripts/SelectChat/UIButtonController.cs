using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class UIButtonSceneLoader : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scene Settings")]
    public int sceneIndex;

    [Header("Hover Effect")]
    public float scaleMultiplier = 1.1f;
    public float smoothSpeed = 10f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * smoothSpeed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * scaleMultiplier;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    public void LoadScene()
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("Invalid scene index. Scene not found in Build Settings.");
            return;
        }

        SceneManager.LoadScene(sceneIndex);
    }
}