#ifndef PIXEL_FILTER_APPLIED
#define PIXEL_FILTER_APPLIED

void AtlasPixelFilter_float(float4 TexelSize, float2 atlasCoords, float2 UV, out float2 FilterUV)
{   
    float2 txMin = atlasCoords * 16 + 0.5;
    float2 txMax = (atlasCoords + 1) * 16 - 0.5;
    
    float2 texel = UV * TexelSize.zw;
    
    float2 boxSize = clamp(fwidth(UV) * TexelSize.zw, 1e-5, 1);
    float2 tx = UV * TexelSize.zw - 0.5 * boxSize;
    float2 txOffset = smoothstep(1 - boxSize, 1, frac(tx));
    float2 uv = (floor(tx) + 0.5 + txOffset) * TexelSize.xy;
   
    texel = floor(texel);
    
    bool isWithinBounds = texel.x > txMin.x && texel.x < txMax.x && texel.y > txMin.y && texel.y < txMax.y;
    float2 centerUV = (texel + 0.5) * TexelSize.xy;
    FilterUV = isWithinBounds ? uv : centerUV;
}
#endif