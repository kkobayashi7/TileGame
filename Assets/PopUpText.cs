using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PopUpText : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    public GameObject PopUp;
    public GameObject obj;
    public GameObject canvas;
    public float time = 0.0f;
    public bool popped = false;
    public bool over = false;
    // Start is called before the first frame update
    void Start()
    {
        canvas = GameObject.Find("Canvas");
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if(over == true){
            if(time > 0.8f && popped == false){
                Vector3 TargetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                obj = Instantiate(PopUp, new Vector3(0.0f, 0.0f, 0.0f ), Quaternion.identity);
                obj.transform.SetParent(canvas.transform, false); 
                obj.transform.position = new Vector3(TargetPos.x+3.5f, TargetPos.y+0.5f, 0.0f );
                popped = true;
            }  
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData){
        over = true;
        time = 0.0f;
    }

    public void OnPointerExit(PointerEventData eventData){
        over = false;
        if(obj != null){
            Destroy(obj);
            popped = false;
        }  
    }
}
