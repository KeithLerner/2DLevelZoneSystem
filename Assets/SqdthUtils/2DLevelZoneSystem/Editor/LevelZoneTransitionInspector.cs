using System;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UIElements;

namespace SqdthUtils
{
    [CustomEditor(typeof(LevelZoneTransition))] [CanEditMultipleObjects]
    public class LevelZoneTransitionInspector : Editor
    {
        public VisualTreeAsset uxml;
        private VisualElement _rootElement;

        private ToolbarToggle _snapLockToolbarToggle;
        private EnumField _transitionTypeEnumField;
        private TextField _transitionSceneNameField;

        private LevelZoneTransition _targetLevelZoneTransition;

        public override VisualElement CreateInspectorGUI()
        {
            // Get target as level zone.
            _targetLevelZoneTransition = target as LevelZoneTransition;

            if (_targetLevelZoneTransition == null) return new VisualElement();
        
            // Create a new VisualElement to be the root of our Inspector UI.
            _rootElement = new VisualElement();

            // Load from default reference.
            uxml.CloneTree(_rootElement);
        
            // Get UI elements
            _snapLockToolbarToggle = _rootElement.Q<ToolbarToggle>("SnapLock");
            _transitionTypeEnumField = _rootElement.Q<EnumField>("TransitionType");
            _transitionSceneNameField = _rootElement.Q<TextField>("TransitionSceneName");
            Debug.Log(_transitionSceneNameField.name);

            // Set up snap lock toggle
            _snapLockToolbarToggle.value = _targetLevelZoneTransition.Lock;
            _snapLockToolbarToggle.RegisterCallback<ChangeEvent<bool>>(
                evt => ((ISnapToBounds)_targetLevelZoneTransition).Lock = evt.newValue);
            
            // Update the cam offset field's visuals
            if (_targetLevelZoneTransition != null)
                UpdateSceneNameField(_targetLevelZoneTransition.TransitionStyle);
            
            // Hide cam offset if not supported by current scroll direction
            _transitionTypeEnumField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                Enum.TryParse(evt.newValue, out LevelZoneTransition.Transition value);
                UpdateSceneNameField(value);
            });

            // Return the finished Inspector UI.
            return _rootElement;
        }

        private void UpdateSceneNameField(
            LevelZoneTransition.Transition transition)
        {
            switch (transition)
            {
                case LevelZoneTransition.Transition.Scene:
                case LevelZoneTransition.Transition.SceneAsync:
                    _transitionSceneNameField.style.display =
                        new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    _transitionSceneNameField.isReadOnly = false;
                    break;
                
                default: 
                    _transitionSceneNameField.style.display =
                        new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    _transitionSceneNameField.isReadOnly = true;
                    break;
            }
        }

        private void DrawZoneTransition()
        {
            Color color = _targetLevelZoneTransition.TransitionStyle switch
            {
                LevelZoneTransition.Transition.Scene => Color.black,
                LevelZoneTransition.Transition.SceneAsync => Color.black,
                _ => Color.white
            };
            /*Handles.DrawWireCube(_targetLevelZoneTransition.transform.position,
                _targetLevelZoneTransition.BColl.size);*/
            Rect transitionRect = new()
            {
                center = _targetLevelZoneTransition.BColl.bounds.center -
                    .5f * _targetLevelZoneTransition.BColl.bounds.size,
                size = _targetLevelZoneTransition.BColl.bounds.size
            };
            Handles.DrawSolidRectangleWithOutline(transitionRect,
                _targetLevelZoneTransition.OwningZone.LevelZoneColor, color);
        }

        private void OnSceneGUI()
        {
            // Get level zone if null
            if (_targetLevelZoneTransition == null)
            {
                _targetLevelZoneTransition = target as LevelZoneTransition;
            }
            
            // Early exit if no reference found
            if (_targetLevelZoneTransition == null) return;
            
            // Draw target zone transition
            DrawZoneTransition();
        }
    }
}

#endif