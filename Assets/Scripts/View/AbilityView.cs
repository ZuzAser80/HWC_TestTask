using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class AbilityView : NetworkBehaviour { 

    [SerializeField] private List<TextMeshProUGUI> abilitiesTexts = new List<TextMeshProUGUI>();

    public override void OnStartAuthority() => abilitiesTexts.ForEach(x => x.gameObject.SetActive(true));

    public void DisplayTexts(List<Ability> abilities) =>
        abilitiesTexts.ForEach(x => { 
            if (abilitiesTexts.IndexOf(x) < abilities.Count) {
                x.text = abilities[abilitiesTexts.IndexOf(x)].cd.ToString(); 
        }}); 
}