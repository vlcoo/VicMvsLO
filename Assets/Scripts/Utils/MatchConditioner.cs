using System;
using UnityEngine;

// Manager for the custom rulesets pairs. Basically the heart of the Custom Match-inator.
// Defines the functions for each action.
// Any entity can trigger a condition by calling the corresponding function in here, with the condition as parameter.
// If the condition is in the list of active rules, the corresponding action is executed.
public class MatchConditioner : MonoBehaviour
{
    // Inner class that represents the condition-action pair.
    // has a condition and an action. the possible values of each are defined in the enums.
    public class Rule {
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
            All = 0,        // COND: anyone can trigger this
                            // ACT:  affects everyone
            Random = 1,     // COND: n.a.
                            // ACT:  affects one random player
            Self = 2,       // COND: n.a.
                            // ACT:  affects the player that triggered the related condition
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
        public float RandomChance = 1.0f;
    }
}
