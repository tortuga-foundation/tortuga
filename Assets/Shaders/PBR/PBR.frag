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
    int reserved;
};

layout(set=0) readonly uniform CAMERA_MVP
{
    mat4 view;
    mat4 projection;
};
layout(set=1) readonly uniform MESH_MVP
{
    mat4 model;
};
layout(set=2) readonly uniform LIGHT_SHADER_INFO
{
    LightInfo info[10];
    int lightsCount;
} lightData;
layout(set=3) readonly uniform MATERIAL_INFO
{
    int enableSmoothShading;
};
layout(set=4, binding=0) uniform sampler2D albedoTexture;
layout(set=5, binding=0) uniform sampler2D normalTexture;
layout(set=6, binding=0) uniform sampler2D metalTexture;
layout(set=7, binding=0) uniform sampler2D roughnessTexture;
layout(set=8, binding=0) uniform sampler2D aoTexture;

layout(location = 0) in vec3 inNormal;
layout(location = 1) in vec2 inUV;
layout(location = 2) in vec3 inCameraPosition;
layout(location = 3) in vec3 inWorldPosition;

layout(location = 0) out vec4 outColor;

const float PI = 3.14159265359;
vec3 getNormalFromMap();
float distributionGGX(vec3 N, vec3 H, float roughness);
float geometrySchlickGGX(float NdotV, float roughness);
float geometrySmith(vec3 N, vec3 V, vec3 L, float roughness);
vec3 fresnelSchlick(float cosTheta, vec3 F0);
vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness);

void main() {
    vec3 albedo = texture(albedoTexture, inUV).rgb;
    float metallic = texture(metalTexture, inUV).r;
    float roughness = texture(roughnessTexture, inUV).r;
    float ao = texture(aoTexture, inUV).r;

    vec3 N = getNormalFromMap();
    vec3 V = normalize(inCameraPosition - inWorldPosition);

    // calculate reflectance at normal incidence; if dia-electric (like plastic) use F0 
    // of 0.04 and if it's a metal, use the albedo color as F0 (metallic workflow)    
    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, albedo, metallic);

    // reflectance equation
    vec3 Lo = vec3(0.0);
    for (int i = 0; i < lightData.lightsCount; i++)
    {
        LightInfo lightInfo = lightData.info[i];
        //calculate per light radiance
        // calculate per-light radiance
        vec3 L = normalize(lightInfo.position.xyz - inWorldPosition);
        vec3 H = normalize(V + L);
        float distance = length(lightInfo.position.xyz - inWorldPosition);
        float attenuation = 1.0 / (distance * distance);
        vec3 radiance = lightInfo.color.rgb * attenuation;

        // Cook-Torrance BRDF
        float NDF = distributionGGX(N, H, roughness);
        float G   = geometrySmith(N, V, L, roughness);
        vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);
           
        vec3 nominator    = NDF * G * F; 
        float denominator = 4 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.001; // 0.001 to prevent divide by zero.
        vec3 specular = nominator / denominator;
        
        // kS is equal to Fresnel
        vec3 kS = F;
        // for energy conservation, the diffuse and specular light can't
        // be above 1.0 (unless the surface emits light); to preserve this
        // relationship the diffuse component (kD) should equal 1.0 - kS.
        vec3 kD = vec3(1.0) - kS;
        // multiply kD by the inverse metalness such that only non-metals 
        // have diffuse lighting, or a linear blend if partly metal (pure metals
        // have no diffuse light).
        kD *= 1.0 - metallic;	  

        // scale light by NdotL
        float NdotL = max(dot(N, L), 0.0);        

        // add to outgoing radiance Lo
        Lo += (kD * albedo / PI + specular) * radiance * NdotL; 
    }

    // ambient lighting (note that the next IBL tutorial will replace 
    // this ambient lighting with environment lighting).
    vec3 ambient = vec3(0.03) * albedo * ao;
    
    vec3 color = ambient + Lo;

    // HDR tonemapping
    color = color / (color + vec3(1.0));
    // gamma correct
    color = pow(color, vec3(1.0/2.2)); 
    outColor = vec4(color, 1.);
}

// ----------------------------------------------------------------------------
// Easy trick to get tangent-normals to world-space to keep PBR code simplified.
// Don't worry if you don't get what's going on; you generally want to do normal 
// mapping the usual way for performance anways; I do plan make a note of this 
// technique somewhere later in the normal mapping tutorial.
vec3 getNormalFromMap()
{
    vec3 tangentNormal = texture(normalTexture, inUV).rbg * 2.0 - 1.0;

    vec3 Q1  = dFdx(inWorldPosition);
    vec3 Q2  = dFdy(inWorldPosition);
    vec2 st1 = dFdx(inUV);
    vec2 st2 = dFdy(inUV);

    vec3 N   = normalize(inNormal);
    vec3 T  = normalize(Q1*st2.t - Q2*st1.t);
    vec3 B  = -normalize(cross(N, T));
    mat3 TBN = mat3(T, B, N);

    return normalize(TBN * tangentNormal);
}
// ----------------------------------------------------------------------------
float distributionGGX(vec3 N, vec3 H, float roughness)
{
    float a = roughness*roughness;
    float a2 = a*a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float nom   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float geometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float geometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = geometrySchlickGGX(NdotV, roughness);
    float ggx1 = geometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}
// ----------------------------------------------------------------------------
vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}
// ----------------------------------------------------------------------------
vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
}   
// ----------------------------------------------------------------------------