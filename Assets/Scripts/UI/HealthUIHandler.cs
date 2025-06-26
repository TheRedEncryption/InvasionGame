using UnityEngine;
using UnityEngine.UIElements;

public class HealthUIHandler : MonoBehaviour
{
    public Entity PlayerEntity;
    public UIDocument UIDoc;
    private VisualElement root;
    private Label healthLabel;
    private VisualElement healthBarMask;
    private void Start()
    {
        AssociateWithPlayer();
        if (UIDoc != null)
        {
            root = UIDoc.rootVisualElement;
            healthLabel = root.Q<Label>("HealthLabel");
            healthBarMask = UIDoc.rootVisualElement.Q<VisualElement>("HealthBarMask");
        }
    }
    private void OnEnable()
    {
        AssociateWithPlayer();
    }
    public void AssociateWithPlayer()
    {
        if (PlayerEntity != null)
        {
            PlayerEntity.HealthChanged += OnEntityHealthChanged;
            UpdateHealthUI();
        }
    }

    private void OnEntityHealthChanged(object sender, Entity.HealthEventArgs e)
    {
        // Update your UI here
        if (UIDoc != null)
        {
            if (healthLabel != null)
            {
                healthLabel.text = $"{e.healthArg}/{e.maxHealthArg}";
                float healthRatio = (float)e.healthArg / e.maxHealthArg;
                float healthPercent = Mathf.Lerp(19, 93, healthRatio);
                healthBarMask.style.width = Length.Percent(healthPercent);
            }
            else
            {
                Debug.LogWarning("HealthLabel not found in the UI document.");
            }
        }
        else
        {
            Debug.LogWarning("UIDocument is not assigned.");
        }
    }

    public void UpdateHealthUI()
    {
        var playerHealth = PlayerEntity.Health.CurrValue;
        var playerMaxHealth = PlayerEntity.Health.MaxValue;
        healthLabel.text = $"{playerHealth}/{playerMaxHealth}";
        float healthRatio = (float)playerHealth / playerMaxHealth;
        float healthPercent = Mathf.Lerp(19, 93, healthRatio);
        healthBarMask.style.width = Length.Percent(healthPercent);
        PlayerEntity.HealthChanged += OnEntityHealthChanged;
    }
}
