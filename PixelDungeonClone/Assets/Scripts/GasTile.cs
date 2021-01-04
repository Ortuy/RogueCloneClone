using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasTile : MonoBehaviour
{
    public Gas parentGas;
    public ParticleSystem gasFX;

    private void Start()
    {
        parentGas = GetComponentInParent<Gas>();
        gasFX = GetComponentInChildren<ParticleSystem>();
    }

    public void Spread()
    {
        var up = new Vector3(transform.position.x, transform.position.y + 1);
        var right = new Vector3(transform.position.x + 1, transform.position.y);
        var down = new Vector3(transform.position.x, transform.position.y - 1);
        var left = new Vector3(transform.position.x - 1, transform.position.y);

        if (Pathfinding.instance.GetGrid().GetGridObject(up) != null && Pathfinding.instance.GetGrid().GetGridObject(up).walkable && !Physics2D.OverlapCircle(up, 0.2f, LayerMask.GetMask("Gases")))
        {
            parentGas.gasTiles.Add(Instantiate(parentGas.gasTileBase, up, Quaternion.identity, parentGas.transform));
        }
        if (Pathfinding.instance.GetGrid().GetGridObject(right) != null && Pathfinding.instance.GetGrid().GetGridObject(right).walkable && !Physics2D.OverlapCircle(right, 0.2f, LayerMask.GetMask("Gases")))
        {
            parentGas.gasTiles.Add(Instantiate(parentGas.gasTileBase, right, Quaternion.identity, parentGas.transform));
        }
        if (Pathfinding.instance.GetGrid().GetGridObject(down) != null && Pathfinding.instance.GetGrid().GetGridObject(down).walkable && !Physics2D.OverlapCircle(down, 0.2f, LayerMask.GetMask("Gases")))
        {
            parentGas.gasTiles.Add(Instantiate(parentGas.gasTileBase, down, Quaternion.identity, parentGas.transform));
        }
        if (Pathfinding.instance.GetGrid().GetGridObject(left) != null && Pathfinding.instance.GetGrid().GetGridObject(left).walkable && !Physics2D.OverlapCircle(left, 0.2f, LayerMask.GetMask("Gases")))
        {
            parentGas.gasTiles.Add(Instantiate(parentGas.gasTileBase, left, Quaternion.identity, parentGas.transform));
        }
    }
}
