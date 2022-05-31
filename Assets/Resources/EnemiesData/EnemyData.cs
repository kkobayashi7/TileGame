using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyTileDataRow {
   public TileKind[] row;
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CreateEnemyData")]
public class EnemyData : ScriptableObject
{
  public string EnemyName;
  public List<int> EnemyAction = new List<int>(); // 0: なにもしない, 1: 攻撃, 2: armor
  public EnemyTileDataRow[] EnemyTileData;
}

// 戦闘中の敵の状態を扱うためのクラス
public class Enemy
{
  // 敵の種類
  public EnemyData EnemyKind;
  // 戦闘中の敵のgridの情報(プレイヤーの攻撃が当たったところにはhitを代入)
  public EnemyTileDataRow[] EnemyGrid;

  // 画面上での敵の位置
  public float enemypos_x;
  public float enemypos_y;

  // 敵のグリッドの長さ
  public int enemylength_x;
  public int enemylength_y;

  // 戦闘シーンにおけるEnemyの親オブジェクト
  public GameObject enemyParent;
  public int EnemyIntension; // 0: なにもしない, 1: 中攻撃, 2: 小攻撃,  3: armor
  public int EnemyAttack; // 敵の次の攻撃値
  public int EnemyState ; // 敵の状態. 0: 死亡, 1: 生存

  public GameObject[, ] enemysquares;

  public Enemy(EnemyData enemy){
    this.EnemyKind = enemy;
    this.EnemyState = 1;

    this.enemylength_x = enemy.EnemyTileData[0].row.Length;
    this.enemylength_y = enemy.EnemyTileData.Length;

     // 敵の戦闘用グリッドを初期化
    this.EnemyGrid = new EnemyTileDataRow[this.enemylength_y];
    this.enemysquares = new GameObject[this.enemylength_y, this.enemylength_x];
    for(int i = 0; i < this.enemylength_y; i++){
        this.EnemyGrid[i] = new EnemyTileDataRow();
        this.EnemyGrid[i].row = new TileKind[this.enemylength_x];
        for(int j = 0; j < this.enemylength_x; j++){
            this.EnemyGrid[i].row[j] = this.EnemyKind.EnemyTileData[i].row[j];
        }
    }

    // 敵の行動を初期化
    this.EnemyIntension = this.EnemyKind.EnemyAction[0]; // 0: なにもしない, 1: 攻撃
    if(this.EnemyIntension == 1) {
        this.EnemyAttack = Random.Range(5,11);
    }else if(this.EnemyIntension == 2){
        this.EnemyAttack = Random.Range(3,6);
    }else{
        this.EnemyAttack = 0;
    }
  }
}
