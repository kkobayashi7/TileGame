using UnityEngine;

// CreateAssetMenu属性を使用することで`Assets > Create > ScriptableObjects > CreateCardData`という項目が表示される
// それを押すとこの`CreateCardData`が`Data`という名前でアセット化されてassetsフォルダに入る


[System.Serializable]
public class TileDataRow
{
  public int[] row = new int[2] {0, 0};
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CreateCardData")]
public class CardData : ScriptableObject
{
  // カード名
  public string CardName;
  public string Text;
  public int cost = 1;
  public bool isTile = true;
  public bool isExile = false;

  // タイルのデータ
  public TileDataRow[] TileData;
}

public class Card 
{
  // カード名
  public CardData CardKind;
  // タイルのデータ
  public TileDataRow[] Tile;
  // コスト
  public int cost;
}