using Photon.Deterministic;
using Quantum;
using Quantum.Profiling;
using UnityEngine;

public unsafe class VersusStageData : AssetObject {

    //---Properties
    public FPVector2 StageWorldMin => new FPVector2(TileOrigin.X, TileOrigin.Y) / 2 + TilemapWorldPosition;
    public FPVector2 StageWorldMax => new FPVector2(TileOrigin.X + TileDimensions.X, TileOrigin.Y + TileDimensions.Y) / 2 + TilemapWorldPosition;
    public FPVector2 StageWorldMidpoint => (StageWorldMin + StageWorldMax) / 2;

    //---Serialized
    [Header("-- Information")]
    public bool ShowAuthorAndComposer;
    public string StageAuthor;
    public string MusicComposer;
    public string LegalEnglishName;
    public int WorldIndex;
#if QUANTUM_UNITY
    public Sprite Icon;
    public Sprite GroundSprite;
#endif

    [Header("-- Tilemap")]
    public bool OverrideAutomaticTilemapSettings;
    public IntVector2 TileDimensions;
    public IntVector2 TileOrigin;
    public FPVector2 TilemapWorldPosition;
    public bool IsWrappingLevel = true;
    public bool ExtendCeilingHitboxes = false;

    [Header("-- Spawnpoints")]
    public FPVector2 Spawnpoint;
    public FPVector2 SpawnpointArea;
    public FPVector2 Checkpoint;

    [Header("-- Camera")]
    public bool OverrideAutomaticCameraSettings;
    public bool ForceOneScreenCameraHeight;
    public bool NoHorizontalCameraMovement;
    public FPVector2 CameraMinPosition;
    public FPVector2 CameraMaxPosition;

    // [Header("-- UI")]
    // public ColorRGBA UIColor = new(24, 178, 170);

    [Header("-- Modifiers")]
    public bool SpawnBigPowerups = true;
    public bool SpawnVerticalPowerups = true;
    public bool ReverbSfx = false;
    public bool VerticalMap = false;

    // [Header("-- Music")]
    // public AssetRef<LoopingMusicData>[] MainMusic;
    // public AssetRef<LoopingMusicData> InvincibleMusic;
    // public AssetRef<LoopingMusicData> MegaMushroomMusic;


    [HideInInspector] public StageTileInstance[] TileData;
    [HideInInspector] public FPVector2[] BigStarSpawnpoints;
    [HideInInspector] public bool IsCampaignMap = false;
    [HideInInspector] public bool HidePlayersOnMinimap;

    // public AssetRef<LoopingMusicData> GetCurrentMusic(Frame f) {
    //     return MainMusic[f.Global->TotalGamesPlayed % MainMusic.Length];
    // }

    public FPVector2 GetWorldSpawnpointForPlayer(int playerIndex, int totalPlayers) {
        FP comp = ((FP) playerIndex / totalPlayers) * 2 * FP.Pi + FP.PiOver2 + (FP.Pi / (2 * totalPlayers));
        FP scale = (FP._2 - ((FP) totalPlayers + 1) / totalPlayers) * SpawnpointArea.X;

        FPVector2 offset = new(
            FPMath.Sin(comp) * scale,
            FPMath.Cos(comp) * (totalPlayers > 2 ? scale * SpawnpointArea.Y: 0)
        );

        FPVector2 result = Spawnpoint + offset;
        result.Y -= FP._0_50;
        return result;
    }
    
    public StageTileInstance GetTileRelative(Frame f, IntVector2 tile) {
        if (tile.X < 0 || tile.Y < 0 || tile.X >= TileDimensions.X || tile.Y >= TileDimensions.Y) {
            return default;
        }

        return f.StageTiles[tile.X + tile.Y * TileDimensions.X];
    }

    public StageTileInstance GetTileWorld(Frame f, FPVector2 worldPosition) {
        return GetTileRelative(f, QuantumUtils.WorldToRelativeTile(this, worldPosition));
    }

    public void SetTileRelative(Frame f, IntVector2 tilePosition, StageTileInstance tile) {
        int index = tilePosition.X + tilePosition.Y * TileDimensions.X;
        if (index < 0 || index >= f.StageTilesLength) {
            return;
        }

        f.StageTiles[index] = tile;
        f.Signals.OnTileChanged(tilePosition, tile);
        f.Events.TileChanged(tilePosition + TileOrigin, tile);
    }

    public void ResetStage(Frame f, bool full) {
        using var scope = HostProfiler.Start("VersusStageData.ResetStage");
        StageTileInstance* stageTiles = f.StageTiles;

        for (int i = 0; i < TileData.Length; i++) {
            ref StageTileInstance newTile = ref TileData[i];
            if (!stageTiles[i].Equals(newTile)) {
                using var callbackScope = HostProfiler.Start("VersusStageData.ExecuteCallbacks");
                int x = i % TileDimensions.X;
                int y = i / TileDimensions.X;
                IntVector2 tile = new(x, y);
                f.Signals.OnTileChanged(tile, newTile);
                f.Events.TileChanged(tile + TileOrigin, newTile);
            }
            stageTiles[i] = newTile;
        }
        f.Signals.OnStageReset(full);
    }
}