using Serial;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Position : MonoBehaviour, IPointerClickHandler
{
    public Slider slider;
    public SerialConnect serial;

    // commit frequency settings
    public const int commitInterval = 500;
    public const int inputDelayTime = 100;
    // 
    long lastCommitTick = 0;
    readonly LinkedList<PositionWithTime> inputCache = new LinkedList<PositionWithTime>();
    private struct PositionWithTime {
        public readonly float pos;
        public readonly long tick;
        public PositionWithTime(float pos, long tick)
        {
            this.pos = pos;
            this.tick = tick;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        slider.onValueChanged.AddListener(OnValue);
        serial = SerialConnect.GetInstance();
        serial.StartListening();
    }

    // Update is called once per frame
    // 如果想要快速相应，那么在按下的瞬间确实应该触发
    // 但是为了满足点击的时候不要重复触发，所以可以收到数据的时候暂缓一会儿，
    // 需要一点缓冲区和松开鼠标之后立即清空缓冲。
    // 同时这里要尊重最后一个值。
    // 但是中间如果想要减少卡顿，那么中间的触发应该进一步减少，但是这不好做
    void Update()
    {
        if(CommitWithInterval(ReadInputCache()))
        {
            inputCache.RemoveFirst();
        }
        else
        {
            if(inputCache.Count > 1)
            {
                inputCache.RemoveFirst();
            }
        }
    }


    void OnDestroy()
    {
        serial.Close();
    }

    // 这个在拖拽途中触发
    void OnValue(float value)
    {
        Debug.Log("Drag to " + value);
        AddInputCache(value);
    }
    // 这个在点击松开或者拖拽结束的时候触发，
    // 实际上在按下的时候会有一次value change但是那个时候无法区分是拖拽还是点击
    public void OnPointerClick(PointerEventData eventData)
    {
        DropInputCache();
        CommitValue(slider.value);
        Debug.Log("Clicked");
    }

    private void AddInputCache(float value)
    {
        inputCache.AddLast(new PositionWithTime(value, DateTime.Now.Ticks));
    }
    private float ReadInputCache()
    {
        if (inputCache.Count == 0)
            return -1;
        PositionWithTime pt = inputCache.First.Value;
        if(new TimeSpan(DateTime.Now.Ticks - pt.tick).TotalMilliseconds > inputDelayTime)
        {
            return pt.pos;
        }
        return -1;
    }
    private void DropInputCache()
    {
        inputCache.Clear();
    }
    private bool CommitWithInterval(float toCommit)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (toCommit != -1)
        {
            Debug.Log("input: " + toCommit);
            long ticks = DateTime.Now.Ticks;
            // commit freq_cache at the pace of interval
            if (new TimeSpan(ticks - lastCommitTick).TotalMilliseconds > commitInterval)
            {
                lastCommitTick = ticks;
                CommitValue(toCommit);
                return true;
            }
        }
        return false;
    }

    void CommitValue(float value)
    {
        serial.SendLine("sig mpp " + (int)value);
    }
}
