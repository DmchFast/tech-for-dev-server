using work5_ASP.NET_Core_API.Models;

namespace work5_ASP.NET_Core_API.Services;

public interface ITaskStorage
{
    TaskItem? GetTaskById(int id);
    IEnumerable<TaskItem> GetAllTasks();
    TaskItem CreateTask(TaskItem task);
    bool UpdateTask(TaskItem task);
    bool DeleteTask(int id);
    void Clear(); // для тестов
}
