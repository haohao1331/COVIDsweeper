using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickableObject : MonoBehaviour, IPointerClickHandler {
    // public MineButton button;
    // public GameController controller;

    // void Start() {
    //     controller = button.controller;
    //     Debug.Log("controller object assigned");
    // }

    public void OnPointerClick(PointerEventData eventData)
    {
        // if(eventData.button == PointerEventData.InputButton.Middle){
        //     Debug.Log("Middle click: " + button.value.ToString());
        // }else if (eventData.button == PointerEventData.InputButton.Left){
        //     button.found = true;
        //     button.ShowState();
        //     Debug.Log(button.pos.x);
        //     controller.OpenNeighbor(0, 0);
        //     Debug.Log("Left click: " + button.value.ToString());
        //     button.ShowState();
        // }else if (eventData.button == PointerEventData.InputButton.Right){
        //     Debug.Log("Right click: " + button.value.ToString());
        // }
    }
}