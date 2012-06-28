Shader "Hidden/Robs Blur Combine For Constant Focus" {


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
	return getPixelBasedOnBlurLevel(i,_FocusOverallIntensity); // Constant blur method.
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
	return getPixelBasedOnBlurLevelSM2(i,_FocusOverallIntensity); // Constant blur method.
}
ENDCG
			}
		}
		
//	SubShader
//		{
//		UsePass "Hidden/Robs Blur Combine/BASE"
//		}
Fallback off
	}
}
			


