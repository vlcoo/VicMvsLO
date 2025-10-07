using System;
using System.IO;

public struct GameVersion : IEquatable<GameVersion>, IComparable<GameVersion> {
    public byte Vanilla, Major, Minor;

    public bool Equals(GameVersion other) {
        return Vanilla == other.Vanilla && Major == other.Major && Minor == other.Minor;
    }

    public int CompareTo(GameVersion other) {
        if (Vanilla != other.Vanilla) {
            return Vanilla - other.Vanilla;
        }
        if (Major != other.Major) {
            return Major - other.Major;
        }
        if (Minor != other.Minor) {
            return Minor - other.Minor;
        }
        return 0;
    }

    public static bool operator >(GameVersion x, GameVersion y) {
        return x.CompareTo(y) > 0;
    }

    public static bool operator <(GameVersion x, GameVersion y) {
        return x.CompareTo(y) < 0;
    }

    public static bool operator >=(GameVersion x, GameVersion y) {
        return x.CompareTo(y) >= 0;
    }

    public static bool operator <=(GameVersion x, GameVersion y) {
        return x.CompareTo(y) <= 0;
    }

    public override string ToString() {
        return $"{Vanilla}.{Major}.{Minor}";
    }

    public int GetHashCode(GameVersion obj) {
        // https://stackoverflow.com/a/1646913/19635374
        unchecked {
            int hash = 17;
            hash = hash * 31 + obj.Vanilla.GetHashCode();
            hash = hash * 31 + obj.Major.GetHashCode();
            hash = hash * 31 + obj.Minor.GetHashCode();
            return hash;
        }
    }

    public void Serialize(BinaryWriter writer) {
        writer.Write(Vanilla);
        writer.Write(Major);
        writer.Write(Minor);
    }

    public static GameVersion Deserialize(BinaryReader reader) {
        return new GameVersion {
            Vanilla = reader.ReadByte(),
            Major = reader.ReadByte(),
            Minor = reader.ReadByte(),
        };
    }


    public static GameVersion Parse(string version) {
        // Span<byte> parsed = stackalloc byte[3];
        // if (version.StartsWith("v", StringComparison.InvariantCultureIgnoreCase)) {
        //     version = version[1..];
        // }
        //
        // for (int i = 0; i < parsed.Length; i++) {
        //     int separator = version.IndexOf('.');
        //     if (separator == -1) {
        //         break;
        //     }
        //
        //     byte.TryParse(version[..separator], out parsed[i]);
        //     version = version[(separator + 1)..];
        // }
        //
        // return new GameVersion {
        //     Vanilla = parsed[0], Major = parsed[1], Minor = parsed[2],
        // };
        if (version.StartsWith("v", StringComparison.InvariantCultureIgnoreCase)) {
            version = version[1..];
        }

        var parts = version.Split(".");
        return new GameVersion {
            Vanilla = byte.Parse(parts[0]), Major = byte.Parse(parts[1]), Minor = byte.Parse(parts[2]),
        };
    }
}