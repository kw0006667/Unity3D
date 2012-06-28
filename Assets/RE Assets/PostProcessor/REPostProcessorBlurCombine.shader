Shader "Hidden/Robs Blur Combine" {

/*
This is the most basic shader for combining blur images.  I couldn't find a way to share stuff effectively, there seems to be no way to check for a
pass's support, so I can't just smack all effects into one shader and switch passes accordingly.  So, all files with REPostProcessorBlurCombine shoult
share the same properties, but handle a different setting for focus mehtod.
*/


// Can't find a way to include into a shader, so sadly looks like a bunch of copy/paste :(
Properties {
	_Blur1 ("_Blur1", RECT) = "red" {}
	_Blur2 ("_Blur2", RECT) = "red" {}
	_Blur3 ("_Blur3", RECT) = "red" {}
	_Blur4 ("_Blur4", RECT) = "red" {}
	_Blur5 ("_Blur5", RECT) = "red" {}
	_Blur6 ("_Blur6", RECT) = "red" {}
	_Blur7 ("_Blur7", RECT) = "red" {}
//	_Blur8 ("_Blur8", RECT) = "red" {}
	_BlurCombineFinalIntensityMult ("", float) = 1
	_BlurCombineFinalIntensitySubtract ("", float) = 0
	_BlurCombineFinalIntensityMidrange ("", float) = .5
	_BlurCombineFinalIntensityPow ("", float) = 1
	_BlurTint ("", color) =(1,1,1,0)
	_FocusOverallIntensity ("", float) = 10
	_FocusIntensityPow ("", float) = 1
	_FocusImageStepDecay("", float) = 1
}

CGINCLUDE
#include "PostProcessor.txt"
ENDCG

Category
	{
	ZTest Always Cull Off ZWrite Off Fog { Mode Off }
	
	// -----------------------------------------------------------
	// ARB fragment program
	
	Subshader
		{ 
		Pass
			{
			Name "BASE"
		
CGPROGRAM
#pragma vertex vert
#pragma fragment frag

half4 frag( v2f i ) : COLOR
{
	half4 blurColor;
	half4 ic;

	const float decayMult=.0;

	blurColor=tex2D( _Blur7, i.uv.xy);
	blurColor+=tex2D( _Blur6, i.uv.xy);
	blurColor+=tex2D( _Blur5, i.uv.xy);
	blurColor+=tex2D( _Blur4, i.uv.xy);
	blurColor+=tex2D( _Blur3, i.uv.xy);
	blurColor+=tex2D( _Blur2, i.uv.xy);
	blurColor+=tex2D( _Blur1, i.uv.xy);

	// We're going to normalize our color and intensity, so that our pow functions work in a way that doesn't make the eyes bleed.
	blurColor.xyz*=1.0/7;  // Normalize to the maximum color value.
//	blurColor.w=length(blurColor.xyz);
	blurColor.w=dot(half3(0.222, 0.707, 0.071), blurColor.xyz);
	blurColor.xyz/=blurColor.w;
	
	blurColor=applyBloomColoring(blurColor);
	blurColor=clamp(blurColor,0,100);

	blurColor.w=1;
//	blurColor.xyz*=.1;  // Gonna scale it down, which allows us to retain some detail at the top end.  Doesn't matter much here, but when better lighting information is supplied (HDR/deferred) it'll matter.
	
//	blurColor=1;
	return blurColor;
}
ENDCG


			}
		}
			
	}

Fallback off
}


