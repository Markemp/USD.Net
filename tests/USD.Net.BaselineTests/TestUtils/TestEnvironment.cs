using System.Collections.Concurrent;

namespace USD.Net.BaselineTests.TestUtils;

public class TestEnvironment : IDisposable
{
    private readonly string _tempDir;
    private readonly Dictionary<string, string?> _originalEnvVars;
    private readonly List<string> _tempFiles;
    private readonly object _lock = new();
    private bool _disposed = false;
    
    private static readonly ConcurrentDictionary<string, int> _tempDirCounters = new();
    
    public TestEnvironment()
    {
        _tempDir = CreateUniqueTempDirectory();
        _originalEnvVars = new Dictionary<string, string?>();
        _tempFiles = new List<string>();
        
        Directory.CreateDirectory(_tempDir);
        SetupTestEnvironment();
    }
    
    public string TempDirectory => _tempDir;
    
    public string GetTempFilePath(string filename)
    {
        var path = Path.Combine(_tempDir, filename);
        lock (_lock)
        {
            _tempFiles.Add(path);
        }
        return path;
    }
    
    public string GetTempDirectoryPath(string dirname)
    {
        var path = Path.Combine(_tempDir, dirname);
        Directory.CreateDirectory(path);
        return path;
    }
    
    public void SetEnvironmentVariable(string name, string? value)
    {
        if (!_originalEnvVars.ContainsKey(name))
        {
            _originalEnvVars[name] = Environment.GetEnvironmentVariable(name);
        }
        
        Environment.SetEnvironmentVariable(name, value);
    }
    
    public void CopyFileToTemp(string sourcePath, string? targetName = null)
    {
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException($"Source file not found: {sourcePath}");
        
        var filename = targetName ?? Path.GetFileName(sourcePath);
        var targetPath = GetTempFilePath(filename);
        
        File.Copy(sourcePath, targetPath, overwrite: true);
    }
    
    public void CopyDirectoryToTemp(string sourceDir, string? targetName = null)
    {
        if (!Directory.Exists(sourceDir))
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
        
        var dirname = targetName ?? Path.GetFileName(sourceDir);
        var targetPath = GetTempDirectoryPath(dirname);
        
        CopyDirectoryRecursive(sourceDir, targetPath);
    }
    
    public void WriteTextFile(string filename, string content)
    {
        var path = GetTempFilePath(filename);
        File.WriteAllText(path, content);
    }
    
    public void WriteBinaryFile(string filename, byte[] content)
    {
        var path = GetTempFilePath(filename);
        File.WriteAllBytes(path, content);
    }
    
    public string ReadTextFile(string filename)
    {
        var path = Path.Combine(_tempDir, filename);
        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found in temp directory: {filename}");
        
        return File.ReadAllText(path);
    }
    
    public byte[] ReadBinaryFile(string filename)
    {
        var path = Path.Combine(_tempDir, filename);
        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found in temp directory: {filename}");
        
        return File.ReadAllBytes(path);
    }
    
    public bool FileExists(string filename)
    {
        var path = Path.Combine(_tempDir, filename);
        return File.Exists(path);
    }
    
    public bool DirectoryExists(string dirname)
    {
        var path = Path.Combine(_tempDir, dirname);
        return Directory.Exists(path);
    }
    
    public void DeleteFile(string filename)
    {
        var path = Path.Combine(_tempDir, filename);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
    
    public void DeleteDirectory(string dirname)
    {
        var path = Path.Combine(_tempDir, dirname);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
    
    public IEnumerable<string> GetFiles(string pattern = "*")
    {
        return Directory.GetFiles(_tempDir, pattern, SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Cast<string>();
    }
    
    public IEnumerable<string> GetDirectories(string pattern = "*")
    {
        return Directory.GetDirectories(_tempDir, pattern, SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(d => d != null)
            .Cast<string>();
    }
    
    public void CleanupTempFiles()
    {
        lock (_lock)
        {
            foreach (var file in _tempFiles.ToList())
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to delete temp file {file}: {ex.Message}");
                }
            }
            _tempFiles.Clear();
        }
    }
    
    private static string CreateUniqueTempDirectory()
    {
        var baseName = "USD.Net.BaselineTests";
        var tempRoot = Path.GetTempPath();
        var counter = _tempDirCounters.AddOrUpdate(baseName, 1, (key, value) => value + 1);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var guid = Guid.NewGuid().ToString("N")[..8];
        
        return Path.Combine(tempRoot, $"{baseName}_{timestamp}_{counter:D3}_{guid}");
    }
    
    private void SetupTestEnvironment()
    {
        SetEnvironmentVariable("USD_NET_TEST_MODE", "1");
        SetEnvironmentVariable("USD_NET_TEST_TEMP_DIR", _tempDir);
        
        var currentDir = Environment.CurrentDirectory;
        SetEnvironmentVariable("USD_NET_TEST_ORIGINAL_DIR", currentDir);
    }
    
    private void CopyDirectoryRecursive(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);
        
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var targetFile = Path.Combine(targetDir, Path.GetFileName(file));
            File.Copy(file, targetFile, overwrite: true);
        }
        
        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            var targetSubDir = Path.Combine(targetDir, Path.GetFileName(subDir));
            CopyDirectoryRecursive(subDir, targetSubDir);
        }
    }
    
    public void Dispose()
    {
        if (_disposed)
            return;
        
        try
        {
            CleanupTempFiles();
            
            RestoreEnvironmentVariables();
            
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to cleanup test environment: {ex.Message}");
        }
        finally
        {
            _disposed = true;
        }
    }
    
    private void RestoreEnvironmentVariables()
    {
        foreach (var kvp in _originalEnvVars)
        {
            try
            {
                Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to restore environment variable {kvp.Key}: {ex.Message}");
            }
        }
    }
    
    public static void CleanupAllTempDirectories()
    {
        try
        {
            var tempRoot = Path.GetTempPath();
            var testDirs = Directory.GetDirectories(tempRoot, "USD.Net.BaselineTests_*", SearchOption.TopDirectoryOnly);
            
            foreach (var dir in testDirs)
            {
                try
                {
                    var dirInfo = new DirectoryInfo(dir);
                    if (dirInfo.CreationTime < DateTime.Now.AddHours(-1))
                    {
                        Directory.Delete(dir, recursive: true);
                        Console.WriteLine($"Cleaned up old temp directory: {dir}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to cleanup temp directory {dir}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to cleanup temp directories: {ex.Message}");
        }
    }
}