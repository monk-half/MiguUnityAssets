/***
 * 
 * 模块管理器(框架入口)
 * 职责：
 *      - 初始化|反初始化各个子模块
 *      - 展示框架相关的版本号、.net版本号等信息
 *      - 设置框架运行全局参数
 * 任务：
 *      - 【完成】按照参数初始化可选模块
 *      - 【完成】必选模块自动初始化
 *      - 【待做】静态模块Log初始化入口
 *      - 【完成】打印版本信息
 *      - 【待做】destroy自动卸载
 * ***/

namespace UGlue {
    public static class UGlueMgr{

        //可选模块
        public enum Module {
            Nothing = 0x0001,
            EventBus = 0x0002, //0x0004 0x0008 ...
            UnitTestUI = 0x0004,
            All = 0xFFFF
        }

        private class UGlueAbout {
            private const string  m_strVersion = "0.0.1";
            private const string  m_strDotNet = "3.5";
            public const string Hello = "Welcome to use UGlue, a tiny core to make coding easier";
            public const string Version = "UGlue Version: " + m_strVersion;
            public const string Require = "Require DotNet Version" + m_strDotNet;
        }

        public static bool Init(Module initCode = Module.Nothing) {
            //信息打印
            PrintAbout();

            //必选模块初始化
            LogMgr.Init();
            Dispatcher.Init();

            if ((initCode & Module.UnitTestUI) == Module.UnitTestUI) {
                UnitTestUI.Init();
            }

            //可选模块初始化
            if ((initCode & Module.EventBus) == Module.EventBus) {
                EventBus.Init();
            }


            //TODO 初始化失败时提示
            return true;
        }

        public static void PrintAbout() {
            Log.I(UGlueAbout.Hello);
            Log.I(UGlueAbout.Version);
            Log.I(UGlueAbout.Require);
        }
    }
}

