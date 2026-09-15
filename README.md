# MauiMessenger

Кроссплатформенный мессенджер на `.NET MAUI` (Android / iOS / Windows / MacCatalyst) с REST API и SignalR.

Бэкенд (ASP.NET Core) - <https://github.com/nougght/MessengerBackend>


<a href="https://postimg.cc/876g47pQ" target="_blank"><img src="https://i.postimg.cc/cH8S6Lps/msg.png" alt="image-2"></a><br><br>
## Функционал

- Авторизация с поддержкой access/refresh токенов
- Просмотр и отправка сообщений в чатах (личных и групповых)
- Отправка и отображение видео и изображений
- Добавление в контакты
- Реалтайм обновление данных через SignalR (получение новых сообщений и событий чата)
- Просмотр и редактирование профиля пользователя
- Подсказки для сообщений от ИИ

Приложение реализовано на .NET MAUI (Android/iOS/Windows/MacCatalyst), использует архитектуру MVVM, HTTP-клиент, сгенерированный с помощью NSwag для взаимодействия с backend REST API.

## Технологии

- UI на MAUI 
- MVVM архитектура (`CommunityToolkit.Mvvm`)
- HTTP-клиент к backend API (генерация через NSwag)
- Реалтайм-события через SignalR



## Генерация API-клиента (NSwag)

В проекте есть конфиги в `MauiMessenger/NswagConfig/`, использующие swagger документацию с бэкенда.  

``` bash
dotnet nswag run "NswagConfig/clientGen.nswag"
dotnet nswag run "NswagConfig/DtoGen.nswag"
```
