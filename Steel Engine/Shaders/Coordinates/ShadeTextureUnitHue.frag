#version 330 core

out vec4 outputColour;

in vec2 texCoord;

uniform sampler2D texture0;
uniform vec3 colMod;
uniform float mixFact;

void main()
{
	outputColour = mix(texture(texture0, texCoord), vec4(colMod, 1), mixFact);
}