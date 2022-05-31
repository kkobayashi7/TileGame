using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class BattleManager : MonoBehaviour
{
    public GameObject canvas;

    private CardManager cardManager;
    public List<Card> Hand = new List<Card>();
    public List<GameObject[]> HandTile = new List<GameObject[]>(); // 生成したタイルのインスタンスへの参照用
    public List<GameObject> Intensions = new List<GameObject>(); 

    public List<Card> DiscardPile = new List<Card>();
    public List<Card> ExilePile = new List<Card>();
    public List<Card> DeckCopy = new List<Card>();

    public int Energy; // プレイヤーの戦闘中のエナジー
    public int Block = 0; // プレイヤーのブロック値

    public int Turn = 1;

    public int EnergyTiles = 0; // 起動しているエナジータイル
    public int DrawTiles = 0; // 起動しているドロータイル
    public int BlockTiles = 0; // 起動しているブロックタイル

    // 戦闘中の敵 (最大5体)
    public Enemy[] enemy = new Enemy[5];
    public EnemyData[] EnemiesInBattle = new EnemyData[5];
    public int enemynum = 3;

    // Start is called before the first frame update
    void Start()
    {

        // CardManagerコンポーネントを持っているオブジェクトを検索し, そのオブジェクトが持つCardManagerコンポーネントへの参照を返す.
        cardManager = FindObjectOfType<CardManager>();
        if(cardManager == null){
            Debug.Log("CardManagerコンポーネントを持つオブジェクトが存在しません.");
        }

        // 初期デッキを用意(テスト用)
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/Attack2")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/Attack2")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/Attack2")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/UPentomino")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/LPentomino")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/AddPentomino")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/EnergyAttack")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/DrawAttack")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/BlockAttack")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/Draw3")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/Draw3")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/ShuffleDraw1")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/Block5")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/Block5")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/Block5")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/Block10")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/Block10")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/Block10")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/Block10")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/Block3Attack1")));
        PlayerStatus.Deck.Add(cardManager.MakeCard(Resources.Load<CardData>("Cards/PutSquares")));

        // エナジーを初期化
        Energy = PlayerStatus.Max_Energy;

        // デッキをシャッフル (いったん全て捨て札にしてから, DeckCopyにランダムに戻す)
        DiscardPile = new List<Card>(PlayerStatus.Deck); 
        ShuffleDeck();

        InitializeEnemies();

        DrawCard(5);

        // 戦闘シーンを描画する 
        ShowBattleScene();
    }

    public void InitializeEnemies(){
        enemynum = 3;

        EnemiesInBattle[0] = Resources.Load<EnemyData>("EnemiesData/Enemy1");
        EnemiesInBattle[1] = Resources.Load<EnemyData>("EnemiesData/Enemy2");
        EnemiesInBattle[2] = Resources.Load<EnemyData>("EnemiesData/Enemy2");

        // Enemyを生成
        float posx = -7.0f;
        for(int i = 0; i < enemynum; i++){

            enemy[i] = new Enemy(EnemiesInBattle[i]);
            if(i > 0) posx += (enemy[i-1].enemylength_x + 1);
            enemy[i].enemypos_x = posx;
            enemy[i].enemypos_y = 8.0f;
            enemy[i].enemyParent = Instantiate((GameObject)Resources.Load("EnemyParent"), new Vector3(enemy[i].enemypos_x, enemy[i].enemypos_y, 0.0f), Quaternion.identity);

            // 敵の行動予定を表示
            GameObject obj = null;
            obj = (GameObject)Resources.Load("Intension");
            Intensions.Add(Instantiate(obj, enemy[i].enemyParent.transform.position + new Vector3((enemy[i].enemylength_x/2), -8.5f, 0.0f), Quaternion.identity, canvas.transform));
        }
    }

    // 戦闘シーンを描画する
    public void ShowBattleScene(){
        ShowEnemy();
        ShowHand();
        ShowIntension();
        // プレイヤーのHP, Energy, Blockは
        // ShowStatusで管理している(統合予定)
    }

    public void ShowIntension(){
        for(int i = 0; i < enemynum; i++){
            string IntensionText = "";
            if(enemy[i].EnemyIntension == 0) {
                IntensionText = "何もしない";
            }else if(enemy[i].EnemyIntension == 1 || enemy[i].EnemyIntension == 2){
                IntensionText = "攻撃: " + enemy[i].EnemyAttack.ToString();
            }else if(enemy[i].EnemyIntension == 3){
                IntensionText = "敵??にアーマーを付与 ";
            }
            Intensions[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = IntensionText;
        }
    }

    public void ShowHand(){
        // 手札を描画
        for(int i = 0; i < HandTile.Count; i++){
            if(HandTile[i] != null)
                Destroy(HandTile[i][0]);
                Destroy(HandTile[i][1]);
        }
        HandTile = new List<GameObject[]>();
        float xmargin = 0.0f;
        for(int i = 0;i < Hand.Count; i++){
            if(Hand[i].CardKind.isTile == true){
                HandTile.Add(cardManager.GenerateTile(Hand[i], -14.0f + xmargin, -5.0f)); 
                xmargin += (float)(Hand[i].Tile[0].row.Length + 2.5);
            }else{
                HandTile.Add(cardManager.GenerateCard(Hand[i], -14.0f + xmargin, -5.0f)); 
                xmargin += 4.2f;
            }
        }
    }

    public void ShowEnemy(){
        for(int k = 0; k < enemynum; k++){
            for(int i = 0; i < enemy[k].EnemyGrid.Length; i++){
                for(int j = 0; j < enemy[k].EnemyGrid[i].row.Length; j++){
                    GameObject obj = null;
                    if(enemy[k].enemysquares[i, j] != null){
                        Destroy(enemy[k].enemysquares[i, j]);
                    }

                    if(enemy[k].EnemyGrid[i].row[j] == TileKind.blank) {
                        obj = (GameObject)Resources.Load("BlankSquare");
                    }
                    else if(enemy[k].EnemyGrid[i].row[j] == TileKind.body){
                        obj = (GameObject)Resources.Load("BodySquare");
                    }
                    else if(enemy[k].EnemyGrid[i].row[j] == TileKind.body2){
                        
                    }
                    else if(enemy[k].EnemyGrid[i].row[j] == TileKind.damage1){
                        
                    }
                    else if(enemy[k].EnemyGrid[i].row[j] == TileKind.hit){
                        obj = (GameObject)Resources.Load("NormalSquare");
                    }
                    else if(enemy[k].EnemyGrid[i].row[j] == TileKind.energy){
                        obj = (GameObject)Resources.Load("EnergySquare");
                    }
                    else if(enemy[k].EnemyGrid[i].row[j] == TileKind.draw){
                        obj = (GameObject)Resources.Load("DrawSquare");
                    }
                    else if(enemy[k].EnemyGrid[i].row[j] == TileKind.block){
                        obj = (GameObject)Resources.Load("BlockSquare");
                    }
                    else if(enemy[k].EnemyGrid[i].row[j] == TileKind.weak){
                        obj = (GameObject)Resources.Load("WeakSquare");
                    }
                    else if(enemy[k].EnemyGrid[i].row[j] == TileKind.armor){
                        obj = (GameObject)Resources.Load("ArmorSquare");
                    }
                    else{
                        Debug.Log("無効なEnemy Tileです");
                    }
                    if(obj != null){
                        if(obj.GetComponent<PopUpText>() != null)
                            obj.GetComponent<PopUpText>().enabled = true;
                        obj.GetComponent<SpriteRenderer>().sortingOrder = -2;
                        enemy[k].enemysquares[i, j] = Instantiate(obj, enemy[k].enemyParent.transform.position + new Vector3(j,-i,0), Quaternion.identity, enemy[k].enemyParent.transform);
                    }
                }
            }
        }
    }

    // 捨て札をデッキにランダムな順番で加える
    public void ShuffleDeck(){
        int cardnum = DiscardPile.Count;
        for(int i = 0; i < cardnum; i++){
            int index = Random.Range(0, cardnum - i);
            DeckCopy.Add(DiscardPile[index]);
            DiscardPile.RemoveAt(index);
        }
    }

    // 手札にcardと同じカードが含まれていたら, それを捨て札に送る
    public void Discard(Card card){
        if(Hand.Contains(card)){
            DiscardPile.Add(card);
            Hand.Remove(card);
        }else{
            Debug.Log("カード名:" +card.CardKind.CardName + "は手札に存在しません.");
        }
    }

    // 山札を全て捨て札にする.
    public void DiscardDeck(){
        if(DeckCopy.Count > 0){
            for(int i = 0; i < DeckCopy.Count; i++){
                DiscardPile.Add(DeckCopy[0]);
                DeckCopy.RemoveAt(0);
            }
        }else{
            Debug.Log("山札は空です");
        }
    }

    // 手札から指定した種類のを廃棄する.
    public void ExileCard(Card card){
        if(Hand.Contains(card)){
            ExilePile.Add(card);
            Hand.Remove(card);
        }else{
            Debug.Log("カード名:" +card.CardKind.CardName + "は手札に存在しません.");
        }
    }


    // ターンを終了する(手札のカードを全て捨て札にし, 新たに手札を引く)
    public void EndTurn(){
        // カードをすべて捨て札に送り, 新たにカードを引く
        for(int i = 0; i < HandTile.Count; i++){
            DiscardPile.Add(Hand[i]);
        }
        Hand = new List<Card>();

        /* 敵の行動, 敵の次の行動の決定 */
        for(int i = 0; i < enemynum; i++){
            int actionnum = enemy[i].EnemyKind.EnemyAction.Count;
            if(enemy[i].EnemyIntension == 1 || enemy[i].EnemyIntension == 2){
                PlayerStatus.HP -= Mathf.Max(0, enemy[i].EnemyAttack - Block);
                Block = Mathf.Max(0, Block - enemy[i].EnemyAttack);
            }else if(enemy[i].EnemyIntension == 3){
                
            }else{

            }
            if(enemy[i].EnemyState == 0){
                enemy[i].EnemyIntension = 0;
            }else{
                enemy[i].EnemyIntension = enemy[i].EnemyKind.EnemyAction[Turn % actionnum];
            }
            if(enemy[i].EnemyIntension == 1){
                enemy[i].EnemyAttack = Random.Range(5, 11);
            }else if(enemy[i].EnemyIntension == 2){
                enemy[i].EnemyAttack = Random.Range(3, 5);
            }else{
                enemy[i].EnemyAttack = 0;
            }
        }

        // 次ターンの開始時のプレイヤーの状態
        DrawCard(5 + DrawTiles);
        Energy = PlayerStatus.Max_Energy + EnergyTiles;
        Block = 3*BlockTiles;
        Turn++;
        ShowBattleScene();
    }

    // 指定したカードを手札に加える
    public void AddCardToHand(Card card){
        Hand.Add(card);
    }

    public void AddCardToDiscardPile(Card card){
        DiscardPile.Add(card);
    }

    // デッキからカードをnum枚引く. デッキが空でカードが引けない場合は, 捨て札をシャッフルしてデッキに戻してから一枚引く. 捨て札もない場合は何もしない.
    public void DrawCard(int num){
        for(int i = 0; i < num; i++){
            // 手札上限 8枚
            if(Hand.Count >= 8){
                break;
            }
            if(DeckCopy.Count > 0){
                AddCardToHand(DeckCopy[0]);
                DeckCopy.RemoveAt(0);
            }else if(DeckCopy.Count == 0 && DiscardPile.Count > 0){
                ShuffleDeck();
                AddCardToHand(DeckCopy[0]);
                DeckCopy.RemoveAt(0);
            }else{
                Debug.Log("山札も捨て札も空です");
            }
        }
        ShowHand();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
