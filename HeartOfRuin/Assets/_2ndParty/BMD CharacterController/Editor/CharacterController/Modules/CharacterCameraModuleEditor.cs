using UnityEditor;
using UnityEngine;

namespace BMD
{
    //[CustomEditor(typeof(CharacterCameraModule))]
    //public class CharacterCameraModuleEditor : Editor
    //{
    //    void OnSceneGUI()
    //    {
    //        CharacterCameraModule cameraModule = (CharacterCameraModule)target;
    //        cameraModule.UpdateCamera();
    //    }

    //    public override void OnInspectorGUI()
    //    {
    //        DrawDefaultInspector();

    //        CharacterCameraModule cameraModule = (CharacterCameraModule)target;

    //        if (GUILayout.Button("Create Camera Focus Visualiser"))
    //        {
    //            CreateFocus(cameraModule);
    //        }

    //        cameraModule.UpdateCamera();
    //    }
    //    void CreateFocus(CharacterCameraModule cameraModule)
    //    {
    //        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //        sphere.name = "VISUALISER: Camera focus";

    //        Undo.RegisterCreatedObjectUndo(sphere, "Create Focus");

    //        sphere.transform.SetParent(cameraModule.transform);
    //        sphere.transform.localPosition = Vector3.forward * 2f;
    //        sphere.transform.localScale = Vector3.one * 0.25f;

    //        cameraModule.SetFocus(sphere.transform);
    //    }
    //}
}

