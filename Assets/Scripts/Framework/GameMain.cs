using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 游戏入口
    /// </summary>
    public class GameMain : MonoBehaviour
    {
        /// <summary>
        /// 游戏入口的单例
        /// </summary>
        public static GameMain Instance { get; private set; }

        /// <summary>
        /// 总管理器
        /// </summary>
        public GameManager MainManager { get { return m_mgrMain; } }

        #region 生命周期
        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            m_mgrMain = new GameManager();
            m_mgrMain.Init();
        }

        void Update()
        {
            m_mgrMain.Update();
        }
        #endregion

        /// <summary>
        /// 总管理器
        /// </summary>
        private GameManager m_mgrMain = null;
    }
}