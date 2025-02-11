using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Unity.Collections;
using UnityEngine;

public class HealthManager : NetworkBehaviour {
    [SyncVar(hook = nameof(PlayerHealthChanged))] [SerializeField] private int Health;
    [SyncVar] private int Block;
    [SerializeField] private HealthView healthView;
    public List<StatusEffect> allStatusEffects = new List<StatusEffect>();
    [SerializeField] private List<StatusEffectView> effectDisplayers = new List<StatusEffectView>();
    private List<StatusEffect> temp = new List<StatusEffect>();
    public event Action<int> OnPlayerHealthChanged;
    [Command(requiresAuthority = false)] void PlayerHealthChanged(int _, int newHealth) => SetHealth(newHealth);
    public override void OnStopClient() => OnPlayerHealthChanged = null;
    public override void OnStartClient() => OnPlayerHealthChanged = SetHealth; 

    #region Health

    [ClientRpc]
    public void SetHealth(int value) { 
        Health = value;
        healthView.DisplayHealth(value);
    }

    public void Damage(int value) {
        if (allStatusEffects.Any(x => x.Type == ImpactType.BLOCK)) {
            if (Block > value) { 
                Block -= value; 
            } 
            else { 
                value -= Block; 
                Block = 0; 
                allStatusEffects.Find(x => x.Type == ImpactType.BLOCK).TurnsLeft = 0;
            }
        }
        if (Health > value) {
            Health -= value;
        } else {
            // go commit die
        }
    }
    
    public void Heal(int value) => Health += value;

    #endregion

    #region Status Effects

    [Command]
    public void ApplyStatusEffects() {
        allStatusEffects.ForEach(x => {
            switch(x.Type) {
                case ImpactType.DOT:
                    Damage(x.Amount);
                    break;
                case ImpactType.HOT:
                    Heal(x.Amount);
                    break;
            }
            x.TurnsLeft--;
        });
    }

    public void UpdateStatusEffects() {
        temp.Clear();
        effectDisplayers.ForEach(x => {
            if (allStatusEffects.Any(y => y.Type == x.effect.Type)) {
                x.DisplayEffect(allStatusEffects.Find(y => y.Type == x.effect.Type));
            } else {
                x.Disable();
            }
        });
        allStatusEffects.ForEach(x => { 
            if (x.TurnsLeft <= 0) { temp.Add(x); }
        });
        temp.ForEach(x => allStatusEffects.Remove(x));
    }

    public void Cleanse() {
        if (allStatusEffects.Any(x => x.Type == ImpactType.DOT)) { allStatusEffects.Remove(allStatusEffects.Find(x => x.Type == ImpactType.DOT)); }
    }

    public void SetEffectList(List<StatusEffect> list) => allStatusEffects = list;
    public void SetBlock(int value) => Block = value;

    #endregion
}