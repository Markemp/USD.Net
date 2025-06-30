using System.Linq;
using Pxr.Base.Vt;
using Pxr.Usd;
using Xunit;

namespace USD.Net.Tests;

public class UsdTimeSamplingTests
{
    #region UsdTimeCode Tests

    [Fact]
    public void UsdTimeCode_Default_ShouldBeValid()
    {
        // Act
        var defaultTime = UsdTimeCode.Default();

        // Assert
        Assert.True(defaultTime.IsDefault());
        Assert.False(defaultTime.IsNumeric());
        Assert.False(defaultTime.IsEarliestTime());
    }

    [Fact]
    public void UsdTimeCode_NumericTime_ShouldBeValid()
    {
        // Act
        var numericTime = UsdTimeCode.Create(1.0);

        // Assert
        Assert.False(numericTime.IsDefault());
        Assert.True(numericTime.IsNumeric());
        Assert.False(numericTime.IsEarliestTime());
        Assert.Equal(1.0, numericTime.GetValue());
    }

    [Fact]
    public void UsdTimeCode_EarliestTime_ShouldBeValid()
    {
        // Act
        var earliestTime = UsdTimeCode.EarliestTime();

        // Assert
        Assert.False(earliestTime.IsDefault());
        Assert.False(earliestTime.IsNumeric());
        Assert.True(earliestTime.IsEarliestTime());
    }

    [Fact]
    public void UsdTimeCode_ImplicitConversion_ShouldWork()
    {
        // Act
        UsdTimeCode timeFromDouble = 5.0;
        double doubleFromTime = (double)UsdTimeCode.Create(3.0);

        // Assert
        Assert.Equal(5.0, timeFromDouble.GetValue());
        Assert.Equal(3.0, doubleFromTime);
    }

    [Fact]
    public void UsdTimeCode_Arithmetic_ShouldWork()
    {
        // Arrange
        var time = UsdTimeCode.Create(10.0);

        // Act
        var plus = time + 5.0;
        var minus = time - 3.0;
        var diff = time - UsdTimeCode.Create(7.0);

        // Assert
        Assert.Equal(15.0, plus.GetValue());
        Assert.Equal(7.0, minus.GetValue());
        Assert.Equal(3.0, diff);
    }

    [Fact]
    public void UsdTimeCode_ArithmeticOnSpecialValues_ShouldBeUnchanged()
    {
        // Arrange
        var defaultTime = UsdTimeCode.Default();
        var earliestTime = UsdTimeCode.EarliestTime();

        // Act
        var defaultPlus = defaultTime + 10.0;
        var earliestMinus = earliestTime - 5.0;

        // Assert
        Assert.True(defaultPlus.IsDefault());
        Assert.True(earliestMinus.IsEarliestTime());
    }

    [Fact]
    public void UsdTimeCode_Comparison_ShouldRespectHierarchy()
    {
        // Arrange
        var defaultTime = UsdTimeCode.Default();
        var numericTime = UsdTimeCode.Create(5.0);
        var earlierTime = UsdTimeCode.Create(1.0);

        // Assert - Default is "less than" numeric time
        Assert.True(defaultTime < numericTime);
        Assert.True(numericTime > defaultTime);

        // Numeric comparison
        Assert.True(earlierTime < numericTime);
        Assert.True(numericTime > earlierTime);

        // Equality
        Assert.True(UsdTimeCode.Default() == UsdTimeCode.Default());
        Assert.True(UsdTimeCode.Create(5.0) == UsdTimeCode.Create(5.0));
    }

    [Fact]
    public void UsdTimeCode_SafeStep_ShouldProvideEpsilon()
    {
        // Act
        var step = UsdTimeCode.SafeStep();

        // Assert
        Assert.True(step > 0.0);
        Assert.True(step < 0.01); // Should be a small epsilon
    }

    #endregion

    #region Time Sampling Tests

    [Fact]
    public void UsdAttribute_SetTimeSamples_ShouldStoreValues()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        var attr = prim.CreateAttribute("test", "float");

        // Act
        attr.Set(1.0f, UsdTimeCode.Create(1.0));
        attr.Set(2.0f, UsdTimeCode.Create(2.0));
        attr.Set(3.0f, UsdTimeCode.Create(3.0));

        // Assert
        Assert.True(attr.HasAuthoredTimeSamples());
        Assert.Equal(3, attr.GetNumTimeSamples());
        Assert.True(attr.ValueMightBeTimeVarying());

        var times = attr.GetTimeSamples();
        Assert.Equal(new[] { 1.0, 2.0, 3.0 }, times);
    }

    [Fact]
    public void UsdAttribute_GetAtExactTime_ShouldReturnExactValue()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        var attr = prim.CreateAttribute("test", "float");
        
        attr.Set(10.0f, UsdTimeCode.Create(1.0));
        attr.Set(20.0f, UsdTimeCode.Create(2.0));

        // Act
        var success1 = attr.Get(out VtValue value1, UsdTimeCode.Create(1.0));
        var success2 = attr.Get(out VtValue value2, UsdTimeCode.Create(2.0));

        // Assert
        Assert.True(success1);
        Assert.True(success2);
        var float1 = value1.Get<float>();
        var float2 = value2.Get<float>();
        Assert.Equal(10.0f, float1);
        Assert.Equal(20.0f, float2);
    }

    [Fact]
    public void UsdAttribute_GetBetweenSamples_ShouldInterpolate()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        var attr = prim.CreateAttribute("test", "float");
        
        attr.Set(10.0f, UsdTimeCode.Create(1.0));
        attr.Set(20.0f, UsdTimeCode.Create(3.0));

        // Act - Get value at time 2.0 (halfway between 1.0 and 3.0)
        var success = attr.Get(out VtValue value, UsdTimeCode.Create(2.0));

        // Assert
        Assert.True(success);
        Assert.True(value.IsHolding<float>());
        var floatValue = value.Get<float>();
        // With held interpolation (default), should return lower value
        Assert.Equal(10.0f, floatValue);
    }

    [Fact]
    public void UsdAttribute_GetBracketingTimeSamples_ShouldFindCorrectBrackets()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        var attr = prim.CreateAttribute("test", "float");
        
        attr.Set(10.0f, UsdTimeCode.Create(1.0));
        attr.Set(20.0f, UsdTimeCode.Create(3.0));
        attr.Set(30.0f, UsdTimeCode.Create(5.0));

        // Act
        var success = attr.GetBracketingTimeSamples(2.5, out var lower, out var upper, out var hasTimeSamples);

        // Assert
        Assert.True(success);
        Assert.True(hasTimeSamples);
        Assert.Equal(1.0, lower);
        Assert.Equal(3.0, upper);
    }

    [Fact]
    public void UsdAttribute_GetTimeSamplesInInterval_ShouldFilterCorrectly()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        var attr = prim.CreateAttribute("test", "float");
        
        attr.Set(10.0f, UsdTimeCode.Create(1.0));
        attr.Set(20.0f, UsdTimeCode.Create(2.0));
        attr.Set(30.0f, UsdTimeCode.Create(3.0));
        attr.Set(40.0f, UsdTimeCode.Create(4.0));
        attr.Set(50.0f, UsdTimeCode.Create(5.0));

        // Act
        var samples = attr.GetTimeSamplesInInterval(UsdTimeCode.Create(2.0), UsdTimeCode.Create(4.0));

        // Assert
        Assert.Equal(new[] { 2.0, 3.0, 4.0 }, samples);
    }

    [Fact]
    public void UsdAttribute_DefaultValue_ShouldWorkWithTimeSamples()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        var attr = prim.CreateAttribute("test", "float");
        
        // Set default value and time samples
        attr.Set(100.0f, UsdTimeCode.Default());
        attr.Set(10.0f, UsdTimeCode.Create(1.0));
        attr.Set(20.0f, UsdTimeCode.Create(2.0));

        // Act
        var defaultSuccess = attr.Get(out VtValue defaultValue, UsdTimeCode.Default());
        var timeSuccess = attr.Get(out VtValue timeValue, UsdTimeCode.Create(1.0));

        // Assert
        Assert.True(defaultSuccess);
        Assert.True(timeSuccess);
        Assert.True(defaultValue.IsHolding<float>());
        Assert.True(timeValue.IsHolding<float>());
        var defaultFloat = defaultValue.Get<float>();
        var timeFloat = timeValue.Get<float>();
        Assert.Equal(100.0f, defaultFloat);
        Assert.Equal(10.0f, timeFloat);
    }

    [Fact]
    public void UsdAttribute_ClearAtTime_ShouldRemoveSpecificSample()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        var attr = prim.CreateAttribute("test", "float");
        
        attr.Set(10.0f, UsdTimeCode.Create(1.0));
        attr.Set(20.0f, UsdTimeCode.Create(2.0));
        attr.Set(30.0f, UsdTimeCode.Create(3.0));

        // Act
        var success = attr.ClearAtTime(UsdTimeCode.Create(2.0));

        // Assert
        Assert.True(success);
        Assert.Equal(2, attr.GetNumTimeSamples());
        
        var times = attr.GetTimeSamples();
        Assert.Equal(new[] { 1.0, 3.0 }, times);
    }

    [Fact]
    public void UsdAttribute_ClearDefault_ShouldRemoveDefaultValue()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        var attr = prim.CreateAttribute("test", "float");
        
        attr.Set(100.0f, UsdTimeCode.Default());
        attr.Set(10.0f, UsdTimeCode.Create(1.0));

        // Act
        var success = attr.ClearDefault();

        // Assert
        Assert.True(success);
        
        var defaultSuccess = attr.Get(out VtValue defaultValue, UsdTimeCode.Default());
        var timeSuccess = attr.Get(out VtValue timeValue, UsdTimeCode.Create(1.0));
        
        // Default should fail, time sample should succeed
        Assert.False(defaultSuccess);
        Assert.True(timeSuccess);
    }

    #endregion

    #region Interpolation Tests

    [Fact]
    public void UsdInterpolation_HeldInterpolation_ShouldReturnLowerValue()
    {
        // Arrange
        var lowerValue = new VtValue(10.0f);
        var upperValue = new VtValue(20.0f);

        // Act
        var success = UsdInterpolation.Interpolate(lowerValue, upperValue, 0.5, UsdInterpolationType.Held, out var result);

        // Assert
        Assert.True(success);
        Assert.True(result.IsHolding<float>());
        var floatResult = result.Get<float>();
        Assert.Equal(10.0f, floatResult); // Should return lower value for held interpolation
    }

    [Fact]
    public void UsdInterpolation_LinearInterpolationFloat_ShouldInterpolate()
    {
        // Arrange
        var lowerValue = new VtValue(10.0f);
        var upperValue = new VtValue(20.0f);

        // Act
        var success = UsdInterpolation.Interpolate(lowerValue, upperValue, 0.5, UsdInterpolationType.Linear, out var result);

        // Assert
        Assert.True(success);
        Assert.True(result.IsHolding<float>());
        var floatResult = result.Get<float>();
        Assert.Equal(15.0f, floatResult); // Should be halfway between 10 and 20
    }

    [Fact]
    public void UsdInterpolation_LinearInterpolationDouble_ShouldInterpolate()
    {
        // Arrange
        var lowerValue = new VtValue(5.0);
        var upperValue = new VtValue(15.0);

        // Act
        var success = UsdInterpolation.Interpolate(lowerValue, upperValue, 0.3, UsdInterpolationType.Linear, out var result);

        // Assert
        Assert.True(success);
        Assert.True(result.IsHolding<double>());
        var doubleResult = result.Get<double>();
        Assert.Equal(8.0, doubleResult, 5); // 5 + 0.3 * (15 - 5) = 8.0
    }

    [Fact]
    public void UsdInterpolation_LinearInterpolationExtremes_ShouldClampAlpha()
    {
        // Arrange
        var lowerValue = new VtValue(10.0f);
        var upperValue = new VtValue(20.0f);

        // Act
        var success1 = UsdInterpolation.Interpolate(lowerValue, upperValue, -0.5, UsdInterpolationType.Linear, out var result1);
        var success2 = UsdInterpolation.Interpolate(lowerValue, upperValue, 1.5, UsdInterpolationType.Linear, out var result2);

        // Assert
        Assert.True(success1);
        Assert.True(success2);
        Assert.True(result1.IsHolding<float>());
        Assert.True(result2.IsHolding<float>());
        var float1 = result1.Get<float>();
        var float2 = result2.Get<float>();
        Assert.Equal(10.0f, float1); // Should clamp to lower value
        Assert.Equal(20.0f, float2); // Should clamp to upper value
    }

    [Fact]
    public void UsdInterpolation_TypeSupportsLinearInterpolation_ShouldDetectSupportedTypes()
    {
        // Assert
        Assert.True(UsdInterpolation.TypeSupportsLinearInterpolation("float"));
        Assert.True(UsdInterpolation.TypeSupportsLinearInterpolation("double"));
        Assert.True(UsdInterpolation.TypeSupportsLinearInterpolation("int"));
        Assert.False(UsdInterpolation.TypeSupportsLinearInterpolation("string"));
        Assert.False(UsdInterpolation.TypeSupportsLinearInterpolation("bool"));
    }

    [Fact]
    public void UsdInterpolation_UnsupportedType_ShouldFallbackToHeld()
    {
        // Arrange
        var lowerValue = new VtValue("hello");
        var upperValue = new VtValue("world");

        // Act
        var success = UsdInterpolation.Interpolate(lowerValue, upperValue, 0.5, UsdInterpolationType.Linear, out var result);

        // Assert
        Assert.True(success);
        Assert.True(result.IsHolding<string>());
        var stringResult = result.Get<string>();
        Assert.Equal("hello", stringResult); // Should fallback to held (lower value)
    }

    [Fact]
    public void UsdInterpolation_EmptyValues_ShouldHandleGracefully()
    {
        // Arrange
        var emptyValue = VtValue.CreateEmpty();
        var validValue = new VtValue(10.0f);

        // Act
        var success1 = UsdInterpolation.Interpolate(emptyValue, validValue, 0.5, UsdInterpolationType.Linear, out var result1);
        var success2 = UsdInterpolation.Interpolate(validValue, emptyValue, 0.5, UsdInterpolationType.Linear, out var result2);
        var success3 = UsdInterpolation.Interpolate(emptyValue, emptyValue, 0.5, UsdInterpolationType.Linear, out var result3);

        // Assert
        Assert.True(success1);
        Assert.True(success2);
        Assert.False(success3);
        
        Assert.True(result1.IsHolding<float>());
        Assert.True(result2.IsHolding<float>());
        var float1 = result1.Get<float>();
        var float2 = result2.Get<float>();
        Assert.Equal(10.0f, float1); // Should use the valid value
        Assert.Equal(10.0f, float2); // Should use the valid value
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void UsdAttribute_ComplexTimeSampling_ShouldWorkEndToEnd()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/animated");
        var attr = prim.CreateAttribute("value", "float");

        // Create an animation curve: 0 at t=0, 10 at t=1, 20 at t=2
        attr.Set(0.0f, UsdTimeCode.Create(0.0));
        attr.Set(10.0f, UsdTimeCode.Create(1.0));
        attr.Set(20.0f, UsdTimeCode.Create(2.0));

        // Act & Assert - Test various time queries
        
        // Exact sample times
        Assert.True(attr.Get(out VtValue value0, UsdTimeCode.Create(0.0)));
        Assert.True(value0.IsHolding<float>());
        var float0 = value0.Get<float>();
        Assert.Equal(0.0f, float0);

        // Between samples (should use held interpolation)
        Assert.True(attr.Get(out VtValue value05, UsdTimeCode.Create(0.5)));
        Assert.True(value05.IsHolding<float>());
        var float05 = value05.Get<float>();
        Assert.Equal(0.0f, float05); // Held interpolation returns lower value

        // Before first sample
        Assert.True(attr.Get(out VtValue valueBefore, UsdTimeCode.Create(-1.0)));
        Assert.True(valueBefore.IsHolding<float>());
        var floatBefore = valueBefore.Get<float>();
        Assert.Equal(0.0f, floatBefore); // Should extrapolate with first value

        // After last sample
        Assert.True(attr.Get(out VtValue valueAfter, UsdTimeCode.Create(3.0)));
        Assert.True(valueAfter.IsHolding<float>());
        var floatAfter = valueAfter.Get<float>();
        Assert.Equal(20.0f, floatAfter); // Should extrapolate with last value

        // Verify time sample metadata
        Assert.Equal(3, attr.GetNumTimeSamples());
        Assert.True(attr.ValueMightBeTimeVarying());
        Assert.True(attr.HasAuthoredTimeSamples());
        
        var times = attr.GetTimeSamples();
        Assert.Equal(new[] { 0.0, 1.0, 2.0 }, times);
    }

    #endregion
}