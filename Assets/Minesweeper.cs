using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Minesweeper : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GameObject _resultPanel;
    [SerializeField] Text _timeText;
    [SerializeField] Text _resultText;

    [SerializeField]
    private int _rows = 1;

    [SerializeField]
    private int _columns = 1;

    [SerializeField]
    private int _mineCount = 1;

    [SerializeField]
    private GridLayoutGroup _gridLayoutGroup = null;

    [SerializeField]
    private Cell _cellPrefab = null;
    Cell[,] cells;
    [Tooltip("最初に開けられたセルかどうか")] bool _isFirstOpen = true;
    private void Start()
    {
        _gridLayoutGroup = GetComponent<GridLayoutGroup>();
        _gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _gridLayoutGroup.constraintCount = _columns;
        GenerateCells();
        GenerateMine();
        foreach (var cell in cells) _ = SearchAroundMine(cell);

    }

    /// <summary> セルを生成する </summary>
    void GenerateCells()
    {
        cells = new Cell[_rows, _columns];
        var parent = _gridLayoutGroup.transform;
        for (var r = 0; r < _rows; r++)
        {
            for (var c = 0; c < _columns; c++)
            {
                var cell = Instantiate(_cellPrefab);
                cell.transform.SetParent(parent);
                cell.Index = new int[2] { r, c };
                cells[r, c] = cell;
            }
        }
    }

    /// <summary> ランダムなセルに地雷を生成する </summary>
    void GenerateMine()
    {
        for (var i = 0; i < _mineCount;)
        {
            var r = Random.Range(0, _rows);
            var c = Random.Range(0, _columns);
            var cell = cells[r, c];
            if (cell.CellState != CellState.Mine)
            {
                cell.CellState = CellState.Mine;
                i++;
            }
        }
    }

    /// <summary> 現在の地雷の配置をリセットする </summary>
    void ResetMine()
    {
        for (var r = 0; r < _rows; r++)
        {
            for (var c = 0; c < _columns; c++)
            {
                var cell = cells[r, c];
                if (cell.CellState == CellState.Mine) cell.CellState = CellState.None;
            }
        }
    }

    /// <summary> 各セルについて周りのセルをListに格納し、周りに何個地雷があるか調べる </summary>
    KeyValuePair<int, List<Cell>> SearchAroundMine(Cell checkCell)
    {
        int[] index = new int[2] { checkCell.Index[0], checkCell.Index[1] };
        int aroundMineCount = 0;
        List<Cell> aroundCells = new List<Cell>();

        for (int aroundR = index[0] - 1; aroundR <= index[0] + 1; aroundR++)
        {
            if (aroundR < 0 || _rows <= aroundR) continue;
            for (int aroundC = index[1] - 1; aroundC <= index[1] + 1; aroundC++)
            {
                if (aroundC < 0 || _columns <= aroundC) continue;
                if (aroundR == index[0] && aroundC == index[1]) continue;
                if (cells[aroundR, aroundC].CellState == CellState.Mine) aroundMineCount++;
                aroundCells.Add(cells[aroundR, aroundC]);
            }
        }

        if (checkCell.CellState != CellState.Mine) checkCell.CellState = (CellState)aroundMineCount;
        return new KeyValuePair<int, List<Cell>>(aroundMineCount, aroundCells);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var target = eventData.pointerCurrentRaycast.gameObject;
        if (target.TryGetComponent<Cell>(out var cell))
        {
            // クリックされたセルに対する処理
            if (eventData.pointerId == -1 && !cell.IsFlag)
            {
                //最初にクリックしたセルが地雷だったら
                if (_isFirstOpen)
                {
                    //地雷じゃなくなるまで再抽選する
                    bool isMineCell = cell.CellState == CellState.Mine;
                    while (isMineCell)
                    {
                        Debug.Log("抽選中");
                        ResetMine();
                        GenerateMine();
                        isMineCell = cell.CellState == CellState.Mine;
                    }
                    //すべてのセルについて、周辺の地雷の数をチェックする
                    foreach (var c in cells) _ = SearchAroundMine(c);
                    _isFirstOpen = false;
                }
                cell.Open(); //左クリックだったら開ける
                if (cell.CellState == CellState.Mine)
                {
                    // ゲームオーバー
                    _resultText.text = "Game Over...";
                    _resultText.color = Color.blue;
                    _resultPanel.SetActive(true);
                }
                else
                {
                    //自動展開する
                    ExpandCells(SearchAroundMine(cell));
                    // 地雷以外のセルがすべて開かれていればゲーム終了
                    if (IsGameClear())
                    {
                        float clearTime = Time.time;
                        _resultText.text = "Game Clear!!";
                        _resultText.color = Color.yellow;   
                        _timeText.text = $"ClearTime:{clearTime.ToString("F")}s";
                        _resultPanel.SetActive(true);

                    }
                }
            }
            else if (eventData.pointerId == -2) cell.Flag(); //右クリックだったら旗を立てる

        }
    }

    bool IsGameClear()
    {
        bool _allComplete = true;
        for (var r = 0; r < _rows; r++)
        {
            for (var c = 0; c < _columns; c++)
            {
                var cell = cells[r, c];
                if (cell.CellState == CellState.Mine) continue;
                if (!cell.IsOpened) _allComplete = false;
            }
        }
        return _allComplete;
    }

    /// <summary> 周辺に地雷がないセルは自動展開する </summary>
    void ExpandCells(KeyValuePair<int, List<Cell>> aroundData)
    {
        if (0 < aroundData.Key) return;
        //周辺に地雷がないとき
        foreach (var cell in aroundData.Value)
        {
            //まだ開いてないセルなら開ける
            if (!cell.IsOpened)
            {
                cell.Open();
                if (cell.CellState != CellState.Mine)
                {
                    ExpandCells(SearchAroundMine(cell));
                }
            }
        }
    }
}
