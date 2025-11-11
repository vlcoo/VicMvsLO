using Quantum;
using System.Collections.Generic;

namespace NSMB.UI.MainMenu.TriggerList {
    public static class TriggerMappings {
        // some conditions, actions and constraints are global (not referring to a player, but rather the stage or match itself)
        // they can't have a target.
        public static readonly List<TriggerCondition> NonPeopleConditions = new() {
            TriggerCondition.MatchStarted,
            // TriggerCondition.XSecondRemaining,
            TriggerCondition.EveryXSeconds,
            // TriggerCondition.SongBahd,
        };
        public static readonly List<TriggerAction> NonPeopleActions = new() {
            TriggerAction.DrawMatch,
            TriggerAction.RespawnLevel,
            // TriggerAction.ExplodeLevel,
            // TriggerAction.SpawnStar
        };
        public static readonly List<TriggerConstraint> NonPeopleConstraints = new() {
            TriggerConstraint.Always,
            TriggerConstraint.TimerIsLessThanX,
            TriggerConstraint.TimerIsMoreThanX,
            TriggerConstraint.StarsExist,
            TriggerConstraint.StarsNotExist,
            TriggerConstraint.EnemiesExist,
            TriggerConstraint.EnemiesNotExist,
            TriggerConstraint.CoinsExist,
            TriggerConstraint.CoinsNotExist,
            TriggerConstraint.XPlayersRemaining,
            TriggerConstraint.LessThanXPlayersRemaining,
            TriggerConstraint.MoreThanXPlayersRemaining,
        };
        
        // some targets are exclusive to either the conditions, the actions or the constraints.
        public static readonly List<TriggerTarget> IncompatibleConditionTargets = new() {
            TriggerTarget.Everyone,
            TriggerTarget.OneRandom,
            TriggerTarget.Randoms,
            TriggerTarget.Conditioner, 
            TriggerTarget.ConditionerTeam, 
            TriggerTarget.NonConditioner,
            TriggerTarget.NonConditionerTeam,
            TriggerTarget.Actioner,
            TriggerTarget.ActionerTeam,
            TriggerTarget.NonActioner,
            TriggerTarget.NonActionerTeam,
        };
        public static readonly List<TriggerTarget> IncompatibleActionTargets = new() {
            TriggerTarget.Any,
            TriggerTarget.Actioner,
            TriggerTarget.ActionerTeam,
            TriggerTarget.NonActioner,
            TriggerTarget.NonActionerTeam,
        };
        public static readonly List<TriggerTarget> IncompatibleConstraintTargets = new() {
            TriggerTarget.OneRandom,
            TriggerTarget.Randoms
        };
        
        // also, we have to map the parameters to each condition, action or constraint.
        public static readonly Dictionary<TriggerCondition, List<string>> ConditionParameters = new() {
            // { TriggerCondition.LookedXDirection, new List<string> { "Any", "Up", "Right", "Down", "Left" } },
            // { TriggerCondition.XSecondRemaining, new List<string> { "60", "10" } },
            { TriggerCondition.EveryXSeconds, new List<string> { "1", "5", "10", "15", "30", "60" } },
            { TriggerCondition.Stunned, new List<string> { "Bump", "Knockback", "HardKnockback" } },
            { TriggerCondition.GotXPowerup, new List<string> { "Any", "Mushroom", "FireFlower", "IceFlower", "PropellerMushroom", "MiniMushroom", "BlueShell", "HammerSuit", "MegaMushroom", "Starman" } },
        };
        public static readonly Dictionary<TriggerAction, List<string>> ActionParameters = new() {
            { TriggerAction.GiveXPowerup, new List<string> { "Random", "Mushroom", "FireFlower", "IceFlower", "PropellerMushroom", "MiniMushroom", "BlueShell", "HammerSuit", "MegaMushroom", "Starman" } },
            { TriggerAction.SpawnXPowerup, new List<string> { "Random", "Mushroom", "FireFlower", "IceFlower", "PropellerMushroom", "MiniMushroom", "BlueShell", "HammerSuit", "MegaMushroom", "Starman" } },
            { TriggerAction.GiveXReserve, new List<string> { "Random", "Mushroom", "FireFlower", "IceFlower", "PropellerMushroom", "MiniMushroom", "BlueShell", "HammerSuit", "MegaMushroom", "Starman" } },
            { TriggerAction.SpawnXEnemy, new List<string> { "Random", "Goomba", "Goombrat", "Koopa", "RedKoopa", "BlueKoopa", "BulletBill", "Boo", "Spiny", "Bobomb" } },
            // { TriggerAction.BecomeXTeam, new List<string> { "Random", "A", "B", "C", "D", "E" } },
            { TriggerAction.Stun, new List<string> { "Bump", "Knockback", "HardKnockback", "ForcefulKnockback" } },
        };
        public static readonly Dictionary<TriggerConstraint, List<string>> ConstraintParameters = new() {
            { TriggerConstraint.IsXPowerup, new List<string> { "SmallMario", "Mushroom", "FireFlower", "IceFlower", "PropellerMushroom", "MiniMushroom", "BlueShell", "HammerSuit", "MegaMushroom" } },
            { TriggerConstraint.IsNotXPowerup, new List<string> { "SmallMario", "Mushroom", "FireFlower", "IceFlower", "PropellerMushroom", "MiniMushroom", "BlueShell", "HammerSuit", "MegaMushroom" } },
        };

        public static readonly List<TriggerConstraint> ConstraintNumberParameters = new() {
            TriggerConstraint.HasXCoins,
            TriggerConstraint.HasXStars,
            TriggerConstraint.HasXLives,
            TriggerConstraint.HasLessThanXCoins,
            TriggerConstraint.HasLessThanXStars,
            TriggerConstraint.HasLessThanXLives,
            TriggerConstraint.HasMoreThanXCoins,
            TriggerConstraint.HasMoreThanXStars,
            TriggerConstraint.HasMoreThanXLives,
            TriggerConstraint.TimerIsLessThanX,
            TriggerConstraint.TimerIsMoreThanX,
            TriggerConstraint.XPlayersRemaining,
            TriggerConstraint.LessThanXPlayersRemaining,
            TriggerConstraint.MoreThanXPlayersRemaining,
        };
        
        // finally, certain condition-action pairs are recursive or contradictory and are forbidden. list them here.
        public static readonly Dictionary<TriggerCondition, TriggerAction> ForbiddenPairs = new() {
            { TriggerCondition.GotStar, TriggerAction.GiveStar }, { TriggerCondition.GotCoin, TriggerAction.GiveCoin },
        };
    }
}