#version 330 core

out vec4 outputColour;

// recieving color value passed from vertex shader
in vec3 vertexColour;
in vec3 vertexPosition;
in vec3 normal;

uniform vec3 lightColour;
uniform vec3 lightPosition;
uniform vec3 cameraPosition;

void main()
{
	float ambient = 0.2;

	vec3 _normal = normalize(normal);
	vec3 lightDirection = normalize(lightPosition - vertexPosition);

	float diffuse = max(dot(_normal, lightDirection), 0.0);

	float specularLight = 0.5;
	vec3 viewDirection = normalize(cameraPosition - vertexPosition);
	vec3 reflectionDirection = reflect(-lightDirection, _normal);
	
	vec3 halfwayVector = normalize(viewDirection + lightDirection);
	
	float specAmount = pow(max(dot(normal, halfwayVector), 0.0), 8);

	float specular = 0.0;
	if (diffuse != 0.0)
	{
		specular = specAmount * specularLight;
	}

	outputColour = vec4(vertexColour*lightColour*(diffuse+ambient+specular), 1);
}