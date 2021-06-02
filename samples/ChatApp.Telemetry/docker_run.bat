docker-compose -f docker-compose.yaml -f docker-compose.telemetry.yaml pull
docker-compose -f docker-compose.yaml -f docker-compose.telemetry.yaml up
docker-compose -f docker-compose.yaml -f docker-compose.telemetry.yaml down --remove-orphans
