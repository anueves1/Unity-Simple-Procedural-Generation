// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'

Shader "Custom/SurvivalKit/Water"
{
	Properties
	{
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Transparency ("Transparency", Float) = 0
        //_Refraction ("Refraction", Float) = 0
        //_VerticeDisplacement("Displacement", vector) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "LightMode"="ForwardBase" }
		LOD 100

		Pass
		{
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;

                float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;

                float3 worldRefl : TEXCOORD1;
                //float3 worldRefrac : TEXCOORD2;
                float4 diff : COLOR0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

            sampler2D _GrabTexture;

            float4 _Color;
            float _Transparency;

            //float _Refraction;
            //float4 _VerticeDisplacement;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half3 worldPos = mul(unity_ObjectToWorld, v.vertex);

                half3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                o.worldRefl = reflect(-viewDir, worldNormal);

                //Calculate refraction.
                //o.worldRefrac = refract(viewDir, worldNormal, 1 / _Refraction);

                half nL = max(0, dot(worldNormal, _WorldSpaceLightPos0));
                o.diff = nL * _LightColor0;

                o.diff.rgb += ShadeSH9(half4(worldNormal, 1));

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.worldRefl);
                half3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);

                //half4 refractData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.worldRefrac);
                //half3 refractColor = DecodeHDR(refractData, unity_SpecCube0_HDR);

                fixed4 l = 0;
                //l.rgb = skyColor * i.diff * _Color * refractColor;
                l.rgb = skyColor * i.diff * _Color;

                // apply fog
				UNITY_APPLY_FOG(i.fogCoord, l);
                l.a = _Transparency;
                return l;
			}
			ENDCG
		}
	}
}
