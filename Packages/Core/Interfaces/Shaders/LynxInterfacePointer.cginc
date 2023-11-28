//#ifndef LYNX_INTERFACE_POINTER_INCULDED
//#define LYNX_INTERFACE_POINTER_INCULDED


#define PI            3.14159265359f

//Define global variables that will be set by LynxUIPointerManager on LynxEventSystem
float4	_PointerRPos;
float4	_PointerLPos;
float4	_PointerRDir;
float4	_PointerLDir;
float	_PointerSize;
float	_PointerDist;
float4  _PointerColor;


//Return a capsule SDF 
//p = worldPosition of the pixel
//a = start of the capsule
//b = end of the capsule
//r = radius of the capsule
float sdCapsule( float4 p, float4 a, float4 b, float r)
{
	float4 pa = p-a, ba = b-a;
	float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
	return length( pa - ba*h )- r;
}


// return a normalizedValue for the pointer 
//1 to it center and 0 to the sides
float getPointerNormalizedCapsule(float4 worldPosition, float4 pointerPos, float4 pointerDir)
{
    float4 rayEnd = pointerPos + (pointerDir * _PointerDist);
    rayEnd.w = 1.0;

    float pSize = distance(pointerPos, worldPosition);
    pSize = pSize/_PointerDist;

    float scaleFactor = lerp(0.0,_PointerSize, pSize);

    float rayDF = sdCapsule(worldPosition, pointerPos, rayEnd, _PointerSize);
    rayDF = min(rayDF, 0);

    float normalizedValue = 0.0 - (rayDF/_PointerSize);

    float nrmPointDist = pow(clamp(length(pointerPos-worldPosition)/_PointerDist,0.0,1.0),0.5);
    float subDF = clamp(normalizedValue - (1.0-nrmPointDist),0.0,1.0);

    return subDF;
}

//point a SDF capsule in direction. 
//Use this SDF to calculate pointer size & shape
float4 pointerColor(float4 worldPosition)
{
    float rayDF = getPointerNormalizedCapsule(worldPosition, _PointerLPos, _PointerLDir);
    rayDF = max(rayDF, getPointerNormalizedCapsule(worldPosition, _PointerRPos, _PointerRDir));

    float4 col = _PointerColor;
    col.w = col.w*rayDF;
    return col;
}
