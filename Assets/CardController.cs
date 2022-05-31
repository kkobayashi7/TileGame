using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/* TileControllerと似たようなコードなので統合すべきな気がする */

public class CardController : MonoBehaviour,IDragHandler,IBeginDragHandler,IEndDragHandler
{
    public Card card;
    private Vector3 prevPos;
    private BattleManager battleManager;
    private CardManager cardManager;


    void Start()
    {
        battleManager = FindObjectOfType<BattleManager>();
        if(battleManager == null){
            Debug.Log("BatttleManagerコンポーネントを持つオブジェクトが存在しません.");
        }

        cardManager = FindObjectOfType<CardManager>();
        if(cardManager == null){
            Debug.Log("CardManagerコンポーネントを持つオブジェクトが存在しません.");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // ドラッグ前の位置を記憶しておく
        prevPos = transform.position;
    }
    
 
    public void OnDrag(PointerEventData eventData)
    {
        // eventData.positionにはスクリーン上のマウスポインタの座標が入っている. これをワールド座標(シーンの中の座標)に変換する.
        Vector3 TargetPos = Camera.main.ScreenToWorldPoint(eventData.position);
		TargetPos.z = 0;
		transform.position = TargetPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        int enemynum = battleManager.enemynum;
        Enemy[] enemy = battleManager.enemy; // 戦闘中の敵への参照

        // エナジーが足りているかどうか判定
        if(battleManager.Energy < card.CardKind.cost){
            Debug.Log("コストが足りません");
            transform.position = prevPos;
            return;
        }

        /* カードがプレイゾーンに入っているか判定 */
        if( Camera.main.ScreenToWorldPoint (eventData.position).y < 3.0f ){
            transform.position = prevPos;
            return;
        }

        // このタイルのカードデータを持つカードを手札から捨て札か廃棄へ送る.
        if(card.CardKind.isExile == true){
            battleManager.ExileCard(card);
        }else{
            battleManager.Discard(card);
        }

        battleManager.Energy -= card.CardKind.cost; // エナジーを消費

        /* カード効果処理部 */
        if(card.CardKind.CardName == "Draw3"){
            battleManager.DrawCard(3);
        }
        if(card.CardKind.CardName == "ShuffleDraw1"){
            battleManager.DiscardDeck();
            battleManager.ShuffleDeck();
            battleManager.DrawCard(1);
        }
        else if(card.CardKind.CardName == "Block5"){
            battleManager.Block += 5;
        }
        else if(card.CardKind.CardName == "Block10"){
            battleManager.Block += 10;
        }
        else if(card.CardKind.CardName == "Block3Attack1"){
            battleManager.Block += 3;
            battleManager.AddCardToHand(cardManager.MakeCard(Resources.Load<CardData>("Cards/Attack1")));
        }
        else if(card.CardKind.CardName == "AddPentomino"){
            //ペントミノの種類F I L N P T U V W X Y Z
            for(int i = 0; i < 2; i++){
                int pentomino = Random.Range(0, 12);
                string pentominokind = "FILNPTUVWXYZ";
                string cardname = "Cards/" + pentominokind[pentomino] + "Pentomino";
                Card card = cardManager.MakeCard(Resources.Load<CardData>(cardname));
                card.cost = 1;
                battleManager.AddCardToDiscardPile(card);
            }
        }
        else if(card.CardKind.CardName == "PutSquares"){
            /* 敵を選択する(予定, 現状はenemy[0]に攻撃) */
            /* 敵を選択する(予定) */
            List<List<int>> positions = TilePositions(TileKind.body, enemy[0].EnemyGrid);
            if(positions.Count < 3) {
                Debug.Log("Bodyタイルの数が3つ以上でなければ使えません");
            }else{
                int energypos = Random.Range(0, positions.Count);
                enemy[0].EnemyGrid[positions[energypos][0]].row[positions[energypos][1]] = TileKind.energy;
                positions.RemoveAt(energypos);
                int drawpos = Random.Range(0, positions.Count);
                enemy[0].EnemyGrid[positions[drawpos][0]].row[positions[drawpos][1]] = TileKind.draw;
                positions.RemoveAt(drawpos);
                int blockpos = Random.Range(0, positions.Count);
                enemy[0].EnemyGrid[positions[blockpos][0]].row[positions[blockpos][1]] = TileKind.block;
                positions.RemoveAt(blockpos);
            }
        }
        /* カード効果処理部(終) */

        // 戦闘画面を更新
        battleManager.ShowBattleScene();

    }

    // EnemyGrid上に存在する指定された種類のtileの座標のリストを返す
    public List<List<int>> TilePositions(TileKind tile, EnemyTileDataRow[] grid){
        List<List<int>> pos = new List<List<int>>();
        for (int i = 0; i < grid[0].row.Length; i++){
            for (int j = 0; j < grid.Length; j++){
                if(grid[j].row[i] == tile) pos.Add(new List<int>(){j, i});
            }   
        }
        return pos;
    }
}
