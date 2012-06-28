// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'
// Upgrade NOTE: replaced 'glstate.matrix.texture[0]' with 'UNITY_MATRIX_TEXTURE0'
// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'
// Upgrade NOTE: replaced 'texRECT' with 'tex2D'

Shader "Hidden/Robs Glow Downsample" {

/*
2x2 downsampler, that works with SM2.  This should give supperior quality, and likely not much of a speed hit than the 4x4.
*/

Properties {
	_MainTex ("", RECT) = "white" {}
	_BrightFactor ("", float) = 1
}

CGINCLUDE
#include "UnityCG.cginc"

struct v2f {
	float4 pos : POSITION;
	float4 uv[4] : TEXCOORD0;
};

float4 _MainTex_TexelSize;

v2f vert (appdata_img v)
{
	v2f o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	float4 uv;
	uv.xy = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
	uv.zw = 0;
	float offX = _MainTex_TexelSize.x;
	float offY = _MainTex_TexelSize.y;
	
	// Direct3D9 needs some texel offset!
	#ifdef SHADER_API_D3D9
	// Seemingly no longer needed in U3, Yipee!
//	uv.x += offX * 1.0f;
//	uv.y += offY * 1.0f;
	#endif
	o.uv[0] = uv + float4(-offX,-offY,0,1);
	o.uv[1] = uv + float4( offX,-offY,0,1);
	o.uv[2] = uv + float4( offX, offY,0,1);
	o.uv[3] = uv + float4(-offX, offY,0,1);
	return o;
}
ENDCG


Category {
	ZTest Always Cull Off ZWrite Off Fog { Mode Off }
	
	// -----------------------------------------------------------
	// ARB fragment program
	
	Subshader { 
		Pass {
			Blend One Zero
		
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
//#pragma target 3.0 

sampler2D _MainTex;
half _BrightFactor;

half4 frag( v2f i ) : COLOR
{
	half4 c;
	c  = tex2D( _MainTex, i.uv[0].xy );
	c += tex2D( _MainTex, i.uv[1].xy );
	c += tex2D( _MainTex, i.uv[2].xy );
	c += tex2D( _MainTex, i.uv[3].xy );
	c /= 4;
//	c.w-=.0001;
	c.xyz *= _BrightFactor;
	return c;
}
ENDCG

		}
	}
			
}

Fallback off

}


