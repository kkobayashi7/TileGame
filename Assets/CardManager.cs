using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using TMPro;

public class CardManager :MonoBehaviour
{

    public GameObject canvas;

    // cardDataのカードのインスタンスを返す
    public Card MakeCard(CardData cardData){
        Card card = new Card();
        card.CardKind = cardData;
        card.cost = cardData.cost;
        // タイル情報を初期化
        if(cardData.isTile == true){
            card.Tile = new TileDataRow[cardData.TileData.Length];
            for(int i = 0; i < cardData.TileData.Length; i++){
                card.Tile[i] = new TileDataRow();
                card.Tile[i].row = new int[cardData.TileData[0].row.Length];
                for(int j = 0; j < cardData.TileData[0].row.Length; j++){
                    card.Tile[i].row[j] = cardData.TileData[i].row[j];
                }
            }
        }
        return card;
    }

    // (pos_x, pos_y)にCard cardのタイルを生成
    public GameObject[] GenerateTile(Card card,  float pos_x = 0.0f, float pos_y = 0.0f){

        // Tileの親オブジェクトを生成
        GameObject[] TileInstance  = new GameObject[2];
        TileInstance[0] = Instantiate((GameObject)Resources.Load("TileParent"), new Vector3 (pos_x, pos_y, 0.0f), Quaternion.identity);
        // 戦闘シーンにおけるカードへの参照を代入
        TileInstance[0].GetComponent<TileController>().card = card;

        // コストを表示
        TileInstance[1] = Instantiate((GameObject)Resources.Load("CostLabel"), new Vector3 (pos_x, pos_y+1.5f, 0.0f), Quaternion.identity, canvas.transform);
        TileInstance[1].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "コスト" + card.cost.ToString();

        // cardData.TileDataの要素に応じて対応するSquareを置く
        for(int i = 0; i < card.Tile.Length; i++){
            for(int j = 0; j < card.Tile[i].row.Length; j++){
                GameObject obj = null;
                if(card.Tile[i].row[j] == 1) {
                    obj = (GameObject)Resources.Load("NormalSquare");
                }else if(card.Tile[i].row[j] == 2) {
                    obj = (GameObject)Resources.Load("EnergySquare");
                }else if(card.Tile[i].row[j] == 3) {
                    obj = (GameObject)Resources.Load("DrawSquare");
                }else if(card.Tile[i].row[j] == 4) {
                    obj = (GameObject)Resources.Load("BlockSquare");
                }else if(card.Tile[i].row[j] == 5) {
                    obj = (GameObject)Resources.Load("HealSquare");
                }
                if(obj != null){
                    obj.GetComponent<PopUpText>().enabled = false;
                    obj.GetComponent<SpriteRenderer>().sortingOrder = 1;
                    Instantiate(obj, TileInstance[0].transform.position + new Vector3(j,-i,0), Quaternion.identity, TileInstance[0].transform);
                }
            }
        }

        // tileParentにマウスオーバー時の説明文を載せる
        if(card.CardKind.CardName == "EnergyAttack"){
            TileInstance[0].GetComponent<PopUpText>().PopUp = Resources.Load<GameObject>("PopUpTexts/EnergyZoneHelp");
        }else if(card.CardKind.CardName == "DrawAttack"){
            TileInstance[0].GetComponent<PopUpText>().PopUp = Resources.Load<GameObject>("PopUpTexts/DrawZoneHelp");
        }else if(card.CardKind.CardName == "BlockAttack"){
            TileInstance[0].GetComponent<PopUpText>().PopUp = Resources.Load<GameObject>("PopUpTexts/BlockZoneHelp");
        }else if(card.CardKind.CardName == "HealAttack"){
            TileInstance[0].GetComponent<PopUpText>().PopUp = Resources.Load<GameObject>("PopUpTexts/HealZoneHelp");
        }else if(card.cost == 0){
            TileInstance[0].GetComponent<PopUpText>().PopUp = Resources.Load<GameObject>("PopUpTexts/Cost0");
        }else if(card.cost == 1){
            TileInstance[0].GetComponent<PopUpText>().PopUp = Resources.Load<GameObject>("PopUpTexts/Cost1");
        }else if(card.cost == 2){
            TileInstance[0].GetComponent<PopUpText>().PopUp = Resources.Load<GameObject>("PopUpTexts/Cost2");
        }else{
            
        }
        
        return TileInstance;
    }

    // (pos_x, pos_y)にcardDataのカードを生成
    public GameObject[] GenerateCard(Card card, float pos_x = 0.0f, float pos_y = 0.0f){

        // Cardの親オブジェクトを生成
        GameObject[] cardParent = new GameObject[2];
        cardParent[0] = Instantiate((GameObject)Resources.Load("CardParent"), new Vector3 (pos_x, pos_y, 0.0f), Quaternion.identity, canvas.transform);

        // cardParentにアタッチされているTileControllerの変数に必要な情報を代入
        cardParent[0].GetComponent<CardController>().card = card;

        // Resourcesフォルダ内の"Square"プレハブをロード
        GameObject Square = (GameObject)Resources.Load ("Card");
        GameObject CardSprite = Instantiate(Square, cardParent[0].transform.position, Quaternion.identity, cardParent[0].transform); // cardParentの下にカードのスプライトを生成
        
        // カードの下部にテキストを設定.
        CardSprite.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "コスト" + card.CardKind.cost.ToString();
        CardSprite.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = card.CardKind.Text;

        return cardParent;
    }

}
