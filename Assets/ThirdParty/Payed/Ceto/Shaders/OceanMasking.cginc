#ifndef CETO_MASKING_INCLUDED
#define CETO_MASKING_INCLUDED

float IsUnderWater(float4 mask)
{

	float error = 0.01;
	float isUnderwater = 0.0;

	if (mask.x <= TOP_MASK + error)
		isUnderwater = 0.0;
	else
		isUnderwater = 1.0;
	
	return isUnderwater;
	
}

float IsOceanSurface(float4 mask)
{

	float isOceanSurface = 0.0;

	if (mask.x > EMPTY_MASK && mask.x < BOTTOM_MASK)
		isOceanSurface = 1.0;
	else
		isOceanSurface = 0.0;

	return isOceanSurface;

}
	



#endif