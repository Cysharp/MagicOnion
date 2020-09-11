:: cysharp/magiconion_sample_chatapp_telemetry
docker-compose -f docker-compose.yaml build magiconion
docker tag chatapp_magiconion:latest cysharp/magiconion_sample_chatapp_telemetry:latest
docker tag chatapp_magiconion:latest cysharp/magiconion_sample_chatapp_telemetry:3.0.13
docker push cysharp/magiconion_sample_chatapp_telemetry:latest
docker push cysharp/magiconion_sample_chatapp_telemetry:3.0.13
