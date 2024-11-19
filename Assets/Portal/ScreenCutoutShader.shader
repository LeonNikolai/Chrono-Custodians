Shader "Unlit/ScreenCutoutShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EmissionMap ("Emission Map", 2D) = "white" {} // New Emission Map
        _EmissionColor ("Emission Color", Color) = (1, 1, 1, 1) // Color multiplier for emission
        _Emission ("Emission Intensity", Float) = 1.0 // Intensity for the emission effect
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Lighting Off
        Cull Back
        ZWrite On
        ZTest Less
        Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
                float2 uv : TEXCOORD0; // Pass UV for emission map
            };

            sampler2D _MainTex;
            sampler2D _EmissionMap; // Emission map
            float4 _EmissionColor;  // Emission color
            float _Emission;        // Emission intensity

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = v.uv; // Pass UV for texture sampling
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                i.screenPos /= i.screenPos.w;

                // Sample main texture
                fixed4 col = tex2D(_MainTex, i.screenPos.xy);

                // Sample emission map
                fixed4 emissionMap = tex2D(_EmissionMap, i.uv);

                // Combine emission map with color and intensity
                fixed3 emission = emissionMap.rgb * _EmissionColor.rgb * _Emission;

                // Add emission to the final color
                col.rgb += emission;

                return col;
            }
            ENDCG
        }
    }
}
