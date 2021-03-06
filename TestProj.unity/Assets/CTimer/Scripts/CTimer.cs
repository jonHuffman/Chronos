﻿/*
MIT License

Copyright (c) 2017 Jon Huffman

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections.Generic;
using UnityEngine;

namespace CTimers
{
    public class CTimer
    {
        public const int INFINITE_LOOP = -1;

        private const int DEFAULT_POOL_SIZE = 10;

        private static bool _isInitialized = false;
        private static UnityTicker _ticker;

        private static HashSet<Timer> _activeTimers;
        private static HashSet<Timer> _processingTimers;
        
        internal static int ActiveTimers
        {
            get { return _activeTimers.Count; }
        }

        /// <summary>
        /// Starts a Timer counting to the set time value and returns it for access and modification.
        /// </summary>
        /// <param name="time">The duration of the Timer</param>
        /// <returns>The Timer</returns>
        public static Timer Start(float time)
        {
            InitCheck();

            Timer timer = new Timer();
            timer.Start(time);
            return timer;
        }

        /// <summary>
        /// Initializes CTimer. Only needs to be called once in the lifecycle of the application. If it is not called, CTimer will auto-initialize when you Start your first Timer.
        /// </summary>
        /// <param name="initialPoolSize">The initial number of Timers to create in the Pool</param>
        /// <param name="recycleTimers">If true, Timers will automatically return to a Pool upon completion. Stopped Timers are not considered completed.</param>
        public static void Init(int initialPoolSize, bool recycleTimers = false)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("Chronos has already been initialized. Make sure that this is called before starting any Timers.");
                return;
            }

            _isInitialized = true;

            _activeTimers = new HashSet<Timer>();
            _processingTimers = new HashSet<Timer>();
        }

        /// <summary>
        /// Links the UnityTicker to Chronos so that we can update our Timers
        /// </summary>
        /// <param name="ticker">The ticker to link</param>
        internal static void SetTicker(UnityTicker ticker)
        {
            _ticker = ticker;
            _ticker.SetTickCallback(Tick);
        }

        /// <summary>
        /// Registers the provided Timer to recieve ticks so that it can progress
        /// </summary>
        /// <param name="timer">Timer to register</param>
        internal static void RegisterForTicks(Timer timer)
        {
            InitCheck();

            _activeTimers.Add(timer);
        }

        /// <summary>
        /// Unregisters the provided Timer so it will not longer receive update ticks
        /// </summary>
        /// <param name="timer">Timer to unregister</param>
        internal static void UnregisterForTicks(Timer timer)
        {
            InitCheck();

            _activeTimers.Remove(timer);
        }

        /// <summary>
        /// Updates all active timers with the provided delat time values
        /// </summary>
        /// <param name="deltaTime">The delta time of the frame</param>
        /// <param name="unscaledDeltaTime">The unscaled delta time of the frame</param>
        private static void Tick(float deltaTime, float unscaledDeltaTime)
        {
            // We don't perform a full initialization check here in order to prevent the ticker from initializing Chronos before we are ready
            if (_isInitialized && _activeTimers.Count > 0)
            {
                // Copy the active timer list and process based on the copy in the event the act of processing the timers causes some to be started or completed
                _processingTimers.Clear();
                _processingTimers.UnionWith(_activeTimers);

                foreach (Timer t in _processingTimers)
                {
                    t.Tick(t.UpdateMode == UpdateMode.Normal ? deltaTime : unscaledDeltaTime);
                }
            }
        }

        /// <summary>
        /// Confirms initialization and if uninitialized, performs initialization... initialize the initialization sequence and get this thing initialized!
        /// </summary>
        private static void InitCheck()
        {
            if (_isInitialized)
            {
                return;
            }

            Init(DEFAULT_POOL_SIZE);
        }
    }
}