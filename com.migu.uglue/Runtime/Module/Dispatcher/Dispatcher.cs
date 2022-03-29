
namespace UGlue{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;

    public class Dispatcher : MonoSingleton<Dispatcher> {

        public override void OnSingletonInit() {
            base.OnSingletonInit();
        }

        public override void Dispose() {
            base.Dispose();
            if (asyncThread != null) {
                asyncThread.Abort();
                asyncThread = null;
            }
            asyncQueue.Clear();
            mainQueue.Clear();
        }

        public static void Init() {
            Log.I(Instance == null ? "Dispather Init Failed" : "Dispather Init Success");
        }

        //委托队列
        private Thread asyncThread = null;
        private readonly int mainFuncNum = 5; //主线程单次执行委托次数上线
        private Queue<Action> asyncQueue = new Queue<Action>();
        private Queue<Action> mainQueue = new Queue<Action>();
        private Action actOnGUI;

        private void Update() {
            if (mainQueue.Count > 0) {
                int number = mainFuncNum;
                do {
                    var func = mainQueue.Dequeue();
                    func();
                    number--;
                } while (number > 0 && mainQueue.Count > 0);
            }

            if (asyncQueue.Count > 0 && asyncThread == null) {
                asyncThread = new Thread(DoAsync) { IsBackground = true };
                asyncThread.Start();
            }
        }

        private void DoAsync() {
            if (asyncQueue.Count > 0) {
                do {
                    var func = asyncQueue.Dequeue();
                    func();
                } while (asyncQueue.Count > 0);
            }
            asyncThread = null;
        }

        public static void InvokeMain(Action action) {
            Instance.mainQueue.Enqueue(action);
        }

        public static void InvokeMain(Action action, float delaySecond) {
            Instance.StartCoroutine(Instance.AddAction(action, true, delaySecond));
        }

        public static void InvokeAsync(Action action) {
            Instance.asyncQueue.Enqueue(action);
        }

        public static void InvokeAsync(Action action, float delaySecond) {
            Instance.StartCoroutine(Instance.AddAction(action, false, delaySecond));
        }


        private IEnumerator AddAction(Action action, bool isMain, float delaySecond) {
            yield return new WaitForSeconds(delaySecond);
            if (isMain) {
                mainQueue.Enqueue(action);
            } else {
                asyncQueue.Enqueue(action);
            }
        }

        public static void RegistGUI(Action act) {
            Instance.actOnGUI += act;
        }

        public static void UnregistGUI(Action act) {
            Instance.actOnGUI -= act;
        }

        private void OnGUI() {
            actOnGUI?.Invoke();
        }
    }
}


