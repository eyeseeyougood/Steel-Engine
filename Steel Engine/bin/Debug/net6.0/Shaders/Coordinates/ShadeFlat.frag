#version 330 core

out vec4 outputColour;

// recieving color value passed from vertex shader
in vec3 vertexColour;

void main()
{
	outputColour = vec4(vertexColour, 1);
}