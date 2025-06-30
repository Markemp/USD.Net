using System;
using Pxr.Base.Vt;

namespace Pxr.Usd;

/// <summary>
/// USD interpolation types for time-varying attributes.
/// </summary>
public enum UsdInterpolationType
{
    /// <summary>
    /// Held (stepped) interpolation - use the lower bracketing value.
    /// This is the default interpolation type.
    /// </summary>
    Held,

    /// <summary>
    /// Linear interpolation between bracketing time samples.
    /// Only supported for numeric types.
    /// </summary>
    Linear
}

/// <summary>
/// Static utility class for performing interpolation between time samples.
/// </summary>
public static class UsdInterpolation
{
    /// <summary>
    /// Interpolate between two values at the specified parametric time.
    /// </summary>
    public static bool Interpolate(VtValue lowerValue, VtValue upperValue, double alpha, UsdInterpolationType interpolationType, out VtValue result)
    {
        result = VtValue.CreateEmpty();

        if (lowerValue.IsEmpty() && upperValue.IsEmpty())
            return false;

        // If only one value is valid, use it
        if (lowerValue.IsEmpty())
        {
            result = upperValue;
            return true;
        }
        if (upperValue.IsEmpty())
        {
            result = lowerValue;
            return true;
        }

        // For held interpolation, always return the lower value
        if (interpolationType == UsdInterpolationType.Held)
        {
            result = lowerValue;
            return true;
        }

        // For linear interpolation, check if types support it
        if (interpolationType == UsdInterpolationType.Linear)
        {
            return LinearInterpolate(lowerValue, upperValue, alpha, out result);
        }

        // Fallback to held
        result = lowerValue;
        return true;
    }

    /// <summary>
    /// Perform linear interpolation between two values.
    /// </summary>
    private static bool LinearInterpolate(VtValue lowerValue, VtValue upperValue, double alpha, out VtValue result)
    {
        result = VtValue.CreateEmpty();

        // Clamp alpha to [0, 1]
        alpha = Math.Clamp(alpha, 0.0, 1.0);

        // Handle exact endpoints
        if (alpha <= 0.0)
        {
            result = lowerValue;
            return true;
        }
        if (alpha >= 1.0)
        {
            result = upperValue;
            return true;
        }

        // Try to interpolate based on the value type
        var lowerType = lowerValue.GetHeldType();
        var upperType = upperValue.GetHeldType();

        if (lowerType != upperType)
        {
            // Type mismatch - fallback to held interpolation
            result = lowerValue;
            return true;
        }

        // Interpolate based on type
        if (lowerType == typeof(float))
            return InterpolateFloat(lowerValue, upperValue, alpha, out result);
        if (lowerType == typeof(double))
            return InterpolateDouble(lowerValue, upperValue, alpha, out result);
        if (lowerType == typeof(int))
            return InterpolateInt(lowerValue, upperValue, alpha, out result);

        // Unsupported type for linear interpolation - fallback to held
        result = lowerValue;
        return true;
    }

    private static bool InterpolateFloat(VtValue lowerValue, VtValue upperValue, double alpha, out VtValue result)
    {
        result = VtValue.CreateEmpty();

        if (lowerValue.IsHolding<float>() && upperValue.IsHolding<float>())
        {
            var lower = lowerValue.Get<float>();
            var upper = upperValue.Get<float>();
            var interpolated = (float)(lower + alpha * (upper - lower));
            result = new VtValue(interpolated);
            return true;
        }

        return false;
    }

    private static bool InterpolateDouble(VtValue lowerValue, VtValue upperValue, double alpha, out VtValue result)
    {
        result = VtValue.CreateEmpty();

        if (lowerValue.IsHolding<double>() && upperValue.IsHolding<double>())
        {
            var lower = lowerValue.Get<double>();
            var upper = upperValue.Get<double>();
            var interpolated = lower + alpha * (upper - lower);
            result = new VtValue(interpolated);
            return true;
        }

        return false;
    }

    private static bool InterpolateInt(VtValue lowerValue, VtValue upperValue, double alpha, out VtValue result)
    {
        result = VtValue.CreateEmpty();

        if (lowerValue.IsHolding<int>() && upperValue.IsHolding<int>())
        {
            var lower = lowerValue.Get<int>();
            var upper = upperValue.Get<int>();
            // For integers, we interpolate as double then round
            var interpolated = (int)Math.Round(lower + alpha * (upper - lower));
            result = new VtValue(interpolated);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Return true if the given type supports linear interpolation.
    /// </summary>
    public static bool TypeSupportsLinearInterpolation(string typeName)
    {
        return typeName switch
        {
            "float" => true,
            "double" => true,
            "int" => true,
            // TODO: Add support for vectors, matrices, quaternions
            _ => false
        };
    }
}