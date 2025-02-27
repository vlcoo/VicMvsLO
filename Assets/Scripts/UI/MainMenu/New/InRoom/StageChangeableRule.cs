using Quantum;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StageChangeableRule : ChangeableRule {

    //---Properties
    public override bool CanIncreaseValue {
        get {
            // QuantumGame game = NetworkHandler.Game;
            // var allStages = game.Configurations.Simulation.AllStages;
            // int currentIndex = allStages.IndexOf(map => map == (AssetRef<Map>) value);
            // return currentIndex < allStages.Length - 1;
            return false;
        }
    }
    public override bool CanDecreaseValue {
        get {
            // QuantumGame game = NetworkHandler.Game;
            // var allStages = game.Configurations.Simulation.AllStages;
            // int currentIndex = allStages.IndexOf(map => map == (AssetRef<Map>) value);
            // return currentIndex > 0;
            return false;
        }
    }
    public override bool Editing {
        set {
            base.Editing = false;
            if (!value) return;
            OnStartEditing?.Invoke();
        }
    }

    //---Serialized Variables
    // [SerializeField] private Image stagePreview;
    // [SerializeField] private Sprite unknownMapSprite;
    [SerializeField] private UnityEvent OnStartEditing;

    protected override void IncreaseValueInternal() {
        QuantumGame game = NetworkHandler.Game;
        var allStages = game.Configurations.Simulation.AllStages;
        int currentIndex = allStages.IndexOf(map => map == (AssetRef<Map>) value);
        int newIndex = Mathf.Min(currentIndex + 1, allStages.Length - 1);

        if (currentIndex != newIndex) {
            value = (AssetRef<Map>) allStages[newIndex];
            cursorSfx.Play();
            SendCommand();
        }
    }

    protected override void DecreaseValueInternal() {
        QuantumGame game = NetworkHandler.Game;
        var allStages = game.Configurations.Simulation.AllStages;
        int currentIndex = allStages.IndexOf(map => map == (AssetRef<Map>) value);
        int newIndex = Mathf.Max(currentIndex - 1, 0);

        if (currentIndex != newIndex) {
            value = (AssetRef<Map>) allStages[newIndex];
            cursorSfx.Play();
            SendCommand();
        }
    }

    private unsafe void SendCommand() {
        CommandChangeRules cmd = new CommandChangeRules {
            EnabledChanges = ruleType,
        };

        QuantumGame game = NetworkHandler.Game;
        switch (ruleType) {
        case CommandChangeRules.Rules.Stage:
            cmd.Stage = (AssetRef<Map>) value;
            break;
        }

        int slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(QuantumUtils.GetHostPlayer(game.Frames.Predicted, out _))];
        game.SendCommand(slot, cmd);
    }

    protected override void UpdateLabel() {
        string stageName;
        Sprite sprite;
        if (value is AssetRef<Map> mapAsset
            && QuantumUnityDB.TryGetGlobalAsset(mapAsset, out Map map)
            && QuantumUnityDB.TryGetGlobalAsset(map.UserAsset, out VersusStageData stage)) {
            stageName = stage.LegalEnglishName;
            sprite = stage.Icon;
        } else {
            stageName = "???";
            // sprite = unknownMapSprite;
        }
        label.text = labelPrefix + stageName;
        // stagePreview.sprite = sprite;
    }
}
