@echo off

::for /F "tokens=1-4 delims=/ " %%i in ('date /t') do (set date=%%i) 
for /f %%i in ('powershell ^(get-date^).DayOfWeek') do (set dow=%%i)


set USER="root"
set PASSWORD="priqua"
set OUTPUT="D:\PQControlBackupTemp_NoDelete\db"
set OUTPUT2="D:\Dropbox\db"

set INPUTDATA="C:\inetpub\wwwroot\PQT_Control\data"
set OUTPUTDATA="D:\PQControlBackupTemp_NoDelete\data"
set OUTPUTDATADOW="D:\PQControlBackupTemp_NoDelete\data\%dow%"
set OUTPUTDATA2="D:\Dropbox\data"

echo MySQL Backup wird gestartet...
cd C:\Program Files\MySQL\MySQL Server 5.6\bin

mkdir %OUTPUT%/%dow%
echo 1/ mysqldump pqtdb...
mysqldump -u%USER% -p%PASSWORD% -hlocalhost pqtdb > %OUTPUT%/%dow%/pqtdb.sql
echo Compressing... 
"C:\Program Files (x86)\WinRAR\WinRAR.EXE" m -ppriqualive -r %OUTPUT%/%dow%.rar %OUTPUT%/%dow%

echo copy bk file to dropbox...
xcopy /s/y %OUTPUT% %OUTPUT2%

echo Backup data resource wird gestartet...
mkdir %OUTPUTDATADOW%
xcopy /s/y %INPUTDATA% %OUTPUTDATADOW%

echo Compressing data ... 
"C:\Program Files (x86)\WinRAR\WinRAR.EXE" m -ppriqualive -r %OUTPUTDATA%/%dow%.rar %OUTPUTDATADOW%

echo copy bk file to dropbox...
xcopy /s/y %OUTPUTDATA% %OUTPUTDATA2%

echo Backup durchgeführt!
pause