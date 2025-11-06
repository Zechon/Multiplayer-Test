using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneListDebugger : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Scenes in build:");
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            Debug.Log($"{i}: {path}");
        }
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            Debug.Log($"[Netcode] Client connected: {id}");
        };
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            Debug.Log("[Netcode] Server started!");
        };
    }
}