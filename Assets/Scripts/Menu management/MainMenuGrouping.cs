using UnityEngine;

public class MainMenuGrouping : MenuGrouping
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public override void Hide()
    {
        Application.Quit();
    }
}
