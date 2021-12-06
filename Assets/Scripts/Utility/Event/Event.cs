using System;
using System.Collections.Generic;

namespace Utility
{
	public class Event
	{
        private List<Action> m_lsCallbacks = null;

        public Event()
        {
            m_lsCallbacks = new List<Action>();
        }

        public void Add(Action callback)
        {
            if (!m_lsCallbacks.Contains(callback))
                m_lsCallbacks.Add(callback);
        }

        public void Remove(Action callback)
        {
            if (m_lsCallbacks.Contains(callback))
                m_lsCallbacks.Remove(callback);
        }

        public void Invoke()
        {
            for (int i = 0; i < m_lsCallbacks.Count; i++)
                m_lsCallbacks[i]();
        }
    }

    public class Event<T>
    {
        private List<Action<T>> m_lsCallbacks = null;

        public Event()
        {
            m_lsCallbacks = new List<Action<T>>();
        }

        public void Add(Action<T> callback)
        {
            if (!m_lsCallbacks.Contains(callback))
                m_lsCallbacks.Add(callback);
        }

        public void Remove(Action<T> callback)
        {
            if (m_lsCallbacks.Contains(callback))
                m_lsCallbacks.Remove(callback);
        }

        public void Invoke(T param)
        {
            for (int i = 0; i < m_lsCallbacks.Count; i++)
                m_lsCallbacks[i](param);
        }
    }
}