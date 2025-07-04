using System.Reflection;

namespace USD.Net.BaselineTests.TestUtils;

public static class BaselineManager
{
    private static readonly string TestAssemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    private static readonly string TestDataDir = Path.Combine(TestAssemblyDir, "TestData");
    private static readonly string BaselineDir = Path.Combine(TestAssemblyDir, "Baselines");
    
    public static string GetTestDataPath(string category, string filename)
    {
        var path = Path.Combine(TestDataDir, category, filename);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Test data file not found: {path}");
        return path;
    }
    
    public static string GetBaselinePath(string category, string filename)
    {
        var path = Path.Combine(BaselineDir, category, filename);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Baseline file not found: {path}");
        return path;
    }
    
    public static string GetTestDataDirectory(string category)
    {
        var dir = Path.Combine(TestDataDir, category);
        if (!Directory.Exists(dir))
            throw new DirectoryNotFoundException($"Test data directory not found: {dir}");
        return dir;
    }
    
    public static string GetBaselineDirectory(string category)
    {
        var dir = Path.Combine(BaselineDir, category);
        if (!Directory.Exists(dir))
            throw new DirectoryNotFoundException($"Baseline directory not found: {dir}");
        return dir;
    }
    
    public static IEnumerable<string> GetTestDataFiles(string category, string pattern = "*")
    {
        var dir = GetTestDataDirectory(category);
        return Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Cast<string>()
            .OrderBy(f => f);
    }
    
    public static IEnumerable<string> GetAllTestDataFiles(string pattern = "*.usda")
    {
        var categories = GetTestDataCategories();
        return categories.SelectMany(category => 
            GetTestDataFiles(category, pattern).Select(file => $"{category}/{file}"));
    }
    
    public static IEnumerable<string> GetTestDataCategories()
    {
        if (!Directory.Exists(TestDataDir))
            return Enumerable.Empty<string>();
        
        return Directory.GetDirectories(TestDataDir)
            .Select(Path.GetFileName)
            .Where(name => name != null)
            .Cast<string>()
            .OrderBy(name => name);
    }
    
    public static void EnsureTestDataExists(string category, string filename)
    {
        var path = Path.Combine(TestDataDir, category, filename);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Test data file not found: {path}");
    }
    
    public static void EnsureBaselineExists(string category, string filename)
    {
        var path = Path.Combine(BaselineDir, category, filename);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Baseline file not found: {path}. Use GenerateBaseline() to create it.");
    }
    
    public static void GenerateBaseline(string category, string filename, string content)
    {
        var baselineDir = Path.Combine(BaselineDir, category);
        Directory.CreateDirectory(baselineDir);
        
        var path = Path.Combine(baselineDir, filename);
        File.WriteAllText(path, content);
        
        Console.WriteLine($"Generated baseline: {path}");
    }
    
    public static void GenerateBaselineFromFile(string category, string filename, string sourcePath)
    {
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException($"Source file not found: {sourcePath}");
        
        var content = File.ReadAllText(sourcePath);
        GenerateBaseline(category, filename, content);
    }
    
    public static void RegenerateAllBaselines()
    {
        Console.WriteLine("Regenerating all baselines...");
        
        var categories = GetTestDataCategories();
        foreach (var category in categories)
        {
            var testFiles = GetTestDataFiles(category);
            foreach (var filename in testFiles)
            {
                try
                {
                    RegenerateBaseline(category, filename);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to regenerate baseline for {category}/{filename}: {ex.Message}");
                }
            }
        }
        
        Console.WriteLine("Baseline regeneration complete.");
    }
    
    public static void RegenerateBaseline(string category, string filename)
    {
        var testDataPath = GetTestDataPath(category, filename);
        
        using (var env = new TestEnvironment())
        {
            var outputPath = env.GetTempFilePath(filename);
            
            if (filename.EndsWith(".usda"))
            {
                RegenerateUsdaBaseline(testDataPath, outputPath);
            }
            else
            {
                File.Copy(testDataPath, outputPath, overwrite: true);
            }
            
            GenerateBaselineFromFile(category, filename, outputPath);
        }
    }
    
    private static void RegenerateUsdaBaseline(string inputPath, string outputPath)
    {
        try
        {
            var layer = Pxr.Usd.Sdf.SdfLayer.FindOrOpen(inputPath);
            if (layer == null)
            {
                throw new InvalidOperationException($"Failed to load USD layer: {inputPath}");
            }
            
            layer.Export(outputPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to process USD file {inputPath}: {ex.Message}", ex);
        }
    }
    
    public static void ValidateBaselines()
    {
        Console.WriteLine("Validating baselines...");
        
        var issues = new List<string>();
        var categories = GetTestDataCategories();
        
        foreach (var category in categories)
        {
            var testFiles = GetTestDataFiles(category);
            foreach (var filename in testFiles)
            {
                try
                {
                    EnsureBaselineExists(category, filename);
                }
                catch (FileNotFoundException)
                {
                    issues.Add($"Missing baseline: {category}/{filename}");
                }
            }
        }
        
        if (issues.Any())
        {
            Console.WriteLine($"Found {issues.Count} baseline issues:");
            foreach (var issue in issues)
            {
                Console.WriteLine($"  - {issue}");
            }
        }
        else
        {
            Console.WriteLine("All baselines are valid.");
        }
    }
    
    public static void CreateTestDataFile(string category, string filename, string content)
    {
        var testDataDir = Path.Combine(TestDataDir, category);
        Directory.CreateDirectory(testDataDir);
        
        var path = Path.Combine(testDataDir, filename);
        File.WriteAllText(path, content);
        
        Console.WriteLine($"Created test data file: {path}");
    }
    
    public static void ImportTestDataFromDirectory(string sourceDir, string category)
    {
        if (!Directory.Exists(sourceDir))
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
        
        var targetDir = Path.Combine(TestDataDir, category);
        Directory.CreateDirectory(targetDir);
        
        var files = Directory.GetFiles(sourceDir, "*.usda", SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            var filename = Path.GetFileName(file);
            var targetPath = Path.Combine(targetDir, filename);
            File.Copy(file, targetPath, overwrite: true);
            Console.WriteLine($"Imported: {filename}");
        }
        
        Console.WriteLine($"Imported {files.Length} files to {category}");
    }
    
    public static TestDataInfo GetTestDataInfo(string category, string filename)
    {
        var testDataPath = GetTestDataPath(category, filename);
        var baselinePath = Path.Combine(BaselineDir, category, filename);
        
        return new TestDataInfo
        {
            Category = category,
            Filename = filename,
            TestDataPath = testDataPath,
            BaselinePath = baselinePath,
            HasBaseline = File.Exists(baselinePath),
            TestDataSize = new FileInfo(testDataPath).Length,
            BaselineSize = File.Exists(baselinePath) ? new FileInfo(baselinePath).Length : 0
        };
    }
}

public class TestDataInfo
{
    public string Category { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public string TestDataPath { get; set; } = string.Empty;
    public string BaselinePath { get; set; } = string.Empty;
    public bool HasBaseline { get; set; }
    public long TestDataSize { get; set; }
    public long BaselineSize { get; set; }
    
    public override string ToString()
    {
        return $"{Category}/{Filename} (TestData: {TestDataSize} bytes, Baseline: {(HasBaseline ? $"{BaselineSize} bytes" : "MISSING")})";
    }
}