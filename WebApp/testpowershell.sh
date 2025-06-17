$body = @{
    title = "Senior Developer"
    description = "Senior .NET Developer position"
    location = "New York"
    status = "open"
    recruiterId = 1
    departmentId = 1
    budget = 100000.00
    closingDate = "2024-12-31"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5246/api/positions" `
        -Method Post `
        -Body $body `
        -ContentType "application/json"
    
    Write-Host "Success! Created position with ID: $($response.id)"
    $response | Format-List *
} catch {
    Write-Host "Error:"
    Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)"
    Write-Host "Status Description: $($_.Exception.Response.StatusDescription)"
    Write-Host "Response: $($_.ErrorDetails.Message)"
}