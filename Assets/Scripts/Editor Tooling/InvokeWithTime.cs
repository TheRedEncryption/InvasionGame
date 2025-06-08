using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonRaider
{
    internal class InvokeWithTime
    {
        /// <summary>
        /// Delegate called when <see cref="_currTime"/> >= <see cref="_time"/>
        /// </summary>
        public delegate void TimesUp();

        /// <summary>
        /// Delegate called everytime <see cref="_currTime"/> is increased.
        /// </summary>
        /// <param name="timeLeftPercent">How complete is the timer?</param>
        public delegate void TimeIterate(double timeLeftPercent);

        /// <summary>
        /// The behavior of a timer when it's timer ends.
        /// </summary>
        public enum PersistanceMode
        {
            /// <summary>
            /// Safe, removes instances when timer is finished
            /// </summary>
            NotPersistant = 0b_00,
            /// <summary>
            /// Unsafe, doesn't remove the instance when the timer is finished.
            /// </summary>
            Persistant = 0b_01,
            /// <summary>
            /// Unsafe, niether removes instance, nor does the action automatically stop when timer is ended.
            /// </summary>
            Looping = 0b_10
        }

        private static List<InvokeWithTime> instances = new(1);

        private readonly TimesUp _timesUp;
        private readonly TimesUp _endEarly;
        private readonly TimeIterate _timeIterate;

        // Timer values
        private double _time;
        private double _currTime;

        // Intance properties (English, not C#, definition)
        private bool _requireManualPause; // Can this timer be paused with methods, or does it need to be set with IsPaused.
        private PersistanceMode _persistance; // Persistance mode

        // Get properties (C# def this time)
        public double TimeLeft { get => _time - _currTime; }
        public bool RequireManualPause { get => _requireManualPause; }
        public PersistanceMode Persistance { get => _persistance; }

        // -- Delegate calls --

        /// <summary>
        /// The delegate that will be called when the timer is finished.
        /// </summary>
        public TimesUp EndCall { get => _timesUp; }

        /// <summary>
        /// The delegate that will be called when the timer is stopped early.
        /// </summary>
        public TimesUp EarlyCall { get => _endEarly; }

        /// <summary>
        /// The delegate that will be called every frame
        /// </summary>
        public TimeIterate IterateCall { get => _timeIterate; }

        /// <summary>
        /// How close to done is this timer
        /// </summary>
        public double PercentDone { get => Math.Clamp(_currTime / _time, 0.0, 1.0); }

        /// <summary>
        /// The Number of timers stored in the list.
        /// </summary>
        public static int NumInstances { get => instances.Count; }

        /// <summary>
        /// Does this timer remain after it gets removed
        /// </summary>
        public bool IsPersistant { get => _persistance != 0; }

        /// <summary>
        /// Is the timer counting down
        /// </summary>
        public bool IsPaused { get; set; }

        #region Constructors

        /// <summary>
        /// Creates a timer
        /// </summary>
        /// <param name="timeSeconds">The amount of time the timer ticks down</param>
        /// <param name="callEveryTick">A method to call every frame VIA <seealso cref="TimeIterate"/></param>
        /// <param name="earlyEndCall">A method to call should the timer be stopped before it's timer is finished</param>
        /// <param name="callAtEnd">The method to call when the timer is finished</param>
        /// <param name="persistanceMode">The behavior of this timer when it's timer is finished; See: <seealso cref="PersistanceMode"/></param>
        /// <param name="requireManualPause">Does this timer get paused when <see cref="GlobalPause"/> or other general pausing methods are called? Or does this timer need an explicit call to pause it</param>
        public InvokeWithTime(double timeSeconds,
            TimeIterate callEveryTick, TimesUp earlyEndCall, TimesUp callAtEnd,
            PersistanceMode persistanceMode, bool requireManualPause)
        {
            _time = timeSeconds;
            _currTime = 0;

            _persistance = persistanceMode;
            _requireManualPause = requireManualPause;

            _timeIterate = callEveryTick;
            _timesUp = callAtEnd;
            _endEarly = earlyEndCall;

            instances.Add(this);
        }

        /// <summary>
        /// Creates a timer
        /// </summary>
        /// <param name="timeSeconds">The amount of time the timer ticks down</param>
        /// <param name="callEveryTick">A method to call every frame VIA <seealso cref="TimeIterate"/></param>
        /// <param name="earlyEndCall">A method to call should the timer be stopped before it's timer is finished</param>
        /// <param name="callAtEnd">The method to call when the timer is finished</param>
        public InvokeWithTime(double timeSeconds,
            TimeIterate callEveryTick, TimesUp earlyEndCall, TimesUp callAtEnd)
            : this(timeSeconds, callEveryTick, callAtEnd, callAtEnd, PersistanceMode.NotPersistant, false)
        { }

        /// <summary>
        /// Creates a timer
        /// </summary>
        /// <param name="timeSeconds">The amount of time the timer ticks down</param>
        /// <param name="callEveryTick">A method to call every frame VIA <seealso cref="TimeIterate"/></param>
        /// <param name="callAtEnd">The method to call when the timer is finished</param>
        /// <param name="persistanceMode">The behavior of this timer when it's timer is finished; See: <seealso cref="PersistanceMode"/></param>
        /// <param name="requireManualPause">Does this timer get paused when <see cref="GlobalPause"/> or other general pausing methods are called? Or does this timer need an explicit call to pause it</param>
        public InvokeWithTime(double timeSeconds, TimeIterate callEveryTick, TimesUp callAtEnd,
            PersistanceMode persistanceMode, bool requireManualPause)
            : this(timeSeconds, callEveryTick, callAtEnd, callAtEnd, persistanceMode, requireManualPause)
        { }

        /// <summary>
        /// Creates a timer
        /// </summary>
        /// <param name="timeSeconds">The amount of time the timer ticks down</param>
        /// <param name="callEveryTick">A method to call every frame VIA <seealso cref="TimeIterate"/></param>
        /// <param name="persistanceMode">The behavior of this timer when it's timer is finished; See: <seealso cref="PersistanceMode"/></param>
        /// <param name="requireManualPause">Does this timer get paused when <see cref="GlobalPause"/> or other general pausing methods are called? Or does this timer need an explicit call to pause it</param>
        public InvokeWithTime(double timeSeconds, TimeIterate callEveryTick,
            PersistanceMode persistanceMode, bool requireManualPause)
            : this(timeSeconds, callEveryTick, null, null, persistanceMode, requireManualPause)
        { }

        /// <summary>
        /// Creates a timer
        /// </summary>
        /// <param name="timeSeconds">The amount of time the timer ticks down</param>
        /// <param name="callAtEnd">The method to call when the timer is finished</param>
        /// <param name="persistanceMode">The behavior of this timer when it's timer is finished; See: <seealso cref="PersistanceMode"/></param>
        /// <param name="requireManualPause">Does this timer get paused when <see cref="GlobalPause"/> or other general pausing methods are called? Or does this timer need an explicit call to pause it</param>
        public InvokeWithTime(double timeSeconds, TimesUp callAtEnd,
            PersistanceMode persistanceMode, bool requireManualPause)
            : this(timeSeconds, null, callAtEnd, callAtEnd, persistanceMode, requireManualPause)
        { }

        /// <summary>
        /// Creates a timer
        /// </summary>
        /// <param name="timeSeconds">The amount of time the timer ticks down</param>
        /// <param name="earlyEndCall">A method to call should the timer be stopped before it's timer is finished</param>
        /// <param name="callAtEnd">The method to call when the timer is finished</param>
        /// <param name="persistanceMode">The behavior of this timer when it's timer is finished; See: <seealso cref="PersistanceMode"/></param>
        /// <param name="requireManualPause">Does this timer get paused when <see cref="GlobalPause"/> or other general pausing methods are called? Or does this timer need an explicit call to pause it</param>
        public InvokeWithTime(double timeSeconds, TimesUp earlyEndCall, TimesUp callAtEnd,
            PersistanceMode persistanceMode, bool requireManualPause)
            : this(timeSeconds, null, earlyEndCall, callAtEnd, persistanceMode, requireManualPause)
        { }

        /// <summary>
        /// Creates a timer
        /// </summary>
        /// <param name="timeSeconds">The amount of time the timer ticks down</param>
        /// <param name="earlyEndCall">A method to call should the timer be stopped before it's timer is finished</param>
        /// <param name="callAtEnd">The method to call when the timer is finished</param>
        public InvokeWithTime(double timeSeconds, TimesUp earlyEndCall, TimesUp callAtEnd)
            : this(timeSeconds, null, earlyEndCall, callAtEnd, PersistanceMode.NotPersistant, false)
        { }

        /// <summary>
        /// Creates a timer
        /// </summary>
        /// <param name="timeSeconds">The amount of time the timer ticks down</param>
        /// <param name="callEveryTick">A method to call every frame VIA <seealso cref="TimeIterate"/></param>
        /// <param name="callAtEnd">The method to call when the timer is finished</param>
        public InvokeWithTime(double timeSeconds, TimeIterate callEveryTick, TimesUp callAtEnd)
            : this(timeSeconds, callEveryTick, callAtEnd, callAtEnd, PersistanceMode.NotPersistant, false)
        { }

        /// <summary>
        /// Creates a timer
        /// </summary>
        /// <param name="timeSeconds">The amount of time the timer ticks down</param>
        /// <param name="callEveryTick">A method to call every frame VIA <seealso cref="TimeIterate"/></param>
        public InvokeWithTime(double timeSeconds, TimeIterate callEveryTick)
            : this(timeSeconds, callEveryTick, null, null, PersistanceMode.NotPersistant, false)
        { }

        /// <summary>
        /// Creates a timer
        /// </summary>
        /// <param name="timeSeconds">The amount of time the timer ticks down</param>
        /// <param name="callAtEnd">The method to call when the timer is finished</param>
        public InvokeWithTime(double timeSeconds, TimesUp callAtEnd)
            : this(timeSeconds, null, callAtEnd, callAtEnd, PersistanceMode.NotPersistant, false)
        { }

        #endregion

        /// <summary>
        /// Perform the earlyCall method and removes this instance from the pool.
        /// </summary>
        public void ExecuteNow()
        {
            _endEarly();
            RemoveMe();
        }

        /// <summary>
        /// Sets the time left to the inital time given and resumes the timer
        /// </summary>
        public void Restart()
        {
            IsPaused = false;
            _currTime = 0;
        }

        #region Global time management

        /// <summary>
        /// Decreases the value of <see cref="_timeLeft"/> in every active instance.
        /// </summary>
        /// <remarks>
        /// Uses the milliseconds stored in <paramref name="gameTime"/>.
        /// </remarks>
        public static void DecrementTimers()
        {
            DecrementTimers(Time.deltaTime);
        }

        /// <summary>
        /// Decreases the value of <see cref="_timeLeft"/> in every active instance.
        /// </summary>
        /// <param name="timeSeconds"></param>
        public static void DecrementTimers(double timeSeconds)
        {
            for (int i = instances.Count - 1; i >= 0; i--)
            {
                // Setup
                InvokeWithTime currInstance = instances[i];

                if (currInstance.IsPaused) continue;

                // Iteritive step 
                currInstance._currTime += timeSeconds;

                currInstance._timeIterate?.Invoke(currInstance.PercentDone);

                // End step
                if (currInstance._currTime < currInstance._time) continue;

                currInstance._timesUp?.Invoke();

                switch (currInstance.Persistance)
                {
                    case PersistanceMode.NotPersistant:
                        if (i < instances.Count) instances.RemoveAt(i);
                        i--;
                        break;

                    case PersistanceMode.Persistant:
                        currInstance.IsPaused = true;
                        break;

                    case PersistanceMode.Looping:
                        currInstance.Restart();
                        break;
                }
            }
        }

        /// <summary>
        /// Attempts to pause every active timer. NOTE: this will not apply to timers with <see cref="RequireManualPause"/> enabled.
        /// </summary>
        public static void GlobalPause()
        {
            foreach (InvokeWithTime timer in instances)
            {
                if (!timer.RequireManualPause)
                    timer.IsPaused = true;
            }
        }

        #endregion

        #region Removing instances

        /// <summary>
        /// Removes all timers without performing their given methods
        /// </summary>
        public static void RemoveAllTimers()
        {
            instances.Clear();
        }

        /// <summary>
        /// Removes all the paused Timers
        /// </summary>
        public static void RemoveAllPausedTimers()
        {
            for (int i = 0; i < instances.Count; i++)
            {
                if (instances[i].IsPaused)
                {
                    instances.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Culls the list of active timers; removes all instances with a PersistanceMode that is not in the mask
        /// </summary>
        /// <param name="mask">The PersistanceMode(s) to keep in the list.</param>
        public static void CullForPersistance(PersistanceMode mask)
        {
            for (int i = 0; i < instances.Count; i++)
            {
                if ((instances[i].Persistance & mask) == 0)
                {
                    instances.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Remove this instance without performing it's operation.
        /// </summary>
        public void RemoveMe() => instances.Remove(this);

        #endregion

        #region Find instances

        /// <summary>
        /// Finds the stored instance of <see cref="InvokeWithTime"/> based on the method name.
        /// </summary>
        /// <param name="methodName">The name of the method, case sensitive</param>
        /// <returns>The found instance of <see cref="InvokeWithTime"/>, <c>null</c> if no instance is found.</returns>
        public static InvokeWithTime GetInstance(TimeIterate iterateMethod, TimesUp endMethod)
        {
            for (int i = 0; i < instances.Count; i++)
            {
                if (instances[i].EndCall == endMethod && instances[i].IterateCall == iterateMethod)
                {
                    return instances[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the stored instance of <see cref="InvokeWithTime"/> at the index <paramref name="index"/>
        /// </summary>
        /// <returns>The found instance of <see cref="InvokeWithTime"/>, <c>null</c> if no instance is found.</returns>
        public static InvokeWithTime GetInstance(int index)
        {
            return index < 0 || index >= instances.Count ?
                null : instances[index];
        }

        #endregion

        public override string ToString()
        {
            string timerComponent = TwoDecimal(_currTime) + " / " + TwoDecimal(_time);

            return $"{timerComponent} seconds have passed,  {TwoDecimal(TimeLeft)} seconds left until {_timesUp.Method.Name} is called.";
        }

        public static string TwoDecimal(double input) => string.Format("{0:0.00}", input);
    }
}
