using Mirror;
using TMPro;
using UnityEngine;

public class StatusEffectView : NetworkBehaviour { 
    public StatusEffect effect;
    public TextMeshProUGUI text;
    
    [Client]
    public void DisplayEffect(StatusEffect statusEffect) {
        gameObject.SetActive(statusEffect.TurnsLeft > 1);
        effect = statusEffect;
        text.text = (effect.TurnsLeft - 1).ToString();
    }

    [Client] public void Disable() => gameObject.SetActive(false);
}