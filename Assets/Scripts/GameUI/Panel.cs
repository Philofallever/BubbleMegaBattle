using UnityEngine;
using System.Collections;
using Logic;

namespace GameUI
{
    public class Panel : MonoBehaviour
    {
        private static GameObject _lastShowPanelObj;

        protected static GameObject LastShowPanelObj
        {
            get => _lastShowPanelObj;
            set
            {
                _lastShowPanelObj?.SetActive(false);
                _lastShowPanelObj = value;
            }
        }

        protected virtual void OnEnable()
        {
            LastShowPanelObj = gameObject;
        }
    }
}