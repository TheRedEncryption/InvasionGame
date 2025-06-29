using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerBootstrap : MonoBehaviour
{
    void Start()
    {
        // figure out if server, if not, return immediately
#if !UNITY_SERVER
        Debug.Log("[ServerBootstrap]: You are running as a client!");
        SceneManager.LoadScene(1); // replace with name of MENU scene
        return;
#else
        // the rest of the code will only run if this is a server
        Debug.Log("[ServerBootstrap]: You are running as a server!");
        SceneManager.LoadScene(1); // replace with name of GAMELOOP scene
#endif
    }
}
