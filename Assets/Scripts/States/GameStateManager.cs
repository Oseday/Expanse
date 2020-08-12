using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static void ConnectToServer(){
        Client.instance.ConnectToServer();
        SceneManager.LoadScene("Workspace");
    } 

    private void Awake() {
    }

    private void Start() {
        Input.simulateMouseWithTouches = false;

        if (SystemInfo.deviceType==DeviceType.Handheld){
            int newH = 900;
            Screen.SetResolution((int)(newH*Screen.width/Screen.height), newH, FullScreenMode.ExclusiveFullScreen);
        }
    }
}
