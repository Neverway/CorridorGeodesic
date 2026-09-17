// ─────────────────────────────────────────────────────────────────────────────
// InteriorMap_AnimTex.hlsl  (fixed)
//
// Changes vs original:
//   1. UV seam fix  – world-space intersect coords are frac()'d into the local
//      room cell, identical approach to InteriorMap.hlsl fix.
//   2. Room height  – new `roomHeight` parameter for independent Y cell size.
// ─────────────────────────────────────────────────────────────────────────────

float4 checkIfCloser(float3 rayDir, float3 rayStartPos, float3 planePos, float3 planeNormal,
                     UnityTexture2DArray PlaneTex, float index, float4 colorAndDist,
                     UnitySamplerState ss, float roomCount, float roomHeight)
{
    float t = dot(planePos - rayStartPos, planeNormal) / dot(planeNormal, rayDir);

    float3 intersectPos = rayStartPos + rayDir * t;
    float2 ipos;

    float wallDistance   = 1.0 / roomCount;
    float heightDistance = wallDistance * roomHeight;

    // ── UV seam fix ──────────────────────────────────────────────────────────
    if (abs(planeNormal.x) == 1)
    {
        ipos = float2(frac(intersectPos.z / wallDistance),
                      frac(intersectPos.y / heightDistance));
    }
    else if (abs(planeNormal.y) == 1)
    {
        ipos = float2(frac(intersectPos.x / wallDistance),
                      frac(intersectPos.z / wallDistance));
    }
    else
    {
        ipos = float2(frac(planeNormal.z * intersectPos.x / wallDistance),
                      frac(intersectPos.y / heightDistance));
    }
    // ─────────────────────────────────────────────────────────────────────────

    if (t < colorAndDist.w)
    {
        float4 color = SAMPLE_TEXTURE2D_ARRAY(PlaneTex, ss, ipos, index);
        if (color.a > 0.9)
        {
            colorAndDist.w   = t;
            colorAndDist.rgb = color.rgb;
        }
    }

    return colorAndDist;
}

// Returns 0..maxvalue-1 stepping through every second, cycling back to 0.
float GetIncermentingValue(float maxvalue)
{
    float count       = _Time.y;
    float fraction    = frac(count);
    float roundValue  = floor(fraction * 10);
    float adjustedvalue = floor(roundValue * (maxvalue / 10));
    return adjustedvalue;
}

// roomHeight: Y cell multiplier (1.0 = original cube room, 0.5 = lower ceiling)
void InteriorMapping_float(float3 objectViewDir, float3 objectPos,
                           float roomCount, float roomHeight,
                           UnitySamplerState ss,
                           UnityTexture2DArray CubeTex, UnityTexture2DArray FurnitureTex,
                           out float4 colorAndDist)
{
    float3 rayDir      = normalize(objectViewDir);
    float3 rayStartPos = objectPos + rayDir * 0.0001;

    colorAndDist = float4(1.0, 1.0, 1.0, 100000000.0);

    float wallDistance   = 1.0 / roomCount;
    float heightDistance = wallDistance * roomHeight;   // ← independent Y step

    float3 upVec      = float3(0, 1, 0);
    float3 rightVec   = float3(1, 0, 0);
    float3 forwardVec = float3(0, 0, 1);

    // ── Ceiling / Floor (Y) ──────────────────────────────────────────────────
    if (dot(upVec, rayDir) > 0)
    {
        float3 wallPos = (ceil(rayStartPos.y / heightDistance) * heightDistance) * upVec;
        colorAndDist = checkIfCloser(rayDir, rayStartPos, wallPos, upVec,
                                     CubeTex, 0, colorAndDist, ss, roomCount, roomHeight);
    }
    else
    {
        float3 wallPos = ((ceil(rayStartPos.y / heightDistance) - 1.0) * heightDistance) * upVec;
        colorAndDist = checkIfCloser(rayDir, rayStartPos, wallPos, upVec * -1,
                                     CubeTex, 1, colorAndDist, ss, roomCount, roomHeight);
    }

    // ── Left / Right (X) ────────────────────────────────────────────────────
    if (dot(rightVec, rayDir) > 0)
    {
        float3 wallPos = (ceil(rayStartPos.x / wallDistance) * wallDistance) * rightVec;
        colorAndDist = checkIfCloser(rayDir, rayStartPos, wallPos, rightVec,
                                     CubeTex, 2, colorAndDist, ss, roomCount, roomHeight);

        if (dot(rightVec, rayDir) > 0.6)
        {
            float3 furniturePos = float3(wallPos.x - 0.1, 0, 0);
            colorAndDist = checkIfCloser(rayDir, rayStartPos, furniturePos, rightVec,
                                         FurnitureTex, GetIncermentingValue(5),
                                         colorAndDist, ss, roomCount, roomHeight);
        }
    }
    else
    {
        float3 wallPos = ((ceil(rayStartPos.x / wallDistance) - 1.0) * wallDistance) * rightVec;
        colorAndDist = checkIfCloser(rayDir, rayStartPos, wallPos, rightVec * -1,
                                     CubeTex, 3, colorAndDist, ss, roomCount, roomHeight);
    }

    // ── Front / Back (Z) ────────────────────────────────────────────────────
    if (dot(forwardVec, rayDir) > 0)
    {
        float3 wallPos = (ceil(rayStartPos.z / wallDistance) * wallDistance) * forwardVec;
        colorAndDist = checkIfCloser(rayDir, rayStartPos, wallPos, forwardVec,
                                     CubeTex, 4, colorAndDist, ss, roomCount, roomHeight);
    }
    else
    {
        float3 wallPos = ((ceil(rayStartPos.z / wallDistance) - 1.0) * wallDistance) * forwardVec;
        colorAndDist = checkIfCloser(rayDir, rayStartPos, wallPos, forwardVec * -1,
                                     CubeTex, 5, colorAndDist, ss, roomCount, roomHeight);
    }
}
