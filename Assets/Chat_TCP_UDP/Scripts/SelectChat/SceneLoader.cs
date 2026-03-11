using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private int sceneIndex; 

    void Start()
    {
        SceneManager.LoadScene(sceneIndex, LoadSceneMode.Additive);
    }
}