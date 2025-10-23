using Photon.Client;
using Photon.Realtime;
using Quantum;
using System.Collections.Generic;

namespace NSMB.Utilities {
    public static class NetworkUtils {

        public static PhotonHashtable DefaultRoomProperties => new() {
            [Enums.NetRoomProperties.HostName] = "noname",
            [Enums.NetRoomProperties.IntProperties] = (int) IntegerProperties.Default,
            [Enums.NetRoomProperties.BoolProperties] = (int) BooleanProperties.Default,
            [Enums.NetRoomProperties.StageGuid] = QuantumUnityDB.GetGlobalAsset(GlobalController.Instance.config.DefaultGamemode).DefaultRules.Stage.Id.ToString(),
            [Enums.NetRoomProperties.GamemodeGuid] = GlobalController.Instance.config.DefaultGamemode.Id.ToString(),
        };

        public static Dictionary<short, string> RealtimeErrorCodes = new() {
            [ErrorCode.CustomAuthenticationFailed] = "ui.error.authentication",
            [ErrorCode.MaxCcuReached] = "ui.error.ccu",
            [ErrorCode.GameDoesNotExist] = "ui.error.join.notfound",
            [ErrorCode.GameClosed] = "ui.error.join.closed",
            [ErrorCode.GameFull] = "ui.error.join.full",
            [ErrorCode.JoinFailedFoundActiveJoiner] = "ui.error.join.alreadyingame",
            [ErrorCode.JoinFailedPeerAlreadyJoined] = "ui.error.join.alreadyingame",
        };

        public static Dictionary<DisconnectCause, string> RealtimeDisconnectCauses = new() {
            [DisconnectCause.CustomAuthenticationFailed] = "ui.error.authentication",
            [DisconnectCause.MaxCcuReached] = "ui.error.ccu",
            [DisconnectCause.DnsExceptionOnConnect] = "ui.error.connection",
            [DisconnectCause.ExceptionOnConnect] = "ui.error.connection",
            [DisconnectCause.ServerTimeout] = "ui.error.timeout",
            [DisconnectCause.ClientTimeout] = "ui.error.timeout",
            [DisconnectCause.Exception] = "ui.error.unknown",
            [DisconnectCause.DisconnectByServerLogic] = "ui.error.plugin",
        };

        public struct IntegerProperties {
            public static readonly IntegerProperties Default = new() {
                CoinRequirement = 8,
                StarRequirement = 10,
            };

            // Level ::               unused. 0 bits
            // Amt. of triggers ::    7 bits (0-127) max 80
            // Timer ::               10 bits (0-1023) max 1000
            // Lives ::               10 bits "
            // CoinRequirement ::     10 bits "
            // StarRequirement ::     10 bits "
            // MaxPlayers ::          unused. 0 bits

            // 47 bits total
            // 46...40  39...30  29...20  19...10  9...0
            // Triggers Timer    Lives    Coins    Stars
            // public int /*Level,*/ Timer, Lives, CoinRequirement, StarRequirement;
            public int TriggerCount, Timer, Lives, CoinRequirement, StarRequirement;

            public static implicit operator long(IntegerProperties props) {
                long value = 0;

                value |= (props.TriggerCount & 0b1111111L) << 40;
                value |= (props.Timer & 0b1111111111L) << 30;
                value |= (props.Lives & 0b1111111111L) << 20;
                value |= (props.CoinRequirement & 0b1111111111L) << 10;
                value |= (props.StarRequirement & 0b1111111111L) << 0;

                return value;
            }

            public static implicit operator IntegerProperties(long bits) {
                IntegerProperties ret = new() {
                    TriggerCount = (int)((bits >> 40) & 0b1111111L),
                    Timer = (int)((bits >> 30) & 0b1111111111L),
                    Lives = (int)((bits >> 20) & 0b1111111111L),
                    CoinRequirement = (int)((bits >> 10) & 0b1111111111L),
                    StarRequirement = (int)((bits >> 0) & 0b1111111111L),
                };
                return ret;
            }
        };

        public struct BooleanProperties {
            public static readonly BooleanProperties Default = new() {
                CustomPowerups = true
            };

            public bool CustomPowerups, Teams, DrawOnTimeUp, GameStarted;

            public static implicit operator int(BooleanProperties props) {
                int value = 0;

                Utils.BitSet(ref value, 0, props.CustomPowerups);
                Utils.BitSet(ref value, 1, props.Teams);
                Utils.BitSet(ref value, 2, props.DrawOnTimeUp);
                Utils.BitSet(ref value, 3, props.GameStarted);

                return value;
            }

            public static implicit operator BooleanProperties(int bits) {
                return new() {
                    CustomPowerups = Utils.BitTest(bits, 0),
                    Teams = Utils.BitTest(bits, 1),
                    DrawOnTimeUp = Utils.BitTest(bits, 2),
                    GameStarted = Utils.BitTest(bits, 3),
                };
            }
        };

        public static bool GetCustomProperty<T>(PhotonHashtable table, string key, out T value) {
            if (table.TryGetValue(key, out object objValue)) {
                value = (T) objValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool GetCustomProperty(PhotonHashtable table, string key, out bool value) {
            if (table.TryGetValue(key, out object objValue)) {
                value = (int) objValue == 1;
                return true;
            }
            value = default;
            return false;
        }
    }
}
