shader_type spatial;
render_mode depth_draw_alpha_prepass, cull_disabled, specular_schlick_ggx;

uniform float New_Strength = 4.0;
uniform float Speed = 0.1;
uniform vec3 Direction = vec3(3.0, 2.0,0.0);
uniform float color_intencenty = 1.0;
uniform float offset_speed = 0.1;
uniform float scale_val = 10.0;
uniform float new_leafs_transform = 1.0;
uniform sampler2D texture_albedo;
uniform sampler2D texture_normal;


// GlobalExpression:0
	vec3 mod289_3(vec3 x) {
	    return x - floor(x * (1.0 / 289.0)) * 289.0;
	}
	
	vec2 mod289_2(vec2 x) {
	    return x - floor(x * (1.0 / 289.0)) * 289.0;
	}
	
	vec3 permute(vec3 x) {
	    return mod289_3(((x*34.0)+1.0)*x);
	}
	
	float snoise(vec2 v) {
	    vec4 C = vec4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
	                  0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
	                 -0.577350269189626,  // -1.0 + 2.0 * C.x
	                  0.024390243902439); // 1.0 / 41.0
	    // First corner
	    vec2 i  = floor(v + dot(v, C.yy) );
	    vec2 x0 = v -   i + dot(i, C.xx);
	    
	    // Other corners
	    vec2 i1;
	    //i1.x = step( x0.y, x0.x ); // x0.x > x0.y ? 1.0 : 0.0
	    //i1.y = 1.0 - i1.x;
	    i1 = (x0.x > x0.y) ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
	    // x0 = x0 - 0.0 + 0.0 * C.xx ;
	    // x1 = x0 - i1 + 1.0 * C.xx ;
	    // x2 = x0 - 1.0 + 2.0 * C.xx ;
	    vec4 x12 = vec4(x0.xy, x0.xy) + C.xxzz;
	    x12.xy -= i1;
	    
	    // Permutations
	    i = mod289_2(i); // Avoid truncation effects in permutation
	    vec3 p = permute( permute( i.y + vec3(0.0, i1.y, 1.0 ))
	    	+ i.x + vec3(0.0, i1.x, 1.0 ));
	    
	    vec3 m = max(0.5 - vec3(dot(x0,x0), dot(x12.xy,x12.xy), dot(x12.zw,x12.zw)), vec3(0.0));
	    m = m*m ;
	    m = m*m ;
	    
	    // Gradients: 41 points uniformly over a line, mapped onto a diamond.
	    // The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)
	    
	    vec3 x = 2.0 * fract(p * C.www) - 1.0;
	    vec3 h = abs(x) - 0.5;
	    vec3 ox = floor(x + 0.5);
	    vec3 a0 = x - ox;
	    
	    // Normalise gradients implicitly by scaling m
	    // Approximation of: m *= inversesqrt( a0*a0 + h*h );
	    m *= 1.79284291400159 - 0.85373472095314 * ( a0*a0 + h*h );
	    
	    // Compute final noise value at P
	    vec3 g;
	    g.x  = a0.x  * x0.x  + h.x  * x0.y;
	    g.yz = a0.yz * x12.xz + h.yz * x12.yw;
	    return 130.0 * dot(m, g);
	}

void vertex() {
	
	
// Input:88
	vec3 n_out88p0 = VERTEX;

// Input:48
	mat4 n_out48p0 = INV_CAMERA_MATRIX;

// Input:51
	mat4 n_out51p0 = CAMERA_MATRIX;

// Input:52
	vec3 n_out52p0 = VERTEX;

// TransformVectorMult:53
	vec3 n_out53p0 = (n_out51p0 * vec4(n_out52p0, 1.0)).xyz;

// Input:7
	vec3 n_out7p0 = VERTEX;

// VectorDecompose:8
	float n_out8p0 = n_out7p0.x;
	float n_out8p1 = n_out7p0.y;
	float n_out8p2 = n_out7p0.z;

// ScalarUniform:54
	float n_out54p0 = New_Strength;

// ScalarUniform:70
	float n_out70p0 = Speed;

// Input:67
	float n_out67p0 = TIME;

// ScalarOp:69
	float n_out69p0 = n_out70p0 * n_out67p0;

// ScalarFunc:68
	float n_out68p0 = cos(n_out69p0);

// ScalarOp:57
	float n_out57p0 = n_out54p0 * n_out68p0;

// ScalarOp:13
	float n_in13p1 = 100.00000;
	float n_out13p0 = n_out57p0 / n_in13p1;

// ScalarOp:11
	float n_out11p0 = n_out8p1 * n_out13p0;

// ScalarOp:14
	float n_in14p1 = 1.00000;
	float n_out14p0 = n_out11p0 + n_in14p1;

// ScalarOp:15
	float n_out15p0 = n_out14p0 * n_out14p0;

// ScalarOp:16
	float n_out16p0 = n_out15p0 * n_out15p0;

// ScalarOp:17
	float n_out17p0 = n_out16p0 - n_out15p0;

// VectorUniform:55
	vec3 n_out55p0 = Direction;

// VectorDecompose:93
	float n_out93p0 = n_out55p0.x;
	float n_out93p1 = n_out55p0.y;
	float n_out93p2 = n_out55p0.z;

// ScalarOp:96
	float n_out96p0 = n_out17p0 * n_out93p0;

// ScalarOp:94
	float n_out94p0 = n_out17p0 * n_out93p1;

// VectorCompose:21
	float n_in21p1 = 0.00000;
	vec3 n_out21p0 = vec3(n_out96p0, n_in21p1, n_out94p0);

// VectorOp:40
	vec3 n_out40p0 = n_out53p0 + n_out21p0;

// TransformVectorMult:50
	vec3 n_out50p0 = (n_out48p0 * vec4(n_out40p0, 1.0)).xyz;

// Input:71
	vec3 n_out71p0 = COLOR.rgb;

// VectorDecompose:72
	float n_out72p0 = n_out71p0.x;
	float n_out72p1 = n_out71p0.y;
	float n_out72p2 = n_out71p0.z;

// VectorCompose:100
	float n_in100p2 = 0.00000;
	vec3 n_out100p0 = vec3(n_out72p0, n_out72p1, n_in100p2);

// VectorScalarMix:95
	vec3 n_out95p0 = mix(n_out88p0, n_out50p0, dot(n_out100p0, vec3(0.333333, 0.333333, 0.333333)));

// Input:98
	vec3 n_out98p0 = VERTEX;

// ScalarUniform:86
	float n_out86p0 = offset_speed;

// ScalarUniform:87
	float n_out87p0 = scale_val;

// Expression:85
	vec3 n_out85p0;
	n_out85p0 = vec3(0.0, 0.0, 0.0);
	{
		
		vec2 offset =  vec2(n_out86p0, n_out86p0) * TIME ;
		float noise = snoise((UV + offset) * n_out87p0) * 0.5 + 0.5;
		
		n_out85p0 =  vec3(noise) * new_leafs_transform;
		
	}

// VectorMix:104
	vec3 n_out104p0 = mix(n_out98p0, n_out95p0, n_out85p0);

// Input:90
	vec3 n_out90p0 = COLOR.rgb;

// VectorDecompose:91
	float n_out91p0 = n_out90p0.x;
	float n_out91p1 = n_out90p0.y;
	float n_out91p2 = n_out90p0.z;

// VectorScalarMix:99
	vec3 n_out99p0 = mix(n_out95p0, n_out104p0, 0.5);

// Output:0
	VERTEX = n_out99p0;
	

}

void fragment() {
	
	
// Input:19
	vec3 n_out19p0 = vec3(UV, 0.0);

// TextureUniform:17
	vec3 n_out17p0;
	float n_out17p1;
	{
		vec4 n_tex_read = texture(texture_albedo, n_out19p0.xy);
		n_out17p0 = n_tex_read.rgb;
		n_out17p1 = n_tex_read.a;
	}

// Input:26
	vec3 n_out26p0 = TANGENT;

// TextureUniform:18
	vec3 n_out18p0;
	float n_out18p1;
	{
		vec4 n_tex_read = texture(texture_normal, n_out19p0.xy);
		n_out18p0 = n_tex_read.rgb;
		n_out18p1 = n_tex_read.a;
	}

// VectorOp:21
	vec3 n_in21p1 = vec3(2.00000, 2.00000, 2.00000);
	vec3 n_out21p0 = n_out18p0 * n_in21p1;

// VectorOp:34
	vec3 n_in34p1 = vec3(1.00000, 1.00000, 1.00000);
	vec3 n_out34p0 = n_out21p0 - n_in34p1;

// VectorDecompose:28
	float n_out28p0 = n_out34p0.x;
	float n_out28p1 = n_out34p0.y;
	float n_out28p2 = n_out34p0.z;

// VectorOp:27
	vec3 n_out27p0 = n_out26p0 * vec3(n_out28p1);

// Input:25
	vec3 n_out25p0 = BINORMAL;

// VectorOp:29
	vec3 n_out29p0 = vec3(n_out28p0) * n_out25p0;

// VectorOp:32
	vec3 n_out32p0 = n_out27p0 + n_out29p0;

// Input:31
	vec3 n_out31p0 = NORMAL;

// VectorOp:30
	vec3 n_out30p0 = vec3(n_out28p2) * n_out31p0;

// VectorOp:33
	vec3 n_out33p0 = n_out32p0 + n_out30p0;

// Output:0
	ALBEDO = n_out17p0;
	ALPHA = n_out17p1;
	NORMAL = n_out33p0;
	NORMALMAP = n_out34p0;
	ROUGHNESS = 1.0;
	
	

}
