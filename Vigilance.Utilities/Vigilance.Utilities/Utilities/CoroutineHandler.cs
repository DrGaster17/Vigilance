using System.Collections.Generic;
using System;

using MEC;

using UnityEngine;

namespace Vigilance.Utilities
{
    public class CoroutineHandler
    {
        private int indexer;
        private static readonly Dictionary<int, CoroutineHandler> _handlers = new Dictionary<int, CoroutineHandler>();

        private CoroutineHandle _handle;

        private IEnumerator<float> _cor;
        private Segment _seg = Segment.FixedUpdate;

        private bool _killOnRoundRestart;
        private bool _checkForExisting;

        private int _index;

        public CoroutineHandler(IEnumerator<float> coroutine, bool killOnRoundRestart = false, bool checkForExisting = true, Segment segment = Segment.FixedUpdate)
        {
            _cor = coroutine;
            _killOnRoundRestart = killOnRoundRestart;
            _checkForExisting = checkForExisting;
            _seg = segment;
            _index = indexer++;
            _handlers.Add(_index, this);
        }

        public static CoroutineHandler Get(IEnumerator<float> coroutine, bool killOnRoundRestart = false, bool checkForExisting = false, Segment segment = Segment.FixedUpdate) => new CoroutineHandler
            (coroutine, killOnRoundRestart, checkForExisting, segment);

        public static CoroutineHandler Start(IEnumerator<float> coroutine)
        {
            CoroutineHandler handler = Get(coroutine);
            handler.Run();
            return handler;
        }

        public static CoroutineHandler Get(int index) => _handlers.TryGetValue(index, out CoroutineHandler handler) ? handler : null;

        public void Run()
        {
            Kill();
            _handle = Timing.RunCoroutine(_cor, _seg);
        }

        public void Run(bool killOnRoundRestart)
        {
            Kill();
            _killOnRoundRestart = killOnRoundRestart;
            _handle = Timing.RunCoroutine(_cor);
        }

        public void Run(GameObject obj)
        {
            Kill();
            _handle = Timing.RunCoroutine(_cor, obj);
        }

        public void Run(int layer)
        {
            Kill();
            _handle = Timing.RunCoroutine(_cor, layer);
        }

        public void Run(Segment segment)
        {
            Kill();
            _seg = segment;
            _handle = Timing.RunCoroutine(_cor, segment);
        }

        public void Run(string tag)
        {
            Kill();
            _handle = Timing.RunCoroutine(_cor, tag);
        }

        public void Run(int layer, string tag)
        {
            Kill();
            _handle = Timing.RunCoroutine(_cor, layer, tag);
        }

        public void Run(GameObject obj, string tag)
        {
            Kill();
            _handle = Timing.RunCoroutine(_cor, obj, tag);
        }

        public void Run(Segment segment, int layer, string tag)
        {
            Kill();
            _handle = Timing.RunCoroutine(_cor, segment, layer, tag);
        }

        public void Run(Segment segment, GameObject obj, string tag)
        {
            Kill();
            _handle = Timing.RunCoroutine(_cor, segment, obj, tag);
        }

        public void Run(Segment segment, string tag)
        {
            Kill();
            _handle = Timing.RunCoroutine(_cor, segment, tag);
        }

        public void Run(Segment segment, int layer)
        {
            Kill();
            _handle = Timing.RunCoroutine(_cor, segment, layer);
        }

        public void Run(Segment segment, GameObject obj)
        {
            Kill();
            _handle = Timing.RunCoroutine(_cor, segment, obj);
        }

        public void Kill(bool force = false)
        {
            if (_handle.IsRunning && !CheckForExisting && !force)
                Timing.KillCoroutines(_handle);
        }

        public void Pause()
        {
            _handle.IsRunning = false;
        }

        public void Resume()
        {
            _handle.IsRunning = true;
        }

        public void ForceStop()
        {
            Timing.KillCoroutines(_handle);
        }

        public bool IsAliveAndPaused { get => _handle.IsAliveAndPaused; set => _handle.IsAliveAndPaused = value; }
        public bool IsRunning { get => _handle.IsRunning; set => _handle.IsRunning = value; }
        public bool KillOnRoundRestart { get => _killOnRoundRestart; set => _killOnRoundRestart = value; }
        public bool CheckForExisting { get => _checkForExisting; set => _checkForExisting = value; }

        public string Tag { get => _handle.Tag; set => _handle.Tag = value; }

        public int? Layer { get => _handle.Layer; set => _handle.Layer = value; }

        public Segment Segment { get => _handle.Segment; set => _handle.Segment = value; }

        public CoroutineHandle Handle => _handle;

        public IEnumerator<float> Enumerator { get => _cor; set => _cor = value; }

        public bool IsValid => _handle.IsValid;
        public byte Key => _handle.Key;

        public int Index => _index;

        public override string ToString() => _handle.ToString();

        public static float WaitForSeconds(float time) => Timing.WaitForSeconds(time);
        public static float WaitForSeconds(int time) => Timing.WaitForSeconds(time);

        public static float WaitUntilFalse(Func<bool> evaulator) => Timing.WaitUntilFalse(evaulator);
        public static float WaitUntilTrue(Func<bool> evaulator) => Timing.WaitUntilTrue(evaulator);

        public static float WaitForOneFrame() => Timing.WaitForOneFrame;

        public static void OnRoundRestart()
        {
            foreach (CoroutineHandler handler in _handlers.Values)
            {
                if (handler._killOnRoundRestart)
                {
                    handler.ForceStop();
                }
            }
        }
    }
}
