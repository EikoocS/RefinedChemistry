using System;
using UnityEngine;

namespace RefinedChemistry.Machinery
{
    internal sealed class MachineryRefresh : MonoBehaviour
    {
        internal Action refresh;
        private float nextRefresh;

        private void OnEnable() => refresh?.Invoke();

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh)
                return;
            nextRefresh = Time.unscaledTime + 0.25f;
            refresh?.Invoke();
        }
    }
}
