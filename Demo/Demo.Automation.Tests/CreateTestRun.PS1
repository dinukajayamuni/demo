 param (
    [Parameter(Mandatory=$true)][string]$jiraUser,
    [Parameter(Mandatory=$true)][string]$jiraPassword,
    [Parameter(Mandatory=$true)][string]$devOpsProject,
    [Parameter(Mandatory=$true)][string]$devopsPassword,
    [Parameter(Mandatory=$true)][string]$releaseId
 )
$devopsUser = ''

$devopsCreds = "$($devopsUser):$($devopsPassword)"

$devOpsEncodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($devopsCreds))

$devOpsBasicAuthValue = "Basic $devOpsEncodedCreds"

$devOpsHeaders = @{
    Authorization = $devOpsBasicAuthValue
}

$releaseUrl = "https://vsrm.dev.azure.com/$($devOpsProject)/_apis/release/releases/$($releaseId)"
$releaseResponse = Invoke-WebRequest -Method GET -Uri $releaseUrl -Headers $devOpsHeaders
$release = ConvertFrom-Json $([String]::new($releaseResponse.Content)) 
$releaseDefinitionId = $release.releaseDefinition.id

$releasesUrl = "https://vsrm.dev.azure.com/$($devOpsProject)/_apis/release/releases?definitionId=$($releaseDefinitionId)"
$releasesResponse = Invoke-WebRequest -Method GET -Uri $releasesUrl -Headers $devOpsHeaders
$releases = ConvertFrom-Json $([String]::new($releasesResponse.Content)) 

$releaseFound = $false
$previousReleaseId = ""
foreach ($release in $releases.value) {
    if ( $release.id -eq $releaseId ) {
        $releaseFound = $true
        continue               
    }
    if ($releaseFound -eq $true) {
        $previousReleaseId = $release.id
        break
    } 
}

$workItemsUrl = "https://vsrm.dev.azure.com/$($devOpsProject)/_apis/release/releases/$($releaseId)/workitems?baseReleaseId=$($previousReleaseId)"
$workItemsResponse = Invoke-WebRequest -Method GET -Uri $workItemsUrl -Headers $devOpsHeaders
$workItems = ConvertFrom-Json $([String]::new($workItemsResponse.Content))

$jiraCreds = "$($jiraUser):$($jiraPassword)"

$jiraEncodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($jiraCreds))

$jiraBasicAuthValue = "Basic $jiraEncodedCreds"

$jiraHeaders = @{
    Authorization = $jiraBasicAuthValue
}
$testPointsArray = @()
foreach ($workItem in $workItems.value) {
    $workItem.url -match "(https:\/\/.*?)\/.*\/(\w+-\d+)\/?"
    $jiraLinksUrl = "$($Matches[1])/rest/api/2/issue/$($Matches[2])/remotelink"
    $jiraLinksResponse = Invoke-WebRequest -Method GET -Uri $jiraLinksUrl -Headers $jiraHeaders
    $jiraLinks = ConvertFrom-Json $([String]::new($jiraLinksResponse.Content))
    foreach ($jiraLink in $jiraLinks) {
        if ($jiraLink.object.url -match "planId=(\d+)&suiteId=(\d+)" -eq $true){
            $planId = $Matches[1]
            $suiteId = $Matches[2]
            if ($jiraLink.object.title -ne $null -and $jiraLink.object.title -ne ""){
                $testCaseIds = $jiraLink.object.title -split ','
                foreach($testCaseId in $testCaseIds){
                    $testCaseId = $testCaseId -replace '^\s+|\s+$'
                    if ($testCaseId -eq ""){
                        continue
                    }
                    $testPointsUrl = "https://dev.azure.com/$($devOpsProject)/_apis/test/Plans/$($planId)/Suites/$($suiteId)/points?testCaseId=$($testCaseId)"
                    $testPointsResponse = Invoke-WebRequest -Method GET -Uri $testPointsUrl -Headers $devOpsHeaders
                    $testPoints = ConvertFrom-Json $([String]::new($testPointsResponse.Content)) 
                    foreach($testPoint in $testPoints.value){
                        $testPointsArray = $testPointsArray + $testPoint.id
                    }
                }
            }
            else{
                $testPointsUrl = "https://vsrm.dev.azure.com/$($devOpsProject)/_apis/test/Plans/$($planId)/Suites/$($suiteId)/points"
                $testPointsResponse = Invoke-WebRequest -Method GET -Uri $testPointsUrl -Headers $devOpsHeaders
                $testpoints = ConvertFrom-Json $([String]::new($testPointsResponse.Content))
                foreach($testPoint in $testPoints.value){
                    $testPointsArray = $testPointsArray + $testPoint.id
                }
            }
        }
    }
}


$createTestRunPayload = @{
    name = "OnDemandTestRun 20/08/2020, 15:12:55"
    automated = $true
    pointIds = $testPointsArray
    state = "NotStarted"
    dtlTestEnvironment = @{
        id = "vstfs://dummy"
    }
    plan = @{
        id = $planId
    }
    filter = @{
        sourceFilter = "*.dll"
        testCaseFilter = ""
    }
} | ConvertTo-Json
 
$createTestRunResponse = Invoke-WebRequest -Uri "https://dev.azure.com/dinukajayamuni/demo/_apis/test/Runs?api-version=6.0-preview.3" -Method POST -ContentType "application/json" -Body $createTestRunPayload -Headers $devOpsHeaders
$testRun = ConvertFrom-Json $([String]::new($createTestRunResponse.Content))
Write-Host "Test Run $($testRun.id)"
Write-Host ##vso[task.setvariable variable=test.RunId;]$testRun.id