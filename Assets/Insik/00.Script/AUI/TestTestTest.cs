using UnityEngine;
using UnityEngine.InputSystem;

public class TestTestTest : MonoBehaviour
{
    bool isActive;
    [SerializeField] private GameObject settingPanel;

    private void Awake()
    {
        settingPanel.SetActive(isActive);
    }

    private void Update()
    {
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            isActive = !isActive;
            settingPanel.SetActive(isActive);
        }
    }

}
