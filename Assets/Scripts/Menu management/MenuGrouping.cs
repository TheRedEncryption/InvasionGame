using UnityEngine;

public class MenuGrouping : MonoBehaviour
{
    public delegate void MenuStackEvent();

    public event MenuStackEvent OnDisplay;
    public event MenuStackEvent OnHide;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void Display()
    {
        gameObject.SetActive(true);
        OnDisplay?.Invoke();
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
        OnHide?.Invoke();
    }
}