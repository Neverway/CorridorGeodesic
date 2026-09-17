// ─────────────────────────────────────────────────────────────────────────────
// InteriorMap.hlsl
//
// Changes:
//   1. UV seam fix    – frac() into local room cell before sampling.
//   2. Room height    – roomHeight scales Y cell independently from X/Z.
//   3. Wall UV scale  – ScaleRight/Left/Front/Back are now float2 (U,V)
//                       so tiling can be corrected when ceiling height changes.
//                       ScaleTop/Bottom remain float (ceiling/floor is square).
// ─────────────────────────────────────────────────────────────────────────────

float4 checkIfCloser(float3 rayDir, float3 rayStartPos, float3 planePos, float3 planeNormal,
                     UnityTexture2D PlaneTex, float4 colorAndDist, UnitySamplerState ss,
                     float roomCount, float roomHeight, float2 texOffset, float2 texScale)
{
    float t = dot(planePos - rayStartPos, planeNormal) / dot(planeNormal, rayDir);
    if (t <= 0) return colorAndDist;

    float3 intersectPos = rayStartPos + rayDir * t;
    float2 ipos;

    float wallDistance   = 1.0 / roomCount;
    float heightDistance = wallDistance * roomHeight;

    if (abs(planeNormal.x) == 1)
    {
        // Left / Right  →  U = Z in cell, V = Y in cell
        ipos = float2(frac(intersectPos.z / wallDistance),
                      frac(intersectPos.y / heightDistance));
    }
    else if (abs(planeNormal.y) == 1)
    {
        // Ceiling / Floor  →  U = X in cell, V = Z in cell
        ipos = float2(frac(intersectPos.x / wallDistance),
                      frac(intersectPos.z / wallDistance));
    }
    else
    {
        // Front / Back  →  U = X in cell (with facing sign), V = Y in cell
        ipos = float2(frac(planeNormal.z * intersectPos.x / wallDistance),
                      frac(intersectPos.y / heightDistance));
    }

    if (t < colorAndDist.w)
    {
        colorAndDist.w   = t;
        colorAndDist.rgb = SAMPLE_TEXTURE2D(PlaneTex, ss, ipos * texScale + texOffset);
    }

    return colorAndDist;
}

// roomHeight   : Y cell multiplier. 1.0 = cubic room. 0.5 = lower ceiling.
// ScaleTop/Bot : float  – ceiling/floor cell is always square, one value suffices.
// ScaleRight/Left/Front/Back : float2(U,V) – lets you correct wall tiling when
//                              roomHeight != 1.  Typical starting point:
//                              x = 1.0, y = roomHeight  (keeps texels square).
void InteriorMapping_float(float3 worldViewDir, float3 worldPos, float roomCount, float roomHeight,
    UnitySamplerState ss,
    UnityTexture2D TexTop,   UnityTexture2D TexBottom,
    UnityTexture2D TexRight, UnityTexture2D TexLeft,
    UnityTexture2D TexFront, UnityTexture2D TexBack,
    float2 OffsetTop,   float2 OffsetBottom,
    float2 OffsetRight, float2 OffsetLeft,
    float2 OffsetFront, float2 OffsetBack,
    float  ScaleTop,    float  ScaleBottom,
    float2 ScaleRight,  float2 ScaleLeft,
    float2 ScaleFront,  float2 ScaleBack,
    out float4 colorAndDist)
{
    float3 rayDir      = normalize(worldViewDir);
    float3 rayStartPos = worldPos + rayDir * 0.0001;

    colorAndDist = float4(1, 1, 1, 1e9);

    float wallDistance   = 1.0 / roomCount;
    float heightDistance = wallDistance * roomHeight;

    float3 upVec      = float3(0, 1, 0);
    float3 rightVec   = float3(1, 0, 0);
    float3 forwardVec = float3(0, 0, 1);

    // ── Ceiling / Floor (Y) ──────────────────────────────────────────────────
    // Scalar scale broadcast to float2 at the call site so checkIfCloser stays uniform.
    if (dot(upVec, rayDir) > 0)
    {
        float3 wallPos = (ceil(rayStartPos.y / heightDistance) * heightDistance) * upVec;
        colorAndDist = checkIfCloser(rayDir, rayStartPos, wallPos, upVec,
                                     TexTop, colorAndDist, ss, roomCount, roomHeight,
                                     OffsetTop, float2(ScaleTop, ScaleTop));
    }
    else
    {
        float3 wallPos = ((ceil(rayStartPos.y / heightDistance) - 1.0) * heightDistance) * upVec;
        colorAndDist = checkIfCloser(rayDir, rayStartPos, wallPos, -upVec,
                                     TexBottom, colorAndDist, ss, roomCount, roomHeight,
                                     OffsetBottom, float2(ScaleBottom, ScaleBottom));
    }

    // ── Left / Right (X) ────────────────────────────────────────────────────
    if (dot(rightVec, rayDir) > 0)
    {
        float3 wallPos = (ceil(rayStartPos.x / wallDistance) * wallDistance) * rightVec;
        colorAndDist = checkIfCloser(rayDir, rayStartPos, wallPos, rightVec,
                                     TexRight, colorAndDist, ss, roomCount, roomHeight,
                                     OffsetRight, ScaleRight);
    }
    else
    {
        float3 wallPos = ((ceil(rayStartPos.x / wallDistance) - 1.0) * wallDistance) * rightVec;
        colorAndDist = checkIfCloser(rayDir, rayStartPos, wallPos, -rightVec,
                                     TexLeft, colorAndDist, ss, roomCount, roomHeight,
                                     OffsetLeft, ScaleLeft);
    }

    // ── Front / Back (Z) ────────────────────────────────────────────────────
    if (dot(forwardVec, rayDir) > 0)
    {
        float3 wallPos = (ceil(rayStartPos.z / wallDistance) * wallDistance) * forwardVec;
        colorAndDist = checkIfCloser(rayDir, rayStartPos, wallPos, forwardVec,
                                     TexFront, colorAndDist, ss, roomCount, roomHeight,
                                     OffsetFront, ScaleFront);
    }
    else
    {
        float3 wallPos = ((ceil(rayStartPos.z / wallDistance) - 1.0) * wallDistance) * forwardVec;
        colorAndDist = checkIfCloser(rayDir, rayStartPos, wallPos, -forwardVec,
                                     TexBack, colorAndDist, ss, roomCount, roomHeight,
                                     OffsetBack, ScaleBack);
    }
}
