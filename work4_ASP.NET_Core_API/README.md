# ASP.NET Core Web API (Entity Framework Core + PostgreSQL)

Использвано:
- Entity Framework Core + PostgreSQL (миграции)
- Пользовательских исключений
- Валидации данных (Data Annotations + кастомный ответ)
- In-memory CRUD для пользователей
- Модульного тестирования (xUnit + Bogus)

## Тестирования API (Bruno)

### Пользовательские исключения

#### CustomExceptionA (400) – условие не выполнено
![GET_400](/work4_ASP.NET_Core_API/assets.md/400.png)

#### CustomExceptionB (404) – ресурс не найден
![GET_404](/work4_ASP.NET_Core_API/assets.md/404.png)



### Валидация данных

#### Ошибка валидации (возраст, email, пароль, имя) 
![VALID](/work4_ASP.NET_Core_API/assets.md/valid.png)

### Работа с пользователями

#### Создание пользователя 
![POST_USER](/work4_ASP.NET_Core_API/assets.md/new_user.png)

#### Получить пользователя по ID
![GET_USER_](/work4_ASP.NET_Core_API/assets.md/get_user.png)

#### Удалить пользователя
![DEL_USER_](/work4_ASP.NET_Core_API/assets.md/del_user.png)

#### Стереть всех пользователей
![RESERT_ALL](/work4_ASP.NET_Core_API/assets.md/reset.png)
 
## Запуск

### Получение исходного кода
```bash
git clone <ссылка_на_репозиторий>
cd work4_ASP.NET_Core_API
```

### Настройка подключения к PostgreSQL
Создать или отредактировать файл work4_ASP.NET_Core_API/appsettings.Development.json

```bash
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=DbWork4;Username=ВАШЕ_ИМЯ;Password=ВАШ_ПАРОЛЬ"
  }
```
> Заменить `ВАШЕ_ИМЯ` и `ВАШ_ПАРОЛЬ` на данные вышего PostgreSQL.

### Восстановление пакетов и применение миграций
```bash
dotnet restore
dotnet ef database update
```

### Запуск приложения
```bash
dotnet run
# → http://localhost:5161 или https://localhost:7203
```
Доступ будет по адресу → HTTP: http://localhost:5161 или -> HTTPS: https://localhost:7203

---
