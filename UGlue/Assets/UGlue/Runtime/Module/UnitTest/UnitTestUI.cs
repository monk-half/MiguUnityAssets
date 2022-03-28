namespace UGlue {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UGlue.Kit;

    public class UnitTestUI : MonoSingleton<UnitTestUI> {

        public override void OnSingletonInit() {
            base.OnSingletonInit();
            m_dicButtons = new Dictionary<string, UnitTestButton>();
            m_strCurrScene = SceneManager.GetActiveScene().name;
            SceneManager.activeSceneChanged += (s, d) => {
                RemoveGroup(m_strCurrScene);
                m_strCurrScene = d.name;
            };
            SetBaseButtonPos();
        }

        public static void Init() {
            Log.I(Instance == null ? "UnitTestUI Init Failed" : "UnitTestUI Init Success");
        }

        private string m_strCurrScene;
        private Dictionary<string, UnitTestButton> m_dicButtons;
        private struct UnitTestButton {
            public string m_strBeloneScene;
            public bool m_bGlobal;
            public Action m_CallBack;
        }

        private Rect m_ButtonsRect = new Rect(1750, 0, 1920, 1000);//x, y, width, height
        private Vector2 m_ButtonSize = new Vector2(100, 50);

        private void OnGUI() {
#if UNITY_EDITOR
            SetBaseButtonPos();
#endif
            GUI.skin.button.fontSize = 18;
            GUI.skin.button.wordWrap = true;
            Vector2 itemPos = new Vector2(m_ButtonsRect.x, m_ButtonsRect.y);
            foreach (var item in m_dicButtons) {
                if (GUI.Button(new Rect(itemPos.x, itemPos.y, m_ButtonSize.x, m_ButtonSize.y), item.Key)) {
                    Dispatcher.InvokeMain( () => item.Value.m_CallBack.Invoke() ); //交给调度中心，避免循环内做按钮字典的修改        
                }
                itemPos.y += m_ButtonSize.y;
                if (itemPos.y >= m_ButtonsRect.y + m_ButtonsRect.height) {
                    itemPos.y = m_ButtonsRect.y;
                    itemPos.x -= m_ButtonSize.x;
                }
            }
        }

        //TODO: 字体大小、按钮大小自适应，外部提供靠左靠右等接口设置排列关系
        private void SetBaseButtonPos() {
            m_ButtonsRect = new Rect(Screen.width - 1.2f * m_ButtonSize.x, 0, Screen.width - m_ButtonSize.x, Screen.height);
        }


        /// <summary>
        /// 增加按钮
        /// </summary>
        /// <param name="name">按钮名称</param>
        /// <param name="callBack">事件回调</param>
        /// <param name="isGlobal">是否全局按钮</param>
        /// <returns></returns>
        public static bool AddButton(string name, Action callBack, bool isGlobal = false) {
            try {
                Instance.m_dicButtons.Add(name, new UnitTestButton() {
                    m_strBeloneScene = Instance.m_strCurrScene,
                    m_bGlobal = isGlobal,
                    m_CallBack = callBack
                });
            } catch {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 移除单个按钮
        /// </summary>
        /// <param name="name">按钮名称</param>
        public static void RemoveButton(string name) {
            Instance.m_dicButtons.Remove(name);
        }

        /// <summary>
        /// 按组(场景)移除按钮
        /// </summary>
        /// <param name="scene">组别场景名，空时默认当前场景</param>
        public static void RemoveGroup(string scene = null) {
            if (scene.IsNull()) {
                scene = Instance.m_strCurrScene;
            }

            var dic = Instance.m_dicButtons;
            foreach (var item in dic) {//TODO 验证是否安全
                if (!item.Value.m_bGlobal && item.Value.m_strBeloneScene == scene) {
                    Dispatcher.InvokeAsync(() => dic.Remove(item.Key));
                }
            }
        }

        /// <summary>
        /// 移除所有按钮
        /// </summary>
        public static void RemoveAll() {
            Instance.m_dicButtons.Clear();
        }
    }
}

