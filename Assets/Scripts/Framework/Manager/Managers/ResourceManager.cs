using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class ResourceManager : Manager<ResourceManager>
    {
        public static string PREFAB_PATH = "Podels/";
        public static string MODEL_PATH = "Models/";
        public static string SCENE_PATH = "Scenes/";
        public static string TEXTURE_PATH = "Textures/";

        public static string ROOT_PATH_AB = Application.dataPath + "/AssetBundles/";
        public static string MAIN_BUNDLE_PATH = "assetbundles";
        public static string PREFAB_PATH_AB = "prefab";
        public static string MODEL_PATH_AB = "model";

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
                rscPath = PREFAB_PATH_AB;
            }
            else
            {
                rscPath = PREFAB_PATH;
            }
            LoadResource<GameObject>(rscName, rscPath, (obj) =>
            {
                m_isLoading = false;
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
                rscPath = PREFAB_PATH_AB;
            }
            else
            {
                rscPath = PREFAB_PATH;
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
            m_isLoading = true;
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
                Debug.Log("[ResourceManager] : 加载AB包 :" + bundleName);
                // 异步加载AB包
                if (async)
                {
                    GameMain.Instance.StartCoroutine(LoadAssetBundleAsync(ROOT_PATH_AB + bundleName, (opt) =>
                    {
                        AssetBundleCreateRequest loadOpt = opt as AssetBundleCreateRequest;
                        if (loadOpt == null || loadOpt.assetBundle == null)
                        {
                            Debug.LogError("[ResourceManager] : " + bundleName + "AB包加载失败");
                            return;
                        }

                        // 加载依赖包
                        string[] bundleDependenses = m_bundleManifest.GetAllDependencies(bundleName);

                        // 没有依赖
                        if (bundleDependenses.Length == 0)
                        {
                            bundleInstance = new BundleInstance(loadOpt.assetBundle, null);
                            m_dicBundles.Add(bundleName, bundleInstance);
                            if (callback != null)
                                callback(bundleInstance);
                        }
                        // 有依赖，递归地加载
                        else
                        {
                            LoadDependeces(bundleDependenses, (dependences) =>
                            {
                                bundleInstance = new BundleInstance(loadOpt.assetBundle, dependences);
                                m_dicBundles.Add(bundleName, bundleInstance);
                                if (callback != null)
                                    callback(bundleInstance);
                            }, true);
                        }
                    }));
                }
                // 同步加载AB包
                else
                {
                    AssetBundle ab = AssetBundle.LoadFromFile(bundleName);
                    if (ab)
                    {
                        // 加载依赖包
                        string[] bundleDependenses = m_bundleManifest.GetAllDependencies(bundleName);

                        // 没有依赖
                        if (bundleDependenses.Length == 0)
                        {
                            bundleInstance = new BundleInstance(ab, null);
                            m_dicBundles.Add(bundleName, bundleInstance);
                            if (callback != null)
                                callback(bundleInstance);
                        }
                        // 有依赖，递归地加载
                        else
                        {
                            LoadDependeces(bundleDependenses, (dependences) =>
                            {
                                bundleInstance = new BundleInstance(ab, dependences);
                                m_dicBundles.Add(bundleName, bundleInstance);
                                if (callback != null)
                                    callback(bundleInstance);
                            }, false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 递归加载AB包依赖包
        /// </summary>
        /// <param name="bundleDependenses">所有依赖包</param>
        /// <param name="async">是否异步</param>
        /// <param name="callback">依赖包加载完成回调</param>
        /// <param name="index">当前加载依赖包索引</param>
        private void LoadDependeces(string[] bundleDependenses, System.Action<List<BundleInstance>> callback, bool async, int index = 0)
        {
            List<BundleInstance> dependences = new List<BundleInstance>();
            GetBundleInstance(bundleDependenses[index], (dependence) =>
            {
                // 跳出递归，添加被依赖项
                dependence.AddOneDependent();
                dependences.Add(dependence);
                if (++index >= bundleDependenses.Length)
                {
                    callback(dependences);
                    return;
                }
                LoadDependeces(bundleDependenses, callback, async, index);
            }, async);
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

        public override void Init()
        {
            base.Init();
            m_mainBundle = AssetBundle.LoadFromFile(ROOT_PATH_AB + MAIN_BUNDLE_PATH);
            m_bundleManifest = m_mainBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        public override void Update()
        {
            base.Update();
            if (m_isLoading)
                return;
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

        private AssetBundle m_mainBundle = null;

        private AssetBundleManifest m_bundleManifest = null;

        /// <summary>
        /// AB包实例
        /// </summary>
        private Dictionary<string, BundleInstance> m_dicBundles = null;

        /// <summary>
        /// 实例-资源名映射
        /// </summary>
        private Dictionary<Object, string> m_dicInstanceMap = null;

        /// <summary>
        /// 是否正在加载
        /// </summary>
        private bool m_isLoading = false;
    }

    public class BundleInstance
    {
        /// <summary>
        /// 该AB包是否需要卸载
        /// 当且仅当该AB包没有加载资源且不被其他任何AB包依赖时可卸载
        /// </summary>
        public bool CanDestroy { get { return m_dicResources.Count == 0 && m_beDependetNum == 0; } }

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
            // 删除依赖包引用
            for (int i = 0; i < m_dependences.Count; i++)
            {
                m_dependences[i].RemoveOneDependent();
            }
            m_dependences.Clear();
            m_dicResources.Clear();
            m_abBundle.Unload(true);
        }

        /// <summary>
        /// 添加一个被依赖项
        /// </summary>
        public void AddOneDependent()
        {
            ++m_beDependetNum;
        }

        /// <summary>
        /// 移除一个被依赖项
        /// </summary>
        public void RemoveOneDependent()
        {
            if (--m_beDependetNum < 0)
            {
                Debug.LogError("[ResourceManager] : 错误删除AB包依赖");
                m_beDependetNum = 0;
            }
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

        public BundleInstance(AssetBundle ab, List<BundleInstance> bundleInstances)
        {
            m_dicResources = new Dictionary<string, ResourceInstance>();
            m_abBundle = ab;
            m_dependences = bundleInstances;
            if (m_dependences == null)
                m_dependences = new List<BundleInstance>();
            m_beDependetNum = 0;
        }

        /// <summary>
        /// 该实例使用的AB包
        /// </summary>
        private AssetBundle m_abBundle = null;

        /// <summary>
        /// 该AB包实例化的资源索引
        /// </summary>
        private Dictionary<string, ResourceInstance> m_dicResources = null;

        /// <summary>
        /// 所有依赖包
        /// </summary>
        private List<BundleInstance> m_dependences = null;

        /// <summary>
        /// 被依赖的包个数
        /// </summary>
        private int m_beDependetNum = 0;
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