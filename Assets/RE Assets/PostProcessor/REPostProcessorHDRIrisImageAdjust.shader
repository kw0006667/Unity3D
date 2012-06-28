// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'
// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'
// Upgrade NOTE: replaced 'texRECT' with 'tex2D'

Shader "Hidden/Robs HDR Iris image adjust" {

/*
This simply takes the last HDR value and blends it with the new one, so that each frame we get a slow adjustment instead of an instant correction to the new color.
*/

Properties {
	_MainTex ("", RECT) = "white" {}
	_IrisAdjust ("_IrisAdjust", RECT) = "white" {}
	_HDRIrisAdjustAdjustSpeed ("", float) = .01
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
       float4 texcoord : TEXCOORD0;
    };
        
v2f vert (a2v v)
{
	v2f o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	o.uv.zw=0;
	o.uv.xy = TRANSFORM_UV(0).xy;

	return o;
}
ENDCG



Category {
		
         	
	// -----------------------------------------------------------
	// ARB fragment program
	ZTest Always Cull Off ZWrite Off Fog { Mode Off }
	
	Subshader { 
		Pass {
CGPROGRAM
#pragma target 3.0 
#pragma vertex vert
#pragma fragment frag

sampler2D _MainTex;
sampler2D _IrisAdjust;
float _HDRIrisAdjustAdjustSpeed;

half4 frag( v2f i ) : COLOR
{
	float4 c;
	float4 c2;
	
	float factor=1;

	c = tex2D( _MainTex, i.uv.xy);
	c2 = tex2D( _IrisAdjust, i.uv.xy);
//	c.xyz*=c.w;
	c*=(1.0-_HDRIrisAdjustAdjustSpeed);
	c2=c2*(_HDRIrisAdjustAdjustSpeed/factor);  // Adjust incomming value by the factor, so that it's not in normalized space.
	c.xyz+=c2.xyz;
		
//	c.xyz=clamp(c.xyz,0,1);
//	c.xyz=.5;
	
//	c.xyz=c2.xyz*c2.w;
//	c = texRECT( _MainTex, half2(0,0));
//	c.xyz*=c.w;
//	c.xyz+=.5;
	c.w=1;
	
//	c = texRECT( _MainTex, i.uv.xy);
//	c = texRECT( _IrisAdjust, i.uv.xy);
	
	return c;
}
ENDCG


		}
	}
			
}

Fallback off

}


