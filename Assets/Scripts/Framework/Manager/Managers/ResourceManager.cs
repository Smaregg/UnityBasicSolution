using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class ResourceManager : Manager<ResourceManager>
    {
        public static string MODEL_PATH = "Models/";
        public static string SCENE_PATH = "Scenes/";
        public static string TEXTURE_PATH = "Textures/";

        public static string ROOT_PATH_AB = Application.dataPath + "/AssetBundles/";
        public static string MODEL_PATH_AB = "Models/";

        /// <summary>
        /// 获取GameObject资源实例
        /// </summary>
        /// <param name="rscName">资源名</param>
        /// <param name="obj">实例</param>
        public void LoadGameObject(string rscName, System.Action<GameObject> callback, bool async = true)
        {
            string rscPath = string.Empty;
            if (GameMain.Instance.AB_Mode)
            {
                rscPath = ROOT_PATH_AB + MODEL_PATH_AB;
            }
            else
            {
                rscPath = MODEL_PATH;
            }
            LoadResource<GameObject>(rscName, rscPath, (obj) =>
            {
                if (callback != null)
                    callback(obj);
            }, async);
        }

        /// <summary>
        /// 销毁GameObject资源实例
        /// </summary>
        /// <param name="obj">销毁实例</param>
        public void UnloadGameObject(ref GameObject obj)
        {
            string rscPath = string.Empty;
            if (GameMain.Instance.AB_Mode)
            {
                rscPath = ROOT_PATH_AB + MODEL_PATH_AB;
            }
            else
            {
                rscPath = MODEL_PATH;
            }
            if (m_dicInstanceMap.ContainsKey(obj))
            {
                string rscName = m_dicInstanceMap[obj];
                UnloadResource(rscName, rscPath, obj);
            }
            else
            {
                Debug.LogWarning("[ResourceManager] : 实例丢失引用！");
                Object.Destroy(obj);
            }
            obj = null;
        }

        /// <summary>
        /// 加载资源并获取资源实例
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="rscName">资源名</param>
        /// <param name="rscPath">资源路径</param>
        /// <param name="obj">实例</param>
        private void LoadResource<T>(string rscName, string rscPath, System.Action<T> callback, bool async = true) where T : UnityEngine.Object
        {
            if (GameMain.Instance.AB_Mode)
            {
                // 获取AB包实例
                GetBundleInstance(rscPath, (bundleInstance) =>
                {
                    // 获取资源实例
                    bundleInstance.GetObjInstance<T>(rscName, (obj) =>
                    {
                        m_dicInstanceMap.Add(obj, rscName);
                        if (callback != null)
                        {
                            callback(obj);
                        }
                    }, async);
                }, async);
            }
            else
            {
                T obj = Object.Instantiate(Resources.Load<T>(rscPath + rscName));
                m_dicInstanceMap.Add(obj, rscName);
                if (callback != null)
                {
                    callback(obj);
                }
            }
        }

        /// <summary>
        /// 销毁实例并移除引用
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="rscName">资源名</param>
        /// <param name="rscPath">资源路径</param>
        /// <param name="obj">实例</param>
        private void UnloadResource<T>(string rscName, string rscPath, T obj) where T : UnityEngine.Object
        {
            m_dicInstanceMap.Remove(obj);
            if (GameMain.Instance.AB_Mode && m_dicBundles.ContainsKey(rscPath))
            {
                BundleInstance bundleInstance = m_dicBundles[rscPath];
                bundleInstance.RemoveObjInstance(rscName, obj);
            }
            else
            {
                Object.Destroy(obj);
                Resources.UnloadUnusedAssets();
            }
        }

        /// <summary>
        /// 获取AB包实例
        /// </summary>
        /// <param name="bundleName">AB包名</param>
        /// <returns>AB包实例</returns>
        private void GetBundleInstance(string bundleName, System.Action<BundleInstance> callback, bool async = true)
        {
            BundleInstance bundleInstance = null;
            // 已加载AB包
            if (m_dicBundles.ContainsKey(bundleName))
            {
                bundleInstance = m_dicBundles[bundleName];
                if (callback != null)
                    callback(bundleInstance);
            }
            // 未加载AB包
            else
            {
                // 异步加载AB包
                if (async)
                {
                    GameMain.Instance.StartCoroutine(LoadAssetBundleAsync(bundleName, (opt) =>
                    {
                        AssetBundleCreateRequest loadOpt = opt as AssetBundleCreateRequest;
                        if (loadOpt == null || loadOpt.assetBundle == null)
                        {
                            Debug.LogError("[ResourceManager] : " + bundleName + "AB包加载失败");
                            return;
                        }

                        bundleInstance = new BundleInstance(loadOpt.assetBundle);
                        m_dicBundles.Add(bundleName, bundleInstance);
                        if (callback != null)
                            callback(bundleInstance);
                    }));
                }
                // 同步加载AB包
                else
                {
                    AssetBundle ab = AssetBundle.LoadFromFile(bundleName);
                    if (ab)
                    {
                        bundleInstance = new BundleInstance(ab);
                        m_dicBundles.Add(bundleName, bundleInstance);
                        if (callback != null)
                            callback(bundleInstance);
                    }
                }
            }
        }

        /// <summary>
        /// 加载AB包协程
        /// </summary>
        /// <param name="bundleName">AB包路径</param>
        /// <param name="callback">协程完成回调</param>
        private IEnumerator LoadAssetBundleAsync(string bundleName, System.Action<AsyncOperation> callback)
        {
            AssetBundleCreateRequest opt = AssetBundle.LoadFromFileAsync(bundleName);
            opt.completed += callback;
            yield return 0;
        }

        #region 生命周期
        public ResourceManager()
        {
            m_dicBundles = new Dictionary<string, BundleInstance>();
            m_dicInstanceMap = new Dictionary<Object, string>();
        }

        public override void Update()
        {
            base.Update();
            if (GameMain.Instance.AB_Mode)
            {
                List<string> lsWaitForUnload = new List<string>();
                foreach (var bundle in m_dicBundles)
                {
                    bundle.Value.DestroyUnusedResource();
                    if (bundle.Value.CanDestroy)
                    {
                        bundle.Value.UnloadBundle();
                        lsWaitForUnload.Add(bundle.Key);
                    }
                }
                for (int i = 0; i < lsWaitForUnload.Count; i++)
                {
                    m_dicBundles.Remove(lsWaitForUnload[i]);
                }
            }
            else
            {
                Resources.UnloadUnusedAssets();
            }
        }

        public override void Dispose()
        {
            if (GameMain.Instance.AB_Mode)
            {
                foreach (var bundle in m_dicBundles)
                {
                    bundle.Value.UnloadBundle();
                }
                m_dicBundles.Clear();
            }
            else
            {
                Resources.UnloadUnusedAssets();
            }
            base.Dispose();
        }
        #endregion

        /// <summary>
        /// AB包实例
        /// </summary>
        private Dictionary<string, BundleInstance> m_dicBundles = null;

        /// <summary>
        /// 实例-资源名映射
        /// </summary>
        private Dictionary<Object, string> m_dicInstanceMap = null;
    }

    public class BundleInstance
    {
        /// <summary>
        /// 该AB包是否需要卸载
        /// </summary>
        public bool CanDestroy { get { return m_dicResources.Count == 0; } }

        /// <summary>
        /// 获取资源实例
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="rscName">资源名</param>
        /// <param name="obj">实例</param>
        public void GetObjInstance<T>(string rscName, System.Action<T> callback, bool async = true) where T : UnityEngine.Object
        {
            // 获取资源
            GetRscInstance<T>(rscName, (rscInstance) =>
            {
                if (rscInstance == null)
                    return;

                // 获取资源实例
                T obj = rscInstance.CreateObjInstance<T>() as T;
                if (callback != null)
                    callback(obj);
            }, async);
        }

        /// <summary>
        /// 移除资源实例
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="rscName">资源名</param>
        /// <param name="obj">实例</param>
        public void RemoveObjInstance<T>(string rscName, T obj) where T : UnityEngine.Object
        {
            if (m_dicResources.ContainsKey(rscName))
            {
                ResourceInstance rscInstance = m_dicResources[rscName];
                rscInstance.DestroyObjInstance(obj);
            }
            else
            {
                Object.Destroy(obj);
            }
        }

        /// <summary>
        /// 销毁未使用的资源
        /// </summary>
        public void DestroyUnusedResource()
        {
            List<string> lsWaitForDestroy = new List<string>();
            foreach (var resource in m_dicResources)
            {
                if (resource.Value.CanDestroy)
                {
                    lsWaitForDestroy.Add(resource.Key);
                }
            }
            for (int i = 0; i < lsWaitForDestroy.Count; i++)
            {
                m_dicResources.Remove(lsWaitForDestroy[i]);
            }
        }

        /// <summary>
        /// 卸载AB包
        /// </summary>
        public void UnloadBundle()
        {
            m_dicResources.Clear();
            m_abBundle.Unload(true);
        }

        /// <summary>
        /// 获取资源实例
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="rscName">资源名</param>
        /// <returns>资源实例</returns>
        private void GetRscInstance<T>(string rscName, System.Action<ResourceInstance> callback, bool async = true) where T : UnityEngine.Object
        {
            ResourceInstance rscInstance = null;
            // 已加载过
            if (m_dicResources.ContainsKey(rscName))
            {
                rscInstance = m_dicResources[rscName];
                if (callback != null)
                    callback(rscInstance);
            }
            // 未加载过
            else
            {
                // 异步加载
                if (async)
                {
                    GameMain.Instance.StartCoroutine(LoadResourceAsync(rscName, (opt) =>
                    {
                        AssetBundleRequest loadOpt = opt as AssetBundleRequest;
                        if (loadOpt == null || loadOpt.asset == null)
                        {
                            Debug.LogError("[ResourceManager] : " + rscName + "资源加载失败");
                            return;
                        }

                        rscInstance = new ResourceInstance(loadOpt.asset);
                        m_dicResources.Add(rscName, rscInstance);
                        if (callback != null)
                            callback(rscInstance);
                    }));
                }
                // 同步加载
                else
                {
                    Object rsc = m_abBundle.LoadAsset<T>(rscName);
                    rscInstance = new ResourceInstance(rsc);
                    m_dicResources.Add(rscName, rscInstance);
                    if (callback != null)
                        callback(rscInstance);
                }
            }
        }

        /// <summary>
        /// 资源加载协程
        /// </summary>
        /// <param name="rscName">资源名</param>
        /// <param name="callback">协程完成回调</param>
        private IEnumerator LoadResourceAsync(string rscName, System.Action<AsyncOperation> callback)
        {
            AssetBundleRequest opt = m_abBundle.LoadAssetAsync(rscName);
            opt.completed += callback;
            yield return 0;
        }

        public BundleInstance(AssetBundle ab)
        {
            m_abBundle = ab;
            m_dicResources = new Dictionary<string, ResourceInstance>();
        }

        /// <summary>
        /// 该实例使用的AB包
        /// </summary>
        private AssetBundle m_abBundle = null;

        /// <summary>
        /// 该AB包实例化的资源索引
        /// </summary>
        private Dictionary<string, ResourceInstance> m_dicResources = null;
    }

    public class ResourceInstance
    {
        /// <summary>
        /// 该资源是否需要卸载
        /// </summary>
        public bool CanDestroy { get { return m_lsInstances.Count == 0; } }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns>实例</returns>
        public Object CreateObjInstance<T>() where T : UnityEngine.Object
        {
            Object instance = Object.Instantiate<T>(m_objResource as T);
            if (!m_lsInstances.Contains(instance))
                m_lsInstances.Add(instance);
            return instance;
        }

        /// <summary>
        /// 销毁实例
        /// </summary>
        /// <param name="instance">销毁实例</param>
        public void DestroyObjInstance(Object instance)
        {
            if (m_lsInstances.Contains(instance))
                m_lsInstances.Remove(instance);
            Object.Destroy(instance);
        }

        public ResourceInstance(Object obj)
        {
            m_objResource = obj;
            m_lsInstances = new List<Object>();
        }

        /// <summary>
        /// 该实例使用的资源
        /// </summary>
        private Object m_objResource = null;

        /// <summary>
        /// 该资源的所有实例化引用
        /// </summary>
        private List<Object> m_lsInstances = null;
    }
}