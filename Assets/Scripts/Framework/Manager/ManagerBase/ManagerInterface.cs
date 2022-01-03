namespace Framework
{
    /// <summary>
    /// 管理器接口
    /// </summary>
    public interface ManagerInterface
    {
        /// <summary>
        /// 初始化管理器
        /// </summary>
        public void Init();

        /// <summary>
        /// 每帧更新管理器
        /// </summary>
        public void Update();

        /// <summary>
        /// 销毁管理器
        /// </summary>
        public void Dispose();
    }
}