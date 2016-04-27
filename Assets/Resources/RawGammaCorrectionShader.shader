Shader "Kinect/RawGammaCorrectionShader"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "black" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma target 5.0
			
			#include "UnityCG.cginc"

			uniform float _TexResX;
			uniform float _TexResY;
			uniform float _Amplification;
			uniform float _Gamma;

			StructuredBuffer<float> _Buffer;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;

			float4 frag (v2f i) : SV_Target
			{
				//compute buffer pos, get color and return it
				int x = (int)(i.uv.x * _TexResX);
				int y = (int)(i.uv.y * _TexResY);
				int index = y * _TexResX + x;

				float normalizedColor = _Buffer[index] / (float)0xFFFF;
				float color = _Amplification * pow(normalizedColor, _Gamma);

				return float4(color, color, color, 1);
			}
			ENDCG
		}
	}
}
