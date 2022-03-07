using Utility;

namespace Framework
{
    /// <summary>
    /// 所有Manager的基类
    /// </summary>
    /// <typeparam name="T">Manager类</typeparam>
    public class Manager<T> : Singleton<T>, ManagerInterface where T : class, ManagerInterface, new()
    {
        /// <summary>
        /// 初始化管理器
        /// </summary>
        public virtual void Init() { }

        /// <summary>
        /// 每帧更新控制器
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// 销毁控制器
        /// </summary>
        public virtual void Dispose() { }
    }
}