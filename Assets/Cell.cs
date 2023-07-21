using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CellState
{
    None = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,

    Mine = -1,
}

public class Cell : MonoBehaviour
{
    [SerializeField]
    private Text _view = null;

    [SerializeField]
    private CellState _cellState = CellState.None;

    /// <summary> 0スタートでインデックスが入る </summary>
    int[] index = new int[2];
    public int[] Index { get => index; set => index = value; }
    Image _image = default;

    public CellState CellState
    {
        get => _cellState;
        set
        {
            _cellState = value;
            //OnCellStateChanged();
        }
    }

    public bool IsOpened { get; private set; }

    public void Open()
    {
        OnCellStateChanged();
        IsOpened = true;
        _image.color = Color.white;
    }

    public bool IsFlag { get; private set; }

    public void Flag()
    {
        if (_view.text == "")
        {
            _view.text = "F";
            IsFlag = true;
        }
        else
        {
            _view.text = "";
            IsFlag = false;
        }
        _view.color = Color.yellow;
    }
    //private void OnValidate()
    //{
    //}

    private void OnCellStateChanged()
    {
        if (_view == null) { return; }

        if (_cellState == CellState.None)
        {
            _view.text = "";
        }
        else if (_cellState == CellState.Mine)
        {
            _view.text = "X";
            _view.color = Color.red;
        }
        else
        {
            _view.text = ((int)_cellState).ToString();
            _view.color = Color.blue;
        }
    }

    private void Start()
    {
        _image = GetComponent<Image>();
    }
}