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
    [CustomEditor(typeof(LevelZone))] [CanEditMultipleObjects]
    public class LevelZoneInspector : Editor
    {
        public VisualTreeAsset uxml;
        private VisualElement _rootElement;

        private ToolbarToggle _snapLockToolbarToggle;
        private EnumField _scrollDirectionEnumField;
        private Vector3Field _cameraOffsetVector3Field;
        private Button _randomizeColorButton;
        private Button _addEntranceButton;
        private Button _addNestedZoneButton;

        private LevelZone _targetLevelZone;

        public override VisualElement CreateInspectorGUI()
        {
            // Get target as level zone.
            _targetLevelZone = target as LevelZone;

            if (_targetLevelZone == null) return new VisualElement();
        
            // Create a new VisualElement to be the root of our Inspector UI.
            _rootElement = new VisualElement();

            // Load from default reference.
            uxml.CloneTree(_rootElement);
        
            // Get UI elements
            _snapLockToolbarToggle = _rootElement.Q<ToolbarToggle>("SnapLock");
            _scrollDirectionEnumField = _rootElement.Q<EnumField>("ScrollDirection");
            _cameraOffsetVector3Field = _rootElement.Q<Vector3Field>("CameraOffset");
            _randomizeColorButton = _rootElement.Q<Button>("RandomizeColor");
            _addEntranceButton = _rootElement.Q<Button>("AddEntrance");
            _addNestedZoneButton = _rootElement.Q<Button>("AddNestedZone");

            // Set up snap lock toggle
            _snapLockToolbarToggle.value = _targetLevelZone.Lock;
            _snapLockToolbarToggle.RegisterCallback<ChangeEvent<bool>>(
                evt => ((ISnapToBounds)_targetLevelZone).Lock = evt.newValue);
            
            // Update the cam offset field's visuals
            if (_targetLevelZone != null)
                UpdateCameraOffsetField(_targetLevelZone.scrollDirection);
            
            // Hide cam offset if not supported by current scroll direction
            _scrollDirectionEnumField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                Enum.TryParse(evt.newValue, out LevelZone.ScrollDirection value);
                UpdateCameraOffsetField(value);
            });
            

            // Link randomize color button to functionality
            _randomizeColorButton.clickable = new Clickable(() =>
            {
                _targetLevelZone.RandomizeColor();
            });
        
            // Link add level zone add entrance button to functionality
            _addEntranceButton.clickable = new Clickable(() =>
            {
                // Create a new level zone
                GameObject go = new GameObject(
                    "new LevelZoneEntrance",
                    typeof(LevelZoneEntrance)
                );
            
                // Set it as a child of the target level zone
                go.transform.SetParent(_targetLevelZone.transform);
            
                // Give it a random position
                go.transform.position = Random.insideUnitCircle * _targetLevelZone.Size.magnitude;

                // Comment the following line to stop automatically selecting the added level zone entrance
                Selection.SetActiveObjectWithContext(go, null);
            });
            
            // Link add level zone add entrance button to functionality
            _addNestedZoneButton.clickable = new Clickable(() =>
            {
                // Create a new level zone
                GameObject go = new GameObject(
                    "new LevelZone",
                    typeof(LevelZone)
                );
            
                // Set it as a child of the target level zone
                go.transform.SetParent(_targetLevelZone.transform);
            
                // Give it a random position
                go.transform.position = Random.insideUnitCircle * _targetLevelZone.Size.magnitude;

                // Set size and mode
                LevelZone newLz = go.GetComponent<LevelZone>();
                newLz.scrollDirection = LevelZone.ScrollDirection.FollowPlayer;
                newLz.Size = (_targetLevelZone.CameraBounds.size -
                              _targetLevelZone.Size) / 2f;

                // Comment the following line to stop automatically selecting the new child level zone
                Selection.SetActiveObjectWithContext(go, null);
            });

            // Return the finished Inspector UI.
            return _rootElement;
        }

        private void UpdateCameraOffsetField(
            LevelZone.ScrollDirection scrollDirection)
        {
            FloatField x = 
                _cameraOffsetVector3Field.Q<FloatField>("unity-x-input");
            FloatField y = 
                _cameraOffsetVector3Field.Q<FloatField>("unity-y-input");
            FloatField z = 
                _cameraOffsetVector3Field.Q<FloatField>("unity-z-input");
            z.style.opacity = .5f;
            z.isReadOnly = true;
            switch (scrollDirection)
            {
                case LevelZone.ScrollDirection.Horizontal:
                    x.style.opacity = .5f;
                    x.isReadOnly = true;
                    //x.pickingMode = PickingMode.Ignore;
                    y.style.opacity = 1f;
                    y.isReadOnly = false;
                    //y.pickingMode = PickingMode.Position;
                    break;
                    
                case LevelZone.ScrollDirection.Vertical:
                    x.style.opacity = 1f;
                    x.isReadOnly = false;
                    //x.pickingMode = PickingMode.Position;
                    y.style.opacity = .5f;
                    y.isReadOnly = true;
                    //y.pickingMode = PickingMode.Ignore;
                    break;
                    
                default: 
                    x.style.opacity = 1f;
                    x.isReadOnly = false;
                    //x.pickingMode = PickingMode.Position;
                    y.style.opacity = 1f;
                    y.isReadOnly = false;
                    //y.pickingMode = PickingMode.Position;
                    break;
            }
        }

        private void OnSceneGUI()
        {
            // Get level zone if null
            if (_targetLevelZone == null)
            {
                _targetLevelZone = target as LevelZone;
            }
            
            // Early exit if no reference found
            if (_targetLevelZone == null) return;
        
            // Get level zone position 
            Vector3 pos = _targetLevelZone.transform.position;
            
            // Draw level zone children handles
            foreach (LevelZone lz in _targetLevelZone.transform.GetComponentsInChildren<LevelZone>())
            {
                // Skip targeted zone
                if (lz == _targetLevelZone) continue;
                
                // Skip snap locked zones
                if (lz.Lock) return;
                
                // Get a level zone entrance position
                Vector3 lzPos = lz.transform.position;
            
                // Track position influence
                Vector3 endLzPos = lzPos;
            
                // Get valid movement axis
                Vector3[] dirs = ((ISnapToBounds)lz).GetValidMovementAxes(lz.BColl.bounds);

                // Draw handles using dirs
                for (var i = 0; i < dirs.Length; i++)
                {
                    // Set handles color
                    if (dirs[i].magnitude == 0)
                    {
                        Handles.color = Color.clear;
                    }
                    else if (Vector3.Angle(dirs[i], Vector3.up) < 1)
                    {
                        Handles.color = Color.green;
                    }
                    else if (Vector3.Angle(dirs[i], Vector3.right) < 1)
                    {
                        Handles.color = Color.red;
                    }
                    else // Shouldn't happen, magenta indicates error
                    {
                        Handles.color = Color.magenta;
                    }

                    // Modify dir if needed
                    if (lzPos.x > pos.x)
                    {
                        dirs[i].x *= -1;
                    }
                    if (lzPos.y > pos.y)
                    {
                        dirs[i].y *= -1;
                    }

                    // Draw handle and add influence to end position
                    endLzPos += 
                        ((ISnapToBounds)lz).RoundLocationToBounds(
                            Handles.Slider(lzPos, dirs[i])
                        ) - lzPos;
                }    

                // Set new level zone entrance position to calculated end position 
                lz.transform.position = endLzPos; // THIS LINE CAUSES CRAZY FLIP FLOPPING
            }
            
            // Draw level zone entrance handles
            foreach (LevelZoneEntrance lze in _targetLevelZone.transform.GetComponentsInChildren<LevelZoneEntrance>())
            {
                // Get a level zone entrance position
                Vector3 lzePos = lze.transform.position;
            
                // Track position influence
                Vector3 endLzePos = lzePos;
                
                // Handles based on locking
                Vector3 handlePos;
                if (lze.Lock)
                {
                    // Get valid movement axis
                    Vector3[] dirs = lze.GetValidMovementAxes();

                    // Draw handles using dirs
                    for (var i = 0; i < dirs.Length; i++)
                    {
                        // Skip drawing handle for invalid directions
                        if (dirs[i].magnitude == 0)
                        {
                            Handles.color = Color.clear;
                            continue;
                        }
                    
                        // Set handles color
                        if (Vector3.Angle(dirs[i], Vector3.up) < 1)
                        {
                            Handles.color = Color.green;
                        }
                        else if (Vector3.Angle(dirs[i], Vector3.right) < 1)
                        {
                            Handles.color = Color.red;
                        }
                        else // Shouldn't happen, magenta indicates error
                        {
                            Handles.color = Color.magenta;
                        }

                        // Modify dir if needed
                        if (lzePos.x > pos.x)
                        {
                            dirs[i].x *= -1;
                        }
                        if (lzePos.y > pos.y)
                        {
                            dirs[i].y *= -1;
                        }
                    
                        // Draw handle and add influence to end position
                        handlePos = Handles.Slider(lzePos, dirs[i]);
                        endLzePos += ((ISnapToBounds)lze).RoundLocationToBounds(handlePos) - lzePos;
                    }
                }
                else
                {
                    handlePos = Handles.PositionHandle(lzePos, Quaternion.identity);
                    endLzePos += handlePos - lzePos;
                }

                // Set new level zone entrance position to calculated end position 
                lze.transform.position = endLzePos;
            }
        }
    }
}

#endif