using System.Collections.Generic;

namespace Utility
{
    /// <summary>
    /// 对象池
    /// </summary>
    /// <typeparam name="T">有默认构造函数的类</typeparam>
    public class ObjectPool<T> where T : class, new()
    {
        /// <summary>
        /// 从池子中取对象
        /// </summary>
        /// <returns>取到的对象</returns>
        public T Get()
        {
            if (m_stkObjects.Count != 0)
                return m_stkObjects.Pop();
            return new T();
        }

        /// <summary>
        /// 返回对象到池子
        /// </summary>
        /// <param name="obj">返回的对象</param>
        public void Return(T obj)
        {
            m_stkObjects.Push(obj);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ObjectPool()
        {
            m_stkObjects = new Stack<T>();
        }

        /// <summary>
        /// 存储所有对象的池子
        /// </summary>
        private Stack<T> m_stkObjects = null;
    }
}