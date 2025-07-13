using Xunit;
using Pxr.Usd.Sdf;
using Pxr.Base.Tf;
using Pxr.Base.Vt;

namespace USD.Net.Tests;

[Trait("Category", "Unit")]
public class SdfDataTests
{
    [Fact]
    public void Constructor_CreatesEmptyData()
    {
        var data = new SdfData();
        Assert.NotNull(data);
        Assert.False(data.StreamsData());
        Assert.True(data.IsDetached());
    }

    [Fact]
    public void CreateSpec_CreatesSpecWithCorrectType()
    {
        var data = new SdfData();
        var path = new SdfPath("/test");
        
        data.CreateSpec(path, SdfSpecType.SdfSpecTypePrim);
        
        Assert.True(data.HasSpec(path));
        Assert.Equal(SdfSpecType.SdfSpecTypePrim, data.GetSpecType(path));
    }

    [Fact]
    public void CreateSpec_UnknownTypeIgnored()
    {
        var data = new SdfData();
        var path = new SdfPath("/test");
        
        data.CreateSpec(path, SdfSpecType.SdfSpecTypeUnknown);
        
        Assert.False(data.HasSpec(path));
    }

    [Fact]
    public void EraseSpec_RemovesSpec()
    {
        var data = new SdfData();
        var path = new SdfPath("/test");
        
        data.CreateSpec(path, SdfSpecType.SdfSpecTypePrim);
        Assert.True(data.HasSpec(path));
        
        data.EraseSpec(path);
        Assert.False(data.HasSpec(path));
        Assert.Equal(SdfSpecType.SdfSpecTypeUnknown, data.GetSpecType(path));
    }

    [Fact]
    public void MoveSpec_MovesSpecToNewPath()
    {
        var data = new SdfData();
        var oldPath = new SdfPath("/old");
        var newPath = new SdfPath("/new");
        
        data.CreateSpec(oldPath, SdfSpecType.SdfSpecTypePrim);
        data.Set(oldPath, new TfToken("testField"), new VtValue("testValue"));
        
        data.MoveSpec(oldPath, newPath);
        
        Assert.False(data.HasSpec(oldPath));
        Assert.True(data.HasSpec(newPath));
        Assert.Equal(SdfSpecType.SdfSpecTypePrim, data.GetSpecType(newPath));
        Assert.Equal("testValue", data.Get(newPath, new TfToken("testField")).Get<string>());
    }

    [Fact]
    public void SetGet_StoresAndRetrievesFieldValues()
    {
        var data = new SdfData();
        var path = new SdfPath("/test");
        var field = new TfToken("testField");
        var value = new VtValue("testValue");
        
        data.CreateSpec(path, SdfSpecType.SdfSpecTypePrim);
        data.Set(path, field, value);
        
        var retrievedValue = data.Get(path, field);
        Assert.Equal("testValue", retrievedValue.Get<string>());
    }

    [Fact]
    public void Set_EmptyValueErasesField()
    {
        var data = new SdfData();
        var path = new SdfPath("/test");
        var field = new TfToken("testField");
        
        data.CreateSpec(path, SdfSpecType.SdfSpecTypePrim);
        data.Set(path, field, new VtValue("testValue"));
        Assert.True(data.Has(path, field));
        
        data.Set(path, field, new VtValue());
        Assert.False(data.Has(path, field));
    }

    [Fact]
    public void Erase_RemovesField()
    {
        var data = new SdfData();
        var path = new SdfPath("/test");
        var field = new TfToken("testField");
        
        data.CreateSpec(path, SdfSpecType.SdfSpecTypePrim);
        data.Set(path, field, new VtValue("testValue"));
        Assert.True(data.Has(path, field));
        
        data.Erase(path, field);
        Assert.False(data.Has(path, field));
    }

    [Fact]
    public void List_ReturnsAllFieldNames()
    {
        var data = new SdfData();
        var path = new SdfPath("/test");
        var field1 = new TfToken("field1");
        var field2 = new TfToken("field2");
        
        data.CreateSpec(path, SdfSpecType.SdfSpecTypePrim);
        data.Set(path, field1, new VtValue("value1"));
        data.Set(path, field2, new VtValue("value2"));
        
        var fields = data.List(path);
        Assert.Equal(2, fields.Count);
        Assert.Contains(field1, fields);
        Assert.Contains(field2, fields);
    }

    [Fact]
    public void HasSpecAndField_ReturnsCorrectSpecTypeAndValue()
    {
        var data = new SdfData();
        var path = new SdfPath("/test");
        var field = new TfToken("testField");
        var value = new VtValue("testValue");
        
        data.CreateSpec(path, SdfSpecType.SdfSpecTypeAttribute);
        data.Set(path, field, value);
        
        var hasField = data.HasSpecAndField(path, field, out var retrievedValue, out var specType);
        
        Assert.True(hasField);
        Assert.Equal(SdfSpecType.SdfSpecTypeAttribute, specType);
        Assert.Equal("testValue", retrievedValue.Get<string>());
    }

    [Fact]
    public void SetTimeSample_CreatesTimeSampleMap()
    {
        var data = new SdfData();
        var path = new SdfPath("/test.attr");
        var time = 1.0;
        var value = new VtValue(42.0);
        
        data.CreateSpec(path, SdfSpecType.SdfSpecTypeAttribute);
        data.SetTimeSample(path, time, value);
        
        Assert.Equal(1, data.GetNumTimeSamplesForPath(path));
        Assert.True(data.QueryTimeSample(path, time, out var retrievedValue));
        Assert.Equal(42.0, retrievedValue.Get<double>());
    }

    [Fact]
    public void SetTimeSample_EmptyValueErasesTimeSample()
    {
        var data = new SdfData();
        var path = new SdfPath("/test.attr");
        var time = 1.0;
        
        data.CreateSpec(path, SdfSpecType.SdfSpecTypeAttribute);
        data.SetTimeSample(path, time, new VtValue(42.0));
        Assert.Equal(1, data.GetNumTimeSamplesForPath(path));
        
        data.SetTimeSample(path, time, new VtValue());
        Assert.Equal(0, data.GetNumTimeSamplesForPath(path));
    }

    [Fact]
    public void EraseTimeSample_RemovesSpecificTimeSample()
    {
        var data = new SdfData();
        var path = new SdfPath("/test.attr");
        
        data.CreateSpec(path, SdfSpecType.SdfSpecTypeAttribute);
        data.SetTimeSample(path, 1.0, new VtValue(10.0));
        data.SetTimeSample(path, 2.0, new VtValue(20.0));
        Assert.Equal(2, data.GetNumTimeSamplesForPath(path));
        
        data.EraseTimeSample(path, 1.0);
        Assert.Equal(1, data.GetNumTimeSamplesForPath(path));
        Assert.False(data.QueryTimeSample(path, 1.0, out _));
        Assert.True(data.QueryTimeSample(path, 2.0, out _));
    }

    [Fact]
    public void ListTimeSamplesForPath_ReturnsAllTimeSamples()
    {
        var data = new SdfData();
        var path = new SdfPath("/test.attr");
        
        data.CreateSpec(path, SdfSpecType.SdfSpecTypeAttribute);
        data.SetTimeSample(path, 1.0, new VtValue(10.0));
        data.SetTimeSample(path, 2.0, new VtValue(20.0));
        data.SetTimeSample(path, 3.0, new VtValue(30.0));
        
        var times = data.ListTimeSamplesForPath(path);
        Assert.Equal(3, times.Count);
        Assert.Contains(1.0, times);
        Assert.Contains(2.0, times);
        Assert.Contains(3.0, times);
    }

    [Fact]
    public void ListAllTimeSamples_ReturnsAllTimeSamplesFromAllPaths()
    {
        var data = new SdfData();
        var path1 = new SdfPath("/test1.attr");
        var path2 = new SdfPath("/test2.attr");
        
        data.CreateSpec(path1, SdfSpecType.SdfSpecTypeAttribute);
        data.CreateSpec(path2, SdfSpecType.SdfSpecTypeAttribute);
        
        data.SetTimeSample(path1, 1.0, new VtValue(10.0));
        data.SetTimeSample(path1, 2.0, new VtValue(20.0));
        data.SetTimeSample(path2, 2.0, new VtValue(200.0));
        data.SetTimeSample(path2, 3.0, new VtValue(300.0));
        
        var allTimes = data.ListAllTimeSamples();
        Assert.Equal(3, allTimes.Count);
        Assert.Contains(1.0, allTimes);
        Assert.Contains(2.0, allTimes);
        Assert.Contains(3.0, allTimes);
    }

    [Fact]
    public void GetBracketingTimeSamples_ReturnsCorrectBrackets()
    {
        var data = new SdfData();
        var path = new SdfPath("/test.attr");
        
        data.CreateSpec(path, SdfSpecType.SdfSpecTypeAttribute);
        data.SetTimeSample(path, 1.0, new VtValue(10.0));
        data.SetTimeSample(path, 3.0, new VtValue(30.0));
        data.SetTimeSample(path, 5.0, new VtValue(50.0));
        
        Assert.True(data.GetBracketingTimeSamplesForPath(path, 2.0, out var tLower, out var tUpper));
        Assert.Equal(1.0, tLower);
        Assert.Equal(3.0, tUpper);
        
        Assert.True(data.GetBracketingTimeSamplesForPath(path, 3.0, out tLower, out tUpper));
        Assert.Equal(3.0, tLower);
        Assert.Equal(3.0, tUpper);
        
        Assert.True(data.GetBracketingTimeSamplesForPath(path, 0.5, out tLower, out tUpper));
        Assert.Equal(1.0, tLower);
        Assert.Equal(1.0, tUpper);
        
        Assert.True(data.GetBracketingTimeSamplesForPath(path, 6.0, out tLower, out tUpper));
        Assert.Equal(5.0, tLower);
        Assert.Equal(5.0, tUpper);
    }

    [Fact]
    public void GetPreviousTimeSampleForPath_ReturnsCorrectPreviousTime()
    {
        var data = new SdfData();
        var path = new SdfPath("/test.attr");
        
        data.CreateSpec(path, SdfSpecType.SdfSpecTypeAttribute);
        data.SetTimeSample(path, 1.0, new VtValue(10.0));
        data.SetTimeSample(path, 3.0, new VtValue(30.0));
        data.SetTimeSample(path, 5.0, new VtValue(50.0));
        
        Assert.True(data.GetPreviousTimeSampleForPath(path, 4.0, out var tPrevious));
        Assert.Equal(3.0, tPrevious);
        
        Assert.True(data.GetPreviousTimeSampleForPath(path, 6.0, out tPrevious));
        Assert.Equal(5.0, tPrevious);
        
        Assert.False(data.GetPreviousTimeSampleForPath(path, 0.5, out _));
    }

    [Fact]
    public void CopyFrom_CopiesAllSpecsAndFields()
    {
        var source = new SdfData();
        var target = new SdfData();
        
        var path1 = new SdfPath("/prim1");
        var path2 = new SdfPath("/prim2");
        var field = new TfToken("testField");
        
        source.CreateSpec(path1, SdfSpecType.SdfSpecTypePrim);
        source.CreateSpec(path2, SdfSpecType.SdfSpecTypeAttribute);
        source.Set(path1, field, new VtValue("value1"));
        source.Set(path2, field, new VtValue("value2"));
        
        target.CopyFrom(source);
        
        Assert.True(target.HasSpec(path1));
        Assert.True(target.HasSpec(path2));
        Assert.Equal(SdfSpecType.SdfSpecTypePrim, target.GetSpecType(path1));
        Assert.Equal(SdfSpecType.SdfSpecTypeAttribute, target.GetSpecType(path2));
        Assert.Equal("value1", target.Get(path1, field).Get<string>());
        Assert.Equal("value2", target.Get(path2, field).Get<string>());
    }
}