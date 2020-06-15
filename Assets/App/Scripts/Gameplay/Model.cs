using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class Vector3Extension
{
    public static Vector2 Round(this Vector3 v)
    {
        int x = Mathf.RoundToInt(v.x);
        int y = Mathf.RoundToInt(v.y);
        return new Vector2(x, y);
    }
}


public class Model : MonoBehaviour
{
    /// <summary>
    /// Upgrade score interval, upgrade once for each scoreStep
    /// </summary>
    public int scoreStep = 5000;
    public const int kNormalRows = 20;
    public const int kMaxRows = 22;
    public const int kMaxColumns = 10;
    public GameObject mapMaskGameObject;

    private SpriteRenderer[] mMapMask;
    private Transform[,] mMap = new Transform[kMaxColumns, kMaxRows];
    private Sequence mMapSequence;
    private int Row { get; set; }
    private int Level { get; set; }
    private int Score { get; set; }
    private int HighScore { get; set; }


    public bool IsShapePositionValid(Transform shapeTransform)
    {
        foreach (Transform childTransform in shapeTransform)
        {
            if (childTransform.CompareTag("Block"))
            {
                Vector2 position = childTransform.position.Round();
                //Map boundary
                if (IsInsideMap(position) == false)
                {
                    return false;
                }
                //Other shape
                if (mMap[(int)position.x, (int)position.y] != null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void PlaceShape(Transform shapeTransform)
    {
        foreach (Transform childTransform in shapeTransform)
        {
            if (childTransform.CompareTag("Block"))
            {
                Vector2 position = childTransform.position.Round();
                mMap[(int)position.x, (int)position.y] = childTransform;
            }
        }
        CheckMap();
    }

    public void CheckMap()
    {
        int count = 0;//Count lines and score
        int firstRowIndex = -1;
        for (int i = 0; i < kMaxRows; i++)
        {
            bool isFull = CheckIsRowFull(i);
            if (isFull)
            {
                count++;
                Row++;
                if (firstRowIndex == -1)
                {
                    firstRowIndex = i;
                }
                AudioManager.Instance.PlayLineClear();
            }
            //If you go to the last line
            if (i == (kMaxRows - 1))
            {
                //If there is a line to eliminate
                if (firstRowIndex != -1)
                {
                    //Pause the game and stop the block from falling
                    GameManager.Instance.PauseGame();
                    ClearOneRow(firstRowIndex, count);
                }
            }
        }
        UpdateScore(count);
        if (count > 0)
        {
            UpdateLevel();
        }
        EventManager.Instance.Fire(UIEvent.REFRESH_SCORE, 0);
    }

    public bool IsGameOver()
    {
        //If there is a square in the top row
        for (int i = 0; i < kMaxColumns; i++)
        {
            if (mMap[i, kMaxRows - 2] != null)
            {
                SaveData();
                return true;
            }
        }
        return false;
    }

    public int[] GetScoreInfo()
    {
        //todo  change to json format
        return new[] { HighScore, Score, Row, Level };
    }


    public void ClearData()
    {
        HighScore = 0;
        Score = 0;
        Row = 0;
        Level = 0;
        SaveData();
    }

    public void RefreshGame()
    {
        //map and data
        mMap = new Transform[kMaxColumns, kMaxRows];
        GameManager.Instance.RestartGame();
        Score = 0;
        Row = 0;
        Level = 0;
        EventManager.Instance.Fire(UIEvent.REFRESH_SCORE, 0);

    }

    private void Awake()
    {
        LoadData();
        mMapSequence = DOTween.Sequence();
        mMapSequence.SetAutoKill(false);
        mMapMask = mapMaskGameObject.GetComponentsInChildren<SpriteRenderer>();
    }

    private bool CheckIsRowFull(int rowIndex)
    {
        for (int i = 0; i < kMaxColumns; i++)
        {
            if (mMap[i, rowIndex] == null)
            {
                return false;
            }
        }
        return true;
    }

    private bool IsInsideMap(Vector2 position)
    {
        return position.x >= 0 && position.x < kMaxColumns && position.y >= 0;
    }

    private void UpdateScore(int count)
    {
        if (count == 0)
        {
            Score += 20;
        }
        else
        {
            Score += count * 1000;
        }
        if (Score > HighScore)
        {
            HighScore = Score;
        }
    }

    private void UpdateLevel()
    {
        var tempLevel = Score / scoreStep;
        if (tempLevel > Level)
        {
            Level = tempLevel;
            GameManager.Instance.UpgradeLevel();
        }
    }

    private void ClearOneRow(int firstRowIndex, int count)
    {
        for (int j = 0; j < count; j++)
        {
            var j1 = j;
            //Line elimination animation
            mMapMask[firstRowIndex + j].DOFade(1, 0.2f)
                .SetLoops(4)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    mMapMask[firstRowIndex + j1].color = new Color(1, 1, 1, 0);
                    for (int i = 0; i < kMaxColumns; i++)
                    {
                        Destroy(mMap[i, firstRowIndex + j1].gameObject);
                        //地图要置空
                        mMap[i, firstRowIndex + j1] = null;
                    }
                    if (j1 == count - 1)
                    {
                        MoveLines(firstRowIndex, count);
                        GameManager.Instance.StartGame();
                    }
                });
        }
    }
    /// <summary>
    /// Move the remaining blocks down
    /// </summary>
    /// <param name="lineIndex"></param>
    /// <param name="count"></param>
    private void MoveLines(int lineIndex, int count)
    {
        for (int i = lineIndex + count; i < kNormalRows; i++)
        {
            for (int j = 0; j < kMaxColumns; j++)
            {
                if (mMap[j, i] == null)
                {
                    continue;
                }
                mMap[j, i - count] = mMap[j, i];
                mMap[j, i] = null;
                mMap[j, i - count].position += Vector3.down * count;
            }
        }
    }

    private void LoadData()
    {
        HighScore = PlayerPrefs.GetInt("HighestScore", 0);
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("HighestScore", HighScore);
    }
}
