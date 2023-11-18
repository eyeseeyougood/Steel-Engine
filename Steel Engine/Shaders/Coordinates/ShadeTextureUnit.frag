#version 330 core

out vec4 outputColour;

in vec2 texCoord;

uniform sampler2D texture0;

void main()
{
	outputColour = texture(texture0, texCoord);
}
