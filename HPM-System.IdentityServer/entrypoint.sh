#!/bin/bash
set -e

# Ждём, пока БД станет доступной
until dotnet ef database update --verbose; do
  >&2 echo "PostgreSQL is unavailable - sleeping"
  sleep 5
done

echo "EF Core migrations applied."

# Проверяем, не включен ли Fast Mode (volume mount)
if [ ! -f /usr/src/app/HPM-System.IdentityServer.dll ]; then
    echo "Fast mode detected — skipping migrations."
    exec dotnet HPM-System.IdentityServer.dll
fi

# Применяем миграции
echo "Applying EF Core migrations for IdentityServer..."
dotnet ef database update --verbose

# Запускаем приложение
exec dotnet HPM-System.IdentityServer.dll