using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Game
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();

        private static MainThreadDispatcher _instance = null;

        public static MainThreadDispatcher Instance()
        {
            if (!_instance)
            {
                throw new Exception("MainThreadDispatcher not found in scene. Please ensure it is added to the scene.");
            }
            return _instance;
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }

        private void Update()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }
    }
}

