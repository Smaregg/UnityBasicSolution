using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public delegate void LoadRscCallback(UnityEngine.Object rsc);

    public class ResourceManager
    {
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <param name="onLoadComplete">加载完成回调</param>
        public static void LoadResource<T>(string path, LoadRscCallback onLoadComplete = null) where T : UnityEngine.Object
        {
            T rsc = Resources.Load<T>(path);
            if (onLoadComplete != null)
                onLoadComplete(rsc);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="rsc">需卸载资源</param>
        public static void UnloadResource(Object rsc)
        {
            Object.Destroy(rsc);
        }
    }
}