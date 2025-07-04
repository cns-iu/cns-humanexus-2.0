using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public string graphic;
    public string ftu;
    public string organ;
    public Item(Item d)
    {
        graphic = d.graphic;
        ftu = d.ftu;
        organ = d.organ;
    }
}