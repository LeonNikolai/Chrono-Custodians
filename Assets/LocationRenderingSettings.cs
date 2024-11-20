using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Chrono Custodians/LocationSettings")]
public class LocationRenderingSettings : ScriptableObject
{
    [Header("Sun Angle")]
    [SerializeField] Vector3 sunAngle = new Vector3(50, 330, 0);
    [SerializeField] float sunIntensity = 1;
    [SerializeField] Color sunColor = Color.white;


    [Header("Skybox")]
    [SerializeField] Material skyboxMaterial;


    [Header("Ambience")]
    [SerializeField] float ambientIntensity = 1;
    [SerializeField] AmbientMode ambientMode = AmbientMode.Skybox;
    [SerializeField] Color ambientLightColor = Color.white;
    [SerializeField] Color ambientEquatorColor = Color.white;
    [SerializeField] Color ambientGroundColor = Color.white;
    [SerializeField] Color ambientSkyColor = Color.white;

    [Header("Reflection")]
    [SerializeField] float reflectionIntensity = 1;
    [SerializeField] float haloStrength = 0.5f;

    [Header("Shadow")]
    [SerializeField] Color subtractiveShadowColor = Color.black;

    [Header("Reflection")]
    [SerializeField] DefaultReflectionMode reflectionMode = DefaultReflectionMode.Skybox;
    [SerializeField] int reflectionResolution = 256;
    [SerializeField] int reflectionBounces = 1;


    [Header("Fog")]
    [SerializeField] bool fogEnabled = true;
    [SerializeField] Color fogColor = Color.white;
    [SerializeField] FogMode fogMode = FogMode.Linear;
    [SerializeField] float fogStartDistance = 100;
    [SerializeField] float fogEndDistance = 300;


    [ContextMenu("Get Current")]
    public void GetCurrent()
    {
        // Sun
        sunAngle = RenderSettings.sun.transform.rotation.eulerAngles;
        sunIntensity = RenderSettings.sun.intensity;
        sunColor = RenderSettings.sun.color;

        // Skybox
        skyboxMaterial = RenderSettings.skybox;
        haloStrength = RenderSettings.haloStrength;
        subtractiveShadowColor = RenderSettings.subtractiveShadowColor;

        // Ambient Light    
        ambientIntensity = RenderSettings.ambientIntensity;
        ambientGroundColor = RenderSettings.ambientGroundColor;
        ambientEquatorColor = RenderSettings.ambientEquatorColor;
        ambientSkyColor = RenderSettings.ambientSkyColor;
        ambientLightColor = RenderSettings.ambientLight;
        ambientMode = RenderSettings.ambientMode;


        // Reflection
        reflectionIntensity = RenderSettings.reflectionIntensity;
        reflectionMode = RenderSettings.defaultReflectionMode;
        reflectionResolution = RenderSettings.defaultReflectionResolution;
        reflectionBounces = RenderSettings.reflectionBounces;

        // Fog
        fogEnabled = RenderSettings.fog;
        fogColor = RenderSettings.fogColor;
        fogStartDistance = RenderSettings.fogStartDistance;
        fogEndDistance = RenderSettings.fogEndDistance;
        fogMode = RenderSettings.fogMode;
    }

    public void Apply()
    {
        // Sun
        RenderSettings.sun.transform.rotation = Quaternion.Euler(sunAngle);
        RenderSettings.sun.color = sunColor;
        RenderSettings.sun.intensity = sunIntensity;

        // Skybox
        RenderSettings.skybox = skyboxMaterial;
        RenderSettings.ambientIntensity = ambientIntensity;
        RenderSettings.haloStrength = haloStrength;

        // Reflection
        RenderSettings.reflectionIntensity = reflectionIntensity;
        RenderSettings.defaultReflectionMode = reflectionMode;
        RenderSettings.defaultReflectionResolution = 256;
        RenderSettings.reflectionBounces = reflectionBounces;

        // Fog
        RenderSettings.fog = fogEnabled;
        if (fogEnabled)
        {
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
            RenderSettings.fogMode = fogMode;
        }

        // Shadow
        RenderSettings.subtractiveShadowColor = subtractiveShadowColor;

        // Ambient Light
        RenderSettings.ambientMode = ambientMode;
        RenderSettings.ambientLight = ambientLightColor;
        RenderSettings.ambientEquatorColor = ambientEquatorColor;
        RenderSettings.ambientGroundColor = ambientGroundColor;
        RenderSettings.ambientSkyColor = ambientSkyColor;
    }
    public LocationRenderingSettingsRefference NetworkRefference() => new LocationRenderingSettingsRefference(this);
}