using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Mirror;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class Ability {
    [SerializeField] protected int Amount;
    [SerializeField] protected ImpactType Type;
    [SerializeField] protected int Cooldown;
    [SerializeField] private int Time;
    protected bool CanUse { get; private set; }
    protected StatusEffect effect;
    [SyncVar(hook = nameof(CdChanged))] public int cd;
    public event Action<int> OnCdChanged;
    void CdChanged(int _, int newHealth) => OnCdChanged?.Invoke(newHealth);

    [ServerCallback]
    public virtual void Use(HealthManager self, HealthManager enemy, out bool success) {
        if (!CanUse && cd > 0) { success = false; return; }
        effect = new StatusEffect(Type, Time + 1, Amount);
        ((TNetworkManager)NetworkManager.singleton).AllAbilitiesDict[Type](self, enemy, effect, Amount);
        success = true;
        cd = Cooldown;
        CanUse = cd == 0;
    }

    [ServerCallback]
    public void Tick() {
        if (cd > 0) { cd--; } else { cd = 0; CanUse = true; }
    }

    public bool isOffCd() => cd <= 0;
}
[Serializable] public enum ImpactType { DAMAGE, DOT, HOT, BLOCK, CLEAN }
[Serializable] public class StatusEffect {
    public ImpactType Type;
    public int TurnsLeft;
    public int Amount;
    public StatusEffect() {}
    public StatusEffect(ImpactType type, int turns, int amount) { Type = type; TurnsLeft = turns; Amount = amount; }
}