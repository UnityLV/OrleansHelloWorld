using Newtonsoft.Json;
using Orleans.Storage;

public class FileSystemGrainStorage : IGrainStorage
{
    private readonly string _basePath;

    public FileSystemGrainStorage(string basePath)
    {
        _basePath = basePath;
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        string filePath = GetFilePath(grainId, stateName);
        Console.WriteLine($"Read From {filePath}");

        if (File.Exists(filePath))
        {
            using (var reader = new StreamReader(filePath))
            using (var jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer serializer = new JsonSerializer();
                grainState.State = serializer.Deserialize<T>(jsonReader);
            }
        }
        else
        {
            grainState.State = Activator.CreateInstance<T>();
        }
    }

    public async Task WriteStateAsync<TGrainState>(string stateName, GrainId grainId, IGrainState<TGrainState> grainState)
    {
        string filePath = GetFilePath(grainId, stateName);
        Console.WriteLine($"Write State to {filePath}");
        JsonSerializer serializer = new JsonSerializer();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
        using (var writer = new StreamWriter(stream))
        using (var jsonWriter = new JsonTextWriter(writer))
        {
            serializer.Serialize(jsonWriter, grainState.State, grainState.State.GetType());
            await writer.FlushAsync();
        }
    }

    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        string filePath = GetFilePath(grainId, stateName);
        Console.WriteLine($"Clear State in {filePath}");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        grainState.State = Activator.CreateInstance<T>();
    }

    private string GetFilePath(GrainId grainId, string storageName)
    {
        return Path.Combine(_basePath, $"{grainId}_{storageName}.json");
    }
}