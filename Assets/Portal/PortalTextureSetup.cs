using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PortalTextureSetup : MonoBehaviour {

	public Camera cameraA;
	public Camera cameraB;

	public CommandBuffer commandBufferA;
	public CommandBuffer commandBufferB;

	public Material cameraMatA;
	public RenderTexture postProcessTexA;
	public RenderTexture postProcessTexB;
	public Material cameraMatB;

	// Use this for initialization
	void Start () 
	{

        postProcessTexA = new RenderTexture(Screen.width, Screen.height, 24);
        cameraMatA.SetTexture("_MainTex", postProcessTexA);

        commandBufferA = new CommandBuffer();
        commandBufferA.Blit(BuiltinRenderTextureType.CurrentActive, postProcessTexA);
        cameraA.AddCommandBuffer(CameraEvent.AfterImageEffects, commandBufferA);
        if (cameraA.targetTexture != null)
		{
			cameraA.targetTexture.Release();
		}
		cameraMatA.mainTexture = cameraA.targetTexture;


        postProcessTexB = new RenderTexture(Screen.width, Screen.height, 24);
        cameraMatB.SetTexture("_MainTex", postProcessTexB);

        commandBufferB = new CommandBuffer();
        commandBufferB.Blit(BuiltinRenderTextureType.CurrentActive, postProcessTexB);
        cameraA.AddCommandBuffer(CameraEvent.AfterImageEffects, commandBufferB);
        if (cameraB.targetTexture != null)
		{
			cameraB.targetTexture.Release();
		}
		cameraMatB.mainTexture = cameraB.targetTexture;
	}
	
}