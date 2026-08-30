$ErrorActionPreference = 'Stop'
$projectName = "user-profile-smoke-$([Guid]::NewGuid().ToString('N').Substring(0, 8))"
$webPort = if ($env:WEB_PORT) { $env:WEB_PORT } else { '4200' }
$baseUrl = "http://localhost:$webPort"
$email = "smoke-$([Guid]::NewGuid().ToString('N'))@example.com"

try {
    docker compose --project-name $projectName up --build --detach --wait
    $health = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get
    if ($health.status -ne 'healthy') { throw "Health check retornou estado inesperado: $($health.status)" }

    $registration = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -ContentType 'application/json' -Body (@{
        name = 'Smoke Test User'; email = $email; password = 'smoke-secret'; confirmPassword = 'smoke-secret'
    } | ConvertTo-Json)
    if (-not $registration.token) { throw 'Cadastro não retornou um JWT.' }

    $profile = Invoke-RestMethod -Uri "$baseUrl/api/profile" -Method Get -Headers @{
        Authorization = "Bearer $($registration.token)"
    }
    if ($profile.email -ne $email) { throw "Perfil retornou email inesperado: $($profile.email)" }

    $login = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -ContentType 'application/json' -Body (@{
        email = $email.ToUpperInvariant(); password = 'smoke-secret'
    } | ConvertTo-Json)
    if (-not $login.token) { throw 'Login não retornou um JWT.' }
    Write-Host 'Smoke test do Compose concluído com sucesso.'
}
finally {
    docker compose --project-name $projectName down --volumes --remove-orphans
}
