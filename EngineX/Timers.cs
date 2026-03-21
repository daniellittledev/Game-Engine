using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace EngineX
{

    namespace Timers
    {

        /// <summary>
        /// High resolution timer
        /// </summary>
        public class HiResTimer
        {
            /// <summary>
            /// Initilize Camera
            /// </summary>
            private HiResTimer() { } // No creation

            /// <summary>
            /// Static creation routine
            /// </summary>
            static HiResTimer()
            {
                isTimerStopped = true;
                ticksPerSecond = 0;
                stopTime = 0;
                lastElapsedTime = 0;
                baseTime = 0;
                // Use QueryPerformanceFrequency to get frequency of the timer
                isUsingQPF = QueryPerformanceFrequency(ref ticksPerSecond);
            }

            /// <summary>
            /// Resets the timer
            /// </summary>
            public static void Reset()
            {
                if (!isUsingQPF)
                    return; // Nothing to do

                // Get either the current time or the stop time
                long time = 0;
                if (stopTime != 0)
                    time = stopTime;
                else
                    QueryPerformanceCounter(ref time);

                baseTime = time;
                lastElapsedTime = time;
                stopTime = 0;
                isTimerStopped = false;
            }

            /// <summary>
            /// Starts the timer
            /// </summary>
            public static void Start()
            {
                if (!isUsingQPF)
                    return; // Nothing to do

                // Get either the current time or the stop time
                long time = 0;
                if (stopTime != 0)
                    time = stopTime;
                else
                    QueryPerformanceCounter(ref time);

                if (isTimerStopped)
                    baseTime += (time - stopTime);
                stopTime = 0;
                lastElapsedTime = time;
                isTimerStopped = false;
            }

            /// <summary>
            /// Stop (or pause) the timer
            /// </summary>
            public static void Stop()
            {
                if (!isUsingQPF)
                    return; // Nothing to do

                if (!isTimerStopped)
                {
                    // Get either the current time or the stop time
                    long time = 0;
                    if (stopTime != 0)
                        time = stopTime;
                    else
                        QueryPerformanceCounter(ref time);

                    stopTime = time;
                    lastElapsedTime = time;
                    isTimerStopped = true;
                }
            }

            /// <summary>
            /// Advance the timer a tenth of a second
            /// </summary>
            public static void Advance()
            {
                if (!isUsingQPF)
                    return; // Nothing to do

                stopTime += ticksPerSecond / 10;
            }

            /// <summary>
            /// Get the absolute system time
            /// </summary>
            public static double GetAbsoluteTime()
            {
                if (!isUsingQPF)
                    return -1.0; // Nothing to do

                // Get either the current time or the stop time
                long time = 0;
                if (stopTime != 0)
                    time = stopTime;
                else
                    QueryPerformanceCounter(ref time);

                double absolueTime = time / (double)ticksPerSecond;
                return absolueTime;
            }

            /// <summary>
            /// Get the current time
            /// </summary>
            public static double GetTime()
            {
                if (!isUsingQPF)
                    return -1.0; // Nothing to do

                // Get either the current time or the stop time
                long time = 0;
                if (stopTime != 0)
                    time = stopTime;
                else
                    QueryPerformanceCounter(ref time);

                double appTime = (double)(time - baseTime) / (double)ticksPerSecond;
                return appTime;
            }

            /// <summary>
            /// get the time that elapsed between GetElapsedTime() calls
            /// </summary>
            public static float GetElapsedTime()
            {
                if (!isUsingQPF)
                    return -1.0f; // Nothing to do

                // Get either the current time or the stop time
                long time = 0;
                if (stopTime != 0)
                    time = stopTime;
                else
                    QueryPerformanceCounter(ref time);

                double elapsedTime = (double)(time - lastElapsedTime) / (double)ticksPerSecond;
                lastElapsedTime = time;
                return (float)elapsedTime;
            }

            /// <summary>
            /// Returns true if timer stopped
            /// </summary>
            public static bool IsStopped
            {
                get { return isTimerStopped; }
            }

            /// <summary>
            /// Using Query Performance Frequency
            /// </summary>
            private static bool isUsingQPF;
            /// <summary>
            /// Timer stopped
            /// </summary>
            private static bool isTimerStopped;
            /// <summary>
            /// Ticks per second
            /// </summary>
            private static long ticksPerSecond;
            /// <summary>
            /// Timer Stoped Time
            /// </summary>
            private static long stopTime;
            /// <summary>
            /// Last ElapsedTime
            /// </summary>
            private static long lastElapsedTime;
            /// <summary>
            /// Base Time
            /// </summary>
            private static long baseTime;

            /// <summary>
            /// Query Performance Counter get: Performance Frequency
            /// </summary>
            /// <param name="PerformanceFrequency"></param>
            /// <returns></returns>
            [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
            [System.Runtime.InteropServices.DllImport("kernel32")]
            public static extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);

            /// <summary>
            /// Query Performance Counter get: Performance Count
            /// </summary>
            /// <param name="PerformanceCount"></param>
            /// <returns></returns>
            [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
            [System.Runtime.InteropServices.DllImport("kernel32")]
            public static extern bool QueryPerformanceCounter(ref long PerformanceCount);
        }


    }

}