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

layout (constant_id = 0) const int LIGHTS_COUNT = 0;

layout(set=0,binding=0) uniform sampler2D colorTexture;
layout(set=0,binding=1) uniform sampler2D normalTexture;
layout(set=0,binding=2) uniform sampler2D positionTexture;
layout(set=0,binding=3) uniform sampler2D detailTexture;

layout(set=1,binding=0) readonly uniform CAMERA_POSITION {
    vec4 inCameraPosition;
};
layout(set=2,binding=0) readonly uniform LIGHT_INFO {
    LightInfo lightsInfos[1];
};

layout(location=0) in vec2 inUV;

layout(location=0) out vec4 outColor;

//constants
const float PI = 3.141592653589793;
const float MIN_ROUGHNESS = 0.04;
#define MANUAL_SRGB 1

//functions
vec4 SRGBtoLINEAR(vec4 srgbIn);
vec3 SpecularReflection(PBRInfo pbrInputs);
float GeometricOcclusion(PBRInfo pbrInputs);
float MicrofacetDistribution(PBRInfo pbrInputs);
vec3 Diffuse(PBRInfo pbrInputs);
vec3 GetIBLContribution(PBRInfo pbrInputs, vec3 n, vec3 reflection);
vec3 Uncharted2Tonemap(vec3 color);
vec4 Tonemap(vec4 color);

void main() {
    //setup
    vec3 f0 = vec3(0.04);
    vec3 n = texture(normalTexture, inUV).rgb;
    vec3 worldPosition = texture(positionTexture, inUV).xyz;
    vec3 v = normalize(inCameraPosition.xyz - worldPosition);

    //metalness workflow
    float metallic = clamp(texture(detailTexture, inUV).r, 0., 1.);
    float roughness = clamp(texture(detailTexture, inUV).g, MIN_ROUGHNESS, 1.);
    float ao = texture(detailTexture, inUV).b;
    vec4 baseColor = SRGBtoLINEAR(texture(colorTexture, inUV));

    //get diffuse color
    vec3 diffuseColor = baseColor.rgb * (vec3(1.) - f0);
    diffuseColor *= 1. - metallic;

    float alphaRoughness = roughness * roughness;
    vec3 specularColor = mix(f0, baseColor.rgb, metallic);

    //compute reflectance
    float reflectance = max(max(specularColor.r, specularColor.g), specularColor.b);
    
    vec3 totalColor = vec3(0.);

    for (int i = 0; i < 1; i++)
    {
        LightInfo light = lightsInfos[i];

        float reflectance90 = clamp(reflectance * 25., 0., 1.);
        vec3 specularEnvironmentR0 = specularColor.rgb;
        vec3 specularEnvironmentR90 = vec3(1.) * reflectance90;

        vec3 l = normalize(light.position.xyz - worldPosition);
        if (light.type == 1)
            l = normalize(light.forward.xyz);
        vec3 h = normalize(l+v);
        vec3 reflection = -normalize(reflect(v, n));
        float dist = length(light.position.xyz - worldPosition);
        if (light.type == 1)
            dist = 1;
        reflection.y *= -1.;

        float nDotL = clamp(dot(n, l), 0.001, 1.);
        float nDotV = clamp(dot(n, v), 0.001, 1.);
        float nDotH = clamp(dot(n, h), 0.001, 1.);
        float lDotH = clamp(dot(l, h), 0.001, 1.);
        float vDotH = clamp(dot(v, h), 0.001, 1.);
        
        PBRInfo pbrInputs = PBRInfo(
            nDotL,
            nDotV,
            nDotH,
            lDotH,
            vDotH,
            roughness,
            metallic,
            specularEnvironmentR0,
            specularEnvironmentR90,
            alphaRoughness,
            diffuseColor,
            specularColor
        );

        vec3 F = SpecularReflection(pbrInputs);
        float G = GeometricOcclusion(pbrInputs);
        float D = MicrofacetDistribution(pbrInputs);

        // Calculation of analytical lighting contribution
        vec3 diffuseContrib = (1.0 - F) * Diffuse(pbrInputs);
        vec3 specContrib = F * G * D / (4.0 * nDotL * nDotV);
        // Obtain final intensity as reflectance (BRDF) scaled by the energy of the light (cosine law)
        vec3 color = (nDotL * light.color.rgb * (diffuseContrib + specContrib) * light.intensity) / (dist * dist);

        // Calculate lighting contribution from image based lighting source (IBL)
        color += GetIBLContribution(pbrInputs, n, reflection);

        totalColor += mix(color, color * ao, 1.);
    }


    outColor = vec4(totalColor, baseColor.a);
}

vec4 SRGBtoLINEAR(vec4 srgbIn) {
	#ifdef MANUAL_SRGB
	#ifdef SRGB_FAST_APPROXIMATION
	vec3 linOut = pow(srgbIn.xyz,vec3(2.2));
	#else //SRGB_FAST_APPROXIMATION
	vec3 bLess = step(vec3(0.04045),srgbIn.xyz);
	vec3 linOut = mix( srgbIn.xyz/vec3(12.92), pow((srgbIn.xyz+vec3(0.055))/vec3(1.055),vec3(2.4)), bLess );
	#endif //SRGB_FAST_APPROXIMATION
	return vec4(linOut,srgbIn.w);;
	#else //MANUAL_SRGB
	return srgbIn;
	#endif //MANUAL_SRGB
}

vec3 SpecularReflection(PBRInfo pbrInputs)
{
	return pbrInputs.reflectance0 + (pbrInputs.reflectance90 - pbrInputs.reflectance0) * pow(clamp(1.0 - pbrInputs.VdotH, 0.0, 1.0), 5.0);
}

float GeometricOcclusion(PBRInfo pbrInputs)
{
	float NdotL = pbrInputs.NdotL;
	float NdotV = pbrInputs.NdotV;
	float r = pbrInputs.alphaRoughness;

	float attenuationL = 2.0 * NdotL / (NdotL + sqrt(r * r + (1.0 - r * r) * (NdotL * NdotL)));
	float attenuationV = 2.0 * NdotV / (NdotV + sqrt(r * r + (1.0 - r * r) * (NdotV * NdotV)));
	return attenuationL * attenuationV;
}

float MicrofacetDistribution(PBRInfo pbrInputs)
{
	float roughnessSq = pbrInputs.alphaRoughness * pbrInputs.alphaRoughness;
	float f = (pbrInputs.NdotH * roughnessSq - pbrInputs.NdotH) * pbrInputs.NdotH + 1.0;
	return roughnessSq / (PI * f * f);
}

vec3 Diffuse(PBRInfo pbrInputs)
{
	return pbrInputs.diffuseColor / PI;
}

vec3 GetIBLContribution(PBRInfo pbrInputs, vec3 n, vec3 reflection)
{
	// retrieve a scale and bias to F0. See [1], Figure 3
	vec3 brdf = vec3(0.).rgb;
	vec3 diffuseLight = SRGBtoLINEAR(Tonemap(vec4(0.))).rgb;

	vec3 specularLight = SRGBtoLINEAR(Tonemap(vec4(0.))).rgb;

	vec3 diffuse = diffuseLight * pbrInputs.diffuseColor;
	vec3 specular = specularLight * (pbrInputs.specularColor * brdf.x + brdf.y);

	// For presentation, this allows us to disable IBL terms
	// For presentation, this allows us to disable IBL terms
	diffuse *= 1;
	specular *= 1;

	return diffuse + specular;
}

vec3 Uncharted2Tonemap(vec3 color)
{
	float A = 0.15;
	float B = 0.50;
	float C = 0.10;
	float D = 0.20;
	float E = 0.02;
	float F = 0.30;
	float W = 11.2;
	return ((color*(A*color+C*B)+D*E)/(color*(A*color+B)+D*F))-E/F;
}

vec4 Tonemap(vec4 color)
{
	vec3 outcol = Uncharted2Tonemap(color.rgb * 1);
	outcol = outcol * (1.0f / Uncharted2Tonemap(vec3(11.2f)));	
	return vec4(pow(outcol, vec3(1.0f / 1)), color.a);
}