#version 460
#extension GL_ARB_separate_shader_objects : enable

struct LightInfo
{
    vec4 position;
    vec4 forward;
    vec4 color;
    int type;
    float intensity;
    float range;
};

layout(set=0,binding=0) readonly uniform CAMERA_MVP
{
    mat4 view;
    mat4 projection;
};
layout(set=1,binding=0) readonly uniform MESH_MVP
{
    mat4 model;
};
layout(set=2,binding=0) readonly uniform LIGHT_SHADER_INFO
{
    int lightsCount;
    int lightReserved1;
    int lightReserved2;
    int lightReserved3;
    LightInfo lights[10];
};
layout(set=3,binding=0) readonly uniform MATERIAL_INFO
{
    float metallic;
    float roughness;
};

layout(location = 0) in vec3 inNormal;
layout(location = 1) in vec2 inUV;
layout(location = 2) in vec3 inCameraDirection;
layout(location = 3) in vec3 inWorldPosition;
layout(location = 4) in vec3 inLightVector[10];

layout(location = 0) out vec4 outColor;

const float PI = 3.14159265359;

float distributionGGX(float nDotH, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float denom = nDotH * nDotH * (a2 - 1.) + 1.;
    denom = PI * denom * denom;
    return a2 / max(denom, 0.0000001); // prevent devide by zero
}

float geometrySmith(float nDotV, float nDotL, float roughness)
{
    float r = roughness + 1.;
    float k = (r * r) / 8.0;
    float ggx1 = nDotV / (nDotV * (1. - k) + k);
    float ggx2 = nDotL / (nDotL * (1. - k) + k);
    return ggx1 * ggx2;
}

vec3 fresnelSchlick(float hDotV, vec3 baseReflectivity)
{
    return baseReflectivity + (1. - baseReflectivity) * pow(1. - hDotV, 5.);
}

void main() {
    vec3 albedo = vec3(1.);

    vec3 N = inNormal;
    vec3 V = inCameraDirection;

    //calculate reflectance at normal incidence; if dia-electric (light plastic) use baseReflectivity
    // of 0.04 and if it's a metal, use the albedo color as baseReflectivity (metallic workflow)
    vec3 baseReflectivity = mix(vec3(0.04), albedo, metallic);

    //ferlectance equation
    vec3 Lo = vec3(0.);
    for (int i = 0; i < lightsCount; i++)
    {
        LightInfo lightInfo = lights[i];
        //calculate per light radiance
        vec3 L = normalize(lightInfo.position.xyz - inWorldPosition);
        vec3 H = normalize(V + L);
        float distance = length(lightInfo.position.xyz - inWorldPosition);
        float attenuation = 1. / (distance * distance);
        vec3 radiance = lightInfo.color.rgb * attenuation;

        //Cook-Torrance BRDF
        float nDotV = max(dot(N, V), 0.0000001);
        float nDotL = max(dot(N, L), 0.0000001);
        float hDotV = max(dot(H, V), 0.);
        float nDotH = max(dot(N, H), 0.);

        float D = distributionGGX(nDotH, roughness);
        float G = geometrySmith(nDotV, nDotL, roughness);
        vec3 F = fresnelSchlick(hDotV, baseReflectivity);

        vec3 specular = D * G * F;
        specular /= 4. * nDotV * nDotL;

        //for energy conservation, the diffuse and specular light can't
        //be above 1. (unless the surface emits light); to preserve this
        //relationship the diffuse component (kD) should equal 1. - KS
        vec3 kD = vec3(1.) - F; //F equals KS
        
        //multiply kD by the inverse metalness such that only non-metals
        //have diffuse lighting, or a linear blend if partly metal
        //(pure metals have no diffuse light)
        kD *= 1. - metallic;

        //note that:
        //1: angle of light to surface affects specular, not just diffuse
        //2: we mix albedo with diffuse, but not specular
        Lo += (kD * albedo / PI + specular) * radiance * nDotL;
    }

    vec3 ambient = vec3(0.03) * albedo;
    vec3 color = ambient + Lo;
    color = color / (color + vec3(1.));
    color = pow(color, vec3(1./2.2));

    outColor = vec4(color, 1.);
}