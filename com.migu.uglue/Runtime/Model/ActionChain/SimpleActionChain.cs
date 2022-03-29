

namespace UGlue {
    using System;
    using System.Collections;
    using UnityEngine;
    using UGlue.Kit;

    public interface IActionChain {
        /// <summary>
        /// 延迟second秒之后执行act事件，act为空时只执行延迟。
        /// </summary>
        /// <param name="second">延迟时间</param>
        /// <param name="act">延迟后执行事件</param>
        /// <returns></returns>
        IActionChain Delay(float second, Action act = null);

        /// <summary>
        /// 每隔second秒循环执行func委托，直到func返回值为true时结束
        /// </summary>
        /// <param name="second">单位秒</param>
        /// <param name="func">循环执行的事件</param>
        /// <returns></returns>
        IActionChain Loop(float second, Func<bool> func);

        /// <summary>
        /// 每帧循环执行func，直到func返回true
        /// </summary>
        /// <param name="func">循环执行的事件</param>
        /// <returns></returns>
        IActionChain LoopFrame(Func<bool> func);

        /// <summary>
        /// 立即执行act
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        IActionChain Do(Action act);

        /// <summary>
        /// 立即执行协程
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        IActionChain Do(IEnumerator enumerator);

        /// <summary>
        /// 开始执行节点
        /// </summary>
        void Begin();
    }

    /// <summary>
    /// ActionChain 静态扩展
    /// </summary>
    public static class ActionChainExterntion {
        public static ActionChain ActionChain<T>(this T selfbehaviour) where T : MonoBehaviour {
            return new ActionChain(selfbehaviour);
        }
    }

    /// <summary>
    /// 简单消息节点，目前只支持主线程创建，主线程执行。
    /// </summary>
    public class ActionChain : IActionChain {

        private readonly MonoBehaviour m_Behaviour;

        //考虑可能主线程创建chain，子线程
        private UnsafeQueue<IEnumerator> m_queActions = new UnsafeQueue<IEnumerator>();

        public ActionChain(MonoBehaviour behaviour) {
            Log.I("创建ActionChain");
            m_Behaviour = behaviour;
        }


        private IEnumerator DelayNode(float second, Action act = null) {
            yield return new WaitForSeconds(second);
            act?.Invoke();
        }

        private IEnumerator LoopNode(float period, Func<bool> func) {
            while (!func()) {
                yield return new WaitForSeconds(period);
            }
        }

        private IEnumerator LoopFrameNode(Func<bool> func) {
            while (!func()) {
                yield return new WaitForEndOfFrame();
            }
        }

        // 直接执行，TODO：执行完默认等待一帧，待优化
        private IEnumerator DoNode(Action act) {
            act.Invoke();
            yield return null;
        }

        // 循环执行代码块(Ienumerator)
        private IEnumerator BeginNode() {
            while (m_queActions.Count > 0) {
                yield return m_Behaviour?.StartCoroutine(m_queActions.Dequeue());
            }
        }


        public void Begin() {
            Log.I("开始执行ActionChain");
            m_Behaviour?.StartCoroutine(BeginNode());
        }

        public IActionChain Delay(float second, Action act = null) {
            m_queActions.Enqueue(DelayNode(second, act));
            return this;
        }

        public IActionChain Do(Action act) {
            m_queActions.Enqueue(DoNode(act));
            return this;
        }

        public IActionChain Do(IEnumerator enumerator) {
            m_queActions.Enqueue(enumerator);
            return this;
        }

        public IActionChain Loop(float second, Func<bool> func) {
            m_queActions.Enqueue(LoopNode(second, func));
            return this;
        }

        public IActionChain LoopFrame(Func<bool> func) {
            m_queActions.Enqueue(LoopFrameNode(func));
            return this;
        }
    }
}
