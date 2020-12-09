using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DropTable", menuName = "Drop Table", order = 2)]
public class ItemDropTable : ScriptableObject
{
    public Item[] itemPool;
    public int[] itemWeightPool;
}
