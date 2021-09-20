using Sirenix.OdinInspector;
using Sirenix.Utilities;

using System;

using UnityEngine;

[CreateAssetMenu(menuName = "LevelBoard", fileName ="BoardLvl_1")]
public class LevelBoardSO : SerializedScriptableObject
{
    [BoxGroup("Name")]
    [PropertyRange(1, 500)]
    public int levelNumber = 1;

    [BoxGroup("Size")]
    [OnValueChanged(nameof(SetBoardSize))]
    [PropertyRange(5, 25)]
    public int width = 6, height = 6;

    [TableMatrix(DrawElementMethod = nameof(DrawColoredEnumElement), HorizontalTitle = "X axis", VerticalTitle = "Y axis", 
        ResizableColumns = true, Transpose = false, SquareCells = true, HideColumnIndices = true, HideRowIndices = true)]
    public TileType[,] startingBoard = new TileType[6, 6];

    private static TileType DrawColoredEnumElement(Rect rect, TileType value)
    {
        if (rect == null) return TileType.Normal;

        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            int nextValue = (int)(value + 1) % (Enum.GetNames(typeof(TileType)).Length);
            value = (TileType) nextValue;
            GUI.changed = true;
            Event.current.Use();
        }

        Color cellColor = new Color(0, 0, 0, 0.5f);

        switch(value)
        {
            case TileType.Normal: break;
            case TileType.Breakable:
                cellColor = new Color(0.3f, 0.8f, 0.3f);
                break;
            case TileType.DoubleBreakable:
                cellColor = new Color(0.2f, 0.5f, 0.2f);
                break;
            case TileType.Obstacle:
                cellColor = new Color(0.8f, 0.3f, 0.3f);
                break;
        }

        UnityEditor.EditorGUI.DrawRect(rect.Padding(1), cellColor);

        return value;
    }

    private void SetBoardSize()
    {
        if (width < 5 || height < 5) return;
        startingBoard = ResizeArray(startingBoard, width, height);
    }

    T[,] ResizeArray<T>(T[,] original, int rows, int cols)
    {
        var newArray = new T[rows, cols];
        int minRows = Math.Min(rows, original.GetLength(0));
        int minCols = Math.Min(cols, original.GetLength(1));
        for (int i = 0; i < minRows; i++)
            for (int j = 0; j < minCols; j++)
                newArray[i, j] = original[i, j];
        return newArray;
    }
}
