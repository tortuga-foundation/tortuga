#version 460
#define MAXIMUM_LIGHT_INFOS 10

layout(set = 0, binding = 0) readonly uniform CameraTransform
{
  mat4 view;
  mat4 projection;
};
layout(set = 1, binding = 0) readonly uniform MeshTransform
{
  mat4 model;
};
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

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec2 inTexture;
layout(location = 2) in vec3 inNormal;
layout(location = 3) in vec3 inTangent;
layout(location = 4) in vec3 inBiTangent;

layout(location = 0) out vec3 surfaceNormal;
layout(location = 1) out vec3 cameraVector;
layout(location = 2) out vec2 uvTexture;
layout(location = 3) out mat3 TBN;
layout(location = 6) out vec3 lightVectors[MAXIMUM_LIGHT_INFOS];

void main()
{
  vec4 worldPosition = model * vec4(inPosition, 1.);
  cameraVector = (inverse(view) * vec4(0., 0., 0., 1.)).xyz - worldPosition.xyz;
  uvTexture = inTexture;
  surfaceNormal = normalize(model * vec4(inNormal, 0.)).xyz;
  //TBN
  vec3 surfaceTangent = normalize(model * vec4(inTangent, 0.)).xyz;
  vec3 SurfaceBiTangent = normalize(model * vec4(inBiTangent, 0.)).xyz;
  TBN = mat3(surfaceTangent, SurfaceBiTangent, surfaceNormal);

  for (uint i = 0; i < lightsAmount; i++)
    lightVectors[i] = lights[i].position.xyz - worldPosition.xyz;

  gl_Position = projection * view * worldPosition;
}