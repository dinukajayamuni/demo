 param (
    [Parameter(Mandatory=$true)][string]$jiraUser,
    [Parameter(Mandatory=$true)][string]$jiraPassword,
    [Parameter(Mandatory=$true)][string]$devOpsProject,
    [Parameter(Mandatory=$true)][string]$devopsPassword,
    [Parameter(Mandatory=$true)][string]$releaseId
 )

Write-Host "Release: $($releaseId)"
$devopsUser = ''

$devopsCreds = "$($devopsUser):$($devopsPassword)"

$devOpsEncodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($devopsCreds))

#$devOpsBasicAuthValue = "Basic $devOpsEncodedCreds"

$devOpsBasicAuthValue = "Bearer $devopsPassword"

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
Write-Host "Previous Release: $($previousReleaseId)"
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
$associatedWorkItems = @()
foreach ($workItem in $workItems.value) {
    if($workItem.url -match "(https:\/\/.*?)\/.*\/(\w+-\d+)\/?" -eq $false){
        continue
    }
    $jiraLinksUrl = "$($Matches[1])/rest/api/2/issue/$($Matches[2])/remotelink"
    Write-Host "Associated Work Item: $($jiraLinksUrl)"
    $associatedWorkItems = $associatedWorkItems + $jiraLinksUrl
    $jiraLinksResponse = Invoke-WebRequest -Method GET -Uri $jiraLinksUrl -Headers $jiraHeaders
    $jiraLinks = ConvertFrom-Json $([String]::new($jiraLinksResponse.Content))
    foreach ($jiraLink in $jiraLinks) {
        if ($jiraLink.object.url -match "planId=(\d+)&suiteId=(\d+)" -eq $true){
            $planId = $Matches[1]
            $suiteId = $Matches[2]
            Write-Host "Test Plan: $($planId)"
            Write-Host "Test Suite: $($suiteId)"
            # TODO: Allow test suite or plan execution
            if ($jiraLink.object.title -ne $null -and $jiraLink.object.title -ne ""){
                $jiraLink.object.title -match "Test\s?Cases\[((\d+,?)*)\]"
                $testCaseIds = $Matches[1] -split ','
                foreach($testCaseId in $testCaseIds){
                    $testCaseId = $testCaseId -replace '^\s+|\s+$'
                    if ($testCaseId -eq ""){
                        continue
                    }
                    Write-Host "Test Case: $($testCaseId)"
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

Write-Host "Test Points: $($testPointsArray)"

if( $testPointsArray.length -gt 0){
    $createTestRunPayload = @{
        name = "OnDemandTestRun"
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

    $createTestRunResponse = Invoke-WebRequest -Uri "https://dev.azure.com/$($devOpsProject)/_apis/test/Runs?api-version=6.0-preview.3" -Method POST -ContentType "application/json" -Body $createTestRunPayload -Headers $devOpsHeaders
    $testRun = ConvertFrom-Json $([String]::new($createTestRunResponse.Content))
    $testRunId = $testRun.id
    Write-Host "Test Run $($testRunId)"
    Write-Host "##vso[task.setvariable variable=runid;]$testRunId"

    $createReleaseLinkPayload = @{
        object = @{
            url = "https://dev.azure.com/$($devOpsProject)/_TestManagement/Runs?runId=$($testRunId)&_a=runCharts"
            title = "Test Run $($testRunId)"
        }
    } | ConvertTo-Json

    foreach($associatedWorkItem in $associatedWorkItems){        
        Invoke-WebRequest -Uri $associatedWorkItem -Method POST -ContentType "application/json" -Body $createReleaseLinkPayload -Headers $jiraHeaders
    }
}
