using System;
using System.Collections.Generic;
using Utility;

namespace Game
{
    /// <summary>
    /// 总管理器
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        /// <summary>
        /// 所有管理器都在这里注册
        /// </summary>
        private void RegisterManagers()
        {
            
        }

        /// <summary>
        /// 注册管理器
        /// </summary>
        /// <typeparam name="T">管理器类型</typeparam>
        private void Register<T>(T instance) where T : class, ManagerInterface, new()
        {
            instance.Init();
            m_lsManagers.Add(instance);
        }

        #region 生命周期
        public void Init()
        {
            m_lsManagers = new List<ManagerInterface>();
            RegisterManagers();
        }

        public void Update()
        {
            for (int i = 0; i < m_lsManagers.Count; i++)
                m_lsManagers[i].Update();
        }

        public void Dispose()
        {
            for (int i = 0; i < m_lsManagers.Count; i++)
                m_lsManagers[i].Dispose();
            m_lsManagers.Clear();
        }
        #endregion

        /// <summary>
        /// 所有已注册的管理器
        /// </summary>
        private List<ManagerInterface> m_lsManagers = null;
    }
}