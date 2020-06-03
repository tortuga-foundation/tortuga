#version 460
#extension GL_ARB_separate_shader_objects : enable

struct LightInfo {
    vec4 position;
    vec4 forward;
    vec4 color;
    int type;
    float intensity;
    int reserved1;
    int reserved2;
};

struct PBRInfo
{
	float NdotL;                    // cos angle between normal and light direction
	float NdotV;                    // cos angle between normal and view direction
	float NdotH;                    // cos angle between normal and half vector
	float LdotH;                    // cos angle between light direction and half vector
	float VdotH;                    // cos angle between view direction and half vector
	float roughness;                // roughness value, as authored by the model creator (input to shader)
	float metalness;                // metallic value at the surface
	vec3 reflectance0;              // full reflectance color (normal incidence angle)
	vec3 reflectance90;             // reflectance color at grazing angle
	float alphaRoughness;           // roughness mapped to a more linear change in the roughness (proposed by [2])
	vec3 diffuseColor;              // color contribution from diffuse lighting
	vec3 specularColor;             // color contribution from specular lighting
};

layout (constant_id = 0) const int LIGHTS_COUNT = 1;

layout(set=0,binding=0) uniform sampler2D colorTexture;
layout(set=0,binding=1) uniform sampler2D normalTexture;
layout(set=0,binding=2) uniform sampler2D positionTexture;
layout(set=0,binding=3) uniform sampler2D detailTexture;

layout(set=1,binding=0) readonly uniform CAMERA_POSITION {
    vec4 inCameraPosition;
};
layout(set=2,binding=0) readonly uniform LIGHT_INFO {
    LightInfo lightsInfos[LIGHTS_COUNT];
};

layout(location=0) in vec2 inUV;

layout(location=0) out vec4 outColor;

//constants
const float PI = 3.141592653589793;

//functions
vec3 fresnelSchlick(float cosTheta, vec3 F0);
float GeometrySchlickGGX(float NdotV, float roughness);
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness);
float DistributionGGX(vec3 N, vec3 H, float roughness);
vec4 SRGBtoLINEAR(vec4 srgbIn);

void main() {
    //texture sampling
    vec4 baseColor = texture(colorTexture, inUV);
    vec3 normal = texture(normalTexture, inUV).rgb;
    vec3 worldPosition = texture(positionTexture, inUV).xyz;
    float metallic = clamp(texture(detailTexture, inUV).r, 0., 1.);
    float roughness = texture(detailTexture, inUV).g;
    float ambientOclusion = texture(detailTexture, inUV).b;

    //camera direction
    vec3 V = normalize(inCameraPosition.xyz - worldPosition);

    //for each light
    vec3 Lo = vec3(0.0);
    for (int i = 0; i < 1; i++)
    {
        //light info
        LightInfo light = lightsInfos[i];
        vec3 L = normalize(light.position.xyz - worldPosition);
        vec3 H = normalize(V + L);
        float dist    = length(light.position.xyz - worldPosition);
        float attenuation = 1.0 / (dist * dist);
        vec3 radiance     = light.color.rgb * attenuation;

        //apply fresnel
        vec3 F0 = vec3(0.04);
        F0      = mix(F0, baseColor.rgb, metallic);
        vec3 F  = fresnelSchlick(max(dot(H, V), 0.0), F0);
    
        float NDF = DistributionGGX(normal, H, roughness);
        float G   = GeometrySmith(normal, V, L, roughness);

        vec3 numerator    = NDF * G * F;
        float denominator = 4.0 * max(dot(normal, V), 0.0) * max(dot(normal, L), 0.0);
        vec3 specular     = numerator / max(denominator, 0.001);

        vec3 kS = F;
        vec3 kD = vec3(1.0) - kS;
        kD *= 1.0 - metallic;

        float NdotL = max(dot(normal, L), 0.0);        
        Lo += (kD * baseColor.rgb / PI + specular) * radiance * NdotL;
    }

    vec3 ambient = vec3(0.03) * baseColor.rgb * ambientOclusion;
    vec3 color   = ambient + Lo;

    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0/2.2)); 

    outColor = SRGBtoLINEAR(vec4(Lo, baseColor.a));
}

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - min(cosTheta, 1.), 5.0);
}  

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;
	
    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
	
    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float num   = NdotV;
    float denom = NdotV * (1.0 - k) + k;
	
    return num / denom;
}

float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2  = GeometrySchlickGGX(NdotV, roughness);
    float ggx1  = GeometrySchlickGGX(NdotL, roughness);
	
    return ggx1 * ggx2;
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