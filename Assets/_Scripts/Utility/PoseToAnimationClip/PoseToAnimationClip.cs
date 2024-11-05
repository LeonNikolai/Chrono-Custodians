using UnityEngine;
using UnityEditor;

public class PoseToAnimationClip : MonoBehaviour
{
    public Transform rootBone; // The root of your rigged model
    public string animationSavePath = "Assets/Animations/";
    public string prefix = "";
    public string animationName = "";
    public string subfix = "";

    public void CreateAnimationClip()
    {
        if (rootBone == null)
        {
            Debug.LogError("Root bone not assigned!");
            return;
        }

        // Get all child transforms
        Transform[] allBones = rootBone.GetComponentsInChildren<Transform>();
        AnimationClip animationClip = new AnimationClip();
        animationClip.legacy = false;

        // Create animation curves for each bone's local rotation
        foreach (Transform bone in allBones)
        {
            string bonePath = GetBonePath(rootBone, bone);

            AnimationCurve curveX = AnimationCurve.Constant(0, 0, bone.localRotation.x);
            AnimationCurve curveY = AnimationCurve.Constant(0, 0, bone.localRotation.y);
            AnimationCurve curveZ = AnimationCurve.Constant(0, 0, bone.localRotation.z);
            AnimationCurve curveW = AnimationCurve.Constant(0, 0, bone.localRotation.w);

            animationClip.SetCurve(bonePath, typeof(Transform), "localRotation.x", curveX);
            animationClip.SetCurve(bonePath, typeof(Transform), "localRotation.y", curveY);
            animationClip.SetCurve(bonePath, typeof(Transform), "localRotation.z", curveZ);
            animationClip.SetCurve(bonePath, typeof(Transform), "localRotation.w", curveW);
        }

        // Save the animation clip as an asset
        AssetDatabase.CreateAsset(animationClip, animationSavePath + prefix + animationName + subfix +".anim");
        AssetDatabase.SaveAssets();

        Debug.Log("Animation clip created and saved at: " + animationSavePath);
    }

    private string GetBonePath(Transform root, Transform bone)
    {
        string path = bone.name;
        Transform current = bone;

        while (current != root && current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }
}
