docker build -t chatapp_magiconion:latest -f ChatApp.Server/Dockerfile .
docker tag chatapp_magiconion:latest cysharp/magiconion_sample_chatapp:3.0.13
docker tag chatapp_magiconion:latest cysharp/magiconion_sample_chatapp:latest
docker push cysharp/magiconion_sample_chatapp:3.0.13
docker push cysharp/magiconion_sample_chatapp:latest
