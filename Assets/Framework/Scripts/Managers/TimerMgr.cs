using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TimerMgr : UnitySingleton<TimerMgr>
{
    public delegate void TimeHandler(object param);
    
    class TimerNode
    {
        public int timerID;
        public TimeHandler OnTimer;
        public object param;
        public int repeat;
        public float nextTriggerTime;
        public float currTime;
        public float interval;
        public bool isCancel;
    }
    
    private Dictionary<int, TimerNode> timerNodes;
    private int autoTimerID = 1;
    private List<int> removeTimerQueue;
    
    public void Init()
    {
        this.timerNodes = new Dictionary<int, TimerNode>();
        this.removeTimerQueue = new List<int>();
        this.autoTimerID = 1;
    }

    void Update()
    {
        if (this.timerNodes == null) return;

        foreach (var key in this.timerNodes.Keys)
        {
            TimerNode timerNode = this.timerNodes[key];
            if (timerNode == null || timerNode.isCancel == true)
            {
                continue;
            }
            
            timerNode.currTime += Time.deltaTime;
            if (timerNode.currTime >= timerNode.nextTriggerTime)
            {
                timerNode.OnTimer(timerNode.param);
                timerNode.repeat = (timerNode.repeat > 0) ? (timerNode.repeat - 1) : timerNode.repeat;
                timerNode.nextTriggerTime = timerNode.interval;
                timerNode.currTime = 0;
                if (timerNode.repeat == 0)//结束，删除
                {
                    this.Unschedule(timerNode.timerID);
                }
            }
        }

        // foreach (var key in this.timerNodes.Keys)
        // {
        //     TimerNode timerNode = this.timerNodes[key];
        //     if(timerNode == null || timerNode.isCancel == false) continue;
        //     this.removeTimerQueue.Add(key);
        // }

        foreach (var key in this.removeTimerQueue)
        {
            if (this.timerNodes.ContainsKey(key))
            {
                this.timerNodes.Remove(key);
            }
        }
        this.removeTimerQueue.Clear();
    }

    public int Schedule(TimeHandler onTimer, object param, int repeat, float interval, float delay)
    {
        if (onTimer == null || interval < 0.0f || delay < 0.0f)
        {
            return 0;
        }
        int timerID = this.autoTimerID++;
        this.autoTimerID = this.autoTimerID == 0 ? 1 : this.autoTimerID;
        
        TimerNode timerNode = new TimerNode();
        timerNode.OnTimer = onTimer;
        timerNode.param = param;
        timerNode.repeat = repeat <= 0 ? -1 : repeat;
        timerNode.currTime = 0;
        timerNode.interval = interval;
        timerNode.nextTriggerTime = delay;
        timerNode.timerID = timerID;
        timerNode.isCancel = false;
        
        this.timerNodes.Add(timerID, timerNode);
        
        return timerNode.timerID;
    }
    
    public int Schedule(TimeHandler onTimer, int repeat, float interval, float delay)
    {
        return Schedule(onTimer, null, repeat, interval, delay);
    }

    public void Unschedule(int timerID)
    {
        if (timerNodes.ContainsKey(timerID))
        {
            TimerNode timerNode = timerNodes[timerID];
            if (timerNode != null)
            {
                timerNode.isCancel = true;
                this.removeTimerQueue.Add(timerID);
            }
        }
    }

    public int ScheduleOnce(TimeHandler onTimer, object param, float delay)
    {
        return Schedule(onTimer, param, 1, 0, delay);
    }
    
    public int ScheduleOnce(TimeHandler onTimer, float delay)
    {
        return ScheduleOnce(onTimer, null, delay);
    }
}
