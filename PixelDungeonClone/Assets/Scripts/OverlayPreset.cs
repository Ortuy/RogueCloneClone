using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "OverlayPreset", menuName = "Overlay Preset", order = 4)]
public class OverlayPreset : ScriptableObject
{
    public TileData[] tiles;

    [System.Serializable]
    public struct TileData
    {
        //public Tile result;
        public int result;

        public bool[] neededTiles;
    }
}
