using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;


public class ShowStatus : MonoBehaviour { 

    private BattleManager battleManager;
    public TextMeshProUGUI text;
    public string intension;
      // 初期化
      void Start () {
        battleManager = FindObjectOfType<BattleManager>();
        if(battleManager == null){
            Debug.Log("BatttleManagerコンポーネントを持つオブジェクトが存在しません.");
        }
      }
 
    // 更新
    void Update () {
        text.text =  "体力: " + PlayerStatus.HP.ToString() + "/" + PlayerStatus.Max_HP.ToString() + "\n" 
                    +"エナジー: " + battleManager.Energy.ToString() + "/" + PlayerStatus.Max_Energy.ToString()+ "\n" 
                    +"防御値: " + battleManager.Block.ToString() +  "\n\n"
                    +"山札: " +  battleManager.DeckCopy.Count + "\n"
                    +"捨て札: " +  battleManager.DiscardPile.Count + "\n"
                    +"廃棄: " +  battleManager.ExilePile.Count + "\n\n";
    }
}