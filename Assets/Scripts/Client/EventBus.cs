using System;
using System.Collections.Generic;
using ArenaGame.Shared.Core;
using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// Simple event bus for dispatching simulation events to subscribers
    /// </summary>
    public static class EventBus
    {
        private static Dictionary<Type, List<Action<ISimulationEvent>>> subscribers = new Dictionary<Type, List<Action<ISimulationEvent>>>();
        private static List<Action<ISimulationEvent>> globalSubscribers = new List<Action<ISimulationEvent>>();
        
        public static void Subscribe<T>(Action<ISimulationEvent> handler) where T : ISimulationEvent
        {
            Type eventType = typeof(T);
            if (!subscribers.ContainsKey(eventType))
            {
                subscribers[eventType] = new List<Action<ISimulationEvent>>();
            }
            subscribers[eventType].Add(handler);
        }
        
        public static void SubscribeAll(Action<ISimulationEvent> handler)
        {
            globalSubscribers.Add(handler);
        }
        
        public static void Unsubscribe<T>(Action<ISimulationEvent> handler) where T : ISimulationEvent
        {
            Type eventType = typeof(T);
            if (subscribers.ContainsKey(eventType))
            {
                subscribers[eventType].Remove(handler);
            }
        }
        
        public static void UnsubscribeAll(Action<ISimulationEvent> handler)
        {
            globalSubscribers.Remove(handler);
        }
        
        public static void Publish(ISimulationEvent evt)
        {
            // Call type-specific handlers
            Type eventType = evt.GetType();
            
            if (subscribers.ContainsKey(eventType))
            {
                foreach (var handler in subscribers[eventType])
                {
                    try
                    {
                        handler(evt);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError($"[EventBus] Error in handler for {eventType.Name}: {e}");
                    }
                }
            }
            
            // Call global handlers
            foreach (var handler in globalSubscribers)
            {
                try
                {
                    handler(evt);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"[EventBus] Error in global handler: {e}");
                }
            }
        }
        
        public static void Clear()
        {
            subscribers.Clear();
            globalSubscribers.Clear();
        }
    }
}

