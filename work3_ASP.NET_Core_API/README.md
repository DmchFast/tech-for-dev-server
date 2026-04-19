# PostgreSQL загрузка и запуск

## 1. Получение исходного кода

```bash
git clone <ссылка_на_репозиторий>
cd work3_ASP.NET_Core_API
```

## 2. Настройка базы данных PostgreSQL

### 2.1. Создание базы данных

Зайти в PostgreSQL и создайть пустую базу данных с именем DbWork3:

```bash
createdb -U postgres DbWork3
```

### 2.2. Восстановление из дампа

В репозитории приложен файл дампа `DbWork3_backup.sql`, нужно выполнить восстановление:

Файл дампа: [DbWork3_backup.sql](./DbWork3_backup.sql)

```bash
psql -U postgres -d DbWork3 -f DbWork3_backup.sql
```

### 2.3. Немного о `DbWork3_backup.sql`

| Таблица | Назначение |
|---------|------------|
| `Users` | Пользователи с **хешированными** паролями (bcrypt) и ролями (`admin`, `user`, `guest`) |
| `PlainUsers` | Пользователи с **открытыми** паролями (без хеширования) |
| `Todos` | Задачи (CRUD) |
| `__EFMigrationsHistory` | Служебная таблица Entity Framework |

### Предзаполненные данные (для быстрого тестирования)

#### Таблица `Users`

| Username | Роль | Хеш пароля (bcrypt) | Пароль в открытом виде |
|----------|------|---------------------|------------------------|
| `admin`  | admin | `...` | `admin` |
| `di`     | user  | `...` | `123` |
| `Dd`     | guest | `...` | `123` |

> **Примечание:** пароль для `admin` — `admin`, для `di` — `123`, для `Dd` — `123`. Это позволяет сразу выполнять логин и получать JWT-токены с соответствующими ролями.


#### Таблица `Todos`

Уже созданы несколько тестовых задач с `Id` от 2 до 4. `Id = 1` был удвлён.

## 3. Настройка приложения

Открыть файл appsettings.json и убедиться, что строка подключения соответствует вашим локальным настройкам:

```bash
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=DbWork3;Username=ВАШЕ_ИМЯ;Password=ВАШ_ПАРОЛЬ"
  },
  "Mode": "DEV",
  "DocsAuth": {
    "User": "admin",
    "Password": "admin"
  }
}
```
Заменить `ВАШЕ_ИМЯ` и `ВАШ_ПАРОЛЬ` на данные вышего PostgreSQL. 

## 4. Запуск

```bash
cd work3_ASP.NET_Core_API
dotnet run
# → http://localhost:5161
```
Приложение будет доступно по адресу → HTTP: http://localhost:5161