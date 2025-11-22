using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventMgr : UnitySingleton<EventMgr>
{
    public delegate void OnEventAction(string eventName, object param);
    private Dictionary<string, OnEventAction> eventActions = null;
    
    public void Init()
    {
        eventActions = new Dictionary<string, OnEventAction>();
    }

    public void AddListener(string eventName, OnEventAction onEvent)
    {
        if (eventActions.ContainsKey(eventName))
        {
            this.eventActions[eventName] += onEvent;  
        }
        else
        {
            this.eventActions[eventName] = onEvent;  
        }
    }
    
    public void RemoveListener(string eventName, OnEventAction onEvent)
    {
        if (eventActions.ContainsKey(eventName))
        {
            this.eventActions[eventName] -= onEvent;  
        }

        if (this.eventActions[eventName] == null)
        {
            this.eventActions.Remove(eventName);
        }
    }
    
    public void Emit(string eventName, object udata)
    {
        if (eventActions.ContainsKey(eventName))
        {
            if (this.eventActions[eventName] != null)
            {
                this.eventActions[eventName](eventName, udata);
            }
        }
    }
    
}
