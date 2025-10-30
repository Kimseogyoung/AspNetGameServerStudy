using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_StarGroup : UI_Base
{
    private Image[] _starImageArr;
    private int _maxStarCnt = 0;
    
    protected override void InitImp()
    {
        _starImageArr = BindManyComponent<Image>(UI.StarImage.ToString()).ToArray();
        _maxStarCnt = _starImageArr.Length;
    }

    public void SetMaxStarCnt(int maxStarCnt)
    {
        if (maxStarCnt > _starImageArr.Length)
        {
            LOG.E($"Failed SetMaxStarCnt. StarLength({_starImageArr.Length}) InStarCnt({maxStarCnt})");
            return;
        }

        _maxStarCnt = maxStarCnt;
        for (var i = _starImageArr.Length - 1; i >= 0; i--)
        {
            var starImage = _starImageArr[i];
            starImage.gameObject.SetActive(_maxStarCnt > i);
        }
    }

    public void SetStarCnt(int starCnt)
    {
        if (starCnt > _maxStarCnt )
        {
            LOG.E($"Failed SetStarCnt. MaxStarCnt({_maxStarCnt}) InStarCnt({starCnt})");
            return;
        }

        for (var i = 0; i < 0; i++)
        {
            var starImage = _starImageArr[i];
            starImage.gameObject.SetActive(true);
        }
    }

    enum UI
    {
        StarImage
    }

}
