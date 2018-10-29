using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ViewController : MonoBehaviour
{
    private int maxNum = 20;
    public UIScrollView mScrollView;

    private EquidistanceRecycle mEquidistanceRecycle;


    // Use this for initialization
    void Start()
    {
        mEquidistanceRecycle = new EquidistanceRecycle(mScrollView, maxNum, 60, 8, LoadCell, UpdateCell,1);
        cellCtrlerDic = new Dictionary<GameObject, CellController>(mEquidistanceRecycle.PanelMaxShowCount);
        mEquidistanceRecycle.UpdateCell();
    }

    private void UpdateCell(GameObject go, int dataindex)
    {
        CellController ctrler;
        if (!cellCtrlerDic.TryGetValue(go, out ctrler)) return;
        //ctrler.GameObject.SetActive(dataindex < maxNum);

        if (dataindex < maxNum)
            ctrler.UpdateLbl(dataindex.ToString());
        else
            ctrler.UpdateLbl("");

    }


    private Dictionary<GameObject, CellController> cellCtrlerDic;
    private GameObject LoadCell()
    {
        var mCell = (GameObject)Instantiate(Resources.Load("Cell"));
        var ctrler = new CellController(mCell);
        cellCtrlerDic.Add(mCell, ctrler);
        return ctrler.GameObject;
    }
}
