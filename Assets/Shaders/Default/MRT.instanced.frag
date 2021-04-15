#version 460
#extension GL_ARB_separate_shader_objects : enable

layout(set=2, binding=0) uniform sampler2D colorTexture;
layout(set=2, binding=1) uniform sampler2D normalTexture;
layout(set=2, binding=2) uniform sampler2D detailTexture;

layout(location = 0) in vec3 inNormal;
layout(location = 1) in vec2 inUV;
layout(location = 2) in vec3 inCameraPosition;
layout(location = 3) in vec3 inWorldPosition;
layout(location = 4) in vec3 inTangent;
layout(location = 5) in vec3 inBiTangent;

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 outNormal;
layout(location = 2) out vec4 outPosition;
layout(location = 3) out vec4 outDetail;

mat3 GetTBN();
vec3 GetNormal();
vec4 SRGBtoLINEAR(vec4 srgbIn);

void main() {
    outColor = SRGBtoLINEAR(texture(colorTexture, inUV));
    outNormal = vec4(GetTBN() * GetNormal(), 1.);
    outPosition = vec4(inWorldPosition, 1.);
    outDetail = texture(detailTexture, inUV);
}

mat3 GetTBN() {
	vec3 N = normalize(inNormal);
	N.y = -N.y;
	vec3 T = normalize(inTangent);
	vec3 B = cross(N, T);
	return mat3(T, B, N);
}

vec3 GetNormal() {
	return normalize(texture(normalTexture, inUV).xyz * 2. - vec3(1.));
}

vec4 SRGBtoLINEAR(vec4 srgbIn) {
	#ifdef SRGB_FAST_APPROXIMATION
	vec3 linOut = pow(srgbIn.xyz,vec3(2.2));
	#else //SRGB_FAST_APPROXIMATION
	vec3 bLess = step(vec3(0.04045),srgbIn.xyz);
	vec3 linOut = mix( srgbIn.xyz/vec3(12.92), pow((srgbIn.xyz+vec3(0.055))/vec3(1.055),vec3(2.4)), bLess );
	#endif //SRGB_FAST_APPROXIMATION
	return vec4(linOut,srgbIn.w);
}