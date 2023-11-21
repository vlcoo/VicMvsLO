using UnityEngine;

public class BehaviourEnableOnGameStart : WaitForGameStart
{
    [SerializeField] private Behaviour[] behaviours;

    public override void Execute()
    {
        foreach (var behaviour in behaviours)
            behaviour.enabled = true;
    }
}