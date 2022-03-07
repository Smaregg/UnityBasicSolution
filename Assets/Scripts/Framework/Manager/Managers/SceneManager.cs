using UnityEngine.SceneManagement;

namespace Framework.Scene
{
    public class SceneManager : Manager<SceneManager>
    {
        /// <summary>
        /// 当前正加载的场景名
        /// </summary>
        public string LoadingScene { get; private set; }

        /// <summary>
        /// 加载模式
        /// Single或Addictive
        /// </summary>
        public LoadSceneMode LoadMode { get; private set; }

        /// <summary>
        /// 切换场景
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <param name="loadMode">加载模式</param>
        /// <param name="isAsync">是否异步加载</param>
        public void SwitchScene(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Additive, bool isAsync = false)
        {
            LoadingScene = sceneName;
            LoadMode = loadMode;
            if (isAsync)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Scene_Load", LoadSceneMode.Additive);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, loadMode);
            }
        }

        #region 生命周期
        public override void Init()
        {
            base.Init();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion
    }
}