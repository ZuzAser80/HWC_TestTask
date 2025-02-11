using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    public List<Ability> abilities = new List<Ability>();
    [SerializeField] private AbilityView abilityView;
    [SerializeField] public HealthManager healthManager;
    [SerializeField] public bool canTurn = false;
    // important, dont touch
    public override void OnStartServer() => ((TNetworkManager)NetworkManager.singleton).FinishTurn();
    public void Heal(int value) => healthManager.Heal(value);
    public void Damage(int value) => healthManager.Damage(value);
    
    #region Commands

    [Command]
    public void UseAbilityCmd(int index) { 
        abilities[index].Use(healthManager, ((TNetworkManager)NetworkManager.singleton).GetEnemy(this), out bool success);
        if (success) {
            ((TNetworkManager)NetworkManager.singleton).FinishTurn();
        }
    }

    #endregion
    public void Cleanse() => healthManager.Cleanse();

    #region RPCs

    [ClientRpc]
    public void Recieve(List<int> cdList, List<StatusEffect> effectsList) {  
        //updaiting abilities and their cooldowns
        abilities.ForEach(x => {
            x.cd = cdList[abilities.IndexOf(x)];
        });
        abilityView.DisplayTexts(abilities);
        healthManager.SetEffectList(effectsList);
        // updating status effects and ticking them
        healthManager.UpdateStatusEffects();
    }

    [ClientRpc] 
    public void TurnChange() { 
        if (!isLocalPlayer) return;
        healthManager.ApplyStatusEffects();
        canTurn = ((TNetworkManager)NetworkManager.singleton).players.Count() > 1 ? !canTurn : true;
    }

    [ClientRpc] public void SetBlock(int value) { healthManager.SetBlock(value); healthManager.UpdateStatusEffects(); }

    #endregion

    private void Update() {
        if (!isLocalPlayer || !canTurn) return;
        abilityView.DisplayTexts(abilities);
        healthManager.UpdateStatusEffects();
        if (Input.GetKeyDown(KeyCode.Alpha1)) { UseAbilityCmd(0); }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) { UseAbilityCmd(1); }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) { UseAbilityCmd(2); }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) { UseAbilityCmd(3); }
        else if (Input.GetKeyDown(KeyCode.Alpha5)) { UseAbilityCmd(4); }
    }
}

