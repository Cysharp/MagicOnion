:: cysharp/magiconion_sample_chatapp
docker-compose -f docker-compose.self.yaml build
docker tag chatapp_magiconion:latest cysharp/magiconion_sample_chatapp:latest
docker tag chatapp_magiconion:latest cysharp/magiconion_sample_chatapp:3.0.13
docker push cysharp/magiconion_sample_chatapp:latest
docker push cysharp/magiconion_sample_chatapp:3.0.13

:: cysharp/magiconion_sample_chatapp_telemetry
docker-compose -f docker-compose.self.telemetry.yaml build magiconion
docker tag chatapp_magiconion:latest cysharp/magiconion_sample_chatapp_telemetry:latest
docker tag chatapp_magiconion:latest cysharp/magiconion_sample_chatapp_telemetry:3.0.13
docker push cysharp/magiconion_sample_chatapp_telemetry:latest
docker push cysharp/magiconion_sample_chatapp_telemetry:3.0.13
