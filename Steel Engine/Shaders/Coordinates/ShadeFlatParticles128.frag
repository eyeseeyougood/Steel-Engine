#version 330 core

out vec4 outputColour;

// recieving color value passed from vertex shader
in vec3 vertexColour;
flat in int visible;

void main()
{
	if (visible == 0)
	{
		discard;
	}
	outputColour = vec4(vertexColour, 1);
}