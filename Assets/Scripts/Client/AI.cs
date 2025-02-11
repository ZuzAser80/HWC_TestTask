using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class AI : NetworkBehaviour {
    public List<Ability> abilities = new List<Ability>();
    [SerializeField] public HealthManager healthManager;

    public void SimulateTurn() {
        if (((TNetworkManager)NetworkManager.singleton).turn == 0) return;
        healthManager.ApplyStatusEffects();
        healthManager.UpdateStatusEffects();
        UseAbilityCmd(Random.Range(0, 5));
    }

    [Command]
    public void UseAbilityCmd(int index) { 
        if (abilities[index].isOffCd()) {
            Debug.Log("AI TURN: " + index);
            abilities[index].Use(healthManager, ((TNetworkManager)NetworkManager.singleton).players[0].healthManager, out bool success);
            healthManager.UpdateStatusEffects();
        } else {
            SimulateTurn();
        }
        
    }
}