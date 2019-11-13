#version 460
#define MAXIMUM_LIGHT_INFOS 10

struct LightInfo
{
  vec4 position;
  vec4 forward;
  vec4 color;
  int type;
  float intensity;
  float range;
};
layout(set = 2, binding = 0) readonly uniform LightInfoStruct
{
  uint lightsAmount;
  uint lightsReserved1;
  uint lightsReserved2;
  uint lightsReserved3;
  LightInfo lights[10];
};

layout(set = 3, binding = 0) readonly uniform Material
{
  vec4 albedoColor;
  float metallic;
  float roughness;
};
layout(set = 4, binding = 0) uniform sampler2D albedoTexture;
layout(set = 5, binding = 0) uniform sampler2D normalTexture;
layout(set = 6, binding = 0) uniform sampler2D detailTexture1;

layout(location = 0) in vec3 surfaceNormal;
layout(location = 1) in vec3 cameraVector;
layout(location = 2) in vec2 uvTexture;
layout(location = 3) in mat3 TBN;
layout(location = 6) in vec3 lightVectors[MAXIMUM_LIGHT_INFOS];

layout(location = 0) out vec4 outColor;

void main()
{
  //image color
  vec4 albedo = texture(albedoTexture, uvTexture, 5.0);
  
  //compute normal
  vec3 normal = texture(normalTexture, uvTexture, 5.0).xyz;
  normal = (normal * 2.) - vec3(1.);
  vec3 N = normalize(TBN * normal);

  //compute camera vector
  vec3 C = normalize(cameraVector);

  //compute specular, reflection & ambient oclusion
  vec3 detail1 = texture(detailTexture1, uvTexture, 5.0).rgb;
  float materialSpecular = detail1.r;
  float materialReflection = detail1.g;
  float materialAmbientOclusion = detail1.b;

  vec3 ambient = vec3(0.005);
  vec3 diffuse = vec3(0.);
  vec3 specular = vec3(0.);
  for (int i = 0; i < lightsAmount; i++) {
    //normalized light
    vec3 L = normalize(lightVectors[i]).xyz;

    //diffuse calculations
    float diffuseAmount = max(dot(N, L), 0.) * lights[i].intensity;
    diffuse += lights[i].color.rgb * diffuseAmount;

    //specular calculations
    vec3 R = reflect(L, N);
    float specularAmount = max(dot(R, -C), 0.);
    specular += lights[i].color.rgb * specularAmount * lights[i].intensity;
  }

  vec3 color = albedo.rgb * (diffuse + ambient); //apply ambient
  color += specular * materialSpecular; //apply specular
  color *= materialAmbientOclusion; //apply AO
  outColor = vec4(color, 1.);
}