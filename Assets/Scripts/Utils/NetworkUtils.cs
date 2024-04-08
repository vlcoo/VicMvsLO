using System;
using System.Collections.Generic;

using Fusion;
using Fusion.Sockets;

namespace NSMB.Utils {
    public static class NetworkUtils {

        public static Dictionary<Enum, string> disconnectMessages = new() {
            [ShutdownReason.MaxCcuReached] = "ui.error.ccu",
            [ShutdownReason.CustomAuthenticationFailed] = "ui.error.authentication",
            [ShutdownReason.DisconnectedByPluginLogic] = "ui.error.hosttimeout",
            [ShutdownReason.PhotonCloudTimeout] = "ui.error.photontimeout",
            [ShutdownReason.ConnectionTimeout] = "ui.error.hosttimeout",
            [ShutdownReason.Error] = "ui.error.unknown",
            [ShutdownReason.GameClosed] = "ui.error.joinclosed",
            [ShutdownReason.GameNotFound] = "ui.error.joinnotfound",
            [ShutdownReason.GameIsFull] = "ui.error.joinfull",
            [ShutdownReason.ConnectionRefused] = "ui.error.kicked",

            [NetConnectFailedReason.Timeout] = "ui.error.jointimeout",
            [NetConnectFailedReason.ServerFull] = "ui.error.joinfull",
            [NetConnectFailedReason.ServerRefused] = "ui.error.joinrefused",

            [NetDisconnectReason.Timeout] = "ui.error.hosttimeout",
            [NetDisconnectReason.Unknown] = "ui.error.unknown"
        };

        public static Dictionary<string, SessionProperty> DefaultRoomProperties = new() {
            [Enums.NetRoomProperties.IntProperties] = (int) new IntegerProperties(),
            [Enums.NetRoomProperties.BoolProperties] = (int) new BooleanProperties(),
            [Enums.NetRoomProperties.HostName] = "noname",
        };

        public class IntegerProperties {
            // Extra vcmi props from 36 onwards
            // Laps :: Value ranges from 1-64: 6 bits

            // Level :: Value ranges from 0-127: 7 bits
            // Timer :: Value ranges from 0-127: 7 bits
            // Lives :: Value ranges from 0-63: 6 bits
            // CoinRequirement :: Value ranges from 0-63: 6 bits
            // StarRequirement :: Value ranges from 0-63: 6 bits
            // MaxPlayers :: Value ranges from 1-10: 4 bits

            // 63*42     41....36     35.....29     28.....22     21....16     15....10     9....4     3..0
            // Unused    Laps         Level         Timer        Lives        Coins        Stars      MaxPly


            public int laps = 1, level = 0, timer = 0, lives = 0, coinRequirement = 8, starRequirement = 10, maxPlayers = 10;

            public static implicit operator int(IntegerProperties props) {
                int value = 0;

                value |= (props.laps & 0b111111) << 36;
                value |= (props.level & 0b1111111) << 29;
                value |= (props.timer & 0b1111111) << 22;
                value |= (props.lives & 0b111111) << 16;
                value |= (props.coinRequirement & 0b111111) << 10;
                value |= (props.starRequirement & 0b111111) << 4;
                value |= (props.maxPlayers & 0b1111) << 0;

                return value;
            }

            public static explicit operator IntegerProperties(int bits) {
                IntegerProperties ret = new() {
                    laps = (bits >> 36) & 0b111111,
                    level = (bits >> 29) & 0b1111111,
                    timer = (bits >> 22) & 0b1111111,
                    lives = (bits >> 16) & 0b111111,
                    coinRequirement = (bits >> 10) & 0b111111,
                    starRequirement = (bits >> 4) & 0b111111,
                    maxPlayers = (bits >> 0) & 0b1111,
                };
                return ret;
            }

            public override string ToString() {
                return $"Laps: {laps}, Level: {level}, Timer: {timer}, Lives: {lives}, CoinRequirement: {coinRequirement}, StarRequirement: {starRequirement}, MaxPlayers: {maxPlayers}";
            }
        };

        public class BooleanProperties {

            public bool gameStarted = false, customPowerups = true, teams = false;

            public static implicit operator int(BooleanProperties props) {
                int value = 0;

                Utils.BitSet(ref value, 0, props.gameStarted);
                Utils.BitSet(ref value, 1, props.customPowerups);
                Utils.BitSet(ref value, 2, props.teams);

                return value;
            }

            public static explicit operator BooleanProperties(int bits) {
                return new() {
                    gameStarted = Utils.BitTest(bits, 0),
                    customPowerups = Utils.BitTest(bits, 1),
                    teams = Utils.BitTest(bits, 2),
                };
            }
        };

        public static bool GetSessionProperty(SessionInfo session, string key, out int value) {
            if (session.Properties != null && session.Properties.TryGetValue(key, out SessionProperty property)) {
                value = property;
                return true;
            }
            value = default;
            return false;
        }

        public static bool GetSessionProperty(SessionInfo session, string key, out string value) {
            if (session.Properties != null && session.Properties.TryGetValue(key, out SessionProperty property)) {
                value = property;
                return true;
            }
            value = default;
            return false;
        }

        public static bool GetSessionProperty(SessionInfo session, string key, out bool value) {
            if (session.Properties != null && session.Properties.TryGetValue(key, out SessionProperty property)) {
                value = property == 1;
                return true;
            }
            value = default;
            return false;
        }
    }
}
