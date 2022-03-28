namespace UGlue {
    using System.Text;
    using UnityEngine;

    public class LogUI
    {
        private const int MaxLength = 30 * 1024;//30kb字节缓存
        private StringBuilder m_sbAllLog = new StringBuilder();
        private bool m_bShow = false;
        private Rect m_LogArea = new Rect(500, 500, 1000, 500);
        private bool m_bLockLog = false;

        public bool Visible {
            get { return m_bShow; }
            set { m_bShow = value; }
        }

        public Rect Area {
            get { return m_LogArea; }
            set { m_LogArea = value; }
        }

        public void AddLog(Log.LogItem item) {
            switch (item.Grade) {
                case Log.LOG_GRADE.debug:
                    m_sbAllLog.AppendLine("<color=black>" + item.Head + item.Info + "</color>");
                    break;
                case Log.LOG_GRADE.info:
                    m_sbAllLog.AppendLine("<color=blue>" + item.Head + item.Info + "</color>");
                    break;
                case Log.LOG_GRADE.warning:
                    m_sbAllLog.AppendLine("<color=yellow>" + item.Head + item.Info + "</color>");
                    break;
                case Log.LOG_GRADE.error:
                    m_sbAllLog.AppendLine("<color=red>" + item.Head + item.Info + "</color>");
                    break;
                default:
                    m_sbAllLog.AppendLine("<color=pink>" + item.Head + item.Info + "</color>");
                    break;
            }
            if (m_sbAllLog.Length > MaxLength) {
                m_sbAllLog.Remove(0, MaxLength / 2);
            }            
        }

        public void Clear() {
#if NET_35
            m_sbAllLog.Remove(0, m_sbAllLog.Length);
#else 
            m_sbAllLog.Clear();
#endif
        }


        public void OnGUI() {
            if (!m_bShow | m_sbAllLog.Length <= 0) {
                return;
            }

            m_bLockLog = GUI.Toggle( 
                new Rect(m_LogArea.x, m_LogArea.y - 28, 100, 100), m_bLockLog, "<color=blue><size=18>锁定日志</size></color>");

            GUIStyle style = new GUIStyle() {
                fontSize = 24,
                richText = true,
                wordWrap = true,
                clipping = TextClipping.Clip
            };

            if (m_bLockLog) {
                style.alignment = TextAnchor.UpperLeft;
            } else {
                style.alignment = TextAnchor.LowerLeft;
            }
            GUI.TextArea(m_LogArea, m_sbAllLog.ToString(), style);
        }
    }
}


