@echo off
REM Скрипт компиляции Lifeblood 3D через csc.exe

echo ========================================
echo Компиляция Lifeblood 3D
echo ========================================

set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
set OUTPUT=bin\Lifeblood.exe
set TARGET=winexe

REM Создание директории для вывода
if not exist bin mkdir bin

REM Список всех .cs файлов
set FILES=^
Program.cs ^
Engine\Vector3.cs ^
Engine\Camera3D.cs ^
Engine\Shader.cs ^
Engine\GL.cs ^
Rendering\Mesh.cs ^
Rendering\Texture.cs ^
Rendering\ModelLoader.cs ^
Rendering\Renderer3D.cs ^
Game\Physics3D.cs ^
Game\Player.cs ^
Game\ModLoader.cs ^
Game\WeaponDefs.cs ^
Game\Combat.cs ^
Game\PlayerStats.cs ^
Game\Economy.cs ^
Game\Settings.cs ^
Game\Physics.cs ^
Network\NetworkProtocol.cs ^
Network\GameServer.cs ^
Network\GameClient.cs ^
Forms\MainMenu.cs ^
Network\DownloadManager.cs ^
Forms\Game3DWindow.cs ^
Forms\MapEditor.cs ^
Forms\SettingsMenu.cs

REM Ссылки на библиотеки
set REFS=^
/r:System.dll ^
/r:System.Core.dll ^
/r:System.Drawing.dll ^
/r:System.Windows.Forms.dll ^
/r:System.Numerics.dll

echo Компиляция...
%CSC% /target:%TARGET% /out:%OUTPUT% %REFS% /unsafe /optimize+ %FILES%

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Компиляция успешна!
    echo Исполняемый файл: %OUTPUT%
    echo ========================================
    echo.
    echo Для запуска игры: bin\Lifeblood.exe
    echo Для запуска сервера: bin\Lifeblood.exe --server
) else (
    echo.
    echo ========================================
    echo ОШИБКА КОМПИЛЯЦИИ!
    echo ========================================
)

pause
