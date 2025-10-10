using NSMB.Networking;
using Quantum;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StageChangeableRule : MonoBehaviour {
    public TMP_Text label;

    public void Start() {
        QuantumEvent.Subscribe<EventRulesChanged>(this, OnRulesChanged);
        QuantumCallback.Subscribe<CallbackGameStarted>(this, OnGameStarted);
    }

    private unsafe void OnRulesChanged(EventRulesChanged e) {
        UpdateLabel(e.Game.Frames.Predicted.Global->Rules.Stage);
    }
    
    private unsafe void OnGameStarted(CallbackGameStarted e) {
        UpdateLabel(e.Game.Frames.Predicted.Global->Rules.Stage);
    }

    private void UpdateLabel(AssetRef<Map> mapAsset) {
        var stageName = "unknown";
        if (QuantumUnityDB.TryGetGlobalAsset(mapAsset, out Map map)
            && QuantumUnityDB.TryGetGlobalAsset(map.UserAsset, out VersusStageData stage)) {
            stageName = stage.LegalEnglishName;
        }
        label.text = $"{stageName}";
        // stagePreview.sprite = sprite;
    }
}
