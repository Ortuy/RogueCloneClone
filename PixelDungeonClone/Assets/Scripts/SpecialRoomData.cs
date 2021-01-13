using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpecialRoomData", menuName = "Special Room Data", order = 3)]
public class SpecialRoomData : ScriptableObject
{
    public GameObject[] gates;
    public Item[] keys;
    public GameObject[] roomContents;

    public int minGold, maxGold;
}
