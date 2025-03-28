#ifndef TEXEL_SNAP_APPLIED
#define TEXEL_SNAP_APPLIED

// Code & matrix math done by GreatestBear
void TexelSnap_float(float3 WorldPos, float2 UV0, float4 TexelSize, out float3 SnappedWorldPos) 
{
    // 1.) Calculate how much the texture UV coords need to
    //     shift to be at the center of the nearest texel.
    float2 originalUV = UV0;
    float2 centerUV = floor(originalUV * (TexelSize.zw)) / TexelSize.zw + (TexelSize.xy / 2.0);
    float2 dUV = (centerUV - originalUV);

    // 2b.) Calculate how much the texture coords vary over fragment space.
    //      This essentially defines a 2x2 matrix that gets
    //      texture space (UV) deltas from fragment space (ST) deltas
    // Note: I call fragment space "ST" to disambiguate from world space "XY".
    float2 dUVdS = ddx(originalUV);
    float2 dUVdT = ddy(originalUV);

    // 2c.) Invert the texture delta from fragment delta matrix
    float2x2 dSTdUV = float2x2(dUVdT[1], -dUVdT[0], -dUVdS[1], dUVdS[0]) * (1.0f / (dUVdS[0] * dUVdT[1] - dUVdT[0] * dUVdS[1]));

    // 2d.) Convert the texture delta to fragment delta
    float2 dST = mul(dSTdUV, dUV);

    // 2e.) Calculate how much the world coords vary over fragment space.
    float3 dXYZdS = ddx(WorldPos);
    float3 dXYZdT = ddy(WorldPos);

    // 2f.) Finally, convert our fragment space delta to a world space delta
    // And be sure to clamp it in case the derivative calc went insane
    float3 dXYZ = dXYZdS * dST[0] + dXYZdT * dST[1];
    dXYZ = clamp(dXYZ, -1, 1);

    // 3a.) Transform the snapped UV back to world space
    SnappedWorldPos = (WorldPos + dXYZ);
}
#endif