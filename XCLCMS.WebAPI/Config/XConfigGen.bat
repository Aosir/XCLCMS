%@Try%
	:bat����Ŀ¼
	set XConfigGenBatPath=%~dp0
	:ִ��node����
	node %XConfigGenPath% --xconfig  %XConfigGenBatPath%XConfigGen-Config.json --rootpath %XConfigGenBatPath%
	:pause
%@EndTry%
:@Catch
  exit /b 0
:@EndCatch