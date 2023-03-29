using UnityEngine;

namespace Assets
{
    public class Timer
    {
        private float SecondsToWait;
        private float LastTime;

        private Timer(float SecondsToWait)
        {
            this.SecondsToWait = SecondsToWait;
            this.LastTime = float.NegativeInfinity;
        }

        /// <summary>
        /// Starts this timer. Essentially making it so it returns false on Elapsed until SecondsToWait time has passed.
        /// </summary>
        public void Start()
        {
            this.LastTime = Time.time;
        }

        /// <summary>
        /// Returns true if the amount of time since this timer was started is bigger or equal to the wait time of this timer.
        /// Returns false otherwise.
        /// </summary>
        public bool Elapsed()
        {
            return Time.time >= this.LastTime + this.SecondsToWait;
        }

        /// <summary>
        /// Creates a timer from an amount of seconds.
        /// </summary>
        public static Timer FromSeconds(float Seconds)
        {
            return new Timer(Seconds);
        }

        /// <summary>
        /// Creates a timer from an amount of milliseconds.
        /// </summary>
        public static Timer FromMillis(uint Millis)
        {
            return new Timer(Millis / 1000f);
        }
    }
}
