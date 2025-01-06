

set x=%~dp0
set x=%x%PMS.Web.Services.exe
sc create "PMS Web Service Net Core HO" binPath="%x%"