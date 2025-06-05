using UnityEngine;
using UnityEngine.UIElements;

public class HealthUIHandler : MonoBehaviour
{
    public Entity PlayerEntity;
    public UIDocument UIDoc;
    private VisualElement root;
    private Label healthLabel;
    private void Start()
    {
        if (PlayerEntity != null)
        {
            PlayerEntity.HealthChanged += OnEntityHealthChanged;
        }
        if (UIDoc != null)
        {
            root = UIDoc.rootVisualElement;
            healthLabel = root.Q<Label>("HealthLabel");
        }
    }

    private void OnEntityHealthChanged(object sender, Entity.HealthEventArgs e)
    {
        Debug.Log("Entity health changed! New health: " + e.healthArg);
        // Update your UI here
        if (UIDoc != null)
        {
            if (healthLabel != null)
            {
                healthLabel.text = $"{e.healthArg}/{PlayerEntity.Health.MaxValue}";
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
        healthLabel.text = $"{PlayerEntity.Health.CurrValue}/{PlayerEntity.Health.MaxValue}";
    }
}
