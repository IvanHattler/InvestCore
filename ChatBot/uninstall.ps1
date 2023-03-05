$serviceName = "winvidmgmt64";

sc.exe stop $serviceName
sc.exe delete $serviceName