using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace NSMB.Utils
{
    public class Utils
    {
        public enum DeviceType
        {
            DESKTOP,
            BROWSER,
            MOBILE,
            EDITOR,
            OTHER
        }

        public static List<GameObject> enemies;

        public static Powerup[] powerups;

        private static readonly Dictionary<string, TileBase> tileCache = new();

        private static readonly Dictionary<char, byte> uiSymbols = new()
        {
            ['c'] = 6,
            ['0'] = 11,
            ['1'] = 12,
            ['2'] = 13,
            ['3'] = 14,
            ['4'] = 15,
            ['5'] = 16,
            ['6'] = 17,
            ['7'] = 18,
            ['8'] = 19,
            ['9'] = 20,
            ['x'] = 21,
            ['C'] = 22,
            ['S'] = 23,
            ['L'] = 65,
            ['/'] = 24,
            [':'] = 25
        };

        public static readonly Dictionary<char, byte> numberSymbols = new()
        {
            ['0'] = 27,
            ['1'] = 28,
            ['2'] = 29,
            ['3'] = 30,
            ['4'] = 31,
            ['5'] = 32,
            ['6'] = 33,
            ['7'] = 34,
            ['8'] = 35,
            ['9'] = 36
        };

        public static readonly Dictionary<char, byte> smallSymbols = new()
        {
            ['0'] = 48,
            ['1'] = 39,
            ['2'] = 40,
            ['3'] = 41,
            ['4'] = 42,
            ['5'] = 43,
            ['6'] = 44,
            ['7'] = 45,
            ['8'] = 46,
            ['9'] = 47
        };

        public static int FirstPlaceStars =>
            GameManager.Instance.players.Where(pl => pl.lives != 0).Max(pc => pc.stars);

        public static bool BitTest(long bit, int index)
        {
            return (bit & (1 << index)) != 0;
        }

        public static Vector3Int WorldToTilemapPosition(Vector3 worldVec, GameManager manager = null, bool wrap = true)
        {
            if (!manager)
                manager = GameManager.Instance;

            var tileLocation = manager.tilemap.WorldToCell(worldVec);
            if (wrap)
                WrapTileLocation(ref tileLocation, manager);

            return tileLocation;
        }

        public static bool WrapWorldLocation(ref Vector3 location, GameManager manager = null)
        {
            if (manager == null)
                manager = GameManager.Instance;

            if (!manager.loopingLevel)
                return false;

            if (location.x < manager.GetLevelMinX())
            {
                location.x += manager.levelWidthTile / 2;
                return true;
            }

            if (location.x >= manager.GetLevelMaxX())
            {
                location.x -= manager.levelWidthTile / 2;
                return true;
            }

            return false;
        }

        public static void WrapTileLocation(ref Vector3Int tileLocation, GameManager manager = null)
        {
            if (manager == null)
                manager = GameManager.Instance;

            if (!manager.loopingLevel)
                return;

            if (tileLocation.x < manager.levelMinTileX)
                tileLocation.x += manager.levelWidthTile;
            else if (tileLocation.x >= manager.levelMinTileX + manager.levelWidthTile)
                tileLocation.x -= manager.levelWidthTile;
        }

        public static Vector3Int WorldToTilemapPosition(float worldX, float worldY)
        {
            return WorldToTilemapPosition(new Vector3(worldX, worldY));
        }

        public static Vector3 TilemapToWorldPosition(Vector3Int tileVec, GameManager manager = null)
        {
            if (!manager)
                manager = GameManager.Instance;
            return manager.tilemap.CellToWorld(tileVec);
        }

        public static Vector3 TilemapToWorldPosition(int tileX, int tileY)
        {
            return TilemapToWorldPosition(new Vector3Int(tileX, tileY));
        }

        public static int GetCharacterIndex(Player player = null)
        {
            if (player == null)
                player = PhotonNetwork.LocalPlayer;

            //Assert.IsNotNull(player, "player is null, are we not connected to Photon?");

            GetCustomProperty(Enums.NetPlayerProperties.Character, out int index, player.CustomProperties);
            return index;
        }

        public static PlayerData GetCharacterData(Player player = null)
        {
            return GlobalController.Instance.characters[GetCharacterIndex(player)];
        }

        public static TileBase GetTileAtTileLocation(Vector3Int tileLocation)
        {
            WrapTileLocation(ref tileLocation);
            return GameManager.Instance.tilemap.GetTile(tileLocation);
        }

        public static TileBase GetTileAtWorldLocation(Vector3 worldLocation)
        {
            return GetTileAtTileLocation(WorldToTilemapPosition(worldLocation));
        }

        public static bool IsTileSolidAtTileLocation(Vector3Int tileLocation)
        {
            WrapTileLocation(ref tileLocation);
            return GetColliderType(tileLocation) == Tile.ColliderType.Grid;
        }

        private static Tile.ColliderType GetColliderType(Vector3Int tileLocation)
        {
            var tm = GameManager.Instance.tilemap;

            if (tm.GetTile<Tile>(tileLocation) is Tile tile)
                return tile.colliderType;

            if (tm.GetTile<RuleTile>(tileLocation) is RuleTile rule)
                return rule.m_DefaultColliderType;

            if (tm.GetTile<AnimatedTile>(tileLocation) is AnimatedTile animated)
                return animated.m_TileColliderType;

            return Tile.ColliderType.None;
        }

        public static bool IsTileSolidBetweenWorldBox(Vector3Int tileLocation, Vector2 worldLocation, Vector2 worldBox,
            bool boxcast = true)
        {
            if (boxcast)
            {
                var collision = Physics2D.OverlapPoint(worldLocation, LayerMask.GetMask("Ground"));
                if (collision && !collision.isTrigger && !collision.CompareTag("Player"))
                    return true;
            }

            var ogWorldLocation = worldLocation;
            while (GetTileAtTileLocation(tileLocation) is TileInteractionRelocator it)
            {
                worldLocation += (Vector2)(Vector3)it.offset * 0.5f;
                tileLocation += it.offset;
            }

            var tileTransform = GameManager.Instance.tilemap.GetTransformMatrix(tileLocation);

            var halfBox = worldBox * 0.5f;
            List<Vector2> boxPoints = new();
            boxPoints.Add(ogWorldLocation + Vector2.up * halfBox + Vector2.right * halfBox); // ++
            boxPoints.Add(ogWorldLocation + Vector2.up * halfBox + Vector2.left * halfBox); // +-
            boxPoints.Add(ogWorldLocation + Vector2.down * halfBox + Vector2.left * halfBox); // --
            boxPoints.Add(ogWorldLocation + Vector2.down * halfBox + Vector2.right * halfBox); // -+

            var sprite = GameManager.Instance.tilemap.GetSprite(tileLocation);
            switch (GetColliderType(tileLocation))
            {
                case Tile.ColliderType.Grid:
                    return true;
                case Tile.ColliderType.None:
                    return false;
                case Tile.ColliderType.Sprite:

                    for (var i = 0; i < sprite.GetPhysicsShapeCount(); i++)
                    {
                        List<Vector2> points = new();
                        sprite.GetPhysicsShape(i, points);

                        for (var j = 0; j < points.Count; j++)
                        {
                            var point = points[j];
                            point *= 0.5f;
                            point = tileTransform.MultiplyPoint(point);
                            point += (Vector2)(Vector3)tileLocation * 0.5f;
                            point += (Vector2)GameManager.Instance.tilemap.transform.position;
                            point += Vector2.one * 0.25f;
                            points[j] = point;
                        }

                        for (var j = 0; j < points.Count; j++)
                            Debug.DrawLine(points[j], points[(j + 1) % points.Count], Color.white, 10);
                        for (var j = 0; j < boxPoints.Count; j++)
                            Debug.DrawLine(boxPoints[j], boxPoints[(j + 1) % boxPoints.Count], Color.blue, 3);


                        if (PolygonsOverlap(points, boxPoints))
                            return true;
                    }

                    return false;
            }

            return IsTileSolidAtTileLocation(WorldToTilemapPosition(worldLocation));
        }

        public static bool PolygonsOverlap(List<Vector2> polygonA, List<Vector2> polygonB)
        {
            var edgeCountA = polygonA.Count;
            var edgeCountB = polygonB.Count;

            // Loop through all the edges of both polygons
            for (var i = 0; i < edgeCountA; i++)
                if (IsInside(polygonB, polygonA[i]))
                    return true;

            for (var i = 0; i < edgeCountB; i++)
                if (IsInside(polygonA, polygonB[i]))
                    return true;

            return false;
        }

        public static bool IsTileSolidAtWorldLocation(Vector3 worldLocation)
        {
            var collision = Physics2D.OverlapPoint(worldLocation, LayerMask.GetMask("Ground"));
            if (collision && !collision.isTrigger && !collision.CompareTag("Player"))
                return true;

            while (GetTileAtWorldLocation(worldLocation) is TileInteractionRelocator it)
                worldLocation += (Vector3)it.offset * 0.5f;

            var tileLocation = WorldToTilemapPosition(worldLocation);
            var tileTransform = GameManager.Instance.tilemap.GetTransformMatrix(tileLocation);

            var sprite = GameManager.Instance.tilemap.GetSprite(tileLocation);
            switch (GetColliderType(tileLocation))
            {
                case Tile.ColliderType.Grid:
                    return true;
                case Tile.ColliderType.None:
                    return false;
                case Tile.ColliderType.Sprite:
                    for (var i = 0; i < sprite.GetPhysicsShapeCount(); i++)
                    {
                        List<Vector2> points = new();
                        sprite.GetPhysicsShape(i, points);

                        for (var j = 0; j < points.Count; j++)
                        {
                            var point = points[j];
                            point *= 0.5f;
                            point = tileTransform.MultiplyPoint(point);
                            point += (Vector2)(Vector3)tileLocation * 0.5f;
                            point += (Vector2)GameManager.Instance.tilemap.transform.position;
                            point += Vector2.one * 0.25f;
                            points[j] = point;
                        }

                        if (IsInside(points, worldLocation))
                            return true;
                    }

                    return false;
            }

            return IsTileSolidAtTileLocation(WorldToTilemapPosition(worldLocation));
        }

        // Given three collinear points p, q, r,
        // the function checks if point q lies
        // on line segment 'pr'
        private static bool OnSegment(Vector2 p, Vector2 q, Vector2 r)
        {
            if (q.x <= Mathf.Max(p.x, r.x) &&
                q.x >= Mathf.Min(p.x, r.x) &&
                q.y <= Mathf.Max(p.y, r.y) &&
                q.y >= Mathf.Min(p.y, r.y))
                return true;
            return false;
        }

        // To find orientation of ordered triplet (p, q, r).
        // The function returns following values
        // 0 --> p, q and r are collinear
        // 1 --> Clockwise
        // 2 --> Counterclockwise
        private static float Orientation(Vector2 p, Vector2 q, Vector2 r)
        {
            var val = (q.y - p.y) * (r.x - q.x) -
                      (q.x - p.x) * (r.y - q.y);

            if (Mathf.Abs(val) < 0.001f) return 0; // collinear
            return val > 0 ? 1 : 2; // clock or counterclock wise
        }

        // The function that returns true if
        // line segment 'p1q1' and 'p2q2' intersect.
        private static bool DoIntersect(Vector2 p1, Vector2 q1,
            Vector2 p2, Vector2 q2)
        {
            // Find the four orientations needed for
            // general and special cases
            var o1 = Orientation(p1, q1, p2);
            var o2 = Orientation(p1, q1, q2);
            var o3 = Orientation(p2, q2, p1);
            var o4 = Orientation(p2, q2, q1);

            // General case
            if (o1 != o2 && o3 != o4) return true;

            // Special Cases
            // p1, q1 and p2 are collinear and
            // p2 lies on segment p1q1
            if (o1 == 0 && OnSegment(p1, p2, q1)) return true;

            // p1, q1 and p2 are collinear and
            // q2 lies on segment p1q1
            if (o2 == 0 && OnSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are collinear and
            // p1 lies on segment p2q2
            if (o3 == 0 && OnSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are collinear and
            // q1 lies on segment p2q2
            if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

            // Doesn't fall in any of the above cases
            return false;
        }

        // Returns true if the point p lies
        // inside the polygon[] with n vertices
        private static bool IsInside(List<Vector2> polygon, Vector2 p)
        {
            // There must be at least 3 vertices in polygon[]
            if (polygon.Count < 3) return false;

            // Create a point for line segment from p to infinite
            Vector2 extreme = new(1000, p.y);

            // Count intersections of the above line
            // with sides of polygon
            int count = 0, i = 0;
            do
            {
                var next = (i + 1) % polygon.Count;

                // Check if the line segment from 'p' to
                // 'extreme' intersects with the line
                // segment from 'polygon[i]' to 'polygon[next]'
                if (DoIntersect(polygon[i],
                        polygon[next], p, extreme))
                {
                    // If the point 'p' is collinear with line
                    // segment 'i-next', then check if it lies
                    // on segment. If it lies, return true, otherwise false
                    if (Orientation(polygon[i], p, polygon[next]) == 0)
                        return OnSegment(polygon[i], p,
                            polygon[next]);
                    count++;
                }

                i = next;
            } while (i != 0);

            // Return true if count is odd, false otherwise
            return count % 2 == 1; // Same as (count%2 == 1)
        }

        public static bool IsAnyTileSolidBetweenWorldBox(Vector2 checkPosition, Vector2 checkSize, bool boxcast = true)
        {
            var minPos = WorldToTilemapPosition(checkPosition - checkSize / 2, wrap: false);
            var size = WorldToTilemapPosition(checkPosition + checkSize / 2, wrap: false) - minPos;

            for (var x = 0; x <= size.x; x++)
            for (var y = 0; y <= size.y; y++)
            {
                Vector3Int tileLocation = new(minPos.x + x, minPos.y + y, 0);
                WrapTileLocation(ref tileLocation);

                if (IsTileSolidBetweenWorldBox(tileLocation, checkPosition, checkSize, boxcast))
                    return true;
            }

            return false;
        }

        public static float WrappedDistance(Vector2 a, Vector2 b)
        {
            var gm = GameManager.Instance;
            if ((gm?.loopingLevel ?? false) && Mathf.Abs(a.x - b.x) > gm.levelWidthTile / 4f)
                a.x -= gm.levelWidthTile / 2f * Mathf.Sign(a.x - b.x);

            return Vector2.Distance(a, b);
        }

        public static bool GetCustomProperty<T>(string key, out T value, Hashtable properties = null)
        {
            if (properties == null)
                properties = PhotonNetwork.CurrentRoom.CustomProperties;
            if (properties == null)
            {
                value = default;
                return false;
            }

            properties.TryGetValue(key, out var temp);
            if (temp != null)
            {
                value = (T)temp;
                return true;
            }

            value = default;
            return false;
        }

        public static GameObject GetRandomEnemy()
        {
            if (enemies == null)
                enemies = Resources.LoadAll<GameObject>("Prefabs/Enemy").ToList();

            return enemies[Random.Range(0, enemies.Count)];
        }

        // MAX(0,$B15+(IF(stars behind >0,LOG(B$1+1, 2.71828),0)*$C15*(1-(($M$15-$M$14))/$M$15)))
        public static Powerup GetRandomItem(PlayerController player)
        {
            var gm = GameManager.Instance;

            // "losing" variable based on ln(x+1), x being the # of stars we're behind
            var ourStars = player.stars;
            var leaderStars = FirstPlaceStars;

            if (powerups == null)
                powerups = Resources.LoadAll<Powerup>("Scriptables/Powerups");

            GetCustomProperty(Enums.NetRoomProperties.StarRequirement, out int starsToWin);
            GetCustomProperty(Enums.NetRoomProperties.NewPowerups, out bool custom);
            GetCustomProperty(Enums.NetRoomProperties.Lives, out int livesOn);
            var lives = livesOn > 0;
            var big = gm.spawnBigPowerups;
            var vertical = gm.spawnVerticalPowerups;

            float totalChance = 0;
            foreach (var powerup in powerups)
            {
                if (powerup.name == "MegaMushroom" && gm.musicState == Enums.MusicState.MegaMushroom)
                    continue;
                if ((powerup.big && !big) || (powerup.vertical && !vertical) || (powerup.custom && !custom) ||
                    (powerup.lives && !lives))
                    continue;

                totalChance += powerup.GetModifiedChance(starsToWin, leaderStars, ourStars);
            }

            var rand = Random.value * totalChance;
            foreach (var powerup in powerups)
            {
                if (powerup.name == "MegaMushroom" && gm.musicState == Enums.MusicState.MegaMushroom)
                    continue;
                if ((powerup.big && !big) || (powerup.vertical && !vertical) || (powerup.custom && !custom) ||
                    (powerup.lives && !lives))
                    continue;

                var chance = powerup.GetModifiedChance(starsToWin, leaderStars, ourStars);

                if (rand < chance)
                    return powerup;
                rand -= chance;
            }

            return powerups[1];
        }

        public static float QuadraticEaseOut(float v)
        {
            return -1 * v * (v - 2);
        }

        public static Hashtable GetTilemapChanges(TileBase[] original, BoundsInt bounds, Tilemap tilemap)
        {
            Dictionary<int, int> changes = new();
            List<string> tiles = new();

            var current = tilemap.GetTilesBlock(bounds);

            for (var i = 0; i < original.Length; i++)
            {
                if (current[i] == original[i])
                    continue;

                var cTile = current[i];
                string path;
                if (cTile == null)
                    path = "";
                else
                    path = ResourceDB.GetAsset(cTile.name).ResourcesPath;

                if (!tiles.Contains(path))
                    tiles.Add(path);

                changes[i] = tiles.IndexOf(path);
            }

            return new Hashtable
            {
                ["T"] = tiles.ToArray(),
                ["C"] = changes
            };
        }

        public static void ApplyTilemapChanges(TileBase[] original, BoundsInt bounds, Tilemap tilemap,
            Hashtable changesTable)
        {
            var copy = (TileBase[])original.Clone();

            var changes = (Dictionary<int, int>)changesTable["C"];
            var tiles = (string[])changesTable["T"];

            foreach (var pairs in changes) copy[pairs.Key] = GetTileFromCache(tiles[pairs.Value]);

            tilemap.SetTilesBlock(bounds, copy);
        }

        public static TileBase GetTileFromCache(string tilename)
        {
            if (tilename == null || tilename == "")
                return null;

            if (!tilename.StartsWith("Tilemaps/Tiles/"))
                tilename = "Tilemaps/Tiles/" + tilename;

            return tileCache.ContainsKey(tilename)
                ? tileCache[tilename]
                : tileCache[tilename] = Resources.Load(tilename) as TileBase;
        }

        public static string GetSymbolString(string str, Dictionary<char, byte> dict = null)
        {
            if (dict == null)
                dict = uiSymbols;

            StringBuilder ret = new();
            foreach (var c in str)
                if (dict.TryGetValue(c, out var index))
                    ret.Append("<sprite=").Append(index).Append(">");
                else
                    ret.Append(c);
            return ret.ToString();
        }

        public static Color GetPlayerColor(Player player, float s = 1, float v = 1)
        {
            var result = -1;
            var count = 0;
            foreach (var pl in PhotonNetwork.PlayerList)
            {
                GetCustomProperty(Enums.NetPlayerProperties.Spectator, out bool spectating, pl.CustomProperties);
                if (spectating)
                    continue;

                if (pl == player)
                    result = count;

                count++;
            }

            if (result == -1)
                return new Color(0.9f, 0.9f, 0.9f, 0.7f);

            return Color.HSVToRGB(result / ((float)count + 1), s, v);
        }

        public static void TickTimer(ref float counter, float min, float delta, float max = float.MaxValue)
        {
            counter = Mathf.Clamp(counter - delta, min, max);
        }

        public static TMP_ColorGradient GetRainbowColor()
        {
            return GlobalController.Instance.logoGradient;
        }

        public static DeviceType GetDeviceType()
        {
            if (Application.isEditor) return DeviceType.EDITOR;
            switch (Application.platform)
            {
                case RuntimePlatform.WebGLPlayer:
                    return Application.isMobilePlatform ? DeviceType.MOBILE : DeviceType.BROWSER;
                case RuntimePlatform.Android:
                case RuntimePlatform.IPhonePlayer:
                    return DeviceType.MOBILE;
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.OSXPlayer:
                    return DeviceType.DESKTOP;
                default:
                    return DeviceType.OTHER;
            }
        }

        public static int GetColorCountForPlayer(PlayerData player)
        {
            return GetColorsForPlayer(player).Count;
        }

        public static List<PlayerColors> GetColorsForPlayer(PlayerData player)
        {
            return (from skin in GlobalController.Instance.skins
                where skin != null
                select skin.colors.FirstOrDefault(color => color.player.Equals(player))
                into color
                where color != null
                select color).ToList();
        }

        public static string RawMessageToEmoji(string message)
        {
            return Regex.Replace(message, ":([^:\\s]+):", match =>
            {
                var capturedGroup = match.Groups[1].Value;
                return GlobalController.Instance.EMOTE_NAMES.Contains(capturedGroup)
                    ? $"<sprite name=\"{capturedGroup}\">"
                    : match.Value;
            });
        }

#if UNITY_STANDALONE_WIN
        [DllImport("libdlgmod.dll", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_LINUX
            [DllImport("libdlgmod.so", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libdlgmod.bundle", CallingConvention = CallingConvention.Cdecl)]
#endif
        private static extern IntPtr get_open_filename(string filter, string fname);
#if UNITY_STANDALONE_WIN
        [DllImport("libdlgmod.dll", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_LINUX
            [DllImport("libdlgmod.so", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libdlgmod.bundle", CallingConvention = CallingConvention.Cdecl)]
#endif
        private static extern IntPtr get_save_filename(string filter, string fname);

        public static string OpenFileBrowser(string filter)
        {
            return Marshal.PtrToStringAnsi(get_open_filename(filter, ""));
        }

        public static string SaveFileBrowser(string filter, string fname)
        {
            return Marshal.PtrToStringAnsi(get_save_filename(filter, fname));
        }
    }
}