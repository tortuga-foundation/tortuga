#version 460
#extension GL_ARB_separate_shader_objects : enable

// structures
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

// constants
const float PI = 3.141592653589793;
layout (constant_id = 0) const int LIGHTS_COUNT = 1;

// uniform buffers
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

// inputs / outputs
layout(location=0) in vec2 inUV;
layout(location=0) out vec4 outColor;

//functions
float distributionGGX(
    vec3 normal,
    vec3 cameraPlusLightDirection,
    float roughness
);
float geometrySchlickGGX(
    float NdotV, 
    float roughness
);
float geometrySmith(
    vec3 normal, 
    vec3 cameraDirection,
    vec3 lightDirection,
    float roughness
);
vec3 fresnelSchlick(
    float cosTheta,
    vec3 F0
);
vec4 SRGBtoLINEAR(vec4 srgbIn);

void main() {
    // texture sampling
    vec4 albedo = texture(colorTexture, inUV);
    vec3 normal = texture(normalTexture, inUV).rgb;
    vec3 worldPosition = texture(positionTexture, inUV).xyz;
    vec3 details = texture(detailTexture, inUV).rgb;
    float metallic = clamp(details.r, 0., 1.);
    float roughness = details.g;
    float ambientOcclusion = details.b;

    // init
    vec3 cameraDirection = normalize(inCameraPosition.xyz - worldPosition.xyz);
    vec3 F0 = mix(vec3(0.04), albedo.rgb, metallic);
    vec3 Lo = vec3(0.);

    // foreach light
    for (int i = 0; i < LIGHTS_COUNT; i++)
    {
        // light variables
        LightInfo lightInfo = lightsInfos[i];
        vec3 lightDirection = normalize(lightInfo.position.xyz - worldPosition.xyz);
        vec3 cameraPlusLightDirection = normalize(cameraDirection + lightDirection);
        float lightDistance = length(lightInfo.position.xyz - worldPosition.xyz);
        float attenuation = 1.0 / (lightDistance * lightDistance);
        vec3 radiance = lightInfo.color.rgb * attenuation;

        float NDF = distributionGGX(normal, cameraPlusLightDirection, roughness);
        float G = geometrySmith(normal, cameraDirection, lightDirection, roughness);
        vec3 F = fresnelSchlick(max(dot(cameraPlusLightDirection, cameraDirection), 0.), F0);

        vec3 KD = vec3(1.) - F;
        KD *= 1. - metallic;

        vec3 numerator = NDF * G * F;
        float denominator = 4. * max(dot(normal, cameraDirection), 0.) * max(dot(normal, lightDirection), 0.);
        vec3 specular = numerator / max(denominator, 0.001);

        float NdotL = max(dot(normal, lightDirection), 0.);
        Lo += (KD * albedo.rgb / PI + specular) * radiance * NdotL;
    }

    vec3 ambient = vec3(0.03) * albedo.rgb * ambientOcclusion;
    vec3 color = ambient + Lo;

    //color = color / (color + vec3(1.));
    //color = pow(color, vec3(1. / 2.2));

    outColor = SRGBtoLINEAR(vec4(color, albedo.a));
}

float distributionGGX(
    vec3 normal,
    vec3 cameraPlusLightDirection,
    float roughness
)
{
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH  = max(dot(normal, cameraPlusLightDirection), 0.0);
    float NdotH2 = NdotH*NdotH;
	
    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
	
    return num / denom;
}

float geometrySchlickGGX(
    float NdotV, 
    float roughness
)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float num   = NdotV;
    float denom = NdotV * (1.0 - k) + k;
	
    return num / denom;
}
float geometrySmith(
    vec3 normal, 
    vec3 cameraDirection,
    vec3 lightDirection,
    float roughness
)
{
    float NdotV = max(dot(normal, cameraDirection), 0.0);
    float NdotL = max(dot(normal, lightDirection), 0.0);
    float ggx2  = geometrySchlickGGX(NdotV, roughness);
    float ggx1  = geometrySchlickGGX(NdotL, roughness);
	
    return ggx1 * ggx2;
}

vec3 fresnelSchlick(
    float cosTheta,
    vec3 F0
)
{
    return F0 + (1.0 - F0) * pow(max(1.0 - cosTheta, 0.0), 5.0);
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