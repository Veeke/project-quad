#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
    #if (SHADERPASS != SHADERPASS_FORWARD)
        #undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
    #endif
#endif

struct CustomLightingData 
{
    float3 positionWS;
    float3 normalWS;
    float4 shadowCoord;
    float3 albedo;
    float3 ambient;
};

#ifndef SHADERGRAPH_PREVIEW
float3 CustomLightHandling(CustomLightingData d, Light light) 
{
    float shadowAttenuation = saturate(dot(d.normalWS, light.direction));
    shadowAttenuation = shadowAttenuation > 0.0 ? 1.0 : 0.0;
    shadowAttenuation = min(shadowAttenuation, light.shadowAttenuation);
    float3 shadowColor = d.albedo * d.ambient;
    return lerp(shadowColor, d.albedo, shadowAttenuation);
}
#endif

#ifndef SHADERGRAPH_PREVIEW
float3 CustomAdditionalLightHandling(CustomLightingData d, Light light)
{
	float3 radiance = light.color * light.distanceAttenuation * light.shadowAttenuation;
    float3 diffuse = saturate(dot(d.normalWS, light.direction));
	float3 color = d.albedo * radiance * diffuse;
	return color;
}
#endif

float3 CalculateCustomLighting(CustomLightingData d) 
{
    #ifdef SHADERGRAPH_PREVIEW
        return d.albedo;
    #else
        Light mainLight = GetMainLight(d.shadowCoord, d.positionWS, 1);   
        float3 color = 0;
        color += CustomLightHandling(d, mainLight);

        #ifdef _ADDITIONAL_LIGHTS
            int pixelLightCount = GetAdditionalLightsCount();
			for (int i = 0; i < pixelLightCount; i++)
			{
				Light light = GetAdditionalLight(i, d.positionWS, 1);             
				color += CustomAdditionalLightHandling(d, light);
			}
        #endif
        return color;
    #endif
}

void CalculateCustomLighting_float(float3 Albedo, float3 Normal, float3 Position, float3 Ambient,
out float3 Color) 
{
    CustomLightingData d;
    d.positionWS = Position;
    d.normalWS = Normal;
    d.albedo = Albedo;
    d.ambient = Ambient;

    #ifdef SHADERGRAPH_PREVIEW
        d.shadowCoord = 0;
    #else
        float4 positionCS = TransformWorldToHClip(Position);
        #if SHADOWS_SCREEN
            d.shadowCoord = ComputeScreenPos(positionCS);
        #else
            d.shadowCoord = TransformWorldToShadowCoord(Position);
        #endif
    #endif
    Color = CalculateCustomLighting(d);
}
#endif