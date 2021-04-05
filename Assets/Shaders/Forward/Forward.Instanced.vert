#version 460

layout(set=0, binding=0) readonly uniform CAMERA_MVP
{
    mat4 view;
    mat4 projection;
    int cameraX;
    int cameraY;
    int cameraWidth;
    int cameraHeight;
};
layout(set=1, binding=0) readonly uniform MESH_MVP
{
    mat4 model;
};
layout(set=3, binding=0) readonly uniform MATERIAL_INFO
{
    int workflow;
};

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec2 inUV;
layout(location = 2) in vec3 inNormal;

layout(location = 3) in vec3 inObjectPosition;
layout(location = 4) in vec4 inObjectRotation;
layout(location = 5) in vec3 inObjectScale;

layout(location = 0) out vec3 outNormal;
layout(location = 1) out vec2 outUV;
layout(location = 2) out vec3 outCameraPosition;
layout(location = 3) out vec3 outWorldPosition;

mat4 translateMatrix(mat4 matrix, vec3 vector)
{
    matrix[3][0] += vector.x;
    matrix[3][1] += vector.y;
    matrix[3][2] += vector.z;
    return matrix;
}

mat4 rotateMatrix(mat4 matrix, vec4 q)
{
    float sqW = q.w * q.w;
    float sqX = q.x * q.x;
    float sqY = q.y * q.y;
    float sqZ = q.z * q.z;

    matrix[0][0] -= (2 * sqY) - (2 * sqZ);
    matrix[1][0] = (2 * q.x * q.y) - (2 * q.z * q.w);
    matrix[2][0] = (2 * q.x * q.z) + (2 * q.y * q.w);
    
    matrix[0][1] = (2 * q.x * q.y) + (2 * q.z * q.w);
    matrix[1][1] -= (2 * sqX) - (2 * sqZ);
    matrix[2][1] = (2 * q.y * q.z) - (2 * q.x * q.w);
    
    matrix[0][2] = (2 * q.x * q.z) - (2 * q.y * q.w);
    matrix[1][2] = (2 * q.y * q.z) + (2 * q.x * q.w);
    matrix[2][2] -= (2 * sqX) - (2 * sqY);

    return matrix;
}

mat4 scaleMatrix(mat4 matrix, vec3 vector)
{
    matrix[0][0] *= vector.x;
    matrix[1][1] *= vector.y;
    matrix[2][2] *= vector.z;
    return matrix;
}

void main() {
    mat4 instancedModel = mat4(1.);
    instancedModel = scaleMatrix(instancedModel, inObjectScale);
    instancedModel = rotateMatrix(instancedModel, inObjectRotation);
    instancedModel = translateMatrix(instancedModel, inObjectPosition);
    vec4 worldPosition = instancedModel * vec4(inPosition, 1.0);


    gl_Position = projection * view * worldPosition;

    outWorldPosition = worldPosition.xyz;
    outUV = inUV;
    outCameraPosition = inverse(view)[3].xyz;
    outNormal = normalize(instancedModel * vec4(inNormal, 0.)).xyz;
}