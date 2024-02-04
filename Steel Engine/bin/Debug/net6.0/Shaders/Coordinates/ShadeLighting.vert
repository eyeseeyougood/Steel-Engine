#version 330 core

in vec3 aPosition;

in vec3 aColour;

in vec3 aNormal;

out vec3 vertexColour;
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

	vertexColour = aColour;

	vertexPosition = vec3(vec4(aPosition, 1.0) * model);

	mat3 rotationMatrix = mat3(model[0].xyz, model[1].xyz, model[2].xyz);

	normal = aNormal * rotationMatrix;
}