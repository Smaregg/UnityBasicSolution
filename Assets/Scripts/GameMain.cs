using UnityEngine;

namespace Game
{
    /// <summary>
    /// 游戏入口
    /// </summary>
    public class GameMain : MonoBehaviour
    {
        #region 生命周期
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