Shader "Hidden/Robs Blur Combine For Radial Focus" {

/*
This is different from the normal bloom creator because it generates pixels that represent the blured image for focus effects.  Because of a slightly
different type of math between the two blur combine methods, it's best to activate the focus shader all the time if you are using focus changes that 
transition from 0 intensity to any other, the look is slightly different.

Note, right now this will not properly fall back to SM2, I think the branching and such are just balooning the methods too much, I could likely do the
focus effects in two parts, one render an image with each channel representing a multiplier for the final blur images.  Although that code would have the
same branching issues as this, it likely would not end up running out of instructions since all it'd be doing is setting up the blur buffer.  On the other hand, 
focus effects are almost assuredly something that you can do without, so I'm not in a hurry to bust my hump making it work on old cards.

With SM2, it can only sample 3 maps before running out of instructions.  Maybe I'll think of a simpler way of handling the blur map lookups, what I'm doing
now does seem more complex than it could be.
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

Category
	{
	ZTest Always Cull Off ZWrite Off Fog { Mode Off }
	SubShader
		{
		Pass
			{

CGPROGRAM
#include "PostProcessor.txt"
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag
	
half4 frag( v2f i ) : COLOR
{
	float pixelBlur=length(i.uv.xy-.5);  // Screen center radial blur method.
	pixelBlur=pow(pixelBlur,_FocusIntensityPow)*_FocusOverallIntensity*16;
	return getPixelBasedOnBlurLevel(i,pixelBlur);
}
ENDCG
			}
		}
		
	SubShader
		{
		Pass
			{

CGPROGRAM
#include "PostProcessor.txt"
#pragma vertex vert
#pragma fragment frag
	
half4 frag( v2f i ) : COLOR
{
	float pixelBlur=length(i.uv.xy-.5);  // Screen center radial blur method.
	
	// Hehe, one instruction too many, oh well, no Pow for you!
//	pixelBlur=pow(pixelBlur,_FocusIntensityPow)*_FocusOverallIntensity*16;
	pixelBlur=pixelBlur*_FocusOverallIntensity*16;
	return getPixelBasedOnBlurLevelSM2(i,pixelBlur);
}
ENDCG
			}
		}
		
Fallback off
	}
}
			


