shader_type spatial;


uniform sampler2D textureSampler;
void fragment() { 
	vec4 wordVertex = INV_VIEW_MATRIX * vec4(VERTEX,1.0);  
	ALBEDO = ( texture(textureSampler,UV)).rgb;
}
