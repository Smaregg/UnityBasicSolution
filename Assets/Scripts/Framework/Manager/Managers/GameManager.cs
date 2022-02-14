using System;
using System.Collections.Generic;
using Utility;

namespace Framework
{
    /// <summary>
    /// 总管理器
    /// </summary>
    public class GameManager : Manager<GameManager>
    {
        /// <summary>
        /// 注册所有管理器
        /// </summary>
        private void RegisterManagers()
        {
            m_lsManagers = new List<ManagerInterface>();

            // ********************系统管理器********************
            Register(ResourceManager.Instance);

            // ********************自定义管理器********************
        }

        /// <summary>
        /// 注销所有管理器
        /// </summary>
        private void UnregisterManagers()
        {
            for (int i = 0; i < m_lsManagers.Count; i++)
                m_lsManagers[i].Dispose();
            m_lsManagers.Clear();
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
        public override void Init()
        {
            RegisterManagers();
        }

        public override void Update()
        {
            for (int i = 0; i < m_lsManagers.Count; i++)
                m_lsManagers[i].Update();
        }

        public override void Dispose()
        {
            UnregisterManagers();
        }
        #endregion

        /// <summary>
        /// 所有已注册的管理器
        /// </summary>
        private List<ManagerInterface> m_lsManagers = null;
    }
}