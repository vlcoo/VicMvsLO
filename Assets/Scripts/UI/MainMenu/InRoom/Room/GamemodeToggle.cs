using NSMB.Networking;
using Quantum;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GamemodeToggle : MonoBehaviour {
    public Image image;
    
    public void Start() {
        QuantumEvent.Subscribe<EventRulesChanged>(this, OnRulesChanged);
        QuantumCallback.Subscribe<CallbackGameStarted>(this, OnGameStarted);
    }

    private unsafe void OnRulesChanged(EventRulesChanged e) {
        UpdateToggle(e.Game.Frames.Predicted.Global->Rules.Gamemode);
    }
    
    private unsafe void OnGameStarted(CallbackGameStarted e) {
        UpdateToggle(e.Game.Frames.Predicted.Global->Rules.Gamemode);
    }

    private void UpdateToggle(AssetRef<GamemodeAsset> gamemodeAsset) {
        if (QuantumUnityDB.TryGetGlobalAsset(gamemodeAsset, out GamemodeAsset gamemode)) {
            image.sprite = gamemode.Icon;
        }
    }

    public unsafe void ToggleGamemode() {
        CommandChangeRules cmd = new CommandChangeRules {
            EnabledChanges = CommandChangeRules.Rules.Gamemode,
        };

        QuantumGame game = QuantumRunner.DefaultGame;
        var allGamemodes = game.Configurations.Simulation.AllGamemodes;
        int currentIndex = allGamemodes.IndexOf(gm => gm == game.Frames.Predicted.Global->Rules.Gamemode);
        int newIndex = (currentIndex + 1) % allGamemodes.Length;
        cmd.Gamemode = allGamemodes[newIndex];

        int slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
        game.SendCommand(slot, cmd);
    }
}
