﻿#version 330 core

in vec3 aPosition;

in vec2 aTexCoord;

in vec3 aNormal;

out vec2 texCoord;
out vec3 vertexPosition;
out vec3 normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform vec3 lightPosition;
uniform vec3 lightColour;
uniform float lightIntensity;

void main(void)
{
	vec4 vertPos = vec4(aPosition, 1.0) * model * view * projection;
	gl_Position = vertPos;

	vertexPosition = vec3(vec4(aPosition, 1.0) * model);

	texCoord = aTexCoord;

	normal = aNormal;
}