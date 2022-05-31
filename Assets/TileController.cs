using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileController : MonoBehaviour,IDragHandler,IBeginDragHandler,IEndDragHandler
{

    // カードのデータ
    public Card card;
    private Vector3 prevPos;
    public Vector3 rotationPoint;
    private BattleManager battleManager;
    private bool isDragging = false; 

    // Start is called before the first frame update
    void Start()
    {
        battleManager = FindObjectOfType<BattleManager>();
        if(battleManager == null){
            Debug.Log("BatttleManagerコンポーネントを持つオブジェクトが存在しません.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isDragging == true){
            if (Input.GetKeyDown(KeyCode.Q))
            {
                // 右回転
                transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                // 左回転
                transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                // 左回転
                Vector3 scale = transform.localScale;
                transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // ドラッグ前の位置を記憶しておく
        prevPos = transform.position;
        isDragging = true;
        
    }
    
 
    public void OnDrag(PointerEventData eventData)
    {
        // eventData.positionにはスクリーン上のマウスポインタの座標が入っている. これをワールド座標(シーンの中の座標)に変換する.
        Vector3 TargetPos = Camera.main.ScreenToWorldPoint(eventData.position);
		TargetPos.z = 0.0f;
		transform.position = TargetPos;

        // グリッドに沿って動かす
        int roundX = Mathf.RoundToInt(transform.position.x);
        int roundY = Mathf.RoundToInt(transform.position.y);
        transform.position = new Vector3(roundX, roundY, 0.0f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        Enemy[] enemy = battleManager.enemy; // 戦闘中の敵への参照
        int enemynum = battleManager.enemynum;
        int target = 0; // 攻撃対象
        bool IsTileInEnemyGrid = true;

        isDragging = false; 

        // タイルが敵のグリッドに入っているか判定
        for(int i = 0; i < enemynum; i++){
            int length_x = enemy[i].enemylength_x;
            int length_y = enemy[i].enemylength_y;
            int pos_x = (int)enemy[i].enemypos_x;
            int pos_y = (int)enemy[i].enemypos_y;
            IsTileInEnemyGrid = true;
            foreach(Transform child in transform){
                int x = Mathf.RoundToInt(child.position.x);
                int y = Mathf.RoundToInt(child.position.y);
                if((x < pos_x) 
                || (x >= pos_x + length_x) 
                || (y <= pos_y - length_y) 
                || (y > pos_y))
                {
                    IsTileInEnemyGrid = false;
                }
            }
            if(IsTileInEnemyGrid == true) {
                target = i;
                break;
            }
        }

        if(IsTileInEnemyGrid == false){
            transform.position = prevPos;
            return;
        }


        if(battleManager.Energy < card.cost){
            Debug.Log("コストが足りません");
            transform.position = prevPos;
            return;
        }

        if(card.CardKind.isTile == false){
            return;
        }

        // 使用可能なとき(タイルが敵のグリッドに入っており, かつエナジーが足りている)
        battleManager.Energy -= card.cost; 
        foreach(Transform child in transform){
            // タイルの各セルの位置を配列の添え字に変換 (左上が(0, 0))

            int i = Mathf.RoundToInt((child.position.x - enemy[target].enemypos_x));
            int j = Mathf.RoundToInt((- child.position.y  + enemy[target].enemypos_y));

            // ダメージゾーンか, 空白か, すでに攻撃している場所に攻撃したらプレイヤーにダメージ
            if(enemy[target].EnemyGrid[j].row[i] == TileKind.damage1 
            || enemy[target].EnemyGrid[j].row[i] == TileKind.blank 
            || enemy[target].EnemyGrid[j].row[i] == TileKind.hit
            || enemy[target].EnemyGrid[j].row[i] == TileKind.energy
            || enemy[target].EnemyGrid[j].row[i] == TileKind.draw
            || enemy[target].EnemyGrid[j].row[i] == TileKind.block)
            {
                if(battleManager.Block > 0){
                    battleManager.Block -= 1;
                }else{
                    PlayerStatus.HP -= 1;
                }
                Debug.Log("あなたは1ダメージ受けた(" + PlayerStatus.HP + "/" + PlayerStatus.Max_HP + ")");
            }
            else if(enemy[target].EnemyGrid[j].row[i] == TileKind.weak){ // weakマスに攻撃を当てたら, 敵の行動をキャンセル
                enemy[target].EnemyIntension = 0;
            }

            // マスの名前に応じて敵のグリッドを埋める(そのうち書き換えたい)
            if(enemy[target].EnemyGrid[j].row[i] == TileKind.armor){
                    enemy[target].EnemyGrid[j].row[i] = TileKind.body; 
            }
            else{
                if(child.name == "EnergySquare(Clone)"){
                    enemy[target].EnemyGrid[j].row[i] = TileKind.energy;
                }else if(child.name == "DrawSquare(Clone)"){
                    enemy[target].EnemyGrid[j].row[i] = TileKind.draw;
                }else if(child.name == "BlockSquare(Clone)"){
                    enemy[target].EnemyGrid[j].row[i] = TileKind.block;
                }else{
                    enemy[target].EnemyGrid[j].row[i] = TileKind.hit;
                }
            }
        }
        
        // 周囲が埋まっているエナジー, ドロー, ブロックタイルの数をチェックして更新する
        int energytiles = 0;
        int drawtiles = 0;
        int blocktiles = 0;

        for(int i = 0; i < enemynum; i++){
            energytiles += NumTilesSurrounded(TileKind.energy, enemy[i].EnemyGrid);
            drawtiles += NumTilesSurrounded(TileKind.draw, enemy[i].EnemyGrid);
            blocktiles += NumTilesSurrounded(TileKind.block, enemy[i].EnemyGrid);
        }

        if(energytiles > battleManager.EnergyTiles){
            battleManager.Energy += energytiles - battleManager.EnergyTiles;
        }
        if(drawtiles > battleManager.DrawTiles){
            battleManager.DrawCard(drawtiles - battleManager.DrawTiles);
        }
        if(blocktiles > battleManager.BlockTiles){
            battleManager.Block += (blocktiles - battleManager.BlockTiles) * 3;
        }

        battleManager.EnergyTiles = energytiles;
        battleManager.DrawTiles = drawtiles;
        battleManager.BlockTiles = blocktiles;

        // PopUpTextが出ていたらそれを消す
        if(this.gameObject.GetComponent<PopUpText>().popped == true){
            Destroy(this.gameObject.GetComponent<PopUpText>().obj);
        } 

        // このタイルのカードデータを持つカードを手札から捨て札か廃棄へ送る
        if(card.CardKind.isExile == true){
            battleManager.ExileCard(card);
        }else{
            battleManager.Discard(card);
        }

        // 戦闘画面におけるEnemyの見た目を更新
        battleManager.ShowBattleScene();

        /* タイルの効果処理部(終) */

        // 敵を倒したか判定 
        if (isEnemyKilled(enemy[target].EnemyGrid)){
            enemy[target].EnemyIntension = 0;
            enemy[target].EnemyState = 0;
            Debug.Log("たおした");
            battleManager.ShowBattleScene();
        }
        
    }

    // 指定された種類のタイルのうち, 周り8マスを囲まれているものの個数を返す
    public int NumTilesSurrounded(TileKind tile, EnemyTileDataRow[] grid){
        int num = 0;
        for (int i = 0; i < grid[0].row.Length; i++){
            for (int j = 0; j < grid.Length; j++){
                bool surrounded = true; 
                if(grid[j].row[i] == tile){
                    for(int k = -1; k < 2; k++){
                        for(int l = -1; l < 2; l++){
                            if(j+k < 0 || j+k >=  grid.Length || i+l < 0 || i+l >= grid[0].row.Length ) break;
                            if( !( grid[j+k].row[i+l] == TileKind.hit 
                                || grid[j+k].row[i+l] == TileKind.energy 
                                || grid[j+k].row[i+l] == TileKind.draw 
                                || grid[j+k].row[i+l] == TileKind.block))
                            {
                                surrounded = false;
                            }
                        }
                    }
                    if(surrounded == true) num++;
                }
            }   
        }
        return num;
    }

    public bool isEnemyKilled(EnemyTileDataRow[] grid){
        bool iskilled = true;
        for (int i = 0; i < grid[0].row.Length; i++){
            for (int j = 0; j < grid.Length; j++){
                // もし, EnemyGrid上ではhitでないようなblank以外のセルがあれば, まだ倒していない.
                if (   grid[j].row[i] != TileKind.blank 
                    && grid[j].row[i] != TileKind.hit 
                    && grid[j].row[i] != TileKind.energy 
                    && grid[j].row[i] != TileKind.draw 
                    && grid[j].row[i] != TileKind.block )
                {
                    iskilled = false;
                }
            }   
        }
        return iskilled;
    }
}
