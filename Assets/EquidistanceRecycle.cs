using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class EquidistanceRecycle
{
    private int cellSize;
    private float halfCellSize;

    /// <summary>
    /// 数据行列数(水平为列，垂直为行)
    /// </summary>
    private int mDataColumnLimit;
    /// <summary>
    /// 数据总数
    /// </summary>
    private int DataCount;

    private UIScrollView mScrollView;
    private UIPanel mPanel;
    private UIScrollView.Movement mMovement;
    private GameObject cellParent;
    private GameObject cellPool;
    private Bounds mPanelBounds;

    public EquidistanceRecycle(UIScrollView sv, int dataCount, int size, int dataColum,
        OnLoadItem loadItem, OnUpdateItem updateItem, int extraShownum = 1)
    {
        mScrollView = sv;
        DataCount = dataCount;
        cellSize = size;
        halfCellSize = (float)cellSize / 2;

        mDataColumnLimit = dataColum;

        onLoadItem = loadItem;
        onUpdateItem = updateItem;

        extraShowNum = extraShownum;
        InitNeed();
    }

    private void InitNeed()
    {
        if (IsSvNull()) return;
        mPanel = mScrollView.panel;
        Vector3 center = mScrollView.transform.position;
        Vector3 boundSize = mPanel.GetViewSize();
        mPanelBounds = new Bounds(center, boundSize);
        mMovement = mScrollView.movement;
        RegisterEvent();

        cellParent = NGUITools.AddChild(mScrollView.gameObject);
        cellParent.name = "EquidistanceRecycle";
        InitPanelColRow();


    }




    #region  辅助

    private bool IsSvNull()
    {
        return mScrollView == null;
    }

    private bool IsPanelNull()
    {
        return mPanel == null;
    }

    //panel左边界
    private float mPanelLeftPos
    {
        get
        {
            if (IsPanelNull()) return 0;
            return -mPanel.baseClipRegion.z / 2;
        }
    }
    //panel右边界
    private float mPanelRightPos
    {
        get
        {
            if (IsPanelNull()) return 0;
            return mPanel.baseClipRegion.z / 2;
        }
    }
    //panel上边界
    private float mPanelTopPos
    {
        get
        {
            if (IsPanelNull()) return 0;
            return mPanel.baseClipRegion.w / 2;
        }
    }
    //panel下边界
    private float mPanelDownPos
    {
        get
        {
            if (IsPanelNull()) return 0;
            return -mPanel.baseClipRegion.w / 2;
        }
    }

    //当前行/列
    private int mCurCol;

    private bool ReFreshData;

    //最左
    private float mPanelLeft;
    private float PanelCellLeft
    {
        get
        {
            if (ReFreshData || mPanelLeft == 0)
                mPanelLeft = -mPanelBounds.extents.x + halfCellSize;
            return mPanelLeft;
        }
    }
    //最上
    private float mPanelTop;
    private float PanelCellTop
    {
        get
        {
            if (ReFreshData || mPanelTop == 0)
                mPanelTop = mPanelBounds.extents.y - halfCellSize;
            return mPanelTop;
        }
    }
    //最右
    private float mPanelRight;

    private float PanelCellRight
    {
        get
        {
            if (ReFreshData || mPanelRight == 0)
                mPanelRight = mPanelBounds.extents.x - halfCellSize;
            return mPanelRight;
        }
    }
    //最下
    private float mPanelBottom;

    private float PanelCellBottom
    {
        get
        {
            if (ReFreshData || mPanelBottom == 0)
                mPanelBottom = -mPanelBounds.extents.y + halfCellSize;
            return mPanelBottom;
        }
    }
    //整体cell是否在panel里
    private bool IsAllInHoriPanel(float x)
    {
        var cellXLeft = x - halfCellSize;
        var cellXRight = x + halfCellSize;
        var horizontialIn = cellXLeft >= mPanelLeftPos && cellXRight <= mPanelRightPos;
        return horizontialIn;
    }
    private bool IsAllInVerPanel(float y)
    {
        var cellYTop = y + halfCellSize;
        var cellYDown = y - halfCellSize;
        var verticalIn = cellYTop <= mPanelTopPos && cellYDown >= mPanelDownPos;
        return verticalIn;
    }
    //整体cell是否不在panel里
    private bool IsAllOutHoriPanel(float x)
    {
        var cellXLeft = x - halfCellSize;
        var cellXRight = x + halfCellSize;
        var horizontialOut = cellXRight <= mPanelLeftPos || cellXLeft >= mPanelRightPos;
        return horizontialOut;
    }
    private bool IsAllOutVerPanel(float y)
    {
        var cellYTop = y + halfCellSize;
        var cellYDown = y - halfCellSize;
        var verticalOut = cellYDown >= mPanelTopPos || cellYTop <= mPanelDownPos;
        return verticalOut;
    }
    #endregion

    #region 委托

    public delegate GameObject OnLoadItem();
    public delegate void OnUpdateItem(GameObject go, int dataIndex);

    private OnLoadItem onLoadItem;
    private OnUpdateItem onUpdateItem;

    #endregion

    /// <summary>
    //界面容许显示的最大列
    /// </summary>
    private int mPanelColumnLimit;
    /// <summary>
    //界面容许显示的最大行
    /// </summary>
    private int mPanelRowLimit;

    /// <summary>
    //界面容许显示的最大数量
    /// </summary>
    public int PanelMaxShowCount { get; private set; }

    /// <summary>
    /// 界面显示时，需要额外增加的行数/列数
    /// </summary>
    private int extraShowNum;

    private List<GameObject> cellGoList;

    /// <summary>
    /// 初始化显示行列数
    /// </summary>
    private void InitPanelColRow()
    {
        if (IsPanelNull()) return;

        float cellX, cellY;
        int curHang = 0, curLie = 0;
        if (mDataColumnLimit == 0) return;

        if (mMovement == UIScrollView.Movement.Horizontal)
        {
            //Debug.LogError("-----------X-----------");
            cellX = PanelCellLeft;
            while (IsAllInHoriPanel(cellX))
            {
                //界面可显示的列 大于 指定数据显示列
                if (curLie >= mDataColumnLimit) break;
                //cell的左边
                if (cellX - halfCellSize <= mPanelRightPos)
                {
                    curLie++;
                    mPanelColumnLimit = curLie;
                }
                cellX = PanelCellLeft + curLie * cellSize;
                //Debug.LogError(cellX);
            }
            //Debug.LogError("-----------Y-----------");

            cellY = PanelCellTop;
            while (IsAllInVerPanel(cellY))
            {
                //cell的上边
                if (cellY - halfCellSize >= mPanelDownPos)
                {
                    curHang++;
                    mPanelRowLimit = curHang;
                }
                cellY = PanelCellTop - curHang * cellSize;
                //Debug.LogError(cellY);
            }
        }
        if (mMovement == UIScrollView.Movement.Vertical)
        {
            //todo:
        }
        PanelMaxShowCount = mPanelColumnLimit * mPanelRowLimit;
        cellGoList = new List<GameObject>(PanelMaxShowCount);
        //Debug.LogError("-----------Result-----------");
        //Debug.LogError(mPanelColumnLimit);
        //Debug.LogError(mPanelRowLimit);
    }

    public void UpdateCell()
    {
        float cellX, cellY;
        GameObject go;
        Transform tf;
        int rowLimit = mMovement == UIScrollView.Movement.Vertical ? mPanelRowLimit + extraShowNum : mPanelRowLimit;
        int lineLimit = mMovement == UIScrollView.Movement.Horizontal ? mPanelColumnLimit + extraShowNum : mPanelColumnLimit;
        int dataIndex = 0;
        for (int curHang = 0; curHang < rowLimit; curHang++)
        {
            for (int curLine = 0; curLine < lineLimit; curLine++)
            {

                cellX = PanelCellLeft + curLine * cellSize;
                cellY = PanelCellTop - curHang * cellSize;

                go = onLoadItem();
                cellGoList.Add(go);

                tf = go.transform;
                tf.SetParent(cellParent.transform);
                tf.localScale = Vector3.one;
                tf.localPosition = new Vector3(cellX, cellY, 0);
                //Debug.LogError(string.Format("{0},{1}", cellX, cellY));
                dataIndex = curHang * mDataColumnLimit + curLine;
                if (mMovement == UIScrollView.Movement.Horizontal && lineLimit <= mDataColumnLimit)
                    onUpdateItem(go, dataIndex);
            }
        }
    }


    //水平：左边为首列，右边有尾列
    /// <summary>
    /// 界面显示的cell首列下标
    /// </summary>
    private int curFirstColIndex = 0;

    /// <summary>
    /// 数据首列下标
    /// </summary>
    private int curFirstShowDataColIndex = 0;


    private enum DragmoveDir
    {
        None,
        Left,
        Right,
        Top,
        Down
    }
    /// <summary>
    /// 检测并移动
    /// </summary>
    private void CheckCellMove()
    {
        int rowLimit = mMovement == UIScrollView.Movement.Vertical ? mPanelRowLimit + extraShowNum : mPanelRowLimit;
        int lineLimit = mMovement == UIScrollView.Movement.Horizontal ? mPanelColumnLimit + extraShowNum : mPanelColumnLimit;
        if (cellGoList.Count <= lineLimit) return;

        int curLastColIndex, curLastShowDateColIndex;
        int cellIndex, moveColIndex, dataIndex = 0;

        bool isCanMove = true;
        float cellX = 0;
        if (mMovement == UIScrollView.Movement.Horizontal)
        {

            curFirstColIndex = curFirstColIndex % lineLimit;
            curLastColIndex = (curFirstColIndex + lineLimit - 1) % lineLimit;

            curFirstShowDataColIndex = curFirstShowDataColIndex % mDataColumnLimit;
            curLastShowDateColIndex = (curFirstShowDataColIndex + mPanelColumnLimit) % mDataColumnLimit;

            int moveDir = mPanel.clipOffset.x - panelStartOffset > 0 ? 1 : mPanel.clipOffset.x - panelStartOffset < 0 ? -1 : 0;
            DragmoveDir dragDir = moveDir == 1 ? DragmoveDir.Left : moveDir == -1 ? DragmoveDir.Right : DragmoveDir.None;
            if (dragDir == DragmoveDir.None) return;

            //需要移动的列index
            moveColIndex = dragDir == DragmoveDir.Left ? curFirstColIndex : dragDir == DragmoveDir.Right ? curLastColIndex : -1;

            cellX = cellGoList[moveColIndex].transform.localPosition.x;
            cellX = cellX - mPanel.clipOffset.x;
            //左拖动 offset变大 cell右移 x增加
            if (IsAllOutHoriPanel(cellX))
            {
                for (int hangIndex = 0; hangIndex < rowLimit; hangIndex++)
                {
                    cellIndex = hangIndex * lineLimit + moveColIndex;
                    cellX = cellGoList[cellIndex].transform.localPosition.x + moveDir * lineLimit * cellSize;

                    if (dragDir == DragmoveDir.Left)
                    {
                        dataIndex = curLastShowDateColIndex + extraShowNum;
                        if (dataIndex >= mDataColumnLimit) //到尾限制
                            return;

                    }
                    else if (dragDir == DragmoveDir.Right)
                    {
                        dataIndex = curFirstShowDataColIndex - extraShowNum;
                        if (dataIndex < 0) //到头限制
                            return;


                    }
                    dataIndex = hangIndex * mDataColumnLimit + dataIndex;
                    onUpdateItem(cellGoList[cellIndex], dataIndex);
                    cellGoList[cellIndex].transform.SetLocalX(cellX);
                }

                if (dragDir == DragmoveDir.Left)
                {
                    curFirstShowDataColIndex++;
                    curFirstColIndex++;
                }
                else if (dragDir == DragmoveDir.Right)
                {
                    curFirstShowDataColIndex--;
                    curFirstColIndex = curLastColIndex;
                }



            }
        }
        if (mMovement == UIScrollView.Movement.Vertical)
        {
            //todo
        }

    }

    #region  事件

    private float panelStartOffset;
    private void RegisterEvent()
    {
        if (mPanel != null) mPanel.onClipMove += OnClipMove;
        if (mScrollView != null)
        {
            mScrollView.onStoppedMoving += OnStoppedMoving;
            mScrollView.onDragStarted += OnDragStarted;
            //    mScrollView.onDragFinished += OnDragFinished;
            //    mScrollView.onScrollWheel += OnScrollWheel;
            //}
        }
    }

    private void OnDragStarted()
    {

        panelStartOffset = mMovement == UIScrollView.Movement.Horizontal ? mPanel.clipOffset.x : mPanel.clipOffset.y;
    }

    private void OnStoppedMoving()
    {
        CheckCellMove();

    }

    private void OnClipMove(UIPanel panel)
    {
        CheckCellMove();
    }

    #endregion


}
public static class Extensions
{
    public static void SetLocalX(this Transform cell, float x)
    {
        float y = cell.transform.localPosition.y;
        cell.transform.localPosition = new Vector3(x, y, 0);
    }
}
