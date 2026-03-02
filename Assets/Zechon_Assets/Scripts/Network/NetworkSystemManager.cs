using UnityEngine;

public class NetworkSessionManager : MonoBehaviour
{
    public static NetworkSessionManager Instance { get; private set; }

    public string JoinCode { get; private set; }
    public string JoinIP { get; private set; }
    public string JoinPort { get; private set; }
    public string NetworkMode { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetSessionData(string code, string ip, string port)
    {
        JoinCode = code;
        JoinIP = ip;
        JoinPort = port;
    }

    public void ClearSession()
    {
        JoinCode = "";
        JoinIP = "";
        JoinPort = "";
        NetworkMode = "";
    }
}