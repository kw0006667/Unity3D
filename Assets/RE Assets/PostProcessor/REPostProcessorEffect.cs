using UnityEngine;
using System.Collections;


/*
Copyright 2010 Rob Elam

This post processor is designed to smack together whatever features I wanna use that derive their results from the same source material.  In this case
the effects are going to be using a series of downsampled color maps.  So, anything I wanna do related to simple blurring will be handled here.

Bloom:
The bloom effect here is a bit different from the Unity glow effect in that it adds a few more options, but it's also a distinctly different approach
to the blur.  This approach can/will result in a more non-linear falloff from the bloom source pixels.  I haven't looked into it, and have no idea
which approach is a more realistic depiction of how blooms work with normal lenses, but I don't much care, I like the look and having a different method
can be useful :).

Focus Effect:
Currently, the focus effect is being handled along with the bloom.  The reason for this is that they both use the same source images, so it seems
more logical and efficient to do it that way.  The math looks a bit goofy, and maybe is, but the results work ok, you just have to make sure to
always be using the focus at some intensity if you're using it at all, there is a slight change in the way it looks when moving from 0 focus intensity
to anything >0.

Distance Focus mode (Depth of Field)
I was trying to get a depth of field effect going, but it turned out to be ill conceived.  I thought I could make the math work, using some of the data
I was already using for other things, and that really didn't work out.  Therefore, there's no real advantage to implimenting the DoF effect in this shader.
might be best implimented before this one anyways, it'd certainly be a bit expensive, but would accurately blur highlights and keep bloom effects from 
being too prominent in out of focus areas.  Gonna comment out the code, might think of some way to make it work later, but doubt it.
Note, while doing this, definately noticed that Unity needs to set the min/mag filter for the depth image to point, not linear.  Also need R16G16 render target available.

Color Correction:
This effect is, performance wise, virtually free.  After it's smacked into a matrix, it's one matrix multiply per pixel.  I might eventually move it
to it's own class, so that people could include it in their own shaders, even for drawing objects, having this color corection applied to the color
of an object in your scene could be pretty powerful.  Only problem is, currently Unity will not show classes inside a monobehaviour as animatable elements, 
so this kinda gimps it's usefulness if it's in a class (as a standalone monobehavior, it'd be good though).  Still, if anyone wants to use it, feel
free to rip it out the relevant code, it's likely got lots of other uses.

You may also see some multiplication of weird numbers when passing values to shaders.  This is generally due to trying to keep numbers in a more palateble
range that 'feels' right (mostly normalized values).

Improvements:
Still need to add the rendering of the focal map(s), so that the blur type math is not handled in the blur overlay builder.

Should add a few presets for applying effects to the blur maps.  Doing some radial blur on the downsampled maps, for instance, would be pretty cool
if combined with a radial focus blur.  This would be really nice in a racing game.

Might also be nice to add some directional blur options for the final blur image.  This would allow for horizontal lense flares, or perhaps multidirectional
flares.

Note, until true HDR data is supplied, there's not a good enough reason to be doing any of the blur stuff in FP16 textures.  I changed them to 8-bit, but
when the data is available, it'll be easy enough to add an option for turning on FP16 in blur downsamplers, so that high range data is preserved.

With the release of U3, I was considering adding support from the U3's bloom system to this, mostly to make it a bit faster, but I don't really think it'd help
that much with speed.  Since the two systems use different blurring methods, U3's bloom using a gaussian blur, and mine using a simpler blur that's essentially trilinear
filtering, they combine to give a whole lot of nice control over the final image.  Using this image effect first allows you to blur the result passed to U3's version, as well
as apply pow/multipliers and color correction to possibly accent or smooth out high color areas, and I'm quite pleased with the control allowed.  Only real shortcomming
is that if the render target were stored in FP16 when moving between these effects, then you could do overbright lighting effects.  Right now they just clamp too quickly,
but still, for the most part the overall effect is quite pleasing for the cost of running the two image effects.  With the FP16 buffer, the
only 'problem' left is that technically Unity's bloom would not be affecting the iris adjustment, but I seriously doubt anyone would ever know the difference.

I think the best overall solution would be to incorporate the ability to draw overbright passes into a FP16 buffer in my post processor, or into a seperate 8-bit one with a divisor, then 
blur/bloom the result into a FP16 or 8-bit buffer, depending on quality settings/availability.  As long as I can pass that to Unity's bloom image effect easily and still
allow Unity's bloom to work as it normally would, and be animatable, that'd be a powerful solution that wouldn't require a new spinnoff of Unity's bloom.  While Unity's lighting system doesn't
allow for overbright lighting to take advantage off this, doing emissive stuff that's really bright as well as super dramatic lens flares is something I'd definately like.
*/

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Robs Post Processor")]

public class REPostProcessorEffect : MonoBehaviour
	{
	public float BloomCombineIntensitySubtract = 1f;
	public float BloomCombineIntensityMultiply = 1;
	public float BloomCombineIntensityPow = 1;
	public float BloomCombineMidrange = .5f;
	public Color BloomTint = new Color(1, 1, 1, 1);

	public bool HDRIrisAdjust = false;
	public float HDRIrisAdjustBrightenFactor = 1;
	public float HDRIrisAdjustDarkenFactor = 1;
	public float HDRIrisAdjustMedianIntensity = .5f;
	public float HDRIrisAdjustSpeed = 1f;

	public Color ColorCorrectionTintRed = new Color(1, 0, 0, 1);
	public Color ColorCorrectionTintGreen = new Color(0, 1, 0, 1);
	public Color ColorCorrectionTintBlue = new Color(0, 0, 1, 1);
	public Color ColorCorrectionOverallColor = new Color(1, 1, 1, 0);
	public float ColorCorrectionSaturation = 1;
	public float ColorCorrectionMultiply = 1f;
	public float ColorCorrectionAdd = 0;
	public float ColorCorrectionContrastShift = 0;
	internal RenderTextureFormat CurrentTargetTypeForBlur;
	internal RenderTextureFormat CurrentTargetTypeForHighRange;

//	public bool GUIActive=false;
	bool GUICurrentlyDisabled;

	public enum EFocusMethod
		{
		None=0,
		Radial=1,
		Constant = 2,
//		Distance = 3,
		}

	public EFocusMethod FocusMethod;
	public float FocusIntensityPow = 1;  // This can be used to change how the blur interpolates along the range of however it's being computed.
	public float FocusOverallIntensity = 1;  // This is the visual 'intensity' of the blur, increase this and your image will blur to a greater degree faster.
	public bool FocusAutoDisable=true;  // Will automatically disable the effect when focus falls to <=0.  This is needed because there is a slight change in how the effect looks between normal and focus effects, so some might not like the effects of the visual change.
	//public float FocusDistance = 100;  // This is the distance at which the focus is accurate.
	//public float FocusDistanceBeforeBlur = 10;  // This is the distance at which the blur effect starts to do it's thing.
	/*public*/ bool HighQualityBlurMap=false;  // This can be set to true to get a better quality final map, mostly helps with distance based focus effects.  Disabled this, because removed distance option, it's pretty useless without that.

	public bool ShowBloomOnly = false;

	/*
	Gonna make copies of the original settings for the values, and set them at the start of Update().  That way, animations can be applied to the camera
	additively and it'd still retain it's initial settings, or with blended animations, it'll just waste a bit of time :).
	*/
	class InitialValues
		{
		internal float BloomCombineIntensitySubtract;
		internal float BloomCombineIntensityMultiply;
		internal float BloomCombineIntensityPow;
		internal float BloomCombineMidrange;
		internal Color BloomTint;
		internal Color ColorCorrectionTintRed;
		internal Color ColorCorrectionTintGreen;
		internal Color ColorCorrectionTintBlue;
		internal Color ColorCorrectionOverallColor;
		internal float ColorCorrectionSaturation;
		internal float ColorCorrectionMultiply;
		internal float ColorCorrectionAdd;
		internal float ColorCorrectionContrastShift;
		internal float FocusIntensityPow;
		internal float FocusOverallIntensity;
		}
	InitialValues Initial=new InitialValues();
	

	// For some odd reason, I was unnable to get any alpha blending going, so ended up having to do the iris adjust with an extra image, instead of blending/swapping.
	// No biggie, not gonna be a speed difference or anything.
	RenderTexture HDRIrisAdjustImage;
	RenderTexture HDRIrisAdjustImage1;
	RenderTexture HDRIrisAdjustImage2;


//	public Shader DownsampleShader;
	public Shader DownsampleShader2x2;
//	public Shader DownsampleShader2x2WithDepth;
	Material _DownsampleMaterial = null;
	protected Material DownsampleMaterial
		{
		get
			{
			if (_DownsampleMaterial == null)
				{
				_DownsampleMaterial = new Material(DownsampleShader2x2);
				_DownsampleMaterial.hideFlags = HideFlags.HideAndDontSave;
				}
			return _DownsampleMaterial;
			}
		}

/*
	Material _DownsampleMaterialWithDepth = null;
	protected Material DownsampleMaterialWithDepth
		{
		get
			{
			if (_DownsampleMaterialWithDepth == null)
				{
				_DownsampleMaterialWithDepth = new Material(DownsampleShader2x2WithDepth);
				_DownsampleMaterialWithDepth.hideFlags = HideFlags.HideAndDontSave;
				}
			return _DownsampleMaterialWithDepth;
			}
		}
*/

	/*
	Final downsampler, this applies the glow and the color correction matrix.
	*/

	public Shader FinalBlendShader;
	Material _FinalBlendShaderMaterial = null;
	protected Material FinalBlendShaderMaterial
		{
		get
			{
			if (_FinalBlendShaderMaterial == null)
				{
				_FinalBlendShaderMaterial = new Material(FinalBlendShader);
				_FinalBlendShaderMaterial.hideFlags = HideFlags.HideAndDontSave;
				}
			return _FinalBlendShaderMaterial;
			}
		}

	public Shader BlurCombineShader;
	public Shader BlurCombineShaderForRadialFocus;
	public Shader BlurCombineShaderForConstantFocus;
//	public Shader BlurCombineShaderForDistanceFocus;
	Shader BlurCombineShaderToUse
		{
		get
			{
			if (!FocusAutoDisable || FocusOverallIntensity > 0)
				{
				if (FocusMethod == EFocusMethod.Radial && BlurCombineShaderForRadialFocus.isSupported)
					return BlurCombineShaderForRadialFocus;
				if (FocusMethod == EFocusMethod.Constant && BlurCombineShaderForConstantFocus.isSupported)
					return BlurCombineShaderForConstantFocus;
				}
//			if (FocusMethod == EFocusMethod.Distance && BlurCombineShaderForDistanceFocus.isSupported)
//				return BlurCombineShaderForDistanceFocus;
			return BlurCombineShader;
			}
		}
	Material _BlurCombineShaderMaterial = null;

	protected Material BlurCombineShaderMaterial
		{
		get
			{
			if (_BlurCombineShaderMaterial && BlurCombineShaderToUse != _BlurCombineShaderMaterial.shader)
				_BlurCombineShaderMaterial = null;

			if (_BlurCombineShaderMaterial == null)
				{
//				Debug.Log("Recreating Blur Combine mat "+Time.frameCount);
				_BlurCombineShaderMaterial = new Material(BlurCombineShaderToUse);
				_BlurCombineShaderMaterial.hideFlags = HideFlags.HideAndDontSave;
				}
			return _BlurCombineShaderMaterial;
			}
		}

	public Shader HDRIrisAdjustShader;
	Material _HDRIrisAdjustShaderMaterial = null;
	protected Material HDRIrisAdjustShaderMaterial
		{
		get
			{
			if (_HDRIrisAdjustShaderMaterial == null)
				{
				_HDRIrisAdjustShaderMaterial = new Material(HDRIrisAdjustShader);
				_HDRIrisAdjustShaderMaterial.hideFlags = HideFlags.HideAndDontSave;
				}
			return _HDRIrisAdjustShaderMaterial;
			}
		}



	protected void OnDisable()
		{
		if (DownsampleMaterial)
			DestroyImmediate(DownsampleMaterial);
//		if (DownsampleMaterialWithDepth)
//			DestroyImmediate(DownsampleMaterialWithDepth);
		if (FinalBlendShaderMaterial)
			DestroyImmediate(FinalBlendShaderMaterial);
		}

	public Object PostProcessorInclude;  // Cause I'm not sure how else to make sure it's added to packages automatically.

	protected void Awake()
		{
        HDRIrisAdjustImage = new RenderTexture(1, 1, 0);
        HDRIrisAdjustImage1 = new RenderTexture(1, 1, 0);
        HDRIrisAdjustImage2 = new RenderTexture(1, 1, 0);
		}

	protected void Start()
		{
		CurrentTargetTypeForHighRange = RenderTextureFormat.ARGBHalf;
		CurrentTargetTypeForBlur = RenderTextureFormat.ARGB32;

//		if (FocusMethod == EFocusMethod.Distance)
//			{
			// No real clean way to get this info setup, so just gonna turn it on and leave it that way if distance is ever set.
			// Sadly, you have to use two different methods to obtain normals from shaders, forcing me to write two paths to handle it if done correcly.
			// Unity's depth/normals is an ugly hack.  For a while, I'm just gonna worry about getting mine to work, it might break other post processors.
			// This doesn't give me high hopes for anything other than a hack-tastic implimentation of the deferred renderer :(.
//			if (camera.depthTextureMode==DepthTextureMode.None)
//				camera.depthTextureMode = DepthTextureMode.Depth;
//			camera.depthTextureMode = DepthTextureMode.DepthNormals;
//			}

		HDRIrisAdjustImage.format = CurrentTargetTypeForHighRange;
		HDRIrisAdjustImage1.format = CurrentTargetTypeForHighRange;
		HDRIrisAdjustImage2.format = CurrentTargetTypeForHighRange;
		// Disable if we don't support image effects
		if (!SystemInfo.supportsImageEffects)
			{
			enabled = false;
			return;
			}

		if (!SystemInfo.SupportsRenderTextureFormat(CurrentTargetTypeForBlur) || !SystemInfo.SupportsRenderTextureFormat(CurrentTargetTypeForHighRange))
			{
			// For now, just gonna disable the thing completely.
			enabled = false;
			return;
			}

		// Disable the effect if no downsample shader is setup
		if (DownsampleShader2x2 == null || FinalBlendShader==null || BlurCombineShader==null)
			{
			Debug.Log("No downsample shader assigned! Disabling glow.");
			enabled = false;
			}
		// Disable if any of the shaders can't run on the users graphics card
		else
			{
			if (!DownsampleMaterial.shader.isSupported)
				{enabled = false;Debug.Log("DownsampleShader not supported");}
			if (!FinalBlendShaderMaterial.shader.isSupported)
				{enabled = false;Debug.Log("FinalBlendShader not supported");}
			if (!BlurCombineShaderMaterial.shader.isSupported)
				{enabled = false;Debug.Log("BlurCombineShader not supported");}
			}
		if (!enabled)
			Debug.Log("REPostProcessorEffect Disabled");

		Initial.BloomCombineIntensitySubtract = BloomCombineIntensitySubtract;
		Initial.BloomCombineIntensityMultiply = BloomCombineIntensityMultiply;
		Initial.BloomCombineIntensityPow=BloomCombineIntensityPow;
		Initial.BloomCombineMidrange = BloomCombineMidrange;
		Initial.BloomTint = BloomTint;
		Initial.ColorCorrectionTintRed = ColorCorrectionTintRed;
		Initial.ColorCorrectionTintGreen=ColorCorrectionTintGreen;
		Initial.ColorCorrectionTintBlue=ColorCorrectionTintBlue;
		Initial.ColorCorrectionOverallColor=ColorCorrectionOverallColor;
		Initial.ColorCorrectionSaturation=ColorCorrectionSaturation;
		Initial.ColorCorrectionMultiply=ColorCorrectionMultiply;
		Initial.ColorCorrectionAdd=ColorCorrectionAdd;
		Initial.ColorCorrectionContrastShift = ColorCorrectionContrastShift;
		Initial.FocusIntensityPow = FocusIntensityPow;
		Initial.FocusOverallIntensity = FocusOverallIntensity;
		}

	void Update()
		{
//		Object[] objects=UnityEditor.AnimationUtility.GetAnimatableObjects();
//		UnityEditor.AnimationUtility.StopAnimationMode();
//		if (UnityEditor.AnimationUtility.InAnimationMode())
//			return;

		// Trying to get the animation editor not to trash my animations when this is run, since I can't seem to find a way to just disable recording, 
		// and although it doesn't say on the record button, programatic changes will be recorded along with inspector changes.
		if (Application.isEditor)
			return;  // Just can't get anything to work right, so just getting out when the editor is running, which is sad, makes tweaking pretty annoying.  It just means that since Unity doesn't play 'all' of an additive animation, in the editor the values animated on the camera will slowly drift off of what they should be.

		BloomCombineIntensitySubtract = Initial.BloomCombineIntensitySubtract;
		BloomCombineIntensityMultiply = Initial.BloomCombineIntensityMultiply;
		BloomCombineIntensityPow = Initial.BloomCombineIntensityPow;
		BloomCombineMidrange = Initial.BloomCombineMidrange;
		ColorCorrectionTintRed = Initial.ColorCorrectionTintRed;
		ColorCorrectionTintGreen = Initial.ColorCorrectionTintGreen;
		ColorCorrectionTintBlue = Initial.ColorCorrectionTintBlue;
		ColorCorrectionOverallColor = Initial.ColorCorrectionOverallColor;
		ColorCorrectionSaturation = Initial.ColorCorrectionSaturation;
		ColorCorrectionMultiply = Initial.ColorCorrectionMultiply;
		ColorCorrectionAdd = Initial.ColorCorrectionAdd;
		ColorCorrectionContrastShift = Initial.ColorCorrectionContrastShift;
		FocusIntensityPow = Initial.FocusIntensityPow;
		FocusOverallIntensity = Initial.FocusOverallIntensity;

//		if (animating)
//			UnityEditor.AnimationUtility.GetAnimatableObjects();
		}

	// Downsamples the texture to a quarter resolution.
	private void DownSample(RenderTexture source, RenderTexture dest, float factor)
		{
		DownsampleMaterial.SetFloat("_BrightFactor", factor);
		//		DownsampleMaterial.color = new Color(glowTint.r, glowTint.g, glowTint.b, glowTint.a / 4.0f);
		Graphics.Blit(source, dest, DownsampleMaterial);
		}

/*	private void DownSampleWithDepth(RenderTexture source, RenderTexture dest, float factor)
		{
		float mult = 1.0f / 1000.0f;  // Couldn't find any solid info, but seems the depth values aren't taken from actual depth, hopefully this is the right number.

		// Let's modify the intensity based on the focal distance, so it'll automatically adjust and fade properly from the viewpoint to the focal distance.
		float intensity = FocusOverallIntensity * (1.0f / ((FocusDistance - FocusDistanceBeforeBlur) * mult));

		DownsampleMaterialWithDepth.SetFloat("_FocusDistance", FocusDistance * mult);
		DownsampleMaterialWithDepth.SetFloat("_FocusDistanceBeforeBlur", FocusDistanceBeforeBlur * mult);
		DownsampleMaterialWithDepth.SetFloat("_FocusOverallIntensity", intensity);
		DownsampleMaterialWithDepth.SetFloat("_FocusIntensityPow", FocusIntensityPow);

		DownsampleMaterialWithDepth.SetFloat("_BrightFactor", factor);
		//		DownsampleMaterial.color = new Color(glowTint.r, glowTint.g, glowTint.b, glowTint.a / 4.0f);
		Graphics.Blit(source, dest, DownsampleMaterialWithDepth);
		}
*/

	// Called by the camera to apply the image effect
	void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
		if (!enabled)
			return;
		const bool UseHighQualityHDRSampler=true;  // Might not bother moving this to user option, if using the 4X downsampled map, it's really not gonna be that much slower than using the smallest map, and less likely to lose information.
		int downsampleDivisor = 2;
		const int numBlurDownsampleBuffers=7;
		RenderTexture[] buffer=new RenderTexture[numBlurDownsampleBuffers];
		RenderTexture pt=source;
		for (int i = 0; i < numBlurDownsampleBuffers; i++)
			{
			int w = pt.width / downsampleDivisor,h=pt.height / downsampleDivisor;
//			if (i == 0) { w = pt.width; h = pt.height; } // Test running with a max sized buffer

			buffer[i] = RenderTexture.GetTemporary(w == 0 ? 1 : w, h == 0 ? 1 : h, 0, CurrentTargetTypeForBlur);
			pt=buffer[i];
			}

		RenderTexture lastBufferDownsample;
		int finalBlurDivisor = HighQualityBlurMap ? 1 : downsampleDivisor;
		RenderTexture finalBlur = RenderTexture.GetTemporary(source.width / finalBlurDivisor, source.height / finalBlurDivisor, 0, CurrentTargetTypeForHighRange);

		if (UseHighQualityHDRSampler)
			lastBufferDownsample = RenderTexture.GetTemporary(buffer[0].width, buffer[0].height, 0, CurrentTargetTypeForHighRange);
		else
			lastBufferDownsample = RenderTexture.GetTemporary(buffer[3].width, buffer[3].height, 0, CurrentTargetTypeForHighRange);

		// Copy source to the 2x2 smaller texture.
		pt = source;
		foreach (RenderTexture b in buffer)
			{
//			if (pt == source && BlurCombineShaderMaterial.shader == BlurCombineShaderForDistanceFocus)  // We're doing the distance based blur, need depth in the buffer.
//				{
//				DownSampleWithDepth(pt, b, (pt == source) ? 1.0f : 1.0f);
//				}
//			else
				DownSample(pt, b, (pt == source) ? 1.0f : 1.0f);

			pt = b;
			}


		// Now we do our blur combine, combining each image into the final blur image.
		BlurCombineShaderMaterial.SetTexture("_Blur1", buffer[0]);
		BlurCombineShaderMaterial.SetTexture("_Blur2", buffer[1]);
		BlurCombineShaderMaterial.SetTexture("_Blur3", buffer[2]);
		BlurCombineShaderMaterial.SetTexture("_Blur4", buffer[3]);
		BlurCombineShaderMaterial.SetTexture("_Blur5", buffer[4]);
		BlurCombineShaderMaterial.SetTexture("_Blur6", buffer[5]);
		BlurCombineShaderMaterial.SetTexture("_Blur7", buffer[6]);

		BlurCombineShaderMaterial.SetFloat("_BlurCombineFinalIntensitySubtract", BloomCombineIntensitySubtract/3.6f);
		BlurCombineShaderMaterial.SetFloat("_BlurCombineFinalIntensityMult", BloomCombineIntensityMultiply*3);
		BlurCombineShaderMaterial.SetFloat("_BlurCombineFinalIntensityPow", BloomCombineIntensityPow);
		BlurCombineShaderMaterial.SetFloat("_BlurCombineFinalIntensityMidrange", BloomCombineMidrange);
		BlurCombineShaderMaterial.SetVector("_BlurTint", BloomTint);

		// For focus version
		BlurCombineShaderMaterial.SetFloat("_FocusImageStepDecay", 1);
		BlurCombineShaderMaterial.SetFloat("_FocusIntensityPow", FocusIntensityPow);
		BlurCombineShaderMaterial.SetFloat("_FocusOverallIntensity", FocusOverallIntensity);
/*
		if (BlurCombineShaderMaterial.shader == BlurCombineShaderForDistanceFocus)  // Checking against the shader becuase it would already have been confirmed as supported.  This allows us to modify the incomming values for shared things, without breaking the system if it's falling back to base combine.
			{
			float mult = 1.0f / 1000.0f;  // Couldn't find any solid info, but seems the depth values aren't taken from actual depth, hopefully this is the right number.

			// Let's modify the intensity based on the focal distance, so it'll automatically adjust and fade properly from the viewpoint to the focal distance.
			float intensity = FocusOverallIntensity * (1.0f/((FocusDistance - FocusDistanceBeforeBlur)*mult)) ;

			BlurCombineShaderMaterial.SetFloat("_FocusDistance", FocusDistance * mult);
			BlurCombineShaderMaterial.SetFloat("_FocusDistanceBeforeBlur", FocusDistanceBeforeBlur * mult);
			BlurCombineShaderMaterial.SetFloat("_FocusOverallIntensity", intensity);

			if (camera.depthTextureMode == DepthTextureMode.Depth) // Have to pick pass based on what the depth mode is, because of the oh so awesome depth/normal hack.
				Graphics.Blit(source, finalBlur, BlurCombineShaderMaterial, 0);
			else
				Graphics.Blit(source, finalBlur, BlurCombineShaderMaterial, 1);
			}
		else
*/
			{
			Graphics.Blit(source, finalBlur, BlurCombineShaderMaterial);
			}
//		Debug.Log("Drawing shader " + BlurCombineShaderMaterial.shader.name);

		Matrix4x4 ColorCorrectionMatrix = new Matrix4x4();
		Vector3 axisX = new Vector3(), axisY = new Vector3(), axisZ = new Vector3(), offset = new Vector3();

		// Apply tint values to color correction matrix.
		// Note, using alpha to indicate how much influence the color values have.
//		Color ColorCorrectionTintRed = ColorCorrectionBase.TintRed + ColorCorrectionAdd.TintRed;
		axisX.x = ColorCorrectionTintRed.r * ColorCorrectionTintRed.a + (1.0f - ColorCorrectionTintRed.a);
		axisX.y = ColorCorrectionTintRed.g * ColorCorrectionTintRed.a;
		axisX.z = ColorCorrectionTintRed.b * ColorCorrectionTintRed.a;

//		Color ColorCorrectionTintGreen = ColorCorrectionBase.TintGreen + ColorCorrectionAdd.TintGreen;
		axisY.x = ColorCorrectionTintGreen.r * ColorCorrectionTintGreen.a;
		axisY.y = ColorCorrectionTintGreen.g * ColorCorrectionTintGreen.a + (1.0f - ColorCorrectionTintGreen.a);
		axisY.z = ColorCorrectionTintGreen.b * ColorCorrectionTintGreen.a;

//		Color ColorCorrectionTintBlue = ColorCorrectionBase.TintBlue + ColorCorrectionAdd.TintBlue;
		axisZ.x = ColorCorrectionTintBlue.r * ColorCorrectionTintBlue.a;
		axisZ.y = ColorCorrectionTintBlue.g * ColorCorrectionTintBlue.a;
		axisZ.z = ColorCorrectionTintBlue.b * ColorCorrectionTintBlue.a + (1.0f - ColorCorrectionTintBlue.a);

		//			if (ColorCorrectionOverallColor != Color.white)
			{
//			Color ColorCorrectionOverallColor = ColorCorrectionBase.OverallColor + ColorCorrectionAdd.OverallColor;
			Vector3 c = new Vector3(ColorCorrectionOverallColor.r, ColorCorrectionOverallColor.g, ColorCorrectionOverallColor.b);
			c *= ColorCorrectionOverallColor.a;
			axisX *= 1.0f - ColorCorrectionOverallColor.a;
			axisY *= 1.0f - ColorCorrectionOverallColor.a;
			axisZ *= 1.0f - ColorCorrectionOverallColor.a;
			axisX += c;
			axisY += c;
			axisZ += c;
//			Debug.Log("ColorCorrectionOverallColor.a=" + ColorCorrectionOverallColor.a);
			}

			// For contrast levels, we're going to slide the vectors between turning the values to black and white, and normal color values.
			// Using estimated color values for relative color channel brightness.  We could make this an app setting, since some users might want different values,
			// but these seem pretty close to the correct ones (couldn't find that on net, oddly, my googlabilities are weak).
//			float ColorCorrectionSaturation = ColorCorrectionBase.Saturation + ColorCorrectionAdd.Saturation;
			if (ColorCorrectionSaturation != 1)
				{
				float saturationShift = 1.0f - ColorCorrectionSaturation, v;

				axisX = (axisX * ColorCorrectionSaturation) + new Vector3(.35f * (saturationShift), .35f * (saturationShift), .35f * (saturationShift));
				axisY = (axisY * ColorCorrectionSaturation) + new Vector3(.5f * (saturationShift), .5f * (saturationShift), .5f * (saturationShift));
				axisZ = (axisZ * ColorCorrectionSaturation) + new Vector3(.15f * (saturationShift), .15f * (saturationShift), .15f * (saturationShift));
				}


			float ccm = ColorCorrectionMultiply;
			float cca = ColorCorrectionAdd;
			if (ColorCorrectionContrastShift != 0)
				{
				cca -= ColorCorrectionContrastShift * .5f;
				ccm += ColorCorrectionContrastShift * .5f;
				}

			axisX *= ccm;
			axisY *= ccm;
			axisZ *= ccm;

			offset.x += cca; offset.y += cca; offset.z += cca;

			ColorCorrectionMatrix.SetColumn(0, axisX);
			ColorCorrectionMatrix.SetColumn(1, axisY);
			ColorCorrectionMatrix.SetColumn(2, axisZ);
			ColorCorrectionMatrix.SetColumn(3, offset);

		ArrayList needRelease = new ArrayList();
		if (HDRIrisAdjust)
			{
			// Now let's take our lowest blur image, and continue to downsample it to a 1x1 image.  This 1x1 image will then be used to colorize the final blend
			// later.

				{
				/*
				Before we start, we really want an approximation of the final viewed buffer, but we don't have that yet.  What we're gonna do is take the smallest buffer
				and sample it to a new buffer with the final blur combine, and use our final pass.  This is actually a poor representation of the final buffer intensity, 
				but it's an easy/fast way to get the results of the blur into the iris adjust calculations, and it's likely a fair enough approximation.  If it fails, though, 
				can always step to a higher resolution buffer, which likely won't cost much time.
				*/
				FinalBlendShaderMaterial.SetTexture("_FinalBlur", finalBlur);
				FinalBlendShaderMaterial.SetTexture("_HDRIrisAdjustImage", null);
				FinalBlendShaderMaterial.SetFloat("_HDRIrisAdjustBrightenFactor", 0);
				FinalBlendShaderMaterial.SetFloat("_HDRIrisAdjustDarkenFactor", 0);
				FinalBlendShaderMaterial.SetFloat("_HDRIrisAdjustMedianIntensity", .5f);
				Matrix4x4 mat;
				mat = ColorCorrectionMatrix;
				FinalBlendShaderMaterial.SetMatrix("_ColorCorrectionMatrix", mat);

				Graphics.Blit(buffer[numBlurDownsampleBuffers-1], lastBufferDownsample, FinalBlendShaderMaterial);
				}

			RenderTexture downsampledSource = lastBufferDownsample, downsampleTarget = null;

			while (downsampledSource.width > 1 || downsampledSource.height > 1)
				{
				int x = (downsampledSource.width / downsampleDivisor), y = (downsampledSource.height / downsampleDivisor);
				if (x == 0) x = 1;
				if (y == 0) y = 1;

				if (x == 1 && x == 1)
					{
					// this is our lowest level image, let's use our persistant one.
					downsampleTarget = HDRIrisAdjustImage1;
					}
				else
					{
					downsampleTarget = RenderTexture.GetTemporary(x, y, 0, CurrentTargetTypeForHighRange);
					needRelease.Add(downsampleTarget);
					}

				DownSample(downsampledSource, downsampleTarget,1);
				downsampledSource = downsampleTarget;
				}
			
			// Yipee, now it's time to render a 1x1 pixel RT.  Ultimately, it'd be nice if the image could be read, then I wouldn't have to have a shader that's 
			// doing this math.  Speed wise would also be a bit faster, since I could use last frame's value, and skip a texture read in final pass.
			HDRIrisAdjustShaderMaterial.SetTexture("_IrisAdjust", HDRIrisAdjustImage1);
			HDRIrisAdjustShaderMaterial.SetFloat("_HDRIrisAdjustAdjustSpeed", Mathf.Clamp(HDRIrisAdjustSpeed * Time.deltaTime * .5f, 0, .5f));
//			HDRIrisAdjustShaderMaterial.SetFloat("_HDRIrisAdjustAdjustSpeed", 0);
			Graphics.Blit(HDRIrisAdjustImage2, HDRIrisAdjustImage, HDRIrisAdjustShaderMaterial);
			{ RenderTexture temp = HDRIrisAdjustImage2; HDRIrisAdjustImage2 = HDRIrisAdjustImage1; HDRIrisAdjustImage1 = temp; }

			//			Global.Swap(ref hdrIrisAdjustImage2, ref  hdrIrisAdjustImage);
			}


		FinalBlendShaderMaterial.SetTexture("_FinalBlur", finalBlur);
		// For some silly reason, had to change this so I was passing in the source image.  Was working for a long time as _MainTex, then all the sudden wasn't :(.
		FinalBlendShaderMaterial.SetTexture("_HDRIrisAdjustImage", HDRIrisAdjust ? HDRIrisAdjustImage : null);
		FinalBlendShaderMaterial.SetFloat("_HDRIrisAdjustBrightenFactor", HDRIrisAdjust ? HDRIrisAdjustBrightenFactor*2.0f  : 0);
		FinalBlendShaderMaterial.SetFloat("_HDRIrisAdjustDarkenFactor", HDRIrisAdjust ? HDRIrisAdjustDarkenFactor*1.2f : 0);
		FinalBlendShaderMaterial.SetFloat("_HDRIrisAdjustMedianIntensity", HDRIrisAdjust ? HDRIrisAdjustMedianIntensity : 100);
		FinalBlendShaderMaterial.SetMatrix("_ColorCorrectionMatrix", ColorCorrectionMatrix);
//        FinalBlendShaderMaterial.SetTexture("_MainTex", source);
        Graphics.Blit(ShowBloomOnly ? null : source, destination, FinalBlendShaderMaterial);
//		Graphics.Blit(lastBufferDownsample, destination);
//		Graphics.Blit(finalBlur, destination);
//		Graphics.Blit(buffer[0], destination);
//		Graphics.Blit(HDRIrisAdjustImage,destination);
//		Graphics.Blit(source,destination);

		foreach (RenderTexture t in needRelease)
			RenderTexture.ReleaseTemporary(t);
		foreach (RenderTexture b in buffer)
			RenderTexture.ReleaseTemporary(b);

		RenderTexture.ReleaseTemporary(finalBlur);
		RenderTexture.ReleaseTemporary(lastBufferDownsample);

		if (HDRIrisAdjust)
			{
			RenderTexture temp = HDRIrisAdjustImage2; HDRIrisAdjustImage2 = HDRIrisAdjustImage; HDRIrisAdjustImage = temp;
			}
		}

	const float sliderWidth = 95;
	void AddColorControl(string name, ref Color color)
		{
		float cw = sliderWidth * .7f;
		GUILayout.BeginHorizontal();
		GUILayout.Label(name+": ");
		color.r = GUILayout.HorizontalSlider(color.r, 0, 1, GUILayout.Width(cw));
		color.g = GUILayout.HorizontalSlider(color.g, 0, 1, GUILayout.Width(cw));
		color.b = GUILayout.HorizontalSlider(color.b, 0, 1, GUILayout.Width(cw));
		GUILayout.Label("I: "); color.a = GUILayout.HorizontalSlider(color.a, 0, 1, GUILayout.Width(sliderWidth));
		GUILayout.EndHorizontal();
		}


	void OnGUI()
		{
/*		if (!GUIActive) return;
		GUILayout.BeginArea(new Rect(0, 0, 400, Screen.height));
		GUILayout.BeginVertical();
		GUICurrentlyDisabled=GUILayout.Toggle(GUICurrentlyDisabled,"GUI Active");

		if (GUICurrentlyDisabled) return;

			GUILayout.Label("Glow Related Settings:");
			GUILayout.BeginHorizontal();
				GUILayout.Label("Intensity: Multiply ");
				Initial.BloomCombineIntensityMultiply = GUILayout.HorizontalSlider(Initial.BloomCombineIntensityMultiply, 0, 2, GUILayout.Width(sliderWidth));
				GUILayout.Label(" Subtract ");
				Initial.BloomCombineIntensitySubtract = GUILayout.HorizontalSlider(Initial.BloomCombineIntensitySubtract, -1, 3, GUILayout.Width(sliderWidth));
				AddColorControl("Tint Red", ref Initial.BloomTint);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Label("Pow Intensity: ");
				Initial.BloomCombineIntensityPow = GUILayout.HorizontalSlider(Initial.BloomCombineIntensityPow, .2f, 4f, GUILayout.Width(sliderWidth));
			GUILayout.EndHorizontal();

			GUILayout.Label("Color Correction Settings:");
			AddColorControl("Tint Red", ref Initial.ColorCorrectionTintRed);
			AddColorControl("Tint Blue", ref Initial.ColorCorrectionTintBlue);
			AddColorControl("Tint Green", ref Initial.ColorCorrectionTintGreen);
			AddColorControl("Overall", ref Initial.ColorCorrectionOverallColor);
			GUILayout.BeginHorizontal();
				GUILayout.Label("Saturation: ");
				Initial.ColorCorrectionSaturation = GUILayout.HorizontalSlider(Initial.ColorCorrectionSaturation, -1, 3, GUILayout.Width(sliderWidth));
				GUILayout.Label("Contrast: ");
				Initial.ColorCorrectionContrastShift = GUILayout.HorizontalSlider(Initial.ColorCorrectionContrastShift, -1, 1, GUILayout.Width(sliderWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Label("Multiply:");
				Initial.ColorCorrectionMultiply = GUILayout.HorizontalSlider(Initial.ColorCorrectionMultiply, 0, 3, GUILayout.Width(sliderWidth));
				GUILayout.Label("Add:");
				Initial.ColorCorrectionAdd = GUILayout.HorizontalSlider(Initial.ColorCorrectionAdd, -1, 1, GUILayout.Width(sliderWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				HDRIrisAdjust=GUILayout.Toggle(HDRIrisAdjust,"Iris Adjust");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Label("Median Intensity: ");
				HDRIrisAdjustMedianIntensity = GUILayout.HorizontalSlider(HDRIrisAdjustMedianIntensity, 0, 1, GUILayout.Width(sliderWidth));
				GUILayout.Label("Adjust Speed: ");
				HDRIrisAdjustAdjustSpeed = GUILayout.HorizontalSlider(HDRIrisAdjustAdjustSpeed, .01f, 1, GUILayout.Width(sliderWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Label("Brighten Factor: ");
				HDRIrisAdjustBrightenFactor = GUILayout.HorizontalSlider(HDRIrisAdjustBrightenFactor, 0, 3, GUILayout.Width(sliderWidth));
				GUILayout.Label("Darken Factor: ");
				HDRIrisAdjustDarkenFactor = GUILayout.HorizontalSlider(HDRIrisAdjustDarkenFactor, 0, 3, GUILayout.Width(sliderWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Reset to Default"))
				{
				// Likely a way to reserialize this stuff, but don't know how.
				BloomCombineIntensitySubtract = 0f;
				BloomCombineIntensityMultiply = 1;
				BloomCombineIntensityPow = 1;

				HDRIrisAdjust = true;
				HDRIrisAdjustBrightenFactor = 1;
				HDRIrisAdjustDarkenFactor = 1;
				HDRIrisAdjustMedianIntensity = .5f;
				HDRIrisAdjustAdjustSpeed = .5f;

				ColorCorrectionTintRed = new Color(1, 0, 0, 1);
				ColorCorrectionTintGreen = new Color(0, 1, 0, 1);
				ColorCorrectionTintBlue = new Color(0, 0, 1, 1);
				ColorCorrectionOverallColor = new Color(1,1,1,0);
				ColorCorrectionSaturation = 1;
				ColorCorrectionMultiply = 1f;
				ColorCorrectionAdd = 0;
				ColorCorrectionContrastShift = 0;
				}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

		GUILayout.EndVertical();
		GUILayout.EndArea();
*/		}

	}
