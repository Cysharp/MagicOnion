:: run from this directory
:: cysharp/magiconion_sample_chatapp_telemetry
docker build -t chatapp_magiconion:latest -f ChatApp.Server/Dockerfile .
docker tag chatapp_magiconion:latest cysharp/magiconion_sample_chatapp_telemetry:latest
docker tag chatapp_magiconion:latest cysharp/magiconion_sample_chatapp_telemetry:4.0.1-0.8.0.beta1
docker push cysharp/magiconion_sample_chatapp_telemetry:latest
docker push cysharp/magiconion_sample_chatapp_telemetry:4.0.1-0.8.0.beta1
