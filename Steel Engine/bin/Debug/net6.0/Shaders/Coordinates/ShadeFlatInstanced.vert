#version 330 core

layout(location = 0) in vec3 inPosition; // Per-Vertex data
layout(location = 1) in vec3 inModelPos; // Per-Instance data
layout(location = 2) in vec4 inRotation; // Per-Instance data
layout(location = 3) in vec3 inScale; // Per-Instance data
layout(location = 4) in vec3 inColour; // Per-Instance data

// passing to frag shader
out vec3 vertexColour;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
	gl_Position = vec4(inPosition+inModelPos, 1.0) * model * view * projection;

	vertexColour = inColour; // passing passed colour value onward to the frag shader
}