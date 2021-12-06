using System.Collections.Generic;

namespace Utility
{
    public class ObjectPool<T> where T : class, new()
    {
        private Stack<T> m_stkObjects = null;

        public ObjectPool()
        {
            m_stkObjects = new Stack<T>();
        }

        public T Get()
        {
            if (m_stkObjects.Count != 0)
                return m_stkObjects.Pop();
            return new T();
        }

        public void Return(T obj)
        {
            m_stkObjects.Push(obj);
        }
    }
}