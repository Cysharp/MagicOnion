# Unity WebGL

MagicOnion supports Unity WebGL platform experimentally. This page explains how to introduce MagicOnion on the WebGL platform and the limitations.

## Installation

To use MagicOnion on the Unity WebGL platform, you need to install [GrpcWebSocketBridge](https://github.com/Cysharp/GrpcWebSocketBridge) in addition to IL2CPP support.

GrpcWebSocketBridge is a library that realizes gRPC communication on WebSocket. By introducing this library to the client and server, you can communicate with the MagicOnion server from the browser.

## Limitations
- Heartbeat from the client side is not supported at this time
    - Heartbeat implementation depends on a runtime feature that internally uses a thread-based timer (System.Threading.Timer), which does not work because Unity WebGL does not support threads
