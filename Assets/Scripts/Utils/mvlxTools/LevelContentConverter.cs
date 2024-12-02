using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Tilemaps;
// ReSharper disable UsageOfDefaultStructEquality

public class LevelContentConverter : MonoBehaviour
{
    public GameManager parent;
    public GameObject spawnsFolder, itemsFolder, platformsFolder;

    private enum ItemTypes
    {
        KoopaGreen, KoopaRed, KoopaBlue, Goomba, Spiny, BulletLauncher, Squishy, Star, Spinner, Pipe, PipeMini, SemiMushroom, SemiMushroomMini, PlatMariobros, PlatCloud, Spawn
    }
    private enum GridTypes {Normal, Background, Semisolid, Squishy}

    private interface IUnityObject {}

    private readonly struct GdTileObject : IEquatable<GdTileObject>
    {
        public readonly int TilesetId;
        public readonly int TileIdX;
        public readonly int TileIdY;
        public readonly int AlternativeId;

        public GdTileObject(int tilesetId, int tileIdX, int tileIdY, int alternativeId = 0)
        {
            TilesetId = tilesetId;
            TileIdX = tileIdX;
            TileIdY = tileIdY;
            AlternativeId = alternativeId;
        }

        public bool Equals(GdTileObject other) => TilesetId == other.TilesetId && TileIdX == other.TileIdX && TileIdY == other.TileIdY && AlternativeId == other.AlternativeId;

        public override bool Equals(object obj) => obj is GdTileObject other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(TilesetId, TileIdX, TileIdY, AlternativeId);
    }

    private readonly struct UnityTileObject : IUnityObject
    {
        public readonly string TilePalettePrefabPath;
        public readonly int TileIdX;
        public readonly int TileIdY;

        public UnityTileObject(string tilePalettePrefabPath, int tileIdX, int tileIdY)
        {
            TilePalettePrefabPath = tilePalettePrefabPath;
            TileIdX = tileIdX;
            TileIdY = tileIdY;
        }

        public UnityTileObject X(int increment) => new(TilePalettePrefabPath, TileIdX + increment, TileIdY);
        public UnityTileObject Y(int increment) => new(TilePalettePrefabPath, TileIdX, TileIdY + increment);
    }

    private readonly struct UnityItemObject : IUnityObject
    {
        public readonly string ItemPrefabPath;
        public readonly bool NeedsNetworkInstantiation;

        public UnityItemObject(string itemPrefabPath, bool needsNetworkInstantiation = false)
        {
            ItemPrefabPath = itemPrefabPath;
            NeedsNetworkInstantiation = needsNetworkInstantiation;
            if (itemPrefabPath.StartsWith("Resources/") || itemPrefabPath.EndsWith(".prefab"))
            {
                Debug.LogWarning(
                    "This will not work! Prefab path for item shouldn't contain 'Resources/' at the beginning, or an extension at the end. Did you intend this?");
            }
        }
    }

    private static readonly Dictionary<GdTileObject, IUnityObject> TileMapping = new()    // tile in gd (tileset and palette pos)
    {
        { new GdTileObject(5, 13, 4), new UnityTileObject("Tilemaps/Palettes/Desert", -4, -5) },
        { new GdTileObject(5, 14, 4), new UnityTileObject("Tilemaps/Palettes/Desert", -4, -5) },
        { new GdTileObject(5, 15, 4), new UnityTileObject("Tilemaps/Palettes/Desert", -4, -5) },
        { new GdTileObject(5, 13, 5), new UnityTileObject("Tilemaps/Palettes/Desert", -4, -5) },
        { new GdTileObject(5, 14, 5), new UnityTileObject("Tilemaps/Palettes/Desert", -4, -5) },
        { new GdTileObject(5, 15, 5), new UnityTileObject("Tilemaps/Palettes/Desert", -4, -5) },
        { new GdTileObject(5, 13, 6), new UnityTileObject("Tilemaps/Palettes/Desert", -4, -5) },
        { new GdTileObject(5, 14, 6), new UnityTileObject("Tilemaps/Palettes/Desert", -4, -5) },
        { new GdTileObject(5, 15, 6), new UnityTileObject("Tilemaps/Palettes/Desert", -4, -5) },
        { new GdTileObject(5, 14, 3), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -1) },
        { new GdTileObject(5, 14, 2), new UnityTileObject("Tilemaps/Palettes/Desert", -5, 0) },
        { new GdTileObject(5, 15, 2), new UnityTileObject("Tilemaps/Palettes/Desert", -4, 0) },
        { new GdTileObject(5, 14, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -4, -3) },
        { new GdTileObject(5, 14, 0), new UnityTileObject("Tilemaps/Palettes/Desert", -4, -2) },
        { new GdTileObject(5, 10, 8), new UnityTileObject("Tilemaps/Palettes/Desert", 2, -5) },
        { new GdTileObject(5, 11, 8), new UnityTileObject("Tilemaps/Palettes/Desert", 3, -5) },
        { new GdTileObject(5, 10, 7), new UnityTileObject("Tilemaps/Palettes/Desert", 2, -4) },
        { new GdTileObject(5, 11, 7), new UnityTileObject("Tilemaps/Palettes/Desert", 3, -4) },
        { new GdTileObject(5, 11, 6), new UnityTileObject("Tilemaps/Palettes/Desert", 3, -3) },
        { new GdTileObject(5, 11, 5), new UnityTileObject("Tilemaps/Palettes/Desert", 3, -2) },
        { new GdTileObject(5, 10, 5), new UnityTileObject("Tilemaps/Palettes/Desert", 2, -2) },
        { new GdTileObject(5, 10, 6), new UnityTileObject("Tilemaps/Palettes/Desert", 2, -3) },
        { new GdTileObject(5, 9, 8), new UnityTileObject("Tilemaps/Palettes/Desert", 0, -5) },
        { new GdTileObject(5, 8, 8), new UnityTileObject("Tilemaps/Palettes/Desert", -1, -5) },
        { new GdTileObject(5, 8, 7), new UnityTileObject("Tilemaps/Palettes/Desert", -1, -4) },
        { new GdTileObject(5, 9, 7), new UnityTileObject("Tilemaps/Palettes/Desert", 0, -4) },
        { new GdTileObject(5, 9, 6), new UnityTileObject("Tilemaps/Palettes/Desert", 0, -3) },
        { new GdTileObject(5, 8, 6), new UnityTileObject("Tilemaps/Palettes/Desert", -1, -3) },
        { new GdTileObject(5, 8, 5), new UnityTileObject("Tilemaps/Palettes/Desert", -1, -2) },
        { new GdTileObject(5, 9, 5), new UnityTileObject("Tilemaps/Palettes/Desert", 0, -2) },
        { new GdTileObject(5, 7, 2), new UnityTileObject("Tilemaps/Palettes/Desert", -2, 1) },
        { new GdTileObject(5, 8, 2), new UnityTileObject("Tilemaps/Palettes/Desert", -1, 1) },
        { new GdTileObject(5, 9, 2), new UnityTileObject("Tilemaps/Palettes/Desert", 0, 1) },
        { new GdTileObject(5, 10, 2), new UnityTileObject("Tilemaps/Palettes/Desert", 1, 1) },
        { new GdTileObject(5, 7, 3), new UnityTileObject("Tilemaps/Palettes/Desert", -2, 0) },
        { new GdTileObject(5, 8, 3), new UnityTileObject("Tilemaps/Palettes/Desert", -1, 0) },
        { new GdTileObject(5, 9, 3), new UnityTileObject("Tilemaps/Palettes/Desert", 0, 0) },
        { new GdTileObject(5, 10, 3), new UnityTileObject("Tilemaps/Palettes/Desert", 1, 0) },
        { new GdTileObject(5, 7, 4), new UnityTileObject("Tilemaps/Palettes/Desert", -2, -1) },
        { new GdTileObject(5, 8, 4), new UnityTileObject("Tilemaps/Palettes/Desert", -1, -1) },
        { new GdTileObject(5, 9, 4), new UnityTileObject("Tilemaps/Palettes/Desert", 0, -1) },
        { new GdTileObject(5, 10, 4), new UnityTileObject("Tilemaps/Palettes/Desert", 1, -1) },
        { new GdTileObject(5, 0, 0), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 1, 0), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 2, 0), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 3, 0), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 4, 0), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 5, 0), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 6, 0), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 7, 0), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 8, 0), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 9, 0), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 10, 0), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 11, 0), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 0, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 1, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 2, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 3, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 4, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 5, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 6, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 7, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 8, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 9, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 10, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 9, 1, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 10, 1, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 4, 1, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 6, 0, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 7, 0, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 8, 0, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(5, 9, 0, 1), new UnityTileObject("Tilemaps/Palettes/Desert", -5, -5) },
        { new GdTileObject(127, 0, 0), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -4, 3) },
        { new GdTileObject(127, 0, 1), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -3, 1) },
        { new GdTileObject(127, 0, 2), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -4, 4) },
        { new GdTileObject(127, 0, 3), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -5, 3) },
        { new GdTileObject(127, 0, 4), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -2, 3) },
        { new GdTileObject(127, 0, 5), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -5, 4) },
        { new GdTileObject(127, 0, 6), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -6, 3) },
        { new GdTileObject(127, 0, 7), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -1, 3) },
        { new GdTileObject(127, 0, 8), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -6, 4) },
        { new GdTileObject(127, 0, 9), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -7, 3) },
        { new GdTileObject(127, 0, 10), new UnityTileObject("Tilemaps/Palettes/Clown", -4, -1) },
        { new GdTileObject(127, 0, 11), new UnityTileObject("Tilemaps/Palettes/Clown", -3, -1) },
        { new GdTileObject(127, 0, 12), new UnityTileObject("Tilemaps/Palettes/Clown", -4, -2) },
        { new GdTileObject(127, 0, 13), new UnityTileObject("Tilemaps/Palettes/Clown", -3, -2) },
        { new GdTileObject(127, 0, 16), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", 1, -1) },
        { new GdTileObject(127, 0, 17), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", 2, -2) },
        { new GdTileObject(126, 0, 0), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -4, 2) },
        { new GdTileObject(126, 0, 1), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -5, 2) },
        { new GdTileObject(126, 0, 2), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -2, 3) },
        { new GdTileObject(126, 0, 3), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -2, 2) },
        { new GdTileObject(126, 0, 4), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -1, 3) },
        { new GdTileObject(126, 0, 5), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -1, 1) },
        { new GdTileObject(0, 0, 0), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 1, 0), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 2, 0), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 3, 0), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 4, 0), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 5, 0), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 6, 0), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 7, 0), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 8, 0), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 9, 0), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 10, 0), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 11, 0), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 12, 0), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 13, 0), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 14, 0), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 15, 0), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 0, 1), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 1, 1), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 2, 1), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 3, 1), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 4, 1), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 5, 1), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 6, 1), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 7, 1), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 8, 1), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 9, 1), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 10, 1), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 7, 2), new UnityTileObject("Tilemaps/Palettes/Grassland", 1, -9) },
        { new GdTileObject(0, 7, 3), new UnityTileObject("Tilemaps/Palettes/Grassland", 1, -10) },
        { new GdTileObject(0, 6, 5), new UnityTileObject("Tilemaps/Palettes/Grassland", 0, -7) },
        { new GdTileObject(0, 5, 3), new UnityTileObject("Tilemaps/Palettes/Grassland", 5, -3) },
        { new GdTileObject(0, 6, 3), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -3) },
        { new GdTileObject(0, 8, 2), new UnityTileObject("Tilemaps/Palettes/Grassland", -1, -4) },
        { new GdTileObject(0, 9, 2), new UnityTileObject("Tilemaps/Palettes/Grassland", 0, -4) },
        { new GdTileObject(0, 8, 3), new UnityTileObject("Tilemaps/Palettes/Grassland", -1, -5) },
        { new GdTileObject(0, 9, 3), new UnityTileObject("Tilemaps/Palettes/Grassland", 0, -5) },
        { new GdTileObject(0, 0, 2), new UnityTileObject("Tilemaps/Palettes/Grassland", 7, -2) },
        { new GdTileObject(0, 1, 2), new UnityTileObject("Tilemaps/Palettes/Grassland", 8, -2) },
        { new GdTileObject(0, 1, 3), new UnityTileObject("Tilemaps/Palettes/Grassland", 8, -3) },
        { new GdTileObject(0, 0, 3), new UnityTileObject("Tilemaps/Palettes/Grassland", 7, -3) },
        { new GdTileObject(0, 2, 2), new UnityTileObject("Tilemaps/Palettes/Grassland", 1, -4) },
        { new GdTileObject(0, 3, 2), new UnityTileObject("Tilemaps/Palettes/Grassland", 2, -4) },
        { new GdTileObject(0, 3, 3), new UnityTileObject("Tilemaps/Palettes/Grassland", 2, -5) },
        { new GdTileObject(0, 2, 3), new UnityTileObject("Tilemaps/Palettes/Grassland", 1, -5) },
        { new GdTileObject(0, 10, 2), new UnityTileObject("Tilemaps/Palettes/Grassland", 3, -2) },
        { new GdTileObject(0, 11, 2), new UnityTileObject("Tilemaps/Palettes/Grassland", 4, -2) },
        { new GdTileObject(0, 11, 3), new UnityTileObject("Tilemaps/Palettes/Grassland", 4, -3) },
        { new GdTileObject(0, 10, 3), new UnityTileObject("Tilemaps/Palettes/Grassland", 3, -3) },
        { new GdTileObject(0, 12, 2), new UnityTileObject("Tilemaps/Palettes/Grassland", -1, -2) },
        { new GdTileObject(0, 13, 2), new UnityTileObject("Tilemaps/Palettes/Grassland", 0, -2) },
        { new GdTileObject(0, 12, 3), new UnityTileObject("Tilemaps/Palettes/Grassland", -1, -3) },
        { new GdTileObject(0, 13, 3), new UnityTileObject("Tilemaps/Palettes/Grassland", 0, -3) },
        { new GdTileObject(0, 14, 2), new UnityTileObject("Tilemaps/Palettes/Grassland", 1, -2) },
        { new GdTileObject(0, 15, 2), new UnityTileObject("Tilemaps/Palettes/Grassland", 2, -2) },
        { new GdTileObject(0, 14, 3), new UnityTileObject("Tilemaps/Palettes/Grassland", 1, -3) },
        { new GdTileObject(0, 15, 3), new UnityTileObject("Tilemaps/Palettes/Grassland", 2, -3) },
        { new GdTileObject(0, 10, 4), new UnityTileObject("Tilemaps/Palettes/Grassland", 3, -4) },
        { new GdTileObject(0, 10, 5), new UnityTileObject("Tilemaps/Palettes/Grassland", 3, -5) },
        { new GdTileObject(0, 10, 6), new UnityTileObject("Tilemaps/Palettes/Grassland", 3, -6) },
        { new GdTileObject(0, 11, 4), new UnityTileObject("Tilemaps/Palettes/Grassland", 4, -4) },
        { new GdTileObject(0, 11, 5), new UnityTileObject("Tilemaps/Palettes/Grassland", 4, -5) },
        { new GdTileObject(0, 11, 6), new UnityTileObject("Tilemaps/Palettes/Grassland", 4, -6) },
        { new GdTileObject(0, 12, 4), new UnityTileObject("Tilemaps/Palettes/Grassland", 5, -4) },
        { new GdTileObject(0, 12, 5), new UnityTileObject("Tilemaps/Palettes/Grassland", 5, -5) },
        { new GdTileObject(0, 12, 6), new UnityTileObject("Tilemaps/Palettes/Grassland", 5, -6) },
        { new GdTileObject(0, 13, 4), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -4) },
        { new GdTileObject(0, 13, 5), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -5) },
        { new GdTileObject(0, 13, 6), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -6) },
        { new GdTileObject(0, 14, 4), new UnityTileObject("Tilemaps/Palettes/Grassland", 7, -4) },
        { new GdTileObject(0, 14, 5), new UnityTileObject("Tilemaps/Palettes/Grassland", 7, -5) },
        { new GdTileObject(0, 14, 6), new UnityTileObject("Tilemaps/Palettes/Grassland", 7, -6) },
        { new GdTileObject(0, 15, 5), new UnityTileObject("Tilemaps/Palettes/Grassland", 8, -5) },
        { new GdTileObject(0, 15, 6), new UnityTileObject("Tilemaps/Palettes/Grassland", 8, -6) },
        { new GdTileObject(0, 10, 0, 1), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 10, 1, 1), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 15, 0, 1), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(9, 0, 0), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", 2, -2) },
        { new GdTileObject(9, 0, 0, 1), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", 2, -2) },
        { new GdTileObject(9, 0, 1), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -5, -2) },
        { new GdTileObject(9, 1, 0), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -2, -9) },
        { new GdTileObject(9, 1, 1), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -4, -2) },
        { new GdTileObject(9, 2, 1), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -5, -3) },
        { new GdTileObject(9, 3, 1), new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -4, -3) },
        { new GdTileObject(4, 8, 1), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 8, 2), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 9, 2), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 9, 3), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 8, 3), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 7, 5), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 6, 5), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 5, 5), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 4, 5), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 4, 4), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 5, 4), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 6, 4), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 3, 5), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 2, 5), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 1, 5), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 0, 5), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 0, 4), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 1, 4), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 2, 4), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 3, 4), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 8, 1, 1), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(4, 8, 2, 1), new UnityTileObject("Tilemaps/Palettes/Sky", -4, 11) },
        { new GdTileObject(2, 12, 4), new UnityTileObject("Tilemaps/Palettes/Snow", 0, -1) },
        { new GdTileObject(2, 13, 4), new UnityTileObject("Tilemaps/Palettes/Snow", 1, -1) },
        { new GdTileObject(2, 14, 4), new UnityTileObject("Tilemaps/Palettes/Snow", 2, -1) },
        { new GdTileObject(2, 15, 4), new UnityTileObject("Tilemaps/Palettes/Snow", 3, -1) },
        { new GdTileObject(2, 0, 0), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 1, 0), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 2, 0), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 3, 0), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 4, 0), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 5, 0), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 6, 0), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 7, 0), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 8, 0), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 9, 0), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 10, 0), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 11, 0), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 12, 0), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 13, 0), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 14, 0), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 15, 0), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 0, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 1, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 2, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 3, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 4, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 5, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 6, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 7, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 8, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 6, 2), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 6, 3), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 5, 4), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 6, 4), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 7, 3), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 7, 2), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 8, 2), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 8, 3), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 11, 2), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 12, 2), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 9, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -3, -1) },
        { new GdTileObject(2, 10, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -2, -1) },
        { new GdTileObject(2, 8, 5), new UnityTileObject("Tilemaps/Palettes/Snow", -1, -3) },
        { new GdTileObject(2, 9, 5), new UnityTileObject("Tilemaps/Palettes/Snow", 1, -3) },
        { new GdTileObject(2, 0, 6), new UnityTileObject("Tilemaps/Palettes/Snow", 0, -7) },
        { new GdTileObject(2, 6, 0, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 7, 0, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 8, 0, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 9, 0, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 6, 1, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 10, 0, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(2, 15, 0, 1), new UnityTileObject("Tilemaps/Palettes/Snow", -1, 1) },
        { new GdTileObject(3, 4, 3), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 2) },
        { new GdTileObject(3, 6, 2), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(3, 7, 2), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(3, 8, 2), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(3, 6, 3), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(3, 7, 3), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(3, 8, 3), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(3, 12, 0), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(3, 12, 1), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(3, 4, 5), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(3, 5, 5), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(3, 6, 5), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(3, 7, 5), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(3, 8, 5), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(3, 9, 5), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(3, 10, 5), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(3, 4, 5, 1), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(3, 5, 5, 1), new UnityTileObject("Tilemaps/Palettes/Castle", 7, 1) },
        { new GdTileObject(1, 0, 0), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 1, 0), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 2, 0), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 3, 0), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 4, 0), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 5, 0), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 6, 0), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 7, 0), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 8, 0), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 9, 0), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 10, 0), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 11, 0), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 12, 0), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 13, 0), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 14, 0), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 15, 0), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 15, 1), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 14, 1), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 13, 1), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 12, 1), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 11, 1), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 10, 1), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 0, 1), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 3, 1), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 4, 2), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 4, 3), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 3, 3), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 1, 1), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", 1, 1) },
        { new GdTileObject(1, 1, 2), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", 1, 0) },
        { new GdTileObject(1, 6, 1), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", 3, 1) },
        { new GdTileObject(1, 6, 2), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", 3, 0) },
        { new GdTileObject(1, 4, 2, 1), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 6, 0, 1), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 7, 0, 1), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 8, 0, 1), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(1, 9, 0, 1), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -1, 1) },
        { new GdTileObject(3, 5, 3), new UnityTileObject("Tilemaps/Palettes/Cave with Ice", -2, -3) },
        { new GdTileObject(7, 7, 2), new UnityTileObject("Tilemaps/Palettes/Volcano", 1, 3) },
        { new GdTileObject(7, 5, 3), new UnityTileObject("Tilemaps/Palettes/Volcano", 4, 3) },
        { new GdTileObject(7, 6, 3), new UnityTileObject("Tilemaps/Palettes/Volcano", 4, 2) },
        { new GdTileObject(7, 0, 0), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 0, 1), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 0, 2), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 0, 3), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 1, 3), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 1, 2), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 1, 1), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 1, 0), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 2, 0), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 2, 1), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 2, 2), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 2, 3), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 3, 3), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 3, 2), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 3, 1), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 3, 0), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 4, 0), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 4, 1), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 4, 2), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 4, 3), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 5, 2), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 5, 1), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 5, 0), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 6, 0), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 6, 1), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 6, 2), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 7, 1), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 7, 0), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 8, 0), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(7, 8, 1), new UnityTileObject("Tilemaps/Palettes/Volcano", 0, 3) },
        { new GdTileObject(8, 0, 0), new UnityTileObject("Tilemaps/Palettes/Clown", -1, -3) },
        { new GdTileObject(8, 1, 0), new UnityTileObject("Tilemaps/Palettes/Clown", 0, -3) },
        { new GdTileObject(8, 2, 0), new UnityTileObject("Tilemaps/Palettes/Clown", 2, -3) },
        { new GdTileObject(8, 3, 0), new UnityTileObject("Tilemaps/Palettes/Clown", 3, -3) },
        { new GdTileObject(8, 3, 4), new UnityTileObject("Tilemaps/Palettes/Clown", -2, -6) },
        { new GdTileObject(8, 0, 1), new UnityTileObject("Tilemaps/Palettes/Clown", -1, -4) },
        { new GdTileObject(8, 1, 1), new UnityTileObject("Tilemaps/Palettes/Clown", 0, -4) },
        { new GdTileObject(8, 2, 1), new UnityTileObject("Tilemaps/Palettes/Clown", 2, -4) },
        { new GdTileObject(8, 3, 1), new UnityTileObject("Tilemaps/Palettes/Clown", 3, -4) },
        { new GdTileObject(8, 2, 2), new UnityTileObject("Tilemaps/Palettes/Clown", -4, -3) },
        { new GdTileObject(8, 3, 2), new UnityTileObject("Tilemaps/Palettes/Clown", -3, -3) },
        { new GdTileObject(8, 2, 3), new UnityTileObject("Tilemaps/Palettes/Clown", -4, -4) },
        { new GdTileObject(8, 3, 3), new UnityTileObject("Tilemaps/Palettes/Clown", -3, -4) },
        { new GdTileObject(8, 4, 0), new UnityTileObject("Tilemaps/Palettes/Clown", -3, -6) },
        { new GdTileObject(8, 4, 1), new UnityTileObject("Tilemaps/Palettes/Clown", 2, -7) },
        { new GdTileObject(8, 4, 3), new UnityTileObject("Tilemaps/Palettes/Clown", 3, -6) },
        { new GdTileObject(8, 0, 2), new UnityTileObject("Tilemaps/Palettes/Clown", -1, -6) },
        { new GdTileObject(8, 1, 2), new UnityTileObject("Tilemaps/Palettes/Clown", 0, -6) },
        { new GdTileObject(8, 0, 3), new UnityTileObject("Tilemaps/Palettes/Clown", 1, -6) },
        { new GdTileObject(8, 1, 3), new UnityTileObject("Tilemaps/Palettes/Clown", 2, -6) },
        { new GdTileObject(8, 0, 4), new UnityTileObject("Tilemaps/Palettes/Clown", 1, -7) },
        { new GdTileObject(8, 1, 4), new UnityTileObject("Tilemaps/Palettes/Clown", 0, -7) },
        { new GdTileObject(8, 2, 4), new UnityTileObject("Tilemaps/Palettes/Clown", -1, -7) },
        { new GdTileObject(5, 0, 2), new UnityTileObject("Tilemaps/Palettes/Desert", -5, 5) },
        { new GdTileObject(5, 1, 2), new UnityTileObject("Tilemaps/Palettes/Desert", -4, 5) },
        { new GdTileObject(5, 2, 2), new UnityTileObject("Tilemaps/Palettes/Desert", -3, 5) },
        { new GdTileObject(5, 0, 3), new UnityTileObject("Tilemaps/Palettes/Desert", -5, 4) },
        { new GdTileObject(5, 1, 3), new UnityTileObject("Tilemaps/Palettes/Desert", -4, 4) },
        { new GdTileObject(5, 2, 3), new UnityTileObject("Tilemaps/Palettes/Desert", -3, 4) },
        { new GdTileObject(5, 0, 4), new UnityTileObject("Tilemaps/Palettes/Desert", -5, 3) },
        { new GdTileObject(5, 1, 4), new UnityTileObject("Tilemaps/Palettes/Desert", -4, 3) },
        { new GdTileObject(5, 2, 4), new UnityTileObject("Tilemaps/Palettes/Desert", -3, 3) },
        { new GdTileObject(5, 3, 2), new UnityTileObject("Tilemaps/Palettes/Desert", 0, 4) },
        { new GdTileObject(5, 3, 3), new UnityTileObject("Tilemaps/Palettes/Desert", 0, 3) },
        { new GdTileObject(5, 4, 2), new UnityTileObject("Tilemaps/Palettes/Desert", -2, 5) },
        { new GdTileObject(5, 3, 4), new UnityTileObject("Tilemaps/Palettes/Desert", -2, 2) },
        { new GdTileObject(5, 4, 4), new UnityTileObject("Tilemaps/Palettes/Desert", -1, 2) },
        { new GdTileObject(5, 5, 2), new UnityTileObject("Tilemaps/Palettes/Desert", -2, 4) },
        { new GdTileObject(5, 6, 2), new UnityTileObject("Tilemaps/Palettes/Desert", -1, 4) },
        { new GdTileObject(5, 5, 3), new UnityTileObject("Tilemaps/Palettes/Desert", -2, 3) },
        { new GdTileObject(5, 6, 3), new UnityTileObject("Tilemaps/Palettes/Desert", -1, 3) },
        { new GdTileObject(3, 2, 2), new UnityTileObject("Tilemaps/Palettes/Snow", 0, 1) },
        { new GdTileObject(4, 8, 4), new UnityTileObject("Tilemaps/Palettes/Sky", 4, 11) },
        { new GdTileObject(4, 9, 4), new UnityTileObject("Tilemaps/Palettes/Sky", 5, 11) },
        { new GdTileObject(4, 8, 5), new UnityTileObject("Tilemaps/Palettes/Sky", 4, 10) },
        { new GdTileObject(4, 9, 5), new UnityTileObject("Tilemaps/Palettes/Sky", 5, 10) },
        { new GdTileObject(1, 15, 4), new UnityTileObject("Tilemaps/Palettes/Snow", 0, 1) },
        { new GdTileObject(8, 0, 5), new UnityTileObject("Tilemaps/Palettes/Clown", 1, -6) },
        { new GdTileObject(8, 1, 5), new UnityTileObject("Tilemaps/Palettes/Clown", 1, -6) },
        { new GdTileObject(8, 2, 5), new UnityTileObject("Tilemaps/Palettes/Clown", 1, -6) },
        { new GdTileObject(8, 0, 6), new UnityTileObject("Tilemaps/Palettes/Clown", 1, -6) },
        { new GdTileObject(8, 1, 6), new UnityTileObject("Tilemaps/Palettes/Clown", 1, -6) },
        { new GdTileObject(8, 2, 6), new UnityTileObject("Tilemaps/Palettes/Clown", 1, -6) },
        { new GdTileObject(8, 0, 7), new UnityTileObject("Tilemaps/Palettes/Clown", 1, -6) },
        { new GdTileObject(8, 1, 7), new UnityTileObject("Tilemaps/Palettes/Clown", 1, -6) },
        { new GdTileObject(8, 2, 7), new UnityTileObject("Tilemaps/Palettes/Clown", 1, -6) },
        { new GdTileObject(8, 3, 5), new UnityTileObject("Tilemaps/Palettes/Clown", 3, -6) },
        { new GdTileObject(8, 3, 6), new UnityTileObject("Tilemaps/Palettes/Clown", 3, -6) },
        { new GdTileObject(8, 3, 7), new UnityTileObject("Tilemaps/Palettes/Clown", 3, -6) },
        { new GdTileObject(8, 0, 8), new UnityTileObject("Tilemaps/Palettes/Clown", 3, -6) },
        { new GdTileObject(8, 1, 8), new UnityTileObject("Tilemaps/Palettes/Clown", 3, -6) },
        { new GdTileObject(8, 2, 8), new UnityTileObject("Tilemaps/Palettes/Clown", 3, -6) },
        { new GdTileObject(8, 3, 8), new UnityTileObject("Tilemaps/Palettes/Clown", 3, -6) },
        { new GdTileObject(0, 0, 5), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 1, 5), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 2, 5), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 3, 5), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 4, 5), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(0, 5, 5), new UnityTileObject("Tilemaps/Palettes/Grassland", 6, -9) },
        { new GdTileObject(6, 0, 0), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 1, 0), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 2, 0), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 3, 0), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 4, 0), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 5, 0), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 6, 0), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 7, 0), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 8, 0), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 9, 0), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 10, 0), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 11, 0), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 12, 0), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 13, 0), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 14, 0), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 15, 0), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 0, 1), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 0, 2), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 1, 1), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 2, 1), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 2, 2), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 2, 3), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 2, 4), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 3, 1), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 3, 2), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 3, 3), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 4, 1), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 5, 1), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 6, 1), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 7, 1), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 8, 1), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 9, 1), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 10, 1), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 11, 1), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 12, 1), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 13, 1), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 14, 1), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 4, 2), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 4, 3), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 5, 2), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 5, 3), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 5, 4), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 5, 5), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 6, 2), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 6, 3), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 6, 4), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 7, 2), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 7, 3), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 8, 2), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 8, 3), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 8, 4), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 9, 2), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 9, 3), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 9, 4), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 9, 5), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 9, 6), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 10, 2), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 10, 3), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 10, 4), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 10, 5), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 11, 2), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 11, 3), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 11, 4), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 12, 2), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 12, 3), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 13, 2), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(6, 14, 2), new UnityTileObject("Tilemaps/Palettes/Jungle", -1, 1) },
        { new GdTileObject(127, 0, 18), new UnityItemObject("Prefabs/FloatingCoin", true) },
        { new GdTileObject(127, 1, 17), new UnityItemObject("Prefabs/Static/RespawningInvisibleBlock") }
    };

    private static readonly Dictionary<ItemTypes, IUnityObject> ItemMapping = new()       // item type in gd
    {
        { ItemTypes.KoopaGreen, new UnityItemObject("Prefabs/Enemy/Koopa") },
        { ItemTypes.KoopaRed, new UnityItemObject("Prefabs/Enemy/RedKoopa") },
        { ItemTypes.KoopaBlue, new UnityItemObject("Prefabs/Enemy/BlueKoopa") },
        { ItemTypes.Goomba, new UnityItemObject("Prefabs/Enemy/Goomba") },
        { ItemTypes.Spiny, new UnityItemObject("Prefabs/Enemy/Spiny") },
        { ItemTypes.Spinner, new UnityItemObject("Prefabs/Static/Spinner") },
        { ItemTypes.PlatMariobros, new UnityItemObject("Prefabs/Static/MarioBrosPlatform", true) },
        { ItemTypes.PlatCloud, new UnityItemObject("Prefabs/Static/CloudPlatform") }
    };

    private static readonly UnityTileObject DefaultTile = new UnityTileObject("Tilemaps/Palettes/Clown", 3, -7);

    // stars: empty gameobject with StarSpawn tag
    // spinner: prefab instance
    // bill launcher: prefab instance (+ tiles)
    // coins: prefab instance
    // piranha plants: prefab instance
    // pipes: prefab instance (+ tiles)
    // invisible blocks: prefab instance
    // clouds: prefab instance
    // semi mushrooms: tiles (in semis grid)
    // mariobros plats: prefab instance
    // squishy: tiles (in squishy grid)
    // rest of enemies: empty gameobject with enemyspawnpoint component
    public void BuildLevelFromContents(Dictionary<string, object> contents)
    {
        if (contents["t"] is not object[] tiles || contents["i"] is not object[] items || contents["p"] is not Dictionary<string, object> properties)
        {
            Debug.LogError("Invalid level content format");
            return;
        }

        parent.levelWidthTile = Convert.ToInt32((long)properties["w"]);
        parent.levelHeightTile = Convert.ToInt32((long)properties["h"]) + 3;    // compensate for the deathplane.
        parent.cameraMaxX = (float)(parent.levelWidthTile / 2.0);
        parent.cameraHeightY = (float)(Convert.ToDouble((long)properties["h"]) / 2.0);

        var themeIndex = Convert.ToInt32((long)properties["t"]);
        for (var i = 0; i < parent.backgroundsHolder.childCount; i++)
        {
            var bgLayer = parent.backgroundsHolder.GetChild(i).gameObject;
            if (i != themeIndex) continue;
            bgLayer.SetActive(true);
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<BackgroundLoop>().InitializeBackground(bgLayer.transform);
        }

        var pitType = Convert.ToInt32((long)properties["p"]);
        for (var i = 0; i < parent.pitsHolder.childCount; i++)
        {
            var pit = parent.pitsHolder.GetChild(i).gameObject;
            if (i != pitType) continue;
            pit.SetActive(true);
            if (i == 0) continue;
            pit.GetComponent<WaterSplash>().widthTiles = parent.levelWidthTile;
            var pitPosition = pit.transform.position;
            pitPosition.x = parent.levelWidthTile / 2.0f / 2.0f;
            pit.transform.position = pitPosition;
        }

        foreach (var tile in tiles)
        {
            if (tile is not Dictionary<string, object> tileDict)
            {
                Debug.LogError("Invalid tile format");
                continue;
            }

            var targetObject = TileMapping.FirstOrDefault(x =>
                x.Key.TilesetId == Convert.ToInt32((long)tileDict["t"]) &&
                x.Key.TileIdX == Convert.ToInt32((long)tileDict["tx"]) &&
                x.Key.TileIdY == Convert.ToInt32((long)tileDict["ty"]) &&
                x.Key.AlternativeId == Convert.ToInt32((long)tileDict["a"])
            ).Value ?? DefaultTile;

            // source is tile...
            var targetPosition = new Vector3(Convert.ToInt32((long)tileDict["x"]), Convert.ToInt32((long)tileDict["y"]));
            var background = (bool)tileDict["b"];

            switch (targetObject)
            {
                // ...target is tile. examples: ground, wall, terrain.
                case UnityTileObject tileObject:
                {
                    PutTile(tileObject, targetPosition.GdTileToUnityTile(), background ? GridTypes.Background : GridTypes.Normal);
                    break;
                }

                // ...target is item. examples: coin.
                case UnityItemObject itemObject:
                {
                    PutPrefab(itemObject, targetPosition.GdTileToUnityWorld(), itemsFolder);
                    break;
                }
            }
        }

        foreach (var item in items)
        {
            if (item is not Dictionary<string, object> itemDict)
            {
                Debug.LogError("Invalid item format");
                continue;
            }

            // source is item...
            var itemType = (ItemTypes)(long)itemDict["t"];
            var targetPos = new Vector3(Convert.ToInt32((long)itemDict["x"]), Convert.ToInt32((long)itemDict["y"]), 0);
            var itemProperties = itemDict["p"] as Dictionary<string, object>;

            if (HandleSpecialItemMapping(itemType, targetPos, itemProperties)) continue;

            var targetObject = ItemMapping[itemType];

            switch (targetObject)
            {
                // ...target is tile.
                case UnityTileObject tileObject:
                {
                    // TODO: a situation in which this happens is yet to be found.
                    PutTile(DefaultTile, targetPos.GdWorldToUnityTile());
                    break;
                }

                // ...target is item. examples: enemies.
                case UnityItemObject itemObject:
                {
                    // var itemPrefab = Resources.Load<GameObject>(itemObject.ItemPrefabPath);
                    switch (itemType)
                    {
                        case ItemTypes.KoopaGreen:
                        case ItemTypes.KoopaRed:
                        case ItemTypes.KoopaBlue:
                        case ItemTypes.Goomba:
                        case ItemTypes.Spiny:
                        {
                            // gameobject with enemyspawnpoint component.
                            var enemySpawnpoint = new GameObject("EnemySpawn")
                            {
                                transform =
                                {
                                    parent = spawnsFolder.transform,
                                    position = targetPos.GdWorldToUnityWorld()
                                }
                            };
                            enemySpawnpoint.AddComponent<EnemySpawnpoint>().prefab = itemObject.ItemPrefabPath;
                            break;
                        }
                        case ItemTypes.Spinner:
                        {
                            // just a prefab instance.
                            PutPrefab(itemObject, targetPos.GdWorldToUnityWorld(), platformsFolder);
                            break;
                        }
                        case ItemTypes.PlatMariobros:
                        {
                            // same jazz, but there are some properties to be set too.
                            var platform = PutPrefab(itemObject, targetPos.GdWorldToUnityWorld() + new Vector3(1, -0.25f, 0), platformsFolder, "MarioBrosPlatform") as MarioBrosPlatform;
                            if (itemProperties != null && platform) platform.platformWidth = Convert.ToInt32((long)itemProperties["width"]) + 1;
                            break;
                        }
                        case ItemTypes.PlatCloud:
                        {
                            var platform = PutPrefab(itemObject, targetPos.GdWorldToUnityWorld() - Vector3.one * 0.5f, platformsFolder, "CloudPlatform") as CloudPlatform;
                            if (itemProperties != null && platform)
                            {
                                platform.platformWidth = Convert.ToInt32((long)itemProperties["width"]) + 3;
                            }
                            break;
                        }
                        default: PutTile(DefaultTile, targetPos.GdWorldToUnityTile()); break;
                    }
                    break;
                }
            }
        }
    }

    private bool HandleSpecialItemMapping(ItemTypes itemType, Vector3 targetPos, Dictionary<string, object> properties)
    {
        // source is item...
        // ...target is extra logic when being placed, i.e. multiple tiles or prefabs. overrides default mapping.
        // examples: pipes, semisolids.
        var targetPosTile = targetPos.GdWorldToUnityTile();
        var targetPosWorld = targetPos.GdWorldToUnityWorld();

        switch (itemType)
        {
            case ItemTypes.BulletLauncher:
            {
                var height = Convert.ToInt32((long)properties["height"]);
                var launcherTop1 = new UnityTileObject("Tilemaps/Palettes/Snow", -3, 2);
                var launcherTop2 = new UnityTileObject("Tilemaps/Palettes/Snow", -3, 1);
                var launcherMid = new UnityTileObject("Tilemaps/Palettes/Snow", -3, 0);
                PutTile(launcherTop1, new Vector3(targetPosTile.x, targetPosTile.y + height / 2.0f - 1, 0));
                PutTile(launcherTop2, new Vector3(targetPosTile.x, targetPosTile.y + height / 2.0f - 2, 0));
                for (var i = targetPosTile.y + height / 2.0f - 3; i > targetPosTile.y - height / 2.0f - 1; i--)
                {
                    PutTile(launcherMid, new Vector3(targetPosTile.x, i, 0));
                }

                PutPrefab("Prefabs/Static/BulletBillLauncher",
                    new Vector3(targetPosTile.x, targetPosTile.y + height / 2.0f - 0.5f, 0) / 2.0f, itemsFolder);
                return true;
            }
            case ItemTypes.Spawn:
            {
                parent.spawnpoint = targetPos.GdWorldToUnityWorld();
                return true;
            }
            case ItemTypes.Star:
            {
                // empty gameobject with StarSpawn tag.
                _ = new GameObject("StarSpawn")
                {
                    transform =
                    {
                        parent = spawnsFolder.transform,
                        position = targetPos.GdWorldToUnityWorld()
                    },
                    tag = "StarSpawn"
                };
                return true;
            }
            case ItemTypes.Squishy:
            {
                var width = Convert.ToInt32((long)properties["width"]);
                var height = Convert.ToInt32((long)properties["height"]);
                var squishyTopLeft = new UnityTileObject("Tilemaps/Palettes/Castle", 6, 4);
                var squishyTopMid = new UnityTileObject("Tilemaps/Palettes/Castle", 7, 4);
                var squishyTopRight = new UnityTileObject("Tilemaps/Palettes/Castle", 8, 4);
                var squishyMidLeft = new UnityTileObject("Tilemaps/Palettes/Castle", 8, 3);
                var squishyMidMid = new UnityTileObject("Tilemaps/Palettes/Castle", 7, 3);
                var squishyMidRight = new UnityTileObject("Tilemaps/Palettes/Castle", 6, 3);
                var squishyBotLeft = new UnityTileObject("Tilemaps/Palettes/Castle", 8, 2);
                var squishyBotMid = new UnityTileObject("Tilemaps/Palettes/Castle", 7, -1);
                var squishyBotRight = new UnityTileObject("Tilemaps/Palettes/Castle", 6, 2);
                var boundsTop = targetPosTile.y - 1;
                var boundsLeft = targetPosTile.x;
                var boundsBottom = targetPosTile.y - height;
                var boundsRight = targetPosTile.x + width - 1;
                for (var i = boundsTop; i >= boundsBottom; i--)
                {
                    PutTile(squishyMidLeft, new Vector3(boundsLeft, i, 0), GridTypes.Squishy);
                    PutTile(squishyMidRight, new Vector3(boundsRight, i, 0), GridTypes.Squishy);
                    for (var j = boundsLeft + 1; j <= boundsRight; j++)
                    {
                        if (i == boundsTop) PutTile(squishyTopMid, new Vector3(j, i, 0), GridTypes.Squishy);
                        else if (i == boundsBottom - 1) PutTile(squishyBotMid, new Vector3(j, i, 0), GridTypes.Squishy);
                        else PutTile(squishyMidMid, new Vector3(j, i, 0), GridTypes.Squishy);
                    }
                }
                PutTile(squishyTopLeft, new Vector3(boundsLeft, boundsTop, 0), GridTypes.Squishy);
                PutTile(squishyTopRight, new Vector3(boundsRight, boundsTop, 0), GridTypes.Squishy);
                PutTile(squishyBotLeft, new Vector3(boundsLeft, boundsBottom, 0), GridTypes.Squishy);
                PutTile(squishyBotRight, new Vector3(boundsRight, boundsBottom, 0), GridTypes.Squishy);
                return true;
            }
            case ItemTypes.Pipe:
            {
                var color = Convert.ToInt32((long)properties["color"]);     // 0 Green, 1 Yellow, 2 Red, 3 Blue
                var rotation = Convert.ToInt32((long)properties["rotation"]);   // 0 Up, 1 Right, 2 Down, 3 Left
                var height = Convert.ToInt32((long)properties["height"]);
                var enterable = (bool)properties["enterable"];
                var pipeUpLeft = new UnityTileObject("Tilemaps/Palettes/Basic Blocks", 4, 1).Y(-4 * color);
                var pipeUpRight = pipeUpLeft.X(1);
                var pipeVertLeft = pipeUpLeft.Y(-1);
                var pipeVertRight = pipeVertLeft.X(1);
                var pipeDownLeft = pipeVertLeft.Y(-1);
                var pipeDownRight = pipeDownLeft.X(1);
                var pipeLeftUp = new UnityTileObject("Tilemaps/Palettes/Basic Blocks", 7, 1).Y(-4 * color);
                var pipeLeftDown = pipeLeftUp.Y(-1);
                var pipeHorizUp = pipeLeftUp.X(1);
                var pipeHorizDown = pipeHorizUp.Y(-1);
                var pipeRightUp = pipeHorizUp.X(1);
                var pipeRightDown = pipeRightUp.Y(-1);
                var pipeBreakUpLeft = new UnityTileObject("Tilemaps/Palettes/Basic Blocks", 11, 1).Y(-4 * color);
                var pipeBreakUpRight = pipeBreakUpLeft.X(1);
                var pipeBreakUpMidLeft = pipeBreakUpLeft.Y(-1);
                var pipeBreakUpMidRight = pipeBreakUpMidLeft.X(1);
                var pipeBreakDownLeft = new UnityTileObject("Tilemaps/Palettes/Basic Blocks", 14, -1).Y(-4 * color);
                var pipeBreakDownRight = pipeBreakDownLeft.X(1);
                var pipeBreakDownMidLeft = pipeBreakDownLeft.Y(1);
                var pipeBreakDownMidRight = pipeBreakDownMidLeft.X(1);
                if (!enterable)
                {
                    // all four rotations are possible. up and down are breakable, left and right are not.
                    switch (rotation)
                    {
                        case 0:
                            PutTile(pipeBreakUpLeft,
                                new Vector3(targetPosTile.x - 1, targetPosTile.y + height / 2.0f - 1, 0));
                            PutTile(pipeBreakUpRight, new Vector3(targetPosTile.x, targetPosTile.y + height / 2.0f - 1, 0));
                            for (var i = targetPosTile.y + height / 2.0f - 2; i > targetPosTile.y - height / 2.0f - 1; i--)
                            {
                                PutTile(pipeBreakUpMidLeft, new Vector3(targetPosTile.x - 1, i, 0));
                                PutTile(pipeBreakUpMidRight, new Vector3(targetPosTile.x, i, 0));
                            }
                            break;
                        case 1:
                            PutTile(pipeRightUp, new Vector3(targetPosTile.x + height / 2.0f - 1, targetPosTile.y, 0));
                            PutTile(pipeRightDown, new Vector3(targetPosTile.x + height / 2.0f - 1, targetPosTile.y - 1, 0));
                            for (var i = targetPosTile.x + height / 2.0f - 2; i > targetPosTile.x - height / 2.0f - 1; i--)
                            {
                                PutTile(pipeHorizUp, new Vector3(i, targetPosTile.y, 0));
                                PutTile(pipeHorizDown, new Vector3(i, targetPosTile.y - 1, 0));
                            }
                            break;
                        case 2:
                            PutTile(pipeBreakDownLeft,
                                new Vector3(targetPosTile.x - 1, targetPosTile.y - height / 2.0f, 0));
                            PutTile(pipeBreakDownRight, new Vector3(targetPosTile.x, targetPosTile.y - height / 2.0f, 0));
                            for (var i = targetPosTile.y - height / 2.0f + 1; i < targetPosTile.y + height / 2.0f; i++)
                            {
                                PutTile(pipeBreakDownMidLeft, new Vector3(targetPosTile.x - 1, i, 0));
                                PutTile(pipeBreakDownMidRight, new Vector3(targetPosTile.x, i, 0));
                            }
                            break;
                        case 3:
                            PutTile(pipeLeftUp, new Vector3(targetPosTile.x - height / 2.0f, targetPosTile.y, 0));
                            PutTile(pipeLeftDown, new Vector3(targetPosTile.x - height / 2.0f, targetPosTile.y - 1, 0));
                            for (var i = targetPosTile.x - height / 2.0f + 1; i < targetPosTile.x + height / 2.0f; i++)
                            {
                                PutTile(pipeHorizUp, new Vector3(i, targetPosTile.y, 0));
                                PutTile(pipeHorizDown, new Vector3(i, targetPosTile.y - 1, 0));
                            }
                            break;
                    }
                }
                else {
                    // only up and down are permitted. both are not breakable.
                    switch (rotation)
                    {
                        case 0:
                            PutTile(pipeUpLeft,
                                new Vector3(targetPosTile.x - 1, targetPosTile.y + height / 2.0f - 1, 0));
                            PutTile(pipeUpRight, new Vector3(targetPosTile.x, targetPosTile.y + height / 2.0f - 1, 0));
                            for (var i = targetPosTile.y + height / 2.0f - 2; i > targetPosTile.y - height / 2.0f - 1; i--)
                            {
                                PutTile(pipeVertLeft, new Vector3(targetPosTile.x - 1, i, 0));
                                PutTile(pipeVertRight, new Vector3(targetPosTile.x, i, 0));
                            }
                            break;
                        case 2:
                            PutTile(pipeDownLeft,
                                new Vector3(targetPosTile.x - 1, targetPosTile.y - height / 2.0f, 0));
                            PutTile(pipeDownRight, new Vector3(targetPosTile.x, targetPosTile.y - height / 2.0f, 0));
                            for (var i = targetPosTile.y - height / 2.0f + 1; i < targetPosTile.y + height / 2.0f; i++)
                            {
                                PutTile(pipeVertLeft, new Vector3(targetPosTile.x - 1, i, 0));
                                PutTile(pipeVertRight, new Vector3(targetPosTile.x, i, 0));
                            }
                            break;
                    }
                }
                return true;
            }
            case ItemTypes.PipeMini:
            {
                var rotation = Convert.ToInt32((long)properties["rotation"]);   // 0 Up, 1 Right, 2 Down, 3 Left
                var height = Convert.ToInt32((long)properties["height"]);
                var enterable = (bool)properties["enterable"];
                var pipeUp = new UnityTileObject("Tilemaps/Palettes/Basic Blocks", -2, -4);
                var pipeVert = pipeUp.Y(-1);
                var pipeDown = pipeVert.Y(-1);
                var pipeLeft = new UnityTileObject("Tilemaps/Palettes/Basic Blocks", 0, -4);
                var pipeHoriz = pipeLeft.X(1);
                var pipeRight = pipeHoriz.X(1);
                // all four rotations are possible. up and down are breakable, left and right are not.
                switch (rotation)
                {
                    case 0:
                        PutTile(pipeUp, new Vector3(targetPosTile.x, targetPosTile.y + height / 2.0f - 1, 0));
                        for (var i = targetPosTile.y + height / 2.0f - 2; i > targetPosTile.y - height / 2.0f - 1; i--)
                        {
                            PutTile(pipeVert, new Vector3(targetPosTile.x, i, 0));
                        }
                        break;
                    case 1:
                        PutTile(pipeRight, new Vector3(targetPosTile.x + height / 2.0f - 1, targetPosTile.y, 0));
                        for (var i = targetPosTile.x + height / 2.0f - 2; i > targetPosTile.x - height / 2.0f - 1; i--)
                        {
                            PutTile(pipeHoriz, new Vector3(i, targetPosTile.y, 0));
                        }
                        break;
                    case 2:
                        PutTile(pipeDown, new Vector3(targetPosTile.x, targetPosTile.y - height / 2.0f, 0));
                        for (var i = targetPosTile.y - height / 2.0f + 1; i < targetPosTile.y + height / 2.0f; i++)
                        {
                            PutTile(pipeVert, new Vector3(targetPosTile.x, i, 0));
                        }
                        break;
                    case 3:
                        PutTile(pipeLeft, new Vector3(targetPosTile.x - height / 2.0f, targetPosTile.y, 0));
                        for (var i = targetPosTile.x - height / 2.0f + 1; i < targetPosTile.x + height / 2.0f; i++)
                        {
                            PutTile(pipeHoriz, new Vector3(i, targetPosTile.y, 0));
                        }
                        break;
                }
                return true;
            }
            case ItemTypes.SemiMushroom:
            {
                var width = Convert.ToInt32((long)properties["width"]) + 3;
                var height = Convert.ToInt32((long)properties["height"]);
                var color = Convert.ToInt32((long)properties["color"]);     // 0 Red, 1 Green
                var semiLeft0 = new UnityTileObject("Tilemaps/Palettes/Sky", 2, 8).X(6 * color);
                var semiLeft1 = semiLeft0.X(1);
                var semiLeft2 = semiLeft0.Y(-1);
                var semiLeft3 = semiLeft2.X(1);
                var semiMid0 = semiLeft1.X(1);
                var semiMid1 = semiMid0.X(1);
                var semiMid2 = semiMid0.Y(-1);
                var semiMid3 = semiMid2.X(1);
                var semiRight0 = semiMid1.X(1);
                var semiRight1 = semiRight0.X(1);
                var semiRight2 = semiRight0.Y(-1);
                var semiRight3 = semiRight2.X(1);
                var semiBase0 = new UnityTileObject("Tilemaps/Palettes/Sky", 4, 6).X(6 * color);
                var semiBase1 = semiBase0.X(1);
                var semiTail0 = semiBase0.Y(-1);
                var semiTail1 = semiTail0.X(1);
                for (var i = targetPosTile.y + height / 2.0f - 2; i > targetPosTile.y - height / 2.0f - 1; i--)
                {
                    PutTile(semiTail0, new Vector3(targetPosTile.x - 1, i, 0));
                    PutTile(semiTail1, new Vector3(targetPosTile.x, i, 0));
                }
                PutTile(semiBase0, new Vector3(targetPosTile.x - 1, targetPosTile.y + height / 2.0f - 1, 0), GridTypes.Semisolid);
                PutTile(semiBase1, new Vector3(targetPosTile.x, targetPosTile.y + height / 2.0f - 1, 0), GridTypes.Semisolid);
                for (var i = targetPosTile.x - width + 3; i < targetPosTile.x + width - 4; i += 2)
                {
                    PutTile(semiMid0, new Vector3(i, targetPosTile.y + height / 2.0f + 1, 0), GridTypes.Semisolid);
                    PutTile(semiMid1, new Vector3(i + 1, targetPosTile.y + height / 2.0f + 1, 0), GridTypes.Semisolid);
                    PutTile(semiMid2, new Vector3(i, targetPosTile.y + height / 2.0f, 0), GridTypes.Semisolid);
                    PutTile(semiMid3, new Vector3(i + 1, targetPosTile.y + height / 2.0f, 0), GridTypes.Semisolid);
                }
                PutTile(semiLeft0, new Vector3(targetPosTile.x - width + 1, targetPosTile.y + height / 2.0f + 1, 0), GridTypes.Semisolid);
                PutTile(semiLeft1, new Vector3(targetPosTile.x - width + 2, targetPosTile.y + height / 2.0f + 1, 0), GridTypes.Semisolid);
                PutTile(semiLeft2, new Vector3(targetPosTile.x - width + 1, targetPosTile.y + height / 2.0f, 0), GridTypes.Semisolid);
                PutTile(semiLeft3, new Vector3(targetPosTile.x - width + 2, targetPosTile.y + height / 2.0f, 0), GridTypes.Semisolid);
                PutTile(semiRight0, new Vector3(targetPosTile.x + width - 3, targetPosTile.y + height / 2.0f + 1, 0), GridTypes.Semisolid);
                PutTile(semiRight1, new Vector3(targetPosTile.x + width - 2, targetPosTile.y + height / 2.0f + 1, 0), GridTypes.Semisolid);
                PutTile(semiRight2, new Vector3(targetPosTile.x + width - 3, targetPosTile.y + height / 2.0f, 0), GridTypes.Semisolid);
                PutTile(semiRight3, new Vector3(targetPosTile.x + width - 2, targetPosTile.y + height / 2.0f, 0), GridTypes.Semisolid);
                return true;
            }
            case ItemTypes.SemiMushroomMini:
            {
                var width = Convert.ToInt32((long)properties["width"]) + 3;
                var height = Convert.ToInt32((long)properties["height"]);
                var color = Convert.ToInt32((long)properties["color"]);     // 0 Red, 1 Green
                PutTile(DefaultTile, new Vector3(targetPosTile.x, targetPosTile.y + height / 2.0f - 1, 0));
                // TODO
                // var semi = new UnityTileObject("Tilemaps/Palettes/Sky", -6, 1).X(-2 * color);
                // for (var i = targetPosTile.y + height / 2.0f - 2; i > targetPosTile.y - height / 2.0f - 1; i--)
                // {
                //     PutTile(semi, new Vector3(targetPosTile.x, i, 0));
                // }
                // for (var i = targetPosTile.x - width; i < targetPosTile.x + width; i++)
                // {
                //     PutTile(semi, new Vector3(i, targetPosTile.y + height / 2.0f, 0));
                // }
                // PutTile(semi, new Vector3(targetPosTile.x - width + 1, targetPosTile.y + height / 2.0f + 1, 0));
                // PutTile(semi, new Vector3(targetPosTile.x + width - 3, targetPosTile.y + height / 2.0f + 1, 0));
                return true;
            }
            default: return false;
        }
    }

    private void PutTile(UnityTileObject tileObject, Vector3 targetPos, GridTypes targetGrid = GridTypes.Normal)
    {
        // TODO: temporary solution. must eventually add a way to reuse objects! *
        var sourceIdPos = new Vector3Int(tileObject.TileIdX, tileObject.TileIdY, 0);
        var targetTilemap = targetGrid switch
        {
            GridTypes.Background => parent.tilemapBackground,
            GridTypes.Semisolid => parent.tilemapSemisolid,
            GridTypes.Squishy => parent.tilemapSquishy,
            _ => parent.tilemap
        };
        var tilePalette = Resources.Load<GameObject>(tileObject.TilePalettePrefabPath).GetComponentInChildren<Tilemap>();   // *
        var usingTile = tilePalette.GetTile(sourceIdPos);
        var usingTileTransform = tilePalette.GetTransformMatrix(sourceIdPos);
        targetTilemap.SetTile(targetPos.Flatten(), usingTile);
        targetTilemap.SetTransformMatrix(targetPos.Flatten(), usingTileTransform);
    }

    private Component PutPrefab(UnityItemObject itemObject, Vector3 targetPos, GameObject parentFolder, string componentName = null)
    {
        return PutPrefab(itemObject.ItemPrefabPath, targetPos, parentFolder, componentName, itemObject.NeedsNetworkInstantiation);
    }

    private Component PutPrefab(string prefabPath, Vector3 targetPos, GameObject parentFolder, string componentName = null, bool networked = false)
    {
        if (networked && !PhotonNetwork.IsMasterClient) return null;
        var itemInstance = networked
            ? PhotonNetwork.InstantiateRoomObject(prefabPath, targetPos, Quaternion.identity)
            : Instantiate(Resources.Load<GameObject>(prefabPath), targetPos, Quaternion.identity);
        itemInstance.transform.parent = parentFolder.transform;
        return componentName != null ? itemInstance.GetComponent(componentName) : itemInstance.transform;
    }
}

public static class Vector3Extensions
{
    public static Vector3 GdTileToUnityTile(this Vector3 input) => new(input.x, -input.y + 44);
    public static Vector3 GdWorldToUnityWorld(this Vector3 input) => new(input.x / 32.0f, -input.y / 32.0f + 22.5f);
    public static Vector3 GdTileToUnityWorld(this Vector3 input) => new(input.x * 16 / 32.0f + 0.25f, -input.y * 16 / 32.0f + 22.25f);
    public static Vector3 GdWorldToUnityTile(this Vector3 input) => input.GdWorldToUnityWorld() * 2;
    public static Vector3Int Flatten(this Vector3 input) => new((int)input.x, (int)input.y, 0);
}
