Shader "Unlit/Loading"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_RotateSpeed("Rotate Speed", Range(1, 200)) = 100
	}
	
	SubShader
	{
		tags{"Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True"}
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass
		{
			Name "Simple"
			Cull off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			float4 _Color;
			sampler2D _MainTex;
			float _RotateSpeed;
			
			struct v2f
			{
				float4 pos:POSITION;
				float4 uv:TEXCOORD0;
			};
			
			v2f vert(appdata_base v)
			{
				v2f o;
				//将物体坐标转化为剪裁坐标（顶点坐标转换：物体坐标->世界坐标->观察坐标->剪裁左边）
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}
			
			half4 frag(v2f i):COLOR
			{
				//以纹理中心为旋转中心
				float2 uv = i.uv.xy - float2(0.5, 0.5);
 
				//旋转矩阵公式
				uv = float2(uv.x * cos(_RotateSpeed * _Time.x) - uv.y * sin(_RotateSpeed * _Time.x),
							uv.x * sin(_RotateSpeed * _Time.x) + uv.y * cos(_RotateSpeed * _Time.x));
				
				//恢复纹理位置
				uv += float2(0.5, 0.5);
				
				half4 c = tex2D(_MainTex , uv) * _Color;
				return c;
			}
			
			ENDCG
		}
	}
}