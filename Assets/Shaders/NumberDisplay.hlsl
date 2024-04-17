// This gist can be found at https://gist.github.com/bestknighter/660e6a53cf6a6643618d8531f962be2c
// I modified Aras Pranckevičius's awesome shader code to be able to handle Infinite and Not A Number numbers
// Tested on Unity 2023.1.0a15 with ShaderGraph 15.0.1.


// Quick try at doing a "print value" node for Unity ShaderGraph.
//
// Use with CustomFunction node, with two inputs:
// - Vector1 Value, the value to display,
// - Vector2 UV, the UVs of area to display at.
// And one output:
// - Vector4 Color, the color.
// Function name is DoDebug.

// "print in shader" based on this excellent ShaderToy by @P_Malin:
// https://www.shadertoy.com/view/4sBSWW

float DigitBin(const int x)
{
    return x==0?480599.0:x==1?139810.0:x==2?476951.0:x==3?476999.0:x==4?350020.0:x==5?464711.0:x==6?464727.0:x==7?476228.0:x==8?481111.0:x==9?481095.0:0.0;
}

float DigitLet(const int x)
{
    // 1=I, 2=n, 3=F, 4=a
    return x==1?467495.0:x==2?1877.0:x==3?463633.0:x==4?30069.0:0.0;
}

bool isnan_NonOptimizableAway(const float fValue) {
    return ((asuint(fValue) & 0x7FFFFFFF) > 0x7F800000);
}

bool isinf_NonOptimizableAway(const float fValue) {
    return ((asuint(fValue) & 0x7FFFFFFF) == 0x7F800000);
}

float GetDigit(const float fValue, const float fDigitIndex) {
    float res = fValue;
    for (int i = 0; i < fDigitIndex; i++) {
        res = res / 10;
    }
    for (int i = 0; i > fDigitIndex; i--) {
        res = res * 10;
    }
    return trunc(res % 10);
}

float PrintValue(float2 vStringCoords, float fValue, float fMaxDigits, float fDecimalPlaces)
{
    if ((vStringCoords.y < 0.0) || (vStringCoords.y >= 1.0))
        return 0.0;

    bool bNeg = (fValue < 0.0);
    bool bNan = isnan_NonOptimizableAway(fValue);
    bool bInf = isinf_NonOptimizableAway(fValue);
    if(bInf) fValue = 123.0;
    else if(bNan) fValue = 242.0;
    fValue = abs(fValue);

    float fLog10Value = log2(fValue) / log2(10.0);
    float fBiggestIndex = max(floor(fLog10Value), 0.0);
    float fDigitIndex = fMaxDigits - floor(vStringCoords.x);
    if (bInf || bNan) fDigitIndex += 2.0f; // Offset so Inf and nan are centralized
    float fCharBin = 0.0;
    if (fDigitIndex > (-fDecimalPlaces - 1.01))
    {
        if(fDigitIndex > fBiggestIndex)
        {
            if(fDigitIndex < (fBiggestIndex+1.5)) {
                if (bNeg) fCharBin = 1792.0; // Minus sign
                else if(bInf) fCharBin = 10016.0; // Plus sign
            }
        }
        else
        {
            if(fDigitIndex == -1.0)
            {
                if(fDecimalPlaces > 0.0 && !(bInf || bNan)) fCharBin = 2.0;
            }
            else
            {
                float fReducedRangeValue = fValue;
                if(fDigitIndex < 0.0) { fReducedRangeValue = frac( fValue ); fDigitIndex += 1.0; }
                //float fDigitValue = (abs(fReducedRangeValue / (pow(10.0, fDigitIndex)))); // Without workaround
                float fDigitValue = GetDigit(abs(fReducedRangeValue), fDigitIndex); // Workaround for precision issues found on more recent versions of Unity
                int arg = int(floor(fmod(fDigitValue, 10.0)));
                fCharBin = (bNan || bInf) ? DigitLet(arg) : DigitBin(arg);
            }
        }
    }
    return floor(fmod((fCharBin / pow(2.0, floor(frac(vStringCoords.x) * 4.0) + (floor(vStringCoords.y * 5.0) * 4.0))), 2.0));
}

float PrintValue(const in float2 fragCoord, const in float2 vPixelCoords, const in float2 vFontSize, const in float fValue, const in float fMaxDigits, const in float fDecimalPlaces)
{
    float2 vStringCharCoords = (fragCoord.xy - vPixelCoords) / vFontSize;
    return PrintValue( vStringCharCoords, fValue, fMaxDigits, fDecimalPlaces );
}

void DoDebug_float(float val, float2 uv, int decimalDigits, out float4 res)
{
    res = float4(0.3,0.2,0.1,1);
    res = PrintValue(uv*200, float2(10,100), float2(8,15), val, 10, decimalDigits);
}