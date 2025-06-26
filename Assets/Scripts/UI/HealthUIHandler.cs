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
        Debug.Log("HealthUIHandler enabled, associating with PlayerEntity.");
        AssociateWithPlayer();
    }
    public void AssociateWithPlayer()
    {
        if (PlayerEntity != null)
        {
            Debug.Log("Associating HealthUIHandler with PlayerEntity: " + PlayerEntity.name);
            PlayerEntity.HealthChanged += OnEntityHealthChanged;
            UpdateHealthUI();
        }
        else
        {
            Debug.LogError("PlayerEntity is not assigned.");
        }
    }

    private void OnEntityHealthChanged(object sender, Entity.HealthEventArgs e)
    {
        // Update your UI here
        if (UIDoc != null)
        {
            if (healthLabel != null)
            {
                Debug.Log($"Updating health UI (via changed): {e.healthArg}/{e.maxHealthArg}");
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
        Debug.Log($"Updating health UI (via force): {playerHealth}/{playerMaxHealth}");
        healthLabel.text = $"{playerHealth}/{playerMaxHealth}";
        float healthRatio = (float)playerHealth / playerMaxHealth;
        float healthPercent = Mathf.Lerp(19, 93, healthRatio);
        healthBarMask.style.width = Length.Percent(healthPercent);
        PlayerEntity.HealthChanged += OnEntityHealthChanged;
    }
}
