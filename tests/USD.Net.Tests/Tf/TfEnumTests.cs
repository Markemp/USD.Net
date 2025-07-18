namespace USD.Net.Tests.Tf;

using Pxr.Usd.Tf;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

public enum Condiment
{
    Salt = 0,
    Pepper = 13,
    Ketchup = 14,
    NoName = 15
}

public enum Season
{
    Spring = 0,
    Summer = 3, // It's ok to have initializers
    Autumn = 4,
    Winter = 5
}

public class TfEnumTests
{
    private readonly ITestOutputHelper _output;

    public TfEnumTests(ITestOutputHelper output)
    {
        _output = output;

        // Register enum names (equivalent to TF_ADD_ENUM_NAME)
        RegisterEnumNames();
    }

    private void RegisterEnumNames()
    {
        // Add enum names and display names
        TfEnum.AddName(Condiment.Salt, "Salt");
        TfEnum.AddName(Condiment.Pepper, "Pepper");
        TfEnum.AddName(Condiment.Ketchup, "Ketchup");
        // Note: NoName is intentionally not registered

        // Omit display names (they'll default to the enum name)
        TfEnum.AddName(Season.Spring);
        TfEnum.AddName(Season.Summer);
        TfEnum.AddName(Season.Autumn);
        TfEnum.AddName(Season.Winter);
    }

    [Fact]
    public void TestBasicNameOperations()
    {
        var c = Condiment.Pepper;

        var name = TfEnum.GetName(c);
        var fullName = TfEnum.GetFullName(c);
        var displayName = TfEnum.GetDisplayName(c);

        _output.WriteLine($"GetName(PEPPER) returns {name}");
        _output.WriteLine($"GetFullName(PEPPER) returns {fullName}");
        _output.WriteLine($"GetDisplayName(PEPPER) returns {displayName}");

        Assert.Equal("Pepper", name);
        Assert.Equal("Condiment::Pepper", fullName);
        Assert.Equal("Pepper", displayName);
    }

    [Fact]
    public void TestGetValueFromName()
    {
        var c = TfEnum.GetValueFromName<Condiment>("Ketchup", out bool foundIt);

        _output.WriteLine($"GetValueFromName(\"Ketchup\") returns {foundIt}: {(int)c}");

        Assert.True(foundIt);
        Assert.Equal(Condiment.Ketchup, c);
        Assert.Equal(14, (int)c);
    }

    [Fact]
    public void TestGetValueFromFullName()
    {
        var tfEnum = TfEnum.GetValueFromFullName("Condiment::Ketchup", out bool foundIt);

        _output.WriteLine($"GetValueFromFullName(\"Condiment::Ketchup\") returns {foundIt}: {tfEnum.GetIntValue()}");

        Assert.True(foundIt);
        Assert.Equal(14, tfEnum.GetIntValue());
        Assert.True(tfEnum.IsA<Condiment>());
        Assert.Equal(Condiment.Ketchup, tfEnum.GetValue<Condiment>());
    }

    [Fact]
    public void TestErrorCases()
    {
        var c = Condiment.NoName;

        var name = TfEnum.GetName(c);
        var fullName = TfEnum.GetFullName(c);

        _output.WriteLine($"GetName(NoName) returns '{name}'");
        _output.WriteLine($"GetFullName(NoName) returns '{fullName}'");

        // Unregistered enum should return empty strings
        Assert.Equal(string.Empty, name);
        Assert.Equal(string.Empty, fullName);

        // Test invalid name lookup
        var invalidEnum = TfEnum.GetValueFromName<Condiment>("SQUID", out bool foundIt);
        _output.WriteLine($"GetValueFromName(\"SQUID\") returns {foundIt}: {(int)invalidEnum}");

        Assert.False(foundIt);
        Assert.Equal(-1, (int)invalidEnum);

        // Test invalid full name lookup
        var invalidTfEnum = TfEnum.GetValueFromFullName("Condiment::SQUID", out foundIt);
        _output.WriteLine($"GetValueFromFullName(\"Condiment::SQUID\") returns {foundIt}: {invalidTfEnum.GetIntValue()}");

        Assert.False(foundIt);
        Assert.Equal(-1, invalidTfEnum.GetIntValue());
    }

    [Fact]
    public void TestSeasonOperations()
    {
        // Look up the name for a value:
        string name1 = TfEnum.GetName(Season.Summer);      // Returns "Summer"
        string name2 = TfEnum.GetFullName(Season.Summer); // Returns "Season::Summer"
        string name3 = TfEnum.GetDisplayName(Season.Summer); // Returns "Summer" (no custom display name)

        _output.WriteLine($"name1 = \"{name1}\"");
        _output.WriteLine($"name2 = \"{name2}\"");
        _output.WriteLine($"name3 = \"{name3}\"");

        Assert.Equal("Summer", name1);
        Assert.Equal("Season::Summer", name2);
        Assert.Equal("Summer", name3);

        // Look up the value for a name:
        Season s1 = TfEnum.GetValueFromName<Season>("Autumn", out bool found);
        _output.WriteLine($"s1 = {(int)s1}, found = {found}");

        Assert.True(found);
        Assert.Equal(Season.Autumn, s1);
        Assert.Equal(4, (int)s1);

        // Test invalid name
        Season s2 = TfEnum.GetValueFromName<Season>("MONDAY", out found);
        _output.WriteLine($"s2 = {(int)s2}, found = {found}");

        Assert.False(found);
        Assert.Equal(-1, (int)s2);

        // Test type-based lookup
        var type = new TfEnum(s2);
        var s3 = TfEnum.GetValueFromName(type.EnumType, "Autumn", out found);
        _output.WriteLine($"s3 = {s3.GetIntValue()}, full name = {TfEnum.GetFullName(s3)}, found = {found}");

        Assert.True(found);
        Assert.Equal(4, s3.GetIntValue());
        Assert.Equal("Season::Autumn", TfEnum.GetFullName(s3));

        // Test full name lookup
        var s4 = TfEnum.GetValueFromFullName("Season::Winter", out found);
        _output.WriteLine($"s4 = {s4.GetIntValue()}, found = {found}");

        Assert.True(found);
        Assert.Equal(5, s4.GetIntValue());

        // Test cross-type lookup (should fail)
        Season s5 = TfEnum.GetValueFromName<Season>("Salt", out found);
        _output.WriteLine($"s5 = {(int)s5}, found = {found}");

        Assert.False(found);
        Assert.Equal(-1, (int)s5);
    }

    [Fact]
    public void TestKnownTypeNames()
    {
        var lookupNames = new[] { "Season", "Summer", "Condiment", "Sandwich" };

        foreach (var name in lookupNames)
        {
            bool isKnown = TfEnum.IsKnownEnumType(name);
            _output.WriteLine($"type name \"{name}\" is {(isKnown ? "known" : "unknown")}");
        }

        Assert.True(TfEnum.IsKnownEnumType("Season"));
        Assert.False(TfEnum.IsKnownEnumType("Summer")); // Summer is a value, not a type
        Assert.True(TfEnum.IsKnownEnumType("Condiment"));
        Assert.False(TfEnum.IsKnownEnumType("Sandwich"));
    }

    [Fact]
    public void TestGetAllNames()
    {
        var names = TfEnum.GetAllNames<Condiment>().OrderBy(n => n).ToList();

        _output.WriteLine("names associated with Condiment:");
        foreach (var name in names)
        {
            _output.WriteLine(name);
        }

        Assert.Equal(3, names.Count); // Only registered names
        Assert.Contains("Ketchup", names);
        Assert.Contains("Pepper", names);
        Assert.Contains("Salt", names);
        Assert.DoesNotContain("NoName", names); // Not registered

        // Test GetAllNames with instance
        var summerNames = TfEnum.GetAllNames(new TfEnum(Season.Summer)).OrderBy(n => n).ToList();

        _output.WriteLine("names associated with SUMMER:");
        foreach (var name in summerNames)
        {
            _output.WriteLine(name);
        }

        Assert.Equal(4, summerNames.Count);
        Assert.Contains("Spring", summerNames);
        Assert.Contains("Summer", summerNames);
        Assert.Contains("Autumn", summerNames);
        Assert.Contains("Winter", summerNames);

        // Test GetAllNames with type lookup
        var seasonType = TfEnum.GetTypeFromName("Season");
        Assert.NotNull(seasonType);

        var seasonNames = TfEnum.GetAllNames(seasonType).OrderBy(n => n).ToList();

        _output.WriteLine("names associated with \"Season\":");
        foreach (var name in seasonNames)
        {
            _output.WriteLine(name);
        }

        Assert.Equal(summerNames, seasonNames);
    }

    [Fact]
    public void TestTfEnumBasicOperations()
    {
        var e = new TfEnum(Season.Summer);

        Assert.True(e.IsA<Season>());
        Assert.Equal(3, e.GetIntValue());
        Assert.Equal(Season.Summer, e.GetValue<Season>());
    }

    [Fact]
    public void TestTfEnumOperators()
    {
        var summer1 = new TfEnum(Season.Summer);
        var summer2 = new TfEnum(Season.Summer);
        var spring = new TfEnum(Season.Spring);

        // Test equality
        Assert.True(summer1 == summer2);
        Assert.True(summer1 <= summer2);
        Assert.True(summer1 >= summer2);

        // Test inequality
        Assert.True(summer1 != spring);
        Assert.True(summer1 > spring);
        Assert.True(summer1 >= spring);
        Assert.True(spring <= summer1);
        Assert.True(spring < summer1);
    }

    [Fact]
    public void TestImplicitConversion()
    {
        // Test implicit conversion from enum to TfEnum
        TfEnum tfEnum = Season.Winter;

        Assert.True(tfEnum.IsA<Season>());
        Assert.Equal(Season.Winter, tfEnum.GetValue<Season>());
        Assert.Equal(5, tfEnum.GetIntValue());
    }

    [Fact]
    public void TestToString()
    {
        var registeredEnum = new TfEnum(Season.Summer);
        var unregisteredEnum = new TfEnum(Condiment.NoName);

        Assert.Equal("Summer", registeredEnum.ToString());
        Assert.Equal("NoName", unregisteredEnum.ToString()); // Falls back to enum.ToString()
    }
}