# Unary or StreamingHub
TBW

MagicOnion can define RPC-style APIs using Unary services and StreamingHub services. You can implement everything with StreamingHub, but for general APIs that do not require notifications from the server, it is recommended to use Unary.

Below are the differences between Unary and StreamingHub:

- Unary can process a single request and response at a time, similar to a simple HTTP POST request
- StreamingHub is a bidirectional communication that sends messages with a single continuous request and response

## Benefits of Unary

- Load Balancing and Observability
    - Unary is essentially an HTTP POST call, so it is compatible with existing eco systems such as ASP.NET Core, load balancers, CDNs, and WAFs
    - StreamingHub is a single long request, and it is difficult to log internal Hub method calls at the infrastructure level
        - This means that Hub method calls cannot be load balanced
- Overhead of StreamingHub
    - Unary is essentially a simple HTTP POST, and does not require the establishment of a connection like StreamingHub
    - StreamingHub has additional overhead such as starting a message loop and setting up a heartbeat during connection

## Benefits of StreamingHub

- Real-time message sending from the server to the client
    - If notifications from the server to the client are required, consider using StreamingHub
    - This is recommended for chat message notifications, game position synchronization, etc.
    - It replaces the need for polling/long polling with Unary or regular HTTP requests
