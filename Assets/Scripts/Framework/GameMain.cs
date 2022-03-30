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
        /// AB包模式
        /// </summary>
        public bool AB_Mode = true;

        #region 生命周期
        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            GameManager.Instance.Init();
        }

        void Update()
        {
            GameManager.Instance.Update();
        }
        #endregion
    }
}