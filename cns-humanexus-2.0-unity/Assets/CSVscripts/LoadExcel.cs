using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadExcel : MonoBehaviour
{
    public Item blankItem;
    public List<Item> itemDatabase = new List<Item>();

    public void LoadItemData()
    {
        //Clear database
        itemDatabase.Clear();

        //READ CSV files
        List<Dictionary<string, object>> data = CSVReader.Read("test_subset_15");
        for (var i = 0; i < data.Count; i++)
        {
            string graphic = data[i]["graphic"].ToString();
            string ftu = data[i]["ftu"].ToString();
            string organ = data[i]["organ"].ToString();
            AddItem(graphic, ftu, organ);
        }
    }

    void AddItem(string graphic, string ftu, string organ)
    {
        Item tempItem = new Item(blankItem);
        tempItem.graphic = graphic;
        tempItem.ftu = ftu;
        tempItem.organ = organ;
        itemDatabase.Add(tempItem);
    }
}