namespace UGlue {
    /**
     * TODO: 1、接入网络模块、文件模块
     */
    using UGlue.Kit;

    public class LogMgr : MonoSingleton<LogMgr> {
        private LogMgr() { }

        public override void OnSingletonInit() {
            base.OnSingletonInit();
            m_LogUI = new LogUI();
            Log.Subscribe(OnLog);
        }

        public override void Dispose() {
            base.Dispose();
            m_LogUI = null;
            Log.UnSubscribe(OnLog);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init() {
            Log.I(Instance == null ? "LogMgr Init Failed" : "LogMgr Init Success");
        }



        #region Log管理
        /// <summary>
        /// 设置日志运行模式
        /// </summary>
        public static Log.MODE Mode {
            get { return Log.Mode; }
            set { Log.Mode = value;}
        }

        /// <summary>
        /// 接收日志消息
        /// </summary>
        /// <param name="item"></param>
        private void OnLog(Log.LogItem item) {
            m_LogUI?.AddLog(item);
            m_LogRemote?.SendMsg(item);
        }
        #endregion


        #region LogUI管理
        /// <summary>
        /// 获取LogUI实例
        /// </summary>
        private LogUI m_LogUI;
        public static LogUI UI {
            get {
                return Instance.m_LogUI;
            }
        }
        /// <summary>
        /// Log绘制
        /// </summary>
        private void OnGUI() {
            m_LogUI?.OnGUI();
        }
        #endregion

        #region LogRemote管理
        private LogRemote m_LogRemote;
        /// <summary>
        /// 开启远程日志。默认作为发送端
        /// </summary>
        /// <param name="port"></param>
        /// <param name="mode"></param>
        public static void EnableRemoteLog(LogRemote.LOG_PORT port, LogRemote.MODE mode = LogRemote.MODE.Send) {
            if (Instance.m_LogRemote.IsNull()) {
                Instance.m_LogRemote = new LogRemote(port, mode);
            }
        }

        public static void DisableRemoteLog() {
            if (Instance.m_LogRemote.IsNull()) {
                return;
            }
            Instance.m_LogRemote.Mode = LogRemote.MODE.None;
        }
        #endregion
    }
}

