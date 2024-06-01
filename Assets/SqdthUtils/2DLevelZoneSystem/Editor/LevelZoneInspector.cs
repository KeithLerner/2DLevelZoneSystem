using System;
using SqdthUtils._2DLevelZoneSystem.Scripts;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR

using UnityEditor;

namespace SqdthUtils._2DLevelZoneSystem.Editor
{
    [CustomEditor(typeof(LevelZone))] [CanEditMultipleObjects]
    public class LevelZoneInspector : UnityEditor.Editor
    {
        public VisualTreeAsset uxml;
        private VisualElement rootElement;
    
        private EnumField scrollDirectionEnumField;
        private Vector2Field cameraOffsetVector2Field;
        private Button randomizeColorButton;
        private Button addEntranceButton;
        private Button addNestedZoneButton;

        private LevelZone targetLevelZone;

        public override VisualElement CreateInspectorGUI()
        {
            // Get target as level zone.
            targetLevelZone = target as LevelZone;
        
            // Create a new VisualElement to be the root of our Inspector UI.
            rootElement = new VisualElement();

            // Load from default reference.
            uxml.CloneTree(rootElement);
        
            // Get UI elements 
            scrollDirectionEnumField = rootElement.Q<EnumField>("ScrollDirection");
            cameraOffsetVector2Field = rootElement.Q<Vector2Field>("CameraOffset");
            randomizeColorButton = rootElement.Q<Button>("RandomizeColor");
            addEntranceButton = rootElement.Q<Button>("AddEntrance");
            addNestedZoneButton = rootElement.Q<Button>("AddNestedZone");
            
            // Update the cam offset field's visuals
            if (targetLevelZone != null)
                UpdateCameraOffsetField(targetLevelZone.scrollDirection);
            
            // Hide cam offset if not supported by current scroll direction
            scrollDirectionEnumField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                Enum.TryParse(evt.newValue, out LevelZone.ScrollDirection value);
                UpdateCameraOffsetField(value);
            });
            

            // Link randomize color button to functionality
            randomizeColorButton.clickable = new Clickable(() =>
            {
                targetLevelZone.RandomizeColor();
            });
        
            // Link add level zone add entrance button to functionality
            addEntranceButton.clickable = new Clickable(() =>
            {
                // Create a new level zone
                GameObject go = new GameObject(
                    "new LevelZoneEntrance",
                    typeof(LevelZoneEntrance)
                );
            
                // Set it as a child of the target level zone
                go.transform.SetParent(targetLevelZone.transform);
            
                // Give it a random position
                go.transform.position = Random.insideUnitCircle * targetLevelZone.Size.magnitude;

                // Comment the following line to stop automatically selecting the added level zone entrance
                Selection.SetActiveObjectWithContext(go, null);
            });
            
            // Link add level zone add entrance button to functionality
            addNestedZoneButton.clickable = new Clickable(() =>
            {
                // Create a new level zone
                GameObject go = new GameObject(
                    "new LevelZone",
                    typeof(LevelZone)
                );
            
                // Set it as a child of the target level zone
                go.transform.SetParent(targetLevelZone.transform);
            
                // Give it a random position
                go.transform.position = Random.insideUnitCircle * targetLevelZone.Size.magnitude;

                // Set size and mode
                LevelZone newLz = go.GetComponent<LevelZone>();
                newLz.scrollDirection = LevelZone.ScrollDirection.FollowPlayer;
                newLz.Size = ((Vector2)targetLevelZone.CameraBounds.size -
                              targetLevelZone.Size) / 2f;

                // Comment the following line to stop automatically selecting the new child level zone
                Selection.SetActiveObjectWithContext(go, null);
            });

            // Return the finished Inspector UI.
            return rootElement;
        }

        private void UpdateCameraOffsetField(
            LevelZone.ScrollDirection scrollDirection)
        {
            FloatField x = 
                cameraOffsetVector2Field.Q<FloatField>("unity-x-input");
            FloatField y = 
                cameraOffsetVector2Field.Q<FloatField>("unity-y-input");
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
            if (targetLevelZone == null)
            {
                targetLevelZone = target as LevelZone;
            }
        
            // Get level zone position 
            Vector3 pos = targetLevelZone.transform.position;
            
            // Draw level zone children handles
            foreach (LevelZone lz in targetLevelZone.transform.GetComponentsInChildren<LevelZone>())
            {
                // Skip targeted zone
                if (lz == targetLevelZone) continue;
                
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
                lz.transform.position = endLzPos;
            }
            
            // Draw level zone entrance handles
            foreach (LevelZoneEntrance lze in targetLevelZone.transform.GetComponentsInChildren<LevelZoneEntrance>())
            {
                // Get a level zone entrance position
                Vector3 lzePos = lze.transform.position;
            
                // Track position influence
                Vector3 endLzePos = lzePos;
                
                // Handles based on locking
                Vector3 handlePos;
                if (lze.LockToParentBounds)
                {
                    // Get valid movement axis
                    Vector3[] dirs = lze.LockToParentBounds ? 
                        lze.GetValidMovementAxes() : 
                        new []{ Vector3.up, Vector3.right, };

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