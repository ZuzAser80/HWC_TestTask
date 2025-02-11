using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class TNetworkManager : NetworkManager {

    public int turn = 0;
    public List<Player> players = new List<Player>();
    private List<int> cds = new List<int>();
    [SerializeField] private AI ai;
    private Dictionary<ImpactType, Func<HealthManager, HealthManager, StatusEffect, int, bool>> abilities = new Dictionary<ImpactType, Func<HealthManager, HealthManager, StatusEffect, int, bool>> {
        { ImpactType.DAMAGE, (self, enemy, effect, amount) => { enemy.Damage(amount); return true; } },
        { ImpactType.DOT, (self, enemy, effect, amount) => { enemy.allStatusEffects.Add(effect); enemy.Damage(amount * 5); return true; } },
        { ImpactType.HOT, (self, enemy, effect, amount) => { self.allStatusEffects.Add(effect); return true; } },
        { ImpactType.BLOCK, (self, enemy, effect, amount) => {  self.allStatusEffects.Add(effect); self.SetBlock(amount); return true; } },
        { ImpactType.CLEAN, (self, enemy, effect, amount) => {  self.Cleanse(); return true; } }
    };

    public Dictionary<ImpactType, Func<HealthManager, HealthManager, StatusEffect, int, bool>> AllAbilitiesDict {
        get { return abilities; }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject player = Instantiate(playerPrefab, players.Count > 0 ? Vector2.down : Vector2.up, Quaternion.identity);
        players.Add(player.GetComponent<Player>());
        NetworkServer.AddPlayerForConnection(conn, player);
        //заглушка
        players[0].canTurn = true;
        ai.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
    }

    public void FinishTurn() {
        if (players.Count == 1) { players[0].canTurn = true; ai.SimulateTurn(); }
        turn++;
        TickAll();
        players.ForEach(x => { 
            cds.Clear();
            x.abilities.ForEach(y => cds.Add(y.cd));
            x.Recieve(cds, x.healthManager.allStatusEffects); 
            x.TurnChange();
        });
    }

    public void TickAll() { players.ForEach(x => x.abilities.ForEach(y => y.Tick())); ai.abilities.ForEach(x => x.Tick()); }

    public HealthManager GetEnemy(Player x) => players.Any(z => z != x) ? players.Find(z => z != x).healthManager : ai.healthManager;

}