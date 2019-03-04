using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] m_gameObjects;
    [SerializeField] private Sprite XSprite;
    [SerializeField] private Sprite OSprite;
    [Header("Game Over Text")]
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private Sprite winText;
    [SerializeField] private Sprite loseText;
    [SerializeField] private Sprite drawText;

    private bool computerMode = true;
    private static string cellsName = "cell";
    private static int m_rows = 3;
    private ETicOrTac[ ,] mas = new ETicOrTac[m_rows,3];

    /// <summary>
    /// true - x; false - o;
    /// </summary>
    private ETicOrTac XO = ETicOrTac.X;

    
    private bool m_computerTurn = false;
    private bool m_GameRun;
    private readonly int[,] lin = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 }, { 1, 4, 7 }, { 2, 5, 8 }, { 3, 6, 9 }, { 1, 5, 9 }, { 3, 5, 7 } };

    
    

    private void Start()
    {
        NewGame();
    }

    public void NewGame()
    {
        foreach (GameObject cell in m_gameObjects)
        {
            cell.transform.Find("CellBack").gameObject.SetActive(false);
            cell.transform.Find("mid").gameObject.SetActive(false);
        }

        int columns = mas.Length / m_rows;

        for (int i = 0; i < m_rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                mas[i, j] = ETicOrTac.Nul;
            }
        }
        gameOverText.SetActive(false);
        m_GameRun = true;
    }

    public void ButtonClick(GameObject o)
    {
        string name = o.name;
        name = name.Replace(cellsName, "");
        int ind = int.Parse(name);
        if ((mas[(ind-1)/m_rows, (ind - 1) % m_rows] == ETicOrTac.Nul) && !m_computerTurn && m_GameRun)
        {
            MakeMove(ind);   
        }
        if (computerMode)
            m_computerTurn = true;
        GameOver();
        if (computerMode && m_GameRun)
            StartCoroutine(ComputerTurn());
    }

    private void MakeMove(int number)
    {
        GameObject o = null;
        string name = cellsName + number;
        foreach (GameObject cell in m_gameObjects)
            if (cell.name == name)
                o = cell;
        Transform sprite = o.transform.Find("mid");
        if (XO == ETicOrTac.X)
            sprite.gameObject.GetComponent<Image>().sprite = XSprite;
        else
            sprite.gameObject.GetComponent<Image>().sprite = OSprite;
        sprite.gameObject.SetActive(true);
        mas[(number - 1) / m_rows, (number - 1) % m_rows] = XO;
        if (XO == ETicOrTac.X)
            XO = ETicOrTac.O;
        else
            XO = ETicOrTac.X;
    }

    private void ShowWin(int number)
    {
        Debug.Log(number);
        GameObject o = null;
        string name = cellsName + number;
        foreach (GameObject cell in m_gameObjects)
            if (cell.name == name)
                o = cell;
        o.transform.Find("CellBack").gameObject.SetActive(true);
    }



    private bool Final(ETicOrTac symbol, ETicOrTac[,] newMas,bool realGame)
    {
        int rows = lin.GetUpperBound(0) + 1;
        int columns = lin.Length / rows;
        int[] indexes = new int[m_rows];
        
        for (int i = 0; i < rows; i++)
        {
            bool result = true;
            int indIndexes = 0;
            for (int j = 0; j < columns && result; j++)
            {
                int ind = lin[i, j];
                if (newMas[(ind - 1) / m_rows, (ind - 1) % m_rows] != symbol)
                    result = false;
                if (realGame)
                {
                    indexes[indIndexes] = ind;
                    indIndexes++;
                }
            }
            if (result)
            {
                if (realGame)
                {
                    for (int k = 0; k < indexes.Length; k++)
                        ShowWin(indexes[k]);
                }
                return true;
            }
        }
        return false;      
    }

    private int? WinComputerResults(ETicOrTac[,] newMas)
    {
        if (Final(ETicOrTac.X, newMas, false)) {
            return -2;
        }
        else {
            if (Final(ETicOrTac.O, newMas,false)) {
                return 2;
            }
            else if (IsFull()) { return 0; }
        }
        return null;
    }


    private bool IsFull()
    {
        foreach (ETicOrTac e in mas)
            if (e == ETicOrTac.Nul)
                return false;
        return true;
    }

    private IEnumerator ComputerTurn()
    {
        
        yield return new WaitForSeconds(0.3f);
        ETicOrTac[,] newMas = new ETicOrTac[m_rows, 3];
        for (int i = 0; i < m_rows; i++)
        {
            for (int j = 0; j < mas.Length / m_rows; j++)
            {
                newMas[i,j] = mas[i, j];
            }
        }
        Dictionary<int,int> ratings = new Dictionary<int,int>();
        for (int i = 0; i < m_rows; i++)
        {
            for (int j = 0; j < mas.Length / m_rows; j++)
            {
                if (newMas[i, j] == ETicOrTac.Nul)
                {
                    newMas[i, j] = ETicOrTac.O;
                    ratings.Add(i * m_rows + j, SimulateTurn(newMas, ETicOrTac.X));
                    newMas[i, j] = ETicOrTac.Nul;
                }
            }
        }
        int max = -200000000;
        int ind = 0;
        foreach(KeyValuePair<int, int> keyValue in ratings)
            if(max < keyValue.Value)
            {
                max = keyValue.Value;
                ind = keyValue.Key;
            }
        MakeMove(ind+1);
        
        GameOver();
        m_computerTurn = false;
    }

    private int SimulateTurn(ETicOrTac[,] newMas, ETicOrTac symbol)
    {
        int result = 0;
        if (WinComputerResults(newMas) == null)
        {
            for (int i = 0; i < m_rows; i++)
            {
                for (int j = 0; j < mas.Length / m_rows; j++)
                {
                    if (newMas[i, j] == ETicOrTac.Nul)
                    {
                        newMas[i, j] = symbol;
                        result += SimulateTurn(newMas, symbol == ETicOrTac.O ? ETicOrTac.X : ETicOrTac.O);
                        newMas[i, j] = ETicOrTac.Nul;
                    }
                }
            }
            return result;
        }
        else
            return result + (int)WinComputerResults(newMas);
    }

    private void GameOver()
    {
        if (Final(ETicOrTac.X,mas,true))
        {
            
            m_GameRun = false;
            gameOverText.gameObject.GetComponent<Image>().sprite = winText;
            gameOverText.SetActive(true);
        }
        else
        {
            if (Final(ETicOrTac.O,mas,true))
            {
                m_GameRun = false;
                gameOverText.gameObject.GetComponent<Image>().sprite = loseText;
                gameOverText.SetActive(true);
            }
            else
            {
                if (IsFull()) {
                    m_GameRun = false;
                    gameOverText.gameObject.GetComponent<Image>().sprite = drawText;
                    gameOverText.SetActive(true);
                }
            }
        }
    }
    
}
