using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace BlueMuffinGames.Utility
{
    public class ProgressTracker
    {
        public event Action<float> ProgressChanged = delegate { };
        public event Action Completed = delegate { };

        public float Progress
        {
            get => _progress;
            set
            {
                if (value <= _progress) return;
                _progress = Mathf.Clamp01(value);
                ProgressChanged?.Invoke(value);
                if (_progress >= 1) Completed?.Invoke();
            }
        }

        private float _progress = 0f;
        private int _subprogressesInProgress = 0;

        public ProgressTracker(Action<float> progressChanged = null, Action completed = null)
        {
            if (progressChanged != null) ProgressChanged += progressChanged;
            if (completed != null) Completed += completed;
        }

        /// <summary>
        /// Assigns at least one sub-ProgressTracker for this tracker to use as a reference for its own progress.
        /// </summary>
        /// <param name="subprogresses">The sub-ProgressTrackers</param>
        public void TrackSubprocesses(params ProgressTracker[] subprogresses) => TrackSubprocesses(subprogresses.ToList());
        /// <summary>
        /// Assigns at least one sub-ProgressTracker for this tracker to use as a reference for its own progress.
        /// </summary>
        /// <param name="subprogresses">The sub-ProgressTrackers</param>
        public void TrackSubprocesses(List<ProgressTracker> subprogresses)
        {
            int count = subprogresses.Count;
            foreach (var subprog in subprogresses)
            {
                if (subprog == this)
                {
                    count--;
                    continue;
                }
                subprog.ProgressChanged += HandleInternalSubprocessChanged;
                subprog.Completed += HandleSubprocessComplete;
            }

            void HandleInternalSubprocessChanged(float progress)
            {
                float total = 0f;
                foreach (var subprog in subprogresses)
                {
                    total += subprog.Progress / count;
                }
                Progress = total;
            }

            void HandleSubprocessComplete()
            {
                _subprogressesInProgress--;
                if (_subprogressesInProgress == 0) Completed?.Invoke();
            }

        }

        #region Loop Progress Helpers
        /// <summary>
        /// Performs a for-loop over the given IEnumerable while updating the progress after each step. Can be awaited.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="iteration"></param>
        /// <returns></returns>
        public async Task AwaitableTrackedForLoop<T>(IEnumerable<T> enumerable, Action<int, T> iteration)
        {
            int count = enumerable.Count();
            for (int i = 0; i < count; i++)
            {
                iteration(i, enumerable.ElementAt(i));
                Progress = (float)(i + 1) / count;
                await Awaitable.NextFrameAsync();
            }
            Progress = 1;
        }

        /// <summary>
        /// Performs a partitioned for-loop over the given IEnumerable while updating the progress after each step, with the given outer and inner loop dimensions. Can be awaited.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="outer"></param>
        /// <param name="inner"></param>
        /// <param name="iteration"></param>
        /// <returns></returns>
        public async Task AwaitableTrackedPartitionedLoop<T>(IEnumerable<T> enumerable, int outer, int inner, Action<int, int, T> iteration)
        {
            int count = enumerable.Count();
            for (int i = 0; i < outer; i++)
            {
                for (int j = 0; j < inner; j++)
                {
                    int index = i * outer + j;
                    iteration(i, j, enumerable.ElementAt(index));
                    Progress = (float)(index + 1) / count;
                    await Awaitable.NextFrameAsync();
                }
            }
            Progress = 1;
        }

        /// <summary>
        /// Performs a nested for-loop over the given outer and inner IEnumerables while updating the progress after each step, with the given outer and inner loop dimensions. Can be awaited.
        /// </summary>
        /// <typeparam name="TOuter"></typeparam>
        /// <typeparam name="TInner"></typeparam>
        /// <param name="outerEnumerable"></param>
        /// <param name="getInner"></param>
        /// <param name="iteration"></param>
        /// <returns></returns>
        public async Task AwaitableTrackedNestedLoop<TOuter, TInner>(IEnumerable<TOuter> outerEnumerable, Func<TOuter, IEnumerable<TInner>> getInner, Action<int, int, TOuter, TInner> iteration)
        {
            int count = 0;
            foreach (TOuter outer in outerEnumerable)
            {
                foreach (TInner inner in getInner(outer))
                {
                    count++;
                }
            }

            int index = 0;
            int outerIndex = 0;
            int innerIndex = 0;
            foreach (TOuter outer in outerEnumerable)
            {
                foreach (TInner inner in getInner(outer))
                {
                    iteration(outerIndex, innerIndex, outer, inner);

                    innerIndex++;
                    index++;
                    Progress = (float)(index + 1) / count;
                    await Awaitable.NextFrameAsync();
                }
                outerIndex++;
            }

            Progress = 1;
        }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        /// <summary>
        /// Performs a for-loop over the given IEnumerable while updating the progress after each step. Use the Completed event to track when it finishes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="iteration"></param>
        public void TrackedForLoop<T>(IEnumerable<T> enumerable, Action<int, T> iteration) => AwaitableTrackedForLoop(enumerable, iteration);
        /// <summary>
        /// Performs a nested for-loop over the given IEnumerable while updating the progress after each step, with the given outer and inner loop dimensions. Use the Completed event to track when it finishes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="outer"></param>
        /// <param name="inner"></param>
        /// <param name="iteration"></param>
        public void TrackedPartitionedLoop<T>(IEnumerable<T> enumerable, int outer, int inner, Action<int, int, T> iteration) => AwaitableTrackedPartitionedLoop(enumerable, outer, inner, iteration);
        /// <summary>
        /// Performs a nested for-loop over the given outer and inner IEnumerables while updating the progress after each step, with the given outer and inner loop dimensions.
        /// </summary>
        /// <typeparam name="TOuter"></typeparam>
        /// <typeparam name="TInner"></typeparam>
        /// <param name="outerEnumerable"></param>
        /// <param name="getInner"></param>
        /// <param name="iteration"></param>
        public void TrackedNestedLoop<TOuter, TInner>(IEnumerable<TOuter> outerEnumerable, Func<TOuter, IEnumerable<TInner>> getInner, Action<int, int, TOuter, TInner> iteration) => AwaitableTrackedNestedLoop(outerEnumerable, getInner, iteration);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        #endregion

        public void Reset()
        {
            _progress = 0;
            ProgressChanged?.Invoke(_progress);
        }
    }

}