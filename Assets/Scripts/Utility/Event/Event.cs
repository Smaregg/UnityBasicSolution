using System;
using System.Collections.Generic;

namespace Utility
{
    /// <summary>
    /// 事件
    /// </summary>
	public class Event
	{
        /// <summary>
        /// 添加回调
        /// </summary>
        /// <param name="callback">待添加的回调</param>
        public void Add(Action callback)
        {
            if (!m_lsCallbacks.Contains(callback))
                m_lsCallbacks.Add(callback);
        }

        /// <summary>
        /// 删除回调
        /// </summary>
        /// <param name="callback">待删除的回调</param>
        public void Remove(Action callback)
        {
            if (m_lsCallbacks.Contains(callback))
                m_lsCallbacks.Remove(callback);
        }

        /// <summary>
        /// 触发所有回调
        /// </summary>
        public void Invoke()
        {
            for (int i = 0; i < m_lsCallbacks.Count; i++)
                m_lsCallbacks[i]();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Event()
        {
            m_lsCallbacks = new List<Action>();
        }

        /// <summary>
        /// 回调列表
        /// </summary>
        private List<Action> m_lsCallbacks = null;
    }

    /// <summary>
    /// 带一个参数的事件
    /// </summary>
    /// <typeparam name="T">参数的类</typeparam>
    public class Event<T>
    {
        /// <summary>
        /// 添加回调
        /// </summary>
        /// <param name="callback">待添加的回调</param>
        public void Add(Action<T> callback)
        {
            if (!m_lsCallbacks.Contains(callback))
                m_lsCallbacks.Add(callback);
        }

        /// <summary>
        /// 删除回调
        /// </summary>
        /// <param name="callback">待删除的回调</param>
        public void Remove(Action<T> callback)
        {
            if (m_lsCallbacks.Contains(callback))
                m_lsCallbacks.Remove(callback);
        }

        /// <summary>
        /// 触发所有回调
        /// </summary>
        public void Invoke(T param)
        {
            for (int i = 0; i < m_lsCallbacks.Count; i++)
                m_lsCallbacks[i](param);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Event()
        {
            m_lsCallbacks = new List<Action<T>>();
        }

        /// <summary>
        /// 回调列表
        /// </summary>
        private List<Action<T>> m_lsCallbacks = null;
    }
}