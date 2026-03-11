#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

inline void Shadowcaster_float(float4 vertexColor, float alpha, out float shadowAlpha)
{
#if ( SHADERPASS == SHADERPASS_SHADOWCASTER )
    if (vertexColor.a < 1)
    {
        shadowAlpha = 0;
    }
    else
    {
        shadowAlpha = alpha;
    }
#else
    shadowAlpha = alpha;
#endif
}