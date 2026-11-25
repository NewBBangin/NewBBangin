using UnityEngine;
using UnityEngine.UI;

public class BtnScript : MonoBehaviour
{
    [SerializeField] private Image _fadingPanel;

    private void Awake()
    {
        _fadingPanel.GetComponent<Image>();
    }

    private void OnEnable()
    {
        _fadingPanel.color = new Vector4(0, 0, 0, 0);
    }

}
