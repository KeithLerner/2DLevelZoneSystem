using System;
using SqdthUtils._2DLevelZoneSystem.Scripts;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR

namespace SqdthUtils._2DLevelZoneSystem.Editor
{
    [UnityEditor.CustomEditor(typeof(LevelZone))]
    public class LevelZoneInspector : UnityEditor.Editor
    {
        public VisualTreeAsset uxml;
        private VisualElement rootElement;
    
        private EnumField scrollDirectionEnumField;
        private FloatField cameraOffsetFloatField;
        private Button randomizeColorButton;
        private Button addEntranceButton;

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
            cameraOffsetFloatField = rootElement.Q<FloatField>("CameraOffset");
            randomizeColorButton = rootElement.Q<Button>("RandomizeColor");
            addEntranceButton = rootElement.Q<Button>("AddEntrance");
        
            // Hide cam offset if not supported by current scroll direction
            scrollDirectionEnumField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                Enum.TryParse(evt.newValue, out LevelZone.ScrollDirection valueInt);
                cameraOffsetFloatField.visible = LevelZone.DoesCameraOffset(valueInt);
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

                // Uncomment the following line to automatically select the added level zone entrance
                //UnityEditor.Selection.SetActiveObjectWithContext(go, null);
            });

            // Return the finished Inspector UI.
            return rootElement;
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
                // Get a level zone entrance position
                Vector3 lzPoz = lz.transform.position;
            
                // Track position influence
                Vector3 endLzPos = lzPoz;
            
                // Get valid movement axis
                Vector3[] dirs = ((ISnapToBounds)lz).GetValidMovementAxes(lz.BColl.bounds);

                // Draw handles using dirs
                for (var i = 0; i < dirs.Length; i++)
                {
                    // Set handles color
                    if (dirs[i].magnitude == 0)
                    {
                        UnityEditor.Handles.color = Color.clear;
                    }
                    else if (Vector3.Angle(dirs[i], Vector3.up) < 1)
                    {
                        UnityEditor.Handles.color = Color.green;
                    }
                    else if (Vector3.Angle(dirs[i], Vector3.right) < 1)
                    {
                        UnityEditor.Handles.color = Color.red;
                    }
                    else // Shouldn't happen, magenta indicates error
                    {
                        UnityEditor.Handles.color = Color.magenta;
                    }

                    // Modify dir if needed
                    if (lzPoz.x > pos.x)
                    {
                        dirs[i].x *= -1;
                    }
                    if (lzPoz.y > pos.y)
                    {
                        dirs[i].y *= -1;
                    }

                    // Draw handle and add influence to end position
                    endLzPos += UnityEditor.Handles.Slider(lzPoz, dirs[i]) - lzPoz;
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
            
                // Get valid movement axis
                Vector3[] dirs = lze.GetValidMovementAxes();

                // Draw handles using dirs
                for (var i = 0; i < dirs.Length; i++)
                {
                    // Set handles color
                    if (dirs[i].magnitude == 0)
                    {
                        UnityEditor.Handles.color = Color.clear;
                    }
                    else if (Vector3.Angle(dirs[i], Vector3.up) < 1)
                    {
                        UnityEditor.Handles.color = Color.green;
                    }
                    else if (Vector3.Angle(dirs[i], Vector3.right) < 1)
                    {
                        UnityEditor.Handles.color = Color.red;
                    }
                    else // Shouldn't happen, magenta indicates error
                    {
                        UnityEditor.Handles.color = Color.magenta;
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
                    endLzePos += UnityEditor.Handles.Slider(lzePos, dirs[i]) - lzePos;
                }

                // Set new level zone entrance position to calculated end position 
                lze.transform.position = endLzePos;
            }
        }
    }
}

#endif