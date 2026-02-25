using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class NetworkBootstrap : MonoBehaviour
{
    private static bool _initialized = false;

    private async void Awake()
    {
        if (_initialized)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        _initialized = true;

        await InitializeServices();

        SceneManager.LoadScene("Main_Menu");
    }

    private async Task InitializeServices()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Authenticated as: " + AuthenticationService.Instance.PlayerId);
        }
    }
}