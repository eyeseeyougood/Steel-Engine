#version 330 core

in vec3 aPosition;

in vec3 aColour;

// passing to frag shader
out vec3 vertexColour;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
	gl_Position = vec4(aPosition, 1.0) * model * view * projection;

	vertexColour = aColour; // passing passed colour value onward to the frag shader
}