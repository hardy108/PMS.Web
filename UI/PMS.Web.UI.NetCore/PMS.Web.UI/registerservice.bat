
set x=%~dp0
set x=%x%PMS.Web.UI.exe
sc create "PMS Web UI Net Core HO" binPath="%x%"