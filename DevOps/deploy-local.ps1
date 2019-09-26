$myProj = "SSW.Consulting"
$myEnv = "staging"
$myResourceGroup = "$($myProj).$($myEnv)"
$myLocation = "Australia Southeast"

.\deploy.ps1 `
    -resourceGroupName $myResourceGroup `
    -location $myLocation `
    -environment $myEnv `
    -project $myProj `
    -templateFile infrastructure.json `
    -parametersFile infrastructure.parameters.local.json

