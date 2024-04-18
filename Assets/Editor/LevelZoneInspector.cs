using UnityEditor;
using UnityEngine.UIElements;

#if UNITY_EDITOR

[CustomEditor(typeof(LevelZone))]
public class LevelZoneInspector : Editor
{
    public VisualTreeAsset uxml;
    private VisualElement rootElement;
    private EnumField ef;
    private FloatField ff;

    public override VisualElement CreateInspectorGUI()
    {
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
}

#endif