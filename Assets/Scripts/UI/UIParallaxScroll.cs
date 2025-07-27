using UnityEngine;

public class UIParallaxScroll : MonoBehaviour
{

    private RectTransform rectTransform;

    public float scrollSpeed;
    public float resetThreshold;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = rectTransform.anchoredPosition;
        float newY = pos.y + (scrollSpeed * Time.deltaTime);
        if (newY > resetThreshold)
        {
            newY %= resetThreshold;
        }
        rectTransform.anchoredPosition = new Vector2(pos.x, newY);
    }
}
