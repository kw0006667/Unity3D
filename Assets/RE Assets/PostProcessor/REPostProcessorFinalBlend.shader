// Upgrade NOTE: replaced 'texRECT' with 'tex2D'

// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'
// Upgrade NOTE: replaced 'glstate.matrix.texture[0]' with 'UNITY_MATRIX_TEXTURE0'
// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'
// Upgrade NOTE: replaced 'texRECT' with 'tex2D'

Shader "Hidden/Robs Final Blend" {

/*
This takes the blended blur image and the original source, and blends them, and uses the iris adjust if available.  The bulk of the iris adjust functionality could
be moved to the 1 pixel iris adjust shader, but I'm not in the mood now, and it's still fairly fast.  The color correction matrix also gets applied here.
*/

Properties {
	_Color ("Color", color) = (1,1,1,0)
	_MainTex ("", RECT) = "black" {}
	_FinalBlur ("", RECT) = "red" {}
	_HDRIrisAdjustImage ("", RECT) = "white" {}
}


CGINCLUDE
#include "UnityCG.cginc"

struct v2f {
	float4 pos : POSITION;
	float4 uv: TEXCOORD0;
};

struct a2v
    {
       float4 vertex : POSITION;
       float3 normal : NORMAL;
       float4 texcoord : TEXCOORD0;
    };
        
float4 _MainTex_TexelSize;

v2f vert (a2v v)
{
	v2f o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	o.uv.xy = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
	o.uv.zw = 0;
	float offX = _MainTex_TexelSize.x;
	float offY = _MainTex_TexelSize.y;
	
	// Direct3D9 needs some texel offset!
	#ifdef SHADER_API_D3D9
//	o.uv.x -= offX;  // This will shift everything 1 pixel to right.
//	o.uv.y += offY;  // For some reason, this is what I need to get into the right place.
	#endif
		
	o.uv.zw = TRANSFORM_UV(0).xy;

	return o;
}
ENDCG



Category {
	ZTest Always Cull Off ZWrite Off Fog { Mode Off }
	
	// -----------------------------------------------------------
	// ARB fragment program
	
	Subshader { 
		Pass {
			
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
//#pragma target 3.0 

sampler2D _MainTex;
sampler2D _FinalBlur;
sampler2D _HDRIrisAdjustImage;
float4 _Color;

float _HDRIrisAdjustBrightenFactor;
float _HDRIrisAdjustDarkenFactor;
float _HDRIrisAdjustMedianIntensity;
float4x4 _ColorCorrectionMatrix;

half4 frag( v2f i ) : COLOR
{
	half4 c;
	half4 blurColor;
	float4 irisAdjustColor;
	float irisAdjust;
	
	c = tex2D( _MainTex, i.uv.xy);
	blurColor=tex2D( _FinalBlur, i.uv.zw);
	c.xyz=(c.xyz*blurColor.w);  // We're putting a value into the blur image's alpha that indicates how much the original color should be blended (for fucus).  This could be done simpler, but it's not bad, this allows the blur/focus stuff to be done in 2X smaller images, bit faster.
//	c=0;
	c.xyz+=blurColor.xyz;
	
	c.w=1;
	c=mul(_ColorCorrectionMatrix,c);
	
	irisAdjustColor=tex2D( _HDRIrisAdjustImage,half2(0,0));
	irisAdjust=dot(half3(0.222, 0.707, 0.071), irisAdjustColor.xyz);
//	irisAdjust=irisAdjustColor.x+irisAdjustColor.y+irisAdjustColor.z;
	float adjustOffset=_HDRIrisAdjustMedianIntensity-irisAdjust;
	
	if (irisAdjust<_HDRIrisAdjustMedianIntensity)
		{
		irisAdjust=1.0+((adjustOffset)*_HDRIrisAdjustBrightenFactor);
//		irisAdjust=1;
		}
	else
		{
		irisAdjust=1.0/pow(1.0-adjustOffset,_HDRIrisAdjustDarkenFactor);
//		irisAdjust=1;
		}
		
	
//	if (_HDRIrisAdjustDarkenFactor>0)
		c.xyz*=irisAdjust;
	
//	if (i.uv.x<.5) c.xyz = blurColor.xyz;  // Visualize final downsampled buffer color as a bar on left side of screen.
//	if (i.uv.x<.05) c.xyz = irisAdjustColor;  // Visualize final downsampled buffer color as a bar on left side of screen.
//	if (i.uv.x>.95) c.xyz = irisAdjust*.5;  // Visualize downsample multiplier as bar on right side of screen, where 50% grey is full bright.
		
//	if (i.uv.x>.5) c = tex2D( _MainTex, i.uv.xy);  // Visualize unmodified buffer on right side of screen.
		
	return c;
}
ENDCG


		}
	}
			
}

Fallback off

}


