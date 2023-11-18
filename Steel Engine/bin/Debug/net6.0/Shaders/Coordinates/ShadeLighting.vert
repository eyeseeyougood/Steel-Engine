#version 330 core

in vec3 aPosition;

in vec3 aColour;

out vec3 vertexColour;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform mat4 lightModel;
uniform vec3 lightColour;
uniform float lightIntensity;

void main(void)
{
	vec4 vertPos = vec4(aPosition, 1.0) * model * view * projection;
	gl_Position = vertPos;

	vertexColour = aColour;

	vec4 lightPos = vec4(0, 0, 0, 1.0) * lightModel * view * projection;

	float mag = length(vec3(lightPos) - vec3(vertPos));
    vertexColour = vertexColour / mag * lightColour * lightIntensity;
}
