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
        public void LoadGameObject(string rscName, out GameObject obj)
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
            LoadResource(rscName, rscPath, out obj);
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
        private void LoadResource<T>(string rscName, string rscPath, out T obj) where T : UnityEngine.Object
        {
            if (GameMain.Instance.AB_Mode)
            {
                BundleInstance bundleInstance = GetBundleInstance(rscPath);
                bundleInstance.GetObjInstance(rscName, out obj);
            }
            else
            {
                obj = Object.Instantiate(Resources.Load<T>(rscPath + rscName));
            }
            m_dicInstanceMap.Add(obj, rscName);
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
            if (GameMain.Instance.AB_Mode)
            {
                BundleInstance bundleInstance = GetBundleInstance(rscPath);
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
        private BundleInstance GetBundleInstance(string bundleName)
        {
            BundleInstance bundleInstance = null;
            if (m_dicBundles.ContainsKey(bundleName))
                bundleInstance = m_dicBundles[bundleName];
            else
            {
                AssetBundle ab = AssetBundle.LoadFromFile(bundleName);
                if (ab)
                {
                    bundleInstance = new BundleInstance(ab);
                    m_dicBundles.Add(bundleName, bundleInstance);
                }
            }
            return bundleInstance;
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
        public void GetObjInstance<T>(string rscName, out T obj) where T : UnityEngine.Object
        {
            ResourceInstance rscInstance = GetRscInstance<T>(rscName);
            obj = rscInstance.CreateObjInstance<T>() as T;
        }

        /// <summary>
        /// 移除资源实例
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="rscName">资源名</param>
        /// <param name="obj">实例</param>
        public void RemoveObjInstance<T>(string rscName, T obj) where T : UnityEngine.Object
        {
            ResourceInstance rscInstance = GetRscInstance<T>(rscName);
            rscInstance.DestroyObjInstance(obj);
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
        private ResourceInstance GetRscInstance<T>(string rscName) where T : UnityEngine.Object
        {
            ResourceInstance rscInstance = null;
            if (m_dicResources.ContainsKey(rscName))
                rscInstance = m_dicResources[rscName];
            else
            {
                Object rsc = m_abBundle.LoadAsset<T>(rscName);
                rscInstance = new ResourceInstance(rsc);
                m_dicResources.Add(rscName, rscInstance);
            }
            return rscInstance;
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