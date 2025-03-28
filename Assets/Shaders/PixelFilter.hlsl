#ifndef PIXEL_FILTER_APPLIED
#define PIXEL_FILTER_APPLIED

void PixelFilter_float(UnityTexture2D tex, float2 UV, out float4 Colour)
{
    float2 boxSize = clamp(fwidth(UV) * tex.texelSize.zw, 1e-5, 1);
    float2 tx = UV * tex.texelSize.zw - 0.5 * boxSize;
    float2 txOffset = smoothstep(1 - boxSize, 1, frac(tx));
    float2 uv = (floor(tx) + 0.5 + txOffset) * tex.texelSize.xy;
    Colour = SAMPLE_TEXTURE2D_GRAD(tex, tex.samplerstate, uv, ddx(UV), ddy(UV));
}
#endif