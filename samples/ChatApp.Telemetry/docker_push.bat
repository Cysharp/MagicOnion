:: cysharp/magiconion_sample_chatapp_telemetry
:: run build first.
docker tag chatapp_magiconion:latest cysharp/magiconion_sample_chatapp_telemetry:latest
docker tag chatapp_magiconion:latest cysharp/magiconion_sample_chatapp_telemetry:4.3.1-1.0.0.rc4
docker push cysharp/magiconion_sample_chatapp_telemetry:latest
docker push cysharp/magiconion_sample_chatapp_telemetry:4.3.1-1.0.0.rc4

docker tag chatapp_microserver:latest cysharp/magiconion_sample_chatapp_microserver:latest
docker tag chatapp_microserver:latest cysharp/magiconion_sample_chatapp_microserver:4.3.1-1.0.0.rc4
docker push cysharp/magiconion_sample_chatapp_microserver:latest
docker push cysharp/magiconion_sample_chatapp_microserver:4.3.1-1.0.0.rc4
