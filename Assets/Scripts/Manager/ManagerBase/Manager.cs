using Utility;

namespace Game
{
    /// <summary>
    /// 所有Manager的基类
    /// </summary>
    /// <typeparam name="T">Manager类</typeparam>
    public class Manager<T> : Singleton<T>, ManagerInterface where T : class, ManagerInterface, new()
    {
        public virtual void Init() { }

        public virtual void Update() { }

        public virtual void Dispose() { }
    }
}