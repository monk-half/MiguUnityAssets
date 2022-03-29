namespace UGlue.Kit {
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Collections;

    /// <summary>
    /// Stack，后进先出(LIFO)。容器数据根据进入容器时机排序时优先考虑。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStack<T> : IEnumerable<T> {
        /// <summary>
        /// 堆栈长度限制，超过长度时入栈失败
        /// </summary>
        int Limit { get; set; }
        /// <summary>
        /// 获取堆栈长度
        /// </summary>
        int Count { get; }
        /// <summary>
        /// 获取栈顶元素，并出栈
        /// </summary>
        /// <returns></returns>
        T Pop();
        /// <summary>
        /// 获取栈顶元素，不出栈
        /// </summary>
        /// <returns></returns>
        T Peek();
        /// <summary>
        /// 入栈，若Limit大于0，当元素个数超过Limit将会入栈失败
        /// </summary>
        /// <param name="t"></param>
        /// <returns>false：入栈失败 true：入栈成功</returns>
        bool Push(T t);
    }

#if NET_35
    public class SafeStack<T> : UnsafeStack<T> { }
#else

    public class SafeStack<T> : IStack<T> {
        public SafeStack(int iLimit = -1) {
            Limit = iLimit;
            m_Stack = new ConcurrentStack<T>();
        }

        private ConcurrentStack<T> m_Stack;
        public int Limit { get; set; }
        public int Count => m_Stack.Count;

        public T Peek() {
            m_Stack.TryPeek(out T t);
            return t;
        }

        public T Pop() {
            m_Stack.TryPop(out T t);
            return t;
        }

        public bool Push(T t) {
            if (Limit <= 0) {
                m_Stack.Push(t);
                return true;
            }

            if (m_Stack.Count >= Limit) {
                return false;
            }
            m_Stack.Push(t);
            return true;
        }

        public IEnumerator<T> GetEnumerator() {
            return m_Stack.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return m_Stack.GetEnumerator();
        }
    }
#endif

        public class UnsafeStack<T> : IStack<T> {
        public UnsafeStack(int iLimit = -1) {
            Limit = iLimit;
            m_Stack = new Stack<T>();
        }

        private Stack<T> m_Stack;
        public int Limit { get; set; }
        public int Count => m_Stack.Count;

        public T Peek() {
            if (m_Stack.Count <= 0) {
                return default;
            }
            return m_Stack.Peek();
        }

        public T Pop() {
            if (m_Stack.Count <= 0) {
                return default;
            }
            return m_Stack.Pop();
        }

        public bool Push(T t) {
            if (Limit <= 0) {
                m_Stack.Push(t);
                return true;
            }

            if (m_Stack.Count >= Limit) {
                return false;
            }
            m_Stack.Push(t);
            return true;
        }

        public IEnumerator<T> GetEnumerator() {
            return m_Stack.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return m_Stack.GetEnumerator();
        }
    }
}
