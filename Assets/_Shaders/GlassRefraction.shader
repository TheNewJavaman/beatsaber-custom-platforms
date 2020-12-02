Shader "BeatSaber/GlassRefraction" {
	Properties {
		_MainTex ("Main Tex", 2D) = "white"{}  //基础纹理
		_BumpMap ("Normal Map", 2D) = "bump"{}  //法线纹理
		_Cubemap ("Environment Cubemap", Cube) = "_Skybox"{}  //立方体纹理
		_Distortion ("Distortion", Range(0, 1000)) = 100  //控制模拟折射时图像的扭曲程度
		_RefractAmount ("Refraction Amount", Range(0, 1)) = 1  //控制折射程度
	}
 
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Opaque" }
        Blend SrcAlpha One
		ColorMask RGB
		Cull Off
        ZWrite Off
 
		GrabPass { "_RefractionTex" }
 
		pass {
			CGPROGRAM
 
			#pragma vertex vert
			#pragma fragment frag
 
			#include "UnityCG.cginc"
 
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _BumpMap;
			float4 _BumpMap_ST;
			samplerCUBE _Cubemap;
			float _Distortion;
			fixed _RefractAmount;
			sampler2D _RefractionTex;
			float4 _RefractionTex_TexelSize;
 
			struct a2v {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
			};
 
			struct v2f {
				float4 pos : SV_POSITION;
				float4 scrPos : TEXCOORD0;
				float4 uv : TEXCOORD1;
				float4 TtoW0 : TEXCOORD2;
				float4 TtoW1 : TEXCOORD3;
				float4 TtoW2 : TEXCOORD4;
			};
 
			v2f vert(a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				//得到对应被抓取的屏幕图像的采样坐标
				o.scrPos = ComputeGrabScreenPos(o.pos);
 
				o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv.zw = TRANSFORM_TEX(v.texcoord, _BumpMap);
 
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
				fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;
 
				//切线空间到世界空间的变换矩阵
				o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
 
				return o;
			}
 
			fixed4 frag(v2f i) : SV_Target {
				float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
 
				fixed3 bump = UnpackNormal(tex2D(_BumpMap, i.uv.zw));
 
				//对屏幕图像的采样坐标进行偏移
				//选择使用切线空间下的法线方向来进行偏移是因为该空间下的法线可以反映顶点局部空间下的法线方向
				fixed2 offset = bump * _Distortion * _RefractionTex_TexelSize;
				//对scrPos偏移后再透视除法得到真正的屏幕坐标
				i.scrPos.xy = (offset + i.scrPos.xy) / i.scrPos.w;
				//折射颜色
				fixed3 refrColor = tex2D(_RefractionTex, i.scrPos.xy).rgb;
 
				bump = normalize(half3(dot(i.TtoW0.xyz, bump), dot(i.TtoW1.xyz, bump), dot(i.TtoW2.xyz, bump)));
				//计算反射方向
				fixed3 reflDir = reflect(-worldViewDir, bump);
				//基础纹理颜色
				fixed3 texColor = tex2D(_MainTex, i.uv.xy).rgb;
				//反射颜色
				fixed3 reflColor = texCUBE(_Cubemap, reflDir).rgb * texColor;
 
				//使用_RefractAmount混合反射颜色和折射颜色
				fixed3 finalColor = lerp(reflColor, refrColor, _RefractAmount);
 
				return fixed4(finalColor, 1.0);
			}
 
			ENDCG
		}
	}
 
	FallBack "Diffuse"
}