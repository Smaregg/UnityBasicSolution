namespace Utility
{
    /// <summary>
    /// 单例
    /// </summary>
    /// <typeparam name="T">有默认构造函数的类</typeparam>
    public class Singleton<T> where T : class, new()
    {
        /// <summary>
        /// 单例接口
        /// </summary>
        public static T Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new T();
                return m_instance;
            }
        }

        /// <summary>
        /// 单例
        /// </summary>
        private static T m_instance;
    }
}