#version 330 core

layout(location = 0) in vec3 inPosition;

out vec3 vertexColour;
flat out int visible;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform int activities[128];
uniform vec3 positions[128];
uniform vec4 rotations[128];
uniform vec3 scales[128];
uniform vec3 colours[128];

vec3 RotateVectorByQuaternion(vec4 q, vec3 v) {
    // Quaternion multiplication
    vec3 t = 2.0 * cross(q.xyz, v);
    return v + q.w * t + cross(q.xyz, t);
}

void main(void)
{
    visible = 1;
    if (activities[gl_InstanceID] == 0)
    {
        visible = 0;
    }

    vec3 rotatedPosition = RotateVectorByQuaternion(rotations[gl_InstanceID], inPosition*scales[gl_InstanceID]);

    gl_Position = vec4(rotatedPosition + positions[gl_InstanceID], 1.0) * model * view * projection;
    vertexColour = colours[gl_InstanceID];
}