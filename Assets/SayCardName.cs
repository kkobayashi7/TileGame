using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SayCardName : MonoBehaviour {

	public CardData cardData;

    void ShowScriptableObjectData(){
		// 参照しているCardDataの中身をコンソールに表示する
		Debug.Log("カード名は" + cardData.CardName + "です。"+ "私の体力は" + PlayerStatus.HP + "です。");
	}

	void Start(){
		ShowScriptableObjectData();
	}
	
	void Update(){
	}
}
