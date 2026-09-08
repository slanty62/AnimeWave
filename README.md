<div align="center">

# 🌊 AnimeWave

### Modern Anime Catalog built with ASP.NET Core MVC

<p>
  <img src="https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core MVC">
  <img src="https://img.shields.io/badge/C%23-.NET-239120?style=for-the-badge&logo=csharp&logoColor=white" alt="C#">
  <img src="https://img.shields.io/badge/PostgreSQL-Database-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL">
  <img src="https://img.shields.io/badge/Entity%20Framework-Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="Entity Framework Core">
</p>

<p>
  <img src="https://img.shields.io/badge/Identity-Authentication-8B5CF6?style=flat-square" alt="Identity">
  <img src="https://img.shields.io/badge/Razor-Views-7C3AED?style=flat-square" alt="Razor">
  <img src="https://img.shields.io/badge/Status-In%20Development-A855F7?style=flat-square" alt="Status">
  <img src="https://img.shields.io/badge/License-MIT-success?style=flat-square" alt="MIT">
</p>

**Discover • Watch • Save**

</div>

---

## 🌊 About AnimeWave

**AnimeWave** — учебное веб-приложение для просмотра, поиска и сохранения аниме, созданное на **ASP.NET Core MVC**.

Проект выполнен в современном тёмном стиле стриминговых сервисов и использует **PostgreSQL** для хранения данных, **Entity Framework Core** для работы с базой данных и **ASP.NET Core Identity** для авторизации пользователей.

Основная цель проекта — продемонстрировать создание полноценного MVC-приложения с авторизацией, ролями, CRUD-операциями, фильтрацией, динамическим поиском и современным пользовательским интерфейсом.

> Видеоконтент в проекте носит демонстрационный характер. Основной акцент сделан на интерфейсе, архитектуре и работе с данными.

---

## ✨ Features

### 👤 User Features

- 🔐 Регистрация и авторизация
- 👤 Персональный профиль пользователя
- ❤️ Добавление аниме в избранное
- 🕘 История недавно просмотренных аниме
- 🔎 Умный поиск прямо из navbar
- ⌨️ Управление поиском с клавиатуры
- 🎭 Фильтрация по жанрам
- ⭐ Фильтрация по рейтингу
- 📅 Сортировка по году выхода
- 🔤 Поиск по русскому и оригинальному названию
- 📱 Адаптивный интерфейс

### 🛠️ Admin Features

- ➕ Добавление нового аниме
- ✏️ Редактирование аниме
- 🗑️ Удаление аниме
- 🎭 Выбор жанров
- 🖼️ Добавление постера и баннера
- ⭐ Изменение рейтинга
- 📅 Изменение года выхода
- 🎬 Указание количества серий
- 🏷️ Управление статусом аниме
- 📊 Просмотр статистики каталога
- 🔒 Доступ только для роли `Admin`

### 🎨 UI / UX

- 🌑 Тёмный интерфейс
- 💜 Фиолетовые акценты
- 🎞️ Анимированная hero-карусель
- ✨ Hover-анимации карточек
- 🌊 Плавное появление элементов
- 🔔 Toast-уведомления
- 💀 Skeleton loading
- 🖼️ Live Preview при добавлении аниме
- 🔍 Popup Smart Search
- 📱 Responsive Design

---

## 🔎 Smart Search

AnimeWave содержит динамический поиск без перезагрузки страницы.

Пользователь нажимает на иконку поиска и начинает вводить название:

```text
Naruto
Death Note
Наруто
Solo Leveling
Attack on Titan
