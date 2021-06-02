:: run from Repository Root
:: cysharp/magiconion_sample_chatapp_telemetry
:: cysharp/magiconion_sample_microserver
docker build -t chatapp_magiconion:latest -f samples/ChatApp.Telemetry/ChatApp.Server/Dockerfile .
docker build -t chatapp_microserver:latest -f samples/ChatApp.Telemetry/MicroServer/Dockerfile .
