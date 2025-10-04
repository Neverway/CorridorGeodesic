using System;
using System.Diagnostics;
using System.Threading;
using Debug = UnityEngine.Debug;

namespace RivenFramework
{
    public class Benchmark
    {
        public const long ONE_FRAME_60FPS = 1000 / 60;
        public static long maxTime = 5 * 1000; //5 seconds
        private static Stopwatch currentBenchmark;

        public static void TestMillisecondsPerIterations(string name, int iterations, Action action)
        {
            name = ConvertBenchmarkName(name);

            //Start tracking the time passed
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                //Call the provided code inside the loop to time it
                action.Invoke();

                //Stop elapsing if a certain amount of time has passed
                if (stopwatch.ElapsedMilliseconds > maxTime)
                {
                    stopwatch.Stop();
                    Debug.Log($"{name}   <color=#ff6666>aborted at {maxTime} milliseconds</color>, (<b>{i}</b> iterations)\n");
                    return;
                }
            }

            stopwatch.Stop();
            Debug.Log($"{name}  took <b>{stopwatch.ElapsedMilliseconds}</b> milliseconds\n");
        }
        public static void TestIterationsPerMilliseconds(string name, long milliseconds, Action action)
        {
            name = ConvertBenchmarkName(name);

            //Start tracking the time passed
            Stopwatch stopwatch = Stopwatch.StartNew();
            int iterations = 0;
            while (stopwatch.ElapsedMilliseconds < milliseconds)
            {
                //Call the provided code inside the loop to time it
                action.Invoke();
                iterations++;
            }
            Debug.Log($"{name}   completed <b>{iterations}</b> iterations in <b>{stopwatch.ElapsedMilliseconds}</b> milliseconds\n");
            return;
        }
        public static void TestOneSecond(string name, Action action)
        {
            name = ConvertBenchmarkName(name);

            //Start tracking the time passed
            int iterations = -1;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < 1000)
            {
                Debug.Log(stopwatch.ElapsedMilliseconds);
                //Call the provided code inside the loop to time it
                action.Invoke();
                iterations++;
            }
            Debug.Log($"{name}   completed <b>{iterations}</b> iterations in one second\n");
            return;
        }
        public static void TestIterationsPerFrameAt60fps(string name, Action action)
        {
            name = ConvertBenchmarkName(name);

            //Start tracking the time passed
            int iterations = -1;
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < ONE_FRAME_60FPS)
            {
                //Call the provided code inside the loop to time it
                action.Invoke();
                iterations++;
            }
            Debug.Log($"{name}   completed <b>{iterations}</b> iterations in the time it takes for 1 frame at 60fps\n");
            return;
        }

        public static void StartTiming()
        {
            if (currentBenchmark != null)
                Debug.LogWarning("Multiple Benchmark timers running, did you forget to stop one?");

            currentBenchmark = Stopwatch.StartNew();
        }
        public static void StopTiming(string name)
        {
            currentBenchmark.Stop();
            name = ConvertBenchmarkName(name);
            Debug.Log($"{name}  took <b>{currentBenchmark.ElapsedMilliseconds}</b> milliseconds\n");

            currentBenchmark = null;
        }

        private static string ConvertBenchmarkName(string name) =>
            string.IsNullOrEmpty(name) ?
                name = $"<size=16>Benchmark</size>" :
                name = $"<size=16>Benchmark <b>{name}</b></size>";
    }
}
