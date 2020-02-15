﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    private Dictionary<string, GameObject> mUIDic;

    /// <summary>
    /// 可隐藏界面栈
    /// </summary>
    private Stack<string> mHideUIStack;

    /// <summary>
    /// 当前打开界面名字
    /// </summary>
    private string mCurrentOpenUIName = "";

    public float mRectWidth = Screen.width;
    public float mRectHeight = Screen.height;

    /// <summary>
    /// 当前界面最大层
    /// </summary>
    private int mCurrentMaxLayer = 0;

    private static UIManager _Instance = null;
    public static UIManager Instance
    {
        get
        {
            return _Instance;
        }
    }

    public HUDUI mHUDUIPanel;

    void Awake()
    {
        _Instance = this;

        mUIDic = new Dictionary<string, GameObject>();
        mHideUIStack = new Stack<string>();

        UIRoot root = GameObject.Find("UI Root").GetComponent<UIRoot>();
        float scale = root.activeHeight / mRectHeight;
        mRectWidth *= scale;
        mRectHeight *= scale;

        // 先隐藏摇杆
        GameObject parent = GameObject.Find("UI Root/Camera/JoyStickUI");
        parent.SetActive(false);

        // 获取头顶文字，伤害飘字，血条等panel
        CreateHUDUI();
    }

    void CreateHUDUI()
    {
        GameObject go = ResourceManager.Instance.GetUIPrefab("HUDUI");
        if (go == null)
        {
            return;
        }

        go = NGUITools.AddChild(gameObject, go);
        if (go == null)
        {
            return;
        }

        go.name = "HUDUI";
        mHUDUIPanel = go.AddComponent<HUDUI>();
    }

    public Transform OpenUI(string uiName)
    {
        if (string.IsNullOrEmpty(uiName))
        {
            LogSystem.LogError("OpenUI uiName is null");

            return null;
        }

        // 不重复打开相同界面
        if (mCurrentOpenUIName == uiName)
        {
            return mUIDic[mCurrentOpenUIName].transform;
        }

        mCurrentOpenUIName = uiName;

        // 界面存在于内存中，之前被隐藏的。
        if (mHideUIStack.Contains(uiName))
        {
            ShowUI(uiName);

            return mUIDic[mCurrentOpenUIName].transform;
        }

        GameObject uiPrefab = ResourceManager.Instance.GetUIPrefab(uiName);
        if (uiPrefab == null)
        {
            LogSystem.LogError("uiPrefab is null");

            return null;
        }

        GameObject go = NGUITools.AddChild(gameObject, uiPrefab);
        go.SetActive(true);
        go.name = uiName;
        go.AddComponent(System.Type.GetType(uiName));
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.GetComponent<UIPanel>().depth = mCurrentMaxLayer++;
        go.layer = LayerMask.NameToLayer("NGui");

        // 将当前界面隐藏
        if (mHideUIStack.Count > 0)
        {
            HideUI(mHideUIStack.Peek());
        }

        mHideUIStack.Push(uiName);

        mUIDic.Add(uiName, go);

        return go.transform;
    }

    public void CloseUI(string uiName)
    {
        if (string.IsNullOrEmpty(uiName))
        {
            return;
        }

        if (!mUIDic.ContainsKey(uiName))
        {
            return;
        }

        GameObject go = mUIDic[uiName];
        if (go != null)
        {
            GameObject.Destroy(go);
        }

        mUIDic.Remove(uiName);

        --mCurrentMaxLayer;

        mHideUIStack.Pop();

        if (mHideUIStack.Count > 0)
        {
            OpenUI(mHideUIStack.Peek());
        }
    }

    public void CloseAllUI()
    {
        int count = mUIDic.Count;
        if (count < 1)
        {
            return;
        }

        GameObject go = null;
        List<string> list = new List<string>(mUIDic.Keys);

        for (int i = 0; i < count; ++i)
        {
            go = mUIDic[list[i]];
            if (go == null)
            {
                continue;
            }

            GameObject.Destroy(go);
        }

        mUIDic.Clear();
    }

    private void HideUI(string uiName)
    {
        if (string.IsNullOrEmpty(uiName))
        {
            return;
        }

        if (!mHideUIStack.Contains(uiName))
        {
            return;
        }

        if (!mUIDic.ContainsKey(uiName))
        {
            return;
        }

        if (mHideUIStack.Peek() != uiName)
        {
            return;
        }

        GameObject go = mUIDic[uiName];
        if (go == null)
        {
            return;
        }

        go.SetActive(false);
    }

    private void ShowUI(string uiName)
    {
        if (string.IsNullOrEmpty(uiName))
        {
            return;
        }

        if (!mHideUIStack.Contains(uiName))
        {
            return;
        }

        if (!mUIDic.ContainsKey(uiName))
        {
            return;
        }

        GameObject go = mUIDic[uiName];
        if (go == null)
        {
            return;
        }

        go.SetActive(true);
    }

    public T GetUI<T>(string uiName) where T : MonoBehaviour
    {
        if (!mUIDic.ContainsKey(uiName))
        {
            OpenUI(uiName);
        }

        return mUIDic[uiName].GetComponent<T>();
    }
}