using System.Collections.Concurrent;
using work5_ASP.NET_Core_API.Models;

namespace work5_ASP.NET_Core_API.Services;

public class InMemoryTaskStorage : ITaskStorage
{
    private readonly ConcurrentDictionary<int, TaskItem> _tasks = new();
    private int _nextId = 1;

    public TaskItem? GetTaskById(int id) => _tasks.TryGetValue(id, out var task) ? task : null;

    public IEnumerable<TaskItem> GetAllTasks() => _tasks.Values;

    public TaskItem CreateTask(TaskItem task)
    {
        task.Id = _nextId++;
        _tasks[task.Id] = task;
        return task;
    }

    public bool UpdateTask(TaskItem task)
    {
        if (!_tasks.ContainsKey(task.Id)) return false;
        _tasks[task.Id] = task;
        return true;
    }

    public bool DeleteTask(int id) => _tasks.TryRemove(id, out _);

    public void Clear() => _tasks.Clear();
}
