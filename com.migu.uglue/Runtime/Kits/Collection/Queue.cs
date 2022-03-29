namespace UGlue.Kit {
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Collections;

    /// <summary>
    /// Stack，先进先出(FIFO)。容器数据根据进入容器时机排序时优先考虑。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQueue<T> : IEnumerable<T> {
        /// <summary>
        /// 定长队列，填满时清除队首元素。值小于0时不限制队列长度
        /// </summary>
        int Limit { get; set; }
        /// <summary>
        /// 当前队列长度
        /// </summary>
        int Count { get;}
        /// <summary>
        /// 获取队首元素，并出队
        /// </summary>
        /// <returns></returns>
        T Dequeue(); 
        /// <summary>
        /// 获取队首元素，不出队
        /// </summary>
        /// <returns></returns>
        T Peek(); 
        /// <summary>
        /// 入队，定长队列模式下剔除队首并入队
        /// </summary>
        /// <param name="t"></param>
        /// <returns>false：失败， true：成功</returns>
        bool Enqueue(T t);
    }

#if NET_35
    public class SafeQueue<T> : UnsafeQueue<T> {
        public SafeQueue(int limit) : base(limit) { }
    }
#else
    /// <summary>
    /// 线程安全队列，用于多线程，性能比不安全队列低。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SafeQueue<T> : IQueue<T>{
        /// <summary>
        /// 创建SafeQueue
        /// </summary>
        /// <param name="iLimit">队列长度限制，默认-1不限制长度</param>
        public SafeQueue(int iLimit = -1) {
            Limit = iLimit;
            m_Queue = new ConcurrentQueue<T>();
        }

        private ConcurrentQueue<T> m_Queue;
        public int Limit { get; set; }
        public int Count => m_Queue.Count;

        public T Dequeue() {
            m_Queue.TryDequeue(out T item);
            return item;
        }

        public bool Enqueue(T t) {
            if (Limit <= 0) {
                m_Queue.Enqueue(t);
                return true;
            }

            if (m_Queue.Count >= Limit) {
                if (!m_Queue.TryDequeue(out T item)) {
                    return false;
                }
            }
            m_Queue.Enqueue(t);
            return true;
        }

        public T Peek() {
            m_Queue.TryPeek(out T item);
            return item;
        }

        public IEnumerator<T> GetEnumerator() {
            return m_Queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return m_Queue.GetEnumerator();
        }
    }
#endif
    /// <summary>
    /// 不安全队列，性能高，多线程不保证数据安全
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UnsafeQueue<T> : IQueue<T>{
        public UnsafeQueue(int iLimit = -1) {
            Limit = iLimit;
            mQueue = new Queue<T>();
        }

        private Queue<T> mQueue;
        public int Limit {get; set;}
        public int Count => mQueue.Count;

        public T Dequeue() {
            if (mQueue.Count <= 0) {
                return default;
            }
            return mQueue.Dequeue();
        }

        public bool Enqueue(T t) {
            if (Limit <= 0) {
                mQueue.Enqueue(t);
                return true;
            }

            if (mQueue.Count >= Limit) {
                mQueue.Dequeue();
            }
            mQueue.Enqueue(t);
            return true;
        }

        public T Peek() {
            if (mQueue.Count <= 0) {
                return default;
            }
            return mQueue.Peek();
        }

        public IEnumerator<T> GetEnumerator() {
            return mQueue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return mQueue.GetEnumerator();
        }
    }
}
