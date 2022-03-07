using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Framework.UI
{
    public class LoadWindow : MonoBehaviour
    {
        /// <summary>
        /// 异步加载协程
        /// </summary>
        private IEnumerator LoadScene()
        {
            m_optLoad = SceneManager.LoadSceneAsync(m_strAsyncLoadScene, m_modLoad);
            m_optLoad.completed += OnLoadComplete;
            if (m_optLoad.progress == 1)
                yield break;
            yield return 0;
        }

        /// <summary>
        /// 场景加载完成回调
        /// </summary>
        private void OnLoadComplete(AsyncOperation opt)
        {
            SceneManager.UnloadSceneAsync("Scene_Load");
        }

        // Start is called before the first frame update
        void Start()
        {
            m_strAsyncLoadScene = Scene.SceneManager.Instance.LoadingScene;
            m_modLoad = Scene.SceneManager.Instance.LoadMode;
            StartCoroutine("LoadScene");
        }

        #region 私有成员
        /// <summary>
        /// 异步加载句柄
        /// </summary>
        private AsyncOperation m_optLoad = null;

        /// <summary>
        /// 加载场景名字
        /// </summary>
        private string m_strAsyncLoadScene = string.Empty;

        /// <summary>
        /// 加载场景模式
        /// </summary>
        private LoadSceneMode m_modLoad = LoadSceneMode.Single;
        #endregion
    }
}