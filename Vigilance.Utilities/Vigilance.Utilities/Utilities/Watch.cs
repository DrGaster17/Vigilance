using System;
using System.Diagnostics;

namespace Vigilance.Utilities
{
    public class Watch
    {
        private Stopwatch _s;
        private bool _paused;

        /// <summary>
        /// Initialzes a new instance of the <see cref="Watch"/> class.
        /// </summary>
        public Watch() => _s = new Stopwatch();

        /// <summary>
        /// Starts or resumes measuring.
        /// </summary>
        public void Start() => _s.Start();

        /// <summary>
        /// Stops time measurement and resets elapsed time to zero.
        /// </summary>
        public void Reset() => _s.Reset();

        /// <summary>
        /// Stops time measurement, resets elapsed time to zero and starts measuring again.
        /// </summary>
        public void Restart() => _s.Restart();

        /// <summary>
        /// Stops measuring elapsed time.
        /// </summary>
        /// <returns>Elapsed time in miliseconds.</returns>
        public long Stop()
        {
            _s.Stop();

            long e = _s.ElapsedMilliseconds;

            _s.Reset();

            return e;
        }

        /// <summary>
        /// Paused measuring elapsed time.
        /// </summary>
        /// <returns>Elapsed time in miliseconds.</returns>
        public long Pause()
        {
            _s.Stop();
            _paused = true;
            return _s.ElapsedMilliseconds;
        }

        /// <summary>
        /// Resumes measuring elapsed time.
        /// </summary>
        public void Resume()
        {
            _paused = false;
            _s.Start();
        }

        /// <summary>
        /// Gets the total elapsed time in miliseconds.
        /// </summary>
        /// <returns>A read-only <see cref="long"/> integer representing the number of elapsed miliseconds.</returns>
        public long Miliseconds => _s.ElapsedMilliseconds;

        /// <summary>
        /// Gets the total elapsed time in timer ticks.
        /// </summary>
        /// <returns>A read-only <see cref="long"/> integer representing the number of timer ticks.</returns>
        public long Ticks => _s.ElapsedTicks;

        /// <summary>
        /// Gets a value indicating whether or not the Stopwatch is running.
        /// </summary>
        /// <returns><see cref="true"/> if the stopwatch is running, otherwise <see cref="false"/></returns>
        public bool IsRunning => !_paused && _s.IsRunning;

        /// <summary>
        /// Gets a value indicating whether or not the Stopwatch is paused.
        /// </summary>
        /// <returns><see cref="true"/> if the stopwatch is paused, otherwise <see cref="false"/></returns>
        public bool Paused => _paused;

        /// <summary>
        /// Gets the total elapsed time measured.
        /// </summary>
        /// <returns>A read-only <see cref="TimeSpan"/> representing the total elapsed time measured.</returns>
        public TimeSpan ElapsedTime => _s.Elapsed;

        public override string ToString() => $"{Miliseconds} ms";
        public override int GetHashCode() => (int)Ticks;
    }
}
