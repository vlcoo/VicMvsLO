using System;

// class that represents the condition-action pair - for use with MatchConditioner.
// has a condition and an action. the possible values of each are defined in the enums.
public class Rule : IEquatable<Rule> {
    //---Enums
    public enum PossibleConditions : byte {
        Spawned, GrabbedCheckpoint, GotHarmed, HarmedSomeone, GotKilled, KilledSomeone, GotDisqualified, GrabbedStar,
        GrabbedCoin, GrabbedPowerup, GrabbedStarcoin, GotBumped, BumpedSomeone, GotStompedOn, StompedOnSomeone,
        GotFrozen, FrozeSomeone, HitBlock, SteppedOnEnemy, Ran, GrabbedSomething, EnteredPipe, TouchedFloor,
        StoppedMoving, GrabbedLastCoin, Reached0Coins, OneMinuteRemains, Lapped, GrabbedFlagpoleTop, Jumped,
        LookedRight, LookedLeft, LookedUp, LookedDown, TriggeredPowerup, Every5Seconds, Every10Seconds, Every15Seconds,
        Every20Seconds, Every30Seconds, Every60Seconds
    }
    public enum PossibleActions : byte {
        GiveStar, RemoveStar, GiveCoin, RemoveCoin, Give1Up, Kill, GivePowerup, GiveIFrames, Harm, Launch, Knockback,
        HardKnockback, Dive, Freeze, TeleportRandomly, EndWithWin, EndWithDraw, Disqualify, RespawnLevel, SpawnPowerup,
        DestroyTerrain, SpawnEnemy, SpawnStar, SpawnLooseStar, SpawnLooseCoin, RemoveReserve, ChangeStarSpawningRate,
        ChangeTimerRate
    }

    // both the condition and action have a target, which is the entity that will be able to trigger the condition or
    // the one that will be affected by the action. they use a flags enum to select them.
    // if the condition or the action is not player-related (i.e. 1-min remaining or respawn level), target is ignored.
    [Flags]
    public enum Target {
        Random = 0,     // COND: n.a.
                        // ACT:  affects one random player
        AnySelf = 1,    // COND: anyone can trigger this
                        // ACT:  affects the player that triggered the related condition
        NonSelf = 2,    // COND: n.a.
                        // ACT:  affects everyone but the player that triggered the related condition
        Host = 4,       // COND: only the host can trigger this
                        // ACT:  only affects the host
        NonHost = 8,    // COND: anyone but the host can trigger this
                        // ACT:  affects everyone but the host
        Winning = 16,   // COND: only the player in first place can trigger this
                        // ACT:  only affects the player in first place
        Losing = 32,    // COND: only the player in last place can trigger this
                        // ACT:  only affects the player in last place
        Team = 64,      // COND: only a player in a team (at least one teammate) can trigger this
                        // ACT:  affects everyone that's in the same team as the player that triggered the related condition
        NonTeam = 128,  // COND: only a player not in a team (zero teammates) can trigger this
                        // ACT:  affects everyone that's in a team different from the player that triggered the related condition
        Tagged = 256,   // COND: only the players that have been marked in the room settings can trigger this
                        // ACT:  affects everyone that has been marked in the room settings
        NonTagged = 512 // COND: anyone but the players that have been marked can trigger this
                        // ACT:  affects everyone that has not been marked in the room settings
    }

    //---Public Variables
    public PossibleConditions Condition;
    public PossibleActions Action;
    public Target ConditionTarget;
    public Target ActionTarget;

    // both the condition and action have an extra parameter, which is an optional string that each condition or action
    // can use to store additional information. up to them to determine what each possible value does.
    public string ConditionParameter;
    public string ActionParameter;

    // the rule's got a percentage, which is the chance of the rule being triggered. 0.0f to 1.0f
    public float RandomChance;

    //---Constructors
    public Rule(PossibleConditions condition, PossibleActions action, Target conditionTarget = Target.AnySelf,
        Target actionTarget = Target.AnySelf, string conditionParameter = "", string actionParameter = "",
        float randomChance = 1.0f)
    {
        Condition = condition;
        Action = action;
        ConditionTarget = conditionTarget;
        ActionTarget = actionTarget;
        ConditionParameter = conditionParameter;
        ActionParameter = actionParameter;
        RandomChance = randomChance;
    }

    //---IEquatatble overrides
    public bool Equals(Rule other) {
        return Condition == other.Condition && Action == other.Action && ConditionTarget == other.ConditionTarget &&
               ActionTarget == other.ActionTarget && ConditionParameter == other.ConditionParameter &&
               ActionParameter == other.ActionParameter;
    }
}
