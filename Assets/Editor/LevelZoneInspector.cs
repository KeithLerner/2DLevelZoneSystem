using UnityEngine;
using UnityEngine.UIElements;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(LevelZone))]
public class LevelZoneInspector : UnityEditor.Editor
{
    public VisualTreeAsset uxml;
    private VisualElement rootElement;
    private EnumField ef;
    private FloatField ff;

    private LevelZone lz;

    public override VisualElement CreateInspectorGUI()
    {
        // Get target as level zone.
        lz = target as LevelZone;
        
        // Create a new VisualElement to be the root of our Inspector UI.
        rootElement = new VisualElement();

        // Load from default reference.
        uxml.CloneTree(rootElement);
        
        // Hide cam offset if not supported by current scroll direction
        ef = rootElement.Q<EnumField>("ScrollDirection");
        ff = rootElement.Q<FloatField>("CameraOffset");
        
        ef.RegisterCallback<ChangeEvent<string>>(evt =>
        {
            LevelZone.ScrollDirection.TryParse(evt.newValue, out LevelZone.ScrollDirection valueInt);
            ff.visible = LevelZone.DoesCameraOffset(valueInt);
        });

        // Return the finished Inspector UI.
        return rootElement;
    }

    private void OnSceneGUI()
    {
        // Get level zone position 
        Vector3 pos = lz.transform.position;
        
        foreach (LevelZoneEntrance lze in lz.transform.GetComponentsInChildren<LevelZoneEntrance>())
        {
            // Get a level zone entrance position
            Vector3 lzePos = lze.transform.position;
            
            // Get valid movement axis
            Vector3[] dirs = lze.GetValidMovementAxes();
            
            // Draw handles using dirs
            for (var i = 0; i < dirs.Length; i++)
            {
                if (Vector3.Angle(dirs[i], Vector3.zero) > 1)
                {
                    continue;
                }
                
                // Set handles color
                if (Vector3.Angle(dirs[i], Vector3.up) < 1)
                {
                    UnityEditor.Handles.color = Color.green;
                }
                else if (Vector3.Angle(dirs[i], Vector3.right) < 1)
                {
                    UnityEditor.Handles.color = Color.red;
                }
                else // Shouldn't happen
                {
                    UnityEditor.Handles.color = Color.magenta;
                }

                // Modify dir if needed
                if (lzePos.x > pos.x)
                {
                    dirs[i].y *= -1;
                }
                if (lzePos.y > pos.y)
                {
                    dirs[i].y *= -1;
                }
                
                // Draw handle linked to position
                lze.transform.position = 
                    UnityEditor.Handles.Slider(lzePos, dirs[i]);
            }
            
            // Round position
            lze.RoundPositionToOwningCameraBounds();
        }
    }
}

#endif