// LCDOverlay.hlsl
// Drop this next to your InteriorMap.hlsl and include it in a Custom Function node in Shader Graph.
// Applies the LCD subpixel pixel mask effect on top of an existing colour input.
//
// Inputs:
//   inputColor   - float3, the colour coming out of InteriorMapping (just the .rgb of colorAndDist)
//   uv           - float2, the surface UV (UV0 node in Shader Graph)
//   pixelMask    - UnityTexture2D, the RGB subpixel mask texture from the LCD package
//   ss           - UnitySamplerState, sampler (reuse the same one from InteriorMapping)
//   pixelScale   - float, scale the virtual pixel grid. 1 = native texel size, 2 = 2x bigger pixels, etc.
//   pixelLuma    - float, brightness multiplier for the mask (4 is a good default)
//
// Output:
//   result       - float3, inputColor with the LCD pixel mask applied

void LCDOverlay_float(
    float3 inputColor,
    float2 uv,
    UnityTexture2D pixelMask,
    UnitySamplerState ss,
    float pixelScale,
    float pixelLuma,
    out float3 result)
{
    // Build a virtual texel size from pixelScale.
    // pixelScale controls how large the fake "pixels" are in UV space.
    // Higher = bigger/chunkier pixels. Lower = finer pixels.
    float2 virtualTexelSize = pixelScale * 0.01;  // 0.01 gives a good baseline; tweak if needed

    // Compute screen-space derivatives of UV for mipmap LOD calculation
    float2 duvdx = ddx(uv);
    float2 duvdy = ddy(uv);

    // Scale derivatives into pixel-grid space
    float2 dpdx = duvdx / virtualTexelSize;
    float2 dpdy = duvdy / virtualTexelSize;

    // Pixel mask UV - how many fake pixels fit across the surface
    float2 pixelMaskUV = uv / virtualTexelSize;

    // Snap UV to pixel centres for the pixelized version
    float2 pixelizedUV = (floor(pixelMaskUV) + float2(0.5, 0.5)) * virtualTexelSize;

    // Compute mip level to blend between pixelized and smooth as camera pulls back
    // ComputeTextureLOD is from URP's core library
    half mipmapLevel = ComputeTextureLOD(dpdx, dpdy, float2(1.0 / virtualTexelSize.x, 1.0 / virtualTexelSize.y));

    half pixelization = saturate((mipmapLevel - 1.0) / (4.0 - 1.0));
    half pixelremoval = saturate((mipmapLevel - 3.0) / (4.0 - 3.0));

    // Blend between pixelized and original UV based on distance
    float2 sampledUV = lerp(pixelizedUV, uv, pixelization);

    // Sample the pixel mask at the pixel-grid UV
    half3 pixelMaskColor = SAMPLE_TEXTURE2D_GRAD(pixelMask, ss, pixelMaskUV, dpdx, dpdy).rgb;
    pixelMaskColor *= pixelLuma;
    // Fade out the mask effect when too far away (pixelremoval = 1 means show plain colour)
    pixelMaskColor = lerp(pixelMaskColor, half3(1.0, 1.0, 1.0), pixelremoval);

    result = inputColor * pixelMaskColor;
}
