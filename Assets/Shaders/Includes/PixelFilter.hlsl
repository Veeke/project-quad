#ifndef PIXEL_FILTER_APPLIED
#define PIXEL_FILTER_APPLIED

void PixelFilter_float(float4 TexelSize, float2 UV, out float2 FilterUV)
{
    float2 boxSize = clamp(fwidth(UV) * TexelSize.zw, 1e-5, 1);
    float2 tx = UV * TexelSize.zw - 0.5 * boxSize;
    float2 txOffset = smoothstep(1 - boxSize, 1, frac(tx));
    FilterUV = (floor(tx) + 0.5 + txOffset) * TexelSize.xy;
    //FilterUV = frac(FilterUV * 16);
}
#endif