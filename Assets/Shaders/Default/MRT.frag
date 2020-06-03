#version 460
#extension GL_ARB_separate_shader_objects : enable

layout(set=3, binding=0) uniform sampler2D colorTexture;
layout(set=3, binding=1) uniform sampler2D normalTexture;
layout(set=3, binding=2) uniform sampler2D detailTexture;

layout(location = 0) in vec3 inNormal;
layout(location = 1) in vec2 inUV;
layout(location = 2) in vec3 inCameraPosition;
layout(location = 3) in vec3 inWorldPosition;

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 outNormal;
layout(location = 2) out vec4 outPosition;
layout(location = 3) out vec4 outDetail;

mat3 GetTBN();
vec3 GetNormal(mat3 TBN);
vec4 SRGBtoLINEAR(vec4 srgbIn);

void main() {
    outColor = SRGBtoLINEAR(texture(colorTexture, inUV));
    outNormal = vec4(GetNormal(GetTBN()), 1.);
    outPosition = vec4(inWorldPosition, 1.);
    outDetail = texture(detailTexture, inUV);
}

mat3 GetTBN()
{
    vec3 q1  = dFdx(inWorldPosition);
    vec3 q2  = dFdy(inWorldPosition);
    vec2 st1 = dFdx(inUV);
    vec2 st2 = dFdy(inUV);

    vec3 N   = normalize(inNormal);
    vec3 T  = normalize(q1 * st2.t - q2 * st1.t);
    vec3 B  = -normalize(cross(N, T));
    mat3 TBN = mat3(T, B, N);
    return TBN;
}

vec3 GetNormal(mat3 TBN)
{
    vec3 tangentNormal = texture(normalTexture, inUV).rgb;
    return normalize(TBN * tangentNormal);
}

vec4 SRGBtoLINEAR(vec4 srgbIn)
{
	#ifdef SRGB_FAST_APPROXIMATION
	vec3 linOut = pow(srgbIn.xyz,vec3(2.2));
	#else //SRGB_FAST_APPROXIMATION
	vec3 bLess = step(vec3(0.04045),srgbIn.xyz);
	vec3 linOut = mix( srgbIn.xyz/vec3(12.92), pow((srgbIn.xyz+vec3(0.055))/vec3(1.055),vec3(2.4)), bLess );
	#endif //SRGB_FAST_APPROXIMATION
	return vec4(linOut,srgbIn.w);
}