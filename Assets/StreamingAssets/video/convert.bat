setlocal enableextensions disabledelayedexpansion
set colorfile=%1
set depthfile=%2
set irfile=%3
FOR %%G IN (%colorfile%, %depthfile%, %irfile%) DO ffmpeg2theora.exe %%G