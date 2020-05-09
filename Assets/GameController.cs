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
    public float genProb;

    public int remainingBlank;
    public int activeMines = 0;

    List<Vector2Int> mineLocations = new List<Vector2Int>();


    // for calculating score:
    public TMPro.TextMeshProUGUI scoreDisplay;
    public TMPro.TextMeshProUGUI recordDisplay;
    public double flagCount = 0.0;
    int score;
    int record = 0;
    double timeCount = 0.0;
    public double correct;
    public double wrong;


    // Start is called before the first frame update
    void Start()
    {
        // PlayerPrefs.DeleteAll();
        gameState = -10;
        remainMines = mineNumber;
        for(int i = 0; i < 10; i++){
            for(int j = 0; j < 10; j++){
                buttonRows[i].buttons[j].pos = new Vector2Int(i, j);
                buttonRows[i].buttons[j].controller = this;
            }
        }
        remainingBlank = 10 * 10 - mineNumber;
        record = PlayerPrefs.GetInt("record", -1000);
        onClickNewGame();
    }

    float timer;
    List<Vector2Int> possible;
    void Update() {
        timer += Time.deltaTime;
        //Debug.Log(timer);
        var rand = new System.Random();
        if(allowUpdate){
            if(timer >= 0.5f){
                timer = 0f;
                timeCount += 1;
                possible = GetPossibleGrowingLocations();
                if(rand.NextDouble() < genProb * possible.Count){
                    Vector2Int genPos = possible[rand.Next(0, possible.Count)];
                    gameboard[genPos.x, genPos.y] = -1;
                    mineNumber += 1;
                    remainingBlank += -1;
                    remainMines += 1;
                    foreach(Vector2Int neighbor in GetNeighbors(genPos.x, genPos.y)){
                        if(gameboard[neighbor.x, neighbor.y] != -1){
                            gameboard[neighbor.x, neighbor.y] += 1;
                        }
                    }
                    // buttonRows[genPos.x].buttons[genPos.y].SetState(State.mine); TESTING
                    //buttonRows[genPos.x].buttons[genPos.y].UpdateState();
                    mineLocations.Add(genPos);
                    Debug.Log("mine generated");
                }
                UpdateState();
            }
        }
    }

    bool allowUpdate = true;

    public void UpdateState() {
        for(int i = 0; i < 10; i++){
            for(int j = 0; j < 10; j++){
                // if(gameboard[i, j] == -1){
                //     buttonRows[i].buttons[j].SetState(State.mine);
                // } TESTING
                buttonRows[i].buttons[j].value = gameboard[i, j];
                buttonRows[i].buttons[j].UpdateState();
            }
        }

        remainMinesDisplay.text = "Mines remaining: " + remainMines.ToString();
        
        // check for win / lose
        
        if(remainingBlank == 0){
            gameState = 1;
        }
        if(remainMines + flagCount >= 10 * 10){
            gameState = -1;
        }
        if(gameState == 0){
            score = (int)( 600 * 100 / (mineNumber + 5) / (timeCount * 2 + 1));
        }
        if(gameState != 0){
            score = (int)((600 * 100 / (mineNumber + 5) / (timeCount * 2 + 1)) + correct * 1.0 - wrong * 1.0 + (gameState == 1 ? 1 : -1) * 800.0);
            if(score > record && score != 3000){
                record = (int)score;
                PlayerPrefs.SetInt("record", record);
            }
        }
        
        scoreDisplay.text = "Score: " + score.ToString();
        recordDisplay.text = "Record: " + record.ToString();
        winPanel.SetActive(gameState == 1);
        losePanel.SetActive(gameState < 0);
        infectLose.SetActive(gameState == -1);
        stepMineLose.SetActive(gameState == -2);
        allowUpdate = gameState == 0;
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
        mineNumber = 15;
        timeCount = 0.0;
        flagCount = 0;
        correct = 0;
        wrong = 0;
        //score = (int)( 100 * 100 / (mineNumber + 5) / (timeCount * 2 + 10));
        allowUpdate = true;
        mineLocations = new List<Vector2Int>();
        activeMines = mineNumber;
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
                buttonRows[i].buttons[j].SetState(State.notfound);
                buttonRows[i].buttons[j].value = gameboard[i, j];
                buttonRows[i].buttons[j].UpdateState();
            }
        }
        UpdateState();
        //Debug.Log(remainingBlank);
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

            mineLocations.Add(new Vector2Int(row, col));

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

    public List<Vector2Int> GetPossibleGrowingLocations() {
        List<Vector2Int> pLoc = new List<Vector2Int>();
        bool isgood = true;
        foreach(Vector2Int mine in mineLocations){
            foreach(Vector2Int candidate in GetNeighbors(mine.x, mine.y)){
                //one candidate can be added multiple times
                //verifying candidate
                if(gameboard[candidate.x, candidate.y] != -1 && buttonRows[candidate.x].buttons[candidate.y].state != State.found){
                    foreach(Vector2Int a in GetNeighbors(candidate.x, candidate.y)){
                        if(buttonRows[a.x].buttons[a.y].state == State.found){
                            isgood = false;
                            break;
                        }
                    }
                    if(isgood){
                        pLoc.Add(candidate);
                    }
                }
                isgood = true;
            };
        }
        return pLoc;
    }
}
