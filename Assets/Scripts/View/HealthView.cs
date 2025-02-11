using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthView : NetworkBehaviour { 
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider healthSlider;
    
    [Client]
    public void DisplayHealth(int value) {
        healthText.text = value.ToString(); 
        healthSlider.value = value;
    }
}