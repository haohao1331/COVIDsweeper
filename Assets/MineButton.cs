using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum State {
    found,
    notfound,
    flag,
    dead,
    mine,
    wrong,
}

public class MineButton : MonoBehaviour, IPointerClickHandler
{
    // total states: neighbor mines: 1-8, -1 mine, 
    public int value;      // +10 -> found, 
    public State state = State.notfound;
    public Vector2Int pos;
    public TMPro.TextMeshProUGUI text;
    public GameController controller;
    public GameObject dead;
    public GameObject flag;
    public GameObject mine;
    public GameObject wrong;
    public GameObject notfound;
    public float timer = 0f;
    public float probvalue;

    List<Vector2Int> neighbors = new List<Vector2Int>();

    public float GetProbability(){
        if(state != State.notfound){
            return 0f;
        }
        if(value == -1){
            return 0f;
        }
        
        float ret = 1f;
        Debug.Log(neighbors.Count);
        foreach(Vector2Int p in neighbors){
            MineButton but = controller.buttonRows[p.x].buttons[p.y];
            Debug.Log("Neighbor value: " + but.value.ToString());
            if(but.state == State.found){
                return 0f;
            } else if (but.value == -1){
                //Debug.Log(ret);
                ret += controller.prob;
            }
        } 
        Debug.Log("ret: " + ret.ToString());
        return ret;
    }

    void Start(){
        // suspect = GetProbability();
        neighbors.AddRange(GameController.GetNeighbors(pos.x, pos.y));
        Debug.Log(GameController.GetNeighbors(pos.x, pos.y).Count);
    }

    void Update() {
        timer += Time.deltaTime;
        //Debug.Log(timer);
        var rand = new System.Random();
        if(timer >= 2.0f){
            timer = 0f;
            if(value == 0 || state == State.found || value == -1){
                return;
            }
            if(rand.NextDouble() * 10 < probvalue * controller.prob){
                controller.gameboard[pos.x, pos.y] = -1;
                controller.mineNumber += 1;
                controller.remainingBlank += -1;
                controller.remainMines += 1;
                Debug.Log("mine generated");
                for(int x = -1; x < 2; x++){
                    for(int y = -1; y < 2; y++){
                        if(pos.x + x >= 0 && pos.y + y >= 0 && pos.x + x < 10 && pos.y + y < 10 && controller.gameboard[pos.x + x, pos.y + y] != -1){
                            controller.gameboard[pos.x + x, pos.y + y] += 1;
                            controller.buttonRows[pos.x + x].buttons[pos.y + y].value += 1;
                        }
                    }
                }
                SetState(State.mine);
                UpdateState();
            }
            controller.UpdateState();
        }
    }

    public void SetState(State newstate) {
        state = newstate;
    }

    public void UpdateState() {
        foreach(Vector2Int p in neighbors){
            if(controller.buttonRows[p.x].buttons[p.y].state == State.found){
                probvalue = 0f;
            }else{
                probvalue = value;
            }
        }

        text.text = value == 0 ? "" : value.ToString();
        timer = 0f;
        // suspect = GetProbability();
        if(state == State.notfound){
            SetTag("notfound");
        }else if(state == State.flag){
            SetTag("flag");
        }else if(state == State.mine){
            SetTag("mine");
        }else if(state == State.dead){
            SetTag("dead");
        }else if(state == State.wrong){
            SetTag("wrong");
        } else {
            SetTag("found");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Middle){
            Debug.Log("Button value: " + value.ToString());
            // Debug.Log("suspect: " + suspect.ToString());
            // Debug.Log("");
            // Debug.Log("");
            // Debug.Log("");
            // Debug.Log("");
            // Debug.Log("");
            // Debug.Log("Middle click: " + value.ToString());
        }else if (eventData.button == PointerEventData.InputButton.Left){
            if (state == State.notfound){
                if(value == -1){
                    state = State.dead;
                    controller.gameState = -2;
                    controller.LabelWrong();
                } else if(value == 0){
                    controller.OpenNeighbor(pos.x, pos.y);
                } else {
                    controller.remainingBlank += -1;
                    state = State.found;
                }
            }
            UpdateState();
            Debug.Log("Left click: " + value.ToString());
        }else if (eventData.button == PointerEventData.InputButton.Right){
            if(state == State.flag){
                state = State.notfound;
                controller.remainMines += 1;
            } else if (state == State.notfound){
                state = State.flag;
                controller.remainMines += -1;
            }
            UpdateState();
            Debug.Log("Right click: " + value.ToString());
        }
        controller.UpdateState();
    }

    public void SetTag(string tag){
        dead.SetActive(false);
        flag.SetActive(false);
        mine.SetActive(false);
        wrong.SetActive(false);
        notfound.SetActive(false);

        if(tag == "dead"){
            dead.SetActive(true);
        } else if (tag == "flag"){
            flag.SetActive(true);
        } else if (tag == "mine"){
            mine.SetActive(true);
        } else if (tag == "wrong"){
            wrong.SetActive(true);
        } else if (tag == "found"){
            
        } else if (tag == "notfound"){
            notfound.SetActive(true);
        } else {
            Debug.Log("Error in SetTag");
        }
    }
}
