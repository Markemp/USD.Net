using System.Text;
using System.Text.RegularExpressions;

namespace USD.Net.BaselineTests.TestUtils;

public static class FileComparison
{
    private static readonly string FailureDir = Path.Combine(Path.GetTempPath(), "USD.Net.BaselineTests", "failures");
    
    public static void AssertFilesEqual(string actualPath, string expectedPath)
    {
        if (!File.Exists(actualPath))
            throw new FileNotFoundException($"Actual file not found: {actualPath}");
        
        if (!File.Exists(expectedPath))
            throw new FileNotFoundException($"Expected file not found: {expectedPath}");
        
        var actualContent = NormalizeContent(File.ReadAllText(actualPath));
        var expectedContent = NormalizeContent(File.ReadAllText(expectedPath));
        
        if (actualContent == expectedContent)
            return;
        
        SaveFailureArtifacts(actualPath, expectedPath, actualContent, expectedContent);
        throw new Exception($"Files differ. Failure artifacts saved to: {FailureDir}");
    }
    
    public static void AssertFileExists(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Expected file not found: {filePath}");
    }
    
    public static void AssertFileNotExists(string filePath)
    {
        if (File.Exists(filePath))
            throw new Exception($"File should not exist: {filePath}");
    }
    
    public static void AssertFileContains(string filePath, string expectedContent)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");
        
        var content = File.ReadAllText(filePath);
        if (!content.Contains(expectedContent))
            throw new Exception($"File does not contain expected content: {expectedContent}");
    }
    
    public static void AssertFileDoesNotContain(string filePath, string unexpectedContent)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");
        
        var content = File.ReadAllText(filePath);
        if (content.Contains(unexpectedContent))
            throw new Exception($"File contains unexpected content: {unexpectedContent}");
    }
    
    public static void AssertFileMatches(string filePath, string pattern)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");
        
        var content = File.ReadAllText(filePath);
        var regex = new Regex(pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        if (!regex.IsMatch(content))
            throw new Exception($"File does not match pattern: {pattern}");
    }
    
    private static string NormalizeContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;
        
        var lines = content.Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .Select(NormalizeLine)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
        
        return string.Join('\n', lines);
    }
    
    private static string NormalizeLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return string.Empty;
        
        line = line.Trim();
        
        line = RemoveAbsolutePaths(line);
        line = RemoveTimestamps(line);
        line = NormalizeWhitespace(line);
        
        return line;
    }
    
    private static string RemoveAbsolutePaths(string line)
    {
        line = Regex.Replace(line, @"[A-Za-z]:\\[^""'\s]*", "<PATH>");
        
        line = Regex.Replace(line, @"/[^""'\s]*", "<PATH>");
        
        line = Regex.Replace(line, @"file://[^""'\s]*", "<PATH>");
        
        return line;
    }
    
    private static string RemoveTimestamps(string line)
    {
        line = Regex.Replace(line, @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}", "<TIMESTAMP>");
        
        line = Regex.Replace(line, @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", "<TIMESTAMP>");
        
        line = Regex.Replace(line, @"timeCode = \d+\.?\d*", "timeCode = <TIME>");
        
        return line;
    }
    
    private static string NormalizeWhitespace(string line)
    {
        return Regex.Replace(line, @"\s+", " ");
    }
    
    private static void SaveFailureArtifacts(string actualPath, string expectedPath, string actualContent, string expectedContent)
    {
        try
        {
            Directory.CreateDirectory(FailureDir);
            
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var baseName = Path.GetFileNameWithoutExtension(actualPath);
            
            var actualFailurePath = Path.Combine(FailureDir, $"{baseName}_actual_{timestamp}.txt");
            var expectedFailurePath = Path.Combine(FailureDir, $"{baseName}_expected_{timestamp}.txt");
            var diffPath = Path.Combine(FailureDir, $"{baseName}_diff_{timestamp}.txt");
            
            File.WriteAllText(actualFailurePath, actualContent);
            File.WriteAllText(expectedFailurePath, expectedContent);
            
            var diff = GenerateDiff(expectedContent, actualContent);
            File.WriteAllText(diffPath, diff);
            
            Console.WriteLine($"Failure artifacts saved:");
            Console.WriteLine($"  Actual: {actualFailurePath}");
            Console.WriteLine($"  Expected: {expectedFailurePath}");
            Console.WriteLine($"  Diff: {diffPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save failure artifacts: {ex.Message}");
        }
    }
    
    private static string GenerateDiff(string expected, string actual)
    {
        var expectedLines = expected.Split('\n');
        var actualLines = actual.Split('\n');
        
        var diff = new StringBuilder();
        diff.AppendLine("DIFF OUTPUT:");
        diff.AppendLine("============");
        diff.AppendLine();
        
        var maxLines = Math.Max(expectedLines.Length, actualLines.Length);
        for (int i = 0; i < maxLines; i++)
        {
            var expectedLine = i < expectedLines.Length ? expectedLines[i] : "<MISSING>";
            var actualLine = i < actualLines.Length ? actualLines[i] : "<MISSING>";
            
            if (expectedLine != actualLine)
            {
                diff.AppendLine($"Line {i + 1}:");
                diff.AppendLine($"  Expected: {expectedLine}");
                diff.AppendLine($"  Actual:   {actualLine}");
                diff.AppendLine();
            }
        }
        
        return diff.ToString();
    }
    
    public static string GetFailureDirectory()
    {
        return FailureDir;
    }
    
    public static void ClearFailureDirectory()
    {
        if (Directory.Exists(FailureDir))
        {
            try
            {
                Directory.Delete(FailureDir, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to clear failure directory: {ex.Message}");
            }
        }
    }
}