# Unary or StreamingHub
MagicOnion provides two types of API implementation methods: Unary services and StreamingHub services. You can define RPC-style APIs using either of these methods.

The differences between Unary and StreamingHub are as follows:

- Unary is a simple HTTP POST request that processes one request and one response at a time
    - See: [Unary service fundamentals](/unary/fundamentals)
- StreamingHub is bidirectional communication that sends messages between the client and server using a continuous connection
    - See: [StreamingHub fundamentals](/streaminghub/fundamentals)

![](/img/docs/fig-unary-streaminghub.png)

You can implement everything with StreamingHub, but for general APIs that do not require notifications from the server (e.g., REST or Web APIs), it is recommended to use Unary.


## Benefits of Unary

- Load Balancing and Observability
    - Unary is essentially an HTTP POST call, so it is compatible with existing eco systems such as ASP.NET Core, load balancers, CDNs, and WAFs
    - StreamingHub is a single long request, and it is difficult to log internal Hub method calls at the infrastructure level
        - This means that Hub method calls cannot be load balanced
- Overhead of StreamingHub
    - Unary is essentially a simple HTTP POST, and does not require the establishment of a connection like StreamingHub
    - StreamingHub has additional overhead such as starting a message loop and setting up a heartbeat during connection

## Benefits of StreamingHub

- Real-time message sending from the server to the client (including multiple clients)
    - If notifications from the server to the client are required, consider using StreamingHub
    - This is recommended for chat message notifications, game position synchronization, etc.
    - It replaces the need for polling/long polling with Unary or regular HTTP requests
