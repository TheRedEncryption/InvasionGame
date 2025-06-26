using UnityEngine;
using UnityEngine.UI;

public class ToggleMenu : MonoBehaviour
{

    [SerializeField] private Toggle toggle;
    [SerializeField] private GameObject buildingPanel;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnToggleChanged(bool toggleValue)
    {
        buildingPanel.SetActive(toggleValue);
    }
}
