using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GameController : MonoBehaviour
{
    public int[,] gameboard = new int[10, 10];     //0 = not found, -1 = mine, other ints = # of mines arround it
    public ButtonRow[] buttonRows;
    public int mineNumber;
    public int remainMines;
    public TMPro.TextMeshProUGUI remainMinesDisplay;
    public int gameState = 0;  //0 = in game, 1 = win, -1 = infectLose, -2 = stepMineLose
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject infectLose;
    public GameObject stepMineLose;
    public int highScore;
    public float prob;

    public int remainingBlank;

    // Start is called before the first frame update
    void Start()
    {
        remainMines = mineNumber;
        for(int i = 0; i < 10; i++){
            for(int j = 0; j < 10; j++){
                buttonRows[i].buttons[j].pos = new Vector2Int(i, j);
                buttonRows[i].buttons[j].controller = this;
            }
        }
        remainingBlank = 10 * 10 - mineNumber;
    }

    // float timer;
    // void Update() {
    //     timer += Time.deltaTime;
    //     //Debug.Log(timer);
    //     var rand = new System.Random();
    //     if(timer >= 1.0f){
    //         timer = 0f;
    //         if(rand.NextDouble() > 1 / suspect){
    //             controller.gameboard[pos.x, pos.y] = -1;
    //             controller.mineNumber += 1;
    //             controller.remainingBlank += -1;
    //             controller.remainMines += 1;
    //             SetState(State.mine);
    //             UpdateState();
    //             Debug.Log("mine generated");
    //         }
    //     }
    // }

    // public void UpdateProbability() {

    // }

    public void UpdateState() {
        remainMinesDisplay.text = "Mines remaining: " + remainMines.ToString();
        
        // check for win / lose
        if(remainingBlank == 0){
            gameState = 1;
        }
        if(remainMines >= 10 * 10){
            gameState = -1;
        }
        winPanel.SetActive(gameState == 1);
        losePanel.SetActive(gameState < 0);
        infectLose.SetActive(gameState == -1);
        stepMineLose.SetActive(gameState == -2);
        // Debug.Log(remainingBlank);
    }

    public static List<Vector2Int> GetNeighbors(int x, int y){
        List<Vector2Int> ret = new List<Vector2Int>();
        for(int i = -1; i < 2; i++){
            for(int j = -1; j < 2; j++){
                if(x + i >= 0 && y + j >= 0 && x + i < 10 && y + j < 10 && !(i == 0 && j == 0)){
                    //Debug.Log("added neighbor");
                    ret.Add(new Vector2Int(x + i, y + j));
                }
            }
        }
        return ret;
    }

    public void onClickNewGame() {
        gameState = 0;
        for(int i = 0; i < 10; i++){
            for(int j = 0; j < 10; j++){
                gameboard[i, j] = 0;
            }
        }
        remainingBlank = 10 * 10 - mineNumber;
        InitGameBoard();    //sets the gameboard state;
        remainMines = mineNumber;

        for(int i = 0; i < 10; i++){
            for(int j = 0; j < 10; j++){
                buttonRows[i].buttons[j].SetState(gameboard[i, j] == -1 ? State.mine : State.notfound);
                buttonRows[i].buttons[j].value = gameboard[i, j];
                buttonRows[i].buttons[j].UpdateState();
            }
        }
        UpdateState();
        Debug.Log(remainingBlank);
    }

    public void onClickQuit() {
        Application.Quit();
    }

    // public void onClickShowState() {
    //     for(int i = 0; i < 10; i++){
    //         for(int j = 0; j < 10; j++){
    //             buttonRows[i].buttons[j].ShowState();
    //         }
    //     }
    // }

    public void InitGameBoard() {
        List<int> possible = Enumerable.Range(0, 10*10).ToList();
        List<int> positions = new List<int>();
        var rand = new System.Random();
        int index, row, col;
        for(int i = 0; i < mineNumber; i++){
            // generating mine position
            index = rand.Next(0, possible.Count);
            row = possible[index] / 10;
            col = possible[index] % 10;
            possible.RemoveAt(index);
            Debug.Log(row.ToString() + " " + col.ToString());
            gameboard[row, col] = -1;

            // generating surrounding numbers
            for(int x = -1; x < 2; x++){
                for(int y = -1; y < 2; y++){
                    if(row + x >= 0 && col + y >= 0 && row + x < 10 && col + y < 10 && gameboard[row + x, col + y] != -1){
                        gameboard[row + x, col + y]+=1;
                    }
                }
            }
        }
    }

    public void OpenNeighbor(int i, int j) {
        // encounter a button that could be: notfound val=0, notfound val!=0, flag, 

        MineButton but = buttonRows[i].buttons[j];

        if(but.state == State.notfound && but.value != 0){
            but.SetState(State.found);
            but.UpdateState();
            remainingBlank += -1;
            return;
        }
        if(but.state == State.flag){
            return;
        }
        if(but.state == State.notfound && but.value == 0){
            but.SetState(State.found);
            but.UpdateState();
            remainingBlank += -1;
            for(int x = -1; x < 2; x++){
                for(int y = -1; y < 2; y++){
                    if(i + x >= 0 && j + y >= 0 && i + x < 10 && j + y < 10){
                        if(buttonRows[i + x].buttons[y + j].state == State.notfound){
                            OpenNeighbor(i + x, y + j);
                        }
                        // else {
                        //     Debug.Log("not found");
                        // }
                    }
                    // else {
                    //     Debug.Log("out of bounds");
                    // }
                }
            }
        }
    }

    public void LabelWrong(){
        for(int i = 0; i < 10; i++){
            for(int j = 0; j < 10; j++){
                if(buttonRows[i].buttons[j].state == State.flag && buttonRows[i].buttons[j].value != -1){
                    buttonRows[i].buttons[j].SetState(State.wrong);
                    buttonRows[i].buttons[j].UpdateState();
                } else if (buttonRows[i].buttons[j].state == State.notfound && buttonRows[i].buttons[j].value == -1){
                    buttonRows[i].buttons[j].SetState(State.mine);
                    buttonRows[i].buttons[j].UpdateState();
                }
            }
        }
    }
}

// TODO: 
//
