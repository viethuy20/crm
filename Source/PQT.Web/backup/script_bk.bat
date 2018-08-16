@echo off
set USER="root"
set PASSWORD="priqua"
set OUTPUT="D:\BackupTemp_NoDelete\db"
set OUTPUT2="D:\Dropbox\db"

set INPUTDATA="C:\inetpub\wwwroot\PQT_Control\data"
set OUTPUTDATA="D:\BackupTemp_NoDelete\data"
set OUTPUTDATA2="D:\Dropbox\data"

echo MySQL Backup wird gestartet...
cd C:\Program Files\MySQL\MySQL Server 5.6\bin
for /F "tokens=1-4 delims=/ " %%i in ('date /t') do (set date=%%i) 
mkdir %OUTPUT%/%date%
echo 1/ mysqldump pqtdb...
mysqldump -u%USER% -p%PASSWORD% -hlocalhost pqtdb > %OUTPUT%/%date%/pqtdb.sql
echo Compressing... 
"C:\Program Files (x86)\WinRAR\WinRAR.EXE" m -ppriqualive -r %OUTPUT%/%date%.rar %OUTPUT%/%date%

echo copy bk file to dropbox...
xcopy /s %OUTPUT% %OUTPUT2%

echo Backup data resource wird gestartet...
mkdir %OUTPUTDATA%/%date%
xcopy /s %INPUTDATA% %OUTPUTDATA%/%date%

echo Compressing data ... 
"C:\Program Files (x86)\WinRAR\WinRAR.EXE" m -ppriqualive -r %OUTPUTDATA%/%date%.rar %OUTPUTDATA%/%date%

echo copy bk file to dropbox...
xcopy /s %OUTPUTDATA% %OUTPUTDATA2%

echo Backup durchgeführt!

