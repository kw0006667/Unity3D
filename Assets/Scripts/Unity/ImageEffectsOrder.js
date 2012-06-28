
@script RequireComponent(Camera)
@script ExecuteInEditMode

private var _tex : RenderTexture[] = new RenderTexture[2]; 

function OnEnable () {
	if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
		enabled = false;
}

function OnRenderImage (source : RenderTexture, destination : RenderTexture) 
{
	_tex[0] = source;
	_tex[1] = RenderTexture.GetTemporary(source.width, source.height);
	var releaseMe : RenderTexture = _tex[1];
	var index : int = 0;
	
	var sorted : Array = new Array();
	
	var i : int = 0;
	for (var fx : PostEffectsBase in GetComponents(PostEffectsBase)) 
	{
		if(fx && fx.enabled) 
		{	
			sorted[i++] = fx;
		}
	}	
	
	while (sorted.length) 
	{
		var indexToUse : int = 0;
		var orderValue : int = -1;
		for(i = 0; i < sorted.length; i++) {
			if(sorted[i].order > orderValue) {
				orderValue = sorted[i].order;	
				indexToUse = i;
			}
		}
        
        var effect : PostEffectsBase = sorted[indexToUse];
		if (effect.PreferRenderImage3())
        {
            effect.OnRenderImage3(_tex[index], _tex[1-index]);
        }
        else
        {
            effect.OnRenderImage2(_tex[index], _tex[1-index]);
            index = 1-index;
        }
		
		sorted.RemoveAt(indexToUse);
	}
	
    Graphics.Blit(_tex[index], destination);
	
	RenderTexture.ReleaseTemporary(releaseMe);
}