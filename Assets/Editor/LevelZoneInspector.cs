using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(LevelZone))]
public class LevelZoneInspector : Editor
{
    public VisualTreeAsset uxml;

    public override VisualElement CreateInspectorGUI()
    {
        // Create a new VisualElement to be the root of our Inspector UI.
        VisualElement rootElement = new VisualElement();

        // Load from default reference.
        uxml.CloneTree(rootElement);

        // Return the finished Inspector UI.
        return rootElement;
    }
}