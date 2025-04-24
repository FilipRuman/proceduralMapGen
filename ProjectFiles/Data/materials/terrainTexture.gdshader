shader_type spatial;

uniform int usedLayersCount;
const int layersCount = 30;

uniform float[layersCount] brightness;
uniform vec3[layersCount] tint;
uniform float[layersCount] tintStrength;
uniform float[layersCount] textureScale;
uniform float[layersCount] textureSaturation;
uniform sampler2D[layersCount] textures;
uniform sampler2D[layersCount] visibilityHeight;

uniform vec2 minMaxHeight;
uniform float globalScale;
uniform float globalBrightness;
uniform float saturation;

vec4 Triplanar (vec3 position, vec3 worldNormal, sampler2D sampler){
	vec3 blendAxes = abs(worldNormal);
	blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

	vec4 xProjection = blendAxes.x * texture(sampler,position.yz );
	vec4 yProjection = blendAxes.y * texture(sampler,position.xz );
	vec4 zProjection = blendAxes.z * texture(sampler,position.xy );

	return xProjection + yProjection + zProjection;
}
float InvLerp(float a, float b, float v){
	return clamp((v-a) / (b-a),0,1);
}

void fragment() {
	vec3 worldPosition = ( INV_VIEW_MATRIX * vec4(VERTEX,1.0) ).xyz;
	vec3 worldNormal = (INV_VIEW_MATRIX * vec4(NORMAL, 0.0)).xyz;
	float heightPercentage =InvLerp(minMaxHeight.x,minMaxHeight.y,worldPosition.y);
	worldPosition /= globalScale;

	 for (int i = 0; i <usedLayersCount; i++) {
	 	float drawStrength =texture(visibilityHeight[i],vec2(heightPercentage)).r;
	 	vec3 tintColor = tint[i] * tintStrength[i];

	 	vec3 textureColor = Triplanar(worldPosition / textureScale[i],worldNormal,textures[i]).rgb * (1.- tintStrength[i]) - vec3(1) * textureSaturation[i];
	 	//? * or +
	 	ALBEDO = ALBEDO * (1.-drawStrength) + (tintColor + textureColor) * drawStrength   ;
	 }

	ALBEDO *= globalBrightness;
	ALBEDO -= vec3(1) *saturation;

//Debug
// ALBEDO = Triplanar(worldPosition,worldNormal,textures[0]).rgb;
}
