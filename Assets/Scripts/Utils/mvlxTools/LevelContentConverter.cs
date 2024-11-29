using System;
using System.Collections.Generic;
using System.Linq;
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
    }

    private readonly struct UnityItemObject : IUnityObject
    {
        public readonly string ItemPrefabPath;

        public UnityItemObject(string itemPrefabPath)
        {
            ItemPrefabPath = itemPrefabPath;
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
        { new GdTileObject(127, 0, 18), new UnityItemObject("Prefabs/FloatingCoin") },
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
        { ItemTypes.PlatMariobros, new UnityItemObject("Prefabs/Static/MarioBrosPlatform") },
        { ItemTypes.PlatCloud, new UnityItemObject("Prefabs/Static/CloudPlatform") }
    };

    private static readonly UnityTileObject DefaultTile = new UnityTileObject("Tilemaps/Palettes/Basic Blocks", 1, -1);

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
            var targetX = Convert.ToInt32((long)tileDict["x"]);
            var targetY = Convert.ToInt32((long)tileDict["y"]);
            var correctedTargetY = -targetY + 44;    // Y-axis is inverted in Unity, so we need to adjust it. 44 is the maximum height of a stage.
            var background = tileDict["b"];

            switch (targetObject)
            {
                // ...target is tile. examples: ground, wall, terrain.
                case UnityTileObject tileObject:
                {
                    PutTile(tileObject.TilePalettePrefabPath, tileObject.TileIdX, tileObject.TileIdY, targetX,
                        correctedTargetY);
                    break;
                }

                // ...target is item. examples: coin.
                case UnityItemObject itemObject:
                {
                    PutPrefab(itemObject.ItemPrefabPath, (float)(targetX * 16 / 32.0),
                        (float)(-targetY * 16 / 32.0 + 22), itemsFolder);
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
            var targetX = Convert.ToInt32((long)itemDict["x"]);
            var targetY = Convert.ToInt32((long)itemDict["y"]);
            var correctedTargetX = (float)(targetX / 32.0);  // 16/0.5 is the ratio between gd and unity units.
            var correctedTargetY = (float)(-targetY / 32.0 + 22);  // Y-axis inversion again before applying scale ratio. Also, shift up 22 units to align origin (1 unity tile = 0.5 units, and 44 is the maximum height of a stage).
            var itemProperties = itemDict["p"] as Dictionary<string, object>;

            if (HandleSpecialItemMapping(itemType, correctedTargetX, correctedTargetY, itemProperties)) continue;

            var targetObject = ItemMapping[itemType];

            switch (targetObject)
            {
                // ...target is tile.
                case UnityTileObject tileObject:
                {
                    PutTile(tileObject.TilePalettePrefabPath, tileObject.TileIdX, tileObject.TileIdY,
                        (int)Math.Floor(correctedTargetX), (int)Math.Floor(correctedTargetY));
                    break;
                }

                // ...target is item. examples: enemies.
                case UnityItemObject itemObject:
                {
                    // var itemPrefab = Resources.Load<GameObject>(itemObject.ItemPrefabPath);
                    // TODO: instantiate the prefab in a networked way, in gamemanager. use enemyspawnpoint where needed.
                    switch (itemType)
                    {
                        // TODO: item type-specific properties
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
                                    position = new Vector3(correctedTargetX, correctedTargetY, 0)
                                }
                            };
                            enemySpawnpoint.AddComponent<EnemySpawnpoint>().prefab = itemObject.ItemPrefabPath;
                            break;
                        }
                        case ItemTypes.Spinner:
                        {
                            // just a prefab instance.
                            PutPrefab(itemObject.ItemPrefabPath, correctedTargetX, correctedTargetY, platformsFolder);
                            break;
                        }
                        case ItemTypes.PlatMariobros:
                        {
                            // same jazz, but there are some properties to be set too.
                            var platform = PutPrefab(itemObject.ItemPrefabPath, correctedTargetX, correctedTargetY, platformsFolder, "MarioBrosPlatform") as MarioBrosPlatform;
                            if (itemProperties != null && platform) platform.platformWidth = Convert.ToInt32((long)itemProperties["width"]);
                            break;
                        }
                        case ItemTypes.PlatCloud:
                        {
                            var platform = PutPrefab(itemObject.ItemPrefabPath, correctedTargetX, correctedTargetY, platformsFolder, "CloudPlatform") as CloudPlatform;
                            if (itemProperties != null && platform) platform.platformWidth = Convert.ToInt32((long)itemProperties["width"]);
                            break;
                        }
                    }
                    break;
                }
            }
        }
    }

    private bool HandleSpecialItemMapping(ItemTypes itemType, float targetX, float targetY, Dictionary<string, object> properties)
    {
        // source is item...
        // ...target is extra logic when being placed, i.e. multiple tiles or prefabs. overrides default mapping.
        // examples: pipes, semisolids.
        switch (itemType)
        {
            // TODO
            case ItemTypes.BulletLauncher:
            {
                return true;
            }
            case ItemTypes.Spawn:
            {
                parent.spawnpoint = new Vector3(targetX, targetY, 0);
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
                        position = new Vector3(targetX, targetY, 0)
                    },
                    tag = "StarSpawn"
                };
                return true;
            }
            case ItemTypes.Squishy:
            {
                return true;
            }
            case ItemTypes.Pipe:
            {
                return true;
            }
            case ItemTypes.PipeMini:
            {
                return true;
            }
            case ItemTypes.SemiMushroom:
            {
                return true;
            }
            case ItemTypes.SemiMushroomMini:
            {
                return true;
            }
            default: return false;
        }
    }

    private void PutTile(string tilePalettePath, int tileIdX, int tileIdY, int targetX, int targetY)
    {
        // TODO: temporary solution. must eventually add a way to reuse objects!
        // TODO: add a way to reference all four (ground, semi, background, squishy) types of tilemaps.
        var tilePalette = Resources.Load<GameObject>(tilePalettePath).GetComponentInChildren<Tilemap>();
        var usingTile = tilePalette.GetTile(new Vector3Int(tileIdX, tileIdY, 0));
        parent.tilemap.SetTile(new Vector3Int(targetX, targetY, 0), usingTile);
    }

    private Component PutPrefab(string prefabPath, float targetX, float targetY, GameObject parentFolder, string componentName = null)
    {
        Debug.Log(prefabPath);
        var itemInstance = Instantiate(Resources.Load<GameObject>(prefabPath), new Vector3(targetX, targetY, 0), Quaternion.identity);
        itemInstance.transform.parent = parentFolder.transform;
        return componentName != null ? itemInstance.GetComponent(componentName) : itemInstance.transform;
    }
}
