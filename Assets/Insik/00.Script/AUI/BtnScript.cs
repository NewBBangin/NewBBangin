using UnityEngine;
using UnityEngine.UI;

public class BtnScript : MonoBehaviour
{


    public void StartNewGame()
    {
        Debug.Log("New Game Start");
    }

    public void LoadSaveFile()
    {
        Debug.Log("Sav File loaded");
    }

    public void QuitButtenClick()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

    public void HidePannel()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
