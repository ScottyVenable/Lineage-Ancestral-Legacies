﻿using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
#if !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace Michsky.MUIP
{
    [RequireComponent(typeof(TMP_InputField))]
    [RequireComponent(typeof(Animator))]
    public class CustomInputField : MonoBehaviour
    {
        [Header("Resources")]
        public TMP_InputField inputText;
        [SerializeField] private Animator inputFieldAnimator;

        [Header("Settings")]
        public bool processSubmit = false;
        public bool clearOnSubmit = true;
        [Tooltip("Set the current event system object as null.")]
        [SerializeField] private bool setEventSystem = false;

        [Header("Events")]
        public UnityEvent onSubmit = new UnityEvent();

        // Hidden variables
        private bool isActive = false;
        const float cachedDuration = 0.5f;
        const string inAnim = "In";
        const string outAnim = "Out";
        const string instaInAnim = "Instant In";
        const string instaOutAnim = "Instant Out";

        void Awake()
        {
            Initialize();

            inputText.onSelect.AddListener(delegate { AnimateIn(); });
            inputText.onEndEdit.AddListener(delegate { HandleEndEdit(); });
            inputText.onValueChanged.AddListener(delegate { UpdateState(); });

            UpdateStateInstant();
        }

        void OnEnable()
        {
            if (inputText == null || inputFieldAnimator == null) { Initialize(); }
            inputText.ForceLabelUpdate();
            UpdateStateInstant();
        }

        void Update()
        {
            if (!processSubmit || string.IsNullOrEmpty(inputText.text) || EventSystem.current.currentSelectedGameObject != inputText.gameObject)
                return;

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Return)) 
            { 
                onSubmit.Invoke();

                if (clearOnSubmit) 
                {
                    inputText.text = ""; 
                    UpdateState();
                } 
            }
#elif ENABLE_INPUT_SYSTEM
            if (Keyboard.current.enterKey.wasPressedThisFrame) 
            { 
                onSubmit.Invoke(); 
                
                if (clearOnSubmit) 
                { 
                    inputText.text = ""; 
                    UpdateState();
                } 
            }
#endif
        }

        void Initialize()
        {
            if (inputText == null) { inputText = gameObject.GetComponent<TMP_InputField>(); }
            if (inputFieldAnimator == null) { inputFieldAnimator = gameObject.GetComponent<Animator>(); }
        }

        public void AnimateIn() 
        {
            if (inputFieldAnimator.gameObject.activeInHierarchy && !isActive) 
            {
                StopCoroutine("DisableAnimator");

                inputFieldAnimator.enabled = true;
                if (!isActive) { inputFieldAnimator.Rebind(); }
                inputFieldAnimator.Play(inAnim);
                isActive = true;

                StartCoroutine("DisableAnimator");
            }
        }

        public void AnimateOut()
        {
            if (inputFieldAnimator.gameObject.activeInHierarchy && inputText.text.Length == 0 && isActive)
            {
                StopCoroutine("DisableAnimator");
      
                inputFieldAnimator.enabled = true;
                if (inputText.text.Length == 0) { inputFieldAnimator.Rebind(); }
                inputFieldAnimator.Play(outAnim);
                isActive = false;

                StartCoroutine("DisableAnimator");
            }
        }

        public void UpdateState()
        {
            if (inputText.text.Length == 0) { AnimateOut(); }
            else { AnimateIn(); }
        }

        public void UpdateStateInstant()
        {
            inputFieldAnimator.enabled = true;
            inputFieldAnimator.Rebind();

            StopCoroutine("DisableAnimator");
            StartCoroutine("DisableAnimator");

            if (inputText.text.Length == 0) { isActive = false; inputFieldAnimator.Play(instaOutAnim);  }
            else { isActive = true; inputFieldAnimator.Play(instaInAnim); }
        }

        void HandleEndEdit()
        {
            if (setEventSystem && string.IsNullOrEmpty(inputText.text) && !EventSystem.current.alreadySelecting && EventSystem.current.currentSelectedGameObject == inputText.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            AnimateOut();
        }

        IEnumerator DisableAnimator()
        {
            yield return new WaitForSecondsRealtime(cachedDuration);
            inputFieldAnimator.enabled = false;
        }
    }
}