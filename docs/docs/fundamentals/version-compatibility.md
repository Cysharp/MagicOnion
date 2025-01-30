# Version Compatibility

MagicOnion efforts to maintain compatibility between client and server versions as much as possible. As long as the functionality level is the same on both the client and server, they are generally compatible with each other.

- v6 clients can connect to v7 servers
- v7 clients can connect to v6 servers

However, please note that you need to be careful when using features that are supported in specific versions. For example, heartbeats and client results are only supported from v7 onwards. If you use such features, you need to align the versions of the client and server.

| Client Version | Server Version | Compatibility | Remarks |
|----------------------|------------------|--------| ------|
| v6                   | v6               | ✅      | |
| v7                   | v7               | ✅      | |
| v6                   | v7               | ⛅      | Heartbeats and client results are not available from the server |
| v7                   | v6               | ⛅      | Heartbeats are not available from the client |
